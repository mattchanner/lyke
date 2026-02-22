using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Lyke.Application.Configuration;
using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Commerce;
using Lyke.Application.Interfaces;
using Lyke.Core.Entities;
using Lyke.Core.Enums;
using Lyke.Core.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lyke.Application.Services;

public class CommerceService : ICommerceService
{
    private readonly DbContext _dbContext;
    private readonly CommerceSettings _settings;
    private readonly ILogger<CommerceService> _logger;
    private bool UseFullTextSearch => _dbContext.Database.ProviderName == "Npgsql.EntityFrameworkCore.PostgreSQL";

    public CommerceService(
        DbContext dbContext,
        IOptions<CommerceSettings> settings,
        ILogger<CommerceService> logger
    )
    {
        _dbContext = dbContext;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<TrackClickResponse> TrackClickAsync(
        Guid? userId,
        TrackClickRequest request,
        string? userAgent,
        string? ipAddress,
        CancellationToken cancellationToken = default
    )
    {
        // Validate PostProduct exists and get product/retailer data
        var postProduct = await _dbContext
            .Set<PostProduct>()
            .Include(pp => pp.Product)
            .ThenInclude(p => p.Retailer)
            .Include(pp => pp.Post)
            .FirstOrDefaultAsync(
                pp => pp.Id == request.PostProductId && pp.PostId == request.PostId,
                cancellationToken
            );

        if (postProduct == null)
        {
            throw new NotFoundException(nameof(PostProduct), request.PostProductId);
        }

        // Check for duplicate clicks within deduplication window
        if (_settings.EnableClickDeduplication && !string.IsNullOrEmpty(request.SessionId))
        {
            var recentClick = await _dbContext
                .Set<ClickEvent>()
                .Where(ce =>
                    ce.SessionId == request.SessionId
                    && ce.PostProductId == request.PostProductId
                    && ce.CreatedAt
                        > DateTime.UtcNow.AddSeconds(-_settings.DeduplicationWindowSeconds)
                )
                .FirstOrDefaultAsync(cancellationToken);

            if (recentClick != null)
            {
                // Return existing click info instead of creating duplicate
                var existingUrl = GenerateAffiliateUrl(
                    postProduct.Product.Retailer,
                    postProduct.Product.ProductUrl,
                    recentClick.Id
                );

                return new TrackClickResponse(
                    recentClick.Id,
                    existingUrl,
                    postProduct.Product.Retailer.Name,
                    postProduct.Product.Name
                );
            }
        }

        // Generate unique click ID
        var clickId = Guid.NewGuid();

        // Build attribution data
        var attributionData = new
        {
            source = request.Source,
            sessionId = request.SessionId,
            userAgent = userAgent,
            platform = request.Platform,
            appVersion = request.AppVersion,
            feedPosition = request.FeedPosition,
            searchQuery = request.SearchQuery,
            creatorId = postProduct.Post.CreatorId,
            timestamp = DateTime.UtcNow,
            ipAddress = ipAddress != null ? HashIpAddress(ipAddress) : null,
        };

        // Generate affiliate URL
        var affiliateUrl = GenerateAffiliateUrl(
            postProduct.Product.Retailer,
            postProduct.Product.ProductUrl,
            clickId
        );

        // Create ClickEvent record
        var clickEvent = new ClickEvent
        {
            Id = clickId,
            UserId = userId,
            PostId = request.PostId,
            PostProductId = request.PostProductId,
            SessionId = request.SessionId,
            CreatedAt = DateTime.UtcNow,
            AttributionData = JsonSerializer.Serialize(attributionData),
        };

        await _dbContext.Set<ClickEvent>().AddAsync(clickEvent, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Click tracked: {ClickId} for PostProduct {PostProductId} by User {UserId}",
            clickId,
            request.PostProductId,
            userId
        );

        return new TrackClickResponse(
            clickId,
            affiliateUrl,
            postProduct.Product.Retailer.Name,
            postProduct.Product.Name
        );
    }

    public async Task<ProductResponse> GetProductAsync(
        Guid productId,
        CancellationToken cancellationToken = default
    )
    {
        var product = await _dbContext
            .Set<Product>()
            .Include(p => p.Retailer)
            .Include(p => p.PostProducts.Where(pp => pp.Post.Status == PostStatus.Published))
            .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);

        if (product == null)
        {
            throw new NotFoundException(nameof(Product), productId);
        }

        return MapToProductResponse(product);
    }

