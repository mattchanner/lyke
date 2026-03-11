using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Admin;
using Lyke.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Lyke.Api.Endpoints.Admin;

public static class GetModerationQueue
{
    public static async Task<IResult> Handle(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        IAdminService adminService = default!,
        CancellationToken cancellationToken = default
    )
    {
        var (items, meta) = await adminService.GetModerationQueueAsync(
            page,
            pageSize,
            cancellationToken
        );
        return Results.Ok(ApiResponse<IReadOnlyList<ModerationQueueItemResponse>>.Ok(items, meta));
    }
}
