using Lyke.Core.Enums;

namespace Lyke.Core.Entities;

/// <summary>
/// Stores the user's Kibbe style profile computed from the quiz.
/// </summary>
public class StyleProfile : BaseEntity
{
    public Guid UserId { get; set; }

    // Computed result from the quiz
    public KibbeFamily? PrimaryFamily { get; set; }
    public KibbeFamily? RunnerUpFamily { get; set; }
    public bool IsMixed { get; set; }
    public KibbeConfidence? Confidence { get; set; }

    // Section dominance (which family dominated each quiz section)
    public KibbeFamily? BoneDominance { get; set; }
    public KibbeFamily? FleshDominance { get; set; }
    public KibbeFamily? FaceDominance { get; set; }

    // Raw answer counts (A-E) stored as JSON: e.g. {"A":5,"B":3,"C":2,"D":1,"E":1}
    public string? CountsJson { get; set; }

    public DateTime? ComputedAt { get; set; }

    // Optional user override (self-selected type)
    public KibbeFamily? OverrideFamily { get; set; }
    public bool IsUserOverride { get; set; }
    public DateTime? OverrideSetAt { get; set; }

    // Navigation
    public User User { get; set; } = null!;
}
