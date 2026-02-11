using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Fairline.Domain.Ingest;

namespace Fairline.Infrastructure.Persistence.Configurations.Ingest;

public sealed class TrackedLeagueConfiguration : IEntityTypeConfiguration<TrackedLeague>
{
    public void Configure(EntityTypeBuilder<TrackedLeague> builder)
    {
        builder.ToTable("tracked_leagues");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).ValueGeneratedNever();

        builder.Property(t => t.Provider).HasMaxLength(100).IsRequired();
        builder.Property(t => t.ProviderSportKey).HasMaxLength(200).IsRequired();
        builder.Property(t => t.Enabled).IsRequired();
        builder.Property(t => t.CreatedAtUtc).IsRequired();
        builder.Property(t => t.UpdatedAtUtc).IsRequired();

        builder.HasIndex(t => new { t.Provider, t.ProviderSportKey }).IsUnique();
        builder.HasIndex(t => t.Enabled);
    }
}
