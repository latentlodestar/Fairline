using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Fairline.Domain.Modeling;

namespace Fairline.Infrastructure.Persistence.Configurations.Modeling;

public sealed class ScenarioComparisonConfiguration : IEntityTypeConfiguration<ScenarioComparison>
{
    public void Configure(EntityTypeBuilder<ScenarioComparison> builder)
    {
        builder.ToTable("scenario_comparisons");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedNever();

        builder.Property(c => c.ScenarioId).IsRequired();
        builder.Property(c => c.EventKey).HasMaxLength(300).IsRequired();
        builder.Property(c => c.Market).HasMaxLength(200).IsRequired();
        builder.Property(c => c.Selection).HasMaxLength(200).IsRequired();
        builder.Property(c => c.ProviderOdds).HasPrecision(18, 6).IsRequired();
        builder.Property(c => c.ModeledOdds).HasPrecision(18, 6).IsRequired();
        builder.Property(c => c.Edge).HasPrecision(18, 6).IsRequired();
        builder.Property(c => c.ComputedAt).IsRequired();

        builder.HasIndex(c => c.ScenarioId);
        builder.HasIndex(c => new { c.EventKey, c.Market });
    }
}
