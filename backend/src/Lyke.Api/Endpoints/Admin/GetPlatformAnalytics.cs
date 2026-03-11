using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Admin;
using Lyke.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Lyke.Api.Endpoints.Admin;

public static class GetPlatformAnalytics
{
    public static async Task<IResult> Handle(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        IAdminService adminService,
        CancellationToken cancellationToken
    )
    {
        var request = new AdminAnalyticsRequest(startDate, endDate);
        var result = await adminService.GetPlatformAnalyticsAsync(request, cancellationToken);
        return Results.Ok(ApiResponse<AdminAnalyticsResponse>.Ok(result));
    }
}
