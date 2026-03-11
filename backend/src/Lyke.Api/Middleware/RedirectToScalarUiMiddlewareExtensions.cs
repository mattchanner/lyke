namespace Lyke.Api.Middleware;

public static class RedirectToScalarUiMiddlewareExtensions
{
    public static IServiceCollection AddRedirectToScalarUiMiddleware(
        this IServiceCollection services
    )
    {
        services.AddScoped<RedirectToScalarUiMiddleware>();
        return services;
    }

    public static IApplicationBuilder UseRedirectToScalarUiMiddleware(
        this IApplicationBuilder builder
    )
    {
        builder.UseMiddleware<RedirectToScalarUiMiddleware>();
        return builder;
    }
}
