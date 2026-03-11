namespace Lyke.Application.DTOs.Retailer;

public record RetailerAnalyticsResponse(
    RetailerAnalyticsSummary Summary,
    List<RetailerDailyMetrics> DailyMetrics,
    List<TopProductAnalytics> TopProducts,
    List<CategoryBreakdown> CategoryBreakdown
);

public record RetailerAnalyticsSummary(
    int TotalViews,
    int TotalClicks,
    int TotalConversions,
    decimal TotalRevenue,
    int TotalPosts,
    string Currency
);

public record RetailerDailyMetrics(
    DateTime Date,
    int Views,
    int Clicks,
    int Conversions,
    decimal Revenue
);

public record TopProductAnalytics(
    Guid ProductId,
    string ProductName,
    string Category,
    int Views,
    int Clicks,
    int Conversions,
    decimal Revenue
);

public record CategoryBreakdown(
    string Category,
    int ProductCount,
    int PostCount,
    int Clicks,
    int Conversions
);
