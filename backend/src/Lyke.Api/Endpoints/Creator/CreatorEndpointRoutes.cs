using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Creator;

namespace Lyke.Api.Endpoints.Creator;

public static class CreatorEndpointRoutes
{
    public static IEndpointRouteBuilder MapCreatorEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/creators/v1").WithTags("Creators");

        // Public creator profile (any authenticated user)
        group
            .MapGet("/{creatorId:guid}/public", GetPublicCreatorProfile.Handle)
            .WithName("GetPublicCreatorProfile")
            .WithSummary("Get a creator's public profile")
            .Produces<ApiResponse<PublicCreatorProfileResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound)
            .RequireAuthorization();

        // Registration & Profile
        // Note: Registration allows any authenticated user (Shopper becoming Creator)
        group
            .MapPost("/register", RegisterAsCreator.Handle)
            .RequireAuthorization() // Override: any authenticated user can register
            .WithName("RegisterAsCreator")
            .WithSummary("Register current user as a creator")
            .Produces<ApiResponse<CreatorProfileResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized);

        group
            .MapGet("/profile", GetCreatorProfile.Handle)
            .WithName("GetCreatorProfile")
            .WithSummary("Get current creator's profile")
            .RequireAuthorization("CreatorOnly")
            .Produces<ApiResponse<CreatorProfileResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound);

        group
            .MapPut("/profile", UpdateCreatorProfile.Handle)
            .WithName("UpdateCreatorProfile")
            .WithSummary("Update current creator's profile")
            .RequireAuthorization("CreatorOnly")
            .Produces<ApiResponse<CreatorProfileResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound);

        // Post Management
        group
            .MapPost("/posts", CreatePost.Handle)
            .WithName("CreateCreatorPost")
            .WithSummary("Create a new post (draft)")
            .RequireAuthorization("CreatorOnly")
            .Produces<ApiResponse<CreatorPostResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound);

        group
            .MapGet("/posts", GetCreatorPosts.Handle)
            .WithName("GetCreatorPosts")
            .WithSummary("Get creator's posts with optional status filter")
            .RequireAuthorization("CreatorOnly")
            .Produces<ApiResponse<IReadOnlyList<CreatorPostResponse>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound);

        group
            .MapGet("/posts/{id:guid}", GetCreatorPost.Handle)
            .WithName("GetCreatorPost")
            .WithSummary("Get a specific post by ID")
            .RequireAuthorization("CreatorOnly")
            .Produces<ApiResponse<CreatorPostResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound);

        group
            .MapPut("/posts/{id:guid}", UpdatePost.Handle)
            .WithName("UpdateCreatorPost")
            .WithSummary("Update a draft post")
            .RequireAuthorization("CreatorOnly")
            .Produces<ApiResponse<CreatorPostResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound);

        group
            .MapDelete("/posts/{id:guid}", DeletePost.Handle)
            .WithName("DeleteCreatorPost")
            .WithSummary("Delete a post")
            .RequireAuthorization("CreatorOnly")
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound);

        group
            .MapPost("/posts/{id:guid}/submit", SubmitPostForReview.Handle)
            .WithName("SubmitPostForReview")
            .WithSummary("Submit a draft post for review")
            .RequireAuthorization("CreatorOnly")
            .Produces<ApiResponse<CreatorPostResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound);

        // Analytics & Earnings
        group
            .MapGet("/analytics", GetAnalytics.Handle)
            .WithName("GetCreatorAnalytics")
            .WithSummary("Get creator performance metrics")
            .RequireAuthorization("CreatorOnly")
            .Produces<ApiResponse<CreatorAnalyticsResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound);

        group
            .MapGet("/earnings", GetEarningsSummary.Handle)
            .WithName("GetCreatorEarningsSummary")
            .WithSummary("Get earnings summary")
            .RequireAuthorization("CreatorOnly")
            .Produces<ApiResponse<EarningsSummaryResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound);

        group
            .MapGet("/earnings/history", GetEarningsHistory.Handle)
            .WithName("GetCreatorEarningsHistory")
            .WithSummary("Get detailed earnings history")
            .RequireAuthorization("CreatorOnly")
            .Produces<ApiResponse<IReadOnlyList<EarningDetailResponse>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound);

        // Verification
        group
            .MapGet("/verification", GetVerificationStatus.Handle)
            .WithName("GetCreatorVerificationStatus")
            .WithSummary("Get current verification status")
            .RequireAuthorization("CreatorOnly")
            .Produces<ApiResponse<VerificationStatusResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound);

        group
            .MapPost("/verification", SubmitVerification.Handle)
            .WithName("SubmitCreatorVerification")
            .WithSummary("Submit verification request")
            .RequireAuthorization("CreatorOnly")
            .Produces<ApiResponse<VerificationStatusResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound);

        return app;
    }
}
