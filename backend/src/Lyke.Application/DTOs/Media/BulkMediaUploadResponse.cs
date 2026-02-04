namespace Lyke.Application.DTOs.Media;

public record BulkMediaUploadResponse
{
    public required IReadOnlyList<MediaUploadResponse> Succeeded { get; init; }
    public required IReadOnlyList<MediaUploadError> Failed { get; init; }
}

public record MediaUploadError
{
    public required string FileName { get; init; }
    public required string ErrorCode { get; init; }
    public required string ErrorMessage { get; init; }
}
