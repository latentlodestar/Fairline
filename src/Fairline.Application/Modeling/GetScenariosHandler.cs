using Fairline.Abstractions.Contracts;
using Fairline.Abstractions.Interfaces;

namespace Fairline.Application.Modeling;

public sealed class GetScenariosHandler(IScenarioRepository repository)
{
    public Task<IReadOnlyList<ScenarioSummary>> HandleAsync(CancellationToken ct = default)
        => repository.GetAllAsync(ct);
}
