using Lyke.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lyke.Infrastructure.Data.Configurations;

public class AnalyticsEventConfiguration : IEntityTypeConfiguration<AnalyticsEvent>
{
    public void Configure(EntityTypeBuilder<AnalyticsEvent> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.EventType)
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(e => e.EntityType)
            .HasMaxLength(50);

        builder.Property(e => e.SessionId)
            .HasMaxLength(100);

        builder.Property(e => e.Properties)
            .HasColumnType("jsonb");

        builder.HasIndex(e => new { e.EventType, e.CreatedAt })
            .HasDatabaseName("IX_AnalyticsEvents_EventType_CreatedAt");

        builder.HasIndex(e => e.UserId)
            .HasDatabaseName("IX_AnalyticsEvents_UserId");

        builder.HasIndex(e => e.CreatedAt)
            .HasDatabaseName("IX_AnalyticsEvents_CreatedAt");
    }
}
