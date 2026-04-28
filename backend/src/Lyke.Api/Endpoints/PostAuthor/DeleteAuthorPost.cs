using System.Security.Claims;
using Lyke.Api.Endpoints.Shared;
using Lyke.Application.DTOs;
using Lyke.Application.Interfaces;

namespace Lyke.Api.Endpoints.PostAuthor;

public static class DeleteAuthorPost
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

        await postAuthorService.DeletePostAsync(userId.Value, id, cancellationToken);
        return Results.Ok(ApiResponse.Ok());
    }
}
