using Fairline.Abstractions.Contracts;
using Fairline.Application.Ingest;

namespace Fairline.Application.Tests.Ingest;

public sealed class GapPlannerTests
{
    private static readonly DateTimeOffset Now = new(2025, 6, 1, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void DetermineLeaguesToRefresh_RefreshesStaleLeagues()
    {
        var leagues = new List<TrackedLeagueState>
        {
            // Event in 2 hours, last snapshot 15 minutes ago => stale (10-min window)
            new("the-odds-api", "basketball_nba", true,
                Now.AddHours(2), Now.AddMinutes(-15)),
        };

        var result = GapPlanner.DetermineLeaguesToRefresh(leagues, Now);
        result.Should().ContainSingle().Which.Should().Be("basketball_nba");
    }

    [Fact]
    public void DetermineLeaguesToRefresh_SkipsFreshLeagues()
    {
        var leagues = new List<TrackedLeagueState>
        {
            // Event in 2 hours, last snapshot 5 minutes ago => fresh (10-min window)
            new("the-odds-api", "basketball_nba", true,
                Now.AddHours(2), Now.AddMinutes(-5)),
        };

        var result = GapPlanner.DetermineLeaguesToRefresh(leagues, Now);
        result.Should().BeEmpty();
    }

    [Fact]
    public void DetermineLeaguesToRefresh_Uses60MinWindowFor24To72Hours()
    {
        var leagues = new List<TrackedLeagueState>
        {
            // Event in 48 hours, last snapshot 30 minutes ago => fresh (60-min window)
            new("the-odds-api", "football_nfl", true,
                Now.AddHours(48), Now.AddMinutes(-30)),
        };

        var result = GapPlanner.DetermineLeaguesToRefresh(leagues, Now);
        result.Should().BeEmpty();
    }

    [Fact]
    public void DetermineLeaguesToRefresh_Uses60MinWindowStale()
    {
        var leagues = new List<TrackedLeagueState>
        {
            // Event in 48 hours, last snapshot 65 minutes ago => stale
            new("the-odds-api", "football_nfl", true,
                Now.AddHours(48), Now.AddMinutes(-65)),
        };

        var result = GapPlanner.DetermineLeaguesToRefresh(leagues, Now);
        result.Should().ContainSingle().Which.Should().Be("football_nfl");
    }

    [Fact]
    public void DetermineLeaguesToRefresh_Uses6HourWindowForDistantEvents()
    {
        var leagues = new List<TrackedLeagueState>
        {
            // Event in 5 days, last snapshot 3 hours ago => fresh (6-hour window)
            new("the-odds-api", "soccer_epl", true,
                Now.AddDays(5), Now.AddHours(-3)),
        };

        var result = GapPlanner.DetermineLeaguesToRefresh(leagues, Now);
        result.Should().BeEmpty();
    }

    [Fact]
    public void DetermineLeaguesToRefresh_Uses6HourWindowStale()
    {
        var leagues = new List<TrackedLeagueState>
        {
            // Event in 5 days, last snapshot 7 hours ago => stale
            new("the-odds-api", "soccer_epl", true,
                Now.AddDays(5), Now.AddHours(-7)),
        };

        var result = GapPlanner.DetermineLeaguesToRefresh(leagues, Now);
        result.Should().ContainSingle().Which.Should().Be("soccer_epl");
    }

    [Fact]
    public void DetermineLeaguesToRefresh_AlwaysRefreshesWhenNoSnapshot()
    {
        var leagues = new List<TrackedLeagueState>
        {
            new("the-odds-api", "basketball_nba", true, Now.AddHours(2), null),
        };

        var result = GapPlanner.DetermineLeaguesToRefresh(leagues, Now);
        result.Should().ContainSingle().Which.Should().Be("basketball_nba");
    }

    [Fact]
    public void DetermineLeaguesToRefresh_SkipsDisabledLeagues()
    {
        var leagues = new List<TrackedLeagueState>
        {
            new("the-odds-api", "basketball_nba", false, Now.AddHours(2), null),
        };

        var result = GapPlanner.DetermineLeaguesToRefresh(leagues, Now);
        result.Should().BeEmpty();
    }

    [Fact]
    public void DetermineLeaguesToRefresh_HandlesNoEventTime()
    {
        var leagues = new List<TrackedLeagueState>
        {
            // No events known, no snapshots => refresh (6-hour default)
            new("the-odds-api", "golf_pga", true, null, null),
        };

        var result = GapPlanner.DetermineLeaguesToRefresh(leagues, Now);
        result.Should().ContainSingle().Which.Should().Be("golf_pga");
    }

    [Fact]
    public void DetermineLeaguesToRefresh_MultipleLeaguesMixed()
    {
        var leagues = new List<TrackedLeagueState>
        {
            new("the-odds-api", "basketball_nba", true, Now.AddHours(2), Now.AddMinutes(-15)),   // stale
            new("the-odds-api", "football_nfl", true, Now.AddHours(48), Now.AddMinutes(-30)),     // fresh
            new("the-odds-api", "soccer_epl", true, Now.AddDays(5), Now.AddHours(-7)),            // stale
            new("the-odds-api", "hockey_nhl", false, Now.AddHours(1), null),                      // disabled
        };

        var result = GapPlanner.DetermineLeaguesToRefresh(leagues, Now);
        result.Should().HaveCount(2);
        result.Should().Contain("basketball_nba");
        result.Should().Contain("soccer_epl");
    }

    [Fact]
    public void GetRequiredFreshness_ReturnsCorrectWindows()
    {
        GapPlanner.GetRequiredFreshness(Now.AddHours(1), Now).Should().Be(TimeSpan.FromMinutes(10));
        GapPlanner.GetRequiredFreshness(Now.AddHours(24), Now).Should().Be(TimeSpan.FromMinutes(10));
        GapPlanner.GetRequiredFreshness(Now.AddHours(25), Now).Should().Be(TimeSpan.FromMinutes(60));
        GapPlanner.GetRequiredFreshness(Now.AddHours(72), Now).Should().Be(TimeSpan.FromMinutes(60));
        GapPlanner.GetRequiredFreshness(Now.AddHours(73), Now).Should().Be(TimeSpan.FromHours(6));
        GapPlanner.GetRequiredFreshness(null, Now).Should().Be(TimeSpan.FromHours(6));
    }
}
