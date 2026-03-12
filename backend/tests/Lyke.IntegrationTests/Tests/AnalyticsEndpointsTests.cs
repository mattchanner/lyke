using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Analytics;
using Lyke.IntegrationTests.Fixtures;

namespace Lyke.IntegrationTests.Tests;

public class AnalyticsEndpointsTests : IClassFixture<LykeWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly LykeWebApplicationFactory _factory;

    public AnalyticsEndpointsTests(LykeWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task TrackEvent_WithKnownEventType_ReturnsOk()
    {
        // Arrange
        var request = new TrackEventRequest(
            EventType: "SearchExecute",
            Properties: new Dictionary<string, string> { ["query"] = "summer dress" }
        );

        // Act
        var response = await _client.PostAsJsonAsync(
            "/api/analytics/v1/events",
            request,
            LykeWebApplicationFactory.JsonOptions
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse>(
            LykeWebApplicationFactory.JsonOptions
        );
        result!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task TrackEvent_WithUnknownEventType_ReturnsOk()
    {
        // Arbitrary client-side event names (e.g. from the Kibbe quiz) must be accepted
        var request = new TrackEventRequest(
            EventType: "kibbe_quiz_start",
            Properties: new Dictionary<string, string> { ["feature"] = "kibbe_quiz" }
        );

        // Act
        var response = await _client.PostAsJsonAsync(
            "/api/analytics/v1/events",
            request,
            LykeWebApplicationFactory.JsonOptions
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse>(
            LykeWebApplicationFactory.JsonOptions
        );
        result!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task TrackEventBatch_WithMixedEventTypes_ReturnsOk()
    {
        // Arrange — mix of known enum-mapped and arbitrary frontend event names
        var request = new TrackEventBatchRequest(
            Events:
            [
                new TrackEventRequest(
                    EventType: "FeedFilter",
                    Properties: new Dictionary<string, string> { ["category"] = "dresses" }
                ),
                new TrackEventRequest(
                    EventType: "kibbe_full_results_view",
                    Properties: new Dictionary<string, string> { ["primary_family"] = "Classic" }
                ),
                new TrackEventRequest(
                    EventType: "SearchExecute",
                    Properties: new Dictionary<string, string> { ["query"] = "jeans" }
                ),
            ]
        );

        // Act
        var response = await _client.PostAsJsonAsync(
            "/api/analytics/v1/events/batch",
            request,
            LykeWebApplicationFactory.JsonOptions
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse>(
            LykeWebApplicationFactory.JsonOptions
        );
        result!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task TrackEvent_IsPubliclyAccessible()
    {
        // No authorization header
        var request = new TrackEventRequest(EventType: "PostView");

        // Act
        var response = await _client.PostAsJsonAsync(
            "/api/analytics/v1/events",
            request,
            LykeWebApplicationFactory.JsonOptions
        );

        // Assert — should not return 401
        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task TrackEventBatch_EmptyBatch_ReturnsOk()
    {
        // Arrange
        var request = new TrackEventBatchRequest(Events: []);

        // Act
        var response = await _client.PostAsJsonAsync(
            "/api/analytics/v1/events/batch",
            request,
            LykeWebApplicationFactory.JsonOptions
        );

        // Assert — endpoint accepts empty batches
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
