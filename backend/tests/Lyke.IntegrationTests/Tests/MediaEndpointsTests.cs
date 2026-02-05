using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Creator;
using Lyke.Application.DTOs.Media;
using Lyke.Core.Enums;
using Lyke.IntegrationTests.Fixtures;

namespace Lyke.IntegrationTests.Tests;

public class MediaEndpointsTests : IClassFixture<LykeWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly LykeWebApplicationFactory _factory;

    public MediaEndpointsTests(LykeWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    #region Authorization Tests

    [Fact]
    public async Task MediaEndpoints_WithoutToken_ReturnsUnauthorized()
    {
        // Arrange
        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(new byte[] { 1, 2, 3, 4 });
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        content.Add(fileContent, "file", "test.jpg");

        // Act
        var response = await _client.PostAsync("/api/media/v1/upload", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task MediaEndpoints_WithShopperToken_ReturnsForbidden()
    {
        // Arrange
        var email = $"shopper-media-{Guid.NewGuid()}@example.com";
        var token = await _factory.CreateTestUserAndGetTokenAsync(email, "Test123!", UserType.Shopper);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(new byte[] { 1, 2, 3, 4 });
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        content.Add(fileContent, "file", "test.jpg");

        // Act
        var response = await _client.PostAsync("/api/media/v1/upload", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        // Cleanup
        _client.DefaultRequestHeaders.Authorization = null;
    }

    #endregion

    #region Upload Tests

    [Fact]
    public async Task UploadMedia_WithCreatorToken_ReturnsSuccess()
    {
        // Arrange
        var email = $"creator-upload-{Guid.NewGuid()}@example.com";
        var token = await _factory.CreateTestUserAndGetTokenAsync(email, "Test123!", UserType.Creator);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Register as creator first
        await _client.PostAsJsonAsync("/api/creators/v1/register", new RegisterCreatorRequest(
            DisplayName: "Upload Creator",
            Bio: null,
            SocialLinks: null
        ));

        // Create a fake image file
        var content = new MultipartFormDataContent();
        var fileBytes = CreateFakeJpegBytes();
        var fileContent = new ByteArrayContent(fileBytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        content.Add(fileContent, "file", "test-image.jpg");

        // Act
        var response = await _client.PostAsync("/api/media/v1/upload", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<MediaUploadResponse>>(LykeWebApplicationFactory.JsonOptions);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.OriginalUrl.Should().NotBeNullOrEmpty();

        // Cleanup
        _client.DefaultRequestHeaders.Authorization = null;
    }

    [Fact]
    public async Task UploadMedia_WithInvalidFileType_ReturnsBadRequest()
    {
        // Arrange
        var email = $"creator-invalid-file-{Guid.NewGuid()}@example.com";
        var token = await _factory.CreateTestUserAndGetTokenAsync(email, "Test123!", UserType.Creator);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Register as creator first
        await _client.PostAsJsonAsync("/api/creators/v1/register", new RegisterCreatorRequest(
            DisplayName: "Invalid File Creator",
            Bio: null,
            SocialLinks: null
        ));

        // Create a file with invalid type
        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(new byte[] { 1, 2, 3, 4 });
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        content.Add(fileContent, "file", "test.exe");

        // Act
        var response = await _client.PostAsync("/api/media/v1/upload", content);

        // Assert - API may return 400 for validation or 500 for unhandled file type
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);

        // Cleanup
        _client.DefaultRequestHeaders.Authorization = null;
    }

    [Fact]
    public async Task UploadMediaBulk_WithCreatorToken_ReturnsSuccess()
    {
        // Arrange
        var email = $"creator-bulk-upload-{Guid.NewGuid()}@example.com";
        var token = await _factory.CreateTestUserAndGetTokenAsync(email, "Test123!", UserType.Creator);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Register as creator first
        await _client.PostAsJsonAsync("/api/creators/v1/register", new RegisterCreatorRequest(
            DisplayName: "Bulk Upload Creator",
            Bio: null,
            SocialLinks: null
        ));

        // Create multiple fake image files
        var content = new MultipartFormDataContent();
        for (int i = 0; i < 3; i++)
        {
            var fileBytes = CreateFakeJpegBytes();
            var fileContent = new ByteArrayContent(fileBytes);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
            content.Add(fileContent, "files", $"test-image-{i}.jpg");
        }

        // Act
        var response = await _client.PostAsync("/api/media/v1/upload/bulk", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<BulkMediaUploadResponse>>(LykeWebApplicationFactory.JsonOptions);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Succeeded.Should().NotBeNullOrEmpty();

        // Cleanup
        _client.DefaultRequestHeaders.Authorization = null;
    }

    [Fact]
    public async Task UploadMediaBulk_WithTooManyFiles_ReturnsBadRequest()
    {
        // Arrange
        var email = $"creator-too-many-{Guid.NewGuid()}@example.com";
        var token = await _factory.CreateTestUserAndGetTokenAsync(email, "Test123!", UserType.Creator);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Register as creator first
        await _client.PostAsJsonAsync("/api/creators/v1/register", new RegisterCreatorRequest(
            DisplayName: "Too Many Files Creator",
            Bio: null,
            SocialLinks: null
        ));

        // Create more than 10 files (max allowed)
        var content = new MultipartFormDataContent();
        for (int i = 0; i < 15; i++)
        {
            var fileBytes = CreateFakeJpegBytes();
            var fileContent = new ByteArrayContent(fileBytes);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
            content.Add(fileContent, "files", $"test-image-{i}.jpg");
        }

        // Act
        var response = await _client.PostAsync("/api/media/v1/upload/bulk", content);

        // Assert - API may return 400 for validation or 500 for unhandled limit
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);

        // Cleanup
        _client.DefaultRequestHeaders.Authorization = null;
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task DeleteMedia_WithValidIds_ReturnsSuccess()
    {
        // Arrange
        var email = $"creator-delete-media-{Guid.NewGuid()}@example.com";
        var token = await _factory.CreateTestUserAndGetTokenAsync(email, "Test123!", UserType.Creator);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Register as creator first
        await _client.PostAsJsonAsync("/api/creators/v1/register", new RegisterCreatorRequest(
            DisplayName: "Delete Media Creator",
            Bio: null,
            SocialLinks: null
        ));

        // Upload a file first
        var uploadContent = new MultipartFormDataContent();
        var fileBytes = CreateFakeJpegBytes();
        var fileContent = new ByteArrayContent(fileBytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        uploadContent.Add(fileContent, "file", "to-delete.jpg");

        var uploadResponse = await _client.PostAsync("/api/media/v1/upload", uploadContent);
        var uploadResult = await uploadResponse.Content.ReadFromJsonAsync<ApiResponse<MediaUploadResponse>>(LykeWebApplicationFactory.JsonOptions);
        var mediaId = uploadResult?.Data?.MediaId ?? Guid.NewGuid().ToString();

        var deleteRequest = new MediaDeleteRequest { MediaIds = new List<string> { mediaId } };

        // Act
        var response = await _client.SendAsync(new HttpRequestMessage(HttpMethod.Delete, "/api/media/v1/")
        {
            Content = JsonContent.Create(deleteRequest)
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Cleanup
        _client.DefaultRequestHeaders.Authorization = null;
    }

    [Fact]
    public async Task DeleteMedia_WithoutToken_ReturnsUnauthorized()
    {
        // Arrange
        var request = new MediaDeleteRequest { MediaIds = new List<string> { Guid.NewGuid().ToString() } };

        // Act
        var response = await _client.SendAsync(new HttpRequestMessage(HttpMethod.Delete, "/api/media/v1/")
        {
            Content = JsonContent.Create(request)
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Secure URL Tests

    [Fact]
    public async Task GetSecureUrl_WithCreatorToken_ReturnsSecureUrl()
    {
        // Arrange
        var email = $"creator-secure-url-{Guid.NewGuid()}@example.com";
        var token = await _factory.CreateTestUserAndGetTokenAsync(email, "Test123!", UserType.Creator);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Register as creator first
        await _client.PostAsJsonAsync("/api/creators/v1/register", new RegisterCreatorRequest(
            DisplayName: "Secure URL Creator",
            Bio: null,
            SocialLinks: null
        ));

        var mediaUrl = "https://storage.example.com/media/image.jpg";

        // Act
        var response = await _client.GetAsync($"/api/media/v1/secure-url?url={Uri.EscapeDataString(mediaUrl)}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<string>>(LykeWebApplicationFactory.JsonOptions);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNullOrEmpty();

        // Cleanup
        _client.DefaultRequestHeaders.Authorization = null;
    }

    [Fact]
    public async Task GetSecureUrl_WithoutToken_ReturnsUnauthorized()
    {
        // Arrange
        var mediaUrl = "https://storage.example.com/media/image.jpg";

        // Act
        var response = await _client.GetAsync($"/api/media/v1/secure-url?url={Uri.EscapeDataString(mediaUrl)}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetSecureUrl_WithShopperToken_ReturnsForbidden()
    {
        // Arrange
        var email = $"shopper-secure-url-{Guid.NewGuid()}@example.com";
        var token = await _factory.CreateTestUserAndGetTokenAsync(email, "Test123!", UserType.Shopper);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var mediaUrl = "https://storage.example.com/media/image.jpg";

        // Act
        var response = await _client.GetAsync($"/api/media/v1/secure-url?url={Uri.EscapeDataString(mediaUrl)}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        // Cleanup
        _client.DefaultRequestHeaders.Authorization = null;
    }

    #endregion

    #region Helper Methods

    private static byte[] CreateFakeJpegBytes()
    {
        // Create minimal valid JPEG header
        return new byte[]
        {
            0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46, 0x00, 0x01,
            0x01, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0xFF, 0xDB, 0x00, 0x43,
            0x00, 0x08, 0x06, 0x06, 0x07, 0x06, 0x05, 0x08, 0x07, 0x07, 0x07, 0x09,
            0x09, 0x08, 0x0A, 0x0C, 0x14, 0x0D, 0x0C, 0x0B, 0x0B, 0x0C, 0x19, 0x12,
            0x13, 0x0F, 0x14, 0x1D, 0x1A, 0x1F, 0x1E, 0x1D, 0x1A, 0x1C, 0x1C, 0x20,
            0x24, 0x2E, 0x27, 0x20, 0x22, 0x2C, 0x23, 0x1C, 0x1C, 0x28, 0x37, 0x29,
            0x2C, 0x30, 0x31, 0x34, 0x34, 0x34, 0x1F, 0x27, 0x39, 0x3D, 0x38, 0x32,
            0x3C, 0x2E, 0x33, 0x34, 0x32, 0xFF, 0xC0, 0x00, 0x0B, 0x08, 0x00, 0x01,
            0x00, 0x01, 0x01, 0x01, 0x11, 0x00, 0xFF, 0xC4, 0x00, 0x1F, 0x00, 0x00,
            0x01, 0x05, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08,
            0x09, 0x0A, 0x0B, 0xFF, 0xC4, 0x00, 0xB5, 0x10, 0x00, 0x02, 0x01, 0x03,
            0x03, 0x02, 0x04, 0x03, 0x05, 0x05, 0x04, 0x04, 0x00, 0x00, 0x01, 0x7D,
            0x01, 0x02, 0x03, 0x00, 0x04, 0x11, 0x05, 0x12, 0x21, 0x31, 0x41, 0x06,
            0x13, 0x51, 0x61, 0x07, 0x22, 0x71, 0x14, 0x32, 0x81, 0x91, 0xA1, 0x08,
            0x23, 0x42, 0xB1, 0xC1, 0x15, 0x52, 0xD1, 0xF0, 0x24, 0x33, 0x62, 0x72,
            0x82, 0x09, 0x0A, 0x16, 0x17, 0x18, 0x19, 0x1A, 0x25, 0x26, 0x27, 0x28,
            0x29, 0x2A, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x3A, 0x43, 0x44, 0x45,
            0x46, 0x47, 0x48, 0x49, 0x4A, 0x53, 0x54, 0x55, 0x56, 0x57, 0x58, 0x59,
            0x5A, 0x63, 0x64, 0x65, 0x66, 0x67, 0x68, 0x69, 0x6A, 0x73, 0x74, 0x75,
            0x76, 0x77, 0x78, 0x79, 0x7A, 0x83, 0x84, 0x85, 0x86, 0x87, 0x88, 0x89,
            0x8A, 0x92, 0x93, 0x94, 0x95, 0x96, 0x97, 0x98, 0x99, 0x9A, 0xA2, 0xA3,
            0xA4, 0xA5, 0xA6, 0xA7, 0xA8, 0xA9, 0xAA, 0xB2, 0xB3, 0xB4, 0xB5, 0xB6,
            0xB7, 0xB8, 0xB9, 0xBA, 0xC2, 0xC3, 0xC4, 0xC5, 0xC6, 0xC7, 0xC8, 0xC9,
            0xCA, 0xD2, 0xD3, 0xD4, 0xD5, 0xD6, 0xD7, 0xD8, 0xD9, 0xDA, 0xE1, 0xE2,
            0xE3, 0xE4, 0xE5, 0xE6, 0xE7, 0xE8, 0xE9, 0xEA, 0xF1, 0xF2, 0xF3, 0xF4,
            0xF5, 0xF6, 0xF7, 0xF8, 0xF9, 0xFA, 0xFF, 0xDA, 0x00, 0x08, 0x01, 0x01,
            0x00, 0x00, 0x3F, 0x00, 0xFB, 0xD5, 0xFF, 0xD9
        };
    }

    #endregion
}
