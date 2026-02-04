namespace Lyke.Application.DTOs.Commerce;

public record TrackClickRequest(
    Guid PostId,
    Guid PostProductId,
    string? SessionId = null,
    string? Source = null,        // feed, post_detail, search, similar_posts
    int? FeedPosition = null,
    string? SearchQuery = null,
    string? Platform = null,      // ios, android, web
    string? AppVersion = null
);
