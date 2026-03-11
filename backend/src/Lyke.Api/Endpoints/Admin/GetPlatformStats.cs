using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Admin;
using Lyke.Application.Interfaces;

namespace Lyke.Api.Endpoints.Admin;

public static class GetPlatformStats
{
    public static async Task<IResult> Handle(
        IAdminService adminService,
        CancellationToken cancellationToken
    )
    {
        var result = await adminService.GetPlatformStatsAsync(cancellationToken);
        return Results.Ok(ApiResponse<PlatformStatsResponse>.Ok(result));
    }
}
