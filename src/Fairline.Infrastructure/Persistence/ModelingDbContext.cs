using Microsoft.EntityFrameworkCore;
using Fairline.Domain.Modeling;

namespace Fairline.Infrastructure.Persistence;

public sealed class ModelingDbContext(DbContextOptions<ModelingDbContext> options) : DbContext(options)
{
    public const string Schema = "modeling";

    public DbSet<Scenario> Scenarios => Set<Scenario>();
    public DbSet<ScenarioComparison> ScenarioComparisons => Set<ScenarioComparison>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(ModelingDbContext).Assembly,
            type => type.Namespace?.Contains("Configurations.Modeling") == true);
    }
}
