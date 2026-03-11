using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Admin;
using Lyke.Application.Interfaces;
using Lyke.Core.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Lyke.Api.Endpoints.Admin;

public static class GetPendingPosts
{
    public static async Task<IResult> Handle(
        [FromQuery] PostStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        IAdminService adminService = default!,
        CancellationToken cancellationToken = default
    )
    {
        var (posts, meta) = await adminService.GetPendingPostsAsync(
            status,
            page,
            pageSize,
            cancellationToken
        );
        return Results.Ok(ApiResponse<IReadOnlyList<PendingPostResponse>>.Ok(posts, meta));
    }
}
