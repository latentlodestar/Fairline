namespace Fairline.Domain.Ingest;

public sealed class IngestLog
{
    public Guid Id { get; private set; }
    public Guid IngestRunId { get; private set; }
    public string Level { get; private set; } = string.Empty;
    public string Message { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAtUtc { get; private set; }

    private IngestLog() { }

    public static IngestLog Create(Guid ingestRunId, string level, string message, DateTimeOffset now)
    {
        return new IngestLog
        {
            Id = Guid.NewGuid(),
            IngestRunId = ingestRunId,
            Level = level,
            Message = message,
            CreatedAtUtc = now
        };
    }

    public static class Levels
    {
        public const string Info = "Info";
        public const string Warning = "Warning";
        public const string Error = "Error";
    }
}
