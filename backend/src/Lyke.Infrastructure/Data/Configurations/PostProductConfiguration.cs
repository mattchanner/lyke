using Lyke.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lyke.Infrastructure.Data.Configurations;

public class PostProductConfiguration : IEntityTypeConfiguration<PostProduct>
{
    public void Configure(EntityTypeBuilder<PostProduct> builder)
    {
        builder.HasKey(pp => pp.Id);

        builder.Property(pp => pp.SizeWorn)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(pp => pp.FitNotes)
            .HasMaxLength(500);

        builder.Property(pp => pp.FitRating)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(pp => pp.StylingNotes)
            .HasMaxLength(500);

        builder.HasOne(pp => pp.Post)
            .WithMany(p => p.PostProducts)
            .HasForeignKey(pp => pp.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pp => pp.Product)
            .WithMany(p => p.PostProducts)
            .HasForeignKey(pp => pp.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(pp => new { pp.PostId, pp.ProductId })
            .IsUnique()
            .HasDatabaseName("IX_PostProducts_PostId_ProductId");
    }
}
