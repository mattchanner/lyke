using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Admin;
using Lyke.Application.Interfaces;

namespace Lyke.Api.Endpoints.Admin;

public static class GetContentReport
{
    public static async Task<IResult> Handle(
        Guid reportId,
        IAdminService adminService,
        CancellationToken cancellationToken
    )
    {
        var result = await adminService.GetContentReportAsync(reportId, cancellationToken);
        return Results.Ok(ApiResponse<ContentReportResponse>.Ok(result));
    }
}
