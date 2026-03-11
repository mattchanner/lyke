using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Retailer;

namespace Lyke.Application.Interfaces;

public interface IRetailerService
{
    // Registration & Profile
    Task<RetailerProfileResponse> RegisterAsRetailerAsync(
        Guid userId,
        RegisterRetailerRequest request,
        CancellationToken cancellationToken = default
    );
    Task<RetailerProfileResponse> GetRetailerProfileAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    );
    Task<RetailerProfileResponse> UpdateRetailerProfileAsync(
        Guid userId,
        UpdateRetailerProfileRequest request,
        CancellationToken cancellationToken = default
    );

    // Products
    Task<ImportProductsResponse> ImportProductsAsync(
        Guid userId,
        Stream csvStream,
        CancellationToken cancellationToken = default
    );
    Task<(IReadOnlyList<RetailerProductResponse> Products, PaginationMeta Meta)> GetProductsAsync(
        Guid userId,
        RetailerProductsRequest request,
        CancellationToken cancellationToken = default
    );
    Task<RetailerProductResponse> UpdateProductAsync(
        Guid userId,
        Guid productId,
        UpdateRetailerProductRequest request,
        CancellationToken cancellationToken = default
    );

    // Campaigns
    Task<CampaignResponse> CreateCampaignAsync(
        Guid userId,
        CreateCampaignRequest request,
        CancellationToken cancellationToken = default
    );
    Task<(IReadOnlyList<CampaignResponse> Campaigns, PaginationMeta Meta)> GetCampaignsAsync(
        Guid userId,
        CampaignListRequest request,
        CancellationToken cancellationToken = default
    );
    Task<CampaignResponse> UpdateCampaignAsync(
        Guid userId,
        Guid campaignId,
        UpdateCampaignRequest request,
        CancellationToken cancellationToken = default
    );

    // Analytics
    Task<RetailerAnalyticsResponse> GetAnalyticsAsync(
        Guid userId,
        RetailerAnalyticsRequest request,
        CancellationToken cancellationToken = default
    );
    Task<byte[]> ExportAnalyticsCsvAsync(
        Guid userId,
        RetailerAnalyticsRequest request,
        CancellationToken cancellationToken = default
    );

    // Insights
    Task<FitInsightsResponse> GetFitInsightsAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    );
    Task<BodyProfileInsightsResponse> GetBodyProfileInsightsAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    );

    // Public brands listing
    Task<(IReadOnlyList<BrandResponse> Brands, PaginationMeta Meta)> GetBrandsAsync(
        BrandsRequest request,
        CancellationToken cancellationToken = default
    );
}
