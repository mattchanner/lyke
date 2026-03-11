using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Profile;

namespace Lyke.Api.Endpoints.Profile;

public static class ProfileEndpointRoutes
{
    public static IEndpointRouteBuilder MapProfileEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/profile/v1").WithTags("Profile").RequireAuthorization();

        group
            .MapGet("/me", GetProfile.Handle)
            .WithName("GetProfile")
            .WithSummary("Get current user's profile")
            .Produces<ApiResponse<UserProfileResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized);

        group
            .MapPut("/", UpdateProfile.Handle)
            .WithName("UpdateProfile")
            .WithSummary("Update current user's profile")
            .Produces<ApiResponse<UserProfileResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized);

        group
            .MapGet("/body", GetBodyProfile.Handle)
            .WithName("GetBodyProfile")
            .WithSummary("Get current user's body profile")
            .Produces<ApiResponse<BodyProfileResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized);

        group
            .MapPost("/body", CreateBodyProfile.Handle)
            .WithName("CreateBodyProfile")
            .WithSummary("Create body profile for current user")
            .Produces<ApiResponse<BodyProfileResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized);

        group
            .MapPut("/body", UpdateBodyProfile.Handle)
            .WithName("UpdateBodyProfile")
            .WithSummary("Update current user's body profile")
            .Produces<ApiResponse<BodyProfileResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized);

        group
            .MapDelete("/body", DeleteBodyProfile.Handle)
            .WithName("DeleteBodyProfile")
            .WithSummary("Delete current user's body profile")
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized);

        group
            .MapPost("/me/image", UploadProfileImage.Handle)
            .WithName("UploadProfileImage")
            .WithSummary("Upload profile image")
            .DisableAntiforgery()
            .Accepts<IFormFile>("multipart/form-data")
            .Produces<ApiResponse<UserProfileResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized);

        group
            .MapDelete("/me/image", DeleteProfileImage.Handle)
            .WithName("DeleteProfileImage")
            .WithSummary("Delete profile image")
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized);

        return app;
    }
}
