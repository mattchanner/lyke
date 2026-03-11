using System.Security.Claims;
using Lyke.Api.Endpoints.Shared;
using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Retailer;
using Lyke.Application.Interfaces;

namespace Lyke.Api.Endpoints.Retailer;

public static class GetAnalytics
{
    public static async Task<IResult> Handle(
        [AsParameters] RetailerAnalyticsRequest request,
        ClaimsPrincipal user,
        IRetailerService retailerService,
        CancellationToken cancellationToken
    )
    {
        var userId = UserIdExtractor.GetUserId(user);
        if (userId == null)
            return Results.Unauthorized();

        var result = await retailerService.GetAnalyticsAsync(
            userId.Value,
            request,
            cancellationToken
        );
        return Results.Ok(ApiResponse<RetailerAnalyticsResponse>.Ok(result));
    }
}
