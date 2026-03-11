using System.Security.Claims;
using Lyke.Api.Endpoints.Shared;
using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Feed;
using Lyke.Application.Interfaces;

namespace Lyke.Api.Endpoints.Search;

public static class Search
{
    public static async Task<IResult> Handle(
        [AsParameters] SearchQueryParams queryParams,
        ClaimsPrincipal user,
        IFeedService feedService,
        CancellationToken cancellationToken
    )
    {
        var userId = UserIdExtractor.GetUserId(user);

        var request = new SearchRequest(
            queryParams.Q ?? "",
            queryParams.Page ?? 1,
            queryParams.PageSize ?? 20,
            queryParams.Type
        );

        var result = await feedService.SearchAsync(request, userId, cancellationToken);
        return Results.Ok(ApiResponse<SearchResponse>.Ok(result));
    }
}
