using Lyke.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Xabe.FFmpeg;

namespace Lyke.Infrastructure.Services;

public class FFmpegVideoProcessingService : IVideoProcessingService
{
    private readonly ILogger<FFmpegVideoProcessingService> _logger;

    public FFmpegVideoProcessingService(ILogger<FFmpegVideoProcessingService> logger)
    {
        _logger = logger;
    }

    public async Task<Stream> ExtractThumbnailAsync(
        string videoPath,
        int width,
        int height,
        double atSecond = 1.0,
        CancellationToken cancellationToken = default)
    {
        var outputPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.jpg");

        try
        {
            var conversion = await FFmpeg.Conversions.FromSnippet.Snapshot(
                videoPath,
                outputPath,
                TimeSpan.FromSeconds(atSecond));

            // Add resize filter
            conversion.AddParameter($"-vf scale={width}:{height}:force_original_aspect_ratio=decrease,pad={width}:{height}:(ow-iw)/2:(oh-ih)/2");

            await conversion.Start(cancellationToken);

            var memoryStream = new MemoryStream();
            await using (var fileStream = File.OpenRead(outputPath))
            {
                await fileStream.CopyToAsync(memoryStream, cancellationToken);
            }
            memoryStream.Position = 0;

            return memoryStream;
        }
        finally
        {
            if (File.Exists(outputPath))
            {
                File.Delete(outputPath);
            }
        }
    }

    public async Task<VideoInfo> GetVideoInfoAsync(
        string videoPath,
        CancellationToken cancellationToken = default)
    {
        var mediaInfo = await FFmpeg.GetMediaInfo(videoPath, cancellationToken);
        var videoStream = mediaInfo.VideoStreams.FirstOrDefault();

        if (videoStream == null)
        {
            throw new InvalidOperationException("No video stream found in file");
        }

        return new VideoInfo(
            Width: videoStream.Width,
            Height: videoStream.Height,
            DurationSeconds: mediaInfo.Duration.TotalSeconds,
            Codec: videoStream.Codec
        );
    }

    public async Task<string> TranscodeAsync(
        string inputPath,
        string outputPath,
        int maxWidth,
        int maxHeight,
        CancellationToken cancellationToken = default)
    {
        var mediaInfo = await FFmpeg.GetMediaInfo(inputPath, cancellationToken);
        var videoStream = mediaInfo.VideoStreams.FirstOrDefault();

        if (videoStream == null)
        {
            throw new InvalidOperationException("No video stream found in file");
        }

        // Calculate new dimensions maintaining aspect ratio
        var (newWidth, newHeight) = CalculateResizedDimensions(
            videoStream.Width, videoStream.Height, maxWidth, maxHeight);

        // Ensure dimensions are even (required for many video codecs)
        newWidth = newWidth % 2 == 0 ? newWidth : newWidth - 1;
        newHeight = newHeight % 2 == 0 ? newHeight : newHeight - 1;

        var conversion = FFmpeg.Conversions.New()
            .AddStream(videoStream.SetSize(newWidth, newHeight))
            .SetOutput(outputPath)
            .SetOverwriteOutput(true);

        // Add audio stream if present
        var audioStream = mediaInfo.AudioStreams.FirstOrDefault();
        if (audioStream != null)
        {
            conversion.AddStream(audioStream);
        }

        // Use H.264 with good compression settings
        conversion.AddParameter("-c:v libx264 -preset medium -crf 23");
        conversion.AddParameter("-c:a aac -b:a 128k");

        _logger.LogInformation("Transcoding video to {Width}x{Height}", newWidth, newHeight);

        await conversion.Start(cancellationToken);

        return outputPath;
    }

    private static (int Width, int Height) CalculateResizedDimensions(
        int originalWidth, int originalHeight, int maxWidth, int maxHeight)
    {
        if (originalWidth <= maxWidth && originalHeight <= maxHeight)
        {
            return (originalWidth, originalHeight);
        }

        var ratioX = (double)maxWidth / originalWidth;
        var ratioY = (double)maxHeight / originalHeight;
        var ratio = Math.Min(ratioX, ratioY);

        return (
            (int)Math.Round(originalWidth * ratio),
            (int)Math.Round(originalHeight * ratio)
        );
    }
}
