using Lyke.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lyke.Infrastructure.Data.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Action).HasConversion<string>().HasMaxLength(30);

        builder.Property(a => a.EntityType).HasMaxLength(100);

        builder.Property(a => a.Details).HasColumnType("jsonb");

        builder.Property(a => a.IpAddress).HasMaxLength(45);

        builder.HasIndex(a => a.UserId).HasDatabaseName("IX_AuditLogs_UserId");

        builder.HasIndex(a => a.TargetUserId).HasDatabaseName("IX_AuditLogs_TargetUserId");

        builder.HasIndex(a => a.Action).HasDatabaseName("IX_AuditLogs_Action");

        builder.HasIndex(a => a.Timestamp).HasDatabaseName("IX_AuditLogs_Timestamp");

        builder
            .HasIndex(a => new { a.TargetUserId, a.Timestamp })
            .HasDatabaseName("IX_AuditLogs_TargetUserId_Timestamp");
    }
}
