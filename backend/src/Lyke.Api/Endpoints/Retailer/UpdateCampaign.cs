using System.Security.Claims;
using Lyke.Api.Endpoints.Shared;
using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Retailer;
using Lyke.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Lyke.Api.Endpoints.Retailer;

public static class UpdateCampaign
{
    public static async Task<IResult> Handle(
        Guid id,
        [FromBody] UpdateCampaignRequest request,
        ClaimsPrincipal user,
        IRetailerService retailerService,
        CancellationToken cancellationToken
    )
    {
        var userId = UserIdExtractor.GetUserId(user);
        if (userId == null)
            return Results.Unauthorized();

        var result = await retailerService.UpdateCampaignAsync(
            userId.Value,
            id,
            request,
            cancellationToken
        );
        return Results.Ok(ApiResponse<CampaignResponse>.Ok(result));
    }
}
