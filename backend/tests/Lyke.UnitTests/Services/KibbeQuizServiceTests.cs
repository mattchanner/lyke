using FluentAssertions;
using Lyke.Application.DTOs.Style;
using Lyke.Application.Services;
using Lyke.Core.Enums;
using Lyke.Core.Exceptions;
using Lyke.UnitTests.Fixtures;

namespace Lyke.UnitTests.Services;

public class KibbeQuizServiceTests : IDisposable
{
    private readonly KibbeQuizService _sut;

    public KibbeQuizServiceTests()
    {
        // Score() is pure and doesn't require DB, but the service needs a DbContext
        var context = TestDbContextFactory.Create();
        _sut = new KibbeQuizService(context);
    }

    public void Dispose()
    {
        // No resources to dispose for these tests
    }

    #region Scoring Algorithm Tests

    [Fact]
    public void AllA_ReturnsDramatic()
    {
        // Arrange - 14 answers all "A" (Dramatic)
        // KibbeAnswer order: SectionId, QuestionId, SelectedOption
        var answers = Enumerable
            .Range(1, 14)
            .Select(i => new KibbeAnswer(
                i <= 4 ? "bone"
                    : i <= 10 ? "flesh"
                    : "face",
                $"Q{i}",
                "A"
            ))
            .ToList();

        var request = new KibbeScoreRequest(answers);

        // Act
        var result = _sut.Score(request);

        // Assert
        result.PrimaryFamily.Should().Be(KibbeFamily.Dramatic);
        result.Confidence.Should().Be(KibbeConfidence.High);
        result.IsMixed.Should().BeFalse();
        result.Counts.A.Should().Be(14);
    }

    [Fact]
    public void AllE_ReturnsRomantic()
    {
        // Arrange - 14 answers all "E" (Romantic)
        // KibbeAnswer order: SectionId, QuestionId, SelectedOption
        var answers = Enumerable
            .Range(1, 14)
            .Select(i => new KibbeAnswer(
                i <= 4 ? "bone"
                    : i <= 10 ? "flesh"
                    : "face",
                $"Q{i}",
                "E"
            ))
            .ToList();

        var request = new KibbeScoreRequest(answers);

        // Act
        var result = _sut.Score(request);

        // Assert
        result.PrimaryFamily.Should().Be(KibbeFamily.Romantic);
        result.Confidence.Should().Be(KibbeConfidence.High);
        result.IsMixed.Should().BeFalse();
    }

    [Fact]
    public void MixedSections_DetectedAsMixed()
    {
        // Arrange - Different dominance per section
        // Bone section (4 questions): All A (Dramatic)
        // Flesh section (6 questions): All E (Romantic)
        // Face section (4 questions): All C (Classic)
        // KibbeAnswer order: SectionId, QuestionId, SelectedOption
        var answers = new List<KibbeAnswer>();

        // Bone: 4 A's
        for (var i = 1; i <= 4; i++)
            answers.Add(new KibbeAnswer("bone", $"Q{i}", "A"));

        // Flesh: 6 E's
        for (var i = 5; i <= 10; i++)
            answers.Add(new KibbeAnswer("flesh", $"Q{i}", "E"));

        // Face: 4 C's
        for (var i = 11; i <= 14; i++)
            answers.Add(new KibbeAnswer("face", $"Q{i}", "C"));

        var request = new KibbeScoreRequest(answers);

        // Act
        var result = _sut.Score(request);

        // Assert
        result.IsMixed.Should().BeTrue();
        result.SectionDominance.Bone.Should().Be(KibbeFamily.Dramatic);
        result.SectionDominance.Flesh.Should().Be(KibbeFamily.Romantic);
        result.SectionDominance.Face.Should().Be(KibbeFamily.Classic);
    }

    [Fact]
    public void TwoWayTie_HandledGracefully()
    {
        // Arrange - 7 A's and 7 E's (tie)
        // KibbeAnswer order: SectionId, QuestionId, SelectedOption
        var answers = new List<KibbeAnswer>();

        // 7 A's across sections
        for (var i = 1; i <= 7; i++)
            answers.Add(new KibbeAnswer(i <= 4 ? "bone" : "flesh", $"Q{i}", "A"));

        // 7 E's across sections
        for (var i = 8; i <= 14; i++)
            answers.Add(new KibbeAnswer(i <= 10 ? "flesh" : "face", $"Q{i}", "E"));

        var request = new KibbeScoreRequest(answers);

        // Act
        var result = _sut.Score(request);

        // Assert
        result.IsMixed.Should().BeTrue();
        result.Confidence.Should().Be(KibbeConfidence.Low);
        result.Counts.A.Should().Be(7);
        result.Counts.E.Should().Be(7);
    }

