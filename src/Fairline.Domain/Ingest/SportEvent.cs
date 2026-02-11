namespace Fairline.Domain.Ingest;

public sealed class SportEvent
{
    public Guid Id { get; private set; }
    public string ProviderEventId { get; private set; } = string.Empty;
    public string SportKey { get; private set; } = string.Empty;
    public string SportTitle { get; private set; } = string.Empty;
    public string HomeTeam { get; private set; } = string.Empty;
    public string AwayTeam { get; private set; } = string.Empty;
    public DateTimeOffset CommenceTimeUtc { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }

    private SportEvent() { }

    public static SportEvent Create(
        string providerEventId,
        string sportKey,
        string sportTitle,
        string homeTeam,
        string awayTeam,
        DateTimeOffset commenceTimeUtc,
        DateTimeOffset now)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(providerEventId);

        return new SportEvent
        {
            Id = Guid.NewGuid(),
            ProviderEventId = providerEventId,
            SportKey = sportKey,
            SportTitle = sportTitle,
            HomeTeam = homeTeam,
            AwayTeam = awayTeam,
            CommenceTimeUtc = commenceTimeUtc,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };
    }

    public void Update(string sportTitle, string homeTeam, string awayTeam, DateTimeOffset commenceTimeUtc, DateTimeOffset now)
    {
        SportTitle = sportTitle;
        HomeTeam = homeTeam;
        AwayTeam = awayTeam;
        CommenceTimeUtc = commenceTimeUtc;
        UpdatedAtUtc = now;
    }
}
