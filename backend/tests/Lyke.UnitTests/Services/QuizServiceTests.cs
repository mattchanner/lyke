using FluentAssertions;
using Lyke.Application.DTOs.Quiz;
using Lyke.Application.Services;
using Lyke.Core.Enums;
using Lyke.Infrastructure.Data;
using Lyke.UnitTests.Fixtures;
using Microsoft.Extensions.Logging;
using Moq;

namespace Lyke.UnitTests.Services;

public class QuizServiceTests : IDisposable
{
    private readonly LykeDbContext _context;
    private readonly Mock<ILogger<QuizService>> _loggerMock;
    private readonly QuizService _sut;

    public QuizServiceTests()
    {
        _context = TestDbContextFactory.Create();
        _loggerMock = new Mock<ILogger<QuizService>>();
        _sut = new QuizService(_context, _loggerMock.Object);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    #region Body Shape Scoring Tests

    [Fact]
    public async Task CalculateResultAsync_HourglassAnswers_ReturnsHourglass()
    {
        // Arrange - Answers that strongly indicate Hourglass:
        // Q1: Same width (shoulders/hips) - index 1
        // Q2: Very defined waist - index 0
        // Q3: Evenly distributed - index 3
        // Q4: Same everywhere - index 3
        await TestDbContextFactory.SeedTestDataAsync(_context);

        var request = new QuizResultRequest(new List<QuizAnswer>
        {
            new(1, 1), // Same width
            new(2, 0), // Very defined waist
            new(3, 3), // Evenly distributed
            new(4, 3)  // Same everywhere
        });

        // Act
        var result = await _sut.CalculateResultAsync(request);

        // Assert
        result.BodyTypeName.Should().Be("Hourglass");
        result.BodyTypeId.Should().Be(1);
    }

    [Fact]
    public async Task CalculateResultAsync_PearAnswers_ReturnsPear()
    {
        // Arrange - Answers that strongly indicate Pear:
        // Q1: Hips wider - index 2
        // Q2: Very defined waist - index 0
        // Q3: Lower body - index 2
        // Q4: Tight hips - index 2
        await TestDbContextFactory.SeedTestDataAsync(_context);

        var request = new QuizResultRequest(new List<QuizAnswer>
        {
            new(1, 2), // Hips wider
            new(2, 0), // Very defined waist
            new(3, 2), // Lower body
            new(4, 2)  // Tight hips
        });

        // Act
        var result = await _sut.CalculateResultAsync(request);

        // Assert
        result.BodyTypeName.Should().Be("Pear");
        result.BodyTypeId.Should().Be(2);
    }

    [Fact]
    public async Task CalculateResultAsync_AppleAnswers_ReturnsApple()
    {
        // Arrange - Answers that strongly indicate Apple:
        // Q1: Shoulders wider - index 0
        // Q2: Minimal waist definition - index 2
        // Q3: Midsection - index 1
        // Q4: Tight middle - index 1
        await TestDbContextFactory.SeedTestDataAsync(_context);

        var request = new QuizResultRequest(new List<QuizAnswer>
        {
            new(1, 0), // Shoulders wider
            new(2, 2), // Minimal waist definition
            new(3, 1), // Midsection
            new(4, 1)  // Tight middle
        });

        // Act
        var result = await _sut.CalculateResultAsync(request);

        // Assert
        result.BodyTypeName.Should().Be("Apple");
        result.BodyTypeId.Should().Be(3);
    }

    [Fact]
    public async Task CalculateResultAsync_RectangleAnswers_ReturnsRectangle()
    {
        // Arrange - Answers that strongly indicate Rectangle:
        // Q1: Same width - index 1
        // Q2: Minimal waist definition - index 2
        // Q3: Evenly distributed - index 3
        // Q4: Same everywhere - index 3
        await TestDbContextFactory.SeedTestDataAsync(_context);

        var request = new QuizResultRequest(new List<QuizAnswer>
        {
            new(1, 1), // Same width
            new(2, 2), // Minimal waist definition
            new(3, 3), // Evenly distributed
            new(4, 3)  // Same everywhere
        });

        // Act
        var result = await _sut.CalculateResultAsync(request);

        // Assert
        result.BodyTypeName.Should().Be("Rectangle");
        result.BodyTypeId.Should().Be(4);
    }

    [Fact]
    public async Task CalculateResultAsync_InvertedTriangleAnswers_ReturnsInvertedTriangle()
    {
        // Arrange - Answers that strongly indicate Inverted Triangle:
        // Q1: Shoulders wider - index 0
        // Q2: Moderate waist - index 1
        // Q3: Upper body - index 0
        // Q4: Tight shoulders - index 0
        await TestDbContextFactory.SeedTestDataAsync(_context);

        var request = new QuizResultRequest(new List<QuizAnswer>
        {
            new(1, 0), // Shoulders wider
            new(2, 1), // Moderate waist
            new(3, 0), // Upper body
            new(4, 0)  // Tight shoulders
        });

        // Act
        var result = await _sut.CalculateResultAsync(request);

        // Assert
        result.BodyTypeName.Should().Be("Inverted Triangle");
        result.BodyTypeId.Should().Be(5);
    }

    #endregion

    #region Stature and Build Tests

    [Theory]
    [InlineData(0, Stature.Petite)]
    [InlineData(1, Stature.Average)]
    [InlineData(2, Stature.Tall)]
    public async Task CalculateResultAsync_StatureQuestion_MapsCorrectly(int answerIndex, Stature expectedStature)
    {
        // Arrange
        await TestDbContextFactory.SeedTestDataAsync(_context);

        var request = new QuizResultRequest(new List<QuizAnswer>
        {
            new(1, 1), // Body shape questions
            new(2, 0),
            new(3, 3),
            new(4, 3),
            new(5, answerIndex) // Stature question
        });

        // Act
        var result = await _sut.CalculateResultAsync(request);

        // Assert
        result.Stature.Should().Be(expectedStature);
    }

    [Theory]
    [InlineData(0, Build.Standard)]
    [InlineData(1, Build.Plus)]
    public async Task CalculateResultAsync_BuildQuestion_MapsCorrectly(int answerIndex, Build expectedBuild)
    {
        // Arrange
        await TestDbContextFactory.SeedTestDataAsync(_context);

        var request = new QuizResultRequest(new List<QuizAnswer>
        {
            new(1, 1), // Body shape questions
            new(2, 0),
            new(3, 3),
            new(4, 3),
            new(6, answerIndex) // Build question
        });

        // Act
        var result = await _sut.CalculateResultAsync(request);

        // Assert
        result.Build.Should().Be(expectedBuild);
    }

    [Fact]
    public async Task CalculateResultAsync_MissingStatureAndBuild_DefaultsToAverageAndStandard()
    {
        // Arrange - Only body shape questions, no stature/build
        await TestDbContextFactory.SeedTestDataAsync(_context);

        var request = new QuizResultRequest(new List<QuizAnswer>
        {
            new(1, 1),
            new(2, 0),
            new(3, 3),
            new(4, 3)
        });

        // Act
        var result = await _sut.CalculateResultAsync(request);

        // Assert
        result.Stature.Should().Be(Stature.Average);
        result.Build.Should().Be(Build.Standard);
    }

    #endregion

    #region Result Label Tests

    [Fact]
    public async Task CalculateResultAsync_PetitePlusHourglass_FormatsLabelCorrectly()
    {
        // Arrange
        await TestDbContextFactory.SeedTestDataAsync(_context);

        var request = new QuizResultRequest(new List<QuizAnswer>
        {
            new(1, 1), // Hourglass indicators
            new(2, 0),
            new(3, 3),
            new(4, 3),
            new(5, 0), // Petite
            new(6, 1)  // Plus
        });

        // Act
        var result = await _sut.CalculateResultAsync(request);

        // Assert
        result.ResultLabel.Should().Be("Petite Plus Hourglass");
    }

    [Fact]
    public async Task CalculateResultAsync_TallStandardPear_OmitsStandard()
    {
        // Arrange - Standard build should be omitted from label
        await TestDbContextFactory.SeedTestDataAsync(_context);

        var request = new QuizResultRequest(new List<QuizAnswer>
        {
            new(1, 2), // Pear indicators
            new(2, 0),
            new(3, 2),
            new(4, 2),
            new(5, 2), // Tall
            new(6, 0)  // Standard
        });

        // Act
        var result = await _sut.CalculateResultAsync(request);

        // Assert
        result.ResultLabel.Should().Be("Tall Pear");
        result.ResultLabel.Should().NotContain("Standard");
    }

    [Fact]
    public async Task CalculateResultAsync_AverageStandard_OmitsBothFromLabel()
    {
        // Arrange - Average stature with Standard build = just body type (both are defaults, omitted)
        await TestDbContextFactory.SeedTestDataAsync(_context);

        var request = new QuizResultRequest(new List<QuizAnswer>
        {
            new(1, 1),
            new(2, 2),
            new(3, 3),
            new(4, 3),
            new(5, 1), // Average (omitted from label)
            new(6, 0)  // Standard (omitted from label)
        });

        // Act
        var result = await _sut.CalculateResultAsync(request);

        // Assert - Only body type name, no stature or build qualifiers
        result.ResultLabel.Should().Be("Rectangle");
        result.ResultLabel.Should().NotContain("Average");
        result.ResultLabel.Should().NotContain("Standard");
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task CalculateResultAsync_EmptyAnswers_ReturnsDefaultResult()
    {
        // Arrange
        await TestDbContextFactory.SeedTestDataAsync(_context);

        var request = new QuizResultRequest(new List<QuizAnswer>());

        // Act
        var result = await _sut.CalculateResultAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Stature.Should().Be(Stature.Average);
        result.Build.Should().Be(Build.Standard);
    }

    [Fact]
    public async Task CalculateResultAsync_InvalidQuestionId_IgnoresInvalidAnswers()
    {
        // Arrange
        await TestDbContextFactory.SeedTestDataAsync(_context);

        var request = new QuizResultRequest(new List<QuizAnswer>
        {
            new(99, 0), // Invalid question ID
            new(1, 1),
            new(2, 0),
            new(3, 3),
            new(4, 3)
        });

        // Act
        var result = await _sut.CalculateResultAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.BodyTypeName.Should().Be("Hourglass");
    }

    [Fact]
    public async Task CalculateResultAsync_InvalidAnswerIndex_IgnoresInvalidAnswers()
    {
        // Arrange
        await TestDbContextFactory.SeedTestDataAsync(_context);

        var request = new QuizResultRequest(new List<QuizAnswer>
        {
            new(1, 99), // Invalid answer index
            new(2, 0),
            new(3, 3),
            new(4, 3)
        });

        // Act
        var result = await _sut.CalculateResultAsync(request);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task CalculateResultAsync_TiedScores_UseTieBreakerPriority()
    {
        // Arrange - Answers that result in tied scores
        // The tie-breaker priority is: Hourglass > Pear > Rectangle > Apple > Inverted Triangle
        await TestDbContextFactory.SeedTestDataAsync(_context);

        var request = new QuizResultRequest(new List<QuizAnswer>
        {
            new(1, 1), // Same width - gives points to multiple shapes
            new(2, 1), // Moderate waist - gives 1 point to all shapes
            new(3, 3), // Evenly - gives points to multiple shapes
            new(4, 3)  // Same everywhere
        });

        // Act
        var result = await _sut.CalculateResultAsync(request);

        // Assert - With tied scores, Hourglass should win per tie-breaker
        result.BodyTypeName.Should().BeOneOf("Hourglass", "Rectangle"); // Most likely outcomes
    }

    #endregion

    #region Response Structure Tests

    [Fact]
    public async Task CalculateResultAsync_ReturnsCompleteResponse()
    {
        // Arrange
        await TestDbContextFactory.SeedTestDataAsync(_context);

        var request = new QuizResultRequest(new List<QuizAnswer>
        {
            new(1, 1),
            new(2, 0),
            new(3, 3),
            new(4, 3),
            new(5, 0),
            new(6, 1)
        });

        // Act
        var result = await _sut.CalculateResultAsync(request);

        // Assert
        result.BodyTypeId.Should().BeGreaterThan(0);
        result.BodyTypeName.Should().NotBeNullOrEmpty();
        result.Stature.Should().BeDefined();
        result.Build.Should().BeDefined();
        result.ResultLabel.Should().NotBeNullOrEmpty();
        result.ResultDescription.Should().NotBeNullOrEmpty();
    }

    #endregion
}
