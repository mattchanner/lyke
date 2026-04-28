using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Creator;
using Lyke.Application.DTOs.Post;

namespace Lyke.Application.Interfaces;

public interface IPostAuthorService
{
    Task<AuthorPostResponse> CreatePostAsync(
        Guid userId,
        CreatePostRequest request,
        CancellationToken cancellationToken = default
    );
    Task<AuthorPostResponse> UpdatePostAsync(
        Guid userId,
        Guid postId,
        UpdatePostRequest request,
        CancellationToken cancellationToken = default
    );
    Task DeletePostAsync(
        Guid userId,
        Guid postId,
        CancellationToken cancellationToken = default
    );
    Task<AuthorPostResponse> GetPostAsync(
        Guid userId,
        Guid postId,
        CancellationToken cancellationToken = default
    );
    Task<(IReadOnlyList<AuthorPostResponse> Posts, PaginationMeta Meta)> GetPostsAsync(
        Guid userId,
        AuthorPostsRequest request,
        CancellationToken cancellationToken = default
    );
    Task<AuthorPostResponse> SubmitPostForReviewAsync(
        Guid userId,
        Guid postId,
        CancellationToken cancellationToken = default
    );
}
