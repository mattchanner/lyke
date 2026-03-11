using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Commerce;

namespace Lyke.Api.Endpoints.Commerce.Clicks;

public static class ClickEndpointRoutes
{
    public static void MapClickEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/commerce/v1/clicks").WithTags("Click Tracking");

        group
            .MapPost("/track", TrackClick.Handle)
            .WithName("TrackClick")
            .WithSummary("Track outbound click and get affiliate URL")
            .Produces<ApiResponse<TrackClickResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound)
            .AllowAnonymous();
    }
}
