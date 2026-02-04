using Lyke.Core.Enums;

namespace Lyke.Application.DTOs.Creator;

public record PendingVerificationResponse(
    Guid CreatorId,
    string DisplayName,
    string? Bio,
    Dictionary<string, string>? SocialLinks,
    VerificationStatus Status,
    string? Notes,
    List<string>? DocumentUrls,
    DateTime? RequestedAt,
    int TotalPosts,
    int PublishedPosts,
    DateTime CreatedAt
);
