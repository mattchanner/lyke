using Lyke.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lyke.Infrastructure.Data.Configurations;

public class CreatorEarningConfiguration : IEntityTypeConfiguration<CreatorEarning>
{
    public void Configure(EntityTypeBuilder<CreatorEarning> builder)
    {
        builder.HasKey(ce => ce.Id);

        builder.Property(ce => ce.EarningType).HasConversion<string>().HasMaxLength(20);

        builder.Property(ce => ce.Amount).HasPrecision(10, 2);

        builder.Property(ce => ce.Currency).HasMaxLength(3).IsRequired();

        builder.Property(ce => ce.Status).HasConversion<string>().HasMaxLength(20);

        builder
            .HasOne(ce => ce.Creator)
            .WithMany(c => c.Earnings)
            .HasForeignKey(ce => ce.CreatorId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(ce => ce.ClickEvent)
            .WithOne(c => c.Earning)
            .HasForeignKey<CreatorEarning>(ce => ce.ClickEventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasIndex(ce => new { ce.CreatorId, ce.Status })
            .HasDatabaseName("IX_CreatorEarnings_CreatorId_Status");
    }
}
