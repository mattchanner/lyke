using Lyke.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lyke.Infrastructure.Data.Configurations;

public class SponsoredPlacementConfiguration : IEntityTypeConfiguration<SponsoredPlacement>
{
    public void Configure(EntityTypeBuilder<SponsoredPlacement> builder)
    {
        builder.HasKey(sp => sp.Id);

        builder.Property(sp => sp.BudgetAmount)
            .HasPrecision(10, 2);

        builder.Property(sp => sp.SpentAmount)
            .HasPrecision(10, 2);

        builder.Property(sp => sp.TargetBodyTypes)
            .HasColumnType("jsonb");

        builder.Property(sp => sp.TargetCategories)
            .HasColumnType("jsonb");

        builder.HasOne(sp => sp.Retailer)
            .WithMany(r => r.SponsoredPlacements)
            .HasForeignKey(sp => sp.RetailerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(sp => sp.Product)
            .WithMany()
            .HasForeignKey(sp => sp.ProductId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(sp => new { sp.IsActive, sp.StartDate, sp.EndDate })
            .HasDatabaseName("IX_SponsoredPlacements_IsActive_Dates");
    }
}
