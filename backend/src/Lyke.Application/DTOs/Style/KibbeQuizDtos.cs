using Lyke.Core.Enums;

namespace Lyke.Application.DTOs.Style;

/// <summary>
/// A single answer in the Kibbe quiz.
/// </summary>
/// <param name="SectionId">The quiz section: "bone", "flesh", or "face"</param>
/// <param name="QuestionId">The question identifier, e.g., "bone_1", "flesh_2"</param>
/// <param name="SelectedOption">The selected option: "A", "B", "C", "D", or "E"</param>
public record KibbeAnswer(string SectionId, string QuestionId, string SelectedOption);

/// <summary>
/// Request to score a completed Kibbe quiz.
/// </summary>
public record KibbeScoreRequest(List<KibbeAnswer> Answers);

/// <summary>
/// Per-section dominance showing which family dominated each quiz section.
/// </summary>
public record KibbeSectionDominance(KibbeFamily Bone, KibbeFamily Flesh, KibbeFamily Face);

/// <summary>
/// Raw answer counts for each family option (A-E).
/// </summary>
public record KibbeCounts(int A, int B, int C, int D, int E);

/// <summary>
/// Response from scoring the Kibbe quiz.
/// </summary>
public record KibbeScoreResponse(
    KibbeFamily PrimaryFamily,
    KibbeFamily? RunnerUpFamily,
    KibbeSectionDominance SectionDominance,
    bool IsMixed,
    KibbeConfidence Confidence,
    KibbeCounts Counts
);