    [Fact]
    public void NearTie_LowConfidence()
    {
        // Arrange - 8 A's and 6 B's (margin = 2)
        // KibbeAnswer order: SectionId, QuestionId, SelectedOption
        var answers = new List<KibbeAnswer>();

        // 8 A's
        for (var i = 1; i <= 8; i++)
            answers.Add(new KibbeAnswer(i <= 4 ? "bone" : "flesh", $"Q{i}", "A"));

        // 6 B's
        for (var i = 9; i <= 14; i++)
            answers.Add(new KibbeAnswer(i <= 10 ? "flesh" : "face", $"Q{i}", "B"));

        var request = new KibbeScoreRequest(answers);

        // Act
        var result = _sut.Score(request);

        // Assert
        result.PrimaryFamily.Should().Be(KibbeFamily.Dramatic);
        result.Confidence.Should().Be(KibbeConfidence.Low);
        result.IsMixed.Should().BeTrue(); // margin <= 2 triggers mixed
    }

    [Fact]
    public void StrongDominance_HighConfidence()
    {
        // Arrange - 10 A's and 4 others (margin >= 5)
        // KibbeAnswer order: SectionId, QuestionId, SelectedOption
        var answers = new List<KibbeAnswer>();

        // 10 A's across all sections
        for (var i = 1; i <= 10; i++)
            answers.Add(new KibbeAnswer(i <= 4 ? "bone" : "flesh", $"Q{i}", "A"));

        // 4 scattered (1 each of B, C, D, E)
        answers.Add(new KibbeAnswer("face", "Q11", "B"));
        answers.Add(new KibbeAnswer("face", "Q12", "C"));
        answers.Add(new KibbeAnswer("face", "Q13", "D"));
        answers.Add(new KibbeAnswer("face", "Q14", "E"));

        var request = new KibbeScoreRequest(answers);

        // Act
        var result = _sut.Score(request);

        // Assert
        result.PrimaryFamily.Should().Be(KibbeFamily.Dramatic);
        result.Confidence.Should().Be(KibbeConfidence.High);
        result.Counts.A.Should().Be(10);
    }

