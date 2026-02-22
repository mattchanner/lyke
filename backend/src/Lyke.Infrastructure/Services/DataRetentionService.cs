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

public class DataRetentionService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DataRetentionService> _logger;
    private readonly DataRetentionSettings _settings;

    public DataRetentionService(
        IServiceScopeFactory scopeFactory,
        ILogger<DataRetentionService> logger,
        IOptions<PrivacySettings> settings)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _settings = settings.Value.DataRetention;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_settings.Enabled)
        {
            _logger.LogInformation("Data retention service is disabled");
            return;
        }

        var initialDelay = CalculateInitialDelay();
        _logger.LogInformation(
            "Data retention service started. First run in {Delay} (at ~{Hour}:00 UTC), then every {Interval}h",
            initialDelay,
            _settings.PreferredRunHourUtc,
            _settings.RunIntervalHours);

        await Task.Delay(initialDelay, stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunCleanupAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Data retention cleanup failed");
            }

            await Task.Delay(TimeSpan.FromHours(_settings.RunIntervalHours), stoppingToken);
        }
    }

    private async Task RunCleanupAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting data retention cleanup");

        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LykeDbContext>();

        var expiredTokens = await CleanupRefreshTokensAsync(dbContext, stoppingToken);
        var anonymizedClicks = await CleanupAnonymizedClickEventsAsync(dbContext, stoppingToken);
        var viewEngagements = await CleanupViewEngagementsAsync(dbContext, stoppingToken);
        var oldAuditLogs = await CleanupAuditLogsAsync(dbContext, stoppingToken);

        _logger.LogInformation(
            "Data retention cleanup completed: {Tokens} refresh tokens, {Clicks} anonymized clicks, {Views} view engagements, {AuditLogs} audit logs removed",
            expiredTokens,
            anonymizedClicks,
            viewEngagements,
            oldAuditLogs);
    }

    private async Task<int> CleanupRefreshTokensAsync(LykeDbContext dbContext, CancellationToken ct)
    {
        var graceCutoff = DateTime.UtcNow.AddDays(-_settings.RefreshTokenGracePeriodDays);

        var deleted = await dbContext.RefreshTokens
            .Where(t =>
                (t.ExpiresAt < graceCutoff) ||
                (t.RevokedAt != null && t.RevokedAt < graceCutoff))
            .Take(_settings.BatchSize)
            .ExecuteDeleteAsync(ct);

        return deleted;
    }

    private async Task<int> CleanupAnonymizedClickEventsAsync(LykeDbContext dbContext, CancellationToken ct)
    {
        var cutoff = DateTime.UtcNow.AddDays(-_settings.AnonymizedClickEventRetentionDays);

        var deleted = await dbContext.ClickEvents
            .Where(c => c.UserId == null && c.CreatedAt < cutoff)
            .Take(_settings.BatchSize)
            .ExecuteDeleteAsync(ct);

        return deleted;
    }

    private async Task<int> CleanupViewEngagementsAsync(LykeDbContext dbContext, CancellationToken ct)
    {
        var cutoff = DateTime.UtcNow.AddDays(-_settings.ViewEngagementRetentionDays);

        var deleted = await dbContext.Engagements
            .Where(e => e.Type == EngagementType.View && e.CreatedAt < cutoff)
            .Take(_settings.BatchSize)
            .ExecuteDeleteAsync(ct);

        return deleted;
    }

    private async Task<int> CleanupAuditLogsAsync(LykeDbContext dbContext, CancellationToken ct)
    {
        var cutoff = DateTime.UtcNow.AddDays(-_settings.AuditLogRetentionDays);

        var deleted = await dbContext.AuditLogs
            .Where(a => a.Timestamp < cutoff)
            .Take(_settings.BatchSize)
            .ExecuteDeleteAsync(ct);

        return deleted;
    }

    private TimeSpan CalculateInitialDelay()
    {
        var now = DateTime.UtcNow;
        var nextRun = now.Date.AddHours(_settings.PreferredRunHourUtc);

        if (nextRun <= now)
            nextRun = nextRun.AddDays(1);

        return nextRun - now;
    }
}
