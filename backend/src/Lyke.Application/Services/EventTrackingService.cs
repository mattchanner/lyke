using System.Text.Json;
using Lyke.Application.Interfaces;
using Lyke.Core.Entities;
using Lyke.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Lyke.Application.Services;

public class EventTrackingService : IEventTrackingService
{
    private readonly DbContext _dbContext;
    private readonly ILogger<EventTrackingService> _logger;

    // Events that are persisted to the AnalyticsEvent table (behavioral events without existing DB tables)
    private static readonly HashSet<AnalyticsEventType> PersistableEvents =
    [
        AnalyticsEventType.FeedFilter,
        AnalyticsEventType.SearchExecute,
        AnalyticsEventType.ProfileComplete,
    ];

    public EventTrackingService(DbContext dbContext, ILogger<EventTrackingService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task TrackAsync(
        AnalyticsEventType eventType,
        Guid? userId = null,
        Guid? entityId = null,
        string? entityType = null,
        Dictionary<string, string>? properties = null,
        string? sessionId = null,
        CancellationToken cancellationToken = default
    )
    {
        // Structured log for ALL events (flows to AppInsights via OpenTelemetry)
        _logger.LogInformation(
            "AnalyticsEvent {EventType} User={UserId} Entity={EntityId} EntityType={EntityType} Session={SessionId}",
            eventType,
            userId,
            entityId,
            entityType,
            sessionId
        );

        // Only persist behavioral events that don't already have a dedicated table
        if (PersistableEvents.Contains(eventType))
        {
            var analyticsEvent = new AnalyticsEvent
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                EventType = eventType,
                EntityId = entityId,
                EntityType = entityType,
                Properties = properties != null ? JsonSerializer.Serialize(properties) : null,
                SessionId = sessionId,
                CreatedAt = DateTime.UtcNow,
            };

            await _dbContext.Set<AnalyticsEvent>().AddAsync(analyticsEvent, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task TrackBatchAsync(
        IEnumerable<TrackEventItem> events,
        CancellationToken cancellationToken = default
    )
    {
        var eventsList = events.ToList();

        foreach (var evt in eventsList)
        {
            _logger.LogInformation(
                "AnalyticsEvent {EventType} User={UserId} Entity={EntityId} EntityType={EntityType} Session={SessionId}",
                evt.EventType,
                evt.UserId,
                evt.EntityId,
                evt.EntityType,
                evt.SessionId
            );
        }

        var persistable = eventsList
            .Where(e => PersistableEvents.Contains(e.EventType))
            .Select(e => new AnalyticsEvent
            {
                Id = Guid.NewGuid(),
                UserId = e.UserId,
                EventType = e.EventType,
                EntityId = e.EntityId,
                EntityType = e.EntityType,
                Properties = e.Properties != null ? JsonSerializer.Serialize(e.Properties) : null,
                SessionId = e.SessionId,
                CreatedAt = DateTime.UtcNow,
            })
            .ToList();

        if (persistable.Count > 0)
        {
            await _dbContext.Set<AnalyticsEvent>().AddRangeAsync(persistable, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
