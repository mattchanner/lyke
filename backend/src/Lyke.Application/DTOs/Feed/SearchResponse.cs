namespace Lyke.Application.DTOs.Feed;

public record SearchResponse(
    List<FeedPostResponse> Posts,
    List<ProductSearchResult> Products,
    List<CreatorSearchResult> Creators,
    int TotalResults
);

public record ProductSearchResult(
    Guid Id,
    string Name,
    string? ImageUrl,
    decimal Price,
    string Currency,
    string RetailerName,
    int PostCount
);

public record CreatorSearchResult(
    Guid Id,
    string DisplayName,
    bool IsVerified,
    int PostCount
);
