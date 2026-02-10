using Fairline.Abstractions.Contracts;
using Fairline.Abstractions.Interfaces;
using Fairline.Application.Ingest;

namespace Fairline.Application.Tests.Ingest;

public sealed class GetProvidersHandlerTests
{
    private readonly IOddsRepository _repository = Substitute.For<IOddsRepository>();
    private readonly GetProvidersHandler _handler;

    public GetProvidersHandlerTests()
    {
        _handler = new GetProvidersHandler(_repository);
    }

    [Fact]
    public async Task HandleAsync_ReturnsProvidersFromRepository()
    {
        var expected = new List<ProviderInfo>
        {
            new(Guid.NewGuid(), "DraftKings", "draftkings", true),
            new(Guid.NewGuid(), "FanDuel", "fanduel", true)
        };
        _repository.GetProvidersAsync(Arg.Any<CancellationToken>()).Returns(expected);

        var result = await _handler.HandleAsync();

        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task HandleAsync_ReturnsEmptyWhenNoProviders()
    {
        _repository.GetProvidersAsync(Arg.Any<CancellationToken>()).Returns(new List<ProviderInfo>());

        var result = await _handler.HandleAsync();

        result.Should().BeEmpty();
    }
}
