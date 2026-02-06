using System.Text;
using System.Text.Json;
using Lyke.Application.Configuration;
using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Retailer;
using Lyke.Application.Interfaces;
using Lyke.Core.Entities;
using Lyke.Core.Enums;
using Lyke.Core.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lyke.Application.Services;

public class RetailerService : IRetailerService
{
    private readonly DbContext _dbContext;
    private readonly UserManager<User> _userManager;
    private readonly RetailerSettings _retailerSettings;
    private readonly ILogger<RetailerService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public RetailerService(
        DbContext dbContext,
        UserManager<User> userManager,
        IOptions<RetailerSettings> retailerSettings,
        ILogger<RetailerService> logger
    )
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _retailerSettings = retailerSettings.Value;
        _logger = logger;
    }

    #region Registration & Profile

    public async Task<RetailerProfileResponse> RegisterAsRetailerAsync(
        Guid userId,
        RegisterRetailerRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            throw new NotFoundException(nameof(User), userId);
        }

        var retailers = _dbContext.Set<Retailer>();

        var existingRetailer = await retailers.FirstOrDefaultAsync(
            r => r.UserId == userId,
            cancellationToken
        );
        if (existingRetailer != null)
        {
            throw new ValidationException("Retailer", "User is already registered as a retailer");
        }

        var retailer = new Retailer
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = request.Name,
            LogoUrl = request.LogoUrl,
            WebsiteUrl = request.WebsiteUrl,
            ContactEmail = request.ContactEmail,
        };

        await retailers.AddAsync(retailer, cancellationToken);

        user.UserType = UserType.Retailer;
        await _userManager.UpdateAsync(user);

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "User {UserId} registered as retailer {RetailerId}",
            userId,
            retailer.Id
        );

        return await GetRetailerProfileAsync(userId, cancellationToken);
    }

    public async Task<RetailerProfileResponse> GetRetailerProfileAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        var retailer = await GetRetailerByUserIdAsync(userId, cancellationToken);

        var totalProducts = await _dbContext
            .Set<Product>()
            .CountAsync(p => p.RetailerId == retailer.Id, cancellationToken);

        var activeProducts = await _dbContext
            .Set<Product>()
            .CountAsync(p => p.RetailerId == retailer.Id && p.IsActive, cancellationToken);

        var totalCampaigns = await _dbContext
            .Set<SponsoredPlacement>()
            .CountAsync(sp => sp.RetailerId == retailer.Id, cancellationToken);

        var activeCampaigns = await _dbContext
            .Set<SponsoredPlacement>()
            .CountAsync(
                sp => sp.RetailerId == retailer.Id && sp.IsActive && sp.EndDate > DateTime.UtcNow,
                cancellationToken
            );

        return new RetailerProfileResponse(
            Id: retailer.Id,
            Name: retailer.Name,
            LogoUrl: retailer.LogoUrl,
            WebsiteUrl: retailer.WebsiteUrl,
            ContactEmail: retailer.ContactEmail,
            IsActive: retailer.IsActive,
            TotalProducts: totalProducts,
            ActiveProducts: activeProducts,
            TotalCampaigns: totalCampaigns,
            ActiveCampaigns: activeCampaigns,
            CreatedAt: retailer.CreatedAt
        );
    }

    public async Task<RetailerProfileResponse> UpdateRetailerProfileAsync(
        Guid userId,
        UpdateRetailerProfileRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var retailer = await GetRetailerByUserIdAsync(userId, cancellationToken);

        if (!string.IsNullOrEmpty(request.Name))
        {
            retailer.Name = request.Name;
        }

        if (request.LogoUrl != null)
        {
            retailer.LogoUrl = request.LogoUrl;
        }

        if (request.WebsiteUrl != null)
        {
            retailer.WebsiteUrl = request.WebsiteUrl;
        }

        if (request.ContactEmail != null)
        {
            retailer.ContactEmail = request.ContactEmail;
        }

        if (request.AffiliateConfig != null)
        {
            retailer.AffiliateConfig = JsonSerializer.Serialize(
                request.AffiliateConfig,
                JsonOptions
            );
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Retailer {RetailerId} profile updated", retailer.Id);

        return await GetRetailerProfileAsync(userId, cancellationToken);
    }

    #endregion

    #region Products

    public async Task<ImportProductsResponse> ImportProductsAsync(
        Guid userId,
        Stream csvStream,
        CancellationToken cancellationToken = default
    )
    {
        var retailer = await GetRetailerByUserIdAsync(userId, cancellationToken);

        var imported = 0;
        var updated = 0;
        var skipped = 0;
        var failed = 0;
        var errors = new List<ImportError>();
        var rowNumber = 0;

        using var reader = new StreamReader(csvStream);

        // Read header line
        var headerLine = await reader.ReadLineAsync(cancellationToken);
        if (string.IsNullOrEmpty(headerLine))
        {
            throw new ValidationException("CSV", "CSV file is empty");
        }

        // Expected: ExternalSku,Name,Description,Category,SubCategory,ImageUrl,ProductUrl,Price,Currency
        var existingProducts = await _dbContext
            .Set<Product>()
            .Where(p => p.RetailerId == retailer.Id)
            .ToDictionaryAsync(p => p.ExternalSku, cancellationToken);

        string? line;
        while ((line = await reader.ReadLineAsync(cancellationToken)) != null)
        {
            rowNumber++;

            if (rowNumber > _retailerSettings.MaxImportRows)
            {
                errors.Add(
                    new ImportError(
                        rowNumber,
                        null,
                        $"Maximum import rows ({_retailerSettings.MaxImportRows}) exceeded"
                    )
                );
                break;
            }

            if (string.IsNullOrWhiteSpace(line))
            {
                skipped++;
                continue;
            }

            try
            {
                var fields = ParseCsvLine(line);
                if (fields.Length < 9)
                {
                    errors.Add(
                        new ImportError(rowNumber, null, "Insufficient columns (expected 9)")
                    );
                    failed++;
                    continue;
                }

                var externalSku = fields[0].Trim();
                if (string.IsNullOrEmpty(externalSku))
                {
                    errors.Add(new ImportError(rowNumber, null, "ExternalSku is required"));
                    failed++;
                    continue;
                }

                if (!decimal.TryParse(fields[7].Trim(), out var price) || price < 0)
                {
                    errors.Add(new ImportError(rowNumber, externalSku, "Invalid price"));
                    failed++;
                    continue;
                }

                var currency = fields[8].Trim();
                if (string.IsNullOrEmpty(currency) || currency.Length != 3)
                {
                    currency = _retailerSettings.DefaultCurrency;
                }

                if (existingProducts.TryGetValue(externalSku, out var existingProduct))
                {
                    // Update existing product
                    existingProduct.Name = fields[1].Trim();
                    existingProduct.Description = string.IsNullOrWhiteSpace(fields[2])
                        ? null
                        : fields[2].Trim();
                    existingProduct.Category = fields[3].Trim();
                    existingProduct.SubCategory = string.IsNullOrWhiteSpace(fields[4])
                        ? null
                        : fields[4].Trim();
                    existingProduct.ImageUrls = string.IsNullOrWhiteSpace(fields[5])
                        ? null
                        : JsonSerializer.Serialize(new[] { fields[5].Trim() }, JsonOptions);
                    existingProduct.ProductUrl = fields[6].Trim();
                    existingProduct.Price = price;
                    existingProduct.Currency = currency;
                    existingProduct.LastSyncedAt = DateTime.UtcNow;
                    updated++;
                }
                else
                {
                    // Create new product
                    var product = new Product
                    {
                        Id = Guid.NewGuid(),
                        RetailerId = retailer.Id,
                        ExternalSku = externalSku,
                        Name = fields[1].Trim(),
                        Description = string.IsNullOrWhiteSpace(fields[2])
                            ? null
                            : fields[2].Trim(),
                        Category = fields[3].Trim(),
                        SubCategory = string.IsNullOrWhiteSpace(fields[4])
                            ? null
                            : fields[4].Trim(),
                        ImageUrls = string.IsNullOrWhiteSpace(fields[5])
                            ? null
                            : JsonSerializer.Serialize(new[] { fields[5].Trim() }, JsonOptions),
                        ProductUrl = fields[6].Trim(),
                        Price = price,
                        Currency = currency,
                        LastSyncedAt = DateTime.UtcNow,
                    };

                    await _dbContext.Set<Product>().AddAsync(product, cancellationToken);
                    existingProducts[externalSku] = product;
                    imported++;
                }
            }
            catch (Exception ex)
            {
                errors.Add(new ImportError(rowNumber, null, ex.Message));
                failed++;
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Retailer {RetailerId} imported products: {Imported} new, {Updated} updated, {Skipped} skipped, {Failed} failed",
            retailer.Id,
            imported,
            updated,
            skipped,
            failed
        );

        return new ImportProductsResponse(rowNumber, imported, updated, skipped, failed, errors);
    }

    public async Task<(
        IReadOnlyList<RetailerProductResponse> Products,
        PaginationMeta Meta
    )> GetProductsAsync(
        Guid userId,
        RetailerProductsRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var retailer = await GetRetailerByUserIdAsync(userId, cancellationToken);

        var query = _dbContext.Set<Product>().Where(p => p.RetailerId == retailer.Id);

        if (!string.IsNullOrEmpty(request.Category))
        {
            query = query.Where(p => p.Category == request.Category);
        }

        if (!string.IsNullOrEmpty(request.Search))
        {
            query = query.Where(p =>
                p.Name.Contains(request.Search) || p.ExternalSku.Contains(request.Search)
            );
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(p => p.IsActive == request.IsActive.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var products = await query
            .Include(p => p.PostProducts)
            .ThenInclude(pp => pp.ClickEvents)
            .OrderByDescending(p => p.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var productResponses = products.Select(p => MapToRetailerProductResponse(p)).ToList();

        var meta = new PaginationMeta
        {
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount,
        };

        return (productResponses, meta);
    }

    public async Task<RetailerProductResponse> UpdateProductAsync(
        Guid userId,
        Guid productId,
        UpdateRetailerProductRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var retailer = await GetRetailerByUserIdAsync(userId, cancellationToken);

        var product = await _dbContext
            .Set<Product>()
            .Include(p => p.PostProducts)
            .ThenInclude(pp => pp.ClickEvents)
            .FirstOrDefaultAsync(
                p => p.Id == productId && p.RetailerId == retailer.Id,
                cancellationToken
            );

        if (product == null)
        {
            throw new NotFoundException(nameof(Product), productId);
        }

        if (!string.IsNullOrEmpty(request.Name))
        {
            product.Name = request.Name;
        }

        if (request.Description != null)
        {
            product.Description = request.Description;
        }

        if (!string.IsNullOrEmpty(request.Category))
        {
            product.Category = request.Category;
        }

        if (request.SubCategory != null)
        {
            product.SubCategory = request.SubCategory;
        }

        if (!string.IsNullOrEmpty(request.ProductUrl))
        {
            product.ProductUrl = request.ProductUrl;
        }

        if (request.Price.HasValue)
        {
            product.Price = request.Price.Value;
        }

        if (!string.IsNullOrEmpty(request.Currency))
        {
            product.Currency = request.Currency;
        }

        if (request.IsActive.HasValue)
        {
            product.IsActive = request.IsActive.Value;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Retailer {RetailerId} updated product {ProductId}",
            retailer.Id,
            productId
        );

        return MapToRetailerProductResponse(product);
    }

    #endregion

    #region Campaigns

    public async Task<CampaignResponse> CreateCampaignAsync(
        Guid userId,
        CreateCampaignRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var retailer = await GetRetailerByUserIdAsync(userId, cancellationToken);

        if (request.BudgetAmount > _retailerSettings.MaxCampaignBudget)
        {
            throw new ValidationException(
                "BudgetAmount",
                $"Budget cannot exceed {_retailerSettings.MaxCampaignBudget}"
            );
        }

        // Validate product if specified
        if (request.ProductId.HasValue)
        {
            var product = await _dbContext
                .Set<Product>()
                .FirstOrDefaultAsync(
                    p => p.Id == request.ProductId.Value && p.RetailerId == retailer.Id,
                    cancellationToken
                );

            if (product == null)
            {
                throw new NotFoundException(nameof(Product), request.ProductId.Value);
            }
        }

        var campaign = new SponsoredPlacement
        {
            Id = Guid.NewGuid(),
            RetailerId = retailer.Id,
            ProductId = request.ProductId,
            BudgetAmount = request.BudgetAmount,
            SpentAmount = 0,
            TargetBodyTypes =
                request.TargetBodyTypes != null
                    ? JsonSerializer.Serialize(request.TargetBodyTypes, JsonOptions)
                    : null,
            TargetCategories =
                request.TargetCategories != null
                    ? JsonSerializer.Serialize(request.TargetCategories, JsonOptions)
                    : null,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            IsActive = true,
        };

        await _dbContext.Set<SponsoredPlacement>().AddAsync(campaign, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Retailer {RetailerId} created campaign {CampaignId}",
            retailer.Id,
            campaign.Id
        );

        return await MapToCampaignResponseAsync(campaign, cancellationToken);
    }

    public async Task<(
        IReadOnlyList<CampaignResponse> Campaigns,
        PaginationMeta Meta
    )> GetCampaignsAsync(
        Guid userId,
        CampaignListRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var retailer = await GetRetailerByUserIdAsync(userId, cancellationToken);

        var query = _dbContext.Set<SponsoredPlacement>().Where(sp => sp.RetailerId == retailer.Id);

        if (request.IsActive.HasValue)
        {
            query = query.Where(sp => sp.IsActive == request.IsActive.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var campaigns = await query
            .Include(sp => sp.Product)
            .OrderByDescending(sp => sp.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var campaignResponses = new List<CampaignResponse>();
        foreach (var campaign in campaigns)
        {
            campaignResponses.Add(await MapToCampaignResponseAsync(campaign, cancellationToken));
        }

        var meta = new PaginationMeta
        {
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount,
        };

        return (campaignResponses, meta);
    }

    public async Task<CampaignResponse> UpdateCampaignAsync(
        Guid userId,
        Guid campaignId,
        UpdateCampaignRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var retailer = await GetRetailerByUserIdAsync(userId, cancellationToken);

        var campaign = await _dbContext
            .Set<SponsoredPlacement>()
            .Include(sp => sp.Product)
            .FirstOrDefaultAsync(
                sp => sp.Id == campaignId && sp.RetailerId == retailer.Id,
                cancellationToken
            );

        if (campaign == null)
        {
            throw new NotFoundException(nameof(SponsoredPlacement), campaignId);
        }

        if (request.BudgetAmount.HasValue)
        {
            if (request.BudgetAmount.Value > _retailerSettings.MaxCampaignBudget)
            {
                throw new ValidationException(
                    "BudgetAmount",
                    $"Budget cannot exceed {_retailerSettings.MaxCampaignBudget}"
                );
            }
            campaign.BudgetAmount = request.BudgetAmount.Value;
        }

        if (request.TargetBodyTypes != null)
        {
            campaign.TargetBodyTypes = JsonSerializer.Serialize(
                request.TargetBodyTypes,
                JsonOptions
            );
        }

        if (request.TargetCategories != null)
        {
            campaign.TargetCategories = JsonSerializer.Serialize(
                request.TargetCategories,
                JsonOptions
            );
        }

        if (request.StartDate.HasValue)
        {
            campaign.StartDate = request.StartDate.Value;
        }

        if (request.EndDate.HasValue)
        {
            campaign.EndDate = request.EndDate.Value;
        }

        if (request.IsActive.HasValue)
        {
            campaign.IsActive = request.IsActive.Value;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Retailer {RetailerId} updated campaign {CampaignId}",
            retailer.Id,
            campaignId
        );

        return await MapToCampaignResponseAsync(campaign, cancellationToken);
    }

    #endregion

    #region Analytics

    public async Task<RetailerAnalyticsResponse> GetAnalyticsAsync(
        Guid userId,
        RetailerAnalyticsRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var retailer = await GetRetailerByUserIdAsync(userId, cancellationToken);

        var startDate = request.StartDate ?? DateTime.UtcNow.AddDays(-30);
        var endDate = request.EndDate ?? DateTime.UtcNow;

        // Get retailer's product IDs
        var productIds = await _dbContext
            .Set<Product>()
            .Where(p => p.RetailerId == retailer.Id)
            .Select(p => p.Id)
            .ToListAsync(cancellationToken);

        // Get post product IDs for this retailer's products
        var postProductIds = await _dbContext
            .Set<PostProduct>()
            .Where(pp => productIds.Contains(pp.ProductId))
            .Select(pp => pp.Id)
            .ToListAsync(cancellationToken);

        // Get post IDs that feature this retailer's products
        var postIds = await _dbContext
            .Set<PostProduct>()
            .Where(pp => productIds.Contains(pp.ProductId))
            .Select(pp => pp.PostId)
            .Distinct()
            .ToListAsync(cancellationToken);

        // Aggregate engagements
        var engagements = await _dbContext
            .Set<Engagement>()
            .Where(e =>
                postIds.Contains(e.PostId) && e.CreatedAt >= startDate && e.CreatedAt <= endDate
            )
            .ToListAsync(cancellationToken);

        var totalViews = engagements.Count(e => e.Type == EngagementType.View);

        // Aggregate clicks
        var clicks = await _dbContext
            .Set<ClickEvent>()
            .Where(c =>
                postProductIds.Contains(c.PostProductId)
                && c.CreatedAt >= startDate
                && c.CreatedAt <= endDate
            )
            .ToListAsync(cancellationToken);

        var totalClicks = clicks.Count;
        var totalConversions = clicks.Count(c => c.ConvertedAt != null);

        // Aggregate revenue from earnings
        var clickEventIds = clicks.Select(c => c.Id).ToList();
        var earnings = await _dbContext
            .Set<CreatorEarning>()
            .Where(e =>
                clickEventIds.Contains(e.ClickEventId)
                && e.CreatedAt >= startDate
                && e.CreatedAt <= endDate
            )
            .ToListAsync(cancellationToken);

        var totalRevenue = earnings.Sum(e => e.Amount);

        var summary = new RetailerAnalyticsSummary(
            TotalViews: totalViews,
            TotalClicks: totalClicks,
            TotalConversions: totalConversions,
            TotalRevenue: totalRevenue,
            TotalPosts: postIds.Count,
            Currency: _retailerSettings.DefaultCurrency
        );

        // Daily metrics
        var dailyMetrics = Enumerable
            .Range(0, (endDate - startDate).Days + 1)
            .Select(i => startDate.AddDays(i).Date)
            .Select(date => new RetailerDailyMetrics(
                Date: date,
                Views: engagements.Count(e =>
                    e.Type == EngagementType.View && e.CreatedAt.Date == date
                ),
                Clicks: clicks.Count(c => c.CreatedAt.Date == date),
                Conversions: clicks.Count(c => c.ConvertedAt != null && c.CreatedAt.Date == date),
                Revenue: earnings.Where(e => e.CreatedAt.Date == date).Sum(e => e.Amount)
            ))
            .ToList();

        // Top products
        var products = await _dbContext
            .Set<Product>()
            .Where(p => p.RetailerId == retailer.Id)
            .Include(p => p.PostProducts)
            .ThenInclude(pp => pp.ClickEvents)
            .ToListAsync(cancellationToken);

        var topProducts = products
            .Select(p =>
            {
                var ppIds = p.PostProducts.Select(pp => pp.Id).ToList();
                var ppPostIds = p.PostProducts.Select(pp => pp.PostId).Distinct().ToList();
                var productClicks = clicks.Where(c => ppIds.Contains(c.PostProductId)).ToList();
                var productClickIds = productClicks.Select(c => c.Id).ToList();

                return new TopProductAnalytics(
                    ProductId: p.Id,
                    ProductName: p.Name,
                    Category: p.Category,
                    Views: engagements.Count(e =>
                        ppPostIds.Contains(e.PostId) && e.Type == EngagementType.View
                    ),
                    Clicks: productClicks.Count,
                    Conversions: productClicks.Count(c => c.ConvertedAt != null),
                    Revenue: earnings
                        .Where(e => productClickIds.Contains(e.ClickEventId))
                        .Sum(e => e.Amount)
                );
            })
            .OrderByDescending(p => p.Clicks)
            .Take(10)
            .ToList();

        // Category breakdown
        var categoryBreakdown = products
            .GroupBy(p => p.Category)
            .Select(g =>
            {
                var categoryProductIds = g.Select(p => p.Id).ToList();
                var categoryPpIds = g.SelectMany(p => p.PostProducts).Select(pp => pp.Id).ToList();
                var categoryPostIds = g.SelectMany(p => p.PostProducts)
                    .Select(pp => pp.PostId)
                    .Distinct()
                    .ToList();
                var categoryClicks = clicks
                    .Where(c => categoryPpIds.Contains(c.PostProductId))
                    .ToList();

                return new CategoryBreakdown(
                    Category: g.Key,
                    ProductCount: g.Count(),
                    PostCount: categoryPostIds.Count,
                    Clicks: categoryClicks.Count,
                    Conversions: categoryClicks.Count(c => c.ConvertedAt != null)
                );
            })
            .OrderByDescending(c => c.Clicks)
            .ToList();

        return new RetailerAnalyticsResponse(summary, dailyMetrics, topProducts, categoryBreakdown);
    }

    public async Task<byte[]> ExportAnalyticsCsvAsync(
        Guid userId,
        RetailerAnalyticsRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var analytics = await GetAnalyticsAsync(userId, request, cancellationToken);

        var sb = new StringBuilder();

        // Summary section
        sb.AppendLine("Section,Metric,Value");
        sb.AppendLine($"Summary,Total Views,{analytics.Summary.TotalViews}");
        sb.AppendLine($"Summary,Total Clicks,{analytics.Summary.TotalClicks}");
        sb.AppendLine($"Summary,Total Conversions,{analytics.Summary.TotalConversions}");
        sb.AppendLine($"Summary,Total Revenue,{analytics.Summary.TotalRevenue}");
        sb.AppendLine($"Summary,Total Posts,{analytics.Summary.TotalPosts}");
        sb.AppendLine();

        // Daily metrics
        sb.AppendLine("Date,Views,Clicks,Conversions,Revenue");
        foreach (var day in analytics.DailyMetrics)
        {
            sb.AppendLine(
                $"{day.Date:yyyy-MM-dd},{day.Views},{day.Clicks},{day.Conversions},{day.Revenue}"
            );
        }
        sb.AppendLine();

        // Top products
        sb.AppendLine("Product ID,Product Name,Category,Views,Clicks,Conversions,Revenue");
        foreach (var product in analytics.TopProducts)
        {
            sb.AppendLine(
                $"{product.ProductId},{EscapeCsv(product.ProductName)},{EscapeCsv(product.Category)},{product.Views},{product.Clicks},{product.Conversions},{product.Revenue}"
            );
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    #endregion

    #region Insights

    public async Task<FitInsightsResponse> GetFitInsightsAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        var retailer = await GetRetailerByUserIdAsync(userId, cancellationToken);

        var productIds = await _dbContext
            .Set<Product>()
            .Where(p => p.RetailerId == retailer.Id)
            .Select(p => p.Id)
            .ToListAsync(cancellationToken);

        var postProducts = await _dbContext
            .Set<PostProduct>()
            .Where(pp => productIds.Contains(pp.ProductId) && pp.FitRating != null)
            .Include(pp => pp.Product)
            .ToListAsync(cancellationToken);

        var productGroups = postProducts
            .GroupBy(pp => pp.ProductId)
            .Select(g =>
            {
                var product = g.First().Product;
                var totalReviews = g.Count();

                // Fit distribution
                var fitDistribution = g.GroupBy(pp => pp.FitRating!.Value)
                    .Select(fg => new FitDistribution(
                        Rating: fg.Key.ToString(),
                        Count: fg.Count(),
                        Percentage: Math.Round((decimal)fg.Count() / totalReviews * 100, 1)
                    ))
                    .OrderBy(fd => fd.Rating)
                    .ToList();

                // Size breakdown
                var sizeBreakdown = g.GroupBy(pp => pp.SizeWorn)
                    .Select(sg =>
                    {
                        var avgFit = sg.Average(pp => (int)pp.FitRating!.Value);
                        var averageFitLabel = avgFit switch
                        {
                            < 1.5 => "Runs Small",
                            < 2.5 => "True to Size",
                            _ => "Runs Large",
                        };

                        return new SizeFitBreakdown(
                            Size: sg.Key,
                            ReviewCount: sg.Count(),
                            AverageFit: averageFitLabel
                        );
                    })
                    .OrderBy(s => s.Size)
                    .ToList();

                // Overall recommendation
                var avgOverall = g.Average(pp => (int)pp.FitRating!.Value);
                var fitRecommendation = avgOverall switch
                {
                    < 1.5 => "Runs Small",
                    < 2.5 => "True to Size",
                    _ => "Runs Large",
                };

                return new ProductFitSummary(
                    ProductId: product.Id,
                    ProductName: product.Name,
                    Category: product.Category,
                    TotalReviews: totalReviews,
                    FitRecommendation: fitRecommendation,
                    FitDistribution: fitDistribution,
                    SizeBreakdown: sizeBreakdown
                );
            })
            .OrderByDescending(p => p.TotalReviews)
            .ToList();

        return new FitInsightsResponse(productGroups);
    }

    public async Task<BodyProfileInsightsResponse> GetBodyProfileInsightsAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        var retailer = await GetRetailerByUserIdAsync(userId, cancellationToken);

        // Get post IDs that feature this retailer's products
        var productIds = await _dbContext
            .Set<Product>()
            .Where(p => p.RetailerId == retailer.Id)
            .Select(p => p.Id)
            .ToListAsync(cancellationToken);

        var postIds = await _dbContext
            .Set<PostProduct>()
            .Where(pp => productIds.Contains(pp.ProductId))
            .Select(pp => pp.PostId)
            .Distinct()
            .ToListAsync(cancellationToken);

        // Get unique users who engaged with these posts
        var engagedUserIds = await _dbContext
            .Set<Engagement>()
            .Where(e => postIds.Contains(e.PostId))
            .Select(e => e.UserId)
            .Distinct()
            .ToListAsync(cancellationToken);

        // Get body profiles for engaged users
        var bodyProfiles = await _dbContext
            .Set<BodyProfile>()
            .Where(bp => engagedUserIds.Contains(bp.UserId))
            .Include(bp => bp.BodyType)
            .ToListAsync(cancellationToken);

        var totalEngagedUsers = bodyProfiles.Count;
        var minGroupSize = _retailerSettings.MinAnonymityGroupSize;

        // Height bands with k-anonymity
        var heightBands = bodyProfiles
            .GroupBy(bp => GetHeightRange(bp.HeightCm))
            .Where(g => g.Count() >= minGroupSize)
            .Select(g => new InsightBand(
                Label: g.Key,
                Count: g.Count(),
                Percentage: totalEngagedUsers > 0
                    ? Math.Round((decimal)g.Count() / totalEngagedUsers * 100, 1)
                    : 0
            ))
            .OrderByDescending(b => b.Count)
            .ToList();

        // Weight bands with k-anonymity
        var weightBands = bodyProfiles
            .GroupBy(bp => GetWeightRange(bp.WeightKg))
            .Where(g => g.Count() >= minGroupSize)
            .Select(g => new InsightBand(
                Label: g.Key,
                Count: g.Count(),
                Percentage: totalEngagedUsers > 0
                    ? Math.Round((decimal)g.Count() / totalEngagedUsers * 100, 1)
                    : 0
            ))
            .OrderByDescending(b => b.Count)
            .ToList();

        // Body types with k-anonymity
        var bodyTypes = bodyProfiles
            .GroupBy(bp => bp.BodyType.Name)
            .Where(g => g.Count() >= minGroupSize)
            .Select(g => new InsightBand(
                Label: g.Key,
                Count: g.Count(),
                Percentage: totalEngagedUsers > 0
                    ? Math.Round((decimal)g.Count() / totalEngagedUsers * 100, 1)
                    : 0
            ))
            .OrderByDescending(b => b.Count)
            .ToList();

        // Fit preferences with k-anonymity
        var fitPreferences = bodyProfiles
            .Where(bp => bp.FitPreference.HasValue)
            .GroupBy(bp => bp.FitPreference!.Value.ToString())
            .Where(g => g.Count() >= minGroupSize)
            .Select(g => new InsightBand(
                Label: g.Key,
                Count: g.Count(),
                Percentage: totalEngagedUsers > 0
                    ? Math.Round((decimal)g.Count() / totalEngagedUsers * 100, 1)
                    : 0
            ))
            .OrderByDescending(b => b.Count)
            .ToList();

        return new BodyProfileInsightsResponse(
            HeightBands: heightBands,
            WeightBands: weightBands,
            BodyTypes: bodyTypes,
            FitPreferences: fitPreferences,
            MinimumGroupSize: minGroupSize,
            TotalEngagedUsers: totalEngagedUsers
        );
    }

    #endregion

    #region Private Methods

    private async Task<Retailer> GetRetailerByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken
    )
    {
        var retailer = await _dbContext
            .Set<Retailer>()
            .FirstOrDefaultAsync(r => r.UserId == userId, cancellationToken);

        if (retailer == null)
        {
            throw new NotFoundException("Retailer", userId);
        }

        return retailer;
    }

    private async Task<CampaignResponse> MapToCampaignResponseAsync(
        SponsoredPlacement campaign,
        CancellationToken cancellationToken
    )
    {
        // Load product name if needed
        string? productName = null;
        if (campaign.ProductId.HasValue)
        {
            productName =
                campaign.Product?.Name
                ?? (
                    await _dbContext
                        .Set<Product>()
                        .Where(p => p.Id == campaign.ProductId.Value)
                        .Select(p => p.Name)
                        .FirstOrDefaultAsync(cancellationToken)
                );
        }

        var status = ComputeCampaignStatus(campaign);

        return new CampaignResponse(
            Id: campaign.Id,
            ProductId: campaign.ProductId,
            ProductName: productName,
            BudgetAmount: campaign.BudgetAmount,
            SpentAmount: campaign.SpentAmount,
            Status: status,
            TargetBodyTypes: ParseJsonList(campaign.TargetBodyTypes),
            TargetCategories: ParseJsonList(campaign.TargetCategories),
            StartDate: campaign.StartDate,
            EndDate: campaign.EndDate,
            IsActive: campaign.IsActive,
            CreatedAt: campaign.CreatedAt
        );
    }

    private static string ComputeCampaignStatus(SponsoredPlacement campaign)
    {
        if (!campaign.IsActive)
            return "Paused";
        if (campaign.SpentAmount >= campaign.BudgetAmount)
            return "BudgetExhausted";
        if (DateTime.UtcNow < campaign.StartDate)
            return "Scheduled";
        if (DateTime.UtcNow > campaign.EndDate)
            return "Ended";
        return "Active";
    }

    private RetailerProductResponse MapToRetailerProductResponse(Product product)
    {
        var postCount = product.PostProducts.Count;
        var clickCount = product.PostProducts.SelectMany(pp => pp.ClickEvents).Count();
        var conversionCount = product
            .PostProducts.SelectMany(pp => pp.ClickEvents)
            .Count(ce => ce.ConvertedAt != null);

        return new RetailerProductResponse(
            Id: product.Id,
            ExternalSku: product.ExternalSku,
            Name: product.Name,
            Description: product.Description,
            Category: product.Category,
            SubCategory: product.SubCategory,
            ImageUrls: ParseJsonList(product.ImageUrls) ?? new List<string>(),
            ProductUrl: product.ProductUrl,
            Price: product.Price,
            Currency: product.Currency,
            IsActive: product.IsActive,
            PostCount: postCount,
            ClickCount: clickCount,
            ConversionCount: conversionCount,
            LastSyncedAt: product.LastSyncedAt,
            CreatedAt: product.CreatedAt
        );
    }

    private static List<string>? ParseJsonList(string? json)
    {
        if (string.IsNullOrEmpty(json))
            return null;

        try
        {
            return JsonSerializer.Deserialize<List<string>>(json, JsonOptions);
        }
        catch
        {
            return null;
        }
    }

    private static string[] ParseCsvLine(string line)
    {
        var fields = new List<string>();
        var current = new StringBuilder();
        var inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            var c = line[i];

            if (inQuotes)
            {
                if (c == '"')
                {
                    if (i + 1 < line.Length && line[i + 1] == '"')
                    {
                        current.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = false;
                    }
                }
                else
                {
                    current.Append(c);
                }
            }
            else
            {
                if (c == '"')
                {
                    inQuotes = true;
                }
                else if (c == ',')
                {
                    fields.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }
        }

        fields.Add(current.ToString());
        return fields.ToArray();
    }

    private static string EscapeCsv(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }
        return value;
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

    #endregion
}
