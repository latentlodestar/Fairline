using Fairline.Domain.Ingest;

namespace Fairline.Domain.Tests.Ingest;

public sealed class ProviderTests
{
    private static readonly DateTimeOffset Now = new(2025, 1, 15, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_SetsAllProperties()
    {
        var provider = Provider.Create("DraftKings", "draftkings", Now);

        provider.Id.Should().NotBeEmpty();
        provider.Name.Should().Be("DraftKings");
        provider.Slug.Should().Be("draftkings");
        provider.IsActive.Should().BeTrue();
        provider.CreatedAt.Should().Be(Now);
        provider.UpdatedAt.Should().Be(Now);
    }

    [Fact]
    public void Create_NormalizesSlugToLower()
    {
        var provider = Provider.Create("Test", "UPPER-SLUG", Now);

        provider.Slug.Should().Be("upper-slug");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ThrowsOnInvalidName(string? name)
    {
        var act = () => Provider.Create(name!, "slug", Now);

        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ThrowsOnInvalidSlug(string? slug)
    {
        var act = () => Provider.Create("Name", slug!, Now);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Deactivate_SetsIsActiveFalseAndUpdatesTimestamp()
    {
        var provider = Provider.Create("Test", "test", Now);
        var later = Now.AddHours(1);

        provider.Deactivate(later);

        provider.IsActive.Should().BeFalse();
        provider.UpdatedAt.Should().Be(later);
    }

    [Fact]
    public void Activate_SetsIsActiveTrueAndUpdatesTimestamp()
    {
        var provider = Provider.Create("Test", "test", Now);
        var later = Now.AddHours(1);
        provider.Deactivate(later);
        var evenLater = Now.AddHours(2);

        provider.Activate(evenLater);

        provider.IsActive.Should().BeTrue();
        provider.UpdatedAt.Should().Be(evenLater);
    }
}
