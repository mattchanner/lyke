using Lyke.Application.DTOs;

namespace Lyke.Api.Endpoints.Analytics;

public static class AnalyticsEndpointRoutes
{
    public static IEndpointRouteBuilder MapAnalyticsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/analytics/v1").WithTags("Analytics");

        group
            .MapPost("/events", TrackEvent.Handle)
            .WithName("TrackEvent")
            .WithSummary("Track a single analytics event")
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .AllowAnonymous();

        group
            .MapPost("/events/batch", TrackEventBatch.Handle)
            .WithName("TrackEventBatch")
            .WithSummary("Track a batch of analytics events")
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .AllowAnonymous();

        return app;
    }
}
