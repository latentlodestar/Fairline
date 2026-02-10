using Microsoft.EntityFrameworkCore;
using Fairline.Abstractions.Contracts;
using Fairline.Abstractions.Interfaces;

namespace Fairline.Infrastructure.Persistence.Repositories;

public sealed class ScenarioRepository(ModelingDbContext db) : IScenarioRepository
{
    public async Task<IReadOnlyList<ScenarioSummary>> GetAllAsync(CancellationToken ct = default)
    {
        return await db.Scenarios
            .AsNoTracking()
            .Select(s => new ScenarioSummary(
                s.Id,
                s.Name,
                s.Description,
                s.Comparisons.Count))
            .ToListAsync(ct);
    }

    public async Task<ScenarioSummary?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await db.Scenarios
            .AsNoTracking()
            .Where(s => s.Id == id)
            .Select(s => new ScenarioSummary(
                s.Id,
                s.Name,
                s.Description,
                s.Comparisons.Count))
            .FirstOrDefaultAsync(ct);
    }
}
