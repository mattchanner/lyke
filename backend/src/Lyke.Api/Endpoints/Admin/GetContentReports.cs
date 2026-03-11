using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Admin;
using Lyke.Application.Interfaces;
using Lyke.Core.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Lyke.Api.Endpoints.Admin;

public static class GetContentReports
{
    public static async Task<IResult> Handle(
        [FromQuery] ReportStatus? status,
        [FromQuery] ReportReason? reason,
        [FromQuery] Guid? postId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        IAdminService adminService = default!,
        CancellationToken cancellationToken = default
    )
    {
        var request = new ContentReportQueryRequest(
            status,
            reason,
            postId,
            from,
            to,
            page,
            pageSize
        );
        var (reports, meta) = await adminService.GetContentReportsAsync(request, cancellationToken);
        return Results.Ok(ApiResponse<IReadOnlyList<ContentReportResponse>>.Ok(reports, meta));
    }
}
