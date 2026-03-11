using System.Security.Claims;
using Lyke.Api.Endpoints.Shared;
using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Retailer;
using Lyke.Application.Interfaces;

namespace Lyke.Api.Endpoints.Retailer;

public static class GetBodyProfileInsights
{
    public static async Task<IResult> Handle(
        ClaimsPrincipal user,
        IRetailerService retailerService,
        CancellationToken cancellationToken
    )
    {
        var userId = UserIdExtractor.GetUserId(user);
        if (userId == null)
            return Results.Unauthorized();

        var result = await retailerService.GetBodyProfileInsightsAsync(
            userId.Value,
            cancellationToken
        );
        return Results.Ok(ApiResponse<BodyProfileInsightsResponse>.Ok(result));
    }
}
