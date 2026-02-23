using FluentAssertions;
using Lyke.Application.Configuration;
using Lyke.Application.DTOs.Retailer;
using Lyke.Application.Services;
using Lyke.Core.Entities;
using Lyke.Core.Enums;
using Lyke.Core.Exceptions;
using Lyke.Infrastructure.Data;
using Lyke.UnitTests.Fixtures;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Lyke.UnitTests.Services;

public class RetailerServiceTests : IDisposable
{
    private readonly LykeDbContext _context;
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly IOptions<RetailerSettings> _retailerSettings;
    private readonly Mock<ILogger<RetailerService>> _loggerMock;
    private readonly RetailerService _sut;

    public RetailerServiceTests()
    {
        _context = TestDbContextFactory.Create();
        _userManagerMock = MockUserManager.Create();
        _retailerSettings = Options.Create(new RetailerSettings
        {
            MaxImportRows = 100,
            MinAnonymityGroupSize = 2,
            DefaultCurrency = "GBP",
            MaxCampaignBudget = 10000m
        });
        _loggerMock = new Mock<ILogger<RetailerService>>();

        _sut = new RetailerService(
            _context,
            _userManagerMock.Object,
            _retailerSettings,
            _loggerMock.Object);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    private async Task SeedBaseDataAsync()
    {
        await TestDbContextFactory.SeedTestDataAsync(_context);
    }

    private async Task<(User user, Retailer retailer)> SetupRetailerDataAsync()
    {
        await SeedBaseDataAsync();

        var user = TestDbContextFactory.CreateTestUser(userType: UserType.Retailer);
        _context.Users.Add(user);

        var retailer = new Retailer
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Name = "Test Retailer",
            WebsiteUrl = "https://test-retailer.com",
            ContactEmail = "contact@test-retailer.com",
            IsActive = true
        };
        _context.Retailers.Add(retailer);
        await _context.SaveChangesAsync();

        return (user, retailer);
    }

    #region Registration Tests

    [Fact]
    public async Task RegisterAsRetailerAsync_ValidRequest_CreatesRetailer()
    {
        // Arrange
        await SeedBaseDataAsync();
        var user = TestDbContextFactory.CreateTestUser(userType: UserType.Shopper);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _userManagerMock.Setup(x => x.FindByIdAsync(user.Id.ToString()))
            .ReturnsAsync(user);
        _userManagerMock.Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync(IdentityResult.Success);

        var request = new RegisterRetailerRequest("New Retailer", null, "https://retailer.com", "info@retailer.com");

        // Act
        var result = await _sut.RegisterAsRetailerAsync(user.Id, request);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("New Retailer");

        var retailer = _context.Retailers.FirstOrDefault(r => r.UserId == user.Id);
        retailer.Should().NotBeNull();
    }

    [Fact]
    public async Task RegisterAsRetailerAsync_AlreadyRegistered_ThrowsValidationException()
    {
        // Arrange
        var (user, _) = await SetupRetailerDataAsync();

        _userManagerMock.Setup(x => x.FindByIdAsync(user.Id.ToString()))
            .ReturnsAsync(user);

        var request = new RegisterRetailerRequest("Another Retailer", null, null, null);

        // Act
        var act = () => _sut.RegisterAsRetailerAsync(user.Id, request);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*already registered as a retailer*");
    }

    [Fact]
    public async Task RegisterAsRetailerAsync_UserNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var nonExistentUserId = Guid.NewGuid();

        _userManagerMock.Setup(x => x.FindByIdAsync(nonExistentUserId.ToString()))
            .ReturnsAsync((User?)null);

        var request = new RegisterRetailerRequest("Retailer", null, null, null);

        // Act
        var act = () => _sut.RegisterAsRetailerAsync(nonExistentUserId, request);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region Profile Tests

