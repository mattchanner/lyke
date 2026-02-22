using Lyke.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lyke.Infrastructure.Data.Configurations;

public class DailyMetricSnapshotConfiguration : IEntityTypeConfiguration<DailyMetricSnapshot>
{
    public void Configure(EntityTypeBuilder<DailyMetricSnapshot> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Scope)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.Revenue)
            .HasPrecision(18, 2);

        builder.Property(e => e.Earnings)
            .HasPrecision(18, 2);

        builder.HasIndex(e => new { e.Date, e.Scope })
            .IsUnique()
            .HasDatabaseName("IX_DailyMetricSnapshots_Date_Scope");
    }
}
