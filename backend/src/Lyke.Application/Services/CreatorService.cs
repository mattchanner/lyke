using System.Text.Json;
using Lyke.Application.Configuration;
using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Creator;
using Lyke.Application.DTOs.Feed;
using Lyke.Application.DTOs.Profile;
using Lyke.Application.Helpers;
using Lyke.Application.Interfaces;
using Lyke.Core.Entities;
using Lyke.Core.Enums;
using Lyke.Core.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lyke.Application.Services;

public class CreatorService : ICreatorService
{
    private readonly DbContext _dbContext;
    private readonly UserManager<User> _userManager;
    private readonly CreatorSettings _creatorSettings;
    private readonly IEmailService _emailService;
    private readonly ILogger<CreatorService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public CreatorService(
        DbContext dbContext,
        UserManager<User> userManager,
        IOptions<CreatorSettings> creatorSettings,
        IEmailService emailService,
        ILogger<CreatorService> logger
    )
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _creatorSettings = creatorSettings.Value;
        _emailService = emailService;
        _logger = logger;
    }

    #region Registration & Profile

    public async Task<CreatorProfileResponse> RegisterAsCreatorAsync(
        Guid userId,
        RegisterCreatorRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            throw new NotFoundException(nameof(User), userId);
        }

        var creators = _dbContext.Set<Creator>();

        // Check if already a creator
        var existingCreator = await creators.FirstOrDefaultAsync(
            c => c.UserId == userId,
            cancellationToken
        );
        if (existingCreator != null)
        {
            throw new ValidationException("Creator", "User is already registered as a creator");
        }

        var creator = new Creator
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            DisplayName = request.DisplayName,
            Bio = request.Bio,
            IsVerified = false,
            SocialLinks =
                request.SocialLinks != null
                    ? JsonSerializer.Serialize(request.SocialLinks, JsonOptions)
                    : null,
        };

        await creators.AddAsync(creator, cancellationToken);

        // Update user type to Creator
        user.UserType = UserType.Creator;
        await _userManager.UpdateAsync(user);

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "User {UserId} registered as creator {CreatorId}",
            userId,
            creator.Id
        );

        return await GetCreatorProfileAsync(userId, cancellationToken);
    }

    public async Task<CreatorProfileResponse> GetCreatorProfileAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        var creator = await GetCreatorByUserIdAsync(userId, cancellationToken);

        var postCounts = await _dbContext
            .Set<Post>()
            .Where(p => p.CreatorId == creator.Id)
            .GroupBy(p => p.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var totalPosts = postCounts.Sum(x => x.Count);
        var publishedPosts =
            postCounts.FirstOrDefault(x => x.Status == PostStatus.Published)?.Count ?? 0;
        var draftPosts = postCounts.FirstOrDefault(x => x.Status == PostStatus.Draft)?.Count ?? 0;
        var pendingPosts =
            postCounts.FirstOrDefault(x => x.Status == PostStatus.PendingReview)?.Count ?? 0;

        return new CreatorProfileResponse(
            Id: creator.Id,
            DisplayName: creator.DisplayName,
            Bio: creator.Bio,
            IsVerified: creator.IsVerified,
            VerificationStatus: creator.VerificationStatus,
            SocialLinks: ParseSocialLinks(creator.SocialLinks),
            TotalPosts: totalPosts,
            PublishedPosts: publishedPosts,
            DraftPosts: draftPosts,
            PendingReviewPosts: pendingPosts,
            CreatedAt: creator.CreatedAt
        );
    }

    public async Task<CreatorProfileResponse> UpdateCreatorProfileAsync(
        Guid userId,
        UpdateCreatorProfileRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var creator = await GetCreatorByUserIdAsync(userId, cancellationToken);

        if (!string.IsNullOrEmpty(request.DisplayName))
        {
            creator.DisplayName = request.DisplayName;
        }

        if (request.Bio != null)
        {
            creator.Bio = request.Bio;
        }

        if (request.SocialLinks != null)
        {
            creator.SocialLinks = JsonSerializer.Serialize(request.SocialLinks, JsonOptions);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Creator {CreatorId} profile updated", creator.Id);

        return await GetCreatorProfileAsync(userId, cancellationToken);
    }

    public async Task<PublicCreatorProfileResponse> GetPublicCreatorProfileAsync(
        Guid creatorId,
        CancellationToken cancellationToken = default
    )
    {
        var creator = await _dbContext
            .Set<Creator>()
            .AsNoTracking()
            .Include(c => c.User)
                .ThenInclude(u => u.BodyProfile)
                    .ThenInclude(bp => bp!.BodyType)
            .Include(c => c.User)
                .ThenInclude(u => u.BodyProfile)
                    .ThenInclude(bp => bp!.FrameSize)
            .Include(c => c.User)
                .ThenInclude(u => u.BodyProfile)
                    .ThenInclude(bp => bp!.FitPreferences)
            .AsSplitQuery()
            .FirstOrDefaultAsync(c => c.Id == creatorId, cancellationToken);

        if (creator == null)
            throw new NotFoundException("Creator", creatorId);

        var publishedPosts = await _dbContext
            .Set<Post>()
            .CountAsync(
                p => p.CreatorId == creatorId && p.Status == PostStatus.Published,
                cancellationToken
            );

        AnonymizedBodyProfileResponse? bodyProfile = null;
        var bp = creator.User.BodyProfile;
        if (bp?.BodyType != null)
        {
            bodyProfile = new AnonymizedBodyProfileResponse(
                HeightRange: BodyProfileHelper.GetHeightRange(bp.HeightCm),
                WeightRange: BodyProfileHelper.GetWeightRange(bp.WeightKg),
                BodyTypeName: bp.BodyType.Name,
                FrameSizeName: bp.FrameSize?.Name,
                Stature: bp.Stature,
                Build: bp.Build,
                BodyTypeLabel: BodyProfileHelper.FormatBodyTypeLabel(
                    bp.Stature,
                    bp.Build,
                    bp.BodyType.Name
                ),
                FitPreferences: bp.FitPreferences.Select(fp => fp.FitPreference).ToList()
            );
        }

        return new PublicCreatorProfileResponse(
            Id: creator.Id,
            DisplayName: creator.DisplayName,
            Bio: creator.Bio,
            IsVerified: creator.IsVerified,
            BodyProfile: bodyProfile,
            ProfileImageUrl: creator.User.ProfileImageUrl,
            PublishedPosts: publishedPosts,
            CreatedAt: creator.CreatedAt
        );
    }

    #endregion

    #region Verification (Creator)

    public async Task<VerificationStatusResponse> GetVerificationStatusAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        var creator = await GetCreatorByUserIdAsync(userId, cancellationToken);

        return new VerificationStatusResponse(
            Status: creator.VerificationStatus,
            Notes: creator.VerificationNotes,
            DocumentUrls: ParseDocumentUrls(creator.VerificationDocumentUrls),
            RequestedAt: creator.VerificationRequestedAt,
            ReviewedAt: creator.VerificationReviewedAt,
            RejectionReason: creator.VerificationRejectionReason
        );
    }

    public async Task<VerificationStatusResponse> SubmitVerificationAsync(
        Guid userId,
        SubmitVerificationRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var creator = await GetCreatorByUserIdAsync(userId, cancellationToken);

        if (creator.VerificationStatus == VerificationStatus.Pending)
        {
            throw new ValidationException(
                "Verification",
                "A verification request is already pending"
            );
        }

        if (creator.VerificationStatus == VerificationStatus.Approved)
        {
            throw new ValidationException("Verification", "Creator is already verified");
        }

        creator.VerificationStatus = VerificationStatus.Pending;
        creator.VerificationNotes = request.Notes;
        creator.VerificationDocumentUrls = JsonSerializer.Serialize(
            request.DocumentUrls,
            JsonOptions
        );
        creator.VerificationRequestedAt = DateTime.UtcNow;
        creator.VerificationReviewedAt = null;
        creator.VerificationReviewedByUserId = null;
        creator.VerificationRejectionReason = null;

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Creator {CreatorId} submitted verification request", creator.Id);

        return await GetVerificationStatusAsync(userId, cancellationToken);
    }

    #endregion

    #region Verification (Admin)

    public async Task<(
        IReadOnlyList<PendingVerificationResponse> Verifications,
        PaginationMeta Meta
    )> GetPendingVerificationsAsync(
        VerificationStatus? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    )
    {
        var query = _dbContext.Set<Creator>().AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(c => c.VerificationStatus == status.Value);
        }
        else
        {
            // Default to pending if no status specified
            query = query.Where(c => c.VerificationStatus == VerificationStatus.Pending);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var creators = await query
            .OrderBy(c => c.VerificationRequestedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var creatorIds = creators.Select(c => c.Id).ToList();

        var postCounts = await _dbContext
            .Set<Post>()
            .Where(p => p.CreatorId.HasValue && creatorIds.Contains(p.CreatorId.Value))
            .GroupBy(p => new { p.CreatorId, p.Status })
            .Select(g => new
            {
                g.Key.CreatorId,
                g.Key.Status,
                Count = g.Count(),
            })
            .ToListAsync(cancellationToken);

        var responses = creators
            .Select(c =>
            {
                var creatorPostCounts = postCounts.Where(pc => pc.CreatorId == c.Id).ToList();
                var totalPosts = creatorPostCounts.Sum(x => x.Count);
                var publishedPosts =
                    creatorPostCounts.FirstOrDefault(x => x.Status == PostStatus.Published)?.Count
                    ?? 0;

                return new PendingVerificationResponse(
                    CreatorId: c.Id,
                    DisplayName: c.DisplayName,
                    Bio: c.Bio,
                    SocialLinks: ParseSocialLinks(c.SocialLinks),
                    Status: c.VerificationStatus,
                    Notes: c.VerificationNotes,
                    DocumentUrls: ParseDocumentUrls(c.VerificationDocumentUrls),
                    RequestedAt: c.VerificationRequestedAt,
                    TotalPosts: totalPosts,
                    PublishedPosts: publishedPosts,
                    CreatedAt: c.CreatedAt
                );
            })
            .ToList();

        var meta = new PaginationMeta
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
        };

        return (responses, meta);
    }

    public async Task<PendingVerificationResponse> GetVerificationDetailsAsync(
        Guid creatorId,
        CancellationToken cancellationToken = default
    )
    {
        var creator = await _dbContext
            .Set<Creator>()
            .FirstOrDefaultAsync(c => c.Id == creatorId, cancellationToken);

        if (creator == null)
        {
            throw new NotFoundException(nameof(Creator), creatorId);
        }

        var postCounts = await _dbContext
            .Set<Post>()
            .Where(p => p.CreatorId == creatorId)
            .GroupBy(p => p.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var totalPosts = postCounts.Sum(x => x.Count);
        var publishedPosts =
            postCounts.FirstOrDefault(x => x.Status == PostStatus.Published)?.Count ?? 0;

        return new PendingVerificationResponse(
            CreatorId: creator.Id,
            DisplayName: creator.DisplayName,
            Bio: creator.Bio,
            SocialLinks: ParseSocialLinks(creator.SocialLinks),
            Status: creator.VerificationStatus,
            Notes: creator.VerificationNotes,
            DocumentUrls: ParseDocumentUrls(creator.VerificationDocumentUrls),
            RequestedAt: creator.VerificationRequestedAt,
            TotalPosts: totalPosts,
            PublishedPosts: publishedPosts,
            CreatedAt: creator.CreatedAt
        );
    }

    public async Task<VerificationStatusResponse> ReviewVerificationAsync(
        Guid adminUserId,
        Guid creatorId,
        ReviewVerificationRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var creator = await _dbContext
            .Set<Creator>()
            .FirstOrDefaultAsync(c => c.Id == creatorId, cancellationToken);

        if (creator == null)
        {
            throw new NotFoundException(nameof(Creator), creatorId);
        }

        if (creator.VerificationStatus != VerificationStatus.Pending)
        {
            throw new ValidationException(
                "Verification",
                "Only pending verifications can be reviewed"
            );
        }

        creator.VerificationReviewedAt = DateTime.UtcNow;
        creator.VerificationReviewedByUserId = adminUserId;

        if (request.Approve)
        {
            creator.VerificationStatus = VerificationStatus.Approved;
            creator.IsVerified = true;
            creator.VerificationRejectionReason = null;

            _logger.LogInformation(
                "Admin {AdminUserId} approved verification for creator {CreatorId}",
                adminUserId,
                creatorId
            );
        }
        else
        {
            creator.VerificationStatus = VerificationStatus.Rejected;
            creator.IsVerified = false;
            creator.VerificationRejectionReason = request.RejectionReason;

            _logger.LogInformation(
                "Admin {AdminUserId} rejected verification for creator {CreatorId}: {Reason}",
                adminUserId,
                creatorId,
                request.RejectionReason
            );
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        var creatorUser = await _userManager.FindByIdAsync(creator.UserId.ToString());
        if (creatorUser?.Email != null)
        {
            _ = _emailService.SendCreatorVerificationResultAsync(
                creatorUser.Email,
                creator.DisplayName,
                request.Approve,
                request.RejectionReason,
                cancellationToken
            );
        }

        return new VerificationStatusResponse(
            Status: creator.VerificationStatus,
            Notes: creator.VerificationNotes,
            DocumentUrls: ParseDocumentUrls(creator.VerificationDocumentUrls),
            RequestedAt: creator.VerificationRequestedAt,
            ReviewedAt: creator.VerificationReviewedAt,
            RejectionReason: creator.VerificationRejectionReason
        );
    }

    #endregion

    #region Post Management

    public async Task<CreatorPostResponse> CreatePostAsync(
        Guid userId,
        CreatePostRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var creator = await GetCreatorByUserIdAsync(userId, cancellationToken);

        // Check draft post limit
        var draftCount = await _dbContext
            .Set<Post>()
            .CountAsync(
                p => p.CreatorId == creator.Id && p.Status == PostStatus.Draft,
                cancellationToken
            );

        if (draftCount >= _creatorSettings.MaxDraftPosts)
        {
            throw new ValidationException(
                "Post",
                $"Maximum draft posts limit ({_creatorSettings.MaxDraftPosts}) reached"
            );
        }

        // Validate media count
        if (request.MediaUrls.Count > _creatorSettings.MaxMediaPerPost)
        {
            throw new ValidationException(
                "MediaUrls",
                $"Maximum {_creatorSettings.MaxMediaPerPost} media items allowed"
            );
        }

        // Validate products count
        if (request.Products.Count > _creatorSettings.MaxProductsPerPost)
        {
            throw new ValidationException(
                "Products",
                $"Maximum {_creatorSettings.MaxProductsPerPost} products allowed"
            );
        }

        // Validate products exist and are active
        var productIds = request.Products.Select(p => p.ProductId).ToList();
        var products = await _dbContext
            .Set<Product>()
            .Where(p => productIds.Contains(p.Id) && p.IsActive)
            .ToListAsync(cancellationToken);

        if (products.Count != productIds.Count)
        {
            throw new ValidationException(
                "Products",
                "One or more products are invalid or inactive"
            );
        }

        var post = new Post
        {
            Id = Guid.NewGuid(),
            AuthorUserId = creator.UserId,
            CreatorId = creator.Id,
            Title = request.Title,
            Description = request.Description,
            MediaType = request.MediaType,
            MediaUrls = JsonSerializer.Serialize(request.MediaUrls, JsonOptions),
            ThumbnailUrls =
                request.ThumbnailUrls != null
                    ? JsonSerializer.Serialize(request.ThumbnailUrls, JsonOptions)
                    : null,
            Status = PostStatus.Draft,
        };

        await _dbContext.Set<Post>().AddAsync(post, cancellationToken);

        // Create post products
        await CreatePostProductsAsync(post.Id, request.Products, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Creator {CreatorId} created post {PostId}", creator.Id, post.Id);

        return await GetPostAsync(userId, post.Id, cancellationToken);
    }

    public async Task<CreatorPostResponse> UpdatePostAsync(
        Guid userId,
        Guid postId,
        UpdatePostRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var creator = await GetCreatorByUserIdAsync(userId, cancellationToken);

        var post = await _dbContext
            .Set<Post>()
            .FirstOrDefaultAsync(
                p => p.Id == postId && p.CreatorId == creator.Id,
                cancellationToken
            );

        if (post == null)
        {
            throw new NotFoundException(nameof(Post), postId);
        }

        if (post.Status != PostStatus.Draft)
        {
            throw new ValidationException("Post", "Only draft posts can be edited");
        }

        if (request.Title != null)
        {
            post.Title = request.Title;
        }

        if (request.Description != null)
        {
            post.Description = request.Description;
        }

        if (request.MediaType.HasValue)
        {
            post.MediaType = request.MediaType.Value;
        }

        if (request.MediaUrls != null)
        {
            if (request.MediaUrls.Count > _creatorSettings.MaxMediaPerPost)
            {
                throw new ValidationException(
                    "MediaUrls",
                    $"Maximum {_creatorSettings.MaxMediaPerPost} media items allowed"
                );
            }
            post.MediaUrls = JsonSerializer.Serialize(request.MediaUrls, JsonOptions);
        }

        if (request.ThumbnailUrls != null)
        {
            post.ThumbnailUrls = JsonSerializer.Serialize(request.ThumbnailUrls, JsonOptions);
        }

        if (request.Products != null)
        {
            if (request.Products.Count > _creatorSettings.MaxProductsPerPost)
            {
                throw new ValidationException(
                    "Products",
                    $"Maximum {_creatorSettings.MaxProductsPerPost} products allowed"
                );
            }

            // Validate products exist and are active
            var productIds = request.Products.Select(p => p.ProductId).ToList();
            var products = await _dbContext
                .Set<Product>()
                .Where(p => productIds.Contains(p.Id) && p.IsActive)
                .ToListAsync(cancellationToken);

            if (products.Count != productIds.Count)
            {
                throw new ValidationException(
                    "Products",
                    "One or more products are invalid or inactive"
                );
            }

            // Remove existing post products and fit tags
            var existingPostProducts = await _dbContext
                .Set<PostProduct>()
                .Where(pp => pp.PostId == postId)
                .ToListAsync(cancellationToken);

            var existingPostProductIds = existingPostProducts.Select(pp => pp.Id).ToList();
            var existingFitTags = await _dbContext
                .Set<PostFitTag>()
                .Where(pft => existingPostProductIds.Contains(pft.PostProductId))
                .ToListAsync(cancellationToken);

            _dbContext.Set<PostFitTag>().RemoveRange(existingFitTags);
            _dbContext.Set<PostProduct>().RemoveRange(existingPostProducts);

            // Create new post products
            await CreatePostProductsAsync(postId, request.Products, cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Creator {CreatorId} updated post {PostId}", creator.Id, postId);

        return await GetPostAsync(userId, postId, cancellationToken);
    }

    public async Task DeletePostAsync(
        Guid userId,
        Guid postId,
        CancellationToken cancellationToken = default
    )
    {
        var creator = await GetCreatorByUserIdAsync(userId, cancellationToken);

        var post = await _dbContext
            .Set<Post>()
            .FirstOrDefaultAsync(
                p => p.Id == postId && p.CreatorId == creator.Id,
                cancellationToken
            );

        if (post == null)
        {
            throw new NotFoundException(nameof(Post), postId);
        }

        // Remove fit tags
        var postProductIds = await _dbContext
            .Set<PostProduct>()
            .Where(pp => pp.PostId == postId)
            .Select(pp => pp.Id)
            .ToListAsync(cancellationToken);

        var fitTags = await _dbContext
            .Set<PostFitTag>()
            .Where(pft => postProductIds.Contains(pft.PostProductId))
            .ToListAsync(cancellationToken);

        _dbContext.Set<PostFitTag>().RemoveRange(fitTags);

        // Remove post products
        var postProducts = await _dbContext
            .Set<PostProduct>()
            .Where(pp => pp.PostId == postId)
            .ToListAsync(cancellationToken);

        _dbContext.Set<PostProduct>().RemoveRange(postProducts);

        // Remove engagements
        var engagements = await _dbContext
            .Set<Engagement>()
            .Where(e => e.PostId == postId)
            .ToListAsync(cancellationToken);

        _dbContext.Set<Engagement>().RemoveRange(engagements);

        // Remove the post
        _dbContext.Set<Post>().Remove(post);

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Creator {CreatorId} deleted post {PostId}", creator.Id, postId);
    }

    public async Task<CreatorPostResponse> GetPostAsync(
        Guid userId,
        Guid postId,
        CancellationToken cancellationToken = default
    )
    {
        var creator = await GetCreatorByUserIdAsync(userId, cancellationToken);

        var post = await _dbContext
            .Set<Post>()
            .AsNoTracking()
            .Include(p => p.PostProducts)
                .ThenInclude(pp => pp.Product)
                    .ThenInclude(prod => prod.Retailer)
            .Include(p => p.PostProducts)
                .ThenInclude(pp => pp.FitTags)
                    .ThenInclude(pft => pft.FitTag)
            .Include(p => p.Engagements)
            .Include(p => p.ClickEvents)
            .AsSplitQuery()
            .FirstOrDefaultAsync(
                p => p.Id == postId && p.CreatorId == creator.Id,
                cancellationToken
            );

        if (post == null)
        {
            throw new NotFoundException(nameof(Post), postId);
        }

        return MapToCreatorPostResponse(post);
    }

    public async Task<(
        IReadOnlyList<CreatorPostResponse> Posts,
        PaginationMeta Meta
    )> GetPostsAsync(
        Guid userId,
        CreatorPostsRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var creator = await GetCreatorByUserIdAsync(userId, cancellationToken);

        var query = _dbContext.Set<Post>().Where(p => p.CreatorId == creator.Id);

        if (request.Status.HasValue)
        {
            query = query.Where(p => p.Status == request.Status.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var posts = await query
            .AsNoTracking()
            .Include(p => p.PostProducts)
                .ThenInclude(pp => pp.Product)
                    .ThenInclude(prod => prod.Retailer)
            .Include(p => p.PostProducts)
                .ThenInclude(pp => pp.FitTags)
                    .ThenInclude(pft => pft.FitTag)
            .Include(p => p.Engagements)
            .Include(p => p.ClickEvents)
            .AsSplitQuery()
            .OrderByDescending(p => p.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var postResponses = posts.Select(MapToCreatorPostResponse).ToList();

        var meta = new PaginationMeta
        {
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount,
        };

        return (postResponses, meta);
    }

    public async Task<CreatorPostResponse> SubmitPostForReviewAsync(
        Guid userId,
        Guid postId,
        CancellationToken cancellationToken = default
    )
    {
        var creator = await GetCreatorByUserIdAsync(userId, cancellationToken);

        var post = await _dbContext
            .Set<Post>()
            .Include(p => p.PostProducts)
            .FirstOrDefaultAsync(
                p => p.Id == postId && p.CreatorId == creator.Id,
                cancellationToken
            );

        if (post == null)
        {
            throw new NotFoundException(nameof(Post), postId);
        }

        if (post.Status != PostStatus.Draft)
        {
            throw new ValidationException("Post", "Only draft posts can be submitted for review");
        }

        // Validate post has media
        var mediaUrls = ParseMediaUrls(post.MediaUrls);
        if (mediaUrls.Count == 0)
        {
            throw new ValidationException("MediaUrls", "Post must have at least one media item");
        }

        // Validate post has products
        if (!post.PostProducts.Any())
        {
            throw new ValidationException("Products", "Post must have at least one product");
        }

        post.Status = PostStatus.PendingReview;
        post.ModerationNotes = null;

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Creator {CreatorId} submitted post {PostId} for review",
            creator.Id,
            postId
        );

        return await GetPostAsync(userId, postId, cancellationToken);
    }

    #endregion

    #region Analytics & Earnings

    public async Task<CreatorAnalyticsResponse> GetAnalyticsAsync(
        Guid userId,
        CreatorAnalyticsRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var creator = await GetCreatorByUserIdAsync(userId, cancellationToken);

        var startDate = request.StartDate ?? DateTime.UtcNow.AddDays(-30);
        var endDate = request.EndDate ?? DateTime.UtcNow;

        // Get post IDs for this creator
        var postIds = await _dbContext
            .Set<Post>()
            .Where(p => p.CreatorId == creator.Id)
            .Select(p => p.Id)
            .ToListAsync(cancellationToken);

        // Aggregate engagements
        var engagements = await _dbContext
            .Set<Engagement>()
            .Where(e =>
                postIds.Contains(e.PostId) && e.CreatedAt >= startDate && e.CreatedAt <= endDate
            )
            .ToListAsync(cancellationToken);

        var totalViews = engagements.Count(e => e.Type == EngagementType.View);
        var totalLikes = engagements.Count(e => e.Type == EngagementType.Like);
        var totalSaves = engagements.Count(e => e.Type == EngagementType.Save);
        var totalShares = engagements.Count(e => e.Type == EngagementType.Share);

        // Aggregate clicks
        var clicks = await _dbContext
            .Set<ClickEvent>()
            .Where(c =>
                postIds.Contains(c.PostId) && c.CreatedAt >= startDate && c.CreatedAt <= endDate
            )
            .ToListAsync(cancellationToken);

        var totalClicks = clicks.Count;

        // Aggregate earnings
        var earnings = await _dbContext
            .Set<CreatorEarning>()
            .Where(e =>
                e.CreatorId == creator.Id && e.CreatedAt >= startDate && e.CreatedAt <= endDate
            )
            .ToListAsync(cancellationToken);

        var totalEarnings = earnings.Sum(e => e.Amount);

        var summary = new AnalyticsSummary(
            TotalViews: totalViews,
            TotalLikes: totalLikes,
            TotalSaves: totalSaves,
            TotalShares: totalShares,
            TotalClicks: totalClicks,
            TotalEarnings: totalEarnings,
            Currency: _creatorSettings.DefaultCurrency
        );

        // Top posts by engagement
        var posts = await _dbContext
            .Set<Post>()
            .Where(p => p.CreatorId == creator.Id && p.Status == PostStatus.Published)
            .Include(p => p.Engagements)
            .Include(p => p.ClickEvents)
            .ToListAsync(cancellationToken);

        var postEarnings = await _dbContext
            .Set<CreatorEarning>()
            .Where(e => e.CreatorId == creator.Id)
            .Include(e => e.ClickEvent)
            .ToListAsync(cancellationToken);

        var topPosts = posts
            .Select(p => new TopPostAnalytics(
                PostId: p.Id,
                Title: p.Title,
                ThumbnailUrl: ParseMediaUrls(p.MediaUrls).FirstOrDefault(),
                Views: p.Engagements.Count(e => e.Type == EngagementType.View),
                Likes: p.Engagements.Count(e => e.Type == EngagementType.Like),
                Clicks: p.ClickEvents.Count,
                Earnings: postEarnings.Where(e => e.ClickEvent.PostId == p.Id).Sum(e => e.Amount)
            ))
            .OrderByDescending(p => p.Views + p.Likes * 2)
            .Take(10)
            .ToList();

        // Daily metrics
        var dailyMetrics = Enumerable
            .Range(0, (endDate - startDate).Days + 1)
            .Select(i => startDate.AddDays(i).Date)
            .Select(date => new DailyMetrics(
                Date: date,
                Views: engagements.Count(e =>
                    e.Type == EngagementType.View && e.CreatedAt.Date == date
                ),
                Likes: engagements.Count(e =>
                    e.Type == EngagementType.Like && e.CreatedAt.Date == date
                ),
                Clicks: clicks.Count(c => c.CreatedAt.Date == date),
                Earnings: earnings.Where(e => e.CreatedAt.Date == date).Sum(e => e.Amount)
            ))
            .ToList();

        return new CreatorAnalyticsResponse(summary, topPosts, dailyMetrics);
    }

    public async Task<EarningsSummaryResponse> GetEarningsSummaryAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        var creator = await GetCreatorByUserIdAsync(userId, cancellationToken);

        var earnings = await _dbContext
            .Set<CreatorEarning>()
            .Where(e => e.CreatorId == creator.Id)
            .ToListAsync(cancellationToken);

        var totalEarnings = earnings.Sum(e => e.Amount);
        var pendingEarnings = earnings
            .Where(e => e.Status == EarningStatus.Pending)
            .Sum(e => e.Amount);
        var confirmedEarnings = earnings
            .Where(e => e.Status == EarningStatus.Confirmed)
            .Sum(e => e.Amount);
        var paidEarnings = earnings.Where(e => e.Status == EarningStatus.Paid).Sum(e => e.Amount);

        var eligibleForPayout = confirmedEarnings >= _creatorSettings.MinPayoutThreshold;

        return new EarningsSummaryResponse(
            TotalEarnings: totalEarnings,
            PendingEarnings: pendingEarnings,
            ConfirmedEarnings: confirmedEarnings,
            PaidEarnings: paidEarnings,
            Currency: _creatorSettings.DefaultCurrency,
            MinPayoutThreshold: _creatorSettings.MinPayoutThreshold,
            EligibleForPayout: eligibleForPayout
        );
    }

    public async Task<(
        IReadOnlyList<EarningDetailResponse> Earnings,
        PaginationMeta Meta
    )> GetEarningsHistoryAsync(
        Guid userId,
        EarningsHistoryRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var creator = await GetCreatorByUserIdAsync(userId, cancellationToken);

        var query = _dbContext.Set<CreatorEarning>().Where(e => e.CreatorId == creator.Id);

        if (request.Status.HasValue)
        {
            query = query.Where(e => e.Status == request.Status.Value);
        }

        if (request.StartDate.HasValue)
        {
            query = query.Where(e => e.CreatedAt >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            query = query.Where(e => e.CreatedAt <= request.EndDate.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var earnings = await query
            .Include(e => e.ClickEvent)
                .ThenInclude(c => c.Post)
            .Include(e => e.ClickEvent)
                .ThenInclude(c => c.PostProduct)
                    .ThenInclude(pp => pp.Product)
            .OrderByDescending(e => e.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var earningResponses = earnings
            .Select(e => new EarningDetailResponse(
                Id: e.Id,
                EarningType: e.EarningType,
                Amount: e.Amount,
                Currency: e.Currency,
                Status: e.Status,
                PostId: e.ClickEvent.PostId,
                PostTitle: e.ClickEvent.Post.Title,
                ProductId: e.ClickEvent.PostProduct.ProductId,
                ProductName: e.ClickEvent.PostProduct.Product.Name,
                PaidAt: e.PaidAt,
                CreatedAt: e.CreatedAt
            ))
            .ToList();

        var meta = new PaginationMeta
        {
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount,
        };

        return (earningResponses, meta);
    }

    #endregion

    #region Private Methods

    private async Task<Creator> GetCreatorByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken
    )
    {
        var creator = await _dbContext
            .Set<Creator>()
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

        if (creator == null)
        {
            throw new NotFoundException("Creator", userId);
        }

        return creator;
    }

    private async Task CreatePostProductsAsync(
        Guid postId,
        List<PostProductRequest> productRequests,
        CancellationToken cancellationToken
    )
    {
        var fitTags = _dbContext.Set<FitTag>();
        var postProducts = _dbContext.Set<PostProduct>();
        var postFitTags = _dbContext.Set<PostFitTag>();

        foreach (var productRequest in productRequests)
        {
            var postProduct = new PostProduct
            {
                Id = Guid.NewGuid(),
                PostId = postId,
                ProductId = productRequest.ProductId,
                SizeWorn = productRequest.SizeWorn,
                FitRating = productRequest.FitRating,
                FitNotes = productRequest.FitNotes,
                StylingNotes = productRequest.StylingNotes,
            };

            await postProducts.AddAsync(postProduct, cancellationToken);

            // Add fit tags
            if (productRequest.FitTagIds != null && productRequest.FitTagIds.Any())
            {
                // Validate fit tags exist
                var validFitTagIds = await fitTags
                    .Where(ft => productRequest.FitTagIds.Contains(ft.Id))
                    .Select(ft => ft.Id)
                    .ToListAsync(cancellationToken);

                foreach (var fitTagId in validFitTagIds)
                {
                    var postFitTag = new PostFitTag
                    {
                        PostProductId = postProduct.Id,
                        FitTagId = fitTagId,
                    };

                    await postFitTags.AddAsync(postFitTag, cancellationToken);
                }
            }
        }
    }

    private static CreatorPostResponse MapToCreatorPostResponse(Post post)
    {
        var products = post
            .PostProducts.Select(pp => new CreatorPostProductResponse(
                Id: pp.Id,
                ProductId: pp.ProductId,
                ProductName: pp.Product.Name,
                ProductImage: ParseMediaUrls(pp.Product.ImageUrls).FirstOrDefault(),
                ProductPrice: pp.Product.Price,
                ProductCurrency: pp.Product.Currency,
                RetailerName: pp.Product.Retailer.Name,
                SizeWorn: pp.SizeWorn,
                FitRating: pp.FitRating,
                FitNotes: pp.FitNotes,
                StylingNotes: pp.StylingNotes,
                FitTags: pp.FitTags.Select(ft => ft.FitTag.Name).ToList()
            ))
            .ToList();

        var engagements = new CreatorPostEngagementResponse(
            Views: post.Engagements.Count(e => e.Type == EngagementType.View),
            Likes: post.Engagements.Count(e => e.Type == EngagementType.Like),
            Saves: post.Engagements.Count(e => e.Type == EngagementType.Save),
            Shares: post.Engagements.Count(e => e.Type == EngagementType.Share),
            Clicks: post.ClickEvents.Count
        );

        return new CreatorPostResponse(
            Id: post.Id,
            Title: post.Title,
            Description: post.Description,
            MediaType: post.MediaType,
            MediaUrls: ParseMediaUrls(post.MediaUrls),
            ThumbnailUrls: ParseMediaUrls(post.ThumbnailUrls),
            Status: post.Status,
            ModerationNotes: post.ModerationNotes,
            PublishedAt: post.PublishedAt,
            Products: products,
            Engagements: engagements,
            CreatedAt: post.CreatedAt,
            UpdatedAt: post.UpdatedAt
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
            return JsonSerializer.Deserialize<List<string>>(json, JsonOptions)
                ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }

    private static Dictionary<string, string>? ParseSocialLinks(string? json)
    {
        if (string.IsNullOrEmpty(json))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, string>>(json, JsonOptions);
        }
        catch
        {
            return null;
        }
    }

    private static List<string>? ParseDocumentUrls(string? json)
    {
        if (string.IsNullOrEmpty(json))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<List<string>>(json, JsonOptions);
        }
        catch
        {
            return null;
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
