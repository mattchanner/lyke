using System.Text.Json;
using Lyke.Application.DTOs.Style;
using Lyke.Application.Interfaces;
using Lyke.Core.Entities;
using Lyke.Core.Enums;
using Lyke.Core.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Lyke.Application.Services;

public class KibbeQuizService : IKibbeQuizService
{
    private readonly DbContext _dbContext;

    private static readonly Dictionary<string, KibbeFamily> OptionToFamily = new()
    {
        { "A", KibbeFamily.Dramatic },
        { "B", KibbeFamily.Natural },
        { "C", KibbeFamily.Classic },
        { "D", KibbeFamily.Gamine },
        { "E", KibbeFamily.Romantic },
    };

    private static readonly HashSet<string> ValidOptions = new() { "A", "B", "C", "D", "E" };
    private static readonly HashSet<string> ValidSections = new() { "bone", "flesh", "face" };

    // Threshold: if top-2 margin <= this, flag as mixed
    private const int MixedThreshold = 2;

    public KibbeQuizService(DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Converts margin between top two scores to confidence level.
    /// >= 5 questions apart -> High
    /// 3-4 questions apart -> Medium
    /// <= 2 questions apart -> Low
    /// </summary>
    private static KibbeConfidence ToConfidence(int margin) =>
        margin switch
        {
            >= 5 => KibbeConfidence.High,
            >= 3 => KibbeConfidence.Medium,
            _ => KibbeConfidence.Low,
        };

    public KibbeScoreResponse Score(KibbeScoreRequest request)
    {
        if (request.Answers == null || request.Answers.Count == 0)
        {
            throw new ValidationException("Answers", "Answers are required");
        }

        // Validate all options are A-E and sections are valid
        foreach (var answer in request.Answers)
        {
            var option = answer.SelectedOption.ToUpperInvariant();
            if (!ValidOptions.Contains(option))
            {
                throw new ValidationException(
                    "SelectedOption",
                    $"Invalid option '{answer.SelectedOption}' for question '{answer.QuestionId}'"
                );
            }

            var section = answer.SectionId.ToLowerInvariant();
            if (!ValidSections.Contains(section))
            {
                throw new ValidationException(
                    "SectionId",
                    $"Invalid section '{answer.SectionId}' for question '{answer.QuestionId}'"
                );
            }
        }

        // Initialize overall counts
        var overallCounts = new Dictionary<KibbeFamily, int>
        {
            { KibbeFamily.Dramatic, 0 },
            { KibbeFamily.Natural, 0 },
            { KibbeFamily.Classic, 0 },
            { KibbeFamily.Gamine, 0 },
            { KibbeFamily.Romantic, 0 },
        };

        // Initialize per-section counts
        var sectionCounts = new Dictionary<string, Dictionary<KibbeFamily, int>>
        {
            {
                "bone",
                new Dictionary<KibbeFamily, int>
                {
                    { KibbeFamily.Dramatic, 0 },
                    { KibbeFamily.Natural, 0 },
                    { KibbeFamily.Classic, 0 },
                    { KibbeFamily.Gamine, 0 },
                    { KibbeFamily.Romantic, 0 },
                }
            },
            {
                "flesh",
                new Dictionary<KibbeFamily, int>
                {
                    { KibbeFamily.Dramatic, 0 },
                    { KibbeFamily.Natural, 0 },
                    { KibbeFamily.Classic, 0 },
                    { KibbeFamily.Gamine, 0 },
                    { KibbeFamily.Romantic, 0 },
                }
            },
            {
                "face",
                new Dictionary<KibbeFamily, int>
                {
                    { KibbeFamily.Dramatic, 0 },
                    { KibbeFamily.Natural, 0 },
                    { KibbeFamily.Classic, 0 },
                    { KibbeFamily.Gamine, 0 },
                    { KibbeFamily.Romantic, 0 },
                }
            },
        };

        // Count answers
        foreach (var answer in request.Answers)
        {
            var option = answer.SelectedOption.ToUpperInvariant();
            var family = OptionToFamily[option];
            overallCounts[family]++;

            var sectionId = answer.SectionId.ToLowerInvariant();
            if (sectionCounts.TryGetValue(sectionId, out var sectionDict))
            {
                sectionDict[family]++;
            }
        }

        // Rank families by overall count
        var ranked = overallCounts.OrderByDescending(kv => kv.Value).ToList();
        var primaryFamily = ranked[0].Key;
        var primaryCount = ranked[0].Value;
        var runnerUpCount = ranked[1].Value;
        var margin = primaryCount - runnerUpCount;

        // Runner-up: may be null if no second vote exists
        KibbeFamily? runnerUpFamily = runnerUpCount > 0 ? ranked[1].Key : null;

        // Mixed flag based on margin
        var isMixed = margin <= MixedThreshold;

        // Section dominance
        static KibbeFamily DominantInSection(Dictionary<KibbeFamily, int> counts) =>
            counts.OrderByDescending(kv => kv.Value).First().Key;

        var boneDominance = DominantInSection(sectionCounts["bone"]);
        var fleshDominance = DominantInSection(sectionCounts["flesh"]);
        var faceDominance = DominantInSection(sectionCounts["face"]);

        // Section-level mixed check also contributes to isMixed flag
        if (
            boneDominance != primaryFamily
            || fleshDominance != primaryFamily
            || faceDominance != primaryFamily
        )
        {
            isMixed = true;
        }

        var confidence = ToConfidence(margin);

        var counts = new KibbeCounts(
            overallCounts[KibbeFamily.Dramatic],
            overallCounts[KibbeFamily.Natural],
            overallCounts[KibbeFamily.Classic],
            overallCounts[KibbeFamily.Gamine],
            overallCounts[KibbeFamily.Romantic]
        );

        return new KibbeScoreResponse(
            primaryFamily,
            runnerUpFamily,
            new KibbeSectionDominance(boneDominance, fleshDominance, faceDominance),
            isMixed,
            confidence,
            counts
        );
    }

    public async Task<StyleProfileResponse> SaveAsync(
        Guid userId,
        KibbeScoreResponse score,
        CancellationToken cancellationToken = default
    )
    {
        var profile = await _dbContext
            .Set<StyleProfile>()
            .Where(sp => sp.UserId == userId)
            .FirstOrDefaultAsync(cancellationToken);

        var countsJson = JsonSerializer.Serialize(
            new
            {
                A = score.Counts.A,
                B = score.Counts.B,
                C = score.Counts.C,
                D = score.Counts.D,
                E = score.Counts.E,
            }
        );

        if (profile == null)
        {
            profile = new StyleProfile
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                PrimaryFamily = score.PrimaryFamily,
                RunnerUpFamily = score.RunnerUpFamily,
                IsMixed = score.IsMixed,
                Confidence = score.Confidence,
                BoneDominance = score.SectionDominance.Bone,
                FleshDominance = score.SectionDominance.Flesh,
                FaceDominance = score.SectionDominance.Face,
                CountsJson = countsJson,
                ComputedAt = DateTime.UtcNow,
                IsUserOverride = false,
                OverrideFamily = null,
                OverrideSetAt = null,
            };
            _dbContext.Set<StyleProfile>().Add(profile);
        }
        else
        {
            profile.PrimaryFamily = score.PrimaryFamily;
            profile.RunnerUpFamily = score.RunnerUpFamily;
            profile.IsMixed = score.IsMixed;
            profile.Confidence = score.Confidence;
            profile.BoneDominance = score.SectionDominance.Bone;
            profile.FleshDominance = score.SectionDominance.Flesh;
            profile.FaceDominance = score.SectionDominance.Face;
            profile.CountsJson = countsJson;
            profile.ComputedAt = DateTime.UtcNow;
            // Clear any existing override when retaking the quiz
            profile.IsUserOverride = false;
            profile.OverrideFamily = null;
            profile.OverrideSetAt = null;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return ToResponse(profile);
    }

    public async Task<StyleProfileResponse?> GetProfileAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        var profile = await _dbContext
            .Set<StyleProfile>()
            .AsNoTracking()
            .Where(sp => sp.UserId == userId)
            .FirstOrDefaultAsync(cancellationToken);

        return profile == null ? null : ToResponse(profile);
    }

