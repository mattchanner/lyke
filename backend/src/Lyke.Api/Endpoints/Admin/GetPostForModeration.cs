using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Admin;
using Lyke.Application.Interfaces;

namespace Lyke.Api.Endpoints.Admin;

public static class GetPostForModeration
{
    public static async Task<IResult> Handle(
        Guid postId,
        IAdminService adminService,
        CancellationToken cancellationToken
    )
    {
        var result = await adminService.GetPostForModerationAsync(postId, cancellationToken);
        return Results.Ok(ApiResponse<PendingPostResponse>.Ok(result));
    }
}
