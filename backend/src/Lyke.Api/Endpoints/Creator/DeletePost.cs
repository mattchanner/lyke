using System.Security.Claims;
using Lyke.Api.Endpoints.Shared;
using Lyke.Application.DTOs;
using Lyke.Application.Interfaces;

namespace Lyke.Api.Endpoints.Creator;

public static class DeletePost
{
    public static async Task<IResult> Handle(
        Guid id,
        ClaimsPrincipal user,
        ICreatorService creatorService,
        CancellationToken cancellationToken
    )
    {
        var userId = UserIdExtractor.GetUserId(user);
        if (userId == null)
            return Results.Unauthorized();

        await creatorService.DeletePostAsync(userId.Value, id, cancellationToken);
        return Results.Ok(ApiResponse.Ok());
    }
}
