using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Fairline.Domain.Ingest;

namespace Fairline.Infrastructure.Persistence.Configurations.Ingest;

public sealed class OddsSnapshotConfiguration : IEntityTypeConfiguration<OddsSnapshot>
{
    public void Configure(EntityTypeBuilder<OddsSnapshot> builder)
    {
        builder.ToTable("odds_snapshots");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedNever();

        builder.Property(s => s.SportEventId).IsRequired();
        builder.Property(s => s.BookmakerKey).HasMaxLength(100).IsRequired();
        builder.Property(s => s.BookmakerTitle).HasMaxLength(200).IsRequired();
        builder.Property(s => s.MarketKey).HasMaxLength(100).IsRequired();
        builder.Property(s => s.OutcomeName).HasMaxLength(300).IsRequired();
        builder.Property(s => s.Price).HasPrecision(18, 6).IsRequired();
        builder.Property(s => s.Point).HasPrecision(18, 6);
        builder.Property(s => s.ProviderLastUpdate);
        builder.Property(s => s.CapturedAtUtc).IsRequired();

        builder.HasOne<SportEvent>()
            .WithMany()
            .HasForeignKey(s => s.SportEventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(s => s.SportEventId);
        builder.HasIndex(s => s.CapturedAtUtc).IsDescending();
        builder.HasIndex(s => new { s.SportEventId, s.MarketKey, s.OutcomeName, s.BookmakerKey, s.CapturedAtUtc })
            .IsDescending(false, false, false, false, true)
            .HasDatabaseName("ix_odds_snapshots_latest_by_key");
    }
}
