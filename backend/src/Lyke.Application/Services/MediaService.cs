using Lyke.Application.Configuration;
using Lyke.Application.DTOs.Media;
using Lyke.Application.Interfaces;
using Lyke.Application.Validators.Media;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lyke.Application.Services;

public class MediaService : IMediaService
{
    private readonly IStorageService _storageService;
    private readonly IImageProcessingService _imageProcessingService;
    private readonly IVideoProcessingService _videoProcessingService;
    private readonly MediaUploadValidator _validator;
    private readonly MediaUploadSettings _settings;
    private readonly ILogger<MediaService> _logger;

    public MediaService(
        IStorageService storageService,
        IImageProcessingService imageProcessingService,
        IVideoProcessingService videoProcessingService,
        MediaUploadValidator validator,
        IOptions<MediaUploadSettings> settings,
        ILogger<MediaService> logger)
    {
        _storageService = storageService;
        _imageProcessingService = imageProcessingService;
        _videoProcessingService = videoProcessingService;
        _validator = validator;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<MediaUploadResponse> UploadMediaAsync(
        Guid userId,
        IFormFile file,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Upload media called with file {FileName}", file.FileName);

        var validation = _validator.Validate(file);
        if (!validation.IsValid)
        {
            _logger.LogWarning("Invalid file detected for file {FileName}", file.FileName);
            throw new InvalidOperationException($"{validation.ErrorCode}: {validation.ErrorMessage}");
        }

        var mediaId = Guid.NewGuid().ToString("N");
        var basePath = $"users/{userId}/media/{mediaId}";
        var contentType = file.ContentType.ToLowerInvariant();
        var isVideo = _validator.IsVideo(contentType);

        _logger.LogDebug("Reading file into memory");

        using var inputStream = new MemoryStream();
        await file.CopyToAsync(inputStream, cancellationToken);
        inputStream.Position = 0;

        if (isVideo)
        {
            _logger.LogDebug("Video processing...");
            return await ProcessVideoUploadAsync(userId, mediaId, basePath, inputStream, contentType, cancellationToken);
        }

        _logger.LogDebug("Image processing...");
        return await ProcessImageUploadAsync(userId, mediaId, basePath, inputStream, contentType, cancellationToken);
    }

    public async Task<BulkMediaUploadResponse> UploadMediaBulkAsync(
        Guid userId,
        IFormFileCollection files,
        CancellationToken cancellationToken = default)
    {
        if (files.Count > _settings.MaxMediaPerPost)
        {
            throw new InvalidOperationException(
                $"Maximum {_settings.MaxMediaPerPost} files can be uploaded at once");
        }

        var succeeded = new List<MediaUploadResponse>();
        var failed = new List<MediaUploadError>();

        foreach (var file in files)
        {
            try
            {
                var result = await UploadMediaAsync(userId, file, cancellationToken);
                succeeded.Add(result);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to upload file {FileName}", file.FileName);
                failed.Add(new MediaUploadError
                {
                    FileName = file.FileName,
                    ErrorCode = "UPLOAD_FAILED",
                    ErrorMessage = ex.Message
                });
            }
        }

        return new BulkMediaUploadResponse
        {
            Succeeded = succeeded,
            Failed = failed
        };
    }

    public async Task DeleteMediaAsync(
        Guid userId,
        IEnumerable<string> mediaIds,
        CancellationToken cancellationToken = default)
    {
        var blobPaths = new List<string>();

        foreach (var mediaId in mediaIds)
        {
            var basePath = $"users/{userId}/media/{mediaId}";

            // Add all possible blob paths for this media
            blobPaths.Add($"{basePath}_original.jpg");
            blobPaths.Add($"{basePath}_original.png");
            blobPaths.Add($"{basePath}_original.webp");
            blobPaths.Add($"{basePath}_standard.webp");
            blobPaths.Add($"{basePath}_thumb.jpg");
            blobPaths.Add($"{basePath}.mp4");
            blobPaths.Add($"{basePath}.mov");
            blobPaths.Add($"{basePath}.webm");
        }

        await _storageService.DeleteManyAsync(blobPaths, cancellationToken);

        _logger.LogInformation(
            "Deleted media for user {UserId}: {MediaIds}",
            userId,
            string.Join(", ", mediaIds));
    }

    public async Task<string> GetSecureUrlAsync(
        string mediaUrl,
        CancellationToken cancellationToken = default)
    {
        // Extract blob path from URL
        var uri = new Uri(mediaUrl);
        var blobPath = uri.AbsolutePath.TrimStart('/');

        // Remove container name (first path segment) from path
        var slashIndex = blobPath.IndexOf('/');
        if (slashIndex > 0)
        {
            blobPath = blobPath[(slashIndex + 1)..];
        }

        var expiry = TimeSpan.FromMinutes(_settings.SasTokenExpiryMinutes);
        return await _storageService.GenerateSasUrlAsync(blobPath, expiry, cancellationToken);
    }

    private async Task<MediaUploadResponse> ProcessImageUploadAsync(
        Guid userId,
        string mediaId,
        string basePath,
        MemoryStream inputStream,
        string contentType,
        CancellationToken cancellationToken)
    {
        var extension = GetExtensionFromContentType(contentType);

        _logger.LogDebug("File extension from content type {ContentType} is {Extension}", contentType, extension);

        // Get original dimensions
        var dimensions = await _imageProcessingService.GetImageDimensionsAsync(inputStream, cancellationToken);

        // Upload original
        inputStream.Position = 0;


        _logger.LogDebug("Uploading file to storage");
        var originalResult = await _storageService.UploadAsync(
            inputStream,
            $"{basePath}_original{extension}",
            contentType,
            cancellationToken);

        // Process and upload standard size
        inputStream.Position = 0;

        _logger.LogDebug("Processing image");
        var processed = await _imageProcessingService.ProcessImageAsync(
            inputStream,
            _settings.StandardWidth,
            _settings.StandardHeight,
            _settings.ImageQuality,
            cancellationToken);

        _logger.LogDebug("Uploading webp file");
        var standardResult = await _storageService.UploadAsync(
            processed.Content,
            $"{basePath}_standard.webp",
            processed.ContentType,
            cancellationToken);

        // Create and upload thumbnail
        inputStream.Position = 0;

        _logger.LogDebug("Creating image thumbnail");
        var thumbnail = await _imageProcessingService.CreateThumbnailAsync(
            inputStream,
            _settings.ThumbnailWidth,
            _settings.ThumbnailHeight,
            cancellationToken);

        _logger.LogDebug("Uploading thumbnail");
        var thumbnailResult = await _storageService.UploadAsync(
            thumbnail.Content,
            $"{basePath}_thumb.jpg",
            thumbnail.ContentType,
            cancellationToken);

        _logger.LogInformation(
            "Uploaded image for user {UserId}, mediaId {MediaId}",
            userId, mediaId);

        return new MediaUploadResponse
        {
            MediaId = mediaId,
            OriginalUrl = originalResult.Url,
            StandardUrl = standardResult.Url,
            ThumbnailUrl = thumbnailResult.Url,
            ContentType = contentType,
            SizeBytes = originalResult.SizeBytes,
            Width = dimensions.Width,
            Height = dimensions.Height,
            IsVideo = false
        };
    }

    private async Task<MediaUploadResponse> ProcessVideoUploadAsync(
        Guid userId,
        string mediaId,
        string basePath,
        MemoryStream inputStream,
        string contentType,
        CancellationToken cancellationToken)
    {
        var extension = GetExtensionFromContentType(contentType);
        var tempPath = Path.Combine(Path.GetTempPath(), $"{mediaId}{extension}");

        try
        {
            // Write to temp file for FFmpeg processing
            await using (var fileStream = File.Create(tempPath))
            {
                inputStream.Position = 0;
                await inputStream.CopyToAsync(fileStream, cancellationToken);
            }

            // Get video info
            var videoInfo = await _videoProcessingService.GetVideoInfoAsync(tempPath, cancellationToken);

            // Upload original video
            inputStream.Position = 0;
            var originalResult = await _storageService.UploadAsync(
                inputStream,
                $"{basePath}{extension}",
                contentType,
                cancellationToken);

            // Extract and upload thumbnail
            using var thumbnailStream = await _videoProcessingService.ExtractThumbnailAsync(
                tempPath,
                _settings.ThumbnailWidth,
                _settings.ThumbnailHeight,
                cancellationToken: cancellationToken);

            var thumbnailResult = await _storageService.UploadAsync(
                thumbnailStream,
                $"{basePath}_thumb.jpg",
                "image/jpeg",
                cancellationToken);

            _logger.LogInformation(
                "Uploaded video for user {UserId}, mediaId {MediaId}, duration {Duration}s",
                userId, mediaId, videoInfo.DurationSeconds);

            return new MediaUploadResponse
            {
                MediaId = mediaId,
                OriginalUrl = originalResult.Url,
                StandardUrl = null,
                ThumbnailUrl = thumbnailResult.Url,
                ContentType = contentType,
                SizeBytes = originalResult.SizeBytes,
                Width = videoInfo.Width,
                Height = videoInfo.Height,
                DurationSeconds = videoInfo.DurationSeconds,
                IsVideo = true
            };
        }
        finally
        {
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
        }
    }

    private static string GetExtensionFromContentType(string contentType) => contentType switch
    {
        "image/jpeg" => ".jpg",
        "image/png" => ".png",
        "image/webp" => ".webp",
        "video/mp4" => ".mp4",
        "video/quicktime" => ".mov",
        "video/webm" => ".webm",
        _ => ".bin"
    };
}
