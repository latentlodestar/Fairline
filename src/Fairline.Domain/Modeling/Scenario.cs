namespace Fairline.Domain.Modeling;

public sealed class Scenario
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private readonly List<ScenarioComparison> _comparisons = [];
    public IReadOnlyList<ScenarioComparison> Comparisons => _comparisons.AsReadOnly();

    private Scenario() { }

    public static Scenario Create(string name, string? description, DateTimeOffset now)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new Scenario
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public ScenarioComparison AddComparison(
        string eventKey,
        string market,
        string selection,
        decimal providerOdds,
        decimal modeledOdds,
        DateTimeOffset now)
    {
        var comparison = ScenarioComparison.Create(
            Id, eventKey, market, selection, providerOdds, modeledOdds, now);

        _comparisons.Add(comparison);
        UpdatedAt = now;

        return comparison;
    }
}
