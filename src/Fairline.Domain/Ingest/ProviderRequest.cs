namespace Fairline.Domain.Ingest;

public sealed class ProviderRequest
{
    public Guid Id { get; private set; }
    public Guid IngestRunId { get; private set; }
    public string Url { get; private set; } = string.Empty;
    public int? StatusCode { get; private set; }
    public long? DurationMs { get; private set; }
    public DateTimeOffset RequestedAtUtc { get; private set; }
    public int? QuotaUsed { get; private set; }
    public string? ErrorMessage { get; private set; }

    private ProviderRequest() { }

    public static ProviderRequest Create(
        Guid ingestRunId,
        string url,
        int? statusCode,
        long? durationMs,
        int? quotaUsed,
        string? errorMessage,
        DateTimeOffset now)
    {
        return new ProviderRequest
        {
            Id = Guid.NewGuid(),
            IngestRunId = ingestRunId,
            Url = url,
            StatusCode = statusCode,
            DurationMs = durationMs,
            QuotaUsed = quotaUsed,
            ErrorMessage = errorMessage,
            RequestedAtUtc = now
        };
    }
}
