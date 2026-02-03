using Lyke.Application.DTOs.Profile;
using Lyke.Application.Interfaces;
using Lyke.Core.Entities;
using Lyke.Core.Enums;
using Lyke.Core.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Lyke.Application.Services;

public class ProfileService : IProfileService
{
    private readonly UserManager<User> _userManager;
    private readonly DbContext _dbContext;
    private readonly ILogger<ProfileService> _logger;

    public ProfileService(
        UserManager<User> userManager,
        DbContext dbContext,
        ILogger<ProfileService> logger)
    {
        _userManager = userManager;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<UserProfileResponse> GetProfileAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.Users
            .Include(u => u.BodyProfile)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
        {
            throw new NotFoundException(nameof(User), userId);
        }

        var completeness = CalculateProfileCompleteness(user);

        return new UserProfileResponse(
            Id: user.Id,
            Email: user.Email!,
            UserType: user.UserType,
            HasBodyProfile: user.BodyProfile != null,
            ProfileCompleteness: completeness,
            CreatedAt: user.CreatedAt
        );
    }

    public async Task<UserProfileResponse> UpdateProfileAsync(Guid userId, UpdateProfileRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.Users
            .Include(u => u.BodyProfile)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
        {
            throw new NotFoundException(nameof(User), userId);
        }

        if (!string.IsNullOrEmpty(request.Email) && request.Email != user.Email)
        {
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null && existingUser.Id != userId)
            {
                throw new ValidationException("Email", "Email is already in use");
            }

            user.Email = request.Email;
            user.UserName = request.Email;
            user.NormalizedEmail = request.Email.ToUpperInvariant();
            user.NormalizedUserName = request.Email.ToUpperInvariant();
        }

        await _userManager.UpdateAsync(user);

        _logger.LogInformation("User {UserId} profile updated", userId);

