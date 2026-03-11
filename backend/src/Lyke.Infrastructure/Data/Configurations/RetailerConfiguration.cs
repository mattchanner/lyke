using Lyke.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lyke.Infrastructure.Data.Configurations;

public class RetailerConfiguration : IEntityTypeConfiguration<Retailer>
{
    public void Configure(EntityTypeBuilder<Retailer> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name).HasMaxLength(200).IsRequired();

        builder.Property(r => r.LogoUrl).HasMaxLength(500);

        builder.Property(r => r.WebsiteUrl).HasMaxLength(500);

        builder.Property(r => r.ContactEmail).HasMaxLength(200);

        builder.Property(r => r.AffiliateConfig).HasColumnType("jsonb");

        builder.HasIndex(r => r.UserId).IsUnique().HasDatabaseName("IX_Retailers_UserId");

        builder
            .HasMany(r => r.Products)
            .WithOne(p => p.Retailer)
            .HasForeignKey(p => p.RetailerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
