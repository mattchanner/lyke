using System.Security.Claims;
using Lyke.Api.Endpoints.Shared;
using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Style;
using Lyke.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Lyke.Api.Endpoints.Style;

public static class OverrideStyleProfile
{
    public static async Task<IResult> Handle(
        [FromBody] OverrideStyleProfileRequest request,
        ClaimsPrincipal user,
        IKibbeQuizService kibbeQuizService,
        CancellationToken cancellationToken
    )
    {
        var userId = UserIdExtractor.GetUserId(user);
        if (userId == null)
            return Results.Unauthorized();

        var result = await kibbeQuizService.OverrideAsync(
            userId.Value,
            request.Family,
            cancellationToken
        );
        return Results.Ok(ApiResponse<StyleProfileResponse>.Ok(result));
    }
}
