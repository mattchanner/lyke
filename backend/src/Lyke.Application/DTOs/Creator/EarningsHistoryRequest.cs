using Lyke.Core.Enums;

namespace Lyke.Application.DTOs.Creator;

public record EarningsHistoryRequest(
    EarningStatus? Status,
    DateTime? StartDate,
    DateTime? EndDate,
    int Page = 1,
    int PageSize = 20
);
