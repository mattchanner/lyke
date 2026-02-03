using Lyke.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lyke.Infrastructure.Data.Configurations;

public class EngagementConfiguration : IEntityTypeConfiguration<Engagement>
{
    public void Configure(EntityTypeBuilder<Engagement> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Type)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasOne(e => e.User)
            .WithMany(u => u.Engagements)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Post)
            .WithMany(p => p.Engagements)
            .HasForeignKey(e => e.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => new { e.UserId, e.PostId, e.Type })
            .HasDatabaseName("IX_Engagements_UserId_PostId_Type");

        builder.HasIndex(e => new { e.PostId, e.CreatedAt })
            .HasDatabaseName("IX_Engagements_PostId_CreatedAt");
    }
}
