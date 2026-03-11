using System.Security.Claims;
using Lyke.Api.Endpoints.Shared;
using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Analytics;
using Lyke.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Lyke.Api.Endpoints.Analytics;

public static class TrackEventBatch
{
    public static async Task<IResult> Handle(
        [FromBody] TrackEventBatchRequest request,
        ClaimsPrincipal user,
        IEventTrackingService trackingService,
        CancellationToken cancellationToken
    )
    {
        var userId = UserIdExtractor.GetUserId(user);

        var items = request.Events.Select(e => new TrackEventItem(
            e.EventType,
            userId,
            e.EntityId,
            e.EntityType,
            e.Properties,
            e.SessionId
        ));

        await trackingService.TrackBatchAsync(items, cancellationToken);

        return Results.Ok(ApiResponse.Ok());
    }
}
