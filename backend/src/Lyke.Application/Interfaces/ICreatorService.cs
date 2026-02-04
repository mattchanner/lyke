using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Creator;

namespace Lyke.Application.Interfaces;

public interface ICreatorService
{
    // Registration & Profile
    Task<CreatorProfileResponse> RegisterAsCreatorAsync(Guid userId, RegisterCreatorRequest request, CancellationToken cancellationToken = default);
    Task<CreatorProfileResponse> GetCreatorProfileAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<CreatorProfileResponse> UpdateCreatorProfileAsync(Guid userId, UpdateCreatorProfileRequest request, CancellationToken cancellationToken = default);

    // Post Management
    Task<CreatorPostResponse> CreatePostAsync(Guid userId, CreatePostRequest request, CancellationToken cancellationToken = default);
    Task<CreatorPostResponse> UpdatePostAsync(Guid userId, Guid postId, UpdatePostRequest request, CancellationToken cancellationToken = default);
    Task DeletePostAsync(Guid userId, Guid postId, CancellationToken cancellationToken = default);
    Task<CreatorPostResponse> GetPostAsync(Guid userId, Guid postId, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<CreatorPostResponse> Posts, PaginationMeta Meta)> GetPostsAsync(Guid userId, CreatorPostsRequest request, CancellationToken cancellationToken = default);
    Task<CreatorPostResponse> SubmitPostForReviewAsync(Guid userId, Guid postId, CancellationToken cancellationToken = default);

    // Analytics & Earnings
    Task<CreatorAnalyticsResponse> GetAnalyticsAsync(Guid userId, CreatorAnalyticsRequest request, CancellationToken cancellationToken = default);
    Task<EarningsSummaryResponse> GetEarningsSummaryAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<EarningDetailResponse> Earnings, PaginationMeta Meta)> GetEarningsHistoryAsync(Guid userId, EarningsHistoryRequest request, CancellationToken cancellationToken = default);
}
