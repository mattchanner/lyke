using Lyke.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lyke.Infrastructure.Data.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.ExternalSku).HasMaxLength(100).IsRequired();

        builder.Property(p => p.Name).HasMaxLength(300).IsRequired();

        builder.Property(p => p.Description).HasMaxLength(2000);

        builder.Property(p => p.Category).HasMaxLength(100).IsRequired();

        builder.Property(p => p.SubCategory).HasMaxLength(100);

        builder.Property(p => p.ImageUrls).HasColumnType("jsonb");

        builder.Property(p => p.ProductUrl).HasMaxLength(1000).IsRequired();

        builder.Property(p => p.Price).HasPrecision(10, 2);

        builder.Property(p => p.Currency).HasMaxLength(3).IsRequired();

        builder
            .HasIndex(p => new
            {
                p.RetailerId,
                p.Category,
                p.IsActive,
            })
            .HasDatabaseName("IX_Products_RetailerId_Category_IsActive");

        builder
            .HasIndex(p => new { p.RetailerId, p.ExternalSku })
            .IsUnique()
            .HasDatabaseName("IX_Products_RetailerId_ExternalSku");
    }
}
