using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Lyke.Application.DTOs.Media;
using Lyke.Application.Interfaces;
using Lyke.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lyke.Infrastructure.Storage;

public class AzureBlobStorageService : IStorageService
{
    private readonly BlobContainerClient _containerClient;
    private readonly AzureBlobSettings _settings;
    private readonly ILogger<AzureBlobStorageService> _logger;

    public AzureBlobStorageService(
        AzureBlobSettings settings,
        ILogger<AzureBlobStorageService> logger
    )
    {
        _settings = settings;
        _logger = logger;

        var blobServiceClient = new BlobServiceClient(_settings.ConnectionString);
        _containerClient = blobServiceClient.GetBlobContainerClient(_settings.ContainerName);
    }

    public async Task<StorageUploadResult> UploadAsync(
        Stream content,
        string blobPath,
        string contentType,
        CancellationToken cancellationToken = default
    )
    {
        var blobClient = _containerClient.GetBlobClient(blobPath);

        await _containerClient.CreateIfNotExistsAsync(
            PublicAccessType.Blob,
            cancellationToken: cancellationToken
        );

        // Ensure access level is correct even if the container already existed
        await _containerClient.SetAccessPolicyAsync(
            PublicAccessType.Blob,
            cancellationToken: cancellationToken
        );

        var options = new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders { ContentType = contentType },
        };

        content.Position = 0;
        await blobClient.UploadAsync(content, options, cancellationToken);

        _logger.LogInformation(
            "Uploaded blob {BlobPath} with content type {ContentType}",
            blobPath,
            contentType
        );

        return new StorageUploadResult(
            Url: blobClient.Uri.ToString(),
            BlobName: blobPath,
            SizeBytes: content.Length,
            ContentType: contentType
        );
    }

    public async Task DeleteAsync(string blobPath, CancellationToken cancellationToken = default)
    {
        var blobClient = _containerClient.GetBlobClient(blobPath);
        var deleted = await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);

        if (deleted)
        {
            _logger.LogInformation("Deleted blob {BlobPath}", blobPath);
        }
    }

    public async Task DeleteManyAsync(
        IEnumerable<string> blobPaths,
        CancellationToken cancellationToken = default
    )
    {
        var tasks = blobPaths.Select(path => DeleteAsync(path, cancellationToken));
        await Task.WhenAll(tasks);
    }

    public Task<string> GenerateSasUrlAsync(
        string blobPath,
        TimeSpan? expiry = null,
        CancellationToken cancellationToken = default
    )
    {
        var blobClient = _containerClient.GetBlobClient(blobPath);

        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = _settings.ContainerName,
            BlobName = blobPath,
            Resource = "b",
            ExpiresOn = DateTimeOffset.UtcNow.Add(expiry ?? TimeSpan.FromHours(1)),
        };

        sasBuilder.SetPermissions(BlobSasPermissions.Read);

        var sasUri = blobClient.GenerateSasUri(sasBuilder);
        return Task.FromResult(sasUri.ToString());
    }

    public async Task<bool> ExistsAsync(
        string blobPath,
        CancellationToken cancellationToken = default
    )
    {
        var blobClient = _containerClient.GetBlobClient(blobPath);
        return await blobClient.ExistsAsync(cancellationToken);
    }
}
