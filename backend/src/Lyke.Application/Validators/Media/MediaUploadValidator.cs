using Lyke.Application.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Lyke.Application.Validators.Media;

public record MediaValidationResult(
    bool IsValid,
    string? ErrorCode = null,
    string? ErrorMessage = null
)
{
    public static MediaValidationResult Success() => new(true);

    public static MediaValidationResult Failure(string code, string message) =>
        new(false, code, message);
}

public class MediaUploadValidator
{
    private readonly MediaUploadSettings _settings;

    public MediaUploadValidator(IOptions<MediaUploadSettings> settings)
    {
        _settings = settings.Value;
    }

    public MediaValidationResult Validate(IFormFile file)
    {
        if (file.Length == 0)
        {
            return MediaValidationResult.Failure("EMPTY_FILE", "File is empty");
        }

        var contentType = file.ContentType.ToLowerInvariant();
        var isImage = _settings.AllowedImageMimeTypes.Contains(contentType);
        var isVideo = _settings.AllowedVideoMimeTypes.Contains(contentType);

        if (!isImage && !isVideo)
        {
            return MediaValidationResult.Failure(
                "INVALID_CONTENT_TYPE",
                $"Content type '{contentType}' is not allowed. Allowed types: {string.Join(", ", _settings.AllowedImageMimeTypes.Concat(_settings.AllowedVideoMimeTypes))}"
            );
        }

        if (isImage && file.Length > _settings.MaxImageFileSizeBytes)
        {
            return MediaValidationResult.Failure(
                "FILE_TOO_LARGE",
                $"Image file size exceeds maximum of {_settings.MaxImageFileSizeBytes / (1024 * 1024)} MB"
            );
        }

        if (isVideo && file.Length > _settings.MaxVideoFileSizeBytes)
        {
            return MediaValidationResult.Failure(
                "FILE_TOO_LARGE",
                $"Video file size exceeds maximum of {_settings.MaxVideoFileSizeBytes / (1024 * 1024)} MB"
            );
        }

        return MediaValidationResult.Success();
    }

    public bool IsVideo(string contentType) =>
        _settings.AllowedVideoMimeTypes.Contains(contentType.ToLowerInvariant());

    public bool IsImage(string contentType) =>
        _settings.AllowedImageMimeTypes.Contains(contentType.ToLowerInvariant());
}
