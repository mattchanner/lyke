using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Follow;
using Lyke.Application.Interfaces;
using Lyke.Core.Entities;
using Lyke.Core.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Lyke.Application.Services;

public class FollowService : IFollowService
{
    private readonly DbContext _dbContext;
    private readonly ILogger<FollowService> _logger;

    public FollowService(DbContext dbContext, ILogger<FollowService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task FollowAsync(
        Guid followerUserId,
        Guid creatorId,
        CancellationToken cancellationToken = default
    )
    {
        var creator = await _dbContext
            .Set<Creator>()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == creatorId, cancellationToken);

        if (creator == null)
            throw new NotFoundException("Creator", creatorId);

        if (creator.UserId == followerUserId)
            throw new ValidationException("Follow", "You cannot follow yourself");

        var existing = await _dbContext
            .Set<UserFollow>()
            .AnyAsync(
                uf => uf.FollowerUserId == followerUserId && uf.FollowedUserId == creator.UserId,
                cancellationToken
            );

        if (existing)
            return; // Already following, idempotent

        var follow = new UserFollow
        {
            Id = Guid.NewGuid(),
            FollowerUserId = followerUserId,
            FollowedUserId = creator.UserId,
            CreatedAt = DateTime.UtcNow,
        };

        await _dbContext.Set<UserFollow>().AddAsync(follow, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "User {FollowerUserId} followed creator {CreatorId}",
            followerUserId,
            creatorId
        );
    }

    public async Task UnfollowAsync(
        Guid followerUserId,
        Guid creatorId,
        CancellationToken cancellationToken = default
    )
    {
        var creator = await _dbContext
            .Set<Creator>()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == creatorId, cancellationToken);

        if (creator == null)
            throw new NotFoundException("Creator", creatorId);

        var follow = await _dbContext
            .Set<UserFollow>()
            .FirstOrDefaultAsync(
                uf => uf.FollowerUserId == followerUserId && uf.FollowedUserId == creator.UserId,
                cancellationToken
            );

        if (follow != null)
        {
            _dbContext.Set<UserFollow>().Remove(follow);
            await _dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation(
                "User {FollowerUserId} unfollowed creator {CreatorId}",
                followerUserId,
                creatorId
            );
        }
    }

    public async Task<bool> IsFollowingAsync(
        Guid followerUserId,
        Guid creatorId,
        CancellationToken cancellationToken = default
    )
    {
        var creator = await _dbContext
            .Set<Creator>()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == creatorId, cancellationToken);

        if (creator == null)
            return false;

        return await _dbContext
            .Set<UserFollow>()
            .AnyAsync(
                uf => uf.FollowerUserId == followerUserId && uf.FollowedUserId == creator.UserId,
                cancellationToken
            );
    }

    public async Task<(
        IReadOnlyList<FollowedCreatorResponse> Creators,
        PaginationMeta Meta
    )> GetFollowingAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    )
    {
        var query = _dbContext
            .Set<UserFollow>()
            .AsNoTracking()
            .Where(uf => uf.FollowerUserId == userId);

        var totalCount = await query.CountAsync(cancellationToken);

        var follows = await query
            .OrderByDescending(uf => uf.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(uf => uf.Followed)
                .ThenInclude(u => u.Creator)
            .Select(uf => new FollowedCreatorResponse(
                uf.Followed.Creator!.Id,
                uf.Followed.Creator!.DisplayName,
                uf.Followed.Creator!.IsVerified,
                uf.Followed.ProfileImageUrl,
                uf.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        var meta = new PaginationMeta
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
        };

        return (follows, meta);
    }

    public async Task<HashSet<Guid>> GetFollowedCreatorIdsAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        var followedUserIds = await _dbContext
            .Set<UserFollow>()
            .AsNoTracking()
            .Where(uf => uf.FollowerUserId == userId)
            .Select(uf => uf.FollowedUserId)
            .ToListAsync(cancellationToken);

        // Map user IDs to creator IDs
        var creatorIds = await _dbContext
            .Set<Creator>()
            .AsNoTracking()
            .Where(c => followedUserIds.Contains(c.UserId))
            .Select(c => c.Id)
            .ToListAsync(cancellationToken);

        return creatorIds.ToHashSet();
    }
}
