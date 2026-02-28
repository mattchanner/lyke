using Lyke.Core.Enums;

namespace Lyke.Core.Entities;

public class MediaProcessingJob : BaseEntity
{
    public required string MediaId { get; set; }
    public required Guid UserId { get; set; }
    public MediaProcessingStatus Status { get; set; } = MediaProcessingStatus.Pending;
    public required string ContentType { get; set; }
    public required bool IsVideo { get; set; }
    public string? OriginalUrl { get; set; }
    public string? StandardUrl { get; set; }
    public string? ThumbnailUrl { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public double? DurationSeconds { get; set; }
    public long SizeBytes { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime? CompletedAt { get; set; }
}
