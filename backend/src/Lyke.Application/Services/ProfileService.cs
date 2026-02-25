using Lyke.Application.DTOs.Feed;
using Lyke.Application.DTOs.Profile;
using Lyke.Application.Helpers;
using Lyke.Application.Interfaces;
using Lyke.Core.Entities;
using Lyke.Core.Enums;
using Lyke.Core.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Lyke.Application.Services;

public class ProfileService : IProfileService
{
    private readonly UserManager<User> _userManager;
    private readonly DbContext _dbContext;
    private readonly IStorageService _storageService;
    private readonly IEventTrackingService _eventTracking;
    private readonly IMemoryCache _cache;
    private readonly ILogger<ProfileService> _logger;

    private const string BodyTypesCacheKey = "lookup:body-types";
    private const string FrameSizesCacheKey = "lookup:frame-sizes";
    private const string FitTagsCacheKey = "lookup:fit-tags";
    private static readonly TimeSpan LookupCacheDuration = TimeSpan.FromHours(1);

    public ProfileService(
        UserManager<User> userManager,
        DbContext dbContext,
        IStorageService storageService,
        IEventTrackingService eventTracking,
        IMemoryCache cache,
        ILogger<ProfileService> logger)
    {
        _userManager = userManager;
        _dbContext = dbContext;
        _storageService = storageService;
        _eventTracking = eventTracking;
        _cache = cache;
        _logger = logger;
    }

