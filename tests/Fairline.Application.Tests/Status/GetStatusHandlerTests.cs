using Fairline.Abstractions.Interfaces;
using Fairline.Application.Status;

namespace Fairline.Application.Tests.Status;

public sealed class GetStatusHandlerTests
{
    private readonly IClock _clock = Substitute.For<IClock>();
    private readonly IDbContextChecker _dbChecker = Substitute.For<IDbContextChecker>();
    private readonly GetStatusHandler _handler;

    public GetStatusHandlerTests()
    {
        _handler = new GetStatusHandler(_clock, _dbChecker);
    }

    [Fact]
    public async Task HandleAsync_ReturnsConnectedWhenDbIsHealthy()
    {
        var now = new DateTimeOffset(2025, 1, 15, 12, 0, 0, TimeSpan.Zero);
        _clock.UtcNow.Returns(now);
        _dbChecker.CanConnectAsync(Arg.Any<CancellationToken>()).Returns(true);

        var result = await _handler.HandleAsync();

        result.DatabaseConnected.Should().BeTrue();
        result.Timestamp.Should().Be(now);
    }

    [Fact]
    public async Task HandleAsync_ReturnsDisconnectedWhenDbIsUnhealthy()
    {
        _clock.UtcNow.Returns(DateTimeOffset.UtcNow);
        _dbChecker.CanConnectAsync(Arg.Any<CancellationToken>()).Returns(false);

        var result = await _handler.HandleAsync();

        result.DatabaseConnected.Should().BeFalse();
    }
}
