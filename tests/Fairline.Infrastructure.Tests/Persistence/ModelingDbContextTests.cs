using Microsoft.EntityFrameworkCore;
using Fairline.Domain.Modeling;
using Fairline.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace Fairline.Infrastructure.Tests.Persistence;

public sealed class ModelingDbContextTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:17-alpine")
        .Build();

    public ValueTask InitializeAsync() => new(_postgres.StartAsync());

    public ValueTask DisposeAsync() => _postgres.DisposeAsync();

    private ModelingDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ModelingDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .Options;

        return new ModelingDbContext(options);
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
    public async Task ScenarioTable_IsInModelingSchema()
    {
        await using var context = CreateContext();
        await context.Database.EnsureCreatedAsync();

        var entityType = context.Model.FindEntityType(typeof(Scenario));
        var schema = entityType?.GetSchema();

        schema.Should().Be("modeling");
    }

    [Fact]
    public async Task CanInsertScenarioWithComparison()
    {
        await using var context = CreateContext();
        await context.Database.EnsureCreatedAsync();

        var now = DateTimeOffset.UtcNow;
        var scenario = Scenario.Create("Test Scenario", "Description", now);
        scenario.AddComparison("nfl:dal-vs-phi", "moneyline", "DAL", 2.10m, 2.25m, now);

        context.Scenarios.Add(scenario);
        await context.SaveChangesAsync();

        await using var readContext = CreateContext();
        var loaded = await readContext.Scenarios
            .Include(s => s.Comparisons)
            .FirstOrDefaultAsync(s => s.Id == scenario.Id);

        loaded.Should().NotBeNull();
        loaded!.Comparisons.Should().HaveCount(1);
        loaded.Comparisons[0].Edge.Should().Be(0.15m);
    }
}
