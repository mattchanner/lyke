using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Feed;
using Lyke.Application.DTOs.Profile;

namespace Lyke.Api.Endpoints.Lookup;

public static class LookupEndpointRoutes
{
    public static IEndpointRouteBuilder MapLookupEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/lookup/v1").WithTags("Lookups");

        group
            .MapGet("/body-types", GetBodyTypes.Handle)
            .WithName("GetBodyTypes")
            .WithSummary("Get available body types")
            .Produces<ApiResponse<IReadOnlyList<BodyTypeResponse>>>(StatusCodes.Status200OK)
            .AllowAnonymous()
            .CacheOutput("LookupData");

        group
            .MapGet("/frame-sizes", GetFrameSizes.Handle)
            .WithName("GetFrameSizes")
            .WithSummary("Get available frame sizes")
            .Produces<ApiResponse<IReadOnlyList<FrameSizeResponse>>>(StatusCodes.Status200OK)
            .AllowAnonymous()
            .CacheOutput("LookupData");

        group
            .MapGet("/fit-preferences", GetFitPreferences.Handle)
            .WithName("GetFitPreferences")
            .WithSummary("Get available fit preferences")
            .Produces<ApiResponse<IReadOnlyList<FitPreferenceResponse>>>(StatusCodes.Status200OK)
            .AllowAnonymous()
            .CacheOutput("LookupData");

        group
            .MapGet("/fit-tags", GetFitTags.Handle)
            .WithName("GetFitTags")
            .WithSummary("Get available fit tags")
            .Produces<ApiResponse<IReadOnlyList<FitTagResponse>>>(StatusCodes.Status200OK)
            .AllowAnonymous()
            .CacheOutput("LookupData");

        return app;
    }
}
