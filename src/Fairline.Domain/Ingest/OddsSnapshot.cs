namespace Fairline.Domain.Ingest;

public sealed class OddsSnapshot
{
    public Guid Id { get; private set; }
    public Guid SportEventId { get; private set; }
    public string BookmakerKey { get; private set; } = string.Empty;
    public string BookmakerTitle { get; private set; } = string.Empty;
    public string MarketKey { get; private set; } = string.Empty;
    public string OutcomeName { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public decimal? Point { get; private set; }
    public DateTimeOffset? ProviderLastUpdate { get; private set; }
    public DateTimeOffset CapturedAtUtc { get; private set; }

    private OddsSnapshot() { }

    public static OddsSnapshot Create(
        Guid sportEventId,
        string bookmakerKey,
        string bookmakerTitle,
        string marketKey,
        string outcomeName,
        decimal price,
        decimal? point,
        DateTimeOffset? providerLastUpdate,
        DateTimeOffset capturedAtUtc)
    {
        return new OddsSnapshot
        {
            Id = Guid.NewGuid(),
            SportEventId = sportEventId,
            BookmakerKey = bookmakerKey,
            BookmakerTitle = bookmakerTitle,
            MarketKey = marketKey,
            OutcomeName = outcomeName,
            Price = price,
            Point = point,
            ProviderLastUpdate = providerLastUpdate,
            CapturedAtUtc = capturedAtUtc
        };
    }
}
