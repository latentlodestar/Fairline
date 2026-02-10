namespace Fairline.Domain.Ingest;

public sealed class OddsRecord
{
    public Guid Id { get; private set; }
    public Guid ProviderId { get; private set; }
    public string EventKey { get; private set; } = string.Empty;
    public string Market { get; private set; } = string.Empty;
    public string Selection { get; private set; } = string.Empty;
    public decimal Odds { get; private set; }
    public string? RawPayload { get; private set; }
    public DateTimeOffset IngestedAt { get; private set; }

    private OddsRecord() { }

    public static OddsRecord Create(
        Guid providerId,
        string eventKey,
        string market,
        string selection,
        decimal odds,
        string? rawPayload,
        DateTimeOffset now)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(eventKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(market);
        ArgumentException.ThrowIfNullOrWhiteSpace(selection);

        if (odds <= 0)
            throw new ArgumentOutOfRangeException(nameof(odds), "Odds must be positive.");

        return new OddsRecord
        {
            Id = Guid.NewGuid(),
            ProviderId = providerId,
            EventKey = eventKey,
            Market = market,
            Selection = selection,
            Odds = odds,
            RawPayload = rawPayload,
            IngestedAt = now
        };
    }
}
