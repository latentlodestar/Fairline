namespace Fairline.Domain.Ingest;

public sealed class ProviderCatalogSnapshot
{
    public Guid Id { get; private set; }
    public string Provider { get; private set; } = string.Empty;
    public string RawJson { get; private set; } = string.Empty;
    public int SportCount { get; private set; }
    public DateTimeOffset CapturedAtUtc { get; private set; }

    private ProviderCatalogSnapshot() { }

    public static ProviderCatalogSnapshot Create(
        string provider,
        string rawJson,
        int sportCount,
        DateTimeOffset capturedAtUtc)
    {
        return new ProviderCatalogSnapshot
        {
            Id = Guid.NewGuid(),
            Provider = provider,
            RawJson = rawJson,
            SportCount = sportCount,
            CapturedAtUtc = capturedAtUtc
        };
    }
}
