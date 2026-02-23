using System.Text.Json;
using Lyke.Application.Configuration;
using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Feed;
using Lyke.Application.DTOs.Profile;
using Lyke.Application.Interfaces;
using Lyke.Core.Entities;
using Lyke.Core.Enums;
using Lyke.Core.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lyke.Application.Services;

public class FeedService : IFeedService
{
    private readonly DbContext _dbContext;
    private readonly MatchingSettings _matchingSettings;
    private readonly ModerationSettings _moderationSettings;
    private readonly IEventTrackingService _eventTracking;
    private readonly ILogger<FeedService> _logger;
    private bool UseFullTextSearch => _dbContext.Database.ProviderName == "Npgsql.EntityFrameworkCore.PostgreSQL";

    public FeedService(
        DbContext dbContext,
        IOptions<MatchingSettings> matchingSettings,
        IOptions<ModerationSettings> moderationSettings,
        IEventTrackingService eventTracking,
        ILogger<FeedService> logger
    )
    {
        _dbContext = dbContext;
        _matchingSettings = matchingSettings.Value;
        _moderationSettings = moderationSettings.Value;
        _eventTracking = eventTracking;
        _logger = logger;
    }

    public async Task<(IReadOnlyList<FeedPostResponse> Posts, PaginationMeta Meta)> GetFeedAsync(
        Guid userId,
        FeedRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var userBodyProfile = await _dbContext
            .Set<BodyProfile>()
            .AsNoTracking()
            .FirstOrDefaultAsync(bp => bp.UserId == userId, cancellationToken);

        var query = BuildFeedQuery(request);

        // For Recent and MostLiked, push sorting + pagination to DB
        if (request.SortBy == FeedSortBy.Recent || request.SortBy == FeedSortBy.MostLiked)
        {
            var totalCount = await query.CountAsync(cancellationToken);

            var orderedQuery = request.SortBy == FeedSortBy.MostLiked
                ? query.OrderByDescending(p => p.Engagements.Count(e => e.Type == EngagementType.Like))
                    .ThenByDescending(p => p.PublishedAt)
                : query.OrderByDescending(p => p.PublishedAt) as IOrderedQueryable<Post>;

            var posts = await orderedQuery
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Include(p => p.Creator)
                .ThenInclude(c => c.User)
                .ThenInclude(u => u.BodyProfile)
                .ThenInclude(bp => bp!.BodyType)
                .Include(p => p.PostProducts)
                .ThenInclude(pp => pp.Product)
                .ThenInclude(prod => prod.Retailer)
                .Include(p => p.PostProducts)
                .ThenInclude(pp => pp.FitTags)
                .ThenInclude(pft => pft.FitTag)
                .Include(p => p.Engagements)
                .AsSplitQuery()
                .ToListAsync(cancellationToken);

            var postIds = posts.Select(p => p.Id).ToList();
            var userEngagements = await GetUserEngagementsAsync(userId, postIds, cancellationToken);

            var feedPosts = posts
                .Select(p => MapToFeedPostResponse(
                    p,
                    CalculateSimilarityScore(userBodyProfile, p.Creator.User.BodyProfile),
                    userEngagements))
                .ToList();

            var meta = new PaginationMeta
            {
                Page = request.Page,
                PageSize = request.PageSize,
                TotalCount = totalCount,
            };

            return (feedPosts, meta);
        }

        // Relevance sort: lightweight projection to score, then fetch full data for page only
        var lightweightPosts = await query
            .Include(p => p.Creator)
            .ThenInclude(c => c.User)
            .ThenInclude(u => u.BodyProfile)
            .Select(p => new
            {
                p.Id,
                p.PublishedAt,
                CreatorBodyProfile = p.Creator.User.BodyProfile,
            })
            .ToListAsync(cancellationToken);

        var allScored = lightweightPosts
            .Select(p => new
            {
                p.Id,
                Score = CalculateSimilarityScore(userBodyProfile, p.CreatorBodyProfile) * 0.6
                    + CalculateRecencyScore(p.PublishedAt) * 0.4,
            })
            .OrderByDescending(x => x.Score)
            .ToList();

        var relevanceTotalCount = allScored.Count;
        var pagedIds = allScored
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var pagedPostIds = pagedIds.Select(x => x.Id).ToList();
        var scoreLookup = pagedIds.ToDictionary(x => x.Id, x => x.Score);

        // Fetch full data only for the page
        var fullPosts = await _dbContext.Set<Post>()
            .AsNoTracking()
            .Where(p => pagedPostIds.Contains(p.Id))
            .Include(p => p.Creator)
            .ThenInclude(c => c.User)
            .ThenInclude(u => u.BodyProfile)
            .ThenInclude(bp => bp!.BodyType)
            .Include(p => p.PostProducts)
            .ThenInclude(pp => pp.Product)
            .ThenInclude(prod => prod.Retailer)
            .Include(p => p.PostProducts)
            .ThenInclude(pp => pp.FitTags)
            .ThenInclude(pft => pft.FitTag)
            .Include(p => p.Engagements)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);

