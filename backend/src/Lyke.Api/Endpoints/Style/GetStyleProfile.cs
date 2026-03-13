using System.Security.Claims;
using Lyke.Api.Endpoints.Shared;
using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Style;
using Lyke.Application.Interfaces;

namespace Lyke.Api.Endpoints.Style;

public static class GetStyleProfile
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

        StyleProfileResponse? result = 
            await kibbeQuizService.GetProfileAsync(userId.Value, cancellationToken);

        if (result == null)
            return Results.NotFound(
                ApiResponse.Fail("STYLE_PROFILE_NOT_FOUND", "No style profile found")
            );

        return Results.Ok(ApiResponse<StyleProfileResponse>.Ok(result));
    }
}
