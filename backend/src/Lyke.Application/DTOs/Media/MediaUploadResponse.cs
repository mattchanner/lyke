using Lyke.Core.Enums;

namespace Lyke.Application.DTOs.Media;

public record MediaUploadResponse
{
    public required string MediaId { get; init; }
    public required string OriginalUrl { get; init; }
    public string? StandardUrl { get; init; }
    public string? ThumbnailUrl { get; init; }
    public required MediaProcessingStatus Status { get; init; }
    public required string ContentType { get; init; }
    public required long SizeBytes { get; init; }
    public int? Width { get; init; }
    public int? Height { get; init; }
    public double? DurationSeconds { get; init; }
    public required bool IsVideo { get; init; }
}
