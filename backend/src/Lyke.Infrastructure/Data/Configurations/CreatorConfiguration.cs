using Lyke.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lyke.Infrastructure.Data.Configurations;

public class CreatorConfiguration : IEntityTypeConfiguration<Creator>
{
    public void Configure(EntityTypeBuilder<Creator> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.DisplayName).HasMaxLength(100).IsRequired();

        builder.Property(c => c.Bio).HasMaxLength(1000);

        builder.Property(c => c.SocialLinks).HasColumnType("jsonb");

        builder.Property(c => c.PayoutDetails).HasMaxLength(500);

        // Verification fields
        builder
            .Property(c => c.VerificationStatus)
            .HasDefaultValue(Core.Enums.VerificationStatus.NotSubmitted);

        builder.Property(c => c.VerificationNotes).HasMaxLength(1000);

        builder.Property(c => c.VerificationDocumentUrls).HasColumnType("jsonb");

        builder.Property(c => c.VerificationRejectionReason).HasMaxLength(500);

        // Navigation properties
        builder
            .HasMany(c => c.Posts)
            .WithOne(p => p.Creator)
            .HasForeignKey(p => p.CreatorId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(c => c.VerificationReviewedByUser)
            .WithMany()
            .HasForeignKey(c => c.VerificationReviewedByUserId)
            .OnDelete(DeleteBehavior.SetNull);

        // Index for finding pending verifications
        builder.HasIndex(c => c.VerificationStatus);
    }
}
