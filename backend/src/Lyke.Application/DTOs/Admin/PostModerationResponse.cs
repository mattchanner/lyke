using Lyke.Core.Enums;

namespace Lyke.Application.DTOs.Admin;

public record PostModerationResponse(
    Guid Id,
    PostStatus Status,
    string? ModerationNotes,
    DateTime? ModeratedAt,
    Guid? ModeratedByUserId
);
