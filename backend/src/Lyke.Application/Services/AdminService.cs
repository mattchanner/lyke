using System.Text.Json;
using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Admin;
using Lyke.Application.Interfaces;
using Lyke.Core.Entities;
using Lyke.Core.Enums;
using Lyke.Core.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Lyke.Application.Services;

public class AdminService : IAdminService
{
    private readonly DbContext _dbContext;
    private readonly ILogger<AdminService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public AdminService(DbContext dbContext, ILogger<AdminService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    #region Post Moderation

    public async Task<(
        IReadOnlyList<PendingPostResponse> Posts,
        PaginationMeta Meta
    )> GetPendingPostsAsync(
        PostStatus? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    )
    {
        var query = _dbContext.Set<Post>().AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(p => p.Status == status.Value);
        }
        else
        {
            query = query.Where(p => p.Status == PostStatus.PendingReview);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var posts = await query
            .Include(p => p.Creator)
            .Include(p => p.PostProducts)
            .ThenInclude(pp => pp.Product)
            .OrderBy(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var creatorIds = posts.Select(p => p.CreatorId).Distinct().ToList();
        var creatorPostCounts = await _dbContext
            .Set<Post>()
            .Where(p => creatorIds.Contains(p.CreatorId))
            .GroupBy(p => new { p.CreatorId, p.Status })
            .Select(g => new
            {
                g.Key.CreatorId,
                g.Key.Status,
                Count = g.Count(),
            })
            .ToListAsync(cancellationToken);

        var responses = posts
            .Select(p =>
            {
                var creatorCounts = creatorPostCounts
                    .Where(c => c.CreatorId == p.CreatorId)
                    .ToList();
                var totalPosts = creatorCounts.Sum(c => c.Count);
                var publishedPosts =
                    creatorCounts.FirstOrDefault(c => c.Status == PostStatus.Published)?.Count ?? 0;

                return new PendingPostResponse(
                    Id: p.Id,
                    Title: p.Title,
                    Description: p.Description,
                    MediaType: p.MediaType,
                    MediaUrls: ParseMediaUrls(p.MediaUrls),
                    Status: p.Status,
                    CreatedAt: p.CreatedAt,
                    SubmittedAt: p.UpdatedAt,
                    Creator: new CreatorSummary(
                        Id: p.Creator.Id,
                        DisplayName: p.Creator.DisplayName,
                        IsVerified: p.Creator.IsVerified,
                        TotalPosts: totalPosts,
                        PublishedPosts: publishedPosts
                    ),
                    Products: p.PostProducts.Select(pp => new PostProductSummary(
                            ProductId: pp.ProductId,
                            ProductName: pp.Product.Name,
                            SizeWorn: pp.SizeWorn,
                            FitNotes: pp.FitNotes
                        ))
                        .ToList()
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

    public async Task<PendingPostResponse> GetPostForModerationAsync(
        Guid postId,
        CancellationToken cancellationToken = default
    )
    {
        var post = await _dbContext
            .Set<Post>()
            .Include(p => p.Creator)
            .Include(p => p.PostProducts)
            .ThenInclude(pp => pp.Product)
            .FirstOrDefaultAsync(p => p.Id == postId, cancellationToken);

        if (post == null)
        {
            throw new NotFoundException(nameof(Post), postId);
        }

        var creatorPostCounts = await _dbContext
            .Set<Post>()
            .Where(p => p.CreatorId == post.CreatorId)
            .GroupBy(p => p.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var totalPosts = creatorPostCounts.Sum(c => c.Count);
        var publishedPosts =
            creatorPostCounts.FirstOrDefault(c => c.Status == PostStatus.Published)?.Count ?? 0;

        return new PendingPostResponse(
            Id: post.Id,
            Title: post.Title,
            Description: post.Description,
            MediaType: post.MediaType,
            MediaUrls: ParseMediaUrls(post.MediaUrls),
            Status: post.Status,
            CreatedAt: post.CreatedAt,
            SubmittedAt: post.UpdatedAt,
            Creator: new CreatorSummary(
                Id: post.Creator.Id,
                DisplayName: post.Creator.DisplayName,
                IsVerified: post.Creator.IsVerified,
                TotalPosts: totalPosts,
                PublishedPosts: publishedPosts
            ),
            Products: post.PostProducts.Select(pp => new PostProductSummary(
                    ProductId: pp.ProductId,
                    ProductName: pp.Product.Name,
                    SizeWorn: pp.SizeWorn,
                    FitNotes: pp.FitNotes
                ))
                .ToList()
        );
    }

    public async Task<PostModerationResponse> ModeratePostAsync(
        Guid adminUserId,
        Guid postId,
        ModeratePostRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var post = await _dbContext
            .Set<Post>()
            .FirstOrDefaultAsync(p => p.Id == postId, cancellationToken);

        if (post == null)
        {
            throw new NotFoundException(nameof(Post), postId);
        }

        if (post.Status != PostStatus.PendingReview)
        {
            throw new ValidationException("Post", "Only posts pending review can be moderated");
        }

        post.ModeratedByUserId = adminUserId;
        post.ModeratedAt = DateTime.UtcNow;

        if (request.Approve)
        {
            post.Status = PostStatus.Published;
            post.PublishedAt = DateTime.UtcNow;
            post.ModerationNotes = null;

            _logger.LogInformation(
                "Admin {AdminUserId} approved post {PostId}",
                adminUserId,
                postId
            );
        }
        else
        {
            post.Status = PostStatus.Rejected;
            post.ModerationNotes = request.RejectionReason;

            _logger.LogInformation(
                "Admin {AdminUserId} rejected post {PostId}: {Reason}",
                adminUserId,
                postId,
                request.RejectionReason
            );
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new PostModerationResponse(
            Id: post.Id,
            Status: post.Status,
            ModerationNotes: post.ModerationNotes,
            ModeratedAt: post.ModeratedAt,
            ModeratedByUserId: post.ModeratedByUserId
        );
    }

    #endregion

    #region User Management

    public async Task<(IReadOnlyList<UserListResponse> Users, PaginationMeta Meta)> GetUsersAsync(
        UserListRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var query = _dbContext.Set<User>().AsQueryable();

        if (request.UserType.HasValue)
        {
            query = query.Where(u => u.UserType == request.UserType.Value);
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(u => u.IsActive == request.IsActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLower();
            query = query.Where(u =>
                (u.Email != null && u.Email.ToLower().Contains(search))
                || (u.UserName != null && u.UserName.ToLower().Contains(search))
            );
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var responses = users
            .Select(u => new UserListResponse(
                Id: u.Id,
                Email: u.Email,
                UserName: u.UserName,
                UserType: u.UserType,
                IsActive: u.IsActive,
                EmailConfirmed: u.EmailConfirmed,
                CreatedAt: u.CreatedAt,
                SuspendedAt: u.SuspendedAt,
                SuspensionReason: u.SuspensionReason
            ))
            .ToList();

        var meta = new PaginationMeta
        {
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount,
        };

        return (responses, meta);
    }

    public async Task<UserDetailResponse> GetUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        var user = await _dbContext
            .Set<User>()
            .Include(u => u.BodyProfile)
            .Include(u => u.Creator)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
        {
            throw new NotFoundException(nameof(User), userId);
        }

        CreatorInfo? creatorInfo = null;
        if (user.Creator != null)
        {
            var creatorPostCounts = await _dbContext
                .Set<Post>()
                .Where(p => p.CreatorId == user.Creator.Id)
                .GroupBy(p => p.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync(cancellationToken);

            var totalPosts = creatorPostCounts.Sum(c => c.Count);
            var publishedPosts =
                creatorPostCounts.FirstOrDefault(c => c.Status == PostStatus.Published)?.Count ?? 0;

            creatorInfo = new CreatorInfo(
                CreatorId: user.Creator.Id,
                DisplayName: user.Creator.DisplayName,
                IsVerified: user.Creator.IsVerified,
                VerificationStatus: user.Creator.VerificationStatus,
                TotalPosts: totalPosts,
                PublishedPosts: publishedPosts
            );
        }

        return new UserDetailResponse(
            Id: user.Id,
            Email: user.Email,
            UserName: user.UserName,
            UserType: user.UserType,
            IsActive: user.IsActive,
            EmailConfirmed: user.EmailConfirmed,
            CreatedAt: user.CreatedAt,
            UpdatedAt: user.UpdatedAt,
            SuspendedAt: user.SuspendedAt,
            SuspendedByUserId: user.SuspendedByUserId,
            SuspensionReason: user.SuspensionReason,
            HasBodyProfile: user.BodyProfile != null,
            IsCreator: user.Creator != null,
            CreatorInfo: creatorInfo
        );
    }

    public async Task<UserSuspensionResponse> SuspendUserAsync(
        Guid adminUserId,
        Guid userId,
        SuspendUserRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var user = await _dbContext
            .Set<User>()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
        {
            throw new NotFoundException(nameof(User), userId);
        }

        if (!user.IsActive)
        {
            throw new ValidationException("User", "User is already suspended");
        }

        if (user.UserType == UserType.Admin)
        {
            throw new ValidationException("User", "Cannot suspend admin users");
        }

        user.IsActive = false;
        user.SuspendedAt = DateTime.UtcNow;
        user.SuspendedByUserId = adminUserId;
        user.SuspensionReason = request.Reason;

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Admin {AdminUserId} suspended user {UserId}: {Reason}",
            adminUserId,
            userId,
            request.Reason
        );

        return new UserSuspensionResponse(
            UserId: user.Id,
            IsActive: user.IsActive,
            SuspendedAt: user.SuspendedAt,
            SuspendedByUserId: user.SuspendedByUserId,
            SuspensionReason: user.SuspensionReason
        );
    }

    public async Task<UserSuspensionResponse> UnsuspendUserAsync(
        Guid adminUserId,
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        var user = await _dbContext
            .Set<User>()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
        {
            throw new NotFoundException(nameof(User), userId);
        }

        if (user.IsActive)
        {
            throw new ValidationException("User", "User is not suspended");
        }

        user.IsActive = true;
        user.SuspendedAt = null;
        user.SuspendedByUserId = null;
        user.SuspensionReason = null;

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Admin {AdminUserId} unsuspended user {UserId}",
            adminUserId,
            userId
        );

        return new UserSuspensionResponse(
            UserId: user.Id,
            IsActive: user.IsActive,
            SuspendedAt: user.SuspendedAt,
            SuspendedByUserId: user.SuspendedByUserId,
            SuspensionReason: user.SuspensionReason
        );
    }

    #endregion

    #region Platform Stats

    public async Task<PlatformStatsResponse> GetPlatformStatsAsync(
        CancellationToken cancellationToken = default
    )
    {
        var now = DateTime.UtcNow;
        var sevenDaysAgo = now.AddDays(-7);
        var thirtyDaysAgo = now.AddDays(-30);

        // User stats
        var users = await _dbContext.Set<User>().ToListAsync(cancellationToken);
        var userStats = new UserStats(
            TotalUsers: users.Count,
            ActiveUsers: users.Count(u => u.IsActive),
            SuspendedUsers: users.Count(u => !u.IsActive),
            Shoppers: users.Count(u => u.UserType == UserType.Shopper),
            Creators: users.Count(u => u.UserType == UserType.Creator),
            Retailers: users.Count(u => u.UserType == UserType.Retailer),
            Admins: users.Count(u => u.UserType == UserType.Admin),
            NewUsersLast7Days: users.Count(u => u.CreatedAt >= sevenDaysAgo),
            NewUsersLast30Days: users.Count(u => u.CreatedAt >= thirtyDaysAgo)
        );

        // Content stats
        var posts = await _dbContext.Set<Post>().ToListAsync(cancellationToken);
        var contentStats = new ContentStats(
            TotalPosts: posts.Count,
            PublishedPosts: posts.Count(p => p.Status == PostStatus.Published),
            PendingReviewPosts: posts.Count(p => p.Status == PostStatus.PendingReview),
            DraftPosts: posts.Count(p => p.Status == PostStatus.Draft),
            RejectedPosts: posts.Count(p => p.Status == PostStatus.Rejected),
            PostsLast7Days: posts.Count(p => p.CreatedAt >= sevenDaysAgo),
            PostsLast30Days: posts.Count(p => p.CreatedAt >= thirtyDaysAgo)
        );

        // Verification stats
        var creators = await _dbContext.Set<Creator>().ToListAsync(cancellationToken);
        var verificationStats = new VerificationStats(
            PendingVerifications: creators.Count(c =>
                c.VerificationStatus == VerificationStatus.Pending
            ),
            ApprovedCreators: creators.Count(c =>
                c.VerificationStatus == VerificationStatus.Approved
            ),
            RejectedVerifications: creators.Count(c =>
                c.VerificationStatus == VerificationStatus.Rejected
            ),
            TotalCreators: creators.Count
        );

        // Engagement stats
        var engagements = await _dbContext.Set<Engagement>().ToListAsync(cancellationToken);
        var clicks = await _dbContext.Set<ClickEvent>().ToListAsync(cancellationToken);

        var engagementStats = new EngagementStats(
            TotalViews: engagements.Count(e => e.Type == EngagementType.View),
            TotalLikes: engagements.Count(e => e.Type == EngagementType.Like),
            TotalSaves: engagements.Count(e => e.Type == EngagementType.Save),
            TotalClicks: clicks.Count,
            ViewsLast7Days: engagements.Count(e =>
                e.Type == EngagementType.View && e.CreatedAt >= sevenDaysAgo
            ),
            ClicksLast7Days: clicks.Count(c => c.CreatedAt >= sevenDaysAgo)
        );

        return new PlatformStatsResponse(
            userStats,
            contentStats,
            verificationStats,
            engagementStats
        );
    }

    #endregion

    #region Private Methods

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

    #endregion
}
