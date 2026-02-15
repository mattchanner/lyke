using Lyke.Core.Enums;

namespace Lyke.Application.DTOs.Creator;

public record CreatePostRequest(
    string? Title,
    string? Description,
    MediaType MediaType,
    List<string> MediaUrls,
    List<string>? ThumbnailUrls,
    List<PostProductRequest> Products
);
