using Lyke.Core.Enums;

namespace Lyke.Application.DTOs.Post;

public record AuthorPostResponse(
    Guid Id,
    string? Title,
    string? Description,
    MediaType MediaType,
    List<string> MediaUrls,
    List<string> ThumbnailUrls,
    PostStatus Status,
    string? ModerationNotes,
    DateTime? PublishedAt,
    List<AuthorPostProductResponse> Products,
    AuthorPostEngagementResponse Engagements,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record AuthorPostProductResponse(
    Guid Id,
    Guid ProductId,
    string ProductName,
    string? ProductImage,
    decimal ProductPrice,
    string ProductCurrency,
    string RetailerName,
    string SizeWorn,
    FitRating? FitRating,
    string? FitNotes,
    string? StylingNotes,
    List<string> FitTags
);

public record AuthorPostEngagementResponse(int Views, int Likes, int Saves, int Shares, int Clicks);

public record AuthorPostsRequest(PostStatus? Status, int Page = 1, int PageSize = 20);
