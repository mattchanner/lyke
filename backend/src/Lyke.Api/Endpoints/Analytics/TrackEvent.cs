using System.Security.Claims;
using Lyke.Api.Endpoints.Shared;
using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Analytics;
using Lyke.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Lyke.Api.Endpoints.Analytics;

public static class TrackEvent
{
    public static async Task<IResult> Handle(
        [FromBody] TrackEventRequest request,
        ClaimsPrincipal user,
        IEventTrackingService trackingService,
        CancellationToken cancellationToken
    )
    {
        var userId = UserIdExtractor.GetUserId(user);

        await trackingService.TrackAsync(
            request.EventType,
            userId,
            request.EntityId,
            request.EntityType,
            request.Properties,
            request.SessionId,
            cancellationToken
        );

        return Results.Ok(ApiResponse.Ok());
    }
}
