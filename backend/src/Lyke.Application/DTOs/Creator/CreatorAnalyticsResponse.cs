namespace Lyke.Application.DTOs.Creator;

public record CreatorAnalyticsResponse(
    AnalyticsSummary Summary,
    List<TopPostAnalytics> TopPosts,
    List<DailyMetrics> DailyMetrics
);

public record AnalyticsSummary(
    int TotalViews,
    int TotalLikes,
    int TotalSaves,
    int TotalShares,
    int TotalClicks,
    decimal TotalEarnings,
    string Currency
);

public record TopPostAnalytics(
    Guid PostId,
    string? Title,
    string? ThumbnailUrl,
    int Views,
    int Likes,
    int Clicks,
    decimal Earnings
);

public record DailyMetrics(DateTime Date, int Views, int Likes, int Clicks, decimal Earnings);
