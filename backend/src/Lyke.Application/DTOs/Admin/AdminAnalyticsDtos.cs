namespace Lyke.Application.DTOs.Admin;

public record AdminAnalyticsRequest(
    DateTime? StartDate,
    DateTime? EndDate
);

public record AdminAnalyticsResponse(
    AdminAnalyticsSummary Summary,
    List<AdminDailyMetrics> DailyMetrics,
    List<TopCreatorAnalytics> TopCreators
);

public record AdminAnalyticsSummary(
    int NewUsers,
    int PostsPublished,
    int Views,
    int Likes,
    int Saves,
    int Clicks
);

public record AdminDailyMetrics(
    DateTime Date,
    int NewUsers,
    int PostsPublished,
    int Views,
    int Likes,
    int Saves,
    int Clicks
);

public record TopCreatorAnalytics(
    Guid CreatorId,
    string DisplayName,
    bool IsVerified,
    int TotalEngagements,
    int Views,
    int Likes,
    int Clicks
);
