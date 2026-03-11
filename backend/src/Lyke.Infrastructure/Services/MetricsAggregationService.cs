using Lyke.Application.Configuration;
using Lyke.Core.Entities;
using Lyke.Core.Enums;
using Lyke.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lyke.Infrastructure.Services;

public class MetricsAggregationService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<MetricsAggregationService> _logger;
    private readonly AnalyticsSettings _settings;

    public MetricsAggregationService(
        IServiceScopeFactory scopeFactory,
        ILogger<MetricsAggregationService> logger,
        IOptions<AnalyticsSettings> settings
    )
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _settings = settings.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_settings.AggregationEnabled)
        {
            _logger.LogInformation("Metrics aggregation service is disabled");
            return;
        }

        _logger.LogInformation(
            "Metrics aggregation service started. Running every {Interval}h",
            _settings.AggregationIntervalHours
        );

        // Run immediately on startup, then on interval
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await AggregateAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Metrics aggregation failed");
            }

            await Task.Delay(TimeSpan.FromHours(_settings.AggregationIntervalHours), stoppingToken);
        }
    }

    private async Task AggregateAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting metrics aggregation");

        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LykeDbContext>();

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // Aggregate platform-level metrics for today
        await AggregatePlatformMetricsAsync(dbContext, today, stoppingToken);

        // Aggregate per-creator metrics for today
        await AggregateCreatorMetricsAsync(dbContext, today, stoppingToken);

        _logger.LogInformation("Metrics aggregation completed");
    }

    private async Task AggregatePlatformMetricsAsync(
        LykeDbContext dbContext,
        DateOnly date,
        CancellationToken ct
    )
    {
        var dayStart = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var dayEnd = date.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

        var views = await dbContext.Engagements.CountAsync(
            e => e.Type == EngagementType.View && e.CreatedAt >= dayStart && e.CreatedAt < dayEnd,
            ct
        );
        var likes = await dbContext.Engagements.CountAsync(
            e => e.Type == EngagementType.Like && e.CreatedAt >= dayStart && e.CreatedAt < dayEnd,
            ct
        );
        var saves = await dbContext.Engagements.CountAsync(
            e => e.Type == EngagementType.Save && e.CreatedAt >= dayStart && e.CreatedAt < dayEnd,
            ct
        );
        var shares = await dbContext.Engagements.CountAsync(
            e => e.Type == EngagementType.Share && e.CreatedAt >= dayStart && e.CreatedAt < dayEnd,
            ct
        );
        var clicks = await dbContext.ClickEvents.CountAsync(
            c => c.CreatedAt >= dayStart && c.CreatedAt < dayEnd,
            ct
        );
        var conversions = await dbContext.ClickEvents.CountAsync(
            c => c.ConvertedAt >= dayStart && c.ConvertedAt < dayEnd,
            ct
        );
        var earnings =
            await dbContext
                .CreatorEarnings.Where(e => e.CreatedAt >= dayStart && e.CreatedAt < dayEnd)
                .SumAsync(e => (decimal?)e.Amount, ct)
            ?? 0;
        var newUsers = await dbContext.Users.CountAsync(
            u => u.CreatedAt >= dayStart && u.CreatedAt < dayEnd,
            ct
        );
        var postsPublished = await dbContext.Posts.CountAsync(
            p =>
                p.Status == PostStatus.Published && p.CreatedAt >= dayStart && p.CreatedAt < dayEnd,
            ct
        );
        var searches = await dbContext.AnalyticsEvents.CountAsync(
            a =>
                a.EventType == AnalyticsEventType.SearchExecute
                && a.CreatedAt >= dayStart
                && a.CreatedAt < dayEnd,
            ct
        );
        var filters = await dbContext.AnalyticsEvents.CountAsync(
            a =>
                a.EventType == AnalyticsEventType.FeedFilter
                && a.CreatedAt >= dayStart
                && a.CreatedAt < dayEnd,
            ct
        );

        await UpsertSnapshotAsync(
            dbContext,
            new DailyMetricSnapshot
            {
                Id = Guid.NewGuid(),
                Date = date,
                Scope = "platform",
                Views = views,
                Likes = likes,
                Saves = saves,
                Shares = shares,
                Clicks = clicks,
                Conversions = conversions,
                Revenue = 0,
                Earnings = earnings,
                NewUsers = newUsers,
                PostsPublished = postsPublished,
                SearchesExecuted = searches,
                FiltersApplied = filters,
                ComputedAt = DateTime.UtcNow,
            },
            ct
        );
    }

    private async Task AggregateCreatorMetricsAsync(
        LykeDbContext dbContext,
        DateOnly date,
        CancellationToken ct
    )
    {
        var dayStart = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var dayEnd = date.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

        // Get active creator IDs that have posts with engagements today
        var creatorIds = await dbContext
            .Engagements.Where(e => e.CreatedAt >= dayStart && e.CreatedAt < dayEnd)
            .Select(e => e.Post.CreatorId)
            .Distinct()
            .ToListAsync(ct);

        foreach (var creatorId in creatorIds)
        {
            var creatorPostIds = await dbContext
                .Posts.Where(p => p.CreatorId == creatorId)
                .Select(p => p.Id)
                .ToListAsync(ct);

            var views = await dbContext.Engagements.CountAsync(
                e =>
                    creatorPostIds.Contains(e.PostId)
                    && e.Type == EngagementType.View
                    && e.CreatedAt >= dayStart
                    && e.CreatedAt < dayEnd,
                ct
            );
            var likes = await dbContext.Engagements.CountAsync(
                e =>
                    creatorPostIds.Contains(e.PostId)
                    && e.Type == EngagementType.Like
                    && e.CreatedAt >= dayStart
                    && e.CreatedAt < dayEnd,
                ct
            );
            var saves = await dbContext.Engagements.CountAsync(
                e =>
                    creatorPostIds.Contains(e.PostId)
                    && e.Type == EngagementType.Save
                    && e.CreatedAt >= dayStart
                    && e.CreatedAt < dayEnd,
                ct
            );
            var shares = await dbContext.Engagements.CountAsync(
                e =>
                    creatorPostIds.Contains(e.PostId)
                    && e.Type == EngagementType.Share
                    && e.CreatedAt >= dayStart
                    && e.CreatedAt < dayEnd,
                ct
            );
            var clicks = await dbContext.ClickEvents.CountAsync(
                c =>
                    creatorPostIds.Contains(c.PostId)
                    && c.CreatedAt >= dayStart
                    && c.CreatedAt < dayEnd,
                ct
            );
            var conversions = await dbContext.ClickEvents.CountAsync(
                c =>
                    creatorPostIds.Contains(c.PostId)
                    && c.ConvertedAt >= dayStart
                    && c.ConvertedAt < dayEnd,
                ct
            );
            var earnings =
                await dbContext
                    .CreatorEarnings.Where(e =>
                        e.CreatorId == creatorId && e.CreatedAt >= dayStart && e.CreatedAt < dayEnd
                    )
                    .SumAsync(e => (decimal?)e.Amount, ct)
                ?? 0;

            await UpsertSnapshotAsync(
                dbContext,
                new DailyMetricSnapshot
                {
                    Id = Guid.NewGuid(),
                    Date = date,
                    Scope = $"creator:{creatorId}",
                    ScopeEntityId = creatorId,
                    Views = views,
                    Likes = likes,
                    Saves = saves,
                    Shares = shares,
                    Clicks = clicks,
                    Conversions = conversions,
                    Revenue = 0,
                    Earnings = earnings,
                    NewUsers = 0,
                    PostsPublished = 0,
                    SearchesExecuted = 0,
                    FiltersApplied = 0,
                    ComputedAt = DateTime.UtcNow,
                },
                ct
            );
        }
    }

    private static async Task UpsertSnapshotAsync(
        LykeDbContext dbContext,
        DailyMetricSnapshot snapshot,
        CancellationToken ct
    )
    {
        var existing = await dbContext.DailyMetricSnapshots.FirstOrDefaultAsync(
            s => s.Date == snapshot.Date && s.Scope == snapshot.Scope,
            ct
        );

        if (existing != null)
        {
            existing.Views = snapshot.Views;
            existing.Likes = snapshot.Likes;
            existing.Saves = snapshot.Saves;
            existing.Shares = snapshot.Shares;
            existing.Clicks = snapshot.Clicks;
            existing.Conversions = snapshot.Conversions;
            existing.Revenue = snapshot.Revenue;
            existing.Earnings = snapshot.Earnings;
            existing.NewUsers = snapshot.NewUsers;
            existing.PostsPublished = snapshot.PostsPublished;
            existing.SearchesExecuted = snapshot.SearchesExecuted;
            existing.FiltersApplied = snapshot.FiltersApplied;
            existing.ComputedAt = DateTime.UtcNow;
        }
        else
        {
            await dbContext.DailyMetricSnapshots.AddAsync(snapshot, ct);
        }

        await dbContext.SaveChangesAsync(ct);
    }
}
