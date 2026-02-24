using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Lyke.Application.Configuration;
using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Commerce;
using Lyke.Application.DTOs.Feed;
using Lyke.Application.DTOs.Profile;
using Lyke.Application.Interfaces;
using Lyke.Core.Entities;
using Lyke.Core.Enums;
using Lyke.Core.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lyke.Application.Services;

public class CommerceService : ICommerceService
{
    private readonly DbContext _dbContext;
    private readonly CommerceSettings _settings;
    private readonly IEventTrackingService _eventTracking;
    private readonly IMemoryCache _cache;
    private readonly ILogger<CommerceService> _logger;
    private bool UseFullTextSearch => _dbContext.Database.ProviderName == "Npgsql.EntityFrameworkCore.PostgreSQL";

    private const string RetailersCacheKey = "commerce:retailers";
    private static readonly TimeSpan RetailerCacheDuration = TimeSpan.FromMinutes(15);

    public CommerceService(
        DbContext dbContext,
        IOptions<CommerceSettings> settings,
        IEventTrackingService eventTracking,
        IMemoryCache cache,
        ILogger<CommerceService> logger
    )
    {
        _dbContext = dbContext;
        _settings = settings.Value;
        _eventTracking = eventTracking;
        _cache = cache;
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

        _ = _eventTracking.TrackAsync(
            AnalyticsEventType.ProductClick,
            userId,
            request.PostProductId,
            nameof(PostProduct),
            new Dictionary<string, string>
            {
                ["postId"] = request.PostId.ToString(),
                ["source"] = request.Source ?? ""
            },
            request.SessionId,
            cancellationToken);

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
        if (_cache.TryGetValue(RetailersCacheKey, out IReadOnlyList<RetailerResponse>? cached) && cached != null)
        {
            return cached;
        }

        var retailers = await _dbContext
            .Set<Retailer>()
            .AsNoTracking()
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

        var result = retailers
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

        _cache.Set(RetailersCacheKey, (IReadOnlyList<RetailerResponse>)result, RetailerCacheDuration);
        return result;
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

    public async Task<IReadOnlyList<FeedPostResponse>> GetProductPostsAsync(
        Guid productId,
        Guid? userId,
        int limit = 10,
        CancellationToken cancellationToken = default
    )
    {
        var product = await _dbContext
            .Set<Product>()
            .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);

        if (product == null)
        {
            throw new NotFoundException(nameof(Product), productId);
        }

        var posts = await _dbContext
            .Set<Post>()
            .AsNoTracking()
            .Where(p =>
                p.Status == PostStatus.Published
                && p.PostProducts.Any(pp => pp.ProductId == productId)
            )
            .Include(p => p.Creator)
            .ThenInclude(c => c.User)
            .ThenInclude(u => u.BodyProfile)
            .ThenInclude(bp => bp!.BodyType)
            .Include(p => p.Creator)
            .ThenInclude(c => c.User)
            .ThenInclude(u => u.BodyProfile!)
            .ThenInclude(bp => bp.FrameSize)
            .Include(p => p.Creator)
            .ThenInclude(c => c.User)
            .ThenInclude(u => u.BodyProfile!)
            .ThenInclude(bp => bp.FitPreferences)
            .Include(p => p.PostProducts)
            .ThenInclude(pp => pp.Product)
            .ThenInclude(prod => prod.Retailer)
            .Include(p => p.PostProducts)
            .ThenInclude(pp => pp.FitTags)
            .ThenInclude(pft => pft.FitTag)
            .Include(p => p.Engagements)
            .AsSplitQuery()
            .OrderByDescending(p => p.PublishedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);

        var postIds = posts.Select(p => p.Id).ToList();
        var userEngagements = userId.HasValue
            ? await GetUserEngagementsAsync(userId.Value, postIds, cancellationToken)
            : new Dictionary<Guid, HashSet<EngagementType>>();

        return posts.Select(p => MapToFeedPostResponse(p, userEngagements)).ToList();
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

        _ = _eventTracking.TrackAsync(
            AnalyticsEventType.ProductConvert,
            clickEvent.UserId,
            clickId,
            nameof(ClickEvent),
            new Dictionary<string, string>
            {
                ["orderId"] = request.OrderId,
                ["amount"] = request.CommissionAmount.ToString("F2"),
                ["currency"] = request.Currency
            },
            cancellationToken: cancellationToken);

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

    private async Task<Dictionary<Guid, HashSet<EngagementType>>> GetUserEngagementsAsync(
        Guid userId,
        IEnumerable<Guid> postIds,
        CancellationToken cancellationToken
    )
    {
        var engagements = await _dbContext
            .Set<Engagement>()
            .AsNoTracking()
            .Where(e => e.UserId == userId && postIds.Contains(e.PostId))
            .ToListAsync(cancellationToken);

        return engagements
            .GroupBy(e => e.PostId)
            .ToDictionary(g => g.Key, g => g.Select(e => e.Type).ToHashSet());
    }

    private static FeedPostResponse MapToFeedPostResponse(
        Post post,
        Dictionary<Guid, HashSet<EngagementType>> userEngagements
    )
    {
        var engagementCounts = new EngagementCountsResponse(
            post.Engagements.Count(e => e.Type == EngagementType.View),
            post.Engagements.Count(e => e.Type == EngagementType.Like),
            post.Engagements.Count(e => e.Type == EngagementType.Save),
            post.Engagements.Count(e => e.Type == EngagementType.Share)
        );

        var userPostEngagements = userEngagements.GetValueOrDefault(
            post.Id,
            new HashSet<EngagementType>()
        );

        var creatorBodyProfile = post.Creator.User.BodyProfile;
        AnonymizedBodyProfileResponse? anonymizedProfile = null;
        if (creatorBodyProfile != null)
        {
            anonymizedProfile = new AnonymizedBodyProfileResponse(
                GetHeightRange(creatorBodyProfile.HeightCm),
                GetWeightRange(creatorBodyProfile.WeightKg),
                creatorBodyProfile.BodyType.Name,
                creatorBodyProfile.FrameSize?.Name,
                creatorBodyProfile.FitPreferences.Select(fp => fp.FitPreference).ToList()
            );
        }

        var creator = new CreatorSummaryResponse(
            post.Creator.Id,
            post.Creator.DisplayName,
            post.Creator.IsVerified,
            anonymizedProfile,
            post.Creator.User.ProfileImageUrl
        );

        var products = post
            .PostProducts.Select(pp => new PostProductSummaryResponse(
                pp.Id,
                pp.ProductId,
                pp.Product.Name,
                ParseMediaUrls(pp.Product.ImageUrls).FirstOrDefault(),
                pp.Product.Price,
                pp.Product.Currency,
                pp.SizeWorn,
                pp.FitRating,
                pp.FitNotes,
                pp.FitTags.Select(ft => ft.FitTag.Name).ToList()
            ))
            .ToList();

        return new FeedPostResponse(
            post.Id,
            post.Title,
            post.Description,
            post.MediaType,
            ParseMediaUrls(post.MediaUrls),
            ParseMediaUrls(post.ThumbnailUrls),
            creator,
            products,
            engagementCounts,
            0,
            userPostEngagements.Contains(EngagementType.Like),
            userPostEngagements.Contains(EngagementType.Save),
            post.PublishedAt ?? post.CreatedAt,
            false
        );
    }

    private static string GetHeightRange(int heightCm)
    {
        return heightCm switch
        {
            < 155 => "Under 155cm (5'1\")",
            < 160 => "155-159cm (5'1\"-5'2\")",
            < 165 => "160-164cm (5'3\"-5'4\")",
            < 170 => "165-169cm (5'5\"-5'6\")",
            < 175 => "170-174cm (5'7\"-5'8\")",
            < 180 => "175-179cm (5'9\"-5'10\")",
            < 185 => "180-184cm (5'11\"-6'0\")",
            < 190 => "185-189cm (6'1\"-6'2\")",
            _ => "190cm+ (6'3\"+)",
        };
    }

    private static string GetWeightRange(decimal weightKg)
    {
        return weightKg switch
        {
            < 50 => "Under 50kg (110lbs)",
            < 55 => "50-54kg (110-121lbs)",
            < 60 => "55-59kg (121-130lbs)",
            < 65 => "60-64kg (132-143lbs)",
            < 70 => "65-69kg (143-152lbs)",
            < 75 => "70-74kg (154-163lbs)",
            < 80 => "75-79kg (165-174lbs)",
            < 85 => "80-84kg (176-185lbs)",
            < 90 => "85-89kg (187-196lbs)",
            < 95 => "90-94kg (198-207lbs)",
            < 100 => "95-99kg (209-218lbs)",
            _ => "100kg+ (220lbs+)",
        };
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
