using System.Security.Claims;
using Lyke.Api.Endpoints.Shared;
using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Creator;
using Lyke.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Lyke.Api.Endpoints.Creator;

public static class UpdatePost
{
    public static async Task<IResult> Handle(
        Guid id,
        [FromBody] UpdatePostRequest request,
        ClaimsPrincipal user,
        ICreatorService creatorService,
        CancellationToken cancellationToken
    )
    {
        var userId = UserIdExtractor.GetUserId(user);
        if (userId == null)
            return Results.Unauthorized();

        var result = await creatorService.UpdatePostAsync(
            userId.Value,
            id,
            request,
            cancellationToken
        );
        return Results.Ok(ApiResponse<CreatorPostResponse>.Ok(result));
    }
}
