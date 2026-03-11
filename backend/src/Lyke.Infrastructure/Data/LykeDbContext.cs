using Lyke.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Lyke.Infrastructure.Data;

public class LykeDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
{
    public LykeDbContext(DbContextOptions<LykeDbContext> options)
        : base(options) { }

    public DbSet<BodyProfile> BodyProfiles => Set<BodyProfile>();
    public DbSet<BodyType> BodyTypes => Set<BodyType>();
    public DbSet<FrameSize> FrameSizes => Set<FrameSize>();
    public DbSet<BodyProfileFitPreference> BodyProfileFitPreferences =>
        Set<BodyProfileFitPreference>();
    public DbSet<Creator> Creators => Set<Creator>();
    public DbSet<Retailer> Retailers => Set<Retailer>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<PostProduct> PostProducts => Set<PostProduct>();
    public DbSet<FitTag> FitTags => Set<FitTag>();
    public DbSet<PostFitTag> PostFitTags => Set<PostFitTag>();
    public DbSet<Engagement> Engagements => Set<Engagement>();
    public DbSet<ClickEvent> ClickEvents => Set<ClickEvent>();
    public DbSet<SponsoredPlacement> SponsoredPlacements => Set<SponsoredPlacement>();
    public DbSet<CreatorEarning> CreatorEarnings => Set<CreatorEarning>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<ContentReport> ContentReports => Set<ContentReport>();
    public DbSet<AnalyticsEvent> AnalyticsEvents => Set<AnalyticsEvent>();
    public DbSet<DailyMetricSnapshot> DailyMetricSnapshots => Set<DailyMetricSnapshot>();
    public DbSet<UserFollow> UserFollows => Set<UserFollow>();
    public DbSet<MediaProcessingJob> MediaProcessingJobs => Set<MediaProcessingJob>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Apply all configurations from assembly
        builder.ApplyConfigurationsFromAssembly(typeof(LykeDbContext).Assembly);

        // Note: Seed data for BodyTypes and FitTags is in InitialCreate migration
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.ConfigureWarnings(warnings =>
            warnings.Ignore(RelationalEventId.PendingModelChangesWarning)
        );
    }

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker
            .Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.Entity is BaseEntity entity)
            {
                if (entry.State == EntityState.Added)
                {
                    entity.CreatedAt = DateTime.UtcNow;
                }
                entity.UpdatedAt = DateTime.UtcNow;
            }

            if (entry.Entity is User user)
            {
                if (entry.State == EntityState.Added)
                {
                    user.CreatedAt = DateTime.UtcNow;
                }
                user.UpdatedAt = DateTime.UtcNow;
            }
        }
    }
}
