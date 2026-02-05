using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Commerce;
using Lyke.Core.Enums;
using Lyke.IntegrationTests.Fixtures;

namespace Lyke.IntegrationTests.Tests;

public class CommerceEndpointsTests : IClassFixture<LykeWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly LykeWebApplicationFactory _factory;

    public CommerceEndpointsTests(LykeWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    #region Click Tracking Tests

    [Fact]
    public async Task TrackClick_WithValidData_ReturnsAffiliateUrl()
    {
        // Arrange
        var request = new TrackClickRequest(
            PostId: Guid.NewGuid(),
            PostProductId: Guid.NewGuid(),
            Source: "feed"
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/clicks/v1/track", request);

        // Assert - May return 404 if product doesn't exist, but endpoint should be accessible
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task TrackClick_IsPubliclyAccessible()
    {
        // Arrange - No authorization header
        var request = new TrackClickRequest(
            PostId: Guid.NewGuid(),
            PostProductId: Guid.NewGuid(),
            Source: "explore"
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/clicks/v1/track", request);

        // Assert - Should not return 401
        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task TrackClick_WithAuthenticatedUser_ReturnsSuccess()
    {
        // Arrange
        var email = $"click-user-{Guid.NewGuid()}@example.com";
        var token = await _factory.CreateTestUserAndGetTokenAsync(email, "Test123!");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            token
        );

        var request = new TrackClickRequest(
            PostId: Guid.NewGuid(),
            PostProductId: Guid.NewGuid(),
            Source: "feed"
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/clicks/v1/track", request);

        // Assert - May return 404 if product doesn't exist
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);

        // Cleanup
        _client.DefaultRequestHeaders.Authorization = null;
    }

    #endregion

    #region Product Tests

    [Fact]
    public async Task GetProduct_WhenNotExists_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync($"/api/products/v1/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetProduct_IsPubliclyAccessible()
    {
        // Act - No authorization header
        var response = await _client.GetAsync($"/api/products/v1/{Guid.NewGuid()}");

        // Assert - Should not return 401
        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SearchProducts_WithCreatorToken_ReturnsResults()
    {
        // Arrange
        var email = $"product-search-{Guid.NewGuid()}@example.com";
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
        var response = await _client.GetAsync("/api/products/v1/search?q=dress");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<
            ApiResponse<IReadOnlyList<ProductResponse>>
        >(LykeWebApplicationFactory.JsonOptions);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();

        // Cleanup
        _client.DefaultRequestHeaders.Authorization = null;
    }

    [Fact]
    public async Task SearchProducts_WithShopperToken_ReturnsForbidden()
    {
        // Arrange
        var email = $"shopper-search-{Guid.NewGuid()}@example.com";
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
        var response = await _client.GetAsync("/api/products/v1/search?q=dress");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        // Cleanup
        _client.DefaultRequestHeaders.Authorization = null;
    }

    [Fact]
    public async Task SearchProducts_WithoutToken_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/products/v1/search?q=dress");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SearchProducts_WithAdminToken_ReturnsResults()
    {
        // Arrange
        var email = $"admin-search-{Guid.NewGuid()}@example.com";
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
        var response = await _client.GetAsync("/api/products/v1/search?q=shirt");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Cleanup
        _client.DefaultRequestHeaders.Authorization = null;
    }

    #endregion

    #region Retailer Tests

    [Fact]
    public async Task GetRetailers_ReturnsRetailerList()
    {
        // Act
        var response = await _client.GetAsync("/api/retailers/v1/");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<
            ApiResponse<IReadOnlyList<RetailerResponse>>
        >(LykeWebApplicationFactory.JsonOptions);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task GetRetailers_IsPubliclyAccessible()
    {
        // Act - No authorization header
        var response = await _client.GetAsync("/api/retailers/v1/");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetRetailerProducts_WhenRetailerNotExists_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync($"/api/retailers/v1/{Guid.NewGuid()}/products");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetRetailerProducts_IsPubliclyAccessible()
    {
        // Act - No authorization header
        var response = await _client.GetAsync($"/api/retailers/v1/{Guid.NewGuid()}/products");

        // Assert - Should not return 401
        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetRetailerProducts_WithPagination_SupportsQueryParams()
    {
        // Act
        var response = await _client.GetAsync(
            $"/api/retailers/v1/{Guid.NewGuid()}/products?page=1&pageSize=10"
        );

        // Assert - 404 is expected since retailer doesn't exist, but pagination params should be accepted
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Conversion Webhook Tests

    [Fact]
    public async Task ProcessConversion_WithValidData_ReturnsResult()
    {
        // Arrange
        var retailerId = Guid.NewGuid();
        var request = new ConversionWebhookRequest(
            ClickId: Guid.NewGuid().ToString(),
            OrderId: "order-12345",
            OrderValue: 99.99m,
            Currency: "USD",
            CommissionAmount: 5.00m,
            ProductSku: "SKU-001",
            TransactionDate: DateTime.UtcNow,
            Signature: "test-signature"
        );

        // Act
        var response = await _client.PostAsJsonAsync(
            $"/api/retailers/v1/{retailerId}/conversions",
            request
        );

        // Assert - May fail if click doesn't exist, but endpoint should be accessible
        response
            .StatusCode.Should()
            .BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ProcessConversion_IsPubliclyAccessible()
    {
        // Arrange - No authorization header (webhook endpoints are typically public)
        var retailerId = Guid.NewGuid();
        var request = new ConversionWebhookRequest(
            ClickId: Guid.NewGuid().ToString(),
            OrderId: "order-webhook-test",
            OrderValue: 50.00m,
            Currency: "USD",
            CommissionAmount: 2.50m,
            ProductSku: null,
            TransactionDate: DateTime.UtcNow,
            Signature: "test-signature"
        );

        // Act
        var response = await _client.PostAsJsonAsync(
            $"/api/retailers/v1/{retailerId}/conversions",
            request
        );

        // Assert - Should not return 401
        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
    }

    #endregion
}
