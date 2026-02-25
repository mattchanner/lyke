using Lyke.Application.DTOs.Quiz;
using Lyke.Application.Interfaces;
using Lyke.Core.Entities;
using Lyke.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Lyke.Application.Services;

public class QuizService : IQuizService
{
    private readonly DbContext _dbContext;
    private readonly ILogger<QuizService> _logger;

    // Body shape names mapped to their database IDs
    private static readonly Dictionary<string, int> BodyShapeIds = new()
    {
        ["Hourglass"] = 1,
        ["Pear"] = 2,
        ["Apple"] = 3,
        ["Rectangle"] = 4,
        ["Inverted Triangle"] = 5
    };

    // Scoring matrices for each question (Q1-Q4)
    // Each matrix maps answer index to score adjustments for each body shape
    // Order: [Hourglass, Pear, Apple, Rectangle, Inverted Triangle]

    // Q1: Shoulder-Hip Comparison
    private static readonly int[][] Q1Scores =
    [
        [0, 0, 1, 1, 3],  // Shoulders wider
        [2, 1, 1, 2, 0],  // Same width
        [0, 3, 0, 1, 0]   // Hips wider
    ];

    // Q2: Waist Definition
    private static readonly int[][] Q2Scores =
    [
        [3, 2, 0, 0, 1],  // Very defined
        [1, 1, 1, 1, 1],  // Moderate
        [0, 0, 2, 3, 1]   // Minimal
    ];

    // Q3: Weight Distribution
    private static readonly int[][] Q3Scores =
    [
        [0, 0, 1, 0, 3],  // Upper body
        [0, 0, 3, 1, 0],  // Midsection
        [1, 3, 0, 0, 0],  // Lower body
        [2, 1, 1, 2, 1]   // Evenly
    ];

    // Q4: Clothing Fit Issues
    private static readonly int[][] Q4Scores =
    [
        [0, 0, 0, 0, 2],  // Tight shoulders
        [0, 0, 2, 0, 0],  // Tight middle
        [1, 2, 0, 0, 0],  // Tight hips
        [0, 0, 0, 2, 0]   // Same everywhere
    ];

    // Body shape names in order of index (matching score arrays)
    private static readonly string[] BodyShapes = ["Hourglass", "Pear", "Apple", "Rectangle", "Inverted Triangle"];

    // Tie-breaker priority (lower index = higher priority)
    private static readonly int[] TieBreakerPriority = [0, 1, 3, 2, 4]; // Hourglass > Pear > Rectangle > Apple > Inverted Triangle

    public QuizService(DbContext dbContext, ILogger<QuizService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<QuizResultResponse> CalculateResultAsync(QuizResultRequest request, CancellationToken cancellationToken = default)
    {
        // Initialize scores for each body shape
        var scores = new int[5]; // [Hourglass, Pear, Apple, Rectangle, Inverted Triangle]

        Stature? stature = null;
        Build? build = null;

        foreach (var answer in request.Answers)
        {
            switch (answer.QuestionId)
            {
                case 1 when answer.AnswerIndex >= 0 && answer.AnswerIndex < Q1Scores.Length:
                    AddScores(scores, Q1Scores[answer.AnswerIndex]);
                    break;
                case 2 when answer.AnswerIndex >= 0 && answer.AnswerIndex < Q2Scores.Length:
                    AddScores(scores, Q2Scores[answer.AnswerIndex]);
                    break;
                case 3 when answer.AnswerIndex >= 0 && answer.AnswerIndex < Q3Scores.Length:
                    AddScores(scores, Q3Scores[answer.AnswerIndex]);
                    break;
                case 4 when answer.AnswerIndex >= 0 && answer.AnswerIndex < Q4Scores.Length:
                    AddScores(scores, Q4Scores[answer.AnswerIndex]);
                    break;
                case 5:
                    // Q5: Stature (direct mapping)
                    stature = answer.AnswerIndex switch
                    {
                        0 => Stature.Petite,
                        1 => Stature.Average,
                        2 => Stature.Tall,
                        _ => Stature.Average
                    };
                    break;
                case 6:
                    // Q6: Build (direct mapping)
                    build = answer.AnswerIndex switch
                    {
                        0 => Build.Standard,
                        1 => Build.Plus,
                        _ => Build.Standard
                    };
                    break;
            }
        }

        // Determine winning body shape with tie-breaker
        var winningShapeIndex = DetermineWinningShape(scores);
        var winningShapeName = BodyShapes[winningShapeIndex];

        // Get the body type ID from database (or use mapping)
        var bodyTypeId = await GetBodyTypeIdAsync(winningShapeName, cancellationToken);

        // Default stature/build if not answered
        stature ??= Stature.Average;
        build ??= Build.Standard;

        // Generate the result label
        var resultLabel = FormatResultLabel(stature.Value, build.Value, winningShapeName);
        var resultDescription = $"We'll show you content from creators with similar proportions.";

        _logger.LogInformation(
            "Quiz completed: Shape={Shape} (scores: {Scores}), Stature={Stature}, Build={Build}",
            winningShapeName, string.Join(",", scores), stature, build);

        return new QuizResultResponse(
            BodyTypeId: bodyTypeId,
            BodyTypeName: winningShapeName,
            Stature: stature.Value,
            Build: build.Value,
            ResultLabel: resultLabel,
            ResultDescription: resultDescription
        );
    }

    private static void AddScores(int[] totals, int[] additions)
    {
        for (var i = 0; i < totals.Length && i < additions.Length; i++)
        {
            totals[i] += additions[i];
        }
    }

    private static int DetermineWinningShape(int[] scores)
    {
        var maxScore = scores.Max();
        var winners = new List<int>();

        for (var i = 0; i < scores.Length; i++)
        {
            if (scores[i] == maxScore)
            {
                winners.Add(i);
            }
        }

        if (winners.Count == 1)
        {
            return winners[0];
        }

        // Apply tie-breaker: return the winner with lowest priority index
        return winners.OrderBy(w => Array.IndexOf(TieBreakerPriority, w)).First();
    }

    private async Task<int> GetBodyTypeIdAsync(string shapeName, CancellationToken cancellationToken)
    {
        // Try to get from database first
        var bodyType = await _dbContext.Set<BodyType>()
            .AsNoTracking()
            .FirstOrDefaultAsync(bt => bt.Name == shapeName, cancellationToken);

        if (bodyType != null)
        {
            return bodyType.Id;
        }

        // Fall back to static mapping
        return BodyShapeIds.TryGetValue(shapeName, out var id) ? id : 1;
    }

    private static string FormatResultLabel(Stature stature, Build build, string shapeName)
    {
        var parts = new List<string>();

        if (stature != Stature.Average)
        {
            parts.Add(stature.ToString());
        }

        if (build != Build.Standard)
        {
            parts.Add(build.ToString());
        }

        parts.Add(shapeName);

        return string.Join(" ", parts);
    }
}
