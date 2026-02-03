using Lyke.Application.DTOs.Profile;

namespace Lyke.Application.Interfaces;

public interface IProfileService
{
    // User Profile
    Task<UserProfileResponse> GetProfileAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<UserProfileResponse> UpdateProfileAsync(Guid userId, UpdateProfileRequest request, CancellationToken cancellationToken = default);

    // Body Profile
    Task<BodyProfileResponse?> GetBodyProfileAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<BodyProfileResponse> CreateBodyProfileAsync(Guid userId, CreateBodyProfileRequest request, CancellationToken cancellationToken = default);
    Task<BodyProfileResponse> UpdateBodyProfileAsync(Guid userId, UpdateBodyProfileRequest request, CancellationToken cancellationToken = default);
    Task DeleteBodyProfileAsync(Guid userId, CancellationToken cancellationToken = default);

    // Anonymized profile for display to other users
    Task<AnonymizedBodyProfileResponse?> GetAnonymizedBodyProfileAsync(Guid userId, CancellationToken cancellationToken = default);

    // Lookups
    Task<IReadOnlyList<BodyTypeResponse>> GetBodyTypesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FitPreferenceResponse>> GetFitPreferencesAsync(CancellationToken cancellationToken = default);
}
