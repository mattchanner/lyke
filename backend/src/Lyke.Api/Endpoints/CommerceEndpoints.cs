using System.Security.Claims;
using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Commerce;
using Lyke.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Lyke.Api.Endpoints;

public static class CommerceEndpoints
{
    public static IEndpointRouteBuilder MapCommerceEndpoints(this IEndpointRouteBuilder app)
    {
        MapClickEndpoints(app);
        MapProductEndpoints(app);
        MapRetailerEndpoints(app);

        return app;
    }

    private static void MapClickEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/commerce/v1/clicks").WithTags("Click Tracking");

        group
            .MapPost("/track", TrackClickAsync)
            .WithName("TrackClick")
            .WithSummary("Track outbound click and get affiliate URL")
            .Produces<ApiResponse<TrackClickResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound)
            .AllowAnonymous();
    }

    private static void MapProductEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/commerce/v1/products").WithTags("Products");

        group
            .MapGet("/{id:guid}", GetProductAsync)
            .WithName("GetProduct")
            .WithSummary("Get product details by ID")
            .Produces<ApiResponse<ProductResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound)
            .AllowAnonymous();

        group
            .MapGet("/search", SearchProductsAsync)
            .WithName("SearchProducts")
            .WithSummary("Search products (for creators tagging posts)")
            .Produces<ApiResponse<IReadOnlyList<ProductResponse>>>(StatusCodes.Status200OK)
            .RequireAuthorization("CreatorOrAdmin");
    }

    private static void MapRetailerEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/commerce/v1/retailers").WithTags("Retailers");

        group
            .MapGet("/", GetRetailersAsync)
            .WithName("GetRetailers")
            .WithSummary("Get all active retailers")
            .Produces<ApiResponse<IReadOnlyList<RetailerResponse>>>(StatusCodes.Status200OK)
            .AllowAnonymous();

        group
            .MapGet("/{id:guid}/products", GetRetailerProductsAsync)
            .WithName("GetRetailerProducts")
            .WithSummary("Get retailer products (paginated)")
            .Produces<ApiResponse<RetailerProductsResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound)
            .AllowAnonymous();

        group
            .MapPost("/{id:guid}/conversions", ProcessConversionAsync)
            .WithName("ProcessConversion")
            .WithSummary("Webhook for retailer to report conversions")
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .AllowAnonymous();
    }

    private static async Task<IResult> TrackClickAsync(
        [FromBody] TrackClickRequest request,
        ClaimsPrincipal user,
        ICommerceService commerceService,
        HttpContext httpContext,
        CancellationToken cancellationToken
    )
    {
        var userId = GetUserId(user);
        var userAgent = httpContext.Request.Headers.UserAgent.ToString();
        var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();

        var result = await commerceService.TrackClickAsync(
            userId,
            request,
            userAgent,
            ipAddress,
            cancellationToken
        );

        return Results.Ok(ApiResponse<TrackClickResponse>.Ok(result));
    }

    private static async Task<IResult> GetProductAsync(
        Guid id,
        ICommerceService commerceService,
        CancellationToken cancellationToken
    )
    {
        var product = await commerceService.GetProductAsync(id, cancellationToken);
        return Results.Ok(ApiResponse<ProductResponse>.Ok(product));
    }

    private static async Task<IResult> SearchProductsAsync(
        [AsParameters] ProductSearchQueryParams queryParams,
        ICommerceService commerceService,
        CancellationToken cancellationToken
    )
    {
        var request = new ProductSearchRequest(
            queryParams.Q ?? "",
            queryParams.RetailerId,
            queryParams.Category,
            queryParams.Page ?? 1,
            queryParams.PageSize ?? 20
        );

        var (products, meta) = await commerceService.SearchProductsAsync(
            request,
            cancellationToken
        );
        return Results.Ok(ApiResponse<IReadOnlyList<ProductResponse>>.Ok(products, meta));
    }

    private static async Task<IResult> GetRetailersAsync(
        ICommerceService commerceService,
        CancellationToken cancellationToken
    )
    {
        var retailers = await commerceService.GetRetailersAsync(cancellationToken);
        return Results.Ok(ApiResponse<IReadOnlyList<RetailerResponse>>.Ok(retailers));
    }

    private static async Task<IResult> GetRetailerProductsAsync(
        Guid id,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        [FromQuery] string? category,
        ICommerceService commerceService,
        CancellationToken cancellationToken
    )
    {
        var (data, meta) = await commerceService.GetRetailerProductsAsync(
            id,
            page ?? 1,
            pageSize ?? 20,
            category,
            cancellationToken
        );
        return Results.Ok(ApiResponse<RetailerProductsResponse>.Ok(data, meta));
    }

    private static async Task<IResult> ProcessConversionAsync(
        Guid id,
        [FromBody] ConversionWebhookRequest request,
        ICommerceService commerceService,
        CancellationToken cancellationToken
    )
    {
        var success = await commerceService.ProcessConversionAsync(id, request, cancellationToken);
        if (!success)
        {
            return Results.BadRequest(
                ApiResponse.Fail("CONVERSION_FAILED", "Failed to process conversion")
            );
        }
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

public class ProductSearchQueryParams
{
    [FromQuery(Name = "q")]
    public string? Q { get; set; }

    [FromQuery(Name = "retailerId")]
    public Guid? RetailerId { get; set; }

    [FromQuery(Name = "category")]
    public string? Category { get; set; }

    [FromQuery(Name = "page")]
    public int? Page { get; set; }

    [FromQuery(Name = "pageSize")]
    public int? PageSize { get; set; }
}
