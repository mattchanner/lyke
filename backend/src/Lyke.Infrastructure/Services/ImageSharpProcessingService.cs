using Lyke.Application.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace Lyke.Infrastructure.Services;

public class ImageSharpProcessingService : IImageProcessingService
{
    public async Task<ProcessedImage> ProcessImageAsync(
        Stream input,
        int maxWidth,
        int maxHeight,
        int quality,
        CancellationToken cancellationToken = default
    )
    {
        input.Position = 0;
        using var image = await Image.LoadAsync(input, cancellationToken);

        // Auto-orient based on EXIF data
        image.Mutate(x => x.AutoOrient());

        // Resize maintaining aspect ratio
        var (newWidth, newHeight) = CalculateResizedDimensions(
            image.Width,
            image.Height,
            maxWidth,
            maxHeight
        );

        if (newWidth != image.Width || newHeight != image.Height)
        {
            image.Mutate(x => x.Resize(newWidth, newHeight));
        }

        var output = new MemoryStream();
        var encoder = new WebpEncoder { Quality = quality };
        await image.SaveAsync(output, encoder, cancellationToken);
        output.Position = 0;

        return new ProcessedImage(
            Content: output,
            ContentType: "image/webp",
            Width: newWidth,
            Height: newHeight
        );
    }

    public async Task<ProcessedImage> CreateThumbnailAsync(
        Stream input,
        int width,
        int height,
        CancellationToken cancellationToken = default
    )
    {
        input.Position = 0;
        using var image = await Image.LoadAsync(input, cancellationToken);

        // Auto-orient based on EXIF data
        image.Mutate(x => x.AutoOrient());

        // Resize and crop to exact dimensions (cover mode)
        image.Mutate(x =>
            x.Resize(new ResizeOptions { Size = new Size(width, height), Mode = ResizeMode.Crop })
        );

        var output = new MemoryStream();
        var encoder = new JpegEncoder { Quality = 80 };
        await image.SaveAsync(output, encoder, cancellationToken);
        output.Position = 0;

        return new ProcessedImage(
            Content: output,
            ContentType: "image/jpeg",
            Width: width,
            Height: height
        );
    }

    public async Task<ImageDimensions> GetImageDimensionsAsync(
        Stream input,
        CancellationToken cancellationToken = default
    )
    {
        input.Position = 0;
        var imageInfo = await Image.IdentifyAsync(input, cancellationToken);

        return new ImageDimensions(imageInfo.Width, imageInfo.Height);
    }

    private static (int Width, int Height) CalculateResizedDimensions(
        int originalWidth,
        int originalHeight,
        int maxWidth,
        int maxHeight
    )
    {
        if (originalWidth <= maxWidth && originalHeight <= maxHeight)
        {
            return (originalWidth, originalHeight);
        }

        var ratioX = (double)maxWidth / originalWidth;
        var ratioY = (double)maxHeight / originalHeight;
        var ratio = Math.Min(ratioX, ratioY);

        return ((int)Math.Round(originalWidth * ratio), (int)Math.Round(originalHeight * ratio));
    }
}
