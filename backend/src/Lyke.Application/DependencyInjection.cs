using Microsoft.Extensions.DependencyInjection;

namespace Lyke.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Register application services here
        // services.AddScoped<IAuthService, AuthService>();
        // services.AddScoped<IProfileService, ProfileService>();
        // services.AddScoped<IFeedService, FeedService>();

        return services;
    }
}
