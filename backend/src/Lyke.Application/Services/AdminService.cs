using System.Text.Json;
using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Admin;
using Lyke.Application.Interfaces;
using Lyke.Core.Entities;
using Lyke.Core.Enums;
using Lyke.Core.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Lyke.Application.Services;

public class AdminService : IAdminService
{
    private readonly DbContext _dbContext;
    private readonly IEmailService _emailService;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<AdminService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public AdminService(
        DbContext dbContext,
        IEmailService emailService,
        UserManager<User> userManager,
        ILogger<AdminService> logger
    )
    {
        _dbContext = dbContext;
        _emailService = emailService;
        _userManager = userManager;
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
        var query = _dbContext.Set<Post>().AsNoTracking().AsQueryable();

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
            .Include(p => p.AuthorUser)
            .Include(p => p.Creator)
            .Include(p => p.PostProducts)
                .ThenInclude(pp => pp.Product)
            .AsSplitQuery()
            .OrderBy(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var authorUserIds = posts.Select(p => p.AuthorUserId).Distinct().ToList();
        var authorPostCounts = await _dbContext
            .Set<Post>()
            .Where(p => authorUserIds.Contains(p.AuthorUserId))
            .GroupBy(p => new { p.AuthorUserId, p.Status })
            .Select(g => new
            {
                g.Key.AuthorUserId,
                g.Key.Status,
                Count = g.Count(),
            })
            .ToListAsync(cancellationToken);

        var responses = posts
            .Select(p =>
            {
                var authorCounts = authorPostCounts
                    .Where(c => c.AuthorUserId == p.AuthorUserId)
                    .ToList();
                var totalPosts = authorCounts.Sum(c => c.Count);
                var publishedPosts =
                    authorCounts.FirstOrDefault(c => c.Status == PostStatus.Published)?.Count ?? 0;

                return new PendingPostResponse(
                    Id: p.Id,
                    Title: p.Title,
                    Description: p.Description,
                    MediaType: p.MediaType,
                    MediaUrls: ParseMediaUrls(p.MediaUrls),
                    ThumbnailUrls: ParseMediaUrls(p.ThumbnailUrls),
                    Status: p.Status,
                    CreatedAt: p.CreatedAt,
                    SubmittedAt: p.UpdatedAt,
                    Author: new AuthorSummary(
                        UserId: p.AuthorUserId,
                        CreatorId: p.CreatorId,
                        DisplayName: p.Creator?.DisplayName
                            ?? p.AuthorUser.DisplayName
                            ?? p.AuthorUser.UserName!,
                        IsVerified: p.Creator?.IsVerified ?? false,
                        UserType: p.AuthorUser.UserType,
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
            .AsNoTracking()
            .Include(p => p.AuthorUser)
            .Include(p => p.Creator)
            .Include(p => p.PostProducts)
                .ThenInclude(pp => pp.Product)
            .AsSplitQuery()
            .FirstOrDefaultAsync(p => p.Id == postId, cancellationToken);

        if (post == null)
        {
            throw new NotFoundException(nameof(Post), postId);
        }

        var authorPostCounts = await _dbContext
            .Set<Post>()
            .Where(p => p.AuthorUserId == post.AuthorUserId)
            .GroupBy(p => p.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var totalPosts = authorPostCounts.Sum(c => c.Count);
        var publishedPosts =
            authorPostCounts.FirstOrDefault(c => c.Status == PostStatus.Published)?.Count ?? 0;

        return new PendingPostResponse(
            Id: post.Id,
            Title: post.Title,
            Description: post.Description,
            MediaType: post.MediaType,
            MediaUrls: ParseMediaUrls(post.MediaUrls),
            ThumbnailUrls: ParseMediaUrls(post.ThumbnailUrls),
            Status: post.Status,
            CreatedAt: post.CreatedAt,
            SubmittedAt: post.UpdatedAt,
            Author: new AuthorSummary(
                UserId: post.AuthorUserId,
                CreatorId: post.CreatorId,
                DisplayName: post.Creator?.DisplayName
                    ?? post.AuthorUser.DisplayName
                    ?? post.AuthorUser.UserName!,
                IsVerified: post.Creator?.IsVerified ?? false,
                UserType: post.AuthorUser.UserType,
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

        var authorUser = await _userManager.FindByIdAsync(post.AuthorUserId.ToString());
        if (authorUser?.Email != null)
        {
            _ = _emailService.SendPostModerationResultAsync(
                authorUser.Email,
                post.Title ?? "Untitled",
                request.Approve,
                request.RejectionReason,
                cancellationToken
            );
        }

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
        var query = _dbContext.Set<User>().AsNoTracking().AsQueryable();

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
            .AsNoTracking()
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

        if (user.Email != null)
        {
            _ = _emailService.SendAccountSuspensionNotificationAsync(
                user.Email,
                request.Reason,
                cancellationToken
            );
        }

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

    #region Content Reports

    public async Task<(
        IReadOnlyList<ContentReportResponse> Reports,
        PaginationMeta Meta
    )> GetContentReportsAsync(
        ContentReportQueryRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var query = _dbContext.Set<ContentReport>().AsNoTracking().AsQueryable();

        if (request.Status.HasValue)
            query = query.Where(cr => cr.Status == request.Status.Value);

        if (request.Reason.HasValue)
            query = query.Where(cr => cr.Reason == request.Reason.Value);

        if (request.PostId.HasValue)
            query = query.Where(cr => cr.PostId == request.PostId.Value);

        if (request.From.HasValue)
            query = query.Where(cr => cr.CreatedAt >= request.From.Value);

        if (request.To.HasValue)
            query = query.Where(cr => cr.CreatedAt <= request.To.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var reports = await query
            .Include(cr => cr.Post)
            .OrderByDescending(cr => cr.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var responses = reports
            .Select(cr => new ContentReportResponse(
                Id: cr.Id,
                PostId: cr.PostId,
                PostTitle: cr.Post.Title,
                ReportedByUserId: cr.ReportedByUserId,
                Reason: cr.Reason,
                AdditionalDetails: cr.AdditionalDetails,
                Status: cr.Status,
                ReviewedByUserId: cr.ReviewedByUserId,
                ReviewedAt: cr.ReviewedAt,
                ReviewNotes: cr.ReviewNotes,
                CreatedAt: cr.CreatedAt
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

    public async Task<ContentReportResponse> GetContentReportAsync(
        Guid reportId,
        CancellationToken cancellationToken = default
    )
    {
        var report = await _dbContext
            .Set<ContentReport>()
            .Include(cr => cr.Post)
            .FirstOrDefaultAsync(cr => cr.Id == reportId, cancellationToken);

        if (report == null)
        {
            throw new NotFoundException(nameof(ContentReport), reportId);
        }

        return new ContentReportResponse(
            Id: report.Id,
            PostId: report.PostId,
            PostTitle: report.Post.Title,
            ReportedByUserId: report.ReportedByUserId,
            Reason: report.Reason,
            AdditionalDetails: report.AdditionalDetails,
            Status: report.Status,
            ReviewedByUserId: report.ReviewedByUserId,
            ReviewedAt: report.ReviewedAt,
            ReviewNotes: report.ReviewNotes,
            CreatedAt: report.CreatedAt
        );
    }

    public async Task<ContentReportResponse> ReviewContentReportAsync(
        Guid adminUserId,
        Guid reportId,
        ReviewContentReportRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var report = await _dbContext
            .Set<ContentReport>()
            .Include(cr => cr.Post)
            .FirstOrDefaultAsync(cr => cr.Id == reportId, cancellationToken);

        if (report == null)
        {
            throw new NotFoundException(nameof(ContentReport), reportId);
        }

        report.Status = request.NewStatus;
        report.ReviewedByUserId = adminUserId;
        report.ReviewedAt = DateTime.UtcNow;
        report.ReviewNotes = request.ReviewNotes;

        // Apply post action if specified
        if (request.PostAction.HasValue)
        {
            report.Post.Status = request.PostAction.Value;

            if (request.PostAction.Value == PostStatus.Published)
            {
                report.Post.PublishedAt ??= DateTime.UtcNow;
            }

            report.Post.ModeratedByUserId = adminUserId;
            report.Post.ModeratedAt = DateTime.UtcNow;
            report.Post.ModerationNotes = request.ReviewNotes;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Admin {AdminUserId} reviewed report {ReportId} with status {Status}",
            adminUserId,
            reportId,
            request.NewStatus
        );

        return new ContentReportResponse(
            Id: report.Id,
            PostId: report.PostId,
            PostTitle: report.Post.Title,
            ReportedByUserId: report.ReportedByUserId,
            Reason: report.Reason,
            AdditionalDetails: report.AdditionalDetails,
            Status: report.Status,
            ReviewedByUserId: report.ReviewedByUserId,
            ReviewedAt: report.ReviewedAt,
            ReviewNotes: report.ReviewNotes,
            CreatedAt: report.CreatedAt
        );
    }

    #endregion

    #region Moderation Queue

    public async Task<(
        IReadOnlyList<ModerationQueueItemResponse> Items,
        PaginationMeta Meta
    )> GetModerationQueueAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    )
    {
        var posts = await _dbContext
            .Set<Post>()
            .AsNoTracking()
            .Where(p => p.Status == PostStatus.PendingReview || p.Status == PostStatus.Flagged)
            .Include(p => p.AuthorUser)
            .Include(p => p.Creator)
            .Include(p => p.PostProducts)
                .ThenInclude(pp => pp.Product)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);

        var postIds = posts.Select(p => p.Id).ToList();

        // Get report counts and top reasons per post
        var reportData = await _dbContext
            .Set<ContentReport>()
            .Where(cr => postIds.Contains(cr.PostId))
            .GroupBy(cr => cr.PostId)
            .Select(g => new
            {
                PostId = g.Key,
                Count = g.Count(),
                TopReason = g.GroupBy(cr => cr.Reason)
                    .OrderByDescending(rg => rg.Count())
                    .Select(rg => rg.Key)
                    .FirstOrDefault(),
            })
            .ToListAsync(cancellationToken);

        var reportLookup = reportData.ToDictionary(r => r.PostId);

        // Get author post counts
        var authorUserIds = posts.Select(p => p.AuthorUserId).Distinct().ToList();
        var authorPostCounts = await _dbContext
            .Set<Post>()
            .Where(p => authorUserIds.Contains(p.AuthorUserId))
            .GroupBy(p => new { p.AuthorUserId, p.Status })
            .Select(g => new
            {
                g.Key.AuthorUserId,
                g.Key.Status,
                Count = g.Count(),
            })
            .ToListAsync(cancellationToken);

        // Calculate priority and build response
        var items = posts
            .Select(p =>
            {
                var report = reportLookup.GetValueOrDefault(p.Id);
                var reportCount = report?.Count ?? 0;
                var topReason = report?.TopReason;
                var isFlagged = p.Status == PostStatus.Flagged;

                // Priority calculation
                var priority = 0;
                if (isFlagged)
                    priority += 100;
                if (reportCount >= 10)
                    priority += 75;
                else if (reportCount >= 5)
                    priority += 50;
                if (
                    topReason == ReportReason.HateSpeech
                    || topReason == ReportReason.InappropriateContent
                )
                    priority += 30;

                var authorCounts = authorPostCounts
                    .Where(c => c.AuthorUserId == p.AuthorUserId)
                    .ToList();
                var authorPublished =
                    authorCounts.FirstOrDefault(c => c.Status == PostStatus.Published)?.Count ?? 0;
                if (authorPublished < 3)
                    priority += 20;

                var hoursInQueue = (DateTime.UtcNow - p.CreatedAt).TotalHours;
                if (hoursInQueue > 48)
                    priority += 40;

                var totalPosts = authorCounts.Sum(c => c.Count);

                return new ModerationQueueItemResponse(
                    Id: p.Id,
                    Title: p.Title,
                    Description: p.Description,
                    MediaType: p.MediaType,
                    MediaUrls: ParseMediaUrls(p.MediaUrls),
                    ThumbnailUrls: ParseMediaUrls(p.ThumbnailUrls),
                    Status: p.Status,
                    CreatedAt: p.CreatedAt,
                    SubmittedAt: p.UpdatedAt,
                    Author: new AuthorSummary(
                        UserId: p.AuthorUserId,
                        CreatorId: p.CreatorId,
                        DisplayName: p.Creator?.DisplayName
                            ?? p.AuthorUser.DisplayName
                            ?? p.AuthorUser.UserName!,
                        IsVerified: p.Creator?.IsVerified ?? false,
                        UserType: p.AuthorUser.UserType,
                        TotalPosts: totalPosts,
                        PublishedPosts: authorPublished
                    ),
                    Products: p.PostProducts.Select(pp => new PostProductSummary(
                            ProductId: pp.ProductId,
                            ProductName: pp.Product.Name,
                            SizeWorn: pp.SizeWorn,
                            FitNotes: pp.FitNotes
                        ))
                        .ToList(),
                    ReportCount: reportCount,
                    TopReportReason: topReason,
                    IsFlagged: isFlagged,
                    Priority: priority
                );
            })
            .OrderByDescending(item => item.Priority)
            .ThenBy(item => item.CreatedAt)
            .ToList();

        var totalCount = items.Count;
        var pagedItems = items.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        var meta = new PaginationMeta
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
        };

        return (pagedItems, meta);
    }

    #endregion

    #region Bulk Actions

    public async Task<BulkActionResult> BulkModeratePostsAsync(
        Guid adminUserId,
        BulkModeratePostsRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var posts = await _dbContext
            .Set<Post>()
            .Where(p => request.PostIds.Contains(p.Id))
            .ToListAsync(cancellationToken);

        var successCount = 0;
        var errors = new List<BulkActionError>();

        foreach (var post in posts)
        {
            try
            {
                switch (request.Action)
                {
                    case BulkPostAction.Approve:
                        if (
                            post.Status != PostStatus.PendingReview
                            && post.Status != PostStatus.Flagged
                        )
                        {
                            errors.Add(
                                new BulkActionError(
                                    post.Id,
                                    "Post is not pending review or flagged"
                                )
                            );
                            continue;
                        }
                        post.Status = PostStatus.Published;
                        post.PublishedAt = DateTime.UtcNow;
                        break;

                    case BulkPostAction.Reject:
                        if (
                            post.Status != PostStatus.PendingReview
                            && post.Status != PostStatus.Flagged
                        )
                        {
                            errors.Add(
                                new BulkActionError(
                                    post.Id,
                                    "Post is not pending review or flagged"
                                )
                            );
                            continue;
                        }
                        post.Status = PostStatus.Rejected;
                        post.ModerationNotes = request.Reason;
                        break;

                    case BulkPostAction.Remove:
                        post.Status = PostStatus.Removed;
                        post.ModerationNotes = request.Reason;
                        break;

                    case BulkPostAction.Flag:
                        if (post.Status != PostStatus.Published)
                        {
                            errors.Add(
                                new BulkActionError(post.Id, "Only published posts can be flagged")
                            );
                            continue;
                        }
                        post.Status = PostStatus.Flagged;
                        break;
                }

                post.ModeratedByUserId = adminUserId;
                post.ModeratedAt = DateTime.UtcNow;
                successCount++;
            }
            catch (Exception ex)
            {
                errors.Add(new BulkActionError(post.Id, ex.Message));
            }
        }

        // Track post IDs not found
        var foundIds = posts.Select(p => p.Id).ToHashSet();
        foreach (var id in request.PostIds.Where(id => !foundIds.Contains(id)))
        {
            errors.Add(new BulkActionError(id, "Post not found"));
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Admin {AdminUserId} bulk moderated {SuccessCount} posts with action {Action}",
            adminUserId,
            successCount,
            request.Action
        );

        return new BulkActionResult(successCount, errors.Count, errors);
    }

    public async Task<BulkActionResult> BulkSuspendUsersAsync(
        Guid adminUserId,
        BulkSuspendUsersRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var users = await _dbContext
            .Set<User>()
            .Where(u => request.UserIds.Contains(u.Id))
            .ToListAsync(cancellationToken);

        var successCount = 0;
        var errors = new List<BulkActionError>();

        foreach (var user in users)
        {
            if (!user.IsActive)
            {
                errors.Add(new BulkActionError(user.Id, "User is already suspended"));
                continue;
            }

            if (user.UserType == UserType.Admin)
            {
                errors.Add(new BulkActionError(user.Id, "Cannot suspend admin users"));
                continue;
            }

            user.IsActive = false;
            user.SuspendedAt = DateTime.UtcNow;
            user.SuspendedByUserId = adminUserId;
            user.SuspensionReason = request.Reason;
            successCount++;
        }

        // Track user IDs not found
        var foundIds = users.Select(u => u.Id).ToHashSet();
        foreach (var id in request.UserIds.Where(id => !foundIds.Contains(id)))
        {
            errors.Add(new BulkActionError(id, "User not found"));
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Admin {AdminUserId} bulk suspended {SuccessCount} users",
            adminUserId,
            successCount
        );

        return new BulkActionResult(successCount, errors.Count, errors);
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

        var userSet = _dbContext.Set<User>();
        var postSet = _dbContext.Set<Post>();
        var reportSet = _dbContext.Set<ContentReport>();
        var creatorSet = _dbContext.Set<Creator>();
        var engagementSet = _dbContext.Set<Engagement>();
        var clickSet = _dbContext.Set<ClickEvent>();

        // User stats — all CountAsync, no data loaded into memory
        var totalUsers = await userSet.CountAsync(cancellationToken);
        var activeUsers = await userSet.CountAsync(u => u.IsActive, cancellationToken);
        var suspendedUsers = await userSet.CountAsync(u => !u.IsActive, cancellationToken);
        var shoppers = await userSet.CountAsync(
            u => u.UserType == UserType.Shopper,
            cancellationToken
        );
        var creatorsCount = await userSet.CountAsync(
            u => u.UserType == UserType.Creator,
            cancellationToken
        );
        var retailers = await userSet.CountAsync(
            u => u.UserType == UserType.Retailer,
            cancellationToken
        );
        var admins = await userSet.CountAsync(u => u.UserType == UserType.Admin, cancellationToken);
        var newUsersLast7 = await userSet.CountAsync(
            u => u.CreatedAt >= sevenDaysAgo,
            cancellationToken
        );
        var newUsersLast30 = await userSet.CountAsync(
            u => u.CreatedAt >= thirtyDaysAgo,
            cancellationToken
        );

        var userStats = new UserStats(
            TotalUsers: totalUsers,
            ActiveUsers: activeUsers,
            SuspendedUsers: suspendedUsers,
            Shoppers: shoppers,
            Creators: creatorsCount,
            Retailers: retailers,
            Admins: admins,
            NewUsersLast7Days: newUsersLast7,
            NewUsersLast30Days: newUsersLast30
        );

        // Content stats
        var totalPosts = await postSet.CountAsync(cancellationToken);
        var publishedPosts = await postSet.CountAsync(
            p => p.Status == PostStatus.Published,
            cancellationToken
        );
        var pendingReviewPosts = await postSet.CountAsync(
            p => p.Status == PostStatus.PendingReview,
            cancellationToken
        );
        var draftPosts = await postSet.CountAsync(
            p => p.Status == PostStatus.Draft,
            cancellationToken
        );
        var rejectedPosts = await postSet.CountAsync(
            p => p.Status == PostStatus.Rejected,
            cancellationToken
        );
        var flaggedPosts = await postSet.CountAsync(
            p => p.Status == PostStatus.Flagged,
            cancellationToken
        );
        var removedPosts = await postSet.CountAsync(
            p => p.Status == PostStatus.Removed,
            cancellationToken
        );
        var totalReports = await reportSet.CountAsync(cancellationToken);
        var pendingReports = await reportSet.CountAsync(
            r => r.Status == ReportStatus.Pending,
            cancellationToken
        );
        var postsLast7 = await postSet.CountAsync(
            p => p.CreatedAt >= sevenDaysAgo,
            cancellationToken
        );
        var postsLast30 = await postSet.CountAsync(
            p => p.CreatedAt >= thirtyDaysAgo,
            cancellationToken
        );

        var contentStats = new ContentStats(
            TotalPosts: totalPosts,
            PublishedPosts: publishedPosts,
            PendingReviewPosts: pendingReviewPosts,
            DraftPosts: draftPosts,
            RejectedPosts: rejectedPosts,
            FlaggedPosts: flaggedPosts,
            RemovedPosts: removedPosts,
            TotalReports: totalReports,
            PendingReports: pendingReports,
            PostsLast7Days: postsLast7,
            PostsLast30Days: postsLast30
        );

        // Verification stats
        var pendingVerifications = await creatorSet.CountAsync(
            c => c.VerificationStatus == VerificationStatus.Pending,
            cancellationToken
        );
        var approvedCreators = await creatorSet.CountAsync(
            c => c.VerificationStatus == VerificationStatus.Approved,
            cancellationToken
        );
        var rejectedVerifications = await creatorSet.CountAsync(
            c => c.VerificationStatus == VerificationStatus.Rejected,
            cancellationToken
        );
        var totalCreators = await creatorSet.CountAsync(cancellationToken);

        var verificationStats = new VerificationStats(
            PendingVerifications: pendingVerifications,
            ApprovedCreators: approvedCreators,
            RejectedVerifications: rejectedVerifications,
            TotalCreators: totalCreators
        );

        // Engagement stats
        var totalViews = await engagementSet.CountAsync(
            e => e.Type == EngagementType.View,
            cancellationToken
        );
        var totalLikes = await engagementSet.CountAsync(
            e => e.Type == EngagementType.Like,
            cancellationToken
        );
        var totalSaves = await engagementSet.CountAsync(
            e => e.Type == EngagementType.Save,
            cancellationToken
        );
        var totalClicks = await clickSet.CountAsync(cancellationToken);
        var viewsLast7 = await engagementSet.CountAsync(
            e => e.Type == EngagementType.View && e.CreatedAt >= sevenDaysAgo,
            cancellationToken
        );
        var clicksLast7 = await clickSet.CountAsync(
            c => c.CreatedAt >= sevenDaysAgo,
            cancellationToken
        );

        var engagementStats = new EngagementStats(
            TotalViews: totalViews,
            TotalLikes: totalLikes,
            TotalSaves: totalSaves,
            TotalClicks: totalClicks,
            ViewsLast7Days: viewsLast7,
            ClicksLast7Days: clicksLast7
        );

        return new PlatformStatsResponse(
            userStats,
            contentStats,
            verificationStats,
            engagementStats
        );
    }

    public async Task<AdminAnalyticsResponse> GetPlatformAnalyticsAsync(
        AdminAnalyticsRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var endDate = request.EndDate?.Date ?? DateTime.UtcNow.Date;
        var startDate = request.StartDate?.Date ?? endDate.AddDays(-30);
        var today = DateTime.UtcNow.Date;

        var startOnly = DateOnly.FromDateTime(startDate);
        var endOnly = DateOnly.FromDateTime(endDate);
        var todayOnly = DateOnly.FromDateTime(today);

        // Query pre-aggregated snapshots for historical days (before today)
        var historicalEnd = endOnly < todayOnly ? endOnly : todayOnly.AddDays(-1);
        var snapshots =
            startOnly <= historicalEnd
                ? await _dbContext
                    .Set<DailyMetricSnapshot>()
                    .Where(s =>
                        s.Scope == "platform" && s.Date >= startOnly && s.Date <= historicalEnd
                    )
                    .ToListAsync(cancellationToken)
                : new List<DailyMetricSnapshot>();

        var snapshotsByDate = snapshots.ToDictionary(s => s.Date);

        // Query raw tables only for today (if in range)
        AdminDailyMetrics? todayMetrics = null;
        if (endDate >= today && startDate <= today)
        {
            var todayStart = today;
            var todayEnd = today.AddDays(1);
            var todayNewUsers = await _dbContext
                .Set<User>()
                .CountAsync(
                    u => u.CreatedAt >= todayStart && u.CreatedAt < todayEnd,
                    cancellationToken
                );
            var todayPosts = await _dbContext
                .Set<Post>()
                .CountAsync(
                    p =>
                        p.Status == PostStatus.Published
                        && p.PublishedAt >= todayStart
                        && p.PublishedAt < todayEnd,
                    cancellationToken
                );
            var todayViews = await _dbContext
                .Set<Engagement>()
                .CountAsync(
                    e =>
                        e.Type == EngagementType.View
                        && e.CreatedAt >= todayStart
                        && e.CreatedAt < todayEnd,
                    cancellationToken
                );
            var todayLikes = await _dbContext
                .Set<Engagement>()
                .CountAsync(
                    e =>
                        e.Type == EngagementType.Like
                        && e.CreatedAt >= todayStart
                        && e.CreatedAt < todayEnd,
                    cancellationToken
                );
            var todaySaves = await _dbContext
                .Set<Engagement>()
                .CountAsync(
                    e =>
                        e.Type == EngagementType.Save
                        && e.CreatedAt >= todayStart
                        && e.CreatedAt < todayEnd,
                    cancellationToken
                );
            var todayClicks = await _dbContext
                .Set<ClickEvent>()
                .CountAsync(
                    c => c.CreatedAt >= todayStart && c.CreatedAt < todayEnd,
                    cancellationToken
                );

            todayMetrics = new AdminDailyMetrics(
                today,
                todayNewUsers,
                todayPosts,
                todayViews,
                todayLikes,
                todaySaves,
                todayClicks
            );
        }

        // Build daily metrics list, filling gaps with zeros
        var totalDays = (int)(endDate - startDate).TotalDays + 1;
        var dailyMetrics = Enumerable
            .Range(0, totalDays)
            .Select(offset =>
            {
                var day = startDate.AddDays(offset);
                var dayOnly = DateOnly.FromDateTime(day);

                if (dayOnly == todayOnly && todayMetrics != null)
                    return todayMetrics;

                if (snapshotsByDate.TryGetValue(dayOnly, out var snap))
                    return new AdminDailyMetrics(
                        day,
                        snap.NewUsers,
                        snap.PostsPublished,
                        (int)snap.Views,
                        (int)snap.Likes,
                        (int)snap.Saves,
                        (int)snap.Clicks
                    );

                return new AdminDailyMetrics(day, 0, 0, 0, 0, 0, 0);
            })
            .ToList();

        // Summary: aggregate all daily metrics
        var summary = new AdminAnalyticsSummary(
            NewUsers: dailyMetrics.Sum(d => d.NewUsers),
            PostsPublished: dailyMetrics.Sum(d => d.PostsPublished),
            Views: dailyMetrics.Sum(d => d.Views),
            Likes: dailyMetrics.Sum(d => d.Likes),
            Saves: dailyMetrics.Sum(d => d.Saves),
            Clicks: dailyMetrics.Sum(d => d.Clicks)
        );

        // Top 10 creators: aggregate from creator-scoped snapshots
        var creatorSnapshots =
            startOnly <= historicalEnd
                ? await _dbContext
                    .Set<DailyMetricSnapshot>()
                    .Where(s =>
                        s.Scope.StartsWith("creator:") && s.Date >= startOnly && s.Date <= endOnly
                    )
                    .GroupBy(s => s.ScopeEntityId)
                    .Select(g => new
                    {
                        CreatorId = g.Key,
                        Views = (int)g.Sum(s => s.Views),
                        Likes = (int)g.Sum(s => s.Likes),
                        Clicks = (int)g.Sum(s => s.Clicks),
                    })
                    .OrderByDescending(c => c.Views + c.Likes + c.Clicks)
                    .Take(10)
                    .ToListAsync(cancellationToken)
                : [];

        var creatorIds = creatorSnapshots
            .Where(c => c.CreatorId.HasValue)
            .Select(c => c.CreatorId!.Value)
            .ToList();

        var creators =
            creatorIds.Count > 0
                ? await _dbContext
                    .Set<Creator>()
                    .Where(c => creatorIds.Contains(c.Id))
                    .ToDictionaryAsync(c => c.Id, cancellationToken)
                : new Dictionary<Guid, Creator>();

        var topCreators = creatorSnapshots
            .Where(c => c.CreatorId.HasValue && creators.ContainsKey(c.CreatorId.Value))
            .Select(c =>
            {
                var creator = creators[c.CreatorId!.Value];
                return new TopCreatorAnalytics(
                    CreatorId: creator.Id,
                    DisplayName: creator.DisplayName,
                    IsVerified: creator.VerificationStatus == VerificationStatus.Approved,
                    TotalEngagements: c.Views + c.Likes + c.Clicks,
                    Views: c.Views,
                    Likes: c.Likes,
                    Clicks: c.Clicks
                );
            })
            .ToList();

        return new AdminAnalyticsResponse(summary, dailyMetrics, topCreators);
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
