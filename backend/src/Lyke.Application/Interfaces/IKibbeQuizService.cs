using Lyke.Application.DTOs.Style;
using Lyke.Core.Enums;

namespace Lyke.Application.Interfaces;

public interface IKibbeQuizService
{
    /// <summary>
    /// Scores a completed Kibbe quiz. This is a pure computation - no database required.
    /// </summary>
    KibbeScoreResponse Score(KibbeScoreRequest request);

    /// <summary>
    /// Saves the computed quiz result to the user's style profile.
    /// </summary>
    Task<StyleProfileResponse> SaveAsync(
        Guid userId,
        KibbeScoreResponse score,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Gets the user's saved style profile.
    /// </summary>
    Task<StyleProfileResponse?> GetProfileAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Overrides the computed style profile with a self-selected family.
    /// </summary>
    Task<StyleProfileResponse> OverrideAsync(
        Guid userId,
        KibbeFamily family,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Clears the override and reverts to the computed result.
    /// </summary>
    Task<StyleProfileResponse> ClearOverrideAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    );
}
