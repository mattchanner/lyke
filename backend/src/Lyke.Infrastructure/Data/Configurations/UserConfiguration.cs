using Lyke.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lyke.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.Property(u => u.UserType).HasConversion<string>().HasMaxLength(20);

        builder.Property(u => u.DisplayName).HasMaxLength(100);

        builder.Property(u => u.SuspensionReason).HasMaxLength(500);

        builder.Property(u => u.PrivacyPolicyVersion).HasMaxLength(20);

        builder
            .HasOne(u => u.BodyProfile)
            .WithOne(bp => bp.User)
            .HasForeignKey<BodyProfile>(bp => bp.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(u => u.Creator)
            .WithOne(c => c.User)
            .HasForeignKey<Creator>(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(u => u.Retailer)
            .WithOne(r => r.User)
            .HasForeignKey<Retailer>(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(u => u.StyleProfile)
            .WithOne(sp => sp.User)
            .HasForeignKey<StyleProfile>(sp => sp.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasMany(u => u.AuthoredPosts)
            .WithOne(p => p.AuthorUser)
            .HasForeignKey(p => p.AuthorUserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Index for finding suspended/active users
        builder.HasIndex(u => u.IsActive).HasDatabaseName("IX_Users_IsActive");
    }
}
