namespace Lyke.Application.Interfaces;

public record VideoInfo(int Width, int Height, double DurationSeconds, string Codec);

public interface IVideoProcessingService
{
    Task<Stream> ExtractThumbnailAsync(
        string videoPath,
        int width,
        int height,
        double atSecond = 1.0,
        CancellationToken cancellationToken = default
    );

    Task<VideoInfo> GetVideoInfoAsync(
        string videoPath,
        CancellationToken cancellationToken = default
    );

    Task<string> TranscodeAsync(
        string inputPath,
        string outputPath,
        int maxWidth,
        int maxHeight,
        CancellationToken cancellationToken = default
    );
}
