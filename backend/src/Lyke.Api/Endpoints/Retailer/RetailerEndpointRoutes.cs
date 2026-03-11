using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Retailer;

namespace Lyke.Api.Endpoints.Retailer;

public static class RetailerEndpointRoutes
{
    public static IEndpointRouteBuilder MapRetailerEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/retailers/v1/portal")
            .WithTags("Retailer Portal")
            .RequireAuthorization("RetailerOnly");

        // Registration - any authenticated user can register
        group
            .MapPost("/register", RegisterAsRetailer.Handle)
            .RequireAuthorization()
            .WithName("RegisterAsRetailer")
            .WithSummary("Register current user as a retailer")
            .Produces<ApiResponse<RetailerProfileResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized);

        // Profile
        group
            .MapGet("/profile", GetRetailerProfile.Handle)
            .WithName("GetRetailerProfile")
            .WithSummary("Get current retailer's profile")
            .Produces<ApiResponse<RetailerProfileResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound);

        group
            .MapPut("/profile", UpdateRetailerProfile.Handle)
            .WithName("UpdateRetailerProfile")
            .WithSummary("Update current retailer's profile")
            .Produces<ApiResponse<RetailerProfileResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound);

        // Products
        group
            .MapPost("/products/import", ImportProducts.Handle)
            .WithName("ImportProducts")
            .WithSummary("Import products from CSV file")
            .Accepts<IFormFile>("multipart/form-data")
            .Produces<ApiResponse<ImportProductsResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .DisableAntiforgery();

        group
            .MapGet("/products", GetProducts.Handle)
            .WithName("GetRetailerPortalProducts")
            .WithSummary("List retailer's products")
            .Produces<ApiResponse<IReadOnlyList<RetailerProductResponse>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound);

        group
            .MapPut("/products/{id:guid}", UpdateProduct.Handle)
            .WithName("UpdateRetailerProduct")
            .WithSummary("Update a product")
            .Produces<ApiResponse<RetailerProductResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound);

        // Campaigns
        group
            .MapPost("/campaigns", CreateCampaign.Handle)
            .WithName("CreateCampaign")
            .WithSummary("Create a sponsored campaign")
            .Produces<ApiResponse<CampaignResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized);

        group
            .MapGet("/campaigns", GetCampaigns.Handle)
            .WithName("GetCampaigns")
            .WithSummary("List campaigns")
            .Produces<ApiResponse<IReadOnlyList<CampaignResponse>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized);

        group
            .MapPut("/campaigns/{id:guid}", UpdateCampaign.Handle)
            .WithName("UpdateCampaign")
            .WithSummary("Update a campaign")
            .Produces<ApiResponse<CampaignResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound);

        // Analytics
        group
            .MapGet("/analytics", GetAnalytics.Handle)
            .WithName("GetRetailerAnalytics")
            .WithSummary("Get engagement analytics")
            .Produces<ApiResponse<RetailerAnalyticsResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized);

        group
            .MapGet("/analytics/export", ExportAnalyticsCsv.Handle)
            .WithName("ExportRetailerAnalytics")
            .WithSummary("Export analytics as CSV")
            .Produces(StatusCodes.Status200OK, contentType: "text/csv")
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized);

        // Insights
        group
            .MapGet("/insights/fit", GetFitInsights.Handle)
            .WithName("GetFitInsights")
            .WithSummary("Get fit feedback insights for products")
            .Produces<ApiResponse<FitInsightsResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized);

        group
            .MapGet("/insights/body-profiles", GetBodyProfileInsights.Handle)
            .WithName("GetBodyProfileInsights")
            .WithSummary("Get anonymized body profile insights for engaged users")
            .Produces<ApiResponse<BodyProfileInsightsResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized);

        return app;
    }
}
