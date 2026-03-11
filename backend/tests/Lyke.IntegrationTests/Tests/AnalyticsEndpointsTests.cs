using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Analytics;
using Lyke.Core.Enums;
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
    public async Task TrackEvent_WithValidEvent_ReturnsOk()
    {
        // Arrange
        var request = new TrackEventRequest(
            EventType: AnalyticsEventType.SearchExecute,
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
    public async Task TrackEventBatch_WithValidBatch_ReturnsOk()
    {
        // Arrange
        var request = new TrackEventBatchRequest(
            Events:
            [
                new TrackEventRequest(
                    EventType: AnalyticsEventType.FeedFilter,
                    Properties: new Dictionary<string, string> { ["category"] = "dresses" }
                ),
                new TrackEventRequest(
                    EventType: AnalyticsEventType.SearchExecute,
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
        // Arrange - No authorization header
        var request = new TrackEventRequest(EventType: AnalyticsEventType.PostView);

        // Act
        var response = await _client.PostAsJsonAsync(
            "/api/analytics/v1/events",
            request,
            LykeWebApplicationFactory.JsonOptions
        );

        // Assert - Should not return 401
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

        // Assert - Endpoint accepts empty batches (no server-side validation)
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
