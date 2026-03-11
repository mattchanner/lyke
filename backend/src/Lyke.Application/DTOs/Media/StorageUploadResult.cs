namespace Lyke.Application.DTOs.Media;

public record StorageUploadResult(string Url, string BlobName, long SizeBytes, string ContentType);
