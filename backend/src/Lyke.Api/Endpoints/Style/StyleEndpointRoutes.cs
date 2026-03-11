using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Style;

namespace Lyke.Api.Endpoints.Style;

public static class StyleEndpointRoutes
{
    public static IEndpointRouteBuilder MapStyleEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/style/v1").WithTags("Style");

        // ── PUBLIC ────────────────────────────────────────────────────────────────
        // Score the quiz (anonymous)
        group
            .MapPost("/quiz/kibbe/score", ScoreKibbeQuiz.Handle)
            .WithName("ScoreKibbeQuiz")
            .WithSummary("Score a completed Kibbe quiz")
            .AllowAnonymous()
            .Produces<ApiResponse<KibbeScoreResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest);

        // ── AUTHENTICATED ─────────────────────────────────────────────────────────
        // Get saved style profile
        group
            .MapGet("/profile", GetStyleProfile.Handle)
            .WithName("GetStyleProfile")
            .WithSummary("Get the user's saved style profile")
            .RequireAuthorization()
            .Produces<ApiResponse<StyleProfileResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized);

        // Save computed result to profile
        group
            .MapPut("/profile", SaveStyleProfile.Handle)
            .WithName("SaveStyleProfile")
            .WithSummary("Save quiz result to user's style profile")
            .RequireAuthorization()
            .Produces<ApiResponse<StyleProfileResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized);

        // Override family (self-selected)
        group
            .MapPut("/profile/override", OverrideStyleProfile.Handle)
            .WithName("OverrideStyleProfile")
            .WithSummary("Override the computed style profile with a self-selected family")
            .RequireAuthorization()
            .Produces<ApiResponse<StyleProfileResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized);

        // Clear override
        group
            .MapDelete("/profile/override", ClearStyleProfileOverride.Handle)
            .WithName("ClearStyleProfileOverride")
            .WithSummary("Clear the override and revert to computed result")
            .RequireAuthorization()
            .Produces<ApiResponse<StyleProfileResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound);

        return app;
    }
}
