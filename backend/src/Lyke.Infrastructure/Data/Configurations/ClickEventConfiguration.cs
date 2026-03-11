using Lyke.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lyke.Infrastructure.Data.Configurations;

public class ClickEventConfiguration : IEntityTypeConfiguration<ClickEvent>
{
    public void Configure(EntityTypeBuilder<ClickEvent> builder)
    {
        builder.HasKey(ce => ce.Id);

        builder.Property(ce => ce.SessionId).HasMaxLength(100);

        builder.Property(ce => ce.AttributionData).HasColumnType("jsonb");

        builder
            .HasOne(ce => ce.User)
            .WithMany(u => u.ClickEvents)
            .HasForeignKey(ce => ce.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder
            .HasOne(ce => ce.Post)
            .WithMany(p => p.ClickEvents)
            .HasForeignKey(ce => ce.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(ce => ce.PostProduct)
            .WithMany(pp => pp.ClickEvents)
            .HasForeignKey(ce => ce.PostProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasIndex(ce => new { ce.PostId, ce.CreatedAt })
            .HasDatabaseName("IX_ClickEvents_PostId_CreatedAt");

        builder.HasIndex(ce => ce.SessionId).HasDatabaseName("IX_ClickEvents_SessionId");
    }
}