    public async Task<(
        IReadOnlyList<ProductResponse> Products,
        PaginationMeta Meta
    )> SearchProductsAsync(
        [AsParameters] ProductSearchRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var searchTerm = request.Query;

        var query = _dbContext
            .Set<Product>()
            .Include(p => p.Retailer)
            .Include(p => p.PostProducts.Where(pp => pp.Post.Status == PostStatus.Published))
            .Where(p => p.IsActive);

        // Apply search filter using full-text search (PostgreSQL) or LIKE fallback
        // Skip filter when query is empty to return all products (browse mode)
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = UseFullTextSearch
                ? query.Where(p =>
                    EF.Functions.ToTsVector("english", p.Name + " " + (p.Description ?? "") + " " + p.ExternalSku)
                        .Matches(EF.Functions.PlainToTsQuery("english", searchTerm)))
                : query.Where(p =>
                    p.Name.ToLower().Contains(searchTerm.ToLower())
                    || (p.Description != null && p.Description.ToLower().Contains(searchTerm.ToLower()))
                    || p.ExternalSku.ToLower().Contains(searchTerm.ToLower()));
        }

        // Apply retailer filter
        if (request.RetailerId.HasValue)
        {
            query = query.Where(p => p.RetailerId == request.RetailerId.Value);
        }

        // Apply category filter
        if (!string.IsNullOrEmpty(request.Category))
        {
            query = query.Where(p => p.Category == request.Category);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var orderedQuery = UseFullTextSearch && !string.IsNullOrWhiteSpace(searchTerm)
            ? query
                .OrderByDescending(p =>
                    EF.Functions.ToTsVector("english", p.Name + " " + (p.Description ?? "") + " " + p.ExternalSku)
                        .Rank(EF.Functions.PlainToTsQuery("english", searchTerm)))
                .ThenBy(p => p.Name)
            : query.OrderBy(p => p.Name);

        var products = await orderedQuery
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var productResponses = products.Select(MapToProductResponse).ToList();

        var meta = new PaginationMeta
        {
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount,
        };

        return (productResponses, meta);
    }

    public async Task<IReadOnlyList<RetailerResponse>> GetRetailersAsync(
        CancellationToken cancellationToken = default
    )
    {
        var retailers = await _dbContext
            .Set<Retailer>()
            .Where(r => r.IsActive)
            .Include(r => r.Products.Where(p => p.IsActive))
            .OrderBy(r => r.Name)
            .ToListAsync(cancellationToken);

        // Get post counts per retailer
        var retailerIds = retailers.Select(r => r.Id).ToList();
        var postCounts = await _dbContext
            .Set<PostProduct>()
            .Include(pp => pp.Product)
            .Include(pp => pp.Post)
            .Where(pp =>
                retailerIds.Contains(pp.Product.RetailerId)
                && pp.Post.Status == PostStatus.Published
            )
            .GroupBy(pp => pp.Product.RetailerId)
            .Select(g => new
            {
                RetailerId = g.Key,
                Count = g.Select(pp => pp.PostId).Distinct().Count(),
            })
            .ToListAsync(cancellationToken);

        var postCountDict = postCounts.ToDictionary(x => x.RetailerId, x => x.Count);

        return retailers
            .Select(r => new RetailerResponse(
                r.Id,
                r.Name,
                r.LogoUrl,
                r.WebsiteUrl,
                r.IsActive,
                r.Products.Count,
                postCountDict.GetValueOrDefault(r.Id, 0)
            ))
            .ToList();
    }

