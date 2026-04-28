using System.Security.Claims;
using Lyke.Api.Endpoints.Shared;
using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Feed;
using Lyke.Application.Interfaces;

namespace Lyke.Api.Endpoints.Feed;

public static class GetExploreFeed
{
    public static async Task<IResult> Handle(
        [AsParameters] FeedQueryParams queryParams,
        ClaimsPrincipal user,
        IFeedService feedService,
        CancellationToken cancellationToken
    )
    {
        var userId = UserIdExtractor.GetUserId(user);

        var request = new FeedRequest(
            queryParams.Page ?? 1,
            queryParams.PageSize ?? 20,
            queryParams.Category,
            queryParams.RetailerId,
            queryParams.FitTagIds?.Split(',').Select(int.Parse).ToList(),
            queryParams.SortBy ?? FeedSortBy.Recent,
            queryParams.CreatorId,
            queryParams.AuthorUserId
        );

        var (posts, meta) = await feedService.GetExploreFeedAsync(
            userId,
            request,
            cancellationToken
        );
        return Results.Ok(ApiResponse<IReadOnlyList<FeedPostResponse>>.Ok(posts, meta));
    }
}
