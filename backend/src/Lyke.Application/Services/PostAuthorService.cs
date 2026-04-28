using System.Text.Json;
using Lyke.Application.Configuration;
using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Creator;
using Lyke.Application.DTOs.Post;
using Lyke.Application.Interfaces;
using Lyke.Core.Entities;
using Lyke.Core.Enums;
using Lyke.Core.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lyke.Application.Services;

public class PostAuthorService : IPostAuthorService
{
    private readonly DbContext _dbContext;
    private readonly CreatorSettings _creatorSettings;
    private readonly ILogger<PostAuthorService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public PostAuthorService(
        DbContext dbContext,
        IOptions<CreatorSettings> creatorSettings,
        ILogger<PostAuthorService> logger
    )
    {
        _dbContext = dbContext;
        _creatorSettings = creatorSettings.Value;
        _logger = logger;
    }

    public async Task<AuthorPostResponse> CreatePostAsync(
        Guid userId,
        CreatePostRequest request,
        CancellationToken cancellationToken = default
    )
    {
        // Check draft post limit
        var draftCount = await _dbContext
            .Set<Post>()
            .CountAsync(
                p => p.AuthorUserId == userId && p.Status == PostStatus.Draft,
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

        // Look up Creator record if user is a creator
        var creator = await _dbContext
            .Set<Creator>()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

        var post = new Post
        {
            Id = Guid.NewGuid(),
            AuthorUserId = userId,
            CreatorId = creator?.Id,
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

        await CreatePostProductsAsync(post.Id, request.Products, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User {UserId} created post {PostId}", userId, post.Id);

        return await GetPostAsync(userId, post.Id, cancellationToken);
    }

    public async Task<AuthorPostResponse> UpdatePostAsync(
        Guid userId,
        Guid postId,
        UpdatePostRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var post = await _dbContext
            .Set<Post>()
            .FirstOrDefaultAsync(
                p => p.Id == postId && p.AuthorUserId == userId,
                cancellationToken
            );

        if (post == null)
        {
            throw new NotFoundException(nameof(Post), postId);
        }

        if (post.Status != PostStatus.Draft && post.Status != PostStatus.Rejected)
        {
            throw new ValidationException(
                "Post",
                "Only draft or rejected posts can be updated"
            );
        }

        if (request.Title != null)
            post.Title = request.Title;
        if (request.Description != null)
            post.Description = request.Description;
        if (request.MediaType.HasValue)
            post.MediaType = request.MediaType.Value;
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
            post.ThumbnailUrls = JsonSerializer.Serialize(request.ThumbnailUrls, JsonOptions);

        if (request.Products != null)
        {
            if (request.Products.Count > _creatorSettings.MaxProductsPerPost)
            {
                throw new ValidationException(
                    "Products",
                    $"Maximum {_creatorSettings.MaxProductsPerPost} products allowed"
                );
            }

            // Validate products exist
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

            // Remove existing post products
            var existingProducts = await _dbContext
                .Set<PostProduct>()
                .Where(pp => pp.PostId == postId)
                .ToListAsync(cancellationToken);
            _dbContext.Set<PostProduct>().RemoveRange(existingProducts);

            await CreatePostProductsAsync(postId, request.Products, cancellationToken);
        }

        if (post.Status == PostStatus.Rejected)
        {
            post.Status = PostStatus.Draft;
            post.ModerationNotes = null;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User {UserId} updated post {PostId}", userId, postId);

        return await GetPostAsync(userId, postId, cancellationToken);
    }

    public async Task DeletePostAsync(
        Guid userId,
        Guid postId,
        CancellationToken cancellationToken = default
    )
    {
        var post = await _dbContext
            .Set<Post>()
            .FirstOrDefaultAsync(
                p => p.Id == postId && p.AuthorUserId == userId,
                cancellationToken
            );

        if (post == null)
        {
            throw new NotFoundException(nameof(Post), postId);
        }

        if (post.Status == PostStatus.Published)
        {
            throw new ValidationException("Post", "Published posts cannot be deleted directly");
        }

        _dbContext.Set<Post>().Remove(post);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User {UserId} deleted post {PostId}", userId, postId);
    }

    public async Task<AuthorPostResponse> GetPostAsync(
        Guid userId,
        Guid postId,
        CancellationToken cancellationToken = default
    )
    {
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
                p => p.Id == postId && p.AuthorUserId == userId,
                cancellationToken
            );

        if (post == null)
        {
            throw new NotFoundException(nameof(Post), postId);
        }

        return MapToAuthorPostResponse(post);
    }

    public async Task<(
        IReadOnlyList<AuthorPostResponse> Posts,
        PaginationMeta Meta
    )> GetPostsAsync(
        Guid userId,
        AuthorPostsRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var query = _dbContext.Set<Post>().Where(p => p.AuthorUserId == userId);

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

        var meta = new PaginationMeta
        {
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount,
        };

        return (posts.Select(MapToAuthorPostResponse).ToList(), meta);
    }

    public async Task<AuthorPostResponse> SubmitPostForReviewAsync(
        Guid userId,
        Guid postId,
        CancellationToken cancellationToken = default
    )
    {
        var post = await _dbContext
            .Set<Post>()
            .FirstOrDefaultAsync(
                p => p.Id == postId && p.AuthorUserId == userId,
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

        post.Status = PostStatus.PendingReview;
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "User {UserId} submitted post {PostId} for review",
            userId,
            postId
        );

        return await GetPostAsync(userId, postId, cancellationToken);
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

            if (productRequest.FitTagIds != null && productRequest.FitTagIds.Any())
            {
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

    private static AuthorPostResponse MapToAuthorPostResponse(Post post)
    {
        var products = post
            .PostProducts.Select(pp => new AuthorPostProductResponse(
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

        var engagements = new AuthorPostEngagementResponse(
            Views: post.Engagements.Count(e => e.Type == EngagementType.View),
            Likes: post.Engagements.Count(e => e.Type == EngagementType.Like),
            Saves: post.Engagements.Count(e => e.Type == EngagementType.Save),
            Shares: post.Engagements.Count(e => e.Type == EngagementType.Share),
            Clicks: post.ClickEvents.Count
        );

        return new AuthorPostResponse(
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
}