    public async Task<(
        RetailerProductsResponse Data,
        PaginationMeta Meta
    )> GetRetailerProductsAsync(
        Guid retailerId,
        int page,
        int pageSize,
        string? category,
        CancellationToken cancellationToken = default
    )
    {
        var retailer = await _dbContext
            .Set<Retailer>()
            .FirstOrDefaultAsync(r => r.Id == retailerId, cancellationToken);

        if (retailer == null)
        {
            throw new NotFoundException(nameof(Retailer), retailerId);
        }

        var query = _dbContext
            .Set<Product>()
            .Include(p => p.Retailer)
            .Include(p => p.PostProducts.Where(pp => pp.Post.Status == PostStatus.Published))
            .Where(p => p.RetailerId == retailerId && p.IsActive);

        if (!string.IsNullOrEmpty(category))
        {
            query = query.Where(p => p.Category == category);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var products = await query
            .OrderBy(p => p.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var productResponses = products.Select(MapToProductResponse).ToList();

        var data = new RetailerProductsResponse(retailerId, retailer.Name, productResponses);

        var meta = new PaginationMeta
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
        };

        return (data, meta);
    }

    public async Task<bool> ProcessConversionAsync(
        Guid retailerId,
        ConversionWebhookRequest request,
        CancellationToken cancellationToken = default
    )
    {
        // Get retailer and verify webhook signature
        var retailer = await _dbContext
            .Set<Retailer>()
            .FirstOrDefaultAsync(r => r.Id == retailerId, cancellationToken);

        if (retailer == null)
        {
            throw new NotFoundException(nameof(Retailer), retailerId);
        }

        var config = ParseAffiliateConfig(retailer.AffiliateConfig);
        if (!VerifyWebhookSignature(request, config?.WebhookSecret))
        {
            _logger.LogWarning("Invalid webhook signature for retailer {RetailerId}", retailerId);
            return false;
        }

        // Find the click event
        if (!Guid.TryParse(request.ClickId, out var clickId))
        {
            _logger.LogWarning("Invalid click ID format: {ClickId}", request.ClickId);
            return false;
        }

        var clickEvent = await _dbContext
            .Set<ClickEvent>()
            .Include(ce => ce.PostProduct)
            .ThenInclude(pp => pp.Post)
            .FirstOrDefaultAsync(ce => ce.Id == clickId, cancellationToken);

        if (clickEvent == null)
        {
            _logger.LogWarning("Click event not found: {ClickId}", clickId);
            return false;
        }

        // Check if already converted
        if (clickEvent.ConvertedAt.HasValue)
        {
            _logger.LogWarning("Click event already converted: {ClickId}", clickId);
            return false;
        }

        // Update click event with conversion
        clickEvent.ConvertedAt = request.TransactionDate;

        // Create creator earning record
        var creatorEarning = new CreatorEarning
        {
            Id = Guid.NewGuid(),
            CreatorId = clickEvent.PostProduct.Post.CreatorId,
            ClickEventId = clickId,
            EarningType = EarningType.Affiliate,
            Amount = request.CommissionAmount * _settings.CreatorCommissionShare,
            Currency = request.Currency,
            Status = EarningStatus.Pending,
            CreatedAt = DateTime.UtcNow,
        };

        await _dbContext.Set<CreatorEarning>().AddAsync(creatorEarning, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Conversion processed: Click {ClickId}, Order {OrderId}, Earnings {Amount} {Currency}",
            clickId,
            request.OrderId,
            creatorEarning.Amount,
            request.Currency
        );

        return true;
    }

    #region Private Methods

    private string GenerateAffiliateUrl(Retailer retailer, string productUrl, Guid clickId)
    {
        var config = ParseAffiliateConfig(retailer.AffiliateConfig);

        if (config == null || !config.Enabled || string.IsNullOrEmpty(config.BaseUrl))
        {
            // No affiliate config - return direct product URL
            return productUrl;
        }

        // Build affiliate URL
        var separator = config.BaseUrl.Contains('?') ? "&" : "?";
        var subIdParam = config.Parameters?.SubIdParam ?? "subId";
        var productUrlParam = config.Parameters?.ProductUrlParam ?? "url";

        var affiliateUrl = $"{config.BaseUrl}{separator}{subIdParam}={clickId}";

        if (!string.IsNullOrEmpty(productUrlParam))
        {
            affiliateUrl += $"&{productUrlParam}={Uri.EscapeDataString(productUrl)}";
        }

        // Add custom parameters
        if (config.CustomParams != null)
        {
            foreach (var param in config.CustomParams)
            {
                affiliateUrl += $"&{param.Key}={Uri.EscapeDataString(param.Value)}";
            }
        }

        return affiliateUrl;
    }

    private static AffiliateConfig? ParseAffiliateConfig(string? json)
    {
        if (string.IsNullOrEmpty(json))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<AffiliateConfig>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );
        }
        catch
        {
            return null;
        }
    }

    private static bool VerifyWebhookSignature(ConversionWebhookRequest request, string? secret)
    {
        if (string.IsNullOrEmpty(secret))
        {
            return true; // No signature verification configured
        }

        // HMAC-SHA256 verification
        var payload = $"{request.ClickId}:{request.OrderId}:{request.OrderValue}";
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        var expectedSignature = Convert.ToBase64String(hash);

        return request.Signature == expectedSignature;
    }

    private static string? HashIpAddress(string ipAddress)
    {
        // Hash IP for privacy - we don't need the actual IP, just consistency for rate limiting
        using var sha = SHA256.Create();
        var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(ipAddress));
        return Convert.ToBase64String(hash)[..16];
    }

    private static ProductResponse MapToProductResponse(Product product)
    {
        return new ProductResponse(
            product.Id,
            product.RetailerId,
            product.Retailer.Name,
            product.Retailer.LogoUrl,
            product.ExternalSku,
            product.Name,
            product.Description,
            product.Category,
            product.SubCategory,
            ParseMediaUrls(product.ImageUrls),
            product.ProductUrl,
            product.Price,
            product.Currency,
            product.IsActive,
            product.PostProducts.Count
        );
    }

    private static List<string> ParseMediaUrls(string? json)
    {
        if (string.IsNullOrEmpty(json))
        {
            return new List<string>();
        }

        try
        {
            return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }

    #endregion
}

#region Internal Classes

internal class AffiliateConfig
{
    public bool Enabled { get; set; }
    public string? BaseUrl { get; set; }
    public AffiliateParameters? Parameters { get; set; }
    public decimal CommissionRate { get; set; }
    public int CookieWindowDays { get; set; }
    public string? WebhookSecret { get; set; }
    public Dictionary<string, string>? CustomParams { get; set; }
}

internal class AffiliateParameters
{
    public string? SubIdParam { get; set; }
    public string? ProductUrlParam { get; set; }
}

#endregion
