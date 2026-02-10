namespace Fairline.Abstractions.Contracts;

public sealed record ScenarioSummary(
    Guid Id,
    string Name,
    string? Description,
    int ComparisonCount);
