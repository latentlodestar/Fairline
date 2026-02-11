namespace Fairline.Domain.Ingest;

public sealed class SportCatalog
{
    public Guid Id { get; private set; }
    public string ProviderSportKey { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string Group { get; private set; } = string.Empty;
    public bool Active { get; private set; }
    public bool HasOutrights { get; private set; }
    public string NormalizedSport { get; private set; } = string.Empty;
    public string NormalizedLeague { get; private set; } = string.Empty;
    public DateTimeOffset CapturedAtUtc { get; private set; }

    private SportCatalog() { }

    public static SportCatalog Create(
        string providerSportKey,
        string title,
        string group,
        bool active,
        bool hasOutrights,
        string normalizedSport,
        string normalizedLeague,
        DateTimeOffset capturedAtUtc)
    {
        return new SportCatalog
        {
            Id = Guid.NewGuid(),
            ProviderSportKey = providerSportKey,
            Title = title,
            Group = group,
            Active = active,
            HasOutrights = hasOutrights,
            NormalizedSport = normalizedSport,
            NormalizedLeague = normalizedLeague,
            CapturedAtUtc = capturedAtUtc
        };
    }

    public void Update(string title, string group, bool active, bool hasOutrights,
        string normalizedSport, string normalizedLeague, DateTimeOffset capturedAtUtc)
    {
        Title = title;
        Group = group;
        Active = active;
        HasOutrights = hasOutrights;
        NormalizedSport = normalizedSport;
        NormalizedLeague = normalizedLeague;
        CapturedAtUtc = capturedAtUtc;
    }
}