    public async Task<StyleProfileResponse> OverrideAsync(
        Guid userId,
        KibbeFamily family,
        CancellationToken cancellationToken = default
    )
    {
        var profile = await _dbContext
            .Set<StyleProfile>()
            .Where(sp => sp.UserId == userId)
            .FirstOrDefaultAsync(cancellationToken);

        if (profile == null)
        {
            // Create a new profile with just the override
            profile = new StyleProfile
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                IsUserOverride = true,
                OverrideFamily = family,
                OverrideSetAt = DateTime.UtcNow,
            };
            _dbContext.Set<StyleProfile>().Add(profile);
        }
        else
        {
            profile.IsUserOverride = true;
            profile.OverrideFamily = family;
            profile.OverrideSetAt = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return ToResponse(profile);
    }

    public async Task<StyleProfileResponse> ClearOverrideAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        var profile = await _dbContext
            .Set<StyleProfile>()
            .Where(sp => sp.UserId == userId)
            .FirstOrDefaultAsync(cancellationToken);

        if (profile == null)
        {
            throw new NotFoundException("StyleProfile", userId);
        }

        profile.IsUserOverride = false;
        profile.OverrideFamily = null;
        profile.OverrideSetAt = null;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return ToResponse(profile);
    }

    private static StyleProfileResponse ToResponse(StyleProfile profile)
    {
        KibbeSectionDominance? sectionDominance = null;
        if (
            profile.BoneDominance.HasValue
            && profile.FleshDominance.HasValue
            && profile.FaceDominance.HasValue
        )
        {
            sectionDominance = new KibbeSectionDominance(
                profile.BoneDominance.Value,
                profile.FleshDominance.Value,
                profile.FaceDominance.Value
            );
        }

        return new StyleProfileResponse(
            profile.PrimaryFamily,
            profile.RunnerUpFamily,
            profile.IsMixed,
            profile.Confidence,
            sectionDominance,
            profile.IsUserOverride,
            profile.OverrideFamily,
            profile.ComputedAt,
            profile.UpdatedAt
        );
    }
}
