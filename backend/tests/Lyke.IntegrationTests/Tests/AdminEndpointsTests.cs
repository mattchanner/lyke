using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Admin;
using Lyke.Application.DTOs.Creator;
using Lyke.Core.Enums;
using Lyke.IntegrationTests.Fixtures;

namespace Lyke.IntegrationTests.Tests;

public class AdminEndpointsTests : IClassFixture<LykeWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly LykeWebApplicationFactory _factory;

    public AdminEndpointsTests(LykeWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    #region Authorization Tests

    [Fact]
    public async Task AdminEndpoints_WithoutToken_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/admin/v1/users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AdminEndpoints_WithShopperToken_ReturnsForbidden()
    {
        // Arrange
        var email = $"shopper-admin-{Guid.NewGuid()}@example.com";
        var token = await _factory.CreateTestUserAndGetTokenAsync(
            email,
            "Test123!",
            UserType.Shopper
        );
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            token
        );

        // Act
        var response = await _client.GetAsync("/api/admin/v1/users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        // Cleanup
        _client.DefaultRequestHeaders.Authorization = null;
    }

    [Fact]
    public async Task AdminEndpoints_WithCreatorToken_ReturnsForbidden()
    {
        // Arrange
        var email = $"creator-admin-{Guid.NewGuid()}@example.com";
        var token = await _factory.CreateTestUserAndGetTokenAsync(
            email,
            "Test123!",
            UserType.Creator
        );
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            token
        );

        // Act
        var response = await _client.GetAsync("/api/admin/v1/users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        // Cleanup
        _client.DefaultRequestHeaders.Authorization = null;
    }

    #endregion

    #region User Management Tests

    [Fact]
    public async Task GetUsers_WithAdminToken_ReturnsUserList()
    {
        // Arrange
        var email = $"admin-users-{Guid.NewGuid()}@example.com";
        var token = await _factory.CreateTestUserAndGetTokenAsync(
            email,
            "Test123!",
            UserType.Admin
        );
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            token
        );

        // Act
        var response = await _client.GetAsync("/api/admin/v1/users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<
            ApiResponse<IReadOnlyList<UserListResponse>>
        >(LykeWebApplicationFactory.JsonOptions);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();

        // Cleanup
        _client.DefaultRequestHeaders.Authorization = null;
    }

    [Fact]
    public async Task GetUsers_WithUserTypeFilter_ReturnsFilteredList()
    {
        // Arrange
        var email = $"admin-filter-{Guid.NewGuid()}@example.com";
        var token = await _factory.CreateTestUserAndGetTokenAsync(
            email,
            "Test123!",
            UserType.Admin
        );
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            token
        );

        // Create a shopper user
        await _factory.CreateTestUserAsync(
            $"shopper-filter-{Guid.NewGuid()}@example.com",
            "Test123!",
            UserType.Shopper
        );

        // Act
        var response = await _client.GetAsync("/api/admin/v1/users?userType=Shopper");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<
            ApiResponse<IReadOnlyList<UserListResponse>>
        >(LykeWebApplicationFactory.JsonOptions);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Should().OnlyContain(u => u.UserType == UserType.Shopper);

        // Cleanup
        _client.DefaultRequestHeaders.Authorization = null;
    }

    [Fact]
    public async Task GetUser_WithValidId_ReturnsUserDetails()
    {
        // Arrange
        var adminEmail = $"admin-detail-{Guid.NewGuid()}@example.com";
        var token = await _factory.CreateTestUserAndGetTokenAsync(
            adminEmail,
            "Test123!",
            UserType.Admin
        );
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            token
        );

        // Create a user to get
        var targetUser = await _factory.CreateTestUserAsync(
            $"target-user-{Guid.NewGuid()}@example.com",
            "Test123!"
        );

        // Act
        var response = await _client.GetAsync($"/api/admin/v1/users/{targetUser.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<UserDetailResponse>>(
            LykeWebApplicationFactory.JsonOptions
        );
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Email.Should().Be(targetUser.Email);

        // Cleanup
        _client.DefaultRequestHeaders.Authorization = null;
    }

    [Fact]
    public async Task GetUser_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var email = $"admin-notfound-{Guid.NewGuid()}@example.com";
        var token = await _factory.CreateTestUserAndGetTokenAsync(
            email,
            "Test123!",
            UserType.Admin
        );
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            token
        );

        // Act
        var response = await _client.GetAsync($"/api/admin/v1/users/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        // Cleanup
        _client.DefaultRequestHeaders.Authorization = null;
    }

    [Fact]
    public async Task SuspendUser_WithValidData_ReturnsSuspensionResponse()
    {
        // Arrange
        var adminEmail = $"admin-suspend-{Guid.NewGuid()}@example.com";
        var token = await _factory.CreateTestUserAndGetTokenAsync(
            adminEmail,
            "Test123!",
            UserType.Admin
        );
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            token
        );

        // Create a user to suspend
        var targetUser = await _factory.CreateTestUserAsync(
            $"suspend-target-{Guid.NewGuid()}@example.com",
            "Test123!"
        );

        var request = new SuspendUserRequest(Reason: "Violation of terms of service");

        // Act
        var response = await _client.PostAsJsonAsync(
            $"/api/admin/v1/users/{targetUser.Id}/suspend",
            request
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<UserSuspensionResponse>>(
            LykeWebApplicationFactory.JsonOptions
        );
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.IsActive.Should().BeFalse();

        // Cleanup
        _client.DefaultRequestHeaders.Authorization = null;
    }

    [Fact]
    public async Task UnsuspendUser_WhenSuspended_ReturnsUnsuspensionResponse()
    {
        // Arrange
        var adminEmail = $"admin-unsuspend-{Guid.NewGuid()}@example.com";
        var token = await _factory.CreateTestUserAndGetTokenAsync(
            adminEmail,
            "Test123!",
            UserType.Admin
        );
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            token
        );

        // Create and suspend a user
        var targetUser = await _factory.CreateTestUserAsync(
            $"unsuspend-target-{Guid.NewGuid()}@example.com",
            "Test123!"
        );
        await _client.PostAsJsonAsync(
            $"/api/admin/v1/users/{targetUser.Id}/suspend",
            new SuspendUserRequest(Reason: "Test suspension")
        );

        // Act
        var response = await _client.PostAsync(
            $"/api/admin/v1/users/{targetUser.Id}/unsuspend",
            null
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<UserSuspensionResponse>>(
            LykeWebApplicationFactory.JsonOptions
        );
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.IsActive.Should().BeTrue();

        // Cleanup
        _client.DefaultRequestHeaders.Authorization = null;
    }

    #endregion

    #region Post Moderation Tests

    [Fact]
    public async Task GetPendingPosts_WithAdminToken_ReturnsPosts()
    {
        // Arrange
        var email = $"admin-posts-{Guid.NewGuid()}@example.com";
        var token = await _factory.CreateTestUserAndGetTokenAsync(
            email,
            "Test123!",
            UserType.Admin
        );
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            token
        );

        // Act
        var response = await _client.GetAsync("/api/admin/v1/posts");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<
            ApiResponse<IReadOnlyList<PendingPostResponse>>
        >(LykeWebApplicationFactory.JsonOptions);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();

        // Cleanup
        _client.DefaultRequestHeaders.Authorization = null;
    }

    [Fact]
    public async Task GetPendingPosts_WithStatusFilter_ReturnsFilteredPosts()
    {
        // Arrange
        var email = $"admin-posts-filter-{Guid.NewGuid()}@example.com";
        var token = await _factory.CreateTestUserAndGetTokenAsync(
            email,
            "Test123!",
            UserType.Admin
        );
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            token
        );

        // Act
        var response = await _client.GetAsync("/api/admin/v1/posts?status=PendingReview");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<
            ApiResponse<IReadOnlyList<PendingPostResponse>>
        >(LykeWebApplicationFactory.JsonOptions);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();

        // Cleanup
        _client.DefaultRequestHeaders.Authorization = null;
    }

    #endregion

    #region Verification Management Tests

    [Fact]
    public async Task GetPendingVerifications_WithAdminToken_ReturnsVerifications()
    {
        // Arrange
        var email = $"admin-verifications-{Guid.NewGuid()}@example.com";
        var token = await _factory.CreateTestUserAndGetTokenAsync(
            email,
            "Test123!",
            UserType.Admin
        );
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            token
        );

        // Act
        var response = await _client.GetAsync("/api/admin/v1/verifications");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<
            ApiResponse<IReadOnlyList<PendingVerificationResponse>>
        >(LykeWebApplicationFactory.JsonOptions);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();

        // Cleanup
        _client.DefaultRequestHeaders.Authorization = null;
    }

    #endregion

    #region Platform Stats Tests

    [Fact]
    public async Task GetPlatformStats_WithAdminToken_ReturnsStats()
    {
        // Arrange
        var email = $"admin-stats-{Guid.NewGuid()}@example.com";
        var token = await _factory.CreateTestUserAndGetTokenAsync(
            email,
            "Test123!",
            UserType.Admin
        );
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            token
        );

        // Act
        var response = await _client.GetAsync("/api/admin/v1/stats");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<PlatformStatsResponse>>(
            LykeWebApplicationFactory.JsonOptions
        );
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();

        // Cleanup
        _client.DefaultRequestHeaders.Authorization = null;
    }

    #endregion
}
