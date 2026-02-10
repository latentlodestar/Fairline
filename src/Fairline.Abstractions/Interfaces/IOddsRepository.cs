using Fairline.Abstractions.Contracts;

namespace Fairline.Abstractions.Interfaces;

public interface IOddsRepository
{
    Task<IReadOnlyList<ProviderInfo>> GetProvidersAsync(CancellationToken ct = default);
    Task<ProviderInfo?> GetProviderBySlugAsync(string slug, CancellationToken ct = default);
}
