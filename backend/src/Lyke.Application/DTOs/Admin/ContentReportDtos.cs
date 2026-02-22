using Lyke.Core.Enums;

namespace Lyke.Application.DTOs.Admin;

public record ContentReportResponse(
    Guid Id,
    Guid PostId,
    string? PostTitle,
    Guid ReportedByUserId,
    ReportReason Reason,
    string? AdditionalDetails,
    ReportStatus Status,
    Guid? ReviewedByUserId,
    DateTime? ReviewedAt,
    string? ReviewNotes,
    DateTime CreatedAt
);

public record ContentReportQueryRequest(
    ReportStatus? Status,
    ReportReason? Reason,
    Guid? PostId,
    DateTime? From,
    DateTime? To,
    int Page = 1,
    int PageSize = 20
);

public record ReviewContentReportRequest(
    ReportStatus NewStatus,
    string? ReviewNotes,
    PostStatus? PostAction
);
