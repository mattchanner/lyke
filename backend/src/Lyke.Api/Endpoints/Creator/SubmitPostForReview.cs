using System.Security.Claims;
using Lyke.Api.Endpoints.Shared;
using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Creator;
using Lyke.Application.Interfaces;

namespace Lyke.Api.Endpoints.Creator;

public static class SubmitPostForReview
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

        var result = await creatorService.SubmitPostForReviewAsync(
            userId.Value,
            id,
            cancellationToken
        );
        return Results.Ok(ApiResponse<CreatorPostResponse>.Ok(result));
    }
}
