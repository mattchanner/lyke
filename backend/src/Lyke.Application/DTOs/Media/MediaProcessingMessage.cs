namespace Lyke.Application.DTOs.Media;

public record MediaProcessingMessage
{
    public required string MediaId { get; init; }
    public required Guid UserId { get; init; }
    public required string OriginalBlobPath { get; init; }
    public required string BaseBlobPath { get; init; }
    public required string ContentType { get; init; }
    public required bool IsVideo { get; init; }
    public required int StandardWidth { get; init; }
    public required int StandardHeight { get; init; }
    public required int ThumbnailWidth { get; init; }
    public required int ThumbnailHeight { get; init; }
    public required int ImageQuality { get; init; }
}