        var relevanceUserEngagements = await GetUserEngagementsAsync(userId, pagedPostIds, cancellationToken);

        // Maintain score ordering
        var orderedFullPosts = pagedPostIds
            .Select(id => fullPosts.FirstOrDefault(p => p.Id == id))
            .Where(p => p != null)
            .ToList();

        var relevanceFeedPosts = orderedFullPosts
            .Select(p => MapToFeedPostResponse(
                p!,
                scoreLookup.GetValueOrDefault(p!.Id, 0),
                relevanceUserEngagements))
            .ToList();

        var relevanceMeta = new PaginationMeta
        {
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = relevanceTotalCount,
        };

        return (relevanceFeedPosts, relevanceMeta);
    }

    public async Task<(
        IReadOnlyList<FeedPostResponse> Posts,
        PaginationMeta Meta
    )> GetExploreFeedAsync(
        Guid? userId,
        FeedRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var query = BuildFeedQuery(request);

        var totalCount = await query.CountAsync(cancellationToken);

        // Push sorting + pagination to DB
        var orderedQuery = request.SortBy switch
        {
            FeedSortBy.MostLiked => query
                .OrderByDescending(p => p.Engagements.Count(e => e.Type == EngagementType.Like))
                .ThenByDescending(p => p.PublishedAt),
            _ => query.OrderByDescending(p => p.PublishedAt) as IOrderedQueryable<Post>,
        };

        var posts = await orderedQuery
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Include(p => p.Creator)
            .ThenInclude(c => c.User)
            .ThenInclude(u => u.BodyProfile)
            .ThenInclude(bp => bp!.BodyType)
            .Include(p => p.PostProducts)
            .ThenInclude(pp => pp.Product)
            .ThenInclude(prod => prod.Retailer)
            .Include(p => p.PostProducts)
            .ThenInclude(pp => pp.FitTags)
            .ThenInclude(pft => pft.FitTag)
            .Include(p => p.Engagements)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);

        var postIds = posts.Select(p => p.Id).ToList();
        var userEngagements = userId.HasValue
            ? await GetUserEngagementsAsync(userId.Value, postIds, cancellationToken)
            : new Dictionary<Guid, HashSet<EngagementType>>();

        var feedPosts = posts.Select(p => MapToFeedPostResponse(p, 0, userEngagements)).ToList();

        var meta = new PaginationMeta
        {
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount,
        };

        return (feedPosts, meta);
    }

    public async Task<PostDetailResponse> GetPostAsync(
        Guid postId,
        Guid? userId,
        CancellationToken cancellationToken = default
    )
    {
        var post = await _dbContext
            .Set<Post>()
            .AsNoTracking()
            .Include(p => p.Creator)
            .ThenInclude(c => c.User)
            .ThenInclude(u => u.BodyProfile)
            .ThenInclude(bp => bp!.BodyType)
            .Include(p => p.PostProducts)
            .ThenInclude(pp => pp.Product)
            .ThenInclude(prod => prod.Retailer)
            .Include(p => p.PostProducts)
            .ThenInclude(pp => pp.FitTags)
            .ThenInclude(pft => pft.FitTag)
            .Include(p => p.Engagements)
            .AsSplitQuery()
            .FirstOrDefaultAsync(
                p => p.Id == postId && p.Status == PostStatus.Published,
                cancellationToken
            );

        if (post == null)
        {
            throw new NotFoundException(nameof(Post), postId);
        }

        // Get creator's published post count separately to avoid no-tracking cycle
        var creatorPublishedPostCount = await _dbContext
            .Set<Post>()
            .CountAsync(
                p => p.CreatorId == post.CreatorId && p.Status == PostStatus.Published,
                cancellationToken
            );

        // Calculate similarity if user has a body profile
        double similarityScore = 0;
        if (userId.HasValue)
        {
            var userBodyProfile = await _dbContext
                .Set<BodyProfile>()
                .FirstOrDefaultAsync(bp => bp.UserId == userId.Value, cancellationToken);
            similarityScore = CalculateSimilarityScore(
                userBodyProfile,
                post.Creator.User.BodyProfile
            );
        }

        var userEngagements = userId.HasValue
            ? await GetUserEngagementsAsync(userId.Value, new[] { postId }, cancellationToken)
            : new Dictionary<Guid, HashSet<EngagementType>>();

        return MapToPostDetailResponse(post, similarityScore, userEngagements, creatorPublishedPostCount);
    }

    public async Task<IReadOnlyList<FeedPostResponse>> GetSimilarPostsAsync(
        Guid postId,
        Guid? userId,
        int limit = 10,
        CancellationToken cancellationToken = default
    )
    {
        var post = await _dbContext
            .Set<Post>()
            .AsNoTracking()
            .Include(p => p.Creator)
            .ThenInclude(c => c.User)
            .ThenInclude(u => u.BodyProfile)
            .Include(p => p.PostProducts)
            .ThenInclude(pp => pp.Product)
            .AsSplitQuery()
            .FirstOrDefaultAsync(p => p.Id == postId, cancellationToken);

        if (post == null)
        {
            throw new NotFoundException(nameof(Post), postId);
        }

        var creatorBodyProfile = post.Creator.User.BodyProfile;
        var productCategories = post
            .PostProducts.Select(pp => pp.Product.Category)
            .Distinct()
            .ToList();

        // Lightweight projection for scoring — no full entity graph
        var lightweightSimilar = await _dbContext
            .Set<Post>()
            .AsNoTracking()
            .Where(p => p.Id != postId && p.Status == PostStatus.Published)
            .Include(p => p.Creator)
            .ThenInclude(c => c.User)
            .ThenInclude(u => u.BodyProfile)
            .Include(p => p.PostProducts)
            .ThenInclude(pp => pp.Product)
            .Select(p => new
            {
                p.Id,
                CreatorBodyProfile = p.Creator.User.BodyProfile,
                HasMatchingCategory = p.PostProducts.Any(pp => productCategories.Contains(pp.Product.Category)),
            })
            .ToListAsync(cancellationToken);

        var topIds = lightweightSimilar
            .Select(p => new
            {
                p.Id,
                Score = CalculateSimilarityScore(creatorBodyProfile, p.CreatorBodyProfile) * 0.7
                    + (p.HasMatchingCategory ? 0.3 : 0),
            })
            .OrderByDescending(x => x.Score)
            .Take(limit)
            .ToList();

        var topPostIds = topIds.Select(x => x.Id).ToList();
        var scoreLookup = topIds.ToDictionary(x => x.Id, x => x.Score);

        // Fetch full data only for top N
        var fullPosts = await _dbContext
            .Set<Post>()
            .AsNoTracking()
            .Where(p => topPostIds.Contains(p.Id))
            .Include(p => p.Creator)
            .ThenInclude(c => c.User)
            .ThenInclude(u => u.BodyProfile)
            .ThenInclude(bp => bp!.BodyType)
            .Include(p => p.PostProducts)
            .ThenInclude(pp => pp.Product)
            .ThenInclude(prod => prod.Retailer)
            .Include(p => p.PostProducts)
            .ThenInclude(pp => pp.FitTags)
            .ThenInclude(pft => pft.FitTag)
            .Include(p => p.Engagements)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);

        var userEngagements = userId.HasValue
            ? await GetUserEngagementsAsync(userId.Value, topPostIds, cancellationToken)
            : new Dictionary<Guid, HashSet<EngagementType>>();

        // Maintain score ordering
        return topPostIds
            .Select(id => fullPosts.FirstOrDefault(p => p.Id == id))
            .Where(p => p != null)
            .Select(p => MapToFeedPostResponse(p!, scoreLookup.GetValueOrDefault(p!.Id, 0), userEngagements))
            .ToList();
    }

    public async Task EngageAsync(
        Guid postId,
        Guid userId,
        EngageRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var post = await _dbContext
            .Set<Post>()
            .FirstOrDefaultAsync(
                p => p.Id == postId && p.Status == PostStatus.Published,
                cancellationToken
            );

        if (post == null)
        {
            throw new NotFoundException(nameof(Post), postId);
        }

        var engagements = _dbContext.Set<Engagement>();

        // Check if engagement already exists (for Like/Save)
        if (request.Type == EngagementType.Like || request.Type == EngagementType.Save)
        {
            var existing = await engagements.FirstOrDefaultAsync(
                e => e.PostId == postId && e.UserId == userId && e.Type == request.Type,
                cancellationToken
            );

            if (existing != null)
            {
                return; // Already engaged
            }
        }

        var engagement = new Engagement
        {
            Id = Guid.NewGuid(),
            PostId = postId,
            UserId = userId,
            Type = request.Type,
            CreatedAt = DateTime.UtcNow,
        };

        await engagements.AddAsync(engagement, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var analyticsType = request.Type switch
        {
            EngagementType.View => AnalyticsEventType.PostView,
            EngagementType.Like => AnalyticsEventType.PostLike,
            EngagementType.Save => AnalyticsEventType.PostSave,
            EngagementType.Share => AnalyticsEventType.PostShare,
            _ => AnalyticsEventType.PostView
        };
        _ = _eventTracking.TrackAsync(analyticsType, userId, postId, nameof(Post), cancellationToken: cancellationToken);

        _logger.LogInformation(
            "User {UserId} engaged with post {PostId}: {Type}",
            userId,
            postId,
            request.Type
        );
    }

    public async Task RemoveEngagementAsync(
        Guid postId,
        Guid userId,
        EngageRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var engagements = _dbContext.Set<Engagement>();

        var engagement = await engagements.FirstOrDefaultAsync(
            e => e.PostId == postId && e.UserId == userId && e.Type == request.Type,
            cancellationToken
        );

        if (engagement != null)
        {
            engagements.Remove(engagement);
            await _dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation(
                "User {UserId} removed engagement from post {PostId}: {Type}",
                userId,
                postId,
                request.Type
            );
        }
    }

    public async Task<(
        IReadOnlyList<FeedPostResponse> Posts,
        PaginationMeta Meta
    )> GetSavedPostsAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    )
    {
        var savedQuery = _dbContext
            .Set<Engagement>()
            .AsNoTracking()
            .Where(e => e.UserId == userId && e.Type == EngagementType.Save);

        var totalCount = await savedQuery.CountAsync(cancellationToken);

        var pagedPostIds = await savedQuery
            .OrderByDescending(e => e.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(e => e.PostId)
            .ToListAsync(cancellationToken);

        var posts = await _dbContext
            .Set<Post>()
            .AsNoTracking()
            .Where(p => pagedPostIds.Contains(p.Id) && p.Status == PostStatus.Published)
            .Include(p => p.Creator)
            .ThenInclude(c => c.User)
            .ThenInclude(u => u.BodyProfile)
            .ThenInclude(bp => bp!.BodyType)
            .Include(p => p.PostProducts)
            .ThenInclude(pp => pp.Product)
            .ThenInclude(prod => prod.Retailer)
            .Include(p => p.PostProducts)
            .ThenInclude(pp => pp.FitTags)
            .ThenInclude(pft => pft.FitTag)
            .Include(p => p.Engagements)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);

        // Maintain order from savedPostIds
        var orderedPosts = pagedPostIds
            .Select(id => posts.FirstOrDefault(p => p.Id == id))
            .Where(p => p != null)
            .ToList();

        var userEngagements = await GetUserEngagementsAsync(
            userId,
            pagedPostIds,
            cancellationToken
        );

        var feedPosts = orderedPosts
            .Select(p => MapToFeedPostResponse(p!, 0, userEngagements))
            .ToList();

        var meta = new PaginationMeta
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
        };

        return (feedPosts, meta);
    }

    public async Task<SearchResponse> SearchAsync(
        SearchRequest request,
        Guid? userId,
        CancellationToken cancellationToken = default
    )
    {
        var searchTerm = request.Query;
        var posts = new List<FeedPostResponse>();
        var products = new List<ProductSearchResult>();
        var creators = new List<CreatorSearchResult>();

        // Search posts
        if (
            request.Type == null
            || request.Type == SearchType.All
            || request.Type == SearchType.Posts
        )
        {
            var postBaseQuery = _dbContext
                .Set<Post>()
                .Where(p => p.Status == PostStatus.Published);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                postBaseQuery = UseFullTextSearch
                    ? postBaseQuery.Where(p =>
                        EF.Functions.ToTsVector("english", (p.Title ?? "") + " " + (p.Description ?? ""))
                            .Matches(EF.Functions.PlainToTsQuery("english", searchTerm)))
                    : postBaseQuery.Where(p =>
                        (p.Title != null && p.Title.ToLower().Contains(searchTerm.ToLower()))
                        || (p.Description != null && p.Description.ToLower().Contains(searchTerm.ToLower())));
            }

            var postQuery = postBaseQuery
                .Include(p => p.Creator)
                .ThenInclude(c => c.User)
                .ThenInclude(u => u.BodyProfile)
                .ThenInclude(bp => bp!.BodyType)
                .Include(p => p.PostProducts)
                .ThenInclude(pp => pp.Product)
                .ThenInclude(prod => prod.Retailer)
                .Include(p => p.PostProducts)
                .ThenInclude(pp => pp.FitTags)
                .ThenInclude(pft => pft.FitTag)
                .Include(p => p.Engagements)
                .AsSplitQuery();

            var postOrdered = UseFullTextSearch && !string.IsNullOrWhiteSpace(searchTerm)
                ? postQuery
                    .OrderByDescending(p =>
                        EF.Functions.ToTsVector("english", (p.Title ?? "") + " " + (p.Description ?? ""))
                            .Rank(EF.Functions.PlainToTsQuery("english", searchTerm)))
                    .ThenByDescending(p => p.PublishedAt)
                : postQuery.OrderByDescending(p => p.PublishedAt);

            var finalPostQuery = postOrdered.Take(request.PageSize);

            var foundPosts = await finalPostQuery.ToListAsync(cancellationToken);
            var postIds = foundPosts.Select(p => p.Id).ToList();
            var userEngagements = userId.HasValue
                ? await GetUserEngagementsAsync(userId.Value, postIds, cancellationToken)
                : new Dictionary<Guid, HashSet<EngagementType>>();

            posts = foundPosts.Select(p => MapToFeedPostResponse(p, 0, userEngagements)).ToList();
        }

        // Search products
        if (
            request.Type == null
            || request.Type == SearchType.All
            || request.Type == SearchType.Products
        )
        {
            var productBaseQuery = _dbContext
                .Set<Product>()
                .Where(p => p.IsActive);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                productBaseQuery = UseFullTextSearch
                    ? productBaseQuery.Where(p =>
                        EF.Functions.ToTsVector("english", p.Name + " " + (p.Description ?? ""))
                            .Matches(EF.Functions.PlainToTsQuery("english", searchTerm)))
                    : productBaseQuery.Where(p =>
                        p.Name.ToLower().Contains(searchTerm.ToLower())
                        || (p.Description != null && p.Description.ToLower().Contains(searchTerm.ToLower())));
            }

            var productIncluded = productBaseQuery
                .Include(p => p.Retailer)
                .Include(p => p.PostProducts.Where(pp => pp.Post.Status == PostStatus.Published));

            var productOrdered = UseFullTextSearch && !string.IsNullOrWhiteSpace(searchTerm)
                ? productIncluded
                    .OrderByDescending(p =>
                        EF.Functions.ToTsVector("english", p.Name + " " + (p.Description ?? ""))
                            .Rank(EF.Functions.PlainToTsQuery("english", searchTerm)))
                    .ThenBy(p => p.Name)
                : productIncluded.OrderBy(p => p.Name);

            var productQuery = productOrdered.Take(request.PageSize);

            var foundProducts = await productQuery.ToListAsync(cancellationToken);
            products = foundProducts
                .Select(p => new ProductSearchResult(
                    p.Id,
                    p.Name,
                    ParseMediaUrls(p.ImageUrls).FirstOrDefault(),
                    p.Price,
                    p.Currency,
                    p.Retailer.Name,
                    p.PostProducts.Count
                ))
                .ToList();
        }

        // Search creators
        if (
            request.Type == null
            || request.Type == SearchType.All
            || request.Type == SearchType.Creators
        )
        {
            var creatorBaseQuery = _dbContext.Set<Creator>().AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                creatorBaseQuery = UseFullTextSearch
                    ? creatorBaseQuery.Where(c =>
                        EF.Functions.ToTsVector("english", c.DisplayName + " " + (c.Bio ?? ""))
                            .Matches(EF.Functions.PlainToTsQuery("english", searchTerm)))
                    : creatorBaseQuery.Where(c =>
                        c.DisplayName.ToLower().Contains(searchTerm.ToLower())
                        || (c.Bio != null && c.Bio.ToLower().Contains(searchTerm.ToLower())));
            }

            var creatorIncluded = creatorBaseQuery
                .Include(c => c.User)
                .Include(c => c.Posts);

            var creatorOrdered = UseFullTextSearch && !string.IsNullOrWhiteSpace(searchTerm)
                ? creatorIncluded
                    .OrderByDescending(c =>
                        EF.Functions.ToTsVector("english", c.DisplayName + " " + (c.Bio ?? ""))
                            .Rank(EF.Functions.PlainToTsQuery("english", searchTerm)))
                    .ThenBy(c => c.DisplayName)
                : creatorIncluded.OrderBy(c => c.DisplayName);

            var creatorQuery = creatorOrdered.Take(request.PageSize);

            var foundCreators = await creatorQuery.ToListAsync(cancellationToken);
            creators = foundCreators
                .Select(c => new CreatorSearchResult(
                    c.Id,
                    c.DisplayName,
                    c.IsVerified,
                    c.User.ProfileImageUrl,
                    c.Posts.Count(p => p.Status == PostStatus.Published)
                ))
                .ToList();
        }

        _ = _eventTracking.TrackAsync(
            AnalyticsEventType.SearchExecute,
            userId,
            properties: new Dictionary<string, string>
            {
                ["query"] = request.Query ?? "",
                ["type"] = request.Type?.ToString() ?? "All",
                ["resultCount"] = (posts.Count + products.Count + creators.Count).ToString()
            },
            cancellationToken: cancellationToken);

        return new SearchResponse(
            posts,
            products,
            creators,
            posts.Count + products.Count + creators.Count
        );
    }

    public async Task ReportPostAsync(
        Guid postId,
        Guid userId,
        CreateContentReportRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var post = await _dbContext
            .Set<Post>()
            .FirstOrDefaultAsync(
                p => p.Id == postId && (p.Status == PostStatus.Published || p.Status == PostStatus.Flagged),
                cancellationToken
            );

        if (post == null)
        {
            throw new NotFoundException(nameof(Post), postId);
        }

        // Check for duplicate report
        var existing = await _dbContext
            .Set<ContentReport>()
            .FirstOrDefaultAsync(
                cr => cr.PostId == postId && cr.ReportedByUserId == userId,
                cancellationToken
            );

        if (existing != null)
        {
            throw new ValidationException("Report", "You have already reported this post");
        }

        var report = new ContentReport
        {
            Id = Guid.NewGuid(),
            PostId = postId,
            ReportedByUserId = userId,
            Reason = request.Reason,
            AdditionalDetails = request.AdditionalDetails,
            Status = ReportStatus.Pending,
        };

        await _dbContext.Set<ContentReport>().AddAsync(report, cancellationToken);

        // Auto-flag if threshold reached
        var reportCount = await _dbContext
            .Set<ContentReport>()
            .CountAsync(cr => cr.PostId == postId, cancellationToken) + 1; // +1 for the new report

        if (reportCount >= _moderationSettings.AutoFlagThreshold && post.Status == PostStatus.Published)
        {
            post.Status = PostStatus.Flagged;
            _logger.LogInformation(
                "Post {PostId} auto-flagged after {ReportCount} reports",
                postId,
                reportCount
            );
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "User {UserId} reported post {PostId} for {Reason}",
            userId,
            postId,
            request.Reason
        );
    }

    #region Private Methods

    private IQueryable<Post> BuildFeedQuery(FeedRequest request)
    {
        var query = _dbContext.Set<Post>().AsNoTracking().Where(p => p.Status == PostStatus.Published);

        if (!string.IsNullOrEmpty(request.Category))
        {
            query = query.Where(p =>
                p.PostProducts.Any(pp => pp.Product.Category == request.Category)
            );
        }

        if (request.RetailerId.HasValue)
        {
            query = query.Where(p =>
                p.PostProducts.Any(pp => pp.Product.RetailerId == request.RetailerId.Value)
            );
        }

        if (request.FitTagIds != null && request.FitTagIds.Any())
        {
            query = query.Where(p =>
                p.PostProducts.Any(pp =>
                    pp.FitTags.Any(ft => request.FitTagIds.Contains(ft.FitTagId))
                )
            );
        }

        if (request.CreatorId.HasValue)
        {
            query = query.Where(p => p.CreatorId == request.CreatorId.Value);
        }

        return query;
    }

    private double CalculateSimilarityScore(BodyProfile? userProfile, BodyProfile? creatorProfile)
    {
        if (userProfile == null || creatorProfile == null)
        {
            return 0.5; // Neutral score if no profile
        }

        // Height similarity (0-1)
        var heightDiff = Math.Abs(userProfile.HeightCm - creatorProfile.HeightCm);
        var heightScore = Math.Max(
            0,
            1 - (heightDiff / (double)(_matchingSettings.HeightToleranceCm * 4))
        );

        // Weight similarity (0-1)
        var weightDiff = Math.Abs(userProfile.WeightKg - creatorProfile.WeightKg);
        var weightScore = Math.Max(
            0,
            1 - ((double)weightDiff / (double)(_matchingSettings.WeightToleranceKg * 4))
        );

        // Body type similarity (0 or 1)
        var bodyTypeScore = userProfile.BodyTypeId == creatorProfile.BodyTypeId ? 1.0 : 0.3;

        // Weighted average
        var score =
            (heightScore * _matchingSettings.HeightWeight)
            + (weightScore * _matchingSettings.WeightWeight)
            + (bodyTypeScore * _matchingSettings.BodyTypeWeight);

        return Math.Round(score, 2);
    }

    private double CalculateRecencyScore(DateTime? publishedAt)
    {
        if (!publishedAt.HasValue)
        {
            return 0;
        }

        var daysSincePublished = (DateTime.UtcNow - publishedAt.Value).TotalDays;
        if (daysSincePublished <= 0)
            return 1;
        if (daysSincePublished >= _matchingSettings.RecencyDecayDays)
            return 0.1;

        return 1 - (daysSincePublished / _matchingSettings.RecencyDecayDays * 0.9);
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

    private FeedPostResponse MapToFeedPostResponse(
        Post post,
        double similarityScore,
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
                creatorBodyProfile.FitPreference
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
            similarityScore,
            userPostEngagements.Contains(EngagementType.Like),
            userPostEngagements.Contains(EngagementType.Save),
            post.PublishedAt ?? post.CreatedAt
        );
    }

    private PostDetailResponse MapToPostDetailResponse(
        Post post,
        double similarityScore,
        Dictionary<Guid, HashSet<EngagementType>> userEngagements,
        int? creatorPublishedPostCount = null
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
                creatorBodyProfile.FitPreference
            );
        }

        var creator = new CreatorDetailResponse(
            post.Creator.Id,
            post.Creator.DisplayName,
            post.Creator.Bio,
            post.Creator.IsVerified,
            anonymizedProfile,
            creatorPublishedPostCount ?? post.Creator.Posts?.Count(p => p.Status == PostStatus.Published) ?? 0,
            post.Creator.User.ProfileImageUrl
        );

        var products = post
            .PostProducts.Select(pp => new PostProductDetailResponse(
                pp.Id,
                pp.ProductId,
                pp.Product.Name,
                pp.Product.Description,
                ParseMediaUrls(pp.Product.ImageUrls),
                pp.Product.ProductUrl,
                pp.Product.Price,
                pp.Product.Currency,
                pp.Product.Retailer.Name,
                pp.SizeWorn,
                pp.FitRating,
                pp.FitNotes,
                pp.StylingNotes,
                pp.FitTags.Select(ft => new FitTagResponse(
                        ft.FitTag.Id,
                        ft.FitTag.Name,
                        ft.FitTag.Category
                    ))
                    .ToList()
            ))
            .ToList();

        return new PostDetailResponse(
            post.Id,
            post.Title,
            post.Description,
            post.MediaType,
            ParseMediaUrls(post.MediaUrls),
            ParseMediaUrls(post.ThumbnailUrls),
            creator,
            products,
            engagementCounts,
            similarityScore,
            userPostEngagements.Contains(EngagementType.Like),
            userPostEngagements.Contains(EngagementType.Save),
            post.PublishedAt ?? post.CreatedAt,
            post.CreatedAt
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
