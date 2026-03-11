using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Feed;

namespace Lyke.Api.Endpoints.Search;

public static class SearchEndpointRoutes
{
    public static IEndpointRouteBuilder MapSearchEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/feed/v1/search").WithTags("Search");

        group
            .MapGet("/", Search.Handle)
            .WithName("Search")
            .WithSummary("Search posts, products, and creators")
            .Produces<ApiResponse<SearchResponse>>(StatusCodes.Status200OK)
            .AllowAnonymous();

        return app;
    }
}
