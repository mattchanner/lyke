using Lyke.Application.DTOs.Profile;
using Lyke.Core.Enums;

namespace Lyke.Application.DTOs.Feed;

public record PostDetailResponse(
    Guid Id,
    string? Title,
    string? Description,
    MediaType MediaType,
    List<string> MediaUrls,
    List<string> ThumbnailUrls,
    CreatorDetailResponse Creator,
    List<PostProductDetailResponse> Products,
    EngagementCountsResponse Engagements,
    double SimilarityScore,
    bool IsLiked,
    bool IsSaved,
    DateTime PublishedAt,
    DateTime CreatedAt
);

public record CreatorDetailResponse(
    Guid Id,
    string DisplayName,
    string? Bio,
    bool IsVerified,
    AnonymizedBodyProfileResponse? BodyProfile,
    int TotalPosts
);

public record PostProductDetailResponse(
    Guid Id,
    Guid ProductId,
    string ProductName,
    string? ProductDescription,
    List<string> ProductImageUrls,
    string ProductUrl,
    decimal Price,
    string Currency,
    string RetailerName,
    string SizeWorn,
    FitRating? FitRating,
    string? FitNotes,
    string? StylingNotes,
    List<FitTagResponse> FitTags
);

public record FitTagResponse(
    int Id,
    string Name,
    string? Category
);
