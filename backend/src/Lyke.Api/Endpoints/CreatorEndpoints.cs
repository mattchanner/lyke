using System.Security.Claims;
using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Creator;
using Lyke.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Lyke.Api.Endpoints;

public static class CreatorEndpoints
{
    public static IEndpointRouteBuilder MapCreatorEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/creators")
            .WithTags("Creators")
            .RequireAuthorization("CreatorOnly");

        // Registration & Profile
        // Note: Registration allows any authenticated user (Shopper becoming Creator)
        group.MapPost("/register", RegisterAsCreatorAsync)
            .RequireAuthorization() // Override: any authenticated user can register
            .WithName("RegisterAsCreator")
            .WithSummary("Register current user as a creator")
            .Produces<ApiResponse<CreatorProfileResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized);

        group.MapGet("/profile", GetCreatorProfileAsync)
            .WithName("GetCreatorProfile")
            .WithSummary("Get current creator's profile")
            .Produces<ApiResponse<CreatorProfileResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound);

        group.MapPut("/profile", UpdateCreatorProfileAsync)
            .WithName("UpdateCreatorProfile")
            .WithSummary("Update current creator's profile")
            .Produces<ApiResponse<CreatorProfileResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound);

        // Post Management
        group.MapPost("/posts", CreatePostAsync)
            .WithName("CreateCreatorPost")
            .WithSummary("Create a new post (draft)")
            .Produces<ApiResponse<CreatorPostResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound);

        group.MapGet("/posts", GetCreatorPostsAsync)
            .WithName("GetCreatorPosts")
            .WithSummary("Get creator's posts with optional status filter")
            .Produces<ApiResponse<IReadOnlyList<CreatorPostResponse>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound);

        group.MapGet("/posts/{id:guid}", GetCreatorPostAsync)
            .WithName("GetCreatorPost")
            .WithSummary("Get a specific post by ID")
            .Produces<ApiResponse<CreatorPostResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound);

        group.MapPut("/posts/{id:guid}", UpdatePostAsync)
            .WithName("UpdateCreatorPost")
            .WithSummary("Update a draft post")
            .Produces<ApiResponse<CreatorPostResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound);

        group.MapDelete("/posts/{id:guid}", DeletePostAsync)
            .WithName("DeleteCreatorPost")
            .WithSummary("Delete a post")
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound);

        group.MapPost("/posts/{id:guid}/submit", SubmitPostForReviewAsync)
            .WithName("SubmitPostForReview")
            .WithSummary("Submit a draft post for review")
            .Produces<ApiResponse<CreatorPostResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound);

        // Analytics & Earnings
        group.MapGet("/analytics", GetAnalyticsAsync)
            .WithName("GetCreatorAnalytics")
            .WithSummary("Get creator performance metrics")
            .Produces<ApiResponse<CreatorAnalyticsResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound);

        group.MapGet("/earnings", GetEarningsSummaryAsync)
            .WithName("GetCreatorEarningsSummary")
            .WithSummary("Get earnings summary")
            .Produces<ApiResponse<EarningsSummaryResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound);

        group.MapGet("/earnings/history", GetEarningsHistoryAsync)
            .WithName("GetCreatorEarningsHistory")
            .WithSummary("Get detailed earnings history")
            .Produces<ApiResponse<IReadOnlyList<EarningDetailResponse>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound);

        // Verification
        group.MapGet("/verification", GetVerificationStatusAsync)
            .WithName("GetCreatorVerificationStatus")
            .WithSummary("Get current verification status")
            .Produces<ApiResponse<VerificationStatusResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound);

        group.MapPost("/verification", SubmitVerificationAsync)
            .WithName("SubmitCreatorVerification")
            .WithSummary("Submit verification request")
            .Produces<ApiResponse<VerificationStatusResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<IResult> RegisterAsCreatorAsync(
        [FromBody] RegisterCreatorRequest request,
        ClaimsPrincipal user,
        ICreatorService creatorService,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(user);
        if (userId == null) return Results.Unauthorized();

        var result = await creatorService.RegisterAsCreatorAsync(userId.Value, request, cancellationToken);
        return Results.Ok(ApiResponse<CreatorProfileResponse>.Ok(result));
    }

    private static async Task<IResult> GetCreatorProfileAsync(
        ClaimsPrincipal user,
        ICreatorService creatorService,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(user);
        if (userId == null) return Results.Unauthorized();

        var result = await creatorService.GetCreatorProfileAsync(userId.Value, cancellationToken);
        return Results.Ok(ApiResponse<CreatorProfileResponse>.Ok(result));
    }

    private static async Task<IResult> UpdateCreatorProfileAsync(
        [FromBody] UpdateCreatorProfileRequest request,
        ClaimsPrincipal user,
        ICreatorService creatorService,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(user);
        if (userId == null) return Results.Unauthorized();

        var result = await creatorService.UpdateCreatorProfileAsync(userId.Value, request, cancellationToken);
        return Results.Ok(ApiResponse<CreatorProfileResponse>.Ok(result));
    }

    private static async Task<IResult> CreatePostAsync(
        [FromBody] CreatePostRequest request,
        ClaimsPrincipal user,
        ICreatorService creatorService,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(user);
        if (userId == null) return Results.Unauthorized();

        var result = await creatorService.CreatePostAsync(userId.Value, request, cancellationToken);
        return Results.Ok(ApiResponse<CreatorPostResponse>.Ok(result));
    }

    private static async Task<IResult> GetCreatorPostsAsync(
        [AsParameters] CreatorPostsRequest request,
        ClaimsPrincipal user,
        ICreatorService creatorService,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(user);
        if (userId == null) return Results.Unauthorized();

        var (posts, meta) = await creatorService.GetPostsAsync(userId.Value, request, cancellationToken);
        return Results.Ok(ApiResponse<IReadOnlyList<CreatorPostResponse>>.Ok(posts, meta));
    }

    private static async Task<IResult> GetCreatorPostAsync(
        Guid id,
        ClaimsPrincipal user,
        ICreatorService creatorService,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(user);
        if (userId == null) return Results.Unauthorized();

        var result = await creatorService.GetPostAsync(userId.Value, id, cancellationToken);
        return Results.Ok(ApiResponse<CreatorPostResponse>.Ok(result));
    }

    private static async Task<IResult> UpdatePostAsync(
        Guid id,
        [FromBody] UpdatePostRequest request,
        ClaimsPrincipal user,
        ICreatorService creatorService,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(user);
        if (userId == null) return Results.Unauthorized();

        var result = await creatorService.UpdatePostAsync(userId.Value, id, request, cancellationToken);
        return Results.Ok(ApiResponse<CreatorPostResponse>.Ok(result));
    }

    private static async Task<IResult> DeletePostAsync(
        Guid id,
        ClaimsPrincipal user,
        ICreatorService creatorService,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(user);
        if (userId == null) return Results.Unauthorized();

        await creatorService.DeletePostAsync(userId.Value, id, cancellationToken);
        return Results.Ok(ApiResponse.Ok());
    }

    private static async Task<IResult> SubmitPostForReviewAsync(
        Guid id,
        ClaimsPrincipal user,
        ICreatorService creatorService,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(user);
        if (userId == null) return Results.Unauthorized();

        var result = await creatorService.SubmitPostForReviewAsync(userId.Value, id, cancellationToken);
        return Results.Ok(ApiResponse<CreatorPostResponse>.Ok(result));
    }

    private static async Task<IResult> GetAnalyticsAsync(
        [AsParameters] CreatorAnalyticsRequest request,
        ClaimsPrincipal user,
        ICreatorService creatorService,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(user);
        if (userId == null) return Results.Unauthorized();

        var result = await creatorService.GetAnalyticsAsync(userId.Value, request, cancellationToken);
        return Results.Ok(ApiResponse<CreatorAnalyticsResponse>.Ok(result));
    }

    private static async Task<IResult> GetEarningsSummaryAsync(
        ClaimsPrincipal user,
        ICreatorService creatorService,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(user);
        if (userId == null) return Results.Unauthorized();

        var result = await creatorService.GetEarningsSummaryAsync(userId.Value, cancellationToken);
        return Results.Ok(ApiResponse<EarningsSummaryResponse>.Ok(result));
    }

    private static async Task<IResult> GetEarningsHistoryAsync(
        [AsParameters] EarningsHistoryRequest request,
        ClaimsPrincipal user,
        ICreatorService creatorService,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(user);
        if (userId == null) return Results.Unauthorized();

        var (earnings, meta) = await creatorService.GetEarningsHistoryAsync(userId.Value, request, cancellationToken);
        return Results.Ok(ApiResponse<IReadOnlyList<EarningDetailResponse>>.Ok(earnings, meta));
    }

    private static async Task<IResult> GetVerificationStatusAsync(
        ClaimsPrincipal user,
        ICreatorService creatorService,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(user);
        if (userId == null) return Results.Unauthorized();

        var result = await creatorService.GetVerificationStatusAsync(userId.Value, cancellationToken);
        return Results.Ok(ApiResponse<VerificationStatusResponse>.Ok(result));
    }

    private static async Task<IResult> SubmitVerificationAsync(
        [FromBody] SubmitVerificationRequest request,
        ClaimsPrincipal user,
        ICreatorService creatorService,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(user);
        if (userId == null) return Results.Unauthorized();

        var result = await creatorService.SubmitVerificationAsync(userId.Value, request, cancellationToken);
        return Results.Ok(ApiResponse<VerificationStatusResponse>.Ok(result));
    }

    private static Guid? GetUserId(ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? user.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return null;
        }

        return userId;
    }
}
