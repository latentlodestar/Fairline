using Fairline.Abstractions.Contracts;
using Fairline.Abstractions.Interfaces;
using Fairline.Application.Modeling;

namespace Fairline.Application.Tests.Modeling;

public sealed class GetScenariosHandlerTests
{
    private readonly IScenarioRepository _repository = Substitute.For<IScenarioRepository>();
    private readonly GetScenariosHandler _handler;

    public GetScenariosHandlerTests()
    {
        _handler = new GetScenariosHandler(_repository);
    }

    [Fact]
    public async Task HandleAsync_ReturnsScenariosFromRepository()
    {
        var expected = new List<ScenarioSummary>
        {
            new(Guid.NewGuid(), "NFL Week 1", "Description", 5)
        };
        _repository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(expected);

        var result = await _handler.HandleAsync();

        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task HandleAsync_ReturnsEmptyWhenNoScenarios()
    {
        _repository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(new List<ScenarioSummary>());

        var result = await _handler.HandleAsync();

        result.Should().BeEmpty();
    }
}
