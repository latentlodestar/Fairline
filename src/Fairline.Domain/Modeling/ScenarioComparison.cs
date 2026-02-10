namespace Fairline.Domain.Modeling;

public sealed class ScenarioComparison
{
    public Guid Id { get; private set; }
    public Guid ScenarioId { get; private set; }
    public string EventKey { get; private set; } = string.Empty;
    public string Market { get; private set; } = string.Empty;
    public string Selection { get; private set; } = string.Empty;
    public decimal ProviderOdds { get; private set; }
    public decimal ModeledOdds { get; private set; }
    public decimal Edge { get; private set; }
    public DateTimeOffset ComputedAt { get; private set; }

    private ScenarioComparison() { }

    internal static ScenarioComparison Create(
        Guid scenarioId,
        string eventKey,
        string market,
        string selection,
        decimal providerOdds,
        decimal modeledOdds,
        DateTimeOffset now)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(eventKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(market);
        ArgumentException.ThrowIfNullOrWhiteSpace(selection);

        if (providerOdds <= 0)
            throw new ArgumentOutOfRangeException(nameof(providerOdds), "Provider odds must be positive.");
        if (modeledOdds <= 0)
            throw new ArgumentOutOfRangeException(nameof(modeledOdds), "Modeled odds must be positive.");

        return new ScenarioComparison
        {
            Id = Guid.NewGuid(),
            ScenarioId = scenarioId,
            EventKey = eventKey,
            Market = market,
            Selection = selection,
            ProviderOdds = providerOdds,
            ModeledOdds = modeledOdds,
            Edge = modeledOdds - providerOdds,
            ComputedAt = now
        };
    }
}
