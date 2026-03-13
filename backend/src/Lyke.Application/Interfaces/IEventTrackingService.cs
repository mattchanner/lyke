using Lyke.Core.Enums;

namespace Lyke.Application.Interfaces;

public interface IEventTrackingService
{
    /// <summary>
    /// Track a single analytics event using a strongly-typed enum (for internal callers).
    /// Emits a structured log and persists behavioral events to the DB.
    /// </summary>
    Task TrackAsync(
        AnalyticsEventType eventType,
        Guid? userId = null,
        Guid? entityId = null,
        string? entityType = null,
        Dictionary<string, string>? properties = null,
        string? sessionId = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Track a batch of analytics events using strongly-typed enums (for internal callers).
    /// </summary>
    Task TrackBatchAsync(
        IEnumerable<TrackEventItem> events,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Track a single analytics event from an arbitrary string event name (for HTTP API callers).
    /// Emits a structured log for all events; persists only known behavioral event types.
    /// </summary>
    Task TrackRawAsync(
        string eventType,
        Guid? userId = null,
        Guid? entityId = null,
        string? entityType = null,
        Dictionary<string, string>? properties = null,
        string? sessionId = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Track a batch of analytics events from arbitrary string event names (for HTTP API callers).
    /// </summary>
    Task TrackRawBatchAsync(
        IEnumerable<TrackRawEventItem> events,
        CancellationToken cancellationToken = default
    );
}

public record TrackEventItem(
    AnalyticsEventType EventType,
    Guid? UserId = null,
    Guid? EntityId = null,
    string? EntityType = null,
    Dictionary<string, string>? Properties = null,
    string? SessionId = null
);

public record TrackRawEventItem(
    string EventType,
    Guid? UserId = null,
    Guid? EntityId = null,
    string? EntityType = null,
    Dictionary<string, string>? Properties = null,
    string? SessionId = null
);
