namespace Fairline.Domain.Ingest;

public sealed class TrackedLeague
{
    public Guid Id { get; private set; }
    public string Provider { get; private set; } = string.Empty;
    public string ProviderSportKey { get; private set; } = string.Empty;
    public bool Enabled { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }

    private TrackedLeague() { }

    public static TrackedLeague Create(string provider, string providerSportKey, bool enabled, DateTimeOffset now)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(provider);
        ArgumentException.ThrowIfNullOrWhiteSpace(providerSportKey);

        return new TrackedLeague
        {
            Id = Guid.NewGuid(),
            Provider = provider,
            ProviderSportKey = providerSportKey,
            Enabled = enabled,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };
    }

    public void SetEnabled(bool enabled, DateTimeOffset now)
    {
        Enabled = enabled;
        UpdatedAtUtc = now;
    }
}