    [Fact]
    public async Task GetRetailerProfileAsync_ValidRetailer_ReturnsProfile()
    {
        // Arrange
        var (user, retailer) = await SetupRetailerDataAsync();

        // Act
        var result = await _sut.GetRetailerProfileAsync(user.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(retailer.Id);
        result.Name.Should().Be("Test Retailer");
    }

    [Fact]
    public async Task UpdateRetailerProfileAsync_ValidRequest_UpdatesFields()
    {
        // Arrange
        var (user, _) = await SetupRetailerDataAsync();

        var request = new UpdateRetailerProfileRequest(
            Name: "Updated Retailer",
            LogoUrl: "https://logo.com/new.png",
            WebsiteUrl: null,
            ContactEmail: null,
            AffiliateConfig: null);

        // Act
        var result = await _sut.UpdateRetailerProfileAsync(user.Id, request);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Updated Retailer");
    }

    #endregion

    #region Campaign Tests

    [Fact]
    public async Task CreateCampaignAsync_ValidRequest_CreatesCampaign()
    {
        // Arrange
        var (user, retailer) = await SetupRetailerDataAsync();

        var product = TestDbContextFactory.CreateTestProduct(retailerId: retailer.Id);
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var request = new CreateCampaignRequest(
            ProductId: product.Id,
            BudgetAmount: 5000m,
            TargetBodyTypes: null,
            TargetCategories: null,
            StartDate: DateTime.UtcNow,
            EndDate: DateTime.UtcNow.AddDays(30));

        // Act
        var result = await _sut.CreateCampaignAsync(user.Id, request);

        // Assert
        result.Should().NotBeNull();
        result.BudgetAmount.Should().Be(5000m);
        result.ProductId.Should().Be(product.Id);
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task CreateCampaignAsync_ExceedsBudget_ThrowsValidationException()
    {
        // Arrange
        var (user, _) = await SetupRetailerDataAsync();

        var request = new CreateCampaignRequest(
            ProductId: null,
            BudgetAmount: 99999m, // Exceeds MaxCampaignBudget of 10000
            TargetBodyTypes: null,
            TargetCategories: null,
            StartDate: DateTime.UtcNow,
            EndDate: DateTime.UtcNow.AddDays(30));

        // Act
        var act = () => _sut.CreateCampaignAsync(user.Id, request);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Budget cannot exceed*");
    }

    #endregion

    #region Products Tests

    [Fact]
    public async Task GetProductsAsync_WithCategoryFilter_ReturnsFilteredProducts()
    {
        // Arrange
        var (user, retailer) = await SetupRetailerDataAsync();

        var topsProduct = TestDbContextFactory.CreateTestProduct(retailerId: retailer.Id, name: "T-Shirt");
        topsProduct.Category = "Tops";

        var bottomsProduct = TestDbContextFactory.CreateTestProduct(retailerId: retailer.Id, name: "Jeans");
        bottomsProduct.Category = "Bottoms";

        _context.Products.AddRange(topsProduct, bottomsProduct);
        await _context.SaveChangesAsync();

        var request = new RetailerProductsRequest { Category = "Tops" };

        // Act
        var (products, meta) = await _sut.GetProductsAsync(user.Id, request);

        // Assert
        products.Should().HaveCount(1);
        products.First().Category.Should().Be("Tops");
        meta.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task UpdateProductAsync_ValidProduct_UpdatesFields()
    {
        // Arrange
        var (user, retailer) = await SetupRetailerDataAsync();

        var product = TestDbContextFactory.CreateTestProduct(retailerId: retailer.Id);
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var request = new UpdateRetailerProductRequest(
            Name: "Updated Product",
            Description: "Updated description",
            Category: null,
            SubCategory: null,
            ProductUrl: null,
            Price: 149.99m,
            Currency: null,
            IsActive: null);

        // Act
        var result = await _sut.UpdateProductAsync(user.Id, product.Id, request);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Updated Product");
        result.Price.Should().Be(149.99m);
    }

    [Fact]
    public async Task UpdateProductAsync_WrongRetailer_ThrowsNotFoundException()
    {
        // Arrange
        var (user, _) = await SetupRetailerDataAsync();

        // Create a product belonging to a different retailer
        var otherRetailer = new Retailer
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Name = "Other Retailer",
            IsActive = true
        };
        _context.Retailers.Add(otherRetailer);

        var product = TestDbContextFactory.CreateTestProduct(retailerId: otherRetailer.Id);
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var request = new UpdateRetailerProductRequest(
            Name: "Hacked Name",
            Description: null,
            Category: null,
            SubCategory: null,
            ProductUrl: null,
            Price: null,
            Currency: null,
            IsActive: null);

        // Act
        var act = () => _sut.UpdateProductAsync(user.Id, product.Id, request);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion
}