    public async Task<UserProfileResponse> GetProfileAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.Users
            .Include(u => u.BodyProfile)
            .ThenInclude(bp => bp!.FitPreferences)
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
            CreatedAt: user.CreatedAt,
            ProfileImageUrl: user.ProfileImageUrl,
            IsEmailVerified: user.EmailConfirmed
        );
    }

    public async Task<UserProfileResponse> UpdateProfileAsync(Guid userId, UpdateProfileRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.Users
            .Include(u => u.BodyProfile)
            .ThenInclude(bp => bp!.FitPreferences)
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

    public async Task<UserProfileResponse> UploadProfileImageAsync(Guid userId, Stream imageStream, string contentType, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.Users
            .Include(u => u.BodyProfile)
            .ThenInclude(bp => bp!.FitPreferences)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
        {
            throw new NotFoundException(nameof(User), userId);
        }

        // Derive extension from content type
        var ext = contentType switch
        {
            "image/jpeg" => "jpg",
            "image/png" => "png",
            "image/webp" => "webp",
            _ => throw new ValidationException("ContentType", "Unsupported image type. Use JPEG, PNG, or WebP.")
        };

        var blobPath = $"profile-images/{userId}.{ext}";

        // Delete old image if it exists
        if (!string.IsNullOrEmpty(user.ProfileImageUrl))
        {
            try
            {
                var oldBlobPath = new Uri(user.ProfileImageUrl).AbsolutePath.TrimStart('/');
                await _storageService.DeleteAsync(oldBlobPath, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete old profile image for user {UserId}", userId);
            }
        }

        var result = await _storageService.UploadAsync(imageStream, blobPath, contentType, cancellationToken);
        user.ProfileImageUrl = result.Url;
        user.UpdatedAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        _logger.LogInformation("Profile image uploaded for user {UserId}", userId);

        var completeness = CalculateProfileCompleteness(user);
        return new UserProfileResponse(
            Id: user.Id,
            Email: user.Email!,
            UserType: user.UserType,
            HasBodyProfile: user.BodyProfile != null,
            ProfileCompleteness: completeness,
            CreatedAt: user.CreatedAt,
            ProfileImageUrl: user.ProfileImageUrl,
            IsEmailVerified: user.EmailConfirmed
        );
    }

    public async Task DeleteProfileImageAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());

        if (user == null)
        {
            throw new NotFoundException(nameof(User), userId);
        }

        if (!string.IsNullOrEmpty(user.ProfileImageUrl))
        {
            try
            {
                var blobPath = new Uri(user.ProfileImageUrl).AbsolutePath.TrimStart('/');
                await _storageService.DeleteAsync(blobPath, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete profile image blob for user {UserId}", userId);
            }

            user.ProfileImageUrl = null;
            user.UpdatedAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            _logger.LogInformation("Profile image deleted for user {UserId}", userId);
        }
    }

    public async Task<BodyProfileResponse?> GetBodyProfileAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var bodyProfiles = _dbContext.Set<BodyProfile>();

        var profile = await bodyProfiles
            .Include(bp => bp.BodyType)
            .Include(bp => bp.FrameSize)
            .Include(bp => bp.FitPreferences)
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

        // Validate frame size if provided
        FrameSize? frameSize = null;
        if (request.FrameSizeId.HasValue)
        {
            frameSize = await _dbContext.Set<FrameSize>().FindAsync(new object[] { request.FrameSizeId.Value }, cancellationToken);
            if (frameSize == null)
            {
                throw new ValidationException("FrameSizeId", "Invalid frame size");
            }
        }

        var profile = new BodyProfile
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            HeightCm = request.HeightCm,
            WeightKg = request.WeightKg,
            BodyTypeId = request.BodyTypeId,
            FrameSizeId = request.FrameSizeId,
            Stature = request.Stature,
            Build = request.Build,
        };

        await bodyProfiles.AddAsync(profile, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // Add fit preferences
        if (request.FitPreferences != null && request.FitPreferences.Count > 0)
        {
            var fitPrefs = request.FitPreferences.Distinct().Select(fp => new BodyProfileFitPreference
            {
                BodyProfileId = profile.Id,
                FitPreference = fp,
            }).ToList();
            _dbContext.Set<BodyProfileFitPreference>().AddRange(fitPrefs);
            await _dbContext.SaveChangesAsync(cancellationToken);
            profile.FitPreferences = fitPrefs;
        }

        _logger.LogInformation("Body profile created for user {UserId}", userId);

        _ = _eventTracking.TrackAsync(
            AnalyticsEventType.ProfileComplete,
            userId,
            profile.Id,
            nameof(BodyProfile),
            cancellationToken: cancellationToken);

        // Set navigation properties for response mapping
        profile.BodyType = bodyType;
        profile.FrameSize = frameSize;
        return MapToBodyProfileResponse(profile);
    }

    public async Task<BodyProfileResponse> UpdateBodyProfileAsync(Guid userId, UpdateBodyProfileRequest request, CancellationToken cancellationToken = default)
    {
        var bodyProfiles = _dbContext.Set<BodyProfile>();
        var bodyTypes = _dbContext.Set<BodyType>();

        var profile = await bodyProfiles
            .Include(bp => bp.BodyType)
            .Include(bp => bp.FrameSize)
            .Include(bp => bp.FitPreferences)
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

        if (request.FrameSizeId.HasValue)
        {
            var frameSize = await _dbContext.Set<FrameSize>().FindAsync(new object[] { request.FrameSizeId.Value }, cancellationToken);
            if (frameSize == null)
            {
                throw new ValidationException("FrameSizeId", "Invalid frame size");
            }
            profile.FrameSizeId = request.FrameSizeId.Value;
            profile.FrameSize = frameSize;
        }

        if (request.Stature.HasValue)
        {
            profile.Stature = request.Stature.Value;
        }

        if (request.Build.HasValue)
        {
            profile.Build = request.Build.Value;
        }

        if (request.FitPreferences != null)
        {
            // Replace all fit preferences
            var existingPrefs = _dbContext.Set<BodyProfileFitPreference>()
                .Where(bpfp => bpfp.BodyProfileId == profile.Id);
            _dbContext.Set<BodyProfileFitPreference>().RemoveRange(existingPrefs);

            var newPrefs = request.FitPreferences.Distinct().Select(fp => new BodyProfileFitPreference
            {
                BodyProfileId = profile.Id,
                FitPreference = fp,
            }).ToList();
            _dbContext.Set<BodyProfileFitPreference>().AddRange(newPrefs);
            profile.FitPreferences = newPrefs;
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
            .Include(bp => bp.FrameSize)
            .Include(bp => bp.FitPreferences)
            .FirstOrDefaultAsync(bp => bp.UserId == userId, cancellationToken);

        if (profile == null)
        {
            return null;
        }

        return new AnonymizedBodyProfileResponse(
            HeightRange: BodyProfileHelper.GetHeightRange(profile.HeightCm),
            WeightRange: BodyProfileHelper.GetWeightRange(profile.WeightKg),
            BodyTypeName: profile.BodyType.Name,
            FrameSizeName: profile.FrameSize?.Name,
            Stature: profile.Stature,
            Build: profile.Build,
            BodyTypeLabel: BodyProfileHelper.FormatBodyTypeLabel(profile.Stature, profile.Build, profile.BodyType.Name),
            FitPreferences: profile.FitPreferences.Select(fp => fp.FitPreference).ToList()
        );
    }

    public async Task<IReadOnlyList<BodyTypeResponse>> GetBodyTypesAsync(CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(BodyTypesCacheKey, out IReadOnlyList<BodyTypeResponse>? cached) && cached != null)
        {
            return cached;
        }

        var types = await _dbContext.Set<BodyType>()
            .AsNoTracking()
            .OrderBy(bt => bt.DisplayOrder)
            .Select(bt => new BodyTypeResponse(
                bt.Id,
                bt.Name,
                bt.Description,
                bt.DisplayOrder
            ))
            .ToListAsync(cancellationToken);

        _cache.Set(BodyTypesCacheKey, (IReadOnlyList<BodyTypeResponse>)types, LookupCacheDuration);
        return types;
    }

    public async Task<IReadOnlyList<FrameSizeResponse>> GetFrameSizesAsync(CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(FrameSizesCacheKey, out IReadOnlyList<FrameSizeResponse>? cached) && cached != null)
        {
            return cached;
        }

        var sizes = await _dbContext.Set<FrameSize>()
            .AsNoTracking()
            .OrderBy(fs => fs.DisplayOrder)
            .Select(fs => new FrameSizeResponse(
                fs.Id,
                fs.Name,
                fs.Description,
                fs.DisplayOrder
            ))
            .ToListAsync(cancellationToken);

        _cache.Set(FrameSizesCacheKey, (IReadOnlyList<FrameSizeResponse>)sizes, LookupCacheDuration);
        return sizes;
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

    public async Task<IReadOnlyList<FitTagResponse>> GetFitTagsAsync(CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(FitTagsCacheKey, out IReadOnlyList<FitTagResponse>? cached) && cached != null)
        {
            return cached;
        }

        var tags = await _dbContext.Set<FitTag>()
            .AsNoTracking()
            .Where(ft => ft.IsActive)
            .OrderBy(ft => ft.Category)
            .ThenBy(ft => ft.Name)
            .Select(ft => new FitTagResponse(
                ft.Id,
                ft.Name,
                ft.Category
            ))
            .ToListAsync(cancellationToken);

        _cache.Set(FitTagsCacheKey, (IReadOnlyList<FitTagResponse>)tags, LookupCacheDuration);
        return tags;
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
            FrameSizeId: profile.FrameSizeId,
            FrameSizeName: profile.FrameSize?.Name,
            Stature: profile.Stature,
            Build: profile.Build,
            BodyTypeLabel: BodyProfileHelper.FormatBodyTypeLabel(profile.Stature, profile.Build, profile.BodyType.Name),
            FitPreferences: profile.FitPreferences.Select(fp => fp.FitPreference).ToList(),
            NeedsProfileUpdate: profile.FrameSizeId == null && profile.Stature == null,
            CreatedAt: profile.CreatedAt,
            UpdatedAt: profile.UpdatedAt
        );
    }

    private static int CalculateProfileCompleteness(User user)
    {
        var score = 0;
        var total = 5;

        // Email verified (assuming it's always set for now)
        if (!string.IsNullOrEmpty(user.Email)) score++;

        // Has body profile
        if (user.BodyProfile != null)
        {
            score++;

            // Has fit preferences
            if (user.BodyProfile.FitPreferences.Count > 0) score++;
        }

        // Account is active
        if (user.IsActive) score++;

        // Has profile image
        if (!string.IsNullOrEmpty(user.ProfileImageUrl)) score++;

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
}