        return await GetProfileAsync(userId, cancellationToken);
    }

    public async Task<BodyProfileResponse?> GetBodyProfileAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var bodyProfiles = _dbContext.Set<BodyProfile>();

        var profile = await bodyProfiles
            .Include(bp => bp.BodyType)
            .FirstOrDefaultAsync(bp => bp.UserId == userId, cancellationToken);

        if (profile == null)
        {
            return null;
        }

        return MapToBodyProfileResponse(profile);
    }

    public async Task<BodyProfileResponse> CreateBodyProfileAsync(Guid userId, CreateBodyProfileRequest request, CancellationToken cancellationToken = default)
    {
        var bodyProfiles = _dbContext.Set<BodyProfile>();
        var bodyTypes = _dbContext.Set<BodyType>();

        // Check if profile already exists
        var existingProfile = await bodyProfiles.FirstOrDefaultAsync(bp => bp.UserId == userId, cancellationToken);
        if (existingProfile != null)
        {
            throw new ValidationException("BodyProfile", "Body profile already exists. Use update instead.");
        }

        // Validate body type exists
        var bodyType = await bodyTypes.FindAsync(new object[] { request.BodyTypeId }, cancellationToken);
        if (bodyType == null)
        {
            throw new ValidationException("BodyTypeId", "Invalid body type");
        }

        var profile = new BodyProfile
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            HeightCm = request.HeightCm,
            WeightKg = request.WeightKg,
            BodyTypeId = request.BodyTypeId,
            FitPreference = request.FitPreference
        };

        await bodyProfiles.AddAsync(profile, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Body profile created for user {UserId}", userId);

        // Reload with body type
        profile.BodyType = bodyType;
        return MapToBodyProfileResponse(profile);
    }

    public async Task<BodyProfileResponse> UpdateBodyProfileAsync(Guid userId, UpdateBodyProfileRequest request, CancellationToken cancellationToken = default)
    {
        var bodyProfiles = _dbContext.Set<BodyProfile>();
        var bodyTypes = _dbContext.Set<BodyType>();

        var profile = await bodyProfiles
            .Include(bp => bp.BodyType)
            .FirstOrDefaultAsync(bp => bp.UserId == userId, cancellationToken);

        if (profile == null)
        {
            throw new NotFoundException(nameof(BodyProfile), userId);
        }

        if (request.HeightCm.HasValue)
        {
            profile.HeightCm = request.HeightCm.Value;
        }

        if (request.WeightKg.HasValue)
        {
            profile.WeightKg = request.WeightKg.Value;
        }

        if (request.BodyTypeId.HasValue)
        {
            var bodyType = await bodyTypes.FindAsync(new object[] { request.BodyTypeId.Value }, cancellationToken);
            if (bodyType == null)
            {
                throw new ValidationException("BodyTypeId", "Invalid body type");
            }
            profile.BodyTypeId = request.BodyTypeId.Value;
            profile.BodyType = bodyType;
        }

        if (request.FitPreference.HasValue)
        {
            profile.FitPreference = request.FitPreference.Value;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Body profile updated for user {UserId}", userId);

        return MapToBodyProfileResponse(profile);
    }

    public async Task DeleteBodyProfileAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var bodyProfiles = _dbContext.Set<BodyProfile>();

        var profile = await bodyProfiles.FirstOrDefaultAsync(bp => bp.UserId == userId, cancellationToken);

        if (profile == null)
        {
            throw new NotFoundException(nameof(BodyProfile), userId);
        }

        bodyProfiles.Remove(profile);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Body profile deleted for user {UserId}", userId);
    }

    public async Task<AnonymizedBodyProfileResponse?> GetAnonymizedBodyProfileAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var bodyProfiles = _dbContext.Set<BodyProfile>();

        var profile = await bodyProfiles
            .Include(bp => bp.BodyType)
            .FirstOrDefaultAsync(bp => bp.UserId == userId, cancellationToken);

        if (profile == null)
        {
            return null;
        }

        return new AnonymizedBodyProfileResponse(
            HeightRange: GetHeightRange(profile.HeightCm),
            WeightRange: GetWeightRange(profile.WeightKg),
            BodyTypeName: profile.BodyType.Name,
            FitPreference: profile.FitPreference
        );
    }

    public async Task<IReadOnlyList<BodyTypeResponse>> GetBodyTypesAsync(CancellationToken cancellationToken = default)
    {
        var bodyTypes = _dbContext.Set<BodyType>();

        var types = await bodyTypes
            .OrderBy(bt => bt.DisplayOrder)
            .Select(bt => new BodyTypeResponse(
                bt.Id,
                bt.Name,
                bt.Description,
                bt.DisplayOrder
            ))
            .ToListAsync(cancellationToken);

        return types;
    }

    public Task<IReadOnlyList<FitPreferenceResponse>> GetFitPreferencesAsync(CancellationToken cancellationToken = default)
    {
        var preferences = new List<FitPreferenceResponse>
        {
            new((int)FitPreference.Fitted, "Fitted", "Prefer clothes that fit close to the body"),
            new((int)FitPreference.Regular, "Regular", "Prefer standard fit clothes"),
            new((int)FitPreference.Relaxed, "Relaxed", "Prefer loose, comfortable fit clothes")
        };

        return Task.FromResult<IReadOnlyList<FitPreferenceResponse>>(preferences);
    }

    private static BodyProfileResponse MapToBodyProfileResponse(BodyProfile profile)
    {
        return new BodyProfileResponse(
            Id: profile.Id,
            HeightCm: profile.HeightCm,
            HeightDisplay: FormatHeight(profile.HeightCm),
            WeightKg: profile.WeightKg,
            WeightDisplay: FormatWeight(profile.WeightKg),
            BodyTypeId: profile.BodyTypeId,
            BodyTypeName: profile.BodyType.Name,
            FitPreference: profile.FitPreference,
            CreatedAt: profile.CreatedAt,
            UpdatedAt: profile.UpdatedAt
        );
    }

    private static int CalculateProfileCompleteness(User user)
    {
        var score = 0;
        var total = 4;

        // Email verified (assuming it's always set for now)
        if (!string.IsNullOrEmpty(user.Email)) score++;

        // Has body profile
        if (user.BodyProfile != null)
        {
            score++;

            // Has fit preference
            if (user.BodyProfile.FitPreference.HasValue) score++;
        }

        // Account is active
        if (user.IsActive) score++;

        return (int)Math.Round((double)score / total * 100);
    }

    private static string FormatHeight(int heightCm)
    {
        var feet = heightCm / 30.48;
        var totalInches = heightCm / 2.54;
        var remainingInches = (int)(totalInches % 12);
        var feetPart = (int)(totalInches / 12);

        return $"{heightCm}cm ({feetPart}'{remainingInches}\")";
    }

    private static string FormatWeight(decimal weightKg)
    {
        var lbs = weightKg * 2.20462m;
        return $"{weightKg:F1}kg ({lbs:F0}lbs)";
    }

    private static string GetHeightRange(int heightCm)
    {
        return heightCm switch
        {
            < 155 => "Under 155cm (5'1\")",
            < 160 => "155-159cm (5'1\"-5'2\")",
            < 165 => "160-164cm (5'3\"-5'4\")",
            < 170 => "165-169cm (5'5\"-5'6\")",
            < 175 => "170-174cm (5'7\"-5'8\")",
            < 180 => "175-179cm (5'9\"-5'10\")",
            < 185 => "180-184cm (5'11\"-6'0\")",
            < 190 => "185-189cm (6'1\"-6'2\")",
            _ => "190cm+ (6'3\"+)"
        };
    }

    private static string GetWeightRange(decimal weightKg)
    {
        return weightKg switch
        {
            < 50 => "Under 50kg (110lbs)",
            < 55 => "50-54kg (110-121lbs)",
            < 60 => "55-59kg (121-130lbs)",
            < 65 => "60-64kg (132-143lbs)",
            < 70 => "65-69kg (143-152lbs)",
            < 75 => "70-74kg (154-163lbs)",
            < 80 => "75-79kg (165-174lbs)",
            < 85 => "80-84kg (176-185lbs)",
            < 90 => "85-89kg (187-196lbs)",
            < 95 => "90-94kg (198-207lbs)",
            < 100 => "95-99kg (209-218lbs)",
            _ => "100kg+ (220lbs+)"
        };
    }
}
