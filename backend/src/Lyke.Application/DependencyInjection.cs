using FluentValidation;
using Lyke.Application.Configuration;
using Lyke.Application.Interfaces;
using Lyke.Application.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lyke.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        // Configuration
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.Configure<MatchingSettings>(configuration.GetSection(MatchingSettings.SectionName));
        services.Configure<CommerceSettings>(configuration.GetSection(CommerceSettings.SectionName));
        services.Configure<CreatorSettings>(configuration.GetSection(CreatorSettings.SectionName));

        // Validators
        services.AddValidatorsFromAssemblyContaining<IAuthService>();

        // Services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IProfileService, ProfileService>();
        services.AddScoped<IFeedService, FeedService>();
        services.AddScoped<ICommerceService, CommerceService>();
        services.AddScoped<ICreatorService, CreatorService>();

        return services;
    }
}
