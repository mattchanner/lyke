namespace Lyke.Application.DTOs.Creator;

public record ReviewVerificationRequest(
    bool Approve,
    string? RejectionReason
);
