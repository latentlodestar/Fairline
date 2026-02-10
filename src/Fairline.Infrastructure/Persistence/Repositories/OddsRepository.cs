using Microsoft.EntityFrameworkCore;
using Fairline.Abstractions.Contracts;
using Fairline.Abstractions.Interfaces;

namespace Fairline.Infrastructure.Persistence.Repositories;

public sealed class OddsRepository(IngestDbContext db) : IOddsRepository
{
    public async Task<IReadOnlyList<ProviderInfo>> GetProvidersAsync(CancellationToken ct = default)
    {
        return await db.Providers
            .AsNoTracking()
            .Select(p => new ProviderInfo(p.Id, p.Name, p.Slug, p.IsActive))
            .ToListAsync(ct);
    }

    public async Task<ProviderInfo?> GetProviderBySlugAsync(string slug, CancellationToken ct = default)
    {
        return await db.Providers
            .AsNoTracking()
            .Where(p => p.Slug == slug)
            .Select(p => new ProviderInfo(p.Id, p.Name, p.Slug, p.IsActive))
            .FirstOrDefaultAsync(ct);
    }
}
