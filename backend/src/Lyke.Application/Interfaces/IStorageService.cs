using Lyke.Application.DTOs.Media;

namespace Lyke.Application.Interfaces;

public interface IStorageService
{
    Task<StorageUploadResult> UploadAsync(
        Stream content,
        string blobPath,
        string contentType,
        CancellationToken cancellationToken = default
    );

    Task DeleteAsync(string blobPath, CancellationToken cancellationToken = default);

    Task DeleteManyAsync(
        IEnumerable<string> blobPaths,
        CancellationToken cancellationToken = default
    );

    Task<string> GenerateSasUrlAsync(
        string blobPath,
        TimeSpan? expiry = null,
        CancellationToken cancellationToken = default
    );

    Task<bool> ExistsAsync(string blobPath, CancellationToken cancellationToken = default);
}
