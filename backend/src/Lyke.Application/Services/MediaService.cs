using Lyke.Application.Configuration;
using Lyke.Application.DTOs.Media;
using Lyke.Application.Interfaces;
using Lyke.Application.Validators.Media;
using Lyke.Core.Entities;
using Lyke.Core.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lyke.Application.Services;

public class MediaService : IMediaService
{
    private readonly IStorageService _storageService;
    private readonly IImageProcessingService _imageProcessingService;
    private readonly IVideoProcessingService _videoProcessingService;
    private readonly IMessageQueue<MediaProcessingMessage> _messageQueue;
    private readonly DbContext _dbContext;
    private readonly MediaUploadValidator _validator;
    private readonly MediaUploadSettings _settings;
    private readonly ILogger<MediaService> _logger;

    public MediaService(
        IStorageService storageService,
        IImageProcessingService imageProcessingService,
        IVideoProcessingService videoProcessingService,
        IMessageQueue<MediaProcessingMessage> messageQueue,
        DbContext dbContext,
        MediaUploadValidator validator,
        IOptions<MediaUploadSettings> settings,
        ILogger<MediaService> logger)
    {
        _storageService = storageService;
        _imageProcessingService = imageProcessingService;
        _videoProcessingService = videoProcessingService;
        _messageQueue = messageQueue;
        _dbContext = dbContext;
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
            _logger.LogDebug("Video upload path...");
            return await EnqueueVideoUploadAsync(userId, mediaId, basePath, inputStream, contentType, cancellationToken);
        }

        _logger.LogDebug("Image upload path...");
        return await EnqueueImageUploadAsync(userId, mediaId, basePath, inputStream, contentType, cancellationToken);
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
        var uri = new Uri(mediaUrl);
        var blobPath = uri.AbsolutePath.TrimStart('/');

        var slashIndex = blobPath.IndexOf('/');
        if (slashIndex > 0)
        {
            blobPath = blobPath[(slashIndex + 1)..];
        }

        var expiry = TimeSpan.FromMinutes(_settings.SasTokenExpiryMinutes);
        return await _storageService.GenerateSasUrlAsync(blobPath, expiry, cancellationToken);
    }

    public async Task<MediaUploadResponse?> GetMediaStatusAsync(
        string mediaId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var job = await _dbContext.Set<MediaProcessingJob>()
            .FirstOrDefaultAsync(j => j.MediaId == mediaId && j.UserId == userId, cancellationToken);

        if (job == null)
            return null;

        return MapJobToResponse(job);
    }

    private async Task<MediaUploadResponse> EnqueueImageUploadAsync(
        Guid userId,
        string mediaId,
        string basePath,
        MemoryStream inputStream,
        string contentType,
        CancellationToken cancellationToken)
    {
        var extension = GetExtensionFromContentType(contentType);
        var originalPath = $"{basePath}_original{extension}";

        // Get image dimensions (fast — reads headers only)
        var dimensions = await _imageProcessingService.GetImageDimensionsAsync(inputStream, cancellationToken);

        // Upload original
        inputStream.Position = 0;
        _logger.LogDebug("Uploading original image to storage");
        var originalResult = await _storageService.UploadAsync(
            inputStream,
            originalPath,
            contentType,
            cancellationToken);

        // Persist job
        var job = new MediaProcessingJob
        {
            MediaId = mediaId,
            UserId = userId,
            Status = MediaProcessingStatus.Pending,
            ContentType = contentType,
            IsVideo = false,
            OriginalUrl = originalResult.Url,
            Width = dimensions.Width,
            Height = dimensions.Height,
            SizeBytes = originalResult.SizeBytes
        };
        _dbContext.Set<MediaProcessingJob>().Add(job);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // Enqueue processing
        await _messageQueue.EnqueueAsync(new MediaProcessingMessage
        {
            MediaId = mediaId,
            UserId = userId,
            OriginalBlobPath = originalPath,
            BaseBlobPath = basePath,
            ContentType = contentType,
            IsVideo = false,
            StandardWidth = _settings.StandardWidth,
            StandardHeight = _settings.StandardHeight,
            ThumbnailWidth = _settings.ThumbnailWidth,
            ThumbnailHeight = _settings.ThumbnailHeight,
            ImageQuality = _settings.ImageQuality
        }, cancellationToken);

        _logger.LogInformation(
            "Enqueued image processing for user {UserId}, mediaId {MediaId}",
            userId, mediaId);

        return MapJobToResponse(job);
    }

    private async Task<MediaUploadResponse> EnqueueVideoUploadAsync(
        Guid userId,
        string mediaId,
        string basePath,
        MemoryStream inputStream,
        string contentType,
        CancellationToken cancellationToken)
    {
        var extension = GetExtensionFromContentType(contentType);
        var originalPath = $"{basePath}{extension}";
        var tempPath = Path.Combine(Path.GetTempPath(), $"{mediaId}{extension}");

        try
        {
            // Write to temp file for FFprobe
            await using (var fileStream = File.Create(tempPath))
            {
                inputStream.Position = 0;
                await inputStream.CopyToAsync(fileStream, cancellationToken);
            }

            // Get video info (FFprobe — fast metadata read)
            var videoInfo = await _videoProcessingService.GetVideoInfoAsync(tempPath, cancellationToken);

            // Upload original
            inputStream.Position = 0;
            _logger.LogDebug("Uploading original video to storage");
            var originalResult = await _storageService.UploadAsync(
                inputStream,
                originalPath,
                contentType,
                cancellationToken);

            // Persist job
            var job = new MediaProcessingJob
            {
                MediaId = mediaId,
                UserId = userId,
                Status = MediaProcessingStatus.Pending,
                ContentType = contentType,
                IsVideo = true,
                OriginalUrl = originalResult.Url,
                Width = videoInfo.Width,
                Height = videoInfo.Height,
                DurationSeconds = videoInfo.DurationSeconds,
                SizeBytes = originalResult.SizeBytes
            };
            _dbContext.Set<MediaProcessingJob>().Add(job);
            await _dbContext.SaveChangesAsync(cancellationToken);

            // Enqueue processing
            await _messageQueue.EnqueueAsync(new MediaProcessingMessage
            {
                MediaId = mediaId,
                UserId = userId,
                OriginalBlobPath = originalPath,
                BaseBlobPath = basePath,
                ContentType = contentType,
                IsVideo = true,
                StandardWidth = _settings.StandardWidth,
                StandardHeight = _settings.StandardHeight,
                ThumbnailWidth = _settings.ThumbnailWidth,
                ThumbnailHeight = _settings.ThumbnailHeight,
                ImageQuality = _settings.ImageQuality
            }, cancellationToken);

            _logger.LogInformation(
                "Enqueued video processing for user {UserId}, mediaId {MediaId}, duration {Duration}s",
                userId, mediaId, videoInfo.DurationSeconds);

            return MapJobToResponse(job);
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }

    private static MediaUploadResponse MapJobToResponse(MediaProcessingJob job) => new()
    {
        MediaId = job.MediaId,
        OriginalUrl = job.OriginalUrl!,
        StandardUrl = job.StandardUrl,
        ThumbnailUrl = job.ThumbnailUrl,
        Status = job.Status,
        ContentType = job.ContentType,
        SizeBytes = job.SizeBytes,
        Width = job.Width,
        Height = job.Height,
        DurationSeconds = job.DurationSeconds,
        IsVideo = job.IsVideo
    };

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
