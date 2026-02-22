using FluentAssertions;
using Lyke.Application.Configuration;
using Lyke.Application.DTOs.Feed;
using Lyke.Application.Services;
using Lyke.Core.Entities;
using Lyke.Core.Enums;
using Lyke.Core.Exceptions;
using Lyke.Infrastructure.Data;
using Lyke.UnitTests.Fixtures;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Lyke.UnitTests.Services;

public class FeedServiceTests : IDisposable
{
    private readonly LykeDbContext _context;
    private readonly IOptions<MatchingSettings> _matchingSettings;
    private readonly Mock<ILogger<FeedService>> _loggerMock;
    private readonly FeedService _sut;

    public FeedServiceTests()
    {
        _context = TestDbContextFactory.Create();
        _matchingSettings = Options.Create(new MatchingSettings
        {
            HeightWeight = 0.3,
            WeightWeight = 0.3,
            BodyTypeWeight = 0.4,
            HeightToleranceCm = 5,
            WeightToleranceKg = 5,
            MinimumSimilarityScore = 0.3,
            SponsoredBoostFactor = 1.2,
            RecencyDecayDays = 30
        });
        _loggerMock = new Mock<ILogger<FeedService>>();

        _sut = new FeedService(
            _context,
            _matchingSettings,
            Options.Create(new ModerationSettings()),
            _loggerMock.Object);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    private async Task<(User user, Creator creator, Retailer retailer, Product product, Post post, PostProduct postProduct)> SetupTestDataAsync()
    {
        await TestDbContextFactory.SeedTestDataAsync(_context);

        var user = TestDbContextFactory.CreateTestUser(userType: UserType.Creator);
        _context.Users.Add(user);

        var bodyProfile = TestDbContextFactory.CreateTestBodyProfile(userId: user.Id, bodyTypeId: 1);
        _context.BodyProfiles.Add(bodyProfile);

        var creator = TestDbContextFactory.CreateTestCreator(userId: user.Id);
        _context.Creators.Add(creator);

        var retailer = TestDbContextFactory.CreateTestRetailer();
        _context.Retailers.Add(retailer);

        var product = TestDbContextFactory.CreateTestProduct(retailerId: retailer.Id);
        product.Retailer = retailer;
        _context.Products.Add(product);

        var post = TestDbContextFactory.CreateTestPost(creatorId: creator.Id, status: PostStatus.Published);
        post.Creator = creator;
        _context.Posts.Add(post);

        var postProduct = TestDbContextFactory.CreateTestPostProduct(postId: post.Id, productId: product.Id);
        postProduct.Post = post;
        postProduct.Product = product;
        _context.PostProducts.Add(postProduct);

        await _context.SaveChangesAsync();

        return (user, creator, retailer, product, post, postProduct);
    }

    [Fact]
    public async Task GetFeedAsync_WithPublishedPosts_ReturnsPosts()
    {
        // Arrange
        var (user, _, _, _, _, _) = await SetupTestDataAsync();
        var request = new FeedRequest(1, 20, null, null, null, FeedSortBy.Recent);

        // Act
        var (posts, meta) = await _sut.GetFeedAsync(user.Id, request);

        // Assert
        posts.Should().NotBeNull();
        posts.Should().HaveCount(1);
        meta.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task GetFeedAsync_WithNoPosts_ReturnsEmptyList()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new FeedRequest(1, 20, null, null, null, FeedSortBy.Recent);

        // Act
        var (posts, meta) = await _sut.GetFeedAsync(userId, request);

        // Assert
        posts.Should().NotBeNull();
        posts.Should().BeEmpty();
        meta.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task GetFeedAsync_WithCategoryFilter_ReturnsFilteredPosts()
    {
        // Arrange
        var (user, _, _, _, _, _) = await SetupTestDataAsync();
        var request = new FeedRequest(1, 20, "Tops", null, null, FeedSortBy.Recent);

        // Act
        var (posts, meta) = await _sut.GetFeedAsync(user.Id, request);

        // Assert
        posts.Should().NotBeNull();
        posts.Should().HaveCount(1); // Product has category "Tops"
    }

    [Fact]
    public async Task GetFeedAsync_WithWrongCategoryFilter_ReturnsNoPosts()
    {
        // Arrange
        var (user, _, _, _, _, _) = await SetupTestDataAsync();
        var request = new FeedRequest(1, 20, "NonExistentCategory", null, null, FeedSortBy.Recent);

        // Act
        var (posts, meta) = await _sut.GetFeedAsync(user.Id, request);

        // Assert
        posts.Should().NotBeNull();
        posts.Should().BeEmpty();
    }

    [Fact]
    public async Task GetExploreFeedAsync_WithPublishedPosts_ReturnsPosts()
    {
        // Arrange
        await SetupTestDataAsync();
        var request = new FeedRequest(1, 20, null, null, null, FeedSortBy.Recent);

        // Act
        var (posts, meta) = await _sut.GetExploreFeedAsync(null, request);

        // Assert
        posts.Should().NotBeNull();
        posts.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetPostAsync_WithValidId_ReturnsPostDetail()
    {
        // Arrange
        var (user, _, _, _, post, _) = await SetupTestDataAsync();

        // Act
        var result = await _sut.GetPostAsync(post.Id, user.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(post.Id);
        result.Title.Should().Be(post.Title);
        result.Products.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetPostAsync_WithInvalidId_ThrowsNotFoundException()
    {
        // Arrange
        var nonexistentPostId = Guid.NewGuid();

        // Act
        var act = () => _sut.GetPostAsync(nonexistentPostId, null);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetPostAsync_WithDraftPost_ThrowsNotFoundException()
    {
        // Arrange
        await TestDbContextFactory.SeedTestDataAsync(_context);

        var user = TestDbContextFactory.CreateTestUser(userType: UserType.Creator);
        _context.Users.Add(user);

        var creator = TestDbContextFactory.CreateTestCreator(userId: user.Id);
        _context.Creators.Add(creator);

        var draftPost = TestDbContextFactory.CreateTestPost(creatorId: creator.Id, status: PostStatus.Draft);
        _context.Posts.Add(draftPost);
        await _context.SaveChangesAsync();

        // Act
        var act = () => _sut.GetPostAsync(draftPost.Id, null);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task EngageAsync_WithLike_CreatesEngagement()
    {
        // Arrange
        var (user, _, _, _, post, _) = await SetupTestDataAsync();
        var viewerUser = TestDbContextFactory.CreateTestUser();
        _context.Users.Add(viewerUser);
        await _context.SaveChangesAsync();

        var request = new EngageRequest(EngagementType.Like);

        // Act
        await _sut.EngageAsync(post.Id, viewerUser.Id, request);

        // Assert
        var engagement = _context.Engagements.FirstOrDefault(e =>
            e.PostId == post.Id &&
            e.UserId == viewerUser.Id &&
            e.Type == EngagementType.Like);

        engagement.Should().NotBeNull();
    }

    [Fact]
    public async Task EngageAsync_WithDuplicateLike_DoesNotCreateDuplicate()
    {
        // Arrange
        var (user, _, _, _, post, _) = await SetupTestDataAsync();
        var viewerUser = TestDbContextFactory.CreateTestUser();
        _context.Users.Add(viewerUser);

        var existingEngagement = new Engagement
        {
            Id = Guid.NewGuid(),
            PostId = post.Id,
            UserId = viewerUser.Id,
            Type = EngagementType.Like,
            CreatedAt = DateTime.UtcNow
        };
        _context.Engagements.Add(existingEngagement);
        await _context.SaveChangesAsync();

        var request = new EngageRequest(EngagementType.Like);

        // Act
        await _sut.EngageAsync(post.Id, viewerUser.Id, request);

        // Assert
        var engagementCount = _context.Engagements.Count(e =>
            e.PostId == post.Id &&
            e.UserId == viewerUser.Id &&
            e.Type == EngagementType.Like);

        engagementCount.Should().Be(1);
    }

    [Fact]
    public async Task EngageAsync_WithNonexistentPost_ThrowsNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var nonexistentPostId = Guid.NewGuid();
        var request = new EngageRequest(EngagementType.Like);

        // Act
        var act = () => _sut.EngageAsync(nonexistentPostId, userId, request);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task RemoveEngagementAsync_WithExistingEngagement_RemovesIt()
    {
        // Arrange
        var (user, _, _, _, post, _) = await SetupTestDataAsync();
        var viewerUser = TestDbContextFactory.CreateTestUser();
        _context.Users.Add(viewerUser);

        var engagement = new Engagement
        {
            Id = Guid.NewGuid(),
            PostId = post.Id,
            UserId = viewerUser.Id,
            Type = EngagementType.Like,
            CreatedAt = DateTime.UtcNow
        };
        _context.Engagements.Add(engagement);
        await _context.SaveChangesAsync();

        var request = new EngageRequest(EngagementType.Like);

        // Act
        await _sut.RemoveEngagementAsync(post.Id, viewerUser.Id, request);

        // Assert
        var removedEngagement = _context.Engagements.FirstOrDefault(e => e.Id == engagement.Id);
        removedEngagement.Should().BeNull();
    }

    [Fact]
    public async Task GetSavedPostsAsync_WithSavedPosts_ReturnsSavedPosts()
    {
        // Arrange
        var (user, _, _, _, post, _) = await SetupTestDataAsync();
        var viewerUser = TestDbContextFactory.CreateTestUser();
        _context.Users.Add(viewerUser);

        var saveEngagement = new Engagement
        {
            Id = Guid.NewGuid(),
            PostId = post.Id,
            UserId = viewerUser.Id,
            Type = EngagementType.Save,
            CreatedAt = DateTime.UtcNow
        };
        _context.Engagements.Add(saveEngagement);
        await _context.SaveChangesAsync();

        // Act
        var (posts, meta) = await _sut.GetSavedPostsAsync(viewerUser.Id, 1, 20);

        // Assert
        posts.Should().NotBeNull();
        posts.Should().HaveCount(1);
        posts.First().Id.Should().Be(post.Id);
    }

    [Fact]
    public async Task GetSimilarPostsAsync_WithExistingPost_ReturnsSimilarPosts()
    {
        // Arrange
        var (user, creator, retailer, product, post, _) = await SetupTestDataAsync();

        // Create another similar post
        var similarPost = TestDbContextFactory.CreateTestPost(creatorId: creator.Id, status: PostStatus.Published, title: "Similar Post");
        similarPost.Creator = creator;
        _context.Posts.Add(similarPost);

        var similarPostProduct = TestDbContextFactory.CreateTestPostProduct(postId: similarPost.Id, productId: product.Id);
        similarPostProduct.Post = similarPost;
        similarPostProduct.Product = product;
        _context.PostProducts.Add(similarPostProduct);

        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetSimilarPostsAsync(post.Id, user.Id, 10);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result.First().Id.Should().Be(similarPost.Id);
    }

    [Fact]
    public async Task SearchAsync_WithMatchingQuery_ReturnsResults()
    {
        // Arrange
        var (user, _, _, product, post, _) = await SetupTestDataAsync();
        var request = new SearchRequest("Test", 1, 20);

        // Act
        var result = await _sut.SearchAsync(request, user.Id);

        // Assert
        result.Should().NotBeNull();
        result.TotalResults.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task SearchAsync_WithNoMatches_ReturnsEmptyResults()
    {
        // Arrange
        await SetupTestDataAsync();
        var request = new SearchRequest("NonExistentSearchTerm12345", 1, 20);

        // Act
        var result = await _sut.SearchAsync(request, null);

        // Assert
        result.Should().NotBeNull();
        result.TotalResults.Should().Be(0);
    }
}
