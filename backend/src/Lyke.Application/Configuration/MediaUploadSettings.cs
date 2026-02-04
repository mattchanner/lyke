namespace Lyke.Application.Configuration;

public class MediaUploadSettings
{
    public const string SectionName = "MediaUpload";

    public long MaxImageFileSizeBytes { get; set; } = 10 * 1024 * 1024; // 10 MB
    public long MaxVideoFileSizeBytes { get; set; } = 100 * 1024 * 1024; // 100 MB
    public int MaxMediaPerPost { get; set; } = 10;
    public string[] AllowedImageMimeTypes { get; set; } = ["image/jpeg", "image/png", "image/webp"];
    public string[] AllowedVideoMimeTypes { get; set; } = ["video/mp4", "video/quicktime", "video/webm"];
    public int ThumbnailWidth { get; set; } = 400;
    public int ThumbnailHeight { get; set; } = 400;
    public int StandardWidth { get; set; } = 1080;
    public int StandardHeight { get; set; } = 1920;
    public int ImageQuality { get; set; } = 85;
    public int SasTokenExpiryMinutes { get; set; } = 60;
}
