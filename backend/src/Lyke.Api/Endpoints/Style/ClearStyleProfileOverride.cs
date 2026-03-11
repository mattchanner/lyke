using System.Security.Claims;
using Lyke.Api.Endpoints.Shared;
using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Style;
using Lyke.Application.Interfaces;

namespace Lyke.Api.Endpoints.Style;

public static class ClearStyleProfileOverride
{
    public static async Task<IResult> Handle(
        ClaimsPrincipal user,
        IKibbeQuizService kibbeQuizService,
        CancellationToken cancellationToken
    )
    {
        var userId = UserIdExtractor.GetUserId(user);
        if (userId == null)
            return Results.Unauthorized();

        var result = await kibbeQuizService.ClearOverrideAsync(userId.Value, cancellationToken);
        return Results.Ok(ApiResponse<StyleProfileResponse>.Ok(result));
    }
}
