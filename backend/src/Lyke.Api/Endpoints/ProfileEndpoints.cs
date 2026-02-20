using System.Security.Claims;
using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Feed;
using Lyke.Application.DTOs.Profile;
using Lyke.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Lyke.Api.Endpoints;

public static class ProfileEndpoints
{
    public static IEndpointRouteBuilder MapProfileEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/profile/v1").WithTags("Profile").RequireAuthorization();

        group
            .MapGet("/me", GetProfileAsync)
            .WithName("GetProfile")
            .WithSummary("Get current user's profile")
            .Produces<ApiResponse<UserProfileResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized);

        group
            .MapPut("/", UpdateProfileAsync)
            .WithName("UpdateProfile")
            .WithSummary("Update current user's profile")
            .Produces<ApiResponse<UserProfileResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized);

        group
            .MapGet("/body", GetBodyProfileAsync)
            .WithName("GetBodyProfile")
            .WithSummary("Get current user's body profile")
            .Produces<ApiResponse<BodyProfileResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized);

        group
            .MapPost("/body", CreateBodyProfileAsync)
            .WithName("CreateBodyProfile")
            .WithSummary("Create body profile for current user")
            .Produces<ApiResponse<BodyProfileResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized);

        group
            .MapPut("/body", UpdateBodyProfileAsync)
            .WithName("UpdateBodyProfile")
            .WithSummary("Update current user's body profile")
            .Produces<ApiResponse<BodyProfileResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized);

        group
            .MapDelete("/body", DeleteBodyProfileAsync)
            .WithName("DeleteBodyProfile")
            .WithSummary("Delete current user's body profile")
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized);

        group
            .MapPost("/me/image", UploadProfileImageAsync)
            .WithName("UploadProfileImage")
            .WithSummary("Upload profile image")
            .DisableAntiforgery()
            .Accepts<IFormFile>("multipart/form-data")
            .Produces<ApiResponse<UserProfileResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized);

        group
            .MapDelete("/me/image", DeleteProfileImageAsync)
            .WithName("DeleteProfileImage")
            .WithSummary("Delete profile image")
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized);

        return app;
    }

    public static IEndpointRouteBuilder MapLookupEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/lookup/v1").WithTags("Lookups");

        group
            .MapGet("/body-types", GetBodyTypesAsync)
            .WithName("GetBodyTypes")
            .WithSummary("Get available body types")
            .Produces<ApiResponse<IReadOnlyList<BodyTypeResponse>>>(StatusCodes.Status200OK)
            .AllowAnonymous();

        group
            .MapGet("/fit-preferences", GetFitPreferencesAsync)
            .WithName("GetFitPreferences")
            .WithSummary("Get available fit preferences")
            .Produces<ApiResponse<IReadOnlyList<FitPreferenceResponse>>>(StatusCodes.Status200OK)
            .AllowAnonymous();

        group
            .MapGet("/fit-tags", GetFitTagsAsync)
            .WithName("GetFitTags")
            .WithSummary("Get available fit tags")
            .Produces<ApiResponse<IReadOnlyList<FitTagResponse>>>(StatusCodes.Status200OK)
            .AllowAnonymous();

        return app;
    }

    private static async Task<IResult> GetProfileAsync(
        ClaimsPrincipal user,
        IProfileService profileService,
        CancellationToken cancellationToken
    )
    {
        var userId = GetUserId(user);
        if (userId == null)
            return Results.Unauthorized();

        var result = await profileService.GetProfileAsync(userId.Value, cancellationToken);
        return Results.Ok(ApiResponse<UserProfileResponse>.Ok(result));
    }

    private static async Task<IResult> UpdateProfileAsync(
        [FromBody] UpdateProfileRequest request,
        ClaimsPrincipal user,
        IProfileService profileService,
        CancellationToken cancellationToken
    )
    {
        var userId = GetUserId(user);
        if (userId == null)
            return Results.Unauthorized();

        var result = await profileService.UpdateProfileAsync(
            userId.Value,
            request,
            cancellationToken
        );
        return Results.Ok(ApiResponse<UserProfileResponse>.Ok(result));
    }

    private static async Task<IResult> GetBodyProfileAsync(
        ClaimsPrincipal user,
        IProfileService profileService,
        CancellationToken cancellationToken
    )
    {
        var userId = GetUserId(user);
        if (userId == null)
            return Results.Unauthorized();

        var result = await profileService.GetBodyProfileAsync(userId.Value, cancellationToken);
        if (result == null)
        {
            return Results.NotFound(ApiResponse.Fail("NOT_FOUND", "Body profile not found"));
        }

        return Results.Ok(ApiResponse<BodyProfileResponse>.Ok(result));
    }

    private static async Task<IResult> CreateBodyProfileAsync(
        [FromBody] CreateBodyProfileRequest request,
        ClaimsPrincipal user,
        IProfileService profileService,
        CancellationToken cancellationToken
    )
    {
        var userId = GetUserId(user);
        if (userId == null)
            return Results.Unauthorized();

        var result = await profileService.CreateBodyProfileAsync(
            userId.Value,
            request,
            cancellationToken
        );
        return Results.Ok(ApiResponse<BodyProfileResponse>.Ok(result));
    }

    private static async Task<IResult> UpdateBodyProfileAsync(
        [FromBody] UpdateBodyProfileRequest request,
        ClaimsPrincipal user,
        IProfileService profileService,
        CancellationToken cancellationToken
    )
    {
        var userId = GetUserId(user);
        if (userId == null)
            return Results.Unauthorized();

        var result = await profileService.UpdateBodyProfileAsync(
            userId.Value,
            request,
            cancellationToken
        );
        return Results.Ok(ApiResponse<BodyProfileResponse>.Ok(result));
    }

    private static async Task<IResult> DeleteBodyProfileAsync(
        ClaimsPrincipal user,
        IProfileService profileService,
        CancellationToken cancellationToken
    )
    {
        var userId = GetUserId(user);
        if (userId == null)
            return Results.Unauthorized();

        await profileService.DeleteBodyProfileAsync(userId.Value, cancellationToken);
        return Results.Ok(ApiResponse.Ok());
    }

    private static async Task<IResult> GetBodyTypesAsync(
        IProfileService profileService,
        CancellationToken cancellationToken
    )
    {
        var result = await profileService.GetBodyTypesAsync(cancellationToken);
        return Results.Ok(ApiResponse<IReadOnlyList<BodyTypeResponse>>.Ok(result));
    }

    private static async Task<IResult> GetFitPreferencesAsync(
        IProfileService profileService,
        CancellationToken cancellationToken
    )
    {
        var result = await profileService.GetFitPreferencesAsync(cancellationToken);
        return Results.Ok(ApiResponse<IReadOnlyList<FitPreferenceResponse>>.Ok(result));
    }

    private static async Task<IResult> GetFitTagsAsync(
        IProfileService profileService,
        CancellationToken cancellationToken
    )
    {
        var result = await profileService.GetFitTagsAsync(cancellationToken);
        return Results.Ok(ApiResponse<IReadOnlyList<FitTagResponse>>.Ok(result));
    }

    private static readonly HashSet<string> AllowedImageTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/webp"
    };

    private const long MaxImageSize = 5 * 1024 * 1024; // 5MB

    private static async Task<IResult> UploadProfileImageAsync(
        [FromForm] IFormFile file,
        ClaimsPrincipal user,
        IProfileService profileService,
        CancellationToken cancellationToken
    )
    {
        var userId = GetUserId(user);
        if (userId == null)
            return Results.Unauthorized();

        if (file == null || file.Length == 0)
            return Results.BadRequest(ApiResponse.Fail("VALIDATION_ERROR", "No file provided"));

        if (!AllowedImageTypes.Contains(file.ContentType))
            return Results.BadRequest(ApiResponse.Fail("VALIDATION_ERROR", "Only JPEG, PNG, and WebP images are allowed"));

        if (file.Length > MaxImageSize)
            return Results.BadRequest(ApiResponse.Fail("VALIDATION_ERROR", "Image must be 5MB or smaller"));

        using var stream = file.OpenReadStream();
        var result = await profileService.UploadProfileImageAsync(userId.Value, stream, file.ContentType, cancellationToken);
        return Results.Ok(ApiResponse<UserProfileResponse>.Ok(result));
    }

    private static async Task<IResult> DeleteProfileImageAsync(
        ClaimsPrincipal user,
        IProfileService profileService,
        CancellationToken cancellationToken
    )
    {
        var userId = GetUserId(user);
        if (userId == null)
            return Results.Unauthorized();

        await profileService.DeleteProfileImageAsync(userId.Value, cancellationToken);
        return Results.Ok(ApiResponse.Ok());
    }

    private static Guid? GetUserId(ClaimsPrincipal user)
    {
        var userIdClaim =
            user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? user.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return null;
        }

        return userId;
    }
}
