using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Fairline.Domain.Ingest;

namespace Fairline.Infrastructure.Persistence.Configurations.Ingest;

public sealed class SportCatalogConfiguration : IEntityTypeConfiguration<SportCatalog>
{
    public void Configure(EntityTypeBuilder<SportCatalog> builder)
    {
        builder.ToTable("sport_catalog");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedNever();

        builder.Property(s => s.ProviderSportKey).HasMaxLength(200).IsRequired();
        builder.Property(s => s.Title).HasMaxLength(300).IsRequired();
        builder.Property(s => s.Group).HasMaxLength(200).IsRequired();
        builder.Property(s => s.Active).IsRequired();
        builder.Property(s => s.HasOutrights).IsRequired();
        builder.Property(s => s.NormalizedSport).HasMaxLength(200).IsRequired();
        builder.Property(s => s.NormalizedLeague).HasMaxLength(200).IsRequired();
        builder.Property(s => s.CapturedAtUtc).IsRequired();

        builder.HasIndex(s => s.ProviderSportKey).IsUnique();
        builder.HasIndex(s => s.NormalizedSport);
    }
}
