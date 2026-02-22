using Lyke.Core.Enums;

namespace Lyke.Application.DTOs.Analytics;

public record TrackEventRequest(
    AnalyticsEventType EventType,
    Guid? EntityId = null,
    string? EntityType = null,
    Dictionary<string, string>? Properties = null,
    string? SessionId = null);

public record TrackEventBatchRequest(
    List<TrackEventRequest> Events);
