using Lyke.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lyke.Infrastructure.Data.Configurations;

public class UserFollowConfiguration : IEntityTypeConfiguration<UserFollow>
{
    public void Configure(EntityTypeBuilder<UserFollow> builder)
    {
        builder.HasKey(uf => uf.Id);

        builder.HasIndex(uf => new { uf.FollowerUserId, uf.FollowedUserId })
            .IsUnique();

        builder.HasIndex(uf => uf.FollowedUserId);

        builder.Property(uf => uf.CreatedAt)
            .HasDefaultValueSql("NOW()");

        builder.HasOne(uf => uf.Follower)
            .WithMany(u => u.Following)
            .HasForeignKey(uf => uf.FollowerUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(uf => uf.Followed)
            .WithMany(u => u.Followers)
            .HasForeignKey(uf => uf.FollowedUserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
