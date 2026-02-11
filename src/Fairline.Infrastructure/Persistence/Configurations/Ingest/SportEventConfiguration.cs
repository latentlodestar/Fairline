using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Fairline.Domain.Ingest;

namespace Fairline.Infrastructure.Persistence.Configurations.Ingest;

public sealed class SportEventConfiguration : IEntityTypeConfiguration<SportEvent>
{
    public void Configure(EntityTypeBuilder<SportEvent> builder)
    {
        builder.ToTable("events");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();

        builder.Property(e => e.ProviderEventId).HasMaxLength(200).IsRequired();
        builder.Property(e => e.SportKey).HasMaxLength(200).IsRequired();
        builder.Property(e => e.SportTitle).HasMaxLength(200).IsRequired();
        builder.Property(e => e.HomeTeam).HasMaxLength(300).IsRequired();
        builder.Property(e => e.AwayTeam).HasMaxLength(300).IsRequired();
        builder.Property(e => e.CommenceTimeUtc).IsRequired();
        builder.Property(e => e.CreatedAtUtc).IsRequired();
        builder.Property(e => e.UpdatedAtUtc).IsRequired();

        builder.HasIndex(e => e.ProviderEventId).IsUnique();
        builder.HasIndex(e => e.SportKey);
        builder.HasIndex(e => e.CommenceTimeUtc);
    }
}
