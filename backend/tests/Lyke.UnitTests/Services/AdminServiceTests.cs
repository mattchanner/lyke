using FluentAssertions;
using Lyke.Application.DTOs.Admin;
using Lyke.Application.Interfaces;
using Lyke.Application.Services;
using Lyke.Core.Entities;
using Lyke.Core.Enums;
using Lyke.Core.Exceptions;
using Lyke.Infrastructure.Data;
using Lyke.UnitTests.Fixtures;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;

namespace Lyke.UnitTests.Services;

public class AdminServiceTests : IDisposable
{
    private readonly LykeDbContext _context;
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<ILogger<AdminService>> _loggerMock;
    private readonly AdminService _sut;

    public AdminServiceTests()
    {
        _context = TestDbContextFactory.Create();
        _userManagerMock = MockUserManager.Create();
        _emailServiceMock = new Mock<IEmailService>();
        _loggerMock = new Mock<ILogger<AdminService>>();

        _sut = new AdminService(
            _context,
            _emailServiceMock.Object,
            _userManagerMock.Object,
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

    #region Post Moderation Tests

    [Fact]
    public async Task ModeratePostAsync_ApprovePendingPost_SetsPublishedStatus()
    {
        // Arrange
        await SeedBaseDataAsync();
        var user = TestDbContextFactory.CreateTestUser(userType: UserType.Creator);
        _context.Users.Add(user);

        var creator = TestDbContextFactory.CreateTestCreator(userId: user.Id);
        _context.Creators.Add(creator);

        var post = TestDbContextFactory.CreateTestPost(creatorId: creator.Id, status: PostStatus.PendingReview);
        _context.Posts.Add(post);
        await _context.SaveChangesAsync();

        _userManagerMock.Setup(x => x.FindByIdAsync(user.Id.ToString()))
            .ReturnsAsync(user);

        var adminUserId = Guid.NewGuid();
        var request = new ModeratePostRequest(Approve: true, RejectionReason: null);

        // Act
        var result = await _sut.ModeratePostAsync(adminUserId, post.Id, request);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(PostStatus.Published);
        result.ModeratedByUserId.Should().Be(adminUserId);
        result.ModeratedAt.Should().NotBeNull();

        var updatedPost = _context.Posts.First(p => p.Id == post.Id);
        updatedPost.Status.Should().Be(PostStatus.Published);
        updatedPost.PublishedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task ModeratePostAsync_RejectPendingPost_SetsRejectedStatusWithNotes()
    {
        // Arrange
        await SeedBaseDataAsync();
        var user = TestDbContextFactory.CreateTestUser(userType: UserType.Creator);
        _context.Users.Add(user);

        var creator = TestDbContextFactory.CreateTestCreator(userId: user.Id);
        _context.Creators.Add(creator);

        var post = TestDbContextFactory.CreateTestPost(creatorId: creator.Id, status: PostStatus.PendingReview);
        _context.Posts.Add(post);
        await _context.SaveChangesAsync();

        _userManagerMock.Setup(x => x.FindByIdAsync(user.Id.ToString()))
            .ReturnsAsync(user);

        var adminUserId = Guid.NewGuid();
        var request = new ModeratePostRequest(Approve: false, RejectionReason: "Inappropriate content");

        // Act
        var result = await _sut.ModeratePostAsync(adminUserId, post.Id, request);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(PostStatus.Rejected);
        result.ModerationNotes.Should().Be("Inappropriate content");
    }

    [Fact]
    public async Task ModeratePostAsync_PostNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var adminUserId = Guid.NewGuid();
        var request = new ModeratePostRequest(Approve: true, RejectionReason: null);

        // Act
        var act = () => _sut.ModeratePostAsync(adminUserId, Guid.NewGuid(), request);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task ModeratePostAsync_AlreadyPublished_ThrowsValidationException()
    {
        // Arrange
        await SeedBaseDataAsync();
        var creator = TestDbContextFactory.CreateTestCreator();
        _context.Creators.Add(creator);

        var post = TestDbContextFactory.CreateTestPost(creatorId: creator.Id, status: PostStatus.Published);
        _context.Posts.Add(post);
        await _context.SaveChangesAsync();

        var adminUserId = Guid.NewGuid();
        var request = new ModeratePostRequest(Approve: true, RejectionReason: null);

        // Act
        var act = () => _sut.ModeratePostAsync(adminUserId, post.Id, request);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Only posts pending review can be moderated*");
    }

    #endregion

    #region User Management Tests

    [Fact]
    public async Task SuspendUserAsync_ActiveUser_SetsInactiveAndSendsEmail()
    {
        // Arrange
        await SeedBaseDataAsync();
        var user = TestDbContextFactory.CreateTestUser(userType: UserType.Shopper, isActive: true);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var adminUserId = Guid.NewGuid();
        var request = new SuspendUserRequest("Violation of terms");

        // Act
        var result = await _sut.SuspendUserAsync(adminUserId, user.Id, request);

        // Assert
        result.Should().NotBeNull();
        result.IsActive.Should().BeFalse();
        result.SuspendedAt.Should().NotBeNull();
        result.SuspendedByUserId.Should().Be(adminUserId);
        result.SuspensionReason.Should().Be("Violation of terms");

        _emailServiceMock.Verify(x => x.SendAccountSuspensionNotificationAsync(
            user.Email!, "Violation of terms", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SuspendUserAsync_AlreadySuspended_ThrowsValidationException()
    {
        // Arrange
        await SeedBaseDataAsync();
        var user = TestDbContextFactory.CreateTestUser(userType: UserType.Shopper, isActive: false);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var adminUserId = Guid.NewGuid();
        var request = new SuspendUserRequest("Some reason");

        // Act
        var act = () => _sut.SuspendUserAsync(adminUserId, user.Id, request);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*already suspended*");
    }

    [Fact]
    public async Task UnsuspendUserAsync_SuspendedUser_RestoresActiveStatus()
    {
        // Arrange
        await SeedBaseDataAsync();
        var user = TestDbContextFactory.CreateTestUser(userType: UserType.Shopper, isActive: false);
        user.SuspendedAt = DateTime.UtcNow.AddDays(-1);
        user.SuspendedByUserId = Guid.NewGuid();
        user.SuspensionReason = "Previous violation";
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var adminUserId = Guid.NewGuid();

        // Act
        var result = await _sut.UnsuspendUserAsync(adminUserId, user.Id);

        // Assert
        result.Should().NotBeNull();
        result.IsActive.Should().BeTrue();
        result.SuspendedAt.Should().BeNull();
        result.SuspendedByUserId.Should().BeNull();
        result.SuspensionReason.Should().BeNull();
    }

    [Fact]
    public async Task GetUsersAsync_WithTypeFilter_ReturnsFilteredUsers()
    {
        // Arrange
        await SeedBaseDataAsync();
        var shopper = TestDbContextFactory.CreateTestUser(userType: UserType.Shopper);
        var creator = TestDbContextFactory.CreateTestUser(userType: UserType.Creator);
        var retailer = TestDbContextFactory.CreateTestUser(userType: UserType.Retailer);
        _context.Users.AddRange(shopper, creator, retailer);
        await _context.SaveChangesAsync();

        var request = new UserListRequest(UserType: UserType.Creator);

        // Act
        var (users, meta) = await _sut.GetUsersAsync(request);

        // Assert
        users.Should().HaveCount(1);
        users.First().UserType.Should().Be(UserType.Creator);
        meta.TotalCount.Should().Be(1);
    }

    #endregion

    #region Platform Stats Tests

    [Fact]
    public async Task GetPlatformStatsAsync_WithData_ReturnsCorrectCounts()
    {
        // Arrange
        await SeedBaseDataAsync();

        var shopper = TestDbContextFactory.CreateTestUser(userType: UserType.Shopper);
        var creatorUser = TestDbContextFactory.CreateTestUser(userType: UserType.Creator);
        _context.Users.AddRange(shopper, creatorUser);

        var creator = TestDbContextFactory.CreateTestCreator(userId: creatorUser.Id);
        _context.Creators.Add(creator);

        var draftPost = TestDbContextFactory.CreateTestPost(creatorId: creator.Id, status: PostStatus.Draft);
        var publishedPost = TestDbContextFactory.CreateTestPost(creatorId: creator.Id, status: PostStatus.Published);
        var pendingPost = TestDbContextFactory.CreateTestPost(creatorId: creator.Id, status: PostStatus.PendingReview);
        _context.Posts.AddRange(draftPost, publishedPost, pendingPost);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetPlatformStatsAsync();

        // Assert
        result.Should().NotBeNull();
        result.Users.TotalUsers.Should().BeGreaterThanOrEqualTo(2);
        result.Users.Shoppers.Should().BeGreaterThanOrEqualTo(1);
        result.Users.Creators.Should().BeGreaterThanOrEqualTo(1);
        result.Content.TotalPosts.Should().BeGreaterThanOrEqualTo(3);
        result.Content.PublishedPosts.Should().BeGreaterThanOrEqualTo(1);
        result.Content.PendingReviewPosts.Should().BeGreaterThanOrEqualTo(1);
        result.Content.DraftPosts.Should().BeGreaterThanOrEqualTo(1);
    }

    #endregion

    #region Bulk Actions Tests

    [Fact]
    public async Task BulkModeratePostsAsync_MixedResults_ReportsCorrectCounts()
    {
        // Arrange
        await SeedBaseDataAsync();
        var creator = TestDbContextFactory.CreateTestCreator();
        _context.Creators.Add(creator);

        var pendingPost = TestDbContextFactory.CreateTestPost(creatorId: creator.Id, status: PostStatus.PendingReview);
        var publishedPost = TestDbContextFactory.CreateTestPost(creatorId: creator.Id, status: PostStatus.Published);
        _context.Posts.AddRange(pendingPost, publishedPost);
        await _context.SaveChangesAsync();

        var nonExistentId = Guid.NewGuid();
        var adminUserId = Guid.NewGuid();
        var request = new BulkModeratePostsRequest(
            PostIds: new List<Guid> { pendingPost.Id, publishedPost.Id, nonExistentId },
            Action: BulkPostAction.Approve,
            Reason: null);

        // Act
        var result = await _sut.BulkModeratePostsAsync(adminUserId, request);

        // Assert
        result.Should().NotBeNull();
        result.SuccessCount.Should().Be(1); // Only the pending post
        result.FailureCount.Should().Be(2); // Published post (wrong status) + non-existent
        result.Errors.Should().HaveCount(2);
    }

    #endregion

    #region Content Reports Tests

    [Fact]
    public async Task ReviewContentReportAsync_DismissReport_SetsReviewedStatus()
    {
        // Arrange
        await SeedBaseDataAsync();
        var creator = TestDbContextFactory.CreateTestCreator();
        _context.Creators.Add(creator);

        var post = TestDbContextFactory.CreateTestPost(creatorId: creator.Id, status: PostStatus.Published);
        _context.Posts.Add(post);

        var report = new ContentReport
        {
            Id = Guid.NewGuid(),
            PostId = post.Id,
            ReportedByUserId = Guid.NewGuid(),
            Reason = ReportReason.Spam,
            Status = ReportStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
        _context.Set<ContentReport>().Add(report);
        await _context.SaveChangesAsync();

        var adminUserId = Guid.NewGuid();
        var request = new ReviewContentReportRequest(
            NewStatus: ReportStatus.Dismissed,
            ReviewNotes: "Not spam, legitimate content",
            PostAction: null);

        // Act
        var result = await _sut.ReviewContentReportAsync(adminUserId, report.Id, request);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(ReportStatus.Dismissed);
        result.ReviewedByUserId.Should().Be(adminUserId);
        result.ReviewedAt.Should().NotBeNull();
        result.ReviewNotes.Should().Be("Not spam, legitimate content");
    }

    #endregion

    #region Platform Analytics Tests

    [Fact]
    public async Task GetPlatformAnalyticsAsync_UsesSnapshotsForHistoricalData()
    {
        // Arrange
        await SeedBaseDataAsync();
        var yesterday = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));

        var snapshot = new DailyMetricSnapshot
        {
            Id = Guid.NewGuid(),
            Date = yesterday,
            Scope = "platform",
            NewUsers = 5,
            PostsPublished = 3,
            Views = 100,
            Likes = 20,
            Saves = 10,
            Clicks = 15,
            ComputedAt = DateTime.UtcNow
        };
        _context.Set<DailyMetricSnapshot>().Add(snapshot);
        await _context.SaveChangesAsync();

        var request = new AdminAnalyticsRequest(
            StartDate: DateTime.UtcNow.AddDays(-1),
            EndDate: DateTime.UtcNow);

        // Act
        var result = await _sut.GetPlatformAnalyticsAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.DailyMetrics.Should().NotBeEmpty();
        result.Summary.Should().NotBeNull();
        // Yesterday's snapshot should contribute to the summary
        result.Summary.NewUsers.Should().BeGreaterThanOrEqualTo(5);
        result.Summary.Views.Should().BeGreaterThanOrEqualTo(100);
    }

    #endregion
}
