using Fairline.Abstractions.Contracts;

namespace Fairline.Abstractions.Interfaces;

public interface IScenarioRepository
{
    Task<IReadOnlyList<ScenarioSummary>> GetAllAsync(CancellationToken ct = default);
    Task<ScenarioSummary?> GetByIdAsync(Guid id, CancellationToken ct = default);
}
