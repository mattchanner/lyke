using System.Security.Claims;
using Lyke.Api.Endpoints.Shared;
using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Creator;
using Lyke.Application.DTOs.Post;
using Lyke.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Lyke.Api.Endpoints.PostAuthor;

public static class UpdateAuthorPost
{
    public static async Task<IResult> Handle(
        Guid id,
        [FromBody] UpdatePostRequest request,
        ClaimsPrincipal user,
        IPostAuthorService postAuthorService,
        CancellationToken cancellationToken
    )
    {
        var userId = UserIdExtractor.GetUserId(user);
        if (userId == null)
            return Results.Unauthorized();

        var result = await postAuthorService.UpdatePostAsync(
            userId.Value,
            id,
            request,
            cancellationToken
        );
        return Results.Ok(ApiResponse<AuthorPostResponse>.Ok(result));
    }
}
