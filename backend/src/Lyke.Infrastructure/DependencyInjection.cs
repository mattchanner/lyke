using Lyke.Application.Interfaces;
using Lyke.Core.Entities;
using Lyke.Core.Interfaces;
using Lyke.Infrastructure.Configuration;
using Lyke.Infrastructure.Data;
using Lyke.Infrastructure.Repositories;
using Lyke.Infrastructure.Services;
using Lyke.Infrastructure.Storage;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lyke.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        // Database
        services.AddDbContext<LykeDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(LykeDbContext).Assembly.FullName)
            )
        );

        // Register DbContext as well for services that need generic access
        services.AddScoped<DbContext>(sp => sp.GetRequiredService<LykeDbContext>());

        // Identity
        services
            .AddIdentity<User, IdentityRole<Guid>>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = false;
            })
            .AddEntityFrameworkStores<LykeDbContext>()
            .AddDefaultTokenProviders();

        // Repositories
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Storage
        services.AddStorage(configuration);

        return services;
    }

    public static IServiceCollection AddStorage(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        // Configuration
        services.Configure<AzureBlobSettings>(
            configuration.GetSection(AzureBlobSettings.SectionName)
        );

        // Services
        services.AddSingleton<IStorageService, AzureBlobStorageService>();
        services.AddSingleton<IImageProcessingService, ImageSharpProcessingService>();
        services.AddSingleton<IVideoProcessingService, FFmpegVideoProcessingService>();

        return services;
    }
}
