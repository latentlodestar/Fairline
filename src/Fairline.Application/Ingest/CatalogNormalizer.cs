using Fairline.Abstractions.Contracts;

namespace Fairline.Application.Ingest;

public static class CatalogNormalizer
{
    public static IReadOnlyList<SportCatalogEntry> Normalize(
        IReadOnlyList<OddsApiSport> sports, DateTimeOffset capturedAtUtc)
    {
        return sports.Select(s => new SportCatalogEntry(
            ProviderSportKey: s.Key,
            Title: s.Title,
            Group: s.Group,
            Active: s.Active,
            HasOutrights: s.HasOutrights,
            NormalizedSport: NormalizeSport(s.Group),
            NormalizedLeague: NormalizeLeague(s.Key, s.Title),
            CapturedAtUtc: capturedAtUtc
        )).ToList();
    }

    public static string NormalizeSport(string group)
        => group.ToLowerInvariant().Replace(" ", "_");

    public static string NormalizeLeague(string key, string title)
    {
        var idx = key.IndexOf('_');
        if (idx >= 0 && idx < key.Length - 1)
            return key[(idx + 1)..].ToLowerInvariant();

        return title.ToLowerInvariant().Replace(" ", "_");
    }
}
