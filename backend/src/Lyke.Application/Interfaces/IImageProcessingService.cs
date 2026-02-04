namespace Lyke.Application.Interfaces;

public record ImageDimensions(int Width, int Height);

public record ProcessedImage(Stream Content, string ContentType, int Width, int Height);

public interface IImageProcessingService
{
    Task<ProcessedImage> ProcessImageAsync(
        Stream input,
        int maxWidth,
        int maxHeight,
        int quality,
        CancellationToken cancellationToken = default
    );

    Task<ProcessedImage> CreateThumbnailAsync(
        Stream input,
        int width,
        int height,
        CancellationToken cancellationToken = default
    );

    Task<ImageDimensions> GetImageDimensionsAsync(
        Stream input,
        CancellationToken cancellationToken = default
    );
}
