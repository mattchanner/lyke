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

        // Validators
        services.AddValidatorsFromAssemblyContaining<IAuthService>();

        // Services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IProfileService, ProfileService>();
        services.AddScoped<IFeedService, FeedService>();

        return services;
    }
}
