using Lyke.Core.Enums;

namespace Lyke.Application.DTOs.Admin;

public record PendingPostResponse(
    Guid Id,
    string? Title,
    string? Description,
    MediaType MediaType,
    List<string> MediaUrls,
    List<string> ThumbnailUrls,
    PostStatus Status,
    DateTime CreatedAt,
    DateTime? SubmittedAt,
    CreatorSummary Creator,
    List<PostProductSummary> Products
);

public record CreatorSummary(
    Guid Id,
    string DisplayName,
    bool IsVerified,
    int TotalPosts,
    int PublishedPosts
);

public record PostProductSummary(
    Guid ProductId,
    string ProductName,
    string? SizeWorn,
    string? FitNotes
);
