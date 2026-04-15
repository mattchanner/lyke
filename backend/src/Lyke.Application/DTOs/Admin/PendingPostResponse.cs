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
    AuthorSummary Author,
    List<PostProductSummary> Products
);

public record AuthorSummary(
    Guid UserId,
    Guid? CreatorId,
    string DisplayName,
    bool IsVerified,
    UserType UserType,
    int TotalPosts,
    int PublishedPosts
);

public record PostProductSummary(
    Guid ProductId,
    string ProductName,
    string? SizeWorn,
    string? FitNotes
);
