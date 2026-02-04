using Lyke.Core.Enums;

namespace Lyke.Application.DTOs.Creator;

public record EarningDetailResponse(
    Guid Id,
    EarningType EarningType,
    decimal Amount,
    string Currency,
    EarningStatus Status,
    Guid PostId,
    string? PostTitle,
    Guid ProductId,
    string ProductName,
    DateTime? PaidAt,
    DateTime CreatedAt
);
