using FluentValidation;
using Lyke.Application.Configuration;
using Lyke.Application.Interfaces;
using Lyke.Application.Services;
using Lyke.Application.Validators.Media;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lyke.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        // Configuration
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.Configure<MatchingSettings>(
            configuration.GetSection(MatchingSettings.SectionName)
        );
        services.Configure<CommerceSettings>(
            configuration.GetSection(CommerceSettings.SectionName)
        );
        services.Configure<CreatorSettings>(configuration.GetSection(CreatorSettings.SectionName));
        services.Configure<MediaUploadSettings>(
            configuration.GetSection(MediaUploadSettings.SectionName)
        );
        services.Configure<RetailerSettings>(
            configuration.GetSection(RetailerSettings.SectionName)
        );
        services.Configure<SocialAuthSettings>(
            configuration.GetSection(SocialAuthSettings.SectionName)
        );
        services.Configure<EmailSettings>(configuration.GetSection(EmailSettings.SectionName));
        services.Configure<PrivacySettings>(configuration.GetSection(PrivacySettings.SectionName));
        services.Configure<ModerationSettings>(
            configuration.GetSection(ModerationSettings.SectionName)
        );
        services.Configure<AnalyticsSettings>(
            configuration.GetSection(AnalyticsSettings.SectionName)
        );

        // Validators
        services.AddValidatorsFromAssemblyContaining<IAuthService>();
        services.AddSingleton<MediaUploadValidator>();

        // Services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IProfileService, ProfileService>();
        services.AddScoped<IFeedService, FeedService>();
        services.AddScoped<ICommerceService, CommerceService>();
        services.AddScoped<ICreatorService, CreatorService>();
        services.AddScoped<IMediaService, MediaService>();
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<IRetailerService, RetailerService>();
        services.AddScoped<IPrivacyService, PrivacyService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IEventTrackingService, EventTrackingService>();
        services.AddScoped<IFollowService, FollowService>();
        services.AddScoped<IQuizService, QuizService>();
        services.AddScoped<IKibbeQuizService, KibbeQuizService>();
        services.AddHttpClient<ISocialTokenValidator, SocialTokenValidator>();

        return services;
    }
}
