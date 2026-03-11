using Lyke.Core.Enums;

namespace Lyke.Application.DTOs.Style;

/// <summary>
/// The user's saved style profile.
/// </summary>
public record StyleProfileResponse(
    KibbeFamily? PrimaryFamily,
    KibbeFamily? RunnerUpFamily,
    bool IsMixed,
    KibbeConfidence? Confidence,
    KibbeSectionDominance? SectionDominance,
    bool IsUserOverride,
    KibbeFamily? OverrideFamily,
    DateTime? ComputedAt,
    DateTime? LastUpdatedAt
);

/// <summary>
/// Request to save a quiz result to the user's style profile.
/// </summary>
public record SaveStyleProfileRequest(KibbeScoreResponse Score);

/// <summary>
/// Request to override the computed style profile with a self-selected family.
/// </summary>
public record OverrideStyleProfileRequest(KibbeFamily Family);
