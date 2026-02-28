using Lyke.Application.DTOs.Media;
using Microsoft.AspNetCore.Http;

namespace Lyke.Application.Interfaces;

public interface IMediaService
{
    Task<MediaUploadResponse> UploadMediaAsync(
        Guid userId,
        IFormFile file,
        CancellationToken cancellationToken = default
    );

    Task<BulkMediaUploadResponse> UploadMediaBulkAsync(
        Guid userId,
        IFormFileCollection files,
        CancellationToken cancellationToken = default
    );

    Task DeleteMediaAsync(
        Guid userId,
        IEnumerable<string> mediaIds,
        CancellationToken cancellationToken = default
    );

    Task<string> GetSecureUrlAsync(string mediaUrl, CancellationToken cancellationToken = default);

    Task<MediaUploadResponse?> GetMediaStatusAsync(
        string mediaId,
        Guid userId,
        CancellationToken cancellationToken = default
    );
}
