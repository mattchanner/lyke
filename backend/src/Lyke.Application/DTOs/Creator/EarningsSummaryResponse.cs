namespace Lyke.Application.DTOs.Creator;

public record EarningsSummaryResponse(
    decimal TotalEarnings,
    decimal PendingEarnings,
    decimal ConfirmedEarnings,
    decimal PaidEarnings,
    string Currency,
    decimal MinPayoutThreshold,
    bool EligibleForPayout
);
