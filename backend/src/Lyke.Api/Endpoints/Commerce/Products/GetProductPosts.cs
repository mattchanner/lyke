using System.Security.Claims;
using Lyke.Api.Endpoints.Shared;
using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Feed;
using Lyke.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Lyke.Api.Endpoints.Commerce.Products;

public static class GetProductPosts
{
    public static async Task<IResult> Handle(
        Guid id,
        [FromQuery] int? limit,
        ClaimsPrincipal user,
        ICommerceService commerceService,
        CancellationToken cancellationToken
    )
    {
        var userId = UserIdExtractor.GetUserId(user);
        var posts = await commerceService.GetProductPostsAsync(
            id,
            userId,
            limit ?? 10,
            cancellationToken
        );
        return Results.Ok(ApiResponse<IReadOnlyList<FeedPostResponse>>.Ok(posts));
    }
}
