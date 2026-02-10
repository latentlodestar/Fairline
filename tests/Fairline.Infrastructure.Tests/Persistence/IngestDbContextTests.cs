using Microsoft.EntityFrameworkCore;
using Fairline.Domain.Ingest;
using Fairline.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace Fairline.Infrastructure.Tests.Persistence;

public sealed class IngestDbContextTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:17-alpine")
        .Build();

    public ValueTask InitializeAsync() => new(_postgres.StartAsync());

    public ValueTask DisposeAsync() => _postgres.DisposeAsync();

    private IngestDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<IngestDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .Options;

        return new IngestDbContext(options);
    }

    [Fact]
    public async Task CanApplyMigrations()
    {
        await using var context = CreateContext();

        await context.Database.EnsureCreatedAsync();

        var canConnect = await context.Database.CanConnectAsync();
        canConnect.Should().BeTrue();
    }

    [Fact]
    public async Task ProviderTable_IsInIngestSchema()
    {
        await using var context = CreateContext();
        await context.Database.EnsureCreatedAsync();

        var entityType = context.Model.FindEntityType(typeof(Provider));
        var schema = entityType?.GetSchema();

        schema.Should().Be("ingest");
    }

    [Fact]
    public async Task CanInsertAndReadProvider()
    {
        await using var context = CreateContext();
        await context.Database.EnsureCreatedAsync();

        var provider = Provider.Create("TestProvider", "test-provider", DateTimeOffset.UtcNow);
        context.Providers.Add(provider);
        await context.SaveChangesAsync();

        await using var readContext = CreateContext();
        var loaded = await readContext.Providers.FirstOrDefaultAsync(p => p.Id == provider.Id);

        loaded.Should().NotBeNull();
        loaded!.Name.Should().Be("TestProvider");
        loaded.Slug.Should().Be("test-provider");
    }
}
