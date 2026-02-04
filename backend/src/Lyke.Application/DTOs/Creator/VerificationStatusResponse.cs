using Lyke.Core.Enums;

namespace Lyke.Application.DTOs.Creator;

public record VerificationStatusResponse(
    VerificationStatus Status,
    string? Notes,
    List<string>? DocumentUrls,
    DateTime? RequestedAt,
    DateTime? ReviewedAt,
    string? RejectionReason
);
