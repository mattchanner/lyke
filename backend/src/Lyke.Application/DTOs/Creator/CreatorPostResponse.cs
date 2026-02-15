using Lyke.Core.Enums;

namespace Lyke.Application.DTOs.Creator;

public record CreatorPostResponse(
    Guid Id,
    string? Title,
    string? Description,
    MediaType MediaType,
    List<string> MediaUrls,
    List<string> ThumbnailUrls,
    PostStatus Status,
    string? ModerationNotes,
    DateTime? PublishedAt,
    List<CreatorPostProductResponse> Products,
    CreatorPostEngagementResponse Engagements,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record CreatorPostProductResponse(
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

public record CreatorPostEngagementResponse(
    int Views,
    int Likes,
    int Saves,
    int Shares,
    int Clicks
);
