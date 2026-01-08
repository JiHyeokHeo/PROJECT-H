using Microsoft.EntityFrameworkCore;
using Minio;
using Minio.DataModel.Args;
using StackExchange.Redis;
using System;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

string? C(string key) => builder.Configuration[key];

// ---- settings ----
var sqlConn = C("ConnectionStrings:SqlServer")!;
var redisConn = C("Redis:Connection")!;
var corsOrigin = C("Cors:Origin") ?? "*";

var minioEndpoint = C("Minio:Endpoint") ?? "localhost:9000"; // ��������
var minioAccessKey = C("Minio:AccessKey") ?? "minioadmin";
var minioSecretKey = C("Minio:SecretKey") ?? "minioadmin";
var minioBucket = C("Minio:Bucket") ?? "vto";
var workerToken = C("Worker:Token") ?? "dev-worker-token";

// ---- EF ----
builder.Services.AddDbContext<VtoDb>(opt => opt.UseSqlServer(sqlConn));

// ---- Redis ----
builder.Services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConn));

// ---- MinIO ----
builder.Services.AddSingleton<IMinioClient>(_ =>
    new MinioClient()
        .WithEndpoint(minioEndpoint)
        .WithCredentials(minioAccessKey, minioSecretKey)
        .Build()
);

// ---- CORS ----
builder.Services.AddCors(opt =>
{
    opt.AddDefaultPolicy(p =>
    {
        if (corsOrigin == "*") p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
        else p.WithOrigins(corsOrigin).AllowAnyHeader().AllowAnyMethod();
    });
});

var app = builder.Build();
app.UseCors();

// dev: auto create tables
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<VtoDb>();
    db.Database.EnsureCreated();
}

// ---------- endpoints ----------
app.MapGet("/health", () => Results.Ok(new { ok = true }));

app.MapPost("/v1/capture-sessions", async (CreateSessionReq req, VtoDb db, IMinioClient minio) =>
{
    var type = req.Type?.Trim().ToLowerInvariant();
    if (type is not ("body" or "garment"))
        return Results.BadRequest(new { error = "type must be 'body' or 'garment'" });

    var frameCount = Math.Clamp(req.FrameCount, 1, 180);
    var ext = string.IsNullOrWhiteSpace(req.Ext) ? "jpg" : req.Ext.Trim('.').ToLowerInvariant();

    var sessionId = Guid.NewGuid().ToString("N");

    var session = new CaptureSession
    {
        Id = sessionId,
        Type = type,
        Status = "Created",
        FrameCount = frameCount,
        CreatedAtUtc = DateTime.UtcNow
    };
    db.CaptureSessions.Add(session);
    await db.SaveChangesAsync();

    var keys = Enumerable.Range(0, frameCount)
        .Select(i => $"raw/{sessionId}/{i:000}.{ext}")
        .ToArray();

    var putUrls = new string[keys.Length];
    for (int i = 0; i < keys.Length; i++)
    {
        var putArgs = new PresignedPutObjectArgs()
      .WithBucket(minioBucket)
      .WithObject(keys[i])
      .WithExpiry(60 * 60);

        putUrls[i] = await minio.PresignedPutObjectAsync(putArgs);
    }

    return Results.Ok(new CreateSessionRes(sessionId, type, frameCount, keys, putUrls));
});

app.MapPost("/v1/capture-sessions/{id}/commit", async (string id, VtoDb db, IConnectionMultiplexer mux) =>
{
    var session = await db.CaptureSessions.FindAsync(id);
    if (session == null) return Results.NotFound(new { error = "session not found" });

    if (session.Status == "Processed")
        return Results.Ok(new CommitRes(session.Id, session.Status));

    session.Status = "Committed";
    await db.SaveChangesAsync();

    var job = JsonSerializer.Serialize(new { sessionId = id, type = session.Type });
    await mux.GetDatabase().ListRightPushAsync("jobs:capture", job);

    return Results.Ok(new CommitRes(session.Id, session.Status));
});

app.MapGet("/v1/capture-sessions/{id}", async (string id, VtoDb db, IMinioClient minio) =>
{
    var session = await db.CaptureSessions.FindAsync(id);
    if (session == null) return Results.NotFound(new { error = "session not found" });

    FrameItem[]? frames = null;

    if (session.Status == "Processed")
    {
        var assets = await db.CaptureAssets
            .Where(a => a.SessionId == id)
            .OrderBy(a => a.ViewIndex)
            .ToListAsync();

        frames = new FrameItem[assets.Count];
        for (int i = 0; i < assets.Count; i++)
        {
            var a = assets[i];
            var getArgs = new PresignedGetObjectArgs()
      .WithBucket(minioBucket)
      .WithObject(a.ObjectKey)
     .WithExpiry(60 * 60);

            var getUrl = await minio.PresignedGetObjectAsync(getArgs);
            frames[i] = new FrameItem(a.ViewIndex, a.ObjectKey, getUrl);
        }
    }

    return Results.Ok(new SessionRes(session.Id, session.Type, session.Status, session.FrameCount, frames));
});

// worker callback (internal)
app.MapPost("/internal/capture-sessions/{id}/processed", async (string id, WorkerProcessedReq req, HttpRequest http, VtoDb db) =>
{
    if (!http.Headers.TryGetValue("X-Worker-Token", out var token) || token != workerToken)
        return Results.Unauthorized();

    var session = await db.CaptureSessions.FindAsync(id);
    if (session == null) return Results.NotFound();

    var existing = await db.CaptureAssets.Where(a => a.SessionId == id).ToListAsync();
    db.CaptureAssets.RemoveRange(existing);

    foreach (var key in req.FrameKeys)
    {
        var file = key.Split('/').LastOrDefault() ?? "";
        var name = file.Split('.').FirstOrDefault() ?? "0";
        int idx = int.TryParse(name, out var parsed) ? parsed : 0;

        db.CaptureAssets.Add(new CaptureAsset
        {
            SessionId = id,
            ViewIndex = idx,
            ObjectKey = key,
            CreatedAtUtc = DateTime.UtcNow
        });
    }

    session.Status = "Processed";
    await db.SaveChangesAsync();

    return Results.Ok(new { ok = true });
});

app.Run();

