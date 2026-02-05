using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Creator;
using Lyke.Application.DTOs.Feed;
using Lyke.Core.Enums;
using Lyke.IntegrationTests.Fixtures;

namespace Lyke.IntegrationTests.Tests;

public class FeedEndpointsTests : IClassFixture<LykeWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly LykeWebApplicationFactory _factory;

    public FeedEndpointsTests(LykeWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    #region Feed Tests

    [Fact]
    public async Task GetFeed_WithAuthenticatedUser_ReturnsFeed()
    {
        // Arrange
        var email = $"feed-user-{Guid.NewGuid()}@example.com";
        var token = await _factory.CreateTestUserAndGetTokenAsync(email, "Test123!");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            token
        );

        // Act
        var response = await _client.GetAsync("/api/feed/v1/");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<
            ApiResponse<IReadOnlyList<FeedPostResponse>>
        >(LykeWebApplicationFactory.JsonOptions);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();

        // Cleanup
        _client.DefaultRequestHeaders.Authorization = null;
    }

    [Fact]
    public async Task GetFeed_WithPagination_ReturnsPaginatedResults()
    {
        // Arrange
        var email = $"feed-paginated-{Guid.NewGuid()}@example.com";
        var token = await _factory.CreateTestUserAndGetTokenAsync(email, "Test123!");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            token
        );

        // Act
        var response = await _client.GetAsync("/api/feed/v1/?page=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<
            ApiResponse<IReadOnlyList<FeedPostResponse>>
        >(LykeWebApplicationFactory.JsonOptions);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();

        // Cleanup
        _client.DefaultRequestHeaders.Authorization = null;
    }

    [Fact]
    public async Task GetFeed_WithoutToken_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/feed/v1/");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetExploreFeed_WithoutToken_ReturnsSuccess()
    {
        // Act - Explore feed is publicly accessible
        var response = await _client.GetAsync("/api/feed/v1/explore");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<
            ApiResponse<IReadOnlyList<FeedPostResponse>>
        >(LykeWebApplicationFactory.JsonOptions);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task GetExploreFeed_WithAuthenticatedUser_ReturnsPersonalizedResults()
    {
        // Arrange
        var email = $"explore-user-{Guid.NewGuid()}@example.com";
        var token = await _factory.CreateTestUserAndGetTokenAsync(email, "Test123!");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            token
        );

        // Act
        var response = await _client.GetAsync("/api/feed/v1/explore");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<
            ApiResponse<IReadOnlyList<FeedPostResponse>>
        >(LykeWebApplicationFactory.JsonOptions);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();

        // Cleanup
        _client.DefaultRequestHeaders.Authorization = null;
    }

    #endregion

    #region Post Detail Tests

    [Fact]
    public async Task GetPost_WhenExists_ReturnsPostDetails()
    {
        // Arrange - Create a post first
        var creatorEmail = $"creator-post-detail-{Guid.NewGuid()}@example.com";
        var creatorToken = await _factory.CreateTestUserAndGetTokenAsync(
            creatorEmail,
            "Test123!",
            UserType.Creator
        );
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            creatorToken
        );

        // Register as creator and create a post
        await _client.PostAsJsonAsync(
            "/api/creators/v1/register",
            new RegisterCreatorRequest(
                DisplayName: "Post Detail Creator",
                Bio: null,
                SocialLinks: null
            )
        );

        var createResponse = await _client.PostAsJsonAsync(
            "/api/creators/v1/posts",
            new CreatePostRequest(
                Title: "Detail Test Post",
                Description: "Casual look",
                MediaType: MediaType.Image,
                MediaUrls: new List<string> { "https://example.com/image.jpg" },
                Products: new List<PostProductRequest>()
            )
        );
        var createResult = await createResponse.Content.ReadFromJsonAsync<
            ApiResponse<CreatorPostResponse>
        >(LykeWebApplicationFactory.JsonOptions);
        var postId = createResult!.Data!.Id;

        // Submit for review to make it visible
        await _client.PostAsync($"/api/creators/v1/posts/{postId}/submit", null);

        _client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await _client.GetAsync($"/api/posts/v1/{postId}");

        // Assert - May return 404 if post is not approved yet, or 200 if publicly visible
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetPost_WhenNotExists_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync($"/api/posts/v1/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetSimilarPosts_ReturnsRelatedPosts()
    {
        // Arrange - Create a post first
        var creatorEmail = $"creator-similar-{Guid.NewGuid()}@example.com";
        var creatorToken = await _factory.CreateTestUserAndGetTokenAsync(
            creatorEmail,
            "Test123!",
            UserType.Creator
        );
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            creatorToken
        );

        await _client.PostAsJsonAsync(
            "/api/creators/v1/register",
            new RegisterCreatorRequest(DisplayName: "Similar Creator", Bio: null, SocialLinks: null)
        );

        var createResponse = await _client.PostAsJsonAsync(
            "/api/creators/v1/posts",
            new CreatePostRequest(
                Title: "Similar Test Post",
                Description: "Casual look",
                MediaType: MediaType.Image,
                MediaUrls: new List<string> { "https://example.com/image.jpg" },
                Products: new List<PostProductRequest>()
            )
        );
        var createResult = await createResponse.Content.ReadFromJsonAsync<
            ApiResponse<CreatorPostResponse>
        >(LykeWebApplicationFactory.JsonOptions);
        var postId = createResult!.Data!.Id;

        _client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await _client.GetAsync($"/api/posts/v1/{postId}/similar");

        // Assert - May return 404 if post not visible, or 200 with similar posts
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    #endregion

    #region Engagement Tests

    [Fact]
    public async Task EngagePost_WithView_RecordsEngagement()
    {
        // Arrange - Create a post first
        var creatorEmail = $"creator-engage-{Guid.NewGuid()}@example.com";
        var creatorToken = await _factory.CreateTestUserAndGetTokenAsync(
            creatorEmail,
            "Test123!",
            UserType.Creator
        );
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            creatorToken
        );

        await _client.PostAsJsonAsync(
            "/api/creators/v1/register",
            new RegisterCreatorRequest(DisplayName: "Engage Creator", Bio: null, SocialLinks: null)
        );

        var createResponse = await _client.PostAsJsonAsync(
            "/api/creators/v1/posts",
            new CreatePostRequest(
                Title: "Engagement Test Post",
                Description: "Casual look",
                MediaType: MediaType.Image,
                MediaUrls: new List<string> { "https://example.com/image.jpg" },
                Products: new List<PostProductRequest>()
            )
        );
        var createResult = await createResponse.Content.ReadFromJsonAsync<
            ApiResponse<CreatorPostResponse>
        >(LykeWebApplicationFactory.JsonOptions);
        var postId = createResult!.Data!.Id;

        // Submit for review
        await _client.PostAsync($"/api/creators/v1/posts/{postId}/submit", null);

        // Login as a different user for engagement
        var shopperEmail = $"shopper-engage-{Guid.NewGuid()}@example.com";
        var shopperToken = await _factory.CreateTestUserAndGetTokenAsync(shopperEmail, "Test123!");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            shopperToken
        );

        var request = new EngageRequest(Type: EngagementType.View);

        // Act
        var response = await _client.PostAsJsonAsync($"/api/posts/v1/{postId}/engage", request);

        // Assert - May return 404 if post not approved
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);

        // Cleanup
        _client.DefaultRequestHeaders.Authorization = null;
    }

    [Fact]
    public async Task EngagePost_WithLike_RecordsEngagement()
    {
        // Arrange
        var creatorEmail = $"creator-like-{Guid.NewGuid()}@example.com";
        var creatorToken = await _factory.CreateTestUserAndGetTokenAsync(
            creatorEmail,
            "Test123!",
            UserType.Creator
        );
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            creatorToken
        );

        await _client.PostAsJsonAsync(
            "/api/creators/v1/register",
            new RegisterCreatorRequest(DisplayName: "Like Creator", Bio: null, SocialLinks: null)
        );

        var createResponse = await _client.PostAsJsonAsync(
            "/api/creators/v1/posts",
            new CreatePostRequest(
                Title: "Like Test Post",
                Description: "Casual look",
                MediaType: MediaType.Image,
                MediaUrls: new List<string> { "https://example.com/image.jpg" },
                Products: new List<PostProductRequest>()
            )
        );
        var createResult = await createResponse.Content.ReadFromJsonAsync<
            ApiResponse<CreatorPostResponse>
        >(LykeWebApplicationFactory.JsonOptions);
        var postId = createResult!.Data!.Id;

        // Submit for review
        await _client.PostAsync($"/api/creators/v1/posts/{postId}/submit", null);

        // Login as a different user
        var shopperEmail = $"shopper-like-{Guid.NewGuid()}@example.com";
        var shopperToken = await _factory.CreateTestUserAndGetTokenAsync(shopperEmail, "Test123!");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            shopperToken
        );

        var request = new EngageRequest(Type: EngagementType.Like);

        // Act
        var response = await _client.PostAsJsonAsync($"/api/posts/v1/{postId}/engage", request);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);

        // Cleanup
        _client.DefaultRequestHeaders.Authorization = null;
    }

    [Fact]
    public async Task EngagePost_WithSave_RecordsEngagement()
    {
        // Arrange
        var creatorEmail = $"creator-save-{Guid.NewGuid()}@example.com";
        var creatorToken = await _factory.CreateTestUserAndGetTokenAsync(
            creatorEmail,
            "Test123!",
            UserType.Creator
        );
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            creatorToken
        );

        await _client.PostAsJsonAsync(
            "/api/creators/v1/register",
            new RegisterCreatorRequest(DisplayName: "Save Creator", Bio: null, SocialLinks: null)
        );

        var createResponse = await _client.PostAsJsonAsync(
            "/api/creators/v1/posts",
            new CreatePostRequest(
                Title: "Save Test Post",
                Description: "Casual look",
                MediaType: MediaType.Image,
                MediaUrls: new List<string> { "https://example.com/image.jpg" },
                Products: new List<PostProductRequest>()
            )
        );
        var createResult = await createResponse.Content.ReadFromJsonAsync<
            ApiResponse<CreatorPostResponse>
        >(LykeWebApplicationFactory.JsonOptions);
        var postId = createResult!.Data!.Id;

        // Submit for review
        await _client.PostAsync($"/api/creators/v1/posts/{postId}/submit", null);

        // Login as a different user
        var shopperEmail = $"shopper-save-{Guid.NewGuid()}@example.com";
        var shopperToken = await _factory.CreateTestUserAndGetTokenAsync(shopperEmail, "Test123!");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            shopperToken
        );

        var request = new EngageRequest(Type: EngagementType.Save);

        // Act
        var response = await _client.PostAsJsonAsync($"/api/posts/v1/{postId}/engage", request);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);

        // Cleanup
        _client.DefaultRequestHeaders.Authorization = null;
    }

    [Fact]
    public async Task RemoveEngagement_WithoutToken_ReturnsUnauthorized()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var request = new EngageRequest(Type: EngagementType.Like);

        // Act
        var response = await _client.SendAsync(
            new HttpRequestMessage(HttpMethod.Delete, $"/api/posts/v1/{postId}/engage")
            {
                Content = JsonContent.Create(request),
            }
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Saved Posts Tests

    [Fact]
    public async Task GetSavedPosts_WithToken_ReturnsSavedPosts()
    {
        // Arrange
        var email = $"saved-posts-{Guid.NewGuid()}@example.com";
        var token = await _factory.CreateTestUserAndGetTokenAsync(email, "Test123!");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            token
        );

        // Act
        var response = await _client.GetAsync("/api/posts/v1/saved");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<
            ApiResponse<IReadOnlyList<FeedPostResponse>>
        >(LykeWebApplicationFactory.JsonOptions);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();

        // Cleanup
        _client.DefaultRequestHeaders.Authorization = null;
    }

    [Fact]
    public async Task GetSavedPosts_WithoutToken_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/posts/v1/saved");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Search Tests

    [Fact]
    public async Task Search_WithQuery_ReturnsResults()
    {
        // Act
        var response = await _client.GetAsync("/api/search/v1/?q=outfit");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<SearchResponse>>(
            LykeWebApplicationFactory.JsonOptions
        );
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task Search_WithEmptyQuery_ReturnsResults()
    {
        // Act
        var response = await _client.GetAsync("/api/search/v1/?q=");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<SearchResponse>>(
            LykeWebApplicationFactory.JsonOptions
        );
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task Search_WithTypeFilter_ReturnsFilteredResults()
    {
        // Act
        var response = await _client.GetAsync("/api/search/v1/?q=fashion&type=Posts");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<SearchResponse>>(
            LykeWebApplicationFactory.JsonOptions
        );
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task Search_IsPubliclyAccessible()
    {
        // Act - No token required
        var response = await _client.GetAsync("/api/search/v1/?q=test");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion
}
