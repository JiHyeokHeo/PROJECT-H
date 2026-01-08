using Microsoft.EntityFrameworkCore;

public class VtoDb : DbContext
{
    public VtoDb(DbContextOptions<VtoDb> opt) : base(opt) { }
    public DbSet<CaptureSession> CaptureSessions => Set<CaptureSession>();
    public DbSet<CaptureAsset> CaptureAssets => Set<CaptureAsset>();
}

public class CaptureSession
{
    public string Id { get; set; } = default!;
    public string Type { get; set; } = default!;
    public string Status { get; set; } = default!;
    public int FrameCount { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

public class CaptureAsset
{
    public int Id { get; set; }
    public string SessionId { get; set; } = default!;
    public int ViewIndex { get; set; }
    public string ObjectKey { get; set; } = default!;
    public DateTime CreatedAtUtc { get; set; }
}
