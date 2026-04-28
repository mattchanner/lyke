using System.Security.Claims;
using Lyke.Api.Endpoints.Shared;
using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Post;
using Lyke.Application.Interfaces;
using Lyke.Core.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Lyke.Api.Endpoints.PostAuthor;

public static class GetAuthorPosts
{
    public static async Task<IResult> Handle(
        ClaimsPrincipal user,
        IPostAuthorService postAuthorService,
        [FromQuery] PostStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default
    )
    {
        var userId = UserIdExtractor.GetUserId(user);
        if (userId == null)
            return Results.Unauthorized();

        var request = new AuthorPostsRequest(
            status,
            page > 0 ? page : 1,
            pageSize > 0 ? pageSize : 20
        );

        var (posts, meta) = await postAuthorService.GetPostsAsync(
            userId.Value,
            request,
            cancellationToken
        );
        return Results.Ok(ApiResponse<IReadOnlyList<AuthorPostResponse>>.Ok(posts, meta));
    }
}
