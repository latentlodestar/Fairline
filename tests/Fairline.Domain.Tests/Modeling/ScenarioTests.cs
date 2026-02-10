using Fairline.Domain.Modeling;

namespace Fairline.Domain.Tests.Modeling;

public sealed class ScenarioTests
{
    private static readonly DateTimeOffset Now = new(2025, 1, 15, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_SetsAllProperties()
    {
        var scenario = Scenario.Create("NFL Week 1", "Test description", Now);

        scenario.Id.Should().NotBeEmpty();
        scenario.Name.Should().Be("NFL Week 1");
        scenario.Description.Should().Be("Test description");
        scenario.CreatedAt.Should().Be(Now);
        scenario.Comparisons.Should().BeEmpty();
    }

    [Fact]
    public void Create_AllowsNullDescription()
    {
        var scenario = Scenario.Create("Test", null, Now);

        scenario.Description.Should().BeNull();
    }

    [Fact]
    public void AddComparison_CreatesAndAddsComparison()
    {
        var scenario = Scenario.Create("Test", null, Now);
        var later = Now.AddMinutes(5);

        var comparison = scenario.AddComparison("nfl:dal-vs-phi", "moneyline", "DAL", 2.10m, 2.25m, later);

        scenario.Comparisons.Should().HaveCount(1);
        comparison.ScenarioId.Should().Be(scenario.Id);
        comparison.Edge.Should().Be(0.15m);
        scenario.UpdatedAt.Should().Be(later);
    }

    [Fact]
    public void AddComparison_ThrowsWhenProviderOddsNotPositive()
    {
        var scenario = Scenario.Create("Test", null, Now);

        var act = () => scenario.AddComparison("event", "market", "sel", 0m, 2.0m, Now);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ThrowsOnInvalidName(string? name)
    {
        var act = () => Scenario.Create(name!, null, Now);

        act.Should().Throw<ArgumentException>();
    }
}
