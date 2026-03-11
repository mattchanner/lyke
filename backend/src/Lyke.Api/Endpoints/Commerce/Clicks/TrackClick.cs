using System.Security.Claims;
using Lyke.Api.Endpoints.Shared;
using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Commerce;
using Lyke.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Lyke.Api.Endpoints.Commerce.Clicks;

public static class TrackClick
{
    public static async Task<IResult> Handle(
        [FromBody] TrackClickRequest request,
        ClaimsPrincipal user,
        ICommerceService commerceService,
        HttpContext httpContext,
        CancellationToken cancellationToken
    )
    {
        var userId = UserIdExtractor.GetUserId(user);
        var userAgent = httpContext.Request.Headers.UserAgent.ToString();
        var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();

        var result = await commerceService.TrackClickAsync(
            userId,
            request,
            userAgent,
            ipAddress,
            cancellationToken
        );

        return Results.Ok(ApiResponse<TrackClickResponse>.Ok(result));
    }
}
