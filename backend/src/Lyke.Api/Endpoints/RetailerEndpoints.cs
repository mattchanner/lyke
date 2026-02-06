using System.Security.Claims;
using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Retailer;
using Lyke.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Lyke.Api.Endpoints;

public static class RetailerEndpoints
{
    public static IEndpointRouteBuilder MapRetailerEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/retailers/v1/portal")
            .WithTags("Retailer Portal")
            .RequireAuthorization("RetailerOnly");

        // Registration - any authenticated user can register
        group
            .MapPost("/register", RegisterAsRetailerAsync)
            .RequireAuthorization()
            .WithName("RegisterAsRetailer")
            .WithSummary("Register current user as a retailer")
            .Produces<ApiResponse<RetailerProfileResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized);

        // Profile
        group
            .MapGet("/profile", GetRetailerProfileAsync)
            .WithName("GetRetailerProfile")
            .WithSummary("Get current retailer's profile")
            .Produces<ApiResponse<RetailerProfileResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound);

        group
            .MapPut("/profile", UpdateRetailerProfileAsync)
            .WithName("UpdateRetailerProfile")
            .WithSummary("Update current retailer's profile")
            .Produces<ApiResponse<RetailerProfileResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound);

        // Products
        group
            .MapPost("/products/import", ImportProductsAsync)
            .WithName("ImportProducts")
            .WithSummary("Import products from CSV file")
            .Accepts<IFormFile>("multipart/form-data")
            .Produces<ApiResponse<ImportProductsResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .DisableAntiforgery();

        group
            .MapGet("/products", GetProductsAsync)
            .WithName("GetRetailerPortalProducts")
            .WithSummary("List retailer's products")
            .Produces<ApiResponse<IReadOnlyList<RetailerProductResponse>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound);

        group
            .MapPut("/products/{id:guid}", UpdateProductAsync)
            .WithName("UpdateRetailerProduct")
            .WithSummary("Update a product")
            .Produces<ApiResponse<RetailerProductResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound);

        // Campaigns
        group
            .MapPost("/campaigns", CreateCampaignAsync)
            .WithName("CreateCampaign")
            .WithSummary("Create a sponsored campaign")
            .Produces<ApiResponse<CampaignResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized);

        group
            .MapGet("/campaigns", GetCampaignsAsync)
            .WithName("GetCampaigns")
            .WithSummary("List campaigns")
            .Produces<ApiResponse<IReadOnlyList<CampaignResponse>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized);

        group
            .MapPut("/campaigns/{id:guid}", UpdateCampaignAsync)
            .WithName("UpdateCampaign")
            .WithSummary("Update a campaign")
            .Produces<ApiResponse<CampaignResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound);

        // Analytics
        group
            .MapGet("/analytics", GetAnalyticsAsync)
            .WithName("GetRetailerAnalytics")
            .WithSummary("Get engagement analytics")
            .Produces<ApiResponse<RetailerAnalyticsResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized);

        group
            .MapGet("/analytics/export", ExportAnalyticsCsvAsync)
            .WithName("ExportRetailerAnalytics")
            .WithSummary("Export analytics as CSV")
            .Produces(StatusCodes.Status200OK, contentType: "text/csv")
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized);

        // Insights
        group
            .MapGet("/insights/fit", GetFitInsightsAsync)
            .WithName("GetFitInsights")
            .WithSummary("Get fit feedback insights for products")
            .Produces<ApiResponse<FitInsightsResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized);

        group
            .MapGet("/insights/body-profiles", GetBodyProfileInsightsAsync)
            .WithName("GetBodyProfileInsights")
            .WithSummary("Get anonymized body profile insights for engaged users")
            .Produces<ApiResponse<BodyProfileInsightsResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized);

        return app;
    }

    private static async Task<IResult> RegisterAsRetailerAsync(
        [FromBody] RegisterRetailerRequest request,
        ClaimsPrincipal user,
        IRetailerService retailerService,
        CancellationToken cancellationToken
    )
    {
        var userId = GetUserId(user);
        if (userId == null)
            return Results.Unauthorized();

        var result = await retailerService.RegisterAsRetailerAsync(
            userId.Value,
            request,
            cancellationToken
        );
        return Results.Ok(ApiResponse<RetailerProfileResponse>.Ok(result));
    }

    private static async Task<IResult> GetRetailerProfileAsync(
        ClaimsPrincipal user,
        IRetailerService retailerService,
        CancellationToken cancellationToken
    )
    {
        var userId = GetUserId(user);
        if (userId == null)
            return Results.Unauthorized();

        var result = await retailerService.GetRetailerProfileAsync(userId.Value, cancellationToken);
        return Results.Ok(ApiResponse<RetailerProfileResponse>.Ok(result));
    }

    private static async Task<IResult> UpdateRetailerProfileAsync(
        [FromBody] UpdateRetailerProfileRequest request,
        ClaimsPrincipal user,
        IRetailerService retailerService,
        CancellationToken cancellationToken
    )
    {
        var userId = GetUserId(user);
        if (userId == null)
            return Results.Unauthorized();

        var result = await retailerService.UpdateRetailerProfileAsync(
            userId.Value,
            request,
            cancellationToken
        );
        return Results.Ok(ApiResponse<RetailerProfileResponse>.Ok(result));
    }

    private static async Task<IResult> ImportProductsAsync(
        IFormFile file,
        ClaimsPrincipal user,
        IRetailerService retailerService,
        CancellationToken cancellationToken
    )
    {
        var userId = GetUserId(user);
        if (userId == null)
            return Results.Unauthorized();

        if (file == null || file.Length == 0)
        {
            return Results.BadRequest(ApiResponse.Fail("INVALID_FILE", "CSV file is required"));
        }

        using var stream = file.OpenReadStream();
        var result = await retailerService.ImportProductsAsync(
            userId.Value,
            stream,
            cancellationToken
        );
        return Results.Ok(ApiResponse<ImportProductsResponse>.Ok(result));
    }

    private static async Task<IResult> GetProductsAsync(
        [AsParameters] RetailerProductsRequest request,
        ClaimsPrincipal user,
        IRetailerService retailerService,
        CancellationToken cancellationToken
    )
    {
        var userId = GetUserId(user);
        if (userId == null)
            return Results.Unauthorized();

        var (products, meta) = await retailerService.GetProductsAsync(
            userId.Value,
            request,
            cancellationToken
        );
        return Results.Ok(ApiResponse<IReadOnlyList<RetailerProductResponse>>.Ok(products, meta));
    }

    private static async Task<IResult> UpdateProductAsync(
        Guid id,
        [FromBody] UpdateRetailerProductRequest request,
        ClaimsPrincipal user,
        IRetailerService retailerService,
        CancellationToken cancellationToken
    )
    {
        var userId = GetUserId(user);
        if (userId == null)
            return Results.Unauthorized();

        var result = await retailerService.UpdateProductAsync(
            userId.Value,
            id,
            request,
            cancellationToken
        );
        return Results.Ok(ApiResponse<RetailerProductResponse>.Ok(result));
    }

    private static async Task<IResult> CreateCampaignAsync(
        [FromBody] CreateCampaignRequest request,
        ClaimsPrincipal user,
        IRetailerService retailerService,
        CancellationToken cancellationToken
    )
    {
        var userId = GetUserId(user);
        if (userId == null)
            return Results.Unauthorized();

        var result = await retailerService.CreateCampaignAsync(
            userId.Value,
            request,
            cancellationToken
        );
        return Results.Ok(ApiResponse<CampaignResponse>.Ok(result));
    }

    private static async Task<IResult> GetCampaignsAsync(
        [AsParameters] CampaignListRequest request,
        ClaimsPrincipal user,
        IRetailerService retailerService,
        CancellationToken cancellationToken
    )
    {
        var userId = GetUserId(user);
        if (userId == null)
            return Results.Unauthorized();

        var (campaigns, meta) = await retailerService.GetCampaignsAsync(
            userId.Value,
            request,
            cancellationToken
        );
        return Results.Ok(ApiResponse<IReadOnlyList<CampaignResponse>>.Ok(campaigns, meta));
    }

    private static async Task<IResult> UpdateCampaignAsync(
        Guid id,
        [FromBody] UpdateCampaignRequest request,
        ClaimsPrincipal user,
        IRetailerService retailerService,
        CancellationToken cancellationToken
    )
    {
        var userId = GetUserId(user);
        if (userId == null)
            return Results.Unauthorized();

        var result = await retailerService.UpdateCampaignAsync(
            userId.Value,
            id,
            request,
            cancellationToken
        );
        return Results.Ok(ApiResponse<CampaignResponse>.Ok(result));
    }

    private static async Task<IResult> GetAnalyticsAsync(
        [AsParameters] RetailerAnalyticsRequest request,
        ClaimsPrincipal user,
        IRetailerService retailerService,
        CancellationToken cancellationToken
    )
    {
        var userId = GetUserId(user);
        if (userId == null)
            return Results.Unauthorized();

        var result = await retailerService.GetAnalyticsAsync(
            userId.Value,
            request,
            cancellationToken
        );
        return Results.Ok(ApiResponse<RetailerAnalyticsResponse>.Ok(result));
    }

    private static async Task<IResult> ExportAnalyticsCsvAsync(
        [AsParameters] RetailerAnalyticsRequest request,
        ClaimsPrincipal user,
        IRetailerService retailerService,
        CancellationToken cancellationToken
    )
    {
        var userId = GetUserId(user);
        if (userId == null)
            return Results.Unauthorized();

        var csvBytes = await retailerService.ExportAnalyticsCsvAsync(
            userId.Value,
            request,
            cancellationToken
        );
        return Results.File(csvBytes, "text/csv", "analytics-export.csv");
    }

    private static async Task<IResult> GetFitInsightsAsync(
        ClaimsPrincipal user,
        IRetailerService retailerService,
        CancellationToken cancellationToken
    )
    {
        var userId = GetUserId(user);
        if (userId == null)
            return Results.Unauthorized();

        var result = await retailerService.GetFitInsightsAsync(userId.Value, cancellationToken);
        return Results.Ok(ApiResponse<FitInsightsResponse>.Ok(result));
    }

    private static async Task<IResult> GetBodyProfileInsightsAsync(
        ClaimsPrincipal user,
        IRetailerService retailerService,
        CancellationToken cancellationToken
    )
    {
        var userId = GetUserId(user);
        if (userId == null)
            return Results.Unauthorized();

        var result = await retailerService.GetBodyProfileInsightsAsync(
            userId.Value,
            cancellationToken
        );
        return Results.Ok(ApiResponse<BodyProfileInsightsResponse>.Ok(result));
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