    [Fact]
    public void EmptyPayload_ThrowsValidation()
    {
        // Arrange
        var request = new KibbeScoreRequest(new List<KibbeAnswer>());

        // Act
        var act = () => _sut.Score(request);

        // Assert
        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void InvalidOption_ThrowsValidation()
    {
        // Arrange - SelectedOption "F" is not valid (only A-E)
        // KibbeAnswer order: SectionId, QuestionId, SelectedOption
        var answers = new List<KibbeAnswer> { new("bone", "Q1", "F") };

        var request = new KibbeScoreRequest(answers);

        // Act
        var act = () => _sut.Score(request);

        // Assert
        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void InvalidSection_ThrowsValidation()
    {
        // Arrange - Section "other" is not valid
        // KibbeAnswer order: SectionId, QuestionId, SelectedOption
        var answers = new List<KibbeAnswer> { new("other", "Q1", "A") };

        var request = new KibbeScoreRequest(answers);

        // Act
        var act = () => _sut.Score(request);

        // Assert
        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void SingleAnswer_Scores()
    {
        // Arrange - Edge case: only 1 answer
        // KibbeAnswer order: SectionId, QuestionId, SelectedOption
        var answers = new List<KibbeAnswer> { new("bone", "Q1", "C") };

        var request = new KibbeScoreRequest(answers);

        // Act
        var result = _sut.Score(request);

        // Assert
        result.Should().NotBeNull();
        result.PrimaryFamily.Should().Be(KibbeFamily.Classic);
        result.Counts.C.Should().Be(1);
    }

    [Fact]
    public void RunnerUp_IsSecondHighestCount()
    {
        // Arrange - 8 A's, 4 B's, 2 C's → runnerUp = Natural (B)
        // KibbeAnswer order: SectionId, QuestionId, SelectedOption
        var answers = new List<KibbeAnswer>();

        // 8 A's
        for (var i = 1; i <= 8; i++)
            answers.Add(new KibbeAnswer(i <= 4 ? "bone" : "flesh", $"Q{i}", "A"));

        // 4 B's
        for (var i = 9; i <= 12; i++)
            answers.Add(new KibbeAnswer("flesh", $"Q{i}", "B"));

        // 2 C's
        answers.Add(new KibbeAnswer("face", "Q13", "C"));
        answers.Add(new KibbeAnswer("face", "Q14", "C"));

        var request = new KibbeScoreRequest(answers);

        // Act
        var result = _sut.Score(request);

        // Assert
        result.PrimaryFamily.Should().Be(KibbeFamily.Dramatic);
        result.RunnerUpFamily.Should().Be(KibbeFamily.Natural);
        result.Counts.A.Should().Be(8);
        result.Counts.B.Should().Be(4);
        result.Counts.C.Should().Be(2);
    }

    [Fact]
    public void SectionDominance_CorrectPerSection()
    {
        // Arrange - All bone=A, all flesh=B, all face=C
        // KibbeAnswer order: SectionId, QuestionId, SelectedOption
        var answers = new List<KibbeAnswer>();

        // Bone: 4 A's (Dramatic)
        for (var i = 1; i <= 4; i++)
            answers.Add(new KibbeAnswer("bone", $"Q{i}", "A"));

        // Flesh: 6 B's (Natural)
        for (var i = 5; i <= 10; i++)
            answers.Add(new KibbeAnswer("flesh", $"Q{i}", "B"));

        // Face: 4 C's (Classic)
        for (var i = 11; i <= 14; i++)
            answers.Add(new KibbeAnswer("face", $"Q{i}", "C"));

        var request = new KibbeScoreRequest(answers);

        // Act
        var result = _sut.Score(request);

        // Assert
        result.SectionDominance.Bone.Should().Be(KibbeFamily.Dramatic);
        result.SectionDominance.Flesh.Should().Be(KibbeFamily.Natural);
        result.SectionDominance.Face.Should().Be(KibbeFamily.Classic);
    }

    #endregion

    #region Confidence Level Tests

    [Fact]
    public void MediumConfidence_WhenMarginIs3Or4()
    {
        // Arrange - 9 A's and 5 B's (margin = 4)
        // KibbeAnswer order: SectionId, QuestionId, SelectedOption
        var answers = new List<KibbeAnswer>();

        // 9 A's
        for (var i = 1; i <= 9; i++)
            answers.Add(new KibbeAnswer(i <= 4 ? "bone" : "flesh", $"Q{i}", "A"));

        // 5 B's
        for (var i = 10; i <= 14; i++)
            answers.Add(new KibbeAnswer(i <= 10 ? "flesh" : "face", $"Q{i}", "B"));

        var request = new KibbeScoreRequest(answers);

        // Act
        var result = _sut.Score(request);

        // Assert
        result.Confidence.Should().Be(KibbeConfidence.Medium);
    }

    #endregion

    #region Case Insensitivity Tests

    [Fact]
    public void OptionsAreCaseInsensitive()
    {
        // Arrange - Use lowercase options
        // KibbeAnswer order: SectionId, QuestionId, SelectedOption
        var answers = new List<KibbeAnswer>
        {
            new("bone", "Q1", "a"),
            new("bone", "Q2", "A"),
            new("flesh", "Q3", "b"),
            new("face", "Q4", "c"),
        };

        var request = new KibbeScoreRequest(answers);

        // Act
        var result = _sut.Score(request);

        // Assert
        result.Counts.A.Should().Be(2);
        result.Counts.B.Should().Be(1);
        result.Counts.C.Should().Be(1);
    }

    [Fact]
    public void SectionsAreCaseInsensitive()
    {
        // Arrange - Use mixed case sections
        // KibbeAnswer order: SectionId, QuestionId, SelectedOption
        var answers = new List<KibbeAnswer>
        {
            new("BONE", "Q1", "A"),
            new("Bone", "Q2", "A"),
            new("FLESH", "Q3", "B"),
            new("Face", "Q4", "C"),
        };

        var request = new KibbeScoreRequest(answers);

        // Act
        var result = _sut.Score(request);

        // Assert - Should not throw, sections are normalized
        result.Should().NotBeNull();
    }

    #endregion
}
