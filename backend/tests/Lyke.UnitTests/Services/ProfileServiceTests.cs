using FluentAssertions;
using Lyke.Application.DTOs.Profile;
using Lyke.Application.Interfaces;
using Lyke.Application.Services;
using Lyke.Core.Entities;
using Lyke.Core.Enums;
using Lyke.Core.Exceptions;
using Lyke.Infrastructure.Data;
using Lyke.UnitTests.Fixtures;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;

namespace Lyke.UnitTests.Services;

public class ProfileServiceTests : IDisposable
{
    private readonly LykeDbContext _context;
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly Mock<IStorageService> _storageServiceMock;
    private readonly Mock<ILogger<ProfileService>> _loggerMock;
    private readonly ProfileService _sut;

    public ProfileServiceTests()
    {
        _context = TestDbContextFactory.Create();
        _userManagerMock = MockUserManager.Create();
        _storageServiceMock = new Mock<IStorageService>();
        _loggerMock = new Mock<ILogger<ProfileService>>();

        _sut = new ProfileService(
            _userManagerMock.Object,
            _context,
            _storageServiceMock.Object,
            new Mock<IEventTrackingService>().Object,
            new MemoryCache(new MemoryCacheOptions()),
            _loggerMock.Object);
    }

    public void Dispose()
    {
        _context.Dispose();
    }


    [Fact]
    public async Task GetBodyProfileAsync_WithExistingProfile_ReturnsProfile()
    {
        // Arrange
        var userId = Guid.NewGuid();
        await TestDbContextFactory.SeedTestDataAsync(_context);

        var bodyProfile = TestDbContextFactory.CreateTestBodyProfile(userId: userId, bodyTypeId: 1);
        _context.BodyProfiles.Add(bodyProfile);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetBodyProfileAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result!.HeightCm.Should().Be(bodyProfile.HeightCm);
        result.WeightKg.Should().Be(bodyProfile.WeightKg);
    }

    [Fact]
    public async Task GetBodyProfileAsync_WithNoProfile_ReturnsNull()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var result = await _sut.GetBodyProfileAsync(userId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateBodyProfileAsync_WithValidRequest_CreatesProfile()
    {
        // Arrange
        var userId = Guid.NewGuid();
        await TestDbContextFactory.SeedTestDataAsync(_context);

        var request = new CreateBodyProfileRequest(175, 70m, 1, null, null, null, new List<FitPreference> { FitPreference.Regular });

        // Act
        var result = await _sut.CreateBodyProfileAsync(userId, request);

        // Assert
        result.Should().NotBeNull();
        result.HeightCm.Should().Be(175);
        result.WeightKg.Should().Be(70m);
        result.BodyTypeId.Should().Be(1);
        result.FitPreferences.Should().Contain(FitPreference.Regular);

        var savedProfile = await _context.BodyProfiles.FirstOrDefaultAsync(bp => bp.UserId == userId);
        savedProfile.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateBodyProfileAsync_WithExistingProfile_ThrowsValidationException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        await TestDbContextFactory.SeedTestDataAsync(_context);

        var existingProfile = TestDbContextFactory.CreateTestBodyProfile(userId: userId, bodyTypeId: 1);
        _context.BodyProfiles.Add(existingProfile);
        await _context.SaveChangesAsync();

        var request = new CreateBodyProfileRequest(175, 70m, 1, null, null, null, new List<FitPreference> { FitPreference.Regular });

        // Act
        var act = () => _sut.CreateBodyProfileAsync(userId, request);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*already exists*");
    }

    [Fact]
    public async Task CreateBodyProfileAsync_WithInvalidBodyType_ThrowsValidationException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        await TestDbContextFactory.SeedTestDataAsync(_context);

        var request = new CreateBodyProfileRequest(175, 70m, 999, null, null, null, new List<FitPreference> { FitPreference.Regular }); // Invalid body type

        // Act
        var act = () => _sut.CreateBodyProfileAsync(userId, request);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Invalid body type*");
    }

    [Fact]
    public async Task UpdateBodyProfileAsync_WithValidRequest_UpdatesProfile()
    {
        // Arrange
        var userId = Guid.NewGuid();
        await TestDbContextFactory.SeedTestDataAsync(_context);

        var existingProfile = TestDbContextFactory.CreateTestBodyProfile(userId: userId, heightCm: 170, weightKg: 65m, bodyTypeId: 1);
        _context.BodyProfiles.Add(existingProfile);
        await _context.SaveChangesAsync();

        var request = new UpdateBodyProfileRequest(180, 75m, null, null, null, null, new List<FitPreference> { FitPreference.Fitted });

        // Act
        var result = await _sut.UpdateBodyProfileAsync(userId, request);

        // Assert
        result.Should().NotBeNull();
        result.HeightCm.Should().Be(180);
        result.WeightKg.Should().Be(75m);
        result.FitPreferences.Should().Contain(FitPreference.Fitted);
    }

    [Fact]
    public async Task UpdateBodyProfileAsync_WithNonexistentProfile_ThrowsNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new UpdateBodyProfileRequest(180, 75m, null, null, null, null, new List<FitPreference> { FitPreference.Fitted });

        // Act
        var act = () => _sut.UpdateBodyProfileAsync(userId, request);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task DeleteBodyProfileAsync_WithExistingProfile_DeletesProfile()
    {
        // Arrange
        var userId = Guid.NewGuid();
        await TestDbContextFactory.SeedTestDataAsync(_context);

        var existingProfile = TestDbContextFactory.CreateTestBodyProfile(userId: userId, bodyTypeId: 1);
        _context.BodyProfiles.Add(existingProfile);
        await _context.SaveChangesAsync();

        // Act
        await _sut.DeleteBodyProfileAsync(userId);

        // Assert
        var deletedProfile = await _context.BodyProfiles.FirstOrDefaultAsync(bp => bp.UserId == userId);
        deletedProfile.Should().BeNull();
    }

    [Fact]
    public async Task DeleteBodyProfileAsync_WithNonexistentProfile_ThrowsNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var act = () => _sut.DeleteBodyProfileAsync(userId);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetBodyTypesAsync_ReturnsAllBodyTypes()
    {
        // Arrange
        await TestDbContextFactory.SeedTestDataAsync(_context);

        // Act
        var result = await _sut.GetBodyTypesAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCountGreaterThan(1);
        result.Should().BeInAscendingOrder(bt => bt.DisplayOrder);
    }

    [Fact]
    public async Task GetFitPreferencesAsync_ReturnsAllFitPreferences()
    {
        // Act
        var result = await _sut.GetFitPreferencesAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.Should().Contain(fp => fp.Name == "Fitted");
        result.Should().Contain(fp => fp.Name == "Regular");
        result.Should().Contain(fp => fp.Name == "Relaxed");
    }

    [Fact]
    public async Task GetAnonymizedBodyProfileAsync_WithExistingProfile_ReturnsAnonymizedProfile()
    {
        // Arrange
        var userId = Guid.NewGuid();
        await TestDbContextFactory.SeedTestDataAsync(_context);

        var bodyProfile = TestDbContextFactory.CreateTestBodyProfile(userId: userId, heightCm: 175, weightKg: 70m, bodyTypeId: 1);
        _context.BodyProfiles.Add(bodyProfile);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetAnonymizedBodyProfileAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result!.HeightRange.Should().Contain("175-179cm"); // Range check
        result.WeightRange.Should().Contain("70-74kg"); // Range check
        result.BodyTypeName.Should().Be("Hourglass"); // ID 1 is now Hourglass
    }
}
