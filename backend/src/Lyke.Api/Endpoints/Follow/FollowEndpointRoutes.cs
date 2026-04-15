using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Follow;

namespace Lyke.Api.Endpoints.Follow;

public static class FollowEndpointRoutes
{
    public static IEndpointRouteBuilder MapFollowEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/creators/v1").WithTags("Follow").RequireAuthorization();

        group
            .MapPost("/{creatorId:guid}/follow", FollowCreator.Handle)
            .WithName("FollowCreator")
            .WithSummary("Follow a creator")
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound);

        group
            .MapDelete("/{creatorId:guid}/follow", UnfollowCreator.Handle)
            .WithName("UnfollowCreator")
            .WithSummary("Unfollow a creator")
            .Produces<ApiResponse>(StatusCodes.Status200OK);

        group
            .MapGet("/{creatorId:guid}/follow-status", GetFollowStatus.Handle)
            .WithName("GetFollowStatus")
            .WithSummary("Check if current user follows a creator")
            .Produces<ApiResponse<bool>>(StatusCodes.Status200OK);

        var userGroup = app.MapGroup("/api/users/v1").WithTags("Follow").RequireAuthorization();

        userGroup
            .MapGet("/following", GetFollowing.Handle)
            .WithName("GetFollowing")
            .WithSummary("Get list of creators the current user follows")
            .Produces<ApiResponse<IReadOnlyList<FollowedCreatorResponse>>>(StatusCodes.Status200OK);

        userGroup
            .MapPost("/{userId:guid}/follow", FollowUser.Handle)
            .WithName("FollowUser")
            .WithSummary("Follow any user")
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound);

        userGroup
            .MapDelete("/{userId:guid}/follow", UnfollowUser.Handle)
            .WithName("UnfollowUser")
            .WithSummary("Unfollow a user")
            .Produces<ApiResponse>(StatusCodes.Status200OK);

        userGroup
            .MapGet("/{userId:guid}/follow-status", GetUserFollowStatus.Handle)
            .WithName("GetUserFollowStatus")
            .WithSummary("Check if current user follows a user")
            .Produces<ApiResponse<bool>>(StatusCodes.Status200OK);

        return app;
    }
}
