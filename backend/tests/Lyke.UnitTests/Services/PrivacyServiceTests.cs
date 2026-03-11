using FluentAssertions;
using Lyke.Application.Configuration;
using Lyke.Application.DTOs.Privacy;
using Lyke.Application.Interfaces;
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

public class PrivacyServiceTests : IDisposable
{
    private readonly LykeDbContext _context;
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly IOptions<PrivacySettings> _privacySettings;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<ILogger<PrivacyService>> _loggerMock;
    private readonly PrivacyService _sut;

    public PrivacyServiceTests()
    {
        _context = TestDbContextFactory.Create();
        _userManagerMock = MockUserManager.Create();
        _privacySettings = Options.Create(new PrivacySettings { CurrentPolicyVersion = "2.0" });
        _emailServiceMock = new Mock<IEmailService>();
        _loggerMock = new Mock<ILogger<PrivacyService>>();

        _sut = new PrivacyService(
            _userManagerMock.Object,
            _context,
            _privacySettings,
            _emailServiceMock.Object,
            _loggerMock.Object
        );
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    private async Task SeedBaseDataAsync()
    {
        await TestDbContextFactory.SeedTestDataAsync(_context);
    }

    #region Export Tests

    [Fact]
    public async Task ExportUserDataAsync_BasicUser_ReturnsUserData()
    {
        // Arrange
        await SeedBaseDataAsync();
        var user = TestDbContextFactory.CreateTestUser(userType: UserType.Shopper);
        user.Email = "shopper@test.com";
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _userManagerMock.Setup(x => x.FindByIdAsync(user.Id.ToString())).ReturnsAsync(user);

        // Act
        var result = await _sut.ExportUserDataAsync(user.Id);

        // Assert
        result.Should().NotBeNull();
        result.User.Should().NotBeNull();
        result.User.Id.Should().Be(user.Id);
        result.User.Email.Should().Be("shopper@test.com");
        result.Creator.Should().BeNull();
        result.Posts.Should().BeEmpty();
        result.ExportedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task ExportUserDataAsync_CreatorUser_ReturnsFullExport()
    {
        // Arrange
        await SeedBaseDataAsync();
        var user = TestDbContextFactory.CreateTestUser(userType: UserType.Creator);
        user.Email = "creator@test.com";
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
        _context.Posts.Add(post);

        var postProduct = TestDbContextFactory.CreateTestPostProduct(
            postId: post.Id,
            productId: product.Id
        );
        postProduct.Product = product;
        postProduct.Post = post;
        _context.PostProducts.Add(postProduct);
        await _context.SaveChangesAsync();

        _userManagerMock.Setup(x => x.FindByIdAsync(user.Id.ToString())).ReturnsAsync(user);

        // Act
        var result = await _sut.ExportUserDataAsync(user.Id);

        // Assert
        result.Should().NotBeNull();
        result.User.Id.Should().Be(user.Id);
        result.Creator.Should().NotBeNull();
        result.Creator!.DisplayName.Should().Be(creator.DisplayName);
        result.Posts.Should().HaveCount(1);
    }

    [Fact]
    public async Task ExportUserDataAsync_UserNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var nonExistentUserId = Guid.NewGuid();

        _userManagerMock
            .Setup(x => x.FindByIdAsync(nonExistentUserId.ToString()))
            .ReturnsAsync((User?)null);

        // Act
        var act = () => _sut.ExportUserDataAsync(nonExistentUserId);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region Consent Status Tests

    [Fact]
    public async Task GetConsentStatusAsync_CurrentPolicy_ReturnsNoReconsent()
    {
        // Arrange
        var user = TestDbContextFactory.CreateTestUser();
        user.PrivacyPolicyVersion = "2.0"; // Matches CurrentPolicyVersion
        user.PrivacyPolicyAcceptedAt = DateTime.UtcNow.AddDays(-30);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _userManagerMock.Setup(x => x.FindByIdAsync(user.Id.ToString())).ReturnsAsync(user);

        // Act
        var result = await _sut.GetConsentStatusAsync(user.Id);

        // Assert
        result.Should().NotBeNull();
        result.NeedsReconsent.Should().BeFalse();
        result.CurrentPolicyVersion.Should().Be("2.0");
        result.PrivacyPolicyVersion.Should().Be("2.0");
    }

    [Fact]
    public async Task GetConsentStatusAsync_OldPolicy_ReturnsNeedsReconsent()
    {
        // Arrange
        var user = TestDbContextFactory.CreateTestUser();
        user.PrivacyPolicyVersion = "1.0"; // Older than CurrentPolicyVersion "2.0"
        user.PrivacyPolicyAcceptedAt = DateTime.UtcNow.AddDays(-365);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _userManagerMock.Setup(x => x.FindByIdAsync(user.Id.ToString())).ReturnsAsync(user);

        // Act
        var result = await _sut.GetConsentStatusAsync(user.Id);

        // Assert
        result.Should().NotBeNull();
        result.NeedsReconsent.Should().BeTrue();
        result.PrivacyPolicyVersion.Should().Be("1.0");
        result.CurrentPolicyVersion.Should().Be("2.0");
    }

    [Fact]
    public async Task GetConsentStatusAsync_UserNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var nonExistentUserId = Guid.NewGuid();

        _userManagerMock
            .Setup(x => x.FindByIdAsync(nonExistentUserId.ToString()))
            .ReturnsAsync((User?)null);

        // Act
        var act = () => _sut.GetConsentStatusAsync(nonExistentUserId);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region Update Consent Tests

    [Fact]
    public async Task UpdateConsentAsync_AcceptPolicy_UpdatesVersionAndTimestamp()
    {
        // Arrange
        var user = TestDbContextFactory.CreateTestUser();
        user.PrivacyPolicyVersion = "1.0";
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _userManagerMock.Setup(x => x.FindByIdAsync(user.Id.ToString())).ReturnsAsync(user);
        _userManagerMock
            .Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync(IdentityResult.Success);

        var request = new UpdateConsentRequest(AcceptPrivacyPolicy: true);

        // Act
        await _sut.UpdateConsentAsync(user.Id, request);

        // Assert
        user.PrivacyPolicyVersion.Should().Be("2.0");
        user.PrivacyPolicyAcceptedAt.Should().NotBeNull();
        user.PrivacyPolicyAcceptedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task UpdateConsentAsync_MarketingOptIn_UpdatesPreference()
    {
        // Arrange
        var user = TestDbContextFactory.CreateTestUser();
        user.MarketingOptIn = false;
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _userManagerMock.Setup(x => x.FindByIdAsync(user.Id.ToString())).ReturnsAsync(user);
        _userManagerMock
            .Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync(IdentityResult.Success);

        var request = new UpdateConsentRequest(MarketingOptIn: true);

        // Act
        await _sut.UpdateConsentAsync(user.Id, request);

        // Assert
        user.MarketingOptIn.Should().BeTrue();
    }

    #endregion
}
