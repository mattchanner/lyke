using System.Security.Claims;
using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Follow;
using Lyke.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Lyke.Api.Endpoints;

public static class FollowEndpoints
{
    public static IEndpointRouteBuilder MapFollowEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/creators/v1")
            .WithTags("Follow")
            .RequireAuthorization();

        group
            .MapPost("/{creatorId:guid}/follow", FollowCreatorAsync)
            .WithName("FollowCreator")
            .WithSummary("Follow a creator")
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound);

        group
            .MapDelete("/{creatorId:guid}/follow", UnfollowCreatorAsync)
            .WithName("UnfollowCreator")
            .WithSummary("Unfollow a creator")
            .Produces<ApiResponse>(StatusCodes.Status200OK);

        group
            .MapGet("/{creatorId:guid}/follow-status", GetFollowStatusAsync)
            .WithName("GetFollowStatus")
            .WithSummary("Check if current user follows a creator")
            .Produces<ApiResponse<bool>>(StatusCodes.Status200OK);

        var userGroup = app.MapGroup("/api/users/v1")
            .WithTags("Follow")
            .RequireAuthorization();

        userGroup
            .MapGet("/following", GetFollowingAsync)
            .WithName("GetFollowing")
            .WithSummary("Get list of creators the current user follows")
            .Produces<ApiResponse<IReadOnlyList<FollowedCreatorResponse>>>(StatusCodes.Status200OK);

        return app;
    }

    private static async Task<IResult> FollowCreatorAsync(
        Guid creatorId,
        ClaimsPrincipal user,
        IFollowService followService,
        CancellationToken cancellationToken
    )
    {
        var userId = GetUserId(user);
        if (userId == null)
            return Results.Unauthorized();

        await followService.FollowAsync(userId.Value, creatorId, cancellationToken);
        return Results.Ok(ApiResponse.Ok());
    }

    private static async Task<IResult> UnfollowCreatorAsync(
        Guid creatorId,
        ClaimsPrincipal user,
        IFollowService followService,
        CancellationToken cancellationToken
    )
    {
        var userId = GetUserId(user);
        if (userId == null)
            return Results.Unauthorized();

        await followService.UnfollowAsync(userId.Value, creatorId, cancellationToken);
        return Results.Ok(ApiResponse.Ok());
    }

    private static async Task<IResult> GetFollowStatusAsync(
        Guid creatorId,
        ClaimsPrincipal user,
        IFollowService followService,
        CancellationToken cancellationToken
    )
    {
        var userId = GetUserId(user);
        if (userId == null)
            return Results.Unauthorized();

        var isFollowing = await followService.IsFollowingAsync(userId.Value, creatorId, cancellationToken);
        return Results.Ok(ApiResponse<bool>.Ok(isFollowing));
    }

    private static async Task<IResult> GetFollowingAsync(
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        ClaimsPrincipal user,
        IFollowService followService,
        CancellationToken cancellationToken
    )
    {
        var userId = GetUserId(user);
        if (userId == null)
            return Results.Unauthorized();

        var (creators, meta) = await followService.GetFollowingAsync(
            userId.Value,
            page ?? 1,
            pageSize ?? 20,
            cancellationToken
        );

        return Results.Ok(ApiResponse<IReadOnlyList<FollowedCreatorResponse>>.Ok(creators, meta));
    }

    private static Guid? GetUserId(ClaimsPrincipal user)
    {
        var userIdClaim =
            user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? user.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return null;

        return userId;
    }
}
