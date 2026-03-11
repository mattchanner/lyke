using Lyke.Application.Configuration;
using Lyke.Application.DTOs.Privacy;
using Lyke.Application.Interfaces;
using Lyke.Core.Entities;
using Lyke.Core.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lyke.Application.Services;

public class PrivacyService : IPrivacyService
{
    private readonly UserManager<User> _userManager;
    private readonly DbContext _dbContext;
    private readonly PrivacySettings _privacySettings;
    private readonly IEmailService _emailService;
    private readonly ILogger<PrivacyService> _logger;

    public PrivacyService(
        UserManager<User> userManager,
        DbContext dbContext,
        IOptions<PrivacySettings> privacySettings,
        IEmailService emailService,
        ILogger<PrivacyService> logger
    )
    {
        _userManager = userManager;
        _dbContext = dbContext;
        _privacySettings = privacySettings.Value;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<DataExportResponse> ExportUserDataAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            throw new NotFoundException(nameof(User), userId);
        }

        var userData = new UserExportData(
            Id: user.Id,
            Email: user.Email,
            UserName: user.UserName,
            PhoneNumber: user.PhoneNumber,
            UserType: user.UserType.ToString(),
            IsActive: user.IsActive,
            EmailConfirmed: user.EmailConfirmed,
            CreatedAt: user.CreatedAt,
            PrivacyPolicyAcceptedAt: user.PrivacyPolicyAcceptedAt,
            PrivacyPolicyVersion: user.PrivacyPolicyVersion,
            MarketingOptIn: user.MarketingOptIn
        );

        // Body profile
        var bodyProfile = await _dbContext
            .Set<BodyProfile>()
            .Include(bp => bp.BodyType)
            .Include(bp => bp.FrameSize)
            .Include(bp => bp.FitPreferences)
            .FirstOrDefaultAsync(bp => bp.UserId == userId, cancellationToken);

        BodyProfileExportData? bodyProfileData =
            bodyProfile != null
                ? new BodyProfileExportData(
                    HeightCm: bodyProfile.HeightCm,
                    WeightKg: bodyProfile.WeightKg,
                    BodyTypeName: bodyProfile.BodyType.Name,
                    FrameSizeName: bodyProfile.FrameSize?.Name,
                    FitPreferences: string.Join(
                        ", ",
                        bodyProfile.FitPreferences.Select(fp => fp.FitPreference.ToString())
                    ),
                    CreatedAt: bodyProfile.CreatedAt
                )
                : null;

        // Creator + Posts
        var creator = await _dbContext
            .Set<Creator>()
            .Include(c => c.Posts)
                .ThenInclude(p => p.PostProducts)
                    .ThenInclude(pp => pp.Product)
            .Include(c => c.Posts)
                .ThenInclude(p => p.PostProducts)
                    .ThenInclude(pp => pp.FitTags)
                        .ThenInclude(ft => ft.FitTag)
            .Include(c => c.Earnings)
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

        CreatorExportData? creatorData = null;
        var postsData = new List<PostExportData>();
        var earningsData = new List<EarningExportData>();

        if (creator != null)
        {
            postsData = creator.Posts.Select(MapPostExport).ToList();

            creatorData = new CreatorExportData(
                DisplayName: creator.DisplayName,
                Bio: creator.Bio,
                IsVerified: creator.IsVerified,
                SocialLinks: creator.SocialLinks,
                VerificationStatus: creator.VerificationStatus.ToString(),
                CreatedAt: creator.CreatedAt,
                Posts: postsData
            );

            earningsData = creator
                .Earnings.Select(e => new EarningExportData(
                    EarningType: e.EarningType.ToString(),
                    Amount: e.Amount,
                    Currency: e.Currency,
                    Status: e.Status.ToString(),
                    PaidAt: e.PaidAt,
                    CreatedAt: e.CreatedAt
                ))
                .ToList();
        }

        // Engagements
        var engagements = await _dbContext
            .Set<Engagement>()
            .Where(e => e.UserId == userId)
            .Select(e => new EngagementExportData(
                PostId: e.PostId,
                Type: e.Type.ToString(),
                CreatedAt: e.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        // Click events
        var clickEvents = await _dbContext
            .Set<ClickEvent>()
            .Where(ce => ce.UserId == userId)
            .Select(ce => new ClickEventExportData(
                PostId: ce.PostId,
                CreatedAt: ce.CreatedAt,
                ConvertedAt: ce.ConvertedAt
            ))
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Data export generated for user {UserId}", userId);

        _ = _emailService.SendDataExportNotificationAsync(
            user.Email!,
            creator != null,
            cancellationToken
        );

        return new DataExportResponse(
            User: userData,
            BodyProfile: bodyProfileData,
            Creator: creatorData,
            Posts: postsData,
            Engagements: engagements,
            ClickEvents: clickEvents,
            Earnings: earningsData,
            ExportedAt: DateTime.UtcNow
        );
    }

    public async Task<ConsentStatusResponse> GetConsentStatusAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            throw new NotFoundException(nameof(User), userId);
        }

        return new ConsentStatusResponse(
            MarketingOptIn: user.MarketingOptIn,
            PrivacyPolicyVersion: user.PrivacyPolicyVersion,
            PrivacyPolicyAcceptedAt: user.PrivacyPolicyAcceptedAt,
            CurrentPolicyVersion: _privacySettings.CurrentPolicyVersion,
            NeedsReconsent: user.PrivacyPolicyVersion != _privacySettings.CurrentPolicyVersion
        );
    }

    public async Task UpdateConsentAsync(
        Guid userId,
        UpdateConsentRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            throw new NotFoundException(nameof(User), userId);
        }

        if (request.MarketingOptIn.HasValue)
        {
            user.MarketingOptIn = request.MarketingOptIn.Value;
        }

        if (request.AcceptPrivacyPolicy == true)
        {
            user.PrivacyPolicyAcceptedAt = DateTime.UtcNow;
            user.PrivacyPolicyVersion = _privacySettings.CurrentPolicyVersion;
        }

        await _userManager.UpdateAsync(user);

        _logger.LogInformation(
            "Consent updated for user {UserId}: Marketing={Marketing}, PolicyAccepted={Policy}",
            userId,
            request.MarketingOptIn,
            request.AcceptPrivacyPolicy
        );
    }

    private static PostExportData MapPostExport(Post post) =>
        new(
            Id: post.Id,
            Title: post.Title,
            Description: post.Description,
            MediaType: post.MediaType.ToString(),
            MediaUrls: post.MediaUrls,
            Status: post.Status.ToString(),
            PublishedAt: post.PublishedAt,
            CreatedAt: post.CreatedAt,
            Products: post.PostProducts.Select(pp => new PostProductExportData(
                    ProductName: pp.Product.Name,
                    SizeWorn: pp.SizeWorn,
                    FitNotes: pp.FitNotes,
                    FitRating: pp.FitRating?.ToString(),
                    StylingNotes: pp.StylingNotes,
                    FitTags: pp.FitTags.Select(ft => ft.FitTag.Name).ToList()
                ))
                .ToList()
        );
}
