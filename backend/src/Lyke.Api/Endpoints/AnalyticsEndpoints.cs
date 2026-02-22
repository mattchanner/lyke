using System.Security.Claims;
using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Analytics;
using Lyke.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Lyke.Api.Endpoints;

public static class AnalyticsEndpoints
{
    public static IEndpointRouteBuilder MapAnalyticsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/analytics/v1")
            .WithTags("Analytics");

        group
            .MapPost("/events", TrackEventAsync)
            .WithName("TrackEvent")
            .WithSummary("Track a single analytics event")
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .AllowAnonymous();

        group
            .MapPost("/events/batch", TrackEventBatchAsync)
            .WithName("TrackEventBatch")
            .WithSummary("Track a batch of analytics events")
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .AllowAnonymous();

        return app;
    }

    private static async Task<IResult> TrackEventAsync(
        [FromBody] TrackEventRequest request,
        ClaimsPrincipal user,
        IEventTrackingService trackingService,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(user);

        await trackingService.TrackAsync(
            request.EventType,
            userId,
            request.EntityId,
            request.EntityType,
            request.Properties,
            request.SessionId,
            cancellationToken);

        return Results.Ok(ApiResponse.Ok());
    }

    private static async Task<IResult> TrackEventBatchAsync(
        [FromBody] TrackEventBatchRequest request,
        ClaimsPrincipal user,
        IEventTrackingService trackingService,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(user);

        var items = request.Events.Select(e => new TrackEventItem(
            e.EventType,
            userId,
            e.EntityId,
            e.EntityType,
            e.Properties,
            e.SessionId));

        await trackingService.TrackBatchAsync(items, cancellationToken);

        return Results.Ok(ApiResponse.Ok());
    }

    private static Guid? GetUserId(ClaimsPrincipal user)
    {
        var sub = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(sub, out var userId) ? userId : null;
    }
}
