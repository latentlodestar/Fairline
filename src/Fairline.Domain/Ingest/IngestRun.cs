namespace Fairline.Domain.Ingest;

public sealed class IngestRun
{
    public Guid Id { get; private set; }
    public string RunType { get; private set; } = string.Empty;
    public string Status { get; private set; } = string.Empty;
    public DateTimeOffset StartedAtUtc { get; private set; }
    public DateTimeOffset? CompletedAtUtc { get; private set; }
    public string? Summary { get; private set; }
    public int RequestCount { get; private set; }
    public int EventCount { get; private set; }
    public int SnapshotCount { get; private set; }
    public int ErrorCount { get; private set; }

    private IngestRun() { }

    public static IngestRun Start(string runType, DateTimeOffset now)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(runType);

        return new IngestRun
        {
            Id = Guid.NewGuid(),
            RunType = runType,
            Status = Statuses.Running,
            StartedAtUtc = now
        };
    }

    public void Complete(int requestCount, int eventCount, int snapshotCount, int errorCount, DateTimeOffset now)
    {
        Status = errorCount > 0 ? Statuses.Failed : Statuses.Completed;
        RequestCount = requestCount;
        EventCount = eventCount;
        SnapshotCount = snapshotCount;
        ErrorCount = errorCount;
        CompletedAtUtc = now;
    }

    public void Fail(string error, DateTimeOffset now)
    {
        Status = Statuses.Failed;
        Summary = error;
        ErrorCount++;
        CompletedAtUtc = now;
    }

    public void SetSummary(string summary) => Summary = summary;

    public static class RunTypes
    {
        public const string CatalogRefresh = "CatalogRefresh";
        public const string GapFill = "GapFill";
    }

    public void Cancel(DateTimeOffset now)
    {
        Status = Statuses.Cancelled;
        CompletedAtUtc = now;
    }

    public void Cancel(int requestCount, int eventCount, int snapshotCount, DateTimeOffset now)
    {
        Status = Statuses.Cancelled;
        RequestCount = requestCount;
        EventCount = eventCount;
        SnapshotCount = snapshotCount;
        CompletedAtUtc = now;
    }

    public static class Statuses
    {
        public const string Running = "Running";
        public const string Completed = "Completed";
        public const string Failed = "Failed";
        public const string Cancelled = "Cancelled";
    }
}
