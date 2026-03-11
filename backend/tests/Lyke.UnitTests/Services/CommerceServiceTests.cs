using FluentAssertions;
using Lyke.Application.Configuration;
using Lyke.Application.DTOs.Commerce;
using Lyke.Application.Interfaces;
using Lyke.Application.Services;
using Lyke.Core.Entities;
using Lyke.Core.Enums;
using Lyke.Core.Exceptions;
using Lyke.Infrastructure.Data;
using Lyke.UnitTests.Fixtures;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Lyke.UnitTests.Services;

public class CommerceServiceTests : IDisposable
{
    private readonly LykeDbContext _context;
    private readonly IOptions<CommerceSettings> _commerceSettings;
    private readonly Mock<ILogger<CommerceService>> _loggerMock;
    private readonly CommerceService _sut;

    public CommerceServiceTests()
    {
        _context = TestDbContextFactory.Create();
        _commerceSettings = Options.Create(
            new CommerceSettings
            {
                CreatorCommissionShare = 0.7m,
                DefaultAttributionWindowDays = 30,
                ClickRateLimitPerMinute = 60,
                EnableClickDeduplication = true,
                DeduplicationWindowSeconds = 10,
            }
        );
        _loggerMock = new Mock<ILogger<CommerceService>>();

        _sut = new CommerceService(
            _context,
            _commerceSettings,
            new Mock<IEventTrackingService>().Object,
            new MemoryCache(new MemoryCacheOptions()),
            _loggerMock.Object
        );
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    private async Task<(
        User user,
        Creator creator,
        Retailer retailer,
        Product product,
        Post post,
        PostProduct postProduct
    )> SetupTestDataAsync()
    {
        await TestDbContextFactory.SeedTestDataAsync(_context);

        var user = TestDbContextFactory.CreateTestUser(userType: UserType.Creator);
        _context.Users.Add(user);

        var creator = TestDbContextFactory.CreateTestCreator(userId: user.Id);
        _context.Creators.Add(creator);

        var retailer = TestDbContextFactory.CreateTestRetailer();
        _context.Retailers.Add(retailer);

        var product = TestDbContextFactory.CreateTestProduct(retailerId: retailer.Id);
        product.Retailer = retailer;
        _context.Products.Add(product);

        var post = TestDbContextFactory.CreateTestPost(
            creatorId: creator.Id,
            status: PostStatus.Published
        );
        post.Creator = creator;
        _context.Posts.Add(post);

        var postProduct = TestDbContextFactory.CreateTestPostProduct(
            postId: post.Id,
            productId: product.Id
        );
        postProduct.Post = post;
        postProduct.Product = product;
        _context.PostProducts.Add(postProduct);

        await _context.SaveChangesAsync();

        return (user, creator, retailer, product, post, postProduct);
    }

    [Fact]
    public async Task TrackClickAsync_WithValidRequest_CreatesClickEvent()
    {
        // Arrange
        var (user, _, retailer, product, post, postProduct) = await SetupTestDataAsync();
        var request = new TrackClickRequest(
            PostId: post.Id,
            PostProductId: postProduct.Id,
            SessionId: "test-session-id",
            Source: "feed",
            Platform: "ios",
            AppVersion: "1.0.0",
            FeedPosition: 1,
            SearchQuery: null
        );

        // Act
        var result = await _sut.TrackClickAsync(user.Id, request, "TestUserAgent", "127.0.0.1");

        // Assert
        result.Should().NotBeNull();
        result.ClickId.Should().NotBeEmpty();
        result.RetailerName.Should().Be(retailer.Name);
        result.ProductName.Should().Be(product.Name);

        var clickEvent = _context.ClickEvents.FirstOrDefault(ce => ce.Id == result.ClickId);
        clickEvent.Should().NotBeNull();
        clickEvent!.PostId.Should().Be(post.Id);
        clickEvent.PostProductId.Should().Be(postProduct.Id);
    }

    [Fact]
    public async Task TrackClickAsync_WithInvalidPostProduct_ThrowsNotFoundException()
    {
        // Arrange
        var (user, _, _, _, post, _) = await SetupTestDataAsync();
        var request = new TrackClickRequest(
            PostId: post.Id,
            PostProductId: Guid.NewGuid(), // Invalid
            SessionId: "test-session-id",
            Source: "feed",
            Platform: "ios",
            AppVersion: "1.0.0",
            FeedPosition: null,
            SearchQuery: null
        );

        // Act
        var act = () => _sut.TrackClickAsync(user.Id, request, null, null);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task TrackClickAsync_WithDuplicateClick_ReturnsSameClickId()
    {
        // Arrange
        var (user, _, retailer, _, post, postProduct) = await SetupTestDataAsync();
        var sessionId = "test-session-for-dedup";

        // Create an existing click
        var existingClick = new ClickEvent
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            PostId = post.Id,
            PostProductId = postProduct.Id,
            SessionId = sessionId,
            CreatedAt = DateTime.UtcNow, // Within deduplication window
        };
        _context.ClickEvents.Add(existingClick);
        await _context.SaveChangesAsync();

        var request = new TrackClickRequest(
            PostId: post.Id,
            PostProductId: postProduct.Id,
            SessionId: sessionId,
            Source: "feed",
            Platform: "ios",
            AppVersion: "1.0.0",
            FeedPosition: null,
            SearchQuery: null
        );

        // Act
        var result = await _sut.TrackClickAsync(user.Id, request, null, null);

        // Assert
        result.ClickId.Should().Be(existingClick.Id); // Should return existing click
    }

    [Fact]
    public async Task GetProductAsync_WithValidId_ReturnsProduct()
    {
        // Arrange
        var (_, _, retailer, product, _, _) = await SetupTestDataAsync();

        // Act
        var result = await _sut.GetProductAsync(product.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(product.Id);
        result.Name.Should().Be(product.Name);
        result.RetailerName.Should().Be(retailer.Name);
    }

    [Fact]
    public async Task GetProductAsync_WithInvalidId_ThrowsNotFoundException()
    {
        // Arrange
        var nonexistentProductId = Guid.NewGuid();

        // Act
        var act = () => _sut.GetProductAsync(nonexistentProductId);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task SearchProductsAsync_WithMatchingQuery_ReturnsProducts()
    {
        // Arrange
        var (_, _, _, _, _, _) = await SetupTestDataAsync();
        var request = new ProductSearchRequest("Test", null, null, 1, 20);

        // Act
        var (products, meta) = await _sut.SearchProductsAsync(request);

        // Assert
        products.Should().NotBeNull();
        products.Should().HaveCount(1);
        meta.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task SearchProductsAsync_WithNoMatches_ReturnsEmptyList()
    {
        // Arrange
        await SetupTestDataAsync();
        var request = new ProductSearchRequest("NonExistentProduct12345", null, null, 1, 20);

        // Act
        var (products, meta) = await _sut.SearchProductsAsync(request);

        // Assert
        products.Should().NotBeNull();
        products.Should().BeEmpty();
        meta.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task SearchProductsAsync_WithRetailerFilter_ReturnsFilteredProducts()
    {
        // Arrange
        var (_, _, retailer, _, _, _) = await SetupTestDataAsync();
        var request = new ProductSearchRequest("Test", retailer.Id, null, 1, 20);

        // Act
        var (products, meta) = await _sut.SearchProductsAsync(request);

        // Assert
        products.Should().NotBeNull();
        products.Should().HaveCount(1);
        products.First().RetailerId.Should().Be(retailer.Id);
    }

    [Fact]
    public async Task GetRetailersAsync_ReturnsActiveRetailers()
    {
        // Arrange
        var (_, _, retailer, _, _, _) = await SetupTestDataAsync();

        // Act
        var result = await _sut.GetRetailersAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result.First().Id.Should().Be(retailer.Id);
        result.First().IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task GetRetailersAsync_ExcludesInactiveRetailers()
    {
        // Arrange
        await TestDbContextFactory.SeedTestDataAsync(_context);

        var inactiveRetailer = TestDbContextFactory.CreateTestRetailer(
            name: "Inactive Retailer",
            isActive: false
        );
        _context.Retailers.Add(inactiveRetailer);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetRetailersAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().NotContain(r => r.Name == "Inactive Retailer");
    }

    [Fact]
    public async Task GetRetailerProductsAsync_WithValidRetailer_ReturnsProducts()
    {
        // Arrange
        var (_, _, retailer, product, _, _) = await SetupTestDataAsync();

        // Act
        var (data, meta) = await _sut.GetRetailerProductsAsync(retailer.Id, 1, 20, null);

        // Assert
        data.Should().NotBeNull();
        data.RetailerId.Should().Be(retailer.Id);
        data.RetailerName.Should().Be(retailer.Name);
        data.Products.Should().HaveCount(1);
        meta.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task GetRetailerProductsAsync_WithInvalidRetailer_ThrowsNotFoundException()
    {
        // Arrange
        var nonexistentRetailerId = Guid.NewGuid();

        // Act
        var act = () => _sut.GetRetailerProductsAsync(nonexistentRetailerId, 1, 20, null);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetRetailerProductsAsync_WithCategoryFilter_ReturnsFilteredProducts()
    {
        // Arrange
        var (_, _, retailer, _, _, _) = await SetupTestDataAsync();

        // Act
        var (data, meta) = await _sut.GetRetailerProductsAsync(retailer.Id, 1, 20, "Tops");

        // Assert
        data.Products.Should().HaveCount(1);
    }

    [Fact]
    public async Task ProcessConversionAsync_WithValidClick_CreatesEarning()
    {
        // Arrange
        var (user, creator, retailer, _, post, postProduct) = await SetupTestDataAsync();

        var clickEvent = new ClickEvent
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            PostId = post.Id,
            PostProductId = postProduct.Id,
            CreatedAt = DateTime.UtcNow,
        };
        _context.ClickEvents.Add(clickEvent);
        await _context.SaveChangesAsync();

        var request = new ConversionWebhookRequest(
            ClickId: clickEvent.Id.ToString(),
            OrderId: "ORDER-123",
            OrderValue: 100.00m,
            CommissionAmount: 10.00m,
            Currency: "USD",
            ProductSku: "SKU",
            TransactionDate: DateTime.UtcNow,
            Signature: default! // No signature verification for this test
        );

        // Act
        var result = await _sut.ProcessConversionAsync(retailer.Id, request);

        // Assert
        result.Should().BeTrue();

        var earning = _context.CreatorEarnings.FirstOrDefault(e => e.ClickEventId == clickEvent.Id);
        earning.Should().NotBeNull();
        earning!.CreatorId.Should().Be(creator.Id);
        earning.Amount.Should().Be(10.00m * 0.7m); // 70% commission share
        earning.Currency.Should().Be("USD");
        earning.Status.Should().Be(EarningStatus.Pending);
    }

    [Fact]
    public async Task ProcessConversionAsync_WithInvalidClickId_ReturnsFalse()
    {
        // Arrange
        var (_, _, retailer, _, _, _) = await SetupTestDataAsync();

        var request = new ConversionWebhookRequest(
            ClickId: "invalid-click-id",
            OrderId: "ORDER-123",
            OrderValue: 100.00m,
            CommissionAmount: 10.00m,
            Currency: "USD",
            ProductSku: "SKU",
            TransactionDate: DateTime.UtcNow,
            Signature: default!
        );

        // Act
        var result = await _sut.ProcessConversionAsync(retailer.Id, request);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ProcessConversionAsync_WithAlreadyConvertedClick_ReturnsFalse()
    {
        // Arrange
        var (user, _, retailer, _, post, postProduct) = await SetupTestDataAsync();

        var clickEvent = new ClickEvent
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            PostId = post.Id,
            PostProductId = postProduct.Id,
            CreatedAt = DateTime.UtcNow,
            ConvertedAt = DateTime.UtcNow, // Already converted
        };
        _context.ClickEvents.Add(clickEvent);
        await _context.SaveChangesAsync();

        var request = new ConversionWebhookRequest(
            ClickId: clickEvent.Id.ToString(),
            OrderId: "ORDER-123",
            OrderValue: 100.00m,
            CommissionAmount: 10.00m,
            Currency: "USD",
            ProductSku: "SKU",
            TransactionDate: DateTime.UtcNow,
            Signature: default!
        );

        // Act
        var result = await _sut.ProcessConversionAsync(retailer.Id, request);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ProcessConversionAsync_WithInvalidRetailer_ThrowsNotFoundException()
    {
        // Arrange
        var nonexistentRetailerId = Guid.NewGuid();
        var request = new ConversionWebhookRequest(
            ClickId: Guid.NewGuid().ToString(),
            OrderId: "ORDER-123",
            OrderValue: 100.00m,
            CommissionAmount: 10.00m,
            Currency: "USD",
            ProductSku: "SKU",
            TransactionDate: DateTime.UtcNow,
            Signature: default!
        );

        // Act
        var act = () => _sut.ProcessConversionAsync(nonexistentRetailerId, request);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
