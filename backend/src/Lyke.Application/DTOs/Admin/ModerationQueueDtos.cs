using Lyke.Core.Enums;

namespace Lyke.Application.DTOs.Admin;

public record ModerationQueueItemResponse(
    Guid Id,
    string? Title,
    string? Description,
    MediaType MediaType,
    List<string> MediaUrls,
    List<string> ThumbnailUrls,
    PostStatus Status,
    DateTime CreatedAt,
    DateTime? SubmittedAt,
    CreatorSummary Creator,
    List<PostProductSummary> Products,
    int ReportCount,
    ReportReason? TopReportReason,
    bool IsFlagged,
    int Priority
);
