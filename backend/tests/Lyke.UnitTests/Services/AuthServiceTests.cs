using FluentAssertions;
using Lyke.Application.Configuration;
using Lyke.Application.DTOs.Auth;
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

public class AuthServiceTests : IDisposable
{
    private readonly LykeDbContext _context;
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly IOptions<JwtSettings> _jwtSettings;
    private readonly IOptions<PrivacySettings> _privacySettings;
    private readonly Mock<ISocialTokenValidator> _socialTokenValidatorMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<ILogger<AuthService>> _loggerMock;
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _context = TestDbContextFactory.Create();
        _userManagerMock = MockUserManager.Create();
        _jwtSettings = Options.Create(
            new JwtSettings
            {
                Secret = "ThisIsAVeryLongSecretKeyForTestingPurposes123!",
                Issuer = "TestIssuer",
                Audience = "TestAudience",
                ExpiryMinutes = 60,
                RefreshTokenExpiryDays = 30,
            }
        );
        _privacySettings = Options.Create(new PrivacySettings { CurrentPolicyVersion = "1.0" });
        _socialTokenValidatorMock = new Mock<ISocialTokenValidator>();
        _emailServiceMock = new Mock<IEmailService>();
        _loggerMock = new Mock<ILogger<AuthService>>();

        _sut = new AuthService(
            _userManagerMock.Object,
            _context,
            _jwtSettings,
            _privacySettings,
            _socialTokenValidatorMock.Object,
            _emailServiceMock.Object,
            _loggerMock.Object
        );
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task RegisterAsync_WithValidRequest_CreatesUserAndReturnsTokens()
    {
        // Arrange
        var request = new RegisterRequest(
            "newuser@test.com",
            "Password123!",
            "Password123!",
            UserType.Shopper
        );

        _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email)).ReturnsAsync((User?)null);

        _userManagerMock
            .Setup(x => x.CreateAsync(It.IsAny<User>(), request.Password))
            .ReturnsAsync(IdentityResult.Success);

        _userManagerMock
            .Setup(x => x.GenerateEmailConfirmationTokenAsync(It.IsAny<User>()))
            .ReturnsAsync("test-email-token");

        // Act
        var result = await _sut.RegisterAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Email.Should().Be(request.Email);
        result.UserType.Should().Be(UserType.Shopper);
        result.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task RegisterAsync_WithExistingEmail_ThrowsValidationException()
    {
        // Arrange
        var existingUser = TestDbContextFactory.CreateTestUser(email: "existing@test.com");
        var request = new RegisterRequest(
            "existing@test.com",
            "Password123!",
            "Password123!",
            UserType.Shopper
        );

        _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email)).ReturnsAsync(existingUser);

        // Act
        var act = () => _sut.RegisterAsync(request);

        // Assert
        await act.Should()
            .ThrowAsync<ValidationException>()
            .WithMessage("Email is already registered");
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsTokens()
    {
        // Arrange
        var user = TestDbContextFactory.CreateTestUser(email: "login@test.com");
        var request = new LoginRequest("login@test.com", "Password123!");

        _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email)).ReturnsAsync(user);

        _userManagerMock
            .Setup(x => x.CheckPasswordAsync(user, request.Password))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.LoginAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(user.Id);
        result.Email.Should().Be(user.Email);
        result.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task LoginAsync_WithInvalidEmail_ThrowsUnauthorizedException()
    {
        // Arrange
        var request = new LoginRequest("nonexistent@test.com", "Password123!");

        _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email)).ReturnsAsync((User?)null);

        // Act
        var act = () => _sut.LoginAsync(request);

        // Assert
        await act.Should()
            .ThrowAsync<UnauthorizedException>()
            .WithMessage("Invalid email or password");
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ThrowsUnauthorizedException()
    {
        // Arrange
        var user = TestDbContextFactory.CreateTestUser(email: "login@test.com");
        var request = new LoginRequest("login@test.com", "WrongPassword!");

        _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email)).ReturnsAsync(user);

        _userManagerMock
            .Setup(x => x.CheckPasswordAsync(user, request.Password))
            .ReturnsAsync(false);

        // Act
        var act = () => _sut.LoginAsync(request);

        // Assert
        await act.Should()
            .ThrowAsync<UnauthorizedException>()
            .WithMessage("Invalid email or password");
    }

    [Fact]
    public async Task LoginAsync_WithInactiveAccount_ThrowsUnauthorizedException()
    {
        // Arrange
        var user = TestDbContextFactory.CreateTestUser(email: "inactive@test.com", isActive: false);
        var request = new LoginRequest("inactive@test.com", "Password123!");

        _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email)).ReturnsAsync(user);

        // Act
        var act = () => _sut.LoginAsync(request);

        // Assert
        await act.Should()
            .ThrowAsync<UnauthorizedException>()
            .WithMessage("Account is deactivated");
    }

    [Fact]
    public async Task RefreshTokenAsync_WithValidToken_ReturnsNewTokens()
    {
        // Arrange
        var user = TestDbContextFactory.CreateTestUser(email: "refresh@test.com");
        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = "valid-refresh-token",
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow,
            User = user,
        };

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        var request = new RefreshTokenRequest("valid-refresh-token");

        // Act
        var result = await _sut.RefreshTokenAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(user.Id);
        result.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBe(refreshToken.Token);
    }

    [Fact]
    public async Task RefreshTokenAsync_WithInvalidToken_ThrowsUnauthorizedException()
    {
        // Arrange
        var request = new RefreshTokenRequest("invalid-token");

        // Act
        var act = () => _sut.RefreshTokenAsync(request);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>().WithMessage("Invalid refresh token");
    }

    [Fact]
    public async Task RefreshTokenAsync_WithExpiredToken_ThrowsUnauthorizedException()
    {
        // Arrange
        var user = TestDbContextFactory.CreateTestUser();
        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = "expired-token",
            ExpiresAt = DateTime.UtcNow.AddDays(-1), // Expired
            CreatedAt = DateTime.UtcNow.AddDays(-31),
            User = user,
        };

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        var request = new RefreshTokenRequest("expired-token");

        // Act
        var act = () => _sut.RefreshTokenAsync(request);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>().WithMessage("Invalid refresh token");
    }

    [Fact]
    public async Task LogoutAsync_WithValidToken_RevokesToken()
    {
        // Arrange
        var user = TestDbContextFactory.CreateTestUser();
        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = "logout-token",
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow,
        };

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        // Act
        await _sut.LogoutAsync("logout-token");

        // Assert
        var token = await _context.RefreshTokens.FindAsync(refreshToken.Id);
        token.Should().NotBeNull();
        token!.RevokedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteAccountAsync_WithValidUser_DeactivatesAccount()
    {
        // Arrange
        var user = TestDbContextFactory.CreateTestUser(email: "delete@test.com");

        _userManagerMock.Setup(x => x.FindByIdAsync(user.Id.ToString())).ReturnsAsync(user);

        _userManagerMock
            .Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        await _sut.DeleteAccountAsync(user.Id);

        // Assert
        _userManagerMock.Verify(
            x =>
                x.UpdateAsync(
                    It.Is<User>(u => u.IsActive == false && u.Email!.StartsWith("deleted_"))
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task DeleteAccountAsync_WithNonexistentUser_ThrowsNotFoundException()
    {
        // Arrange
        var nonexistentUserId = Guid.NewGuid();

        _userManagerMock
            .Setup(x => x.FindByIdAsync(nonexistentUserId.ToString()))
            .ReturnsAsync((User?)null);

        // Act
        var act = () => _sut.DeleteAccountAsync(nonexistentUserId);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
