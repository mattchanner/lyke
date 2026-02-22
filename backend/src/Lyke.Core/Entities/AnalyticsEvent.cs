using Lyke.Core.Enums;

namespace Lyke.Core.Entities;

public class AnalyticsEvent
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public AnalyticsEventType EventType { get; set; }
    public Guid? EntityId { get; set; }
    public string? EntityType { get; set; }
    public string? Properties { get; set; }
    public string? SessionId { get; set; }
    public DateTime CreatedAt { get; set; }
}
