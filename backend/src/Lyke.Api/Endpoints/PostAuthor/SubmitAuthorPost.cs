using System.Security.Claims;
using Lyke.Api.Endpoints.Shared;
using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Post;
using Lyke.Application.Interfaces;

namespace Lyke.Api.Endpoints.PostAuthor;

public static class SubmitAuthorPost
{
    public static async Task<IResult> Handle(
        Guid id,
        ClaimsPrincipal user,
        IPostAuthorService postAuthorService,
        CancellationToken cancellationToken
    )
    {
        var userId = UserIdExtractor.GetUserId(user);
        if (userId == null)
            return Results.Unauthorized();

        var result = await postAuthorService.SubmitPostForReviewAsync(
            userId.Value,
            id,
            cancellationToken
        );
        return Results.Ok(ApiResponse<AuthorPostResponse>.Ok(result));
    }
}
