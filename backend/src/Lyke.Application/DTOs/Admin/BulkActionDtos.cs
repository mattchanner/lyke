using Lyke.Core.Enums;

namespace Lyke.Application.DTOs.Admin;

public enum BulkPostAction
{
    Approve,
    Reject,
    Remove,
    Flag
}

public record BulkModeratePostsRequest(
    List<Guid> PostIds,
    BulkPostAction Action,
    string? Reason
);

public record BulkSuspendUsersRequest(
    List<Guid> UserIds,
    string Reason
);

public record BulkActionResult(
    int SuccessCount,
    int FailureCount,
    List<BulkActionError> Errors
);

public record BulkActionError(
    Guid Id,
    string Error
);
