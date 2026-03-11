namespace Lyke.Core.Enums;

/// <summary>
/// Confidence level of a Kibbe quiz result based on the margin between top answers.
/// </summary>
public enum KibbeConfidence
{
    /// <summary>Strong dominance - margin of 5+ between primary and runner-up</summary>
    High = 0,

    /// <summary>Moderate dominance - margin of 3-4 between primary and runner-up</summary>
    Medium = 1,

    /// <summary>Weak dominance - margin of 0-2 between primary and runner-up</summary>
    Low = 2,
}
