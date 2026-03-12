using FluentAssertions;
using Lyke.Application.Interfaces;
using Lyke.Application.Services;
using Lyke.Core.Entities;
using Lyke.Core.Enums;
using Lyke.Infrastructure.Data;
using Lyke.UnitTests.Fixtures;
using Microsoft.Extensions.Logging;
using Moq;

namespace Lyke.UnitTests.Services;

public class EventTrackingServiceTests : IDisposable
{
    private readonly LykeDbContext _context;
    private readonly Mock<ILogger<EventTrackingService>> _loggerMock;
    private readonly EventTrackingService _sut;

    public EventTrackingServiceTests()
    {
        _context = TestDbContextFactory.Create();
        _loggerMock = new Mock<ILogger<EventTrackingService>>();
        _sut = new EventTrackingService(_context, _loggerMock.Object);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Theory]
    [InlineData(AnalyticsEventType.FeedFilter)]
    [InlineData(AnalyticsEventType.SearchExecute)]
    [InlineData(AnalyticsEventType.ProfileComplete)]
    public async Task TrackAsync_BehavioralEvent_PersistsToDatabase(AnalyticsEventType eventType)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var entityId = Guid.NewGuid();

        // Act
        await _sut.TrackAsync(
            eventType,
            userId,
            entityId,
            "Test",
            new Dictionary<string, string> { ["key"] = "value" }
        );

        // Assert
        var events = _context.AnalyticsEvents.ToList();
        events.Should().HaveCount(1);
        events[0].EventType.Should().Be(eventType);
        events[0].UserId.Should().Be(userId);
        events[0].EntityId.Should().Be(entityId);
        events[0].Properties.Should().Contain("key");
    }

    [Theory]
    [InlineData(AnalyticsEventType.PostView)]
    [InlineData(AnalyticsEventType.PostLike)]
    [InlineData(AnalyticsEventType.PostSave)]
    [InlineData(AnalyticsEventType.PostShare)]
    [InlineData(AnalyticsEventType.ProductClick)]
    [InlineData(AnalyticsEventType.ProductConvert)]
    public async Task TrackAsync_EngagementEvent_DoesNotPersistToDatabase(
        AnalyticsEventType eventType
    )
    {
        // Act
        await _sut.TrackAsync(eventType, Guid.NewGuid(), Guid.NewGuid(), "Post");

        // Assert
        _context.AnalyticsEvents.Should().BeEmpty();
    }

    [Fact]
    public async Task TrackBatchAsync_MixedEvents_OnlyPersistsBehavioralEvents()
    {
        // Arrange
        var events = new List<TrackEventItem>
        {
            new(AnalyticsEventType.SearchExecute, Guid.NewGuid()),
            new(AnalyticsEventType.PostView, Guid.NewGuid()),
            new(AnalyticsEventType.FeedFilter, Guid.NewGuid()),
            new(AnalyticsEventType.PostLike, Guid.NewGuid()),
            new(AnalyticsEventType.ProfileComplete, Guid.NewGuid()),
        };

        // Act
        await _sut.TrackBatchAsync(events);

        // Assert
        var persisted = _context.AnalyticsEvents.ToList();
        persisted.Should().HaveCount(3);
        persisted
            .Select(e => e.EventType)
            .Should()
            .BeEquivalentTo(
                new[]
                {
                    AnalyticsEventType.SearchExecute,
                    AnalyticsEventType.FeedFilter,
                    AnalyticsEventType.ProfileComplete,
                }
            );
    }

    [Fact]
    public async Task TrackBatchAsync_EmptyBatch_DoesNotThrow()
    {
        // Act
        var act = () => _sut.TrackBatchAsync([]);

        // Assert
        await act.Should().NotThrowAsync();
        _context.AnalyticsEvents.Should().BeEmpty();
    }

    // ── TrackRawAsync ──

    [Theory]
    [InlineData("FeedFilter")]
    [InlineData("feedfilter")]
    [InlineData("SEARCHEXECUTE")]
    [InlineData("ProfileComplete")]
    public async Task TrackRawAsync_KnownPersistableEventType_PersistsToDatabase(string eventType)
    {
        // Act
        await _sut.TrackRawAsync(eventType, Guid.NewGuid());

        // Assert
        _context.AnalyticsEvents.Should().HaveCount(1);
    }

    [Theory]
    [InlineData("kibbe_quiz_start")]
    [InlineData("kibbe_full_results_view")]
    [InlineData("PostView")]
    [InlineData("completely_unknown_event")]
    public async Task TrackRawAsync_UnknownOrNonPersistableEventType_DoesNotPersist(
        string eventType
    )
    {
        // Act
        await _sut.TrackRawAsync(eventType, Guid.NewGuid());

        // Assert — logged but not saved to DB
        _context.AnalyticsEvents.Should().BeEmpty();
    }

    [Fact]
    public async Task TrackRawBatchAsync_MixedEventTypes_OnlyPersistsKnownBehavioralEvents()
    {
        // Arrange
        var events = new List<TrackRawEventItem>
        {
            new("SearchExecute", Guid.NewGuid()),
            new("kibbe_quiz_start", Guid.NewGuid()),
            new("FeedFilter", Guid.NewGuid()),
            new("kibbe_share_complete", Guid.NewGuid()),
            new("ProfileComplete", Guid.NewGuid()),
        };

        // Act
        await _sut.TrackRawBatchAsync(events);

        // Assert — only the 3 persistable events are saved
        var persisted = _context.AnalyticsEvents.ToList();
        persisted.Should().HaveCount(3);
        persisted
            .Select(e => e.EventType)
            .Should()
            .BeEquivalentTo(
                new[]
                {
                    AnalyticsEventType.SearchExecute,
                    AnalyticsEventType.FeedFilter,
                    AnalyticsEventType.ProfileComplete,
                }
            );
    }

    [Fact]
    public async Task TrackRawBatchAsync_EmptyBatch_DoesNotThrow()
    {
        var act = () => _sut.TrackRawBatchAsync([]);
        await act.Should().NotThrowAsync();
        _context.AnalyticsEvents.Should().BeEmpty();
    }
}
