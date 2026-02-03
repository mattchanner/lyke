using Lyke.Application.DTOs.Profile;
using Lyke.Core.Enums;

namespace Lyke.Application.DTOs.Feed;

public record FeedPostResponse(
    Guid Id,
    string? Title,
    string? Description,
    MediaType MediaType,
    List<string> MediaUrls,
    CreatorSummaryResponse Creator,
    List<PostProductSummaryResponse> Products,
    EngagementCountsResponse Engagements,
    double SimilarityScore,
    bool IsLiked,
    bool IsSaved,
    DateTime PublishedAt
);

public record CreatorSummaryResponse(
    Guid Id,
    string DisplayName,
    bool IsVerified,
    AnonymizedBodyProfileResponse? BodyProfile
);

public record PostProductSummaryResponse(
    Guid Id,
    Guid ProductId,
    string ProductName,
    string? ProductImageUrl,
    decimal Price,
    string Currency,
    string SizeWorn,
    FitRating? FitRating,
    string? FitNotes,
    List<string> FitTags
);

public record EngagementCountsResponse(
    int Views,
    int Likes,
    int Saves,
    int Shares
);
