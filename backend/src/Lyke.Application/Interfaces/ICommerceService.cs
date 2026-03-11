using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Commerce;
using Lyke.Application.DTOs.Feed;

namespace Lyke.Application.Interfaces;

public interface ICommerceService
{
    /// <summary>
    /// Track an outbound click and generate affiliate URL
    /// </summary>
    Task<TrackClickResponse> TrackClickAsync(
        Guid? userId,
        TrackClickRequest request,
        string? userAgent,
        string? ipAddress,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Get product details by ID
    /// </summary>
    Task<ProductResponse> GetProductAsync(
        Guid productId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Search products (for creators tagging posts)
    /// </summary>
    Task<(IReadOnlyList<ProductResponse> Products, PaginationMeta Meta)> SearchProductsAsync(
        ProductSearchRequest request,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Get all active retailers
    /// </summary>
    Task<IReadOnlyList<RetailerResponse>> GetRetailersAsync(
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Get retailer products (paginated)
    /// </summary>
    Task<(RetailerProductsResponse Data, PaginationMeta Meta)> GetRetailerProductsAsync(
        Guid retailerId,
        int page,
        int pageSize,
        string? category,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Get published posts linked to a product via PostProduct records
    /// </summary>
    Task<IReadOnlyList<FeedPostResponse>> GetProductPostsAsync(
        Guid productId,
        Guid? userId,
        int limit = 10,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Process conversion webhook from affiliate network/retailer
    /// </summary>
    Task<bool> ProcessConversionAsync(
        Guid retailerId,
        ConversionWebhookRequest request,
        CancellationToken cancellationToken = default
    );
}
