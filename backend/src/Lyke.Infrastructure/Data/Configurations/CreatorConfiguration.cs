using Lyke.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lyke.Infrastructure.Data.Configurations;

public class CreatorConfiguration : IEntityTypeConfiguration<Creator>
{
    public void Configure(EntityTypeBuilder<Creator> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.DisplayName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(c => c.Bio)
            .HasMaxLength(1000);

        builder.Property(c => c.SocialLinks)
            .HasColumnType("jsonb");

        builder.Property(c => c.PayoutDetails)
            .HasMaxLength(500);

        builder.HasMany(c => c.Posts)
            .WithOne(p => p.Creator)
            .HasForeignKey(p => p.CreatorId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
