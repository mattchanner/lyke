using FluentAssertions;
using Lyke.Application.Configuration;
using Lyke.Application.DTOs.Creator;
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

public class CreatorServiceTests : IDisposable
{
    private readonly LykeDbContext _context;
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly IOptions<CreatorSettings> _creatorSettings;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<ILogger<CreatorService>> _loggerMock;
    private readonly CreatorService _sut;

    public CreatorServiceTests()
    {
        _context = TestDbContextFactory.Create();
        _userManagerMock = MockUserManager.Create();
        _creatorSettings = Options.Create(new CreatorSettings
        {
            MaxDraftPosts = 10,
            MaxMediaPerPost = 10,
            MaxProductsPerPost = 20,
            MinPayoutThreshold = 50.00m,
            DefaultCurrency = "GBP"
        });
        _emailServiceMock = new Mock<IEmailService>();
        _loggerMock = new Mock<ILogger<CreatorService>>();

        _sut = new CreatorService(
            _context,
            _userManagerMock.Object,
            _creatorSettings,
            _emailServiceMock.Object,
            _loggerMock.Object);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    private async Task<(User user, Retailer retailer, Product product)> SetupBaseDataAsync()
    {
        await TestDbContextFactory.SeedTestDataAsync(_context);

        var user = TestDbContextFactory.CreateTestUser(userType: UserType.Shopper);
        _context.Users.Add(user);

        var retailer = TestDbContextFactory.CreateTestRetailer();
        _context.Retailers.Add(retailer);

        var product = TestDbContextFactory.CreateTestProduct(retailerId: retailer.Id);
        product.Retailer = retailer;
        _context.Products.Add(product);

        await _context.SaveChangesAsync();

        return (user, retailer, product);
    }

    private async Task<(User user, Creator creator, Retailer retailer, Product product)> SetupCreatorDataAsync()
    {
        var (user, retailer, product) = await SetupBaseDataAsync();

        user.UserType = UserType.Creator;
        var creator = TestDbContextFactory.CreateTestCreator(userId: user.Id);
        _context.Creators.Add(creator);
        await _context.SaveChangesAsync();

        return (user, creator, retailer, product);
    }

    #region Registration Tests

    [Fact]
    public async Task RegisterAsCreatorAsync_WithValidRequest_CreatesCreator()
    {
        // Arrange
        var (user, _, _) = await SetupBaseDataAsync();
        var request = new RegisterCreatorRequest("Test Creator", "Test bio", null);

        _userManagerMock.Setup(x => x.FindByIdAsync(user.Id.ToString()))
            .ReturnsAsync(user);

        _userManagerMock.Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _sut.RegisterAsCreatorAsync(user.Id, request);

        // Assert
        result.Should().NotBeNull();
        result.DisplayName.Should().Be("Test Creator");
        result.Bio.Should().Be("Test bio");
        result.IsVerified.Should().BeFalse();

        var creator = _context.Creators.FirstOrDefault(c => c.UserId == user.Id);
        creator.Should().NotBeNull();
    }

    [Fact]
    public async Task RegisterAsCreatorAsync_WithExistingCreator_ThrowsValidationException()
    {
        // Arrange
        var (user, creator, _, _) = await SetupCreatorDataAsync();
        var request = new RegisterCreatorRequest("Another Creator", null, null);

        _userManagerMock.Setup(x => x.FindByIdAsync(user.Id.ToString()))
            .ReturnsAsync(user);

        // Act
        var act = () => _sut.RegisterAsCreatorAsync(user.Id, request);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*already registered as a creator*");
    }

    [Fact]
    public async Task RegisterAsCreatorAsync_WithNonexistentUser_ThrowsNotFoundException()
    {
        // Arrange
        var nonexistentUserId = Guid.NewGuid();
        var request = new RegisterCreatorRequest("Test Creator", null, null);

        _userManagerMock.Setup(x => x.FindByIdAsync(nonexistentUserId.ToString()))
            .ReturnsAsync((User?)null);

        // Act
        var act = () => _sut.RegisterAsCreatorAsync(nonexistentUserId, request);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region Profile Tests

    [Fact]
    public async Task GetCreatorProfileAsync_WithValidCreator_ReturnsProfile()
    {
        // Arrange
        var (user, creator, _, _) = await SetupCreatorDataAsync();

        // Act
        var result = await _sut.GetCreatorProfileAsync(user.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(creator.Id);
        result.DisplayName.Should().Be(creator.DisplayName);
    }

    [Fact]
    public async Task GetCreatorProfileAsync_WithNonCreator_ThrowsNotFoundException()
    {
        // Arrange
        var (user, _, _) = await SetupBaseDataAsync();

        // Act
        var act = () => _sut.GetCreatorProfileAsync(user.Id);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateCreatorProfileAsync_WithValidRequest_UpdatesProfile()
    {
        // Arrange
        var (user, creator, _, _) = await SetupCreatorDataAsync();
        var request = new UpdateCreatorProfileRequest("Updated Name", "Updated bio", null);

        // Act
        var result = await _sut.UpdateCreatorProfileAsync(user.Id, request);

        // Assert
        result.Should().NotBeNull();
        result.DisplayName.Should().Be("Updated Name");
        result.Bio.Should().Be("Updated bio");
    }

    #endregion

    #region Post Management Tests

    [Fact]
    public async Task CreatePostAsync_WithValidRequest_CreatesPost()
    {
        // Arrange
        var (user, creator, _, product) = await SetupCreatorDataAsync();
        var request = new CreatePostRequest(
            "Test Post",
            "Test description",
            MediaType.Image,
            new List<string> { "https://example.com/image1.jpg" },
            null,
            new List<PostProductRequest>
            {
                new(product.Id, "M", FitRating.TrueToSize, "Fits well", null, null)
            }
        );

        // Act
        var result = await _sut.CreatePostAsync(user.Id, request);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("Test Post");
        result.Status.Should().Be(PostStatus.Draft);
        result.Products.Should().HaveCount(1);

        var post = _context.Posts.FirstOrDefault(p => p.Id == result.Id);
        post.Should().NotBeNull();
    }

    [Fact]
    public async Task CreatePostAsync_WithTooManyDrafts_ThrowsValidationException()
    {
        // Arrange
        var (user, creator, _, product) = await SetupCreatorDataAsync();

        // Create max draft posts
        for (int i = 0; i < 10; i++)
        {
            var post = TestDbContextFactory.CreateTestPost(creatorId: creator.Id, status: PostStatus.Draft, title: $"Draft {i}");
            _context.Posts.Add(post);
        }
        await _context.SaveChangesAsync();

        var request = new CreatePostRequest(
            "Another Post",
            "Description",
            MediaType.Image,
            new List<string> { "https://example.com/image.jpg" },
            null,
            new List<PostProductRequest>
            {
                new(product.Id, "M", null, null, null, null)
            }
        );

        // Act
        var act = () => _sut.CreatePostAsync(user.Id, request);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Maximum draft posts*");
    }

    [Fact]
    public async Task CreatePostAsync_WithInvalidProduct_ThrowsValidationException()
    {
        // Arrange
        var (user, creator, _, _) = await SetupCreatorDataAsync();
        var request = new CreatePostRequest(
            "Test Post",
            "Description",
            MediaType.Image,
            new List<string> { "https://example.com/image.jpg" },
            null,
            new List<PostProductRequest>
            {
                new(Guid.NewGuid(), "M", null, null, null, null) // Invalid product ID
            }
        );

        // Act
        var act = () => _sut.CreatePostAsync(user.Id, request);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*products are invalid*");
    }

    [Fact]
    public async Task UpdatePostAsync_WithDraftPost_UpdatesPost()
    {
        // Arrange
        var (user, creator, _, product) = await SetupCreatorDataAsync();

        var post = TestDbContextFactory.CreateTestPost(creatorId: creator.Id, status: PostStatus.Draft);
        _context.Posts.Add(post);

        var postProduct = TestDbContextFactory.CreateTestPostProduct(postId: post.Id, productId: product.Id);
        postProduct.Product = product;
        _context.PostProducts.Add(postProduct);
        await _context.SaveChangesAsync();

        var request = new UpdatePostRequest("Updated Title", "Updated description", null, null, null, null);

        // Act
        var result = await _sut.UpdatePostAsync(user.Id, post.Id, request);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("Updated Title");
        result.Description.Should().Be("Updated description");
    }

    [Fact]
    public async Task UpdatePostAsync_WithNonDraftPost_ThrowsValidationException()
    {
        // Arrange
        var (user, creator, _, product) = await SetupCreatorDataAsync();

        var post = TestDbContextFactory.CreateTestPost(creatorId: creator.Id, status: PostStatus.Published);
        _context.Posts.Add(post);
        await _context.SaveChangesAsync();

        var request = new UpdatePostRequest("Updated Title", null, null, null, null, null);

        // Act
        var act = () => _sut.UpdatePostAsync(user.Id, post.Id, request);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Only draft posts can be edited*");
    }

    [Fact]
    public async Task DeletePostAsync_WithValidPost_DeletesPost()
    {
        // Arrange
        var (user, creator, _, _) = await SetupCreatorDataAsync();

        var post = TestDbContextFactory.CreateTestPost(creatorId: creator.Id, status: PostStatus.Draft);
        _context.Posts.Add(post);
        await _context.SaveChangesAsync();

        // Act
        await _sut.DeletePostAsync(user.Id, post.Id);

        // Assert
        var deletedPost = _context.Posts.FirstOrDefault(p => p.Id == post.Id);
        deletedPost.Should().BeNull();
    }

    [Fact]
    public async Task DeletePostAsync_WithNonexistentPost_ThrowsNotFoundException()
    {
        // Arrange
        var (user, creator, _, _) = await SetupCreatorDataAsync();

        // Act
        var act = () => _sut.DeletePostAsync(user.Id, Guid.NewGuid());

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetPostAsync_WithValidPost_ReturnsPost()
    {
        // Arrange
        var (user, creator, _, product) = await SetupCreatorDataAsync();

        var post = TestDbContextFactory.CreateTestPost(creatorId: creator.Id, status: PostStatus.Draft);
        _context.Posts.Add(post);

        var postProduct = TestDbContextFactory.CreateTestPostProduct(postId: post.Id, productId: product.Id);
        postProduct.Product = product;
        _context.PostProducts.Add(postProduct);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetPostAsync(user.Id, post.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(post.Id);
        result.Products.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetPostsAsync_WithStatusFilter_ReturnsFilteredPosts()
    {
        // Arrange
        var (user, creator, _, _) = await SetupCreatorDataAsync();

        var draftPost = TestDbContextFactory.CreateTestPost(creatorId: creator.Id, status: PostStatus.Draft);
        var publishedPost = TestDbContextFactory.CreateTestPost(creatorId: creator.Id, status: PostStatus.Published);
        _context.Posts.AddRange(draftPost, publishedPost);
        await _context.SaveChangesAsync();

        var request = new CreatorPostsRequest(PostStatus.Draft, 1, 20);

        // Act
        var (posts, meta) = await _sut.GetPostsAsync(user.Id, request);

        // Assert
        posts.Should().HaveCount(1);
        posts.First().Status.Should().Be(PostStatus.Draft);
    }

    [Fact]
    public async Task SubmitPostForReviewAsync_WithValidDraftPost_ChangesStatusToPendingReview()
    {
        // Arrange
        var (user, creator, _, product) = await SetupCreatorDataAsync();

        var post = TestDbContextFactory.CreateTestPost(creatorId: creator.Id, status: PostStatus.Draft);
        _context.Posts.Add(post);

        var postProduct = TestDbContextFactory.CreateTestPostProduct(postId: post.Id, productId: product.Id);
        postProduct.Product = product;
        _context.PostProducts.Add(postProduct);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.SubmitPostForReviewAsync(user.Id, post.Id);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(PostStatus.PendingReview);

        var updatedPost = _context.Posts.First(p => p.Id == post.Id);
        updatedPost.Status.Should().Be(PostStatus.PendingReview);
    }

    [Fact]
    public async Task SubmitPostForReviewAsync_WithNoMedia_ThrowsValidationException()
    {
        // Arrange
        var (user, creator, _, product) = await SetupCreatorDataAsync();

        var post = new Post
        {
            Id = Guid.NewGuid(),
            CreatorId = creator.Id,
            Title = "Test Post",
            MediaType = MediaType.Image,
            MediaUrls = "[]", // Empty media
            Status = PostStatus.Draft
        };
        _context.Posts.Add(post);

        var postProduct = TestDbContextFactory.CreateTestPostProduct(postId: post.Id, productId: product.Id);
        postProduct.Product = product;
        postProduct.Post = post;
        _context.PostProducts.Add(postProduct);
        await _context.SaveChangesAsync();

        // Act
        var act = () => _sut.SubmitPostForReviewAsync(user.Id, post.Id);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*at least one media item*");
    }

    [Fact]
    public async Task SubmitPostForReviewAsync_WithNoProducts_ThrowsValidationException()
    {
        // Arrange
        var (user, creator, _, _) = await SetupCreatorDataAsync();

        var post = TestDbContextFactory.CreateTestPost(creatorId: creator.Id, status: PostStatus.Draft);
        _context.Posts.Add(post);
        await _context.SaveChangesAsync();

        // Act
        var act = () => _sut.SubmitPostForReviewAsync(user.Id, post.Id);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*at least one product*");
    }

    #endregion

    #region Analytics Tests

    [Fact]
    public async Task GetAnalyticsAsync_WithValidCreator_ReturnsAnalytics()
    {
        // Arrange
        var (user, creator, _, _) = await SetupCreatorDataAsync();

        var post = TestDbContextFactory.CreateTestPost(creatorId: creator.Id, status: PostStatus.Published);
        _context.Posts.Add(post);

        var engagement = new Engagement
        {
            Id = Guid.NewGuid(),
            PostId = post.Id,
            UserId = Guid.NewGuid(),
            Type = EngagementType.View,
            CreatedAt = DateTime.UtcNow
        };
        _context.Engagements.Add(engagement);
        await _context.SaveChangesAsync();

        var request = new CreatorAnalyticsRequest(DateTime.UtcNow.AddDays(-30), DateTime.UtcNow);

        // Act
        var result = await _sut.GetAnalyticsAsync(user.Id, request);

        // Assert
        result.Should().NotBeNull();
        result.Summary.TotalViews.Should().BeGreaterThanOrEqualTo(1);
    }

    #endregion

    #region Earnings Tests

    [Fact]
    public async Task GetEarningsSummaryAsync_WithValidCreator_ReturnsSummary()
    {
        // Arrange
        var (user, creator, _, product) = await SetupCreatorDataAsync();

        var post = TestDbContextFactory.CreateTestPost(creatorId: creator.Id, status: PostStatus.Published);
        _context.Posts.Add(post);

        var postProduct = TestDbContextFactory.CreateTestPostProduct(postId: post.Id, productId: product.Id);
        _context.PostProducts.Add(postProduct);

        var clickEvent = new ClickEvent
        {
            Id = Guid.NewGuid(),
            PostId = post.Id,
            PostProductId = postProduct.Id,
            CreatedAt = DateTime.UtcNow
        };
        _context.ClickEvents.Add(clickEvent);

        var earning = new CreatorEarning
        {
            Id = Guid.NewGuid(),
            CreatorId = creator.Id,
            ClickEventId = clickEvent.Id,
            EarningType = EarningType.Affiliate,
            Amount = 10.00m,
            Currency = "GBP",
            Status = EarningStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
        _context.CreatorEarnings.Add(earning);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetEarningsSummaryAsync(user.Id);

        // Assert
        result.Should().NotBeNull();
        result.TotalEarnings.Should().Be(10.00m);
        result.PendingEarnings.Should().Be(10.00m);
        result.Currency.Should().Be("GBP");
        result.EligibleForPayout.Should().BeFalse(); // Below threshold
    }

    [Fact]
    public async Task GetEarningsHistoryAsync_WithValidCreator_ReturnsHistory()
    {
        // Arrange
        var (user, creator, _, product) = await SetupCreatorDataAsync();

        var post = TestDbContextFactory.CreateTestPost(creatorId: creator.Id, status: PostStatus.Published);
        post.Creator = creator;
        _context.Posts.Add(post);

        var postProduct = TestDbContextFactory.CreateTestPostProduct(postId: post.Id, productId: product.Id);
        postProduct.Post = post;
        postProduct.Product = product;
        _context.PostProducts.Add(postProduct);

        var clickEvent = new ClickEvent
        {
            Id = Guid.NewGuid(),
            PostId = post.Id,
            PostProductId = postProduct.Id,
            CreatedAt = DateTime.UtcNow,
            Post = post,
            PostProduct = postProduct
        };
        _context.ClickEvents.Add(clickEvent);

        var earning = new CreatorEarning
        {
            Id = Guid.NewGuid(),
            CreatorId = creator.Id,
            ClickEventId = clickEvent.Id,
            EarningType = EarningType.Affiliate,
            Amount = 10.00m,
            Currency = "USD",
            Status = EarningStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            ClickEvent = clickEvent
        };
        _context.CreatorEarnings.Add(earning);
        await _context.SaveChangesAsync();

        var request = new EarningsHistoryRequest(null, null, null, 1, 20);

        // Act
        var (earnings, meta) = await _sut.GetEarningsHistoryAsync(user.Id, request);

        // Assert
        earnings.Should().HaveCount(1);
        earnings.First().Amount.Should().Be(10.00m);
        meta.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task GetEarningsHistoryAsync_WithStatusFilter_ReturnsFilteredEarnings()
    {
        // Arrange
        var (user, creator, _, product) = await SetupCreatorDataAsync();

        var post = TestDbContextFactory.CreateTestPost(creatorId: creator.Id, status: PostStatus.Published);
        post.Creator = creator;
        _context.Posts.Add(post);

        var postProduct = TestDbContextFactory.CreateTestPostProduct(postId: post.Id, productId: product.Id);
        postProduct.Post = post;
        postProduct.Product = product;
        _context.PostProducts.Add(postProduct);

        var clickEvent = new ClickEvent
        {
            Id = Guid.NewGuid(),
            PostId = post.Id,
            PostProductId = postProduct.Id,
            CreatedAt = DateTime.UtcNow,
            Post = post,
            PostProduct = postProduct
        };
        _context.ClickEvents.Add(clickEvent);

        var pendingEarning = new CreatorEarning
        {
            Id = Guid.NewGuid(),
            CreatorId = creator.Id,
            ClickEventId = clickEvent.Id,
            EarningType = EarningType.Affiliate,
            Amount = 10.00m,
            Currency = "USD",
            Status = EarningStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            ClickEvent = clickEvent
        };
        _context.CreatorEarnings.Add(pendingEarning);
        await _context.SaveChangesAsync();

        var request = new EarningsHistoryRequest(EarningStatus.Paid, null, null, 1, 20);

        // Act
        var (earnings, meta) = await _sut.GetEarningsHistoryAsync(user.Id, request);

        // Assert
        earnings.Should().BeEmpty();
        meta.TotalCount.Should().Be(0);
    }

    #endregion
}
