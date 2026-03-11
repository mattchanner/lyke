namespace Lyke.Application.DTOs.Privacy;

public record DataExportResponse(
    UserExportData User,
    BodyProfileExportData? BodyProfile,
    CreatorExportData? Creator,
    IReadOnlyList<PostExportData> Posts,
    IReadOnlyList<EngagementExportData> Engagements,
    IReadOnlyList<ClickEventExportData> ClickEvents,
    IReadOnlyList<EarningExportData> Earnings,
    DateTime ExportedAt
);

public record UserExportData(
    Guid Id,
    string? Email,
    string? UserName,
    string? PhoneNumber,
    string UserType,
    bool IsActive,
    bool EmailConfirmed,
    DateTime CreatedAt,
    DateTime? PrivacyPolicyAcceptedAt,
    string? PrivacyPolicyVersion,
    bool MarketingOptIn
);

public record BodyProfileExportData(
    int HeightCm,
    decimal WeightKg,
    string BodyTypeName,
    string? FrameSizeName,
    string FitPreferences,
    DateTime CreatedAt
);

public record CreatorExportData(
    string DisplayName,
    string? Bio,
    bool IsVerified,
    string? SocialLinks,
    string VerificationStatus,
    DateTime CreatedAt,
    IReadOnlyList<PostExportData> Posts
);

public record PostExportData(
    Guid Id,
    string? Title,
    string? Description,
    string MediaType,
    string? MediaUrls,
    string Status,
    DateTime? PublishedAt,
    DateTime CreatedAt,
    IReadOnlyList<PostProductExportData> Products
);

public record PostProductExportData(
    string ProductName,
    string SizeWorn,
    string? FitNotes,
    string? FitRating,
    string? StylingNotes,
    IReadOnlyList<string> FitTags
);

public record EngagementExportData(Guid PostId, string Type, DateTime CreatedAt);

public record ClickEventExportData(Guid PostId, DateTime CreatedAt, DateTime? ConvertedAt);

public record EarningExportData(
    string EarningType,
    decimal Amount,
    string Currency,
    string Status,
    DateTime? PaidAt,
    DateTime CreatedAt
);
