using Lyke.Core.Enums;

namespace Lyke.Application.Interfaces;

public interface IEventTrackingService
{
    /// <summary>
    /// Track a single analytics event. Emits structured log and persists behavioral events to DB.
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
    /// Track a batch of analytics events.
    /// </summary>
    Task TrackBatchAsync(
        IEnumerable<TrackEventItem> events,
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
