namespace Lyke.Application.DTOs.Admin;

public record PlatformStatsResponse(
    UserStats Users,
    ContentStats Content,
    VerificationStats Verifications,
    EngagementStats Engagement
);

public record UserStats(
    int TotalUsers,
    int ActiveUsers,
    int SuspendedUsers,
    int Shoppers,
    int Creators,
    int Retailers,
    int Admins,
    int NewUsersLast7Days,
    int NewUsersLast30Days
);

public record ContentStats(
    int TotalPosts,
    int PublishedPosts,
    int PendingReviewPosts,
    int DraftPosts,
    int RejectedPosts,
    int FlaggedPosts,
    int RemovedPosts,
    int TotalReports,
    int PendingReports,
    int PostsLast7Days,
    int PostsLast30Days
);

public record VerificationStats(
    int PendingVerifications,
    int ApprovedCreators,
    int RejectedVerifications,
    int TotalCreators
);

public record EngagementStats(
    int TotalViews,
    int TotalLikes,
    int TotalSaves,
    int TotalClicks,
    int ViewsLast7Days,
    int ClicksLast7Days
);
