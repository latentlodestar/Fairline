namespace Fairline.Abstractions.Interfaces;

public interface IOddsIngestor
{
    string ProviderSlug { get; }
    Task IngestAsync(CancellationToken ct = default);
}
