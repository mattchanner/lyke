using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Follow;

namespace Lyke.Application.Interfaces;

public interface IFollowService
{
    Task FollowAsync(
        Guid followerUserId,
        Guid creatorId,
        CancellationToken cancellationToken = default
    );
    Task UnfollowAsync(
        Guid followerUserId,
        Guid creatorId,
        CancellationToken cancellationToken = default
    );
    Task<bool> IsFollowingAsync(
        Guid followerUserId,
        Guid creatorId,
        CancellationToken cancellationToken = default
    );
    Task<(IReadOnlyList<FollowedCreatorResponse> Creators, PaginationMeta Meta)> GetFollowingAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    );
    Task<HashSet<Guid>> GetFollowedCreatorIdsAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    );

    // User-based follow methods (works for any user, not just creators)
    Task FollowUserAsync(
        Guid followerUserId,
        Guid followedUserId,
        CancellationToken cancellationToken = default
    );
    Task UnfollowUserAsync(
        Guid followerUserId,
        Guid followedUserId,
        CancellationToken cancellationToken = default
    );
    Task<bool> IsFollowingUserAsync(
        Guid followerUserId,
        Guid followedUserId,
        CancellationToken cancellationToken = default
    );
    Task<HashSet<Guid>> GetFollowedUserIdsAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    );
    Task<(
        IReadOnlyList<FollowedUserResponse> Users,
        PaginationMeta Meta
    )> GetFollowingUsersAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    );
}
