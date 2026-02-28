using Lyke.Application.DTOs.Media;
using Lyke.Application.Interfaces;
using Lyke.Core.Entities;
using Lyke.Core.Enums;
using Lyke.Infrastructure.Data;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Lyke.Functions.Functions;

public class MediaProcessingFunction
{
    private readonly LykeDbContext _dbContext;
    private readonly IStorageService _storageService;
    private readonly IImageProcessingService _imageProcessingService;
    private readonly IVideoProcessingService _videoProcessingService;
    private readonly ILogger<MediaProcessingFunction> _logger;

    public MediaProcessingFunction(
        LykeDbContext dbContext,
        IStorageService storageService,
        IImageProcessingService imageProcessingService,
        IVideoProcessingService videoProcessingService,
        ILogger<MediaProcessingFunction> logger)
    {
        _dbContext = dbContext;
        _storageService = storageService;
        _imageProcessingService = imageProcessingService;
        _videoProcessingService = videoProcessingService;
        _logger = logger;
    }

    [Function("MediaProcessingFunction")]
    public async Task Run(
        [QueueTrigger("media-processing", Connection = "AzureStorage")] MediaProcessingMessage message,
        FunctionContext context)
    {
        _logger.LogInformation(
            "Processing media {MediaId} for user {UserId}",
            message.MediaId, message.UserId);

        var job = await _dbContext.MediaProcessingJobs
            .FirstOrDefaultAsync(j => j.MediaId == message.MediaId && j.UserId == message.UserId);

        if (job == null)
        {
            _logger.LogError("No MediaProcessingJob found for mediaId {MediaId}", message.MediaId);
            return;
        }

        job.Status = MediaProcessingStatus.Processing;
        await _dbContext.SaveChangesAsync();

        try
        {
            if (message.IsVideo)
                await ProcessVideoAsync(job, message);
            else
                await ProcessImageAsync(job, message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process media {MediaId}", message.MediaId);
            job.Status = MediaProcessingStatus.Failed;
            job.ErrorMessage = ex.Message;
            await _dbContext.SaveChangesAsync();
        }
    }

    private async Task ProcessImageAsync(MediaProcessingJob job, MediaProcessingMessage message)
    {
        // Download original into a seekable MemoryStream
        using var originalStream = (MemoryStream)await _storageService.DownloadAsync(message.OriginalBlobPath);

        // Process standard size
        var processed = await _imageProcessingService.ProcessImageAsync(
            originalStream,
            message.StandardWidth,
            message.StandardHeight,
            message.ImageQuality);

        var standardResult = await _storageService.UploadAsync(
            processed.Content,
            $"{message.BaseBlobPath}_standard.webp",
            processed.ContentType);

        // Create thumbnail (reset stream first)
        originalStream.Position = 0;
        var thumbnail = await _imageProcessingService.CreateThumbnailAsync(
            originalStream,
            message.ThumbnailWidth,
            message.ThumbnailHeight);

        var thumbnailResult = await _storageService.UploadAsync(
            thumbnail.Content,
            $"{message.BaseBlobPath}_thumb.jpg",
            thumbnail.ContentType);

        job.StandardUrl = standardResult.Url;
        job.ThumbnailUrl = thumbnailResult.Url;
        job.Status = MediaProcessingStatus.Completed;
        job.CompletedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Image processing completed for mediaId {MediaId}", message.MediaId);
    }

    private async Task ProcessVideoAsync(MediaProcessingJob job, MediaProcessingMessage message)
    {
        var extension = Path.GetExtension(message.OriginalBlobPath);
        var tempPath = Path.Combine(Path.GetTempPath(), $"{message.MediaId}{extension}");

        try
        {
            // Download original to temp file for FFmpeg
            using var videoStream = await _storageService.DownloadAsync(message.OriginalBlobPath);
            await using (var fileStream = File.Create(tempPath))
            {
                await videoStream.CopyToAsync(fileStream);
            }

            // Extract thumbnail
            using var thumbnailStream = await _videoProcessingService.ExtractThumbnailAsync(
                tempPath,
                message.ThumbnailWidth,
                message.ThumbnailHeight);

            var thumbnailResult = await _storageService.UploadAsync(
                thumbnailStream,
                $"{message.BaseBlobPath}_thumb.jpg",
                "image/jpeg");

            job.ThumbnailUrl = thumbnailResult.Url;
            job.Status = MediaProcessingStatus.Completed;
            job.CompletedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Video processing completed for mediaId {MediaId}", message.MediaId);
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }
}
