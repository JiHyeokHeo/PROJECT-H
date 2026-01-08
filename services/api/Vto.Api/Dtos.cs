public record CreateSessionReq(string Type, int FrameCount = 36, string Ext = "jpg");
public record CreateSessionRes(string SessionId, string Type, int FrameCount, string[] ObjectKeys, string[] PutUrls);
public record CommitRes(string SessionId, string Status);
public record SessionRes(string SessionId, string Type, string Status, int FrameCount, FrameItem[]? Frames);
public record FrameItem(int Index, string ObjectKey, string GetUrl);
public record WorkerProcessedReq(string[] FrameKeys);