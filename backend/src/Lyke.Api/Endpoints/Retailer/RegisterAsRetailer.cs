using System.Security.Claims;
using Lyke.Api.Endpoints.Shared;
using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Retailer;
using Lyke.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Lyke.Api.Endpoints.Retailer;

public static class RegisterAsRetailer
{
    public static async Task<IResult> Handle(
        [FromBody] RegisterRetailerRequest request,
        ClaimsPrincipal user,
        IRetailerService retailerService,
        CancellationToken cancellationToken
    )
    {
        var userId = UserIdExtractor.GetUserId(user);
        if (userId == null)
            return Results.Unauthorized();

        var result = await retailerService.RegisterAsRetailerAsync(
            userId.Value,
            request,
            cancellationToken
        );
        return Results.Ok(ApiResponse<RetailerProfileResponse>.Ok(result));
    }
}
