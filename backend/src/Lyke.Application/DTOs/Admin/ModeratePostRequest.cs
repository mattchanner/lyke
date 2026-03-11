namespace Lyke.Application.DTOs.Admin;

public record ModeratePostRequest(bool Approve, string? RejectionReason);
