using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Profile;
using Lyke.Core.Enums;
using Lyke.IntegrationTests.Fixtures;

namespace Lyke.IntegrationTests.Tests;

public class ProfileEndpointsTests : IClassFixture<LykeWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly LykeWebApplicationFactory _factory;

    public ProfileEndpointsTests(LykeWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    #region Get Profile Tests

    [Fact]
    public async Task GetProfile_WithValidToken_ReturnsProfile()
    {
        // Arrange
        var email = $"profile-get-{Guid.NewGuid()}@example.com";
        var token = await _factory.CreateTestUserAndGetTokenAsync(email, "Test123!");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            token
        );

        // Act
        var response = await _client.GetAsync("/api/profile/v1/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<UserProfileResponse>>(
            LykeWebApplicationFactory.JsonOptions
        );
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Email.Should().Be(email);

        // Cleanup
        _client.DefaultRequestHeaders.Authorization = null;
    }

    [Fact]
    public async Task GetProfile_WithoutToken_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/profile/v1/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Update Profile Tests

    [Fact]
    public async Task UpdateProfile_WithValidData_ReturnsUpdatedProfile()
    {
        // Arrange
        var email = $"profile-update-{Guid.NewGuid()}@example.com";
        var token = await _factory.CreateTestUserAndGetTokenAsync(email, "Test123!");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            token
        );

        var newEmail = $"updated-{Guid.NewGuid()}@example.com";
        var request = new UpdateProfileRequest(Email: newEmail);

        // Act
        var response = await _client.PutAsJsonAsync("/api/profile/v1/", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<UserProfileResponse>>(
            LykeWebApplicationFactory.JsonOptions
        );
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();

        // Cleanup
        _client.DefaultRequestHeaders.Authorization = null;
    }

    [Fact]
    public async Task UpdateProfile_WithoutToken_ReturnsUnauthorized()
    {
        // Arrange
        var request = new UpdateProfileRequest(Email: "test@example.com");

        // Act
        var response = await _client.PutAsJsonAsync("/api/profile/v1/", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Body Profile Tests

    [Fact]
    public async Task CreateBodyProfile_WithValidData_ReturnsCreatedProfile()
    {
        // Arrange
        var email = $"body-create-{Guid.NewGuid()}@example.com";
        var token = await _factory.CreateTestUserAndGetTokenAsync(email, "Test123!");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            token
        );

        var request = new CreateBodyProfileRequest(
            HeightCm: 170,
            WeightKg: 65.5m,
            BodyTypeId: 1, // Petite (seeded in test data)
            FitPreference: FitPreference.Regular
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/profile/v1/body", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<BodyProfileResponse>>(
            LykeWebApplicationFactory.JsonOptions
        );
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.HeightCm.Should().Be(170);
        result.Data.WeightKg.Should().Be(65.5m);

        // Cleanup
        _client.DefaultRequestHeaders.Authorization = null;
    }

    [Fact]
    public async Task GetBodyProfile_WhenExists_ReturnsProfile()
    {
        // Arrange
        var email = $"body-get-{Guid.NewGuid()}@example.com";
        var token = await _factory.CreateTestUserAndGetTokenAsync(email, "Test123!");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            token
        );

        // Create body profile first
        var createRequest = new CreateBodyProfileRequest(
            HeightCm: 175,
            WeightKg: 70m,
            BodyTypeId: 2, // Athletic
            FitPreference: FitPreference.Relaxed
        );
        await _client.PostAsJsonAsync("/api/profile/v1/body", createRequest);

        // Act
        var response = await _client.GetAsync("/api/profile/v1/body");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<BodyProfileResponse>>(
            LykeWebApplicationFactory.JsonOptions
        );
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.HeightCm.Should().Be(175);

        // Cleanup
        _client.DefaultRequestHeaders.Authorization = null;
    }

    [Fact]
    public async Task GetBodyProfile_WhenNotExists_ReturnsNotFound()
    {
        // Arrange
        var email = $"body-notfound-{Guid.NewGuid()}@example.com";
        var token = await _factory.CreateTestUserAndGetTokenAsync(email, "Test123!");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            token
        );

        // Act
        var response = await _client.GetAsync("/api/profile/v1/body");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        // Cleanup
        _client.DefaultRequestHeaders.Authorization = null;
    }

    [Fact]
    public async Task UpdateBodyProfile_WithValidData_ReturnsUpdatedProfile()
    {
        // Arrange
        var email = $"body-update-{Guid.NewGuid()}@example.com";
        var token = await _factory.CreateTestUserAndGetTokenAsync(email, "Test123!");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            token
        );

        // Create body profile first
        var createRequest = new CreateBodyProfileRequest(
            HeightCm: 165,
            WeightKg: 60m,
            BodyTypeId: 1
        );
        await _client.PostAsJsonAsync("/api/profile/v1/body", createRequest);

        var updateRequest = new UpdateBodyProfileRequest(
            HeightCm: 166,
            WeightKg: 62m,
            BodyTypeId: 3, // Curvy
            FitPreference: FitPreference.Fitted
        );

        // Act
        var response = await _client.PutAsJsonAsync("/api/profile/v1/body", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<BodyProfileResponse>>(
            LykeWebApplicationFactory.JsonOptions
        );
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.HeightCm.Should().Be(166);
        result.Data.WeightKg.Should().Be(62m);

        // Cleanup
        _client.DefaultRequestHeaders.Authorization = null;
    }

    [Fact]
    public async Task DeleteBodyProfile_WhenExists_ReturnsSuccess()
    {
        // Arrange
        var email = $"body-delete-{Guid.NewGuid()}@example.com";
        var token = await _factory.CreateTestUserAndGetTokenAsync(email, "Test123!");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            token
        );

        // Create body profile first
        var createRequest = new CreateBodyProfileRequest(
            HeightCm: 160,
            WeightKg: 55m,
            BodyTypeId: 1
        );
        await _client.PostAsJsonAsync("/api/profile/v1/body", createRequest);

        // Act
        var response = await _client.DeleteAsync("/api/profile/v1/body");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify deletion
        var getResponse = await _client.GetAsync("/api/profile/v1/body");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);

        // Cleanup
        _client.DefaultRequestHeaders.Authorization = null;
    }

    #endregion

    #region Lookup Endpoints Tests

    [Fact]
    public async Task GetBodyTypes_ReturnsAllBodyTypes()
    {
        // Act
        var response = await _client.GetAsync("/api/lookup/v1/body-types");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<
            ApiResponse<IReadOnlyList<BodyTypeResponse>>
        >(LykeWebApplicationFactory.JsonOptions);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Count.Should().BeGreaterThan(0); // Body types should be available
        // Verify structure of body types rather than specific names
        result.Data.Should().OnlyContain(bt => !string.IsNullOrEmpty(bt.Name));
        result.Data.Should().OnlyContain(bt => bt.Id > 0);
    }

    [Fact]
    public async Task GetFitPreferences_ReturnsFitPreferences()
    {
        // Act
        var response = await _client.GetAsync("/api/lookup/v1/fit-preferences");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<
            ApiResponse<IReadOnlyList<FitPreferenceResponse>>
        >(LykeWebApplicationFactory.JsonOptions);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetBodyTypes_IsAnonymouslyAccessible()
    {
        // Act - No authorization header
        var response = await _client.GetAsync("/api/lookup/v1/body-types");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetFitPreferences_IsAnonymouslyAccessible()
    {
        // Act - No authorization header
        var response = await _client.GetAsync("/api/lookup/v1/fit-preferences");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion
}
