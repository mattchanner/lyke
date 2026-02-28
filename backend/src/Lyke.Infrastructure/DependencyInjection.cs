using Lyke.Application.DTOs.Media;
using Lyke.Application.Interfaces;
using Lyke.Core.Entities;
using Lyke.Infrastructure.Configuration;
using Lyke.Infrastructure.Data;
using Lyke.Infrastructure.Queue;
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
        services.AddDatabase(configuration);

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
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;
            })
            .AddEntityFrameworkStores<LykeDbContext>()
            .AddDefaultTokenProviders();

        // Email
        services.AddSingleton<IEmailService, Lyke.Infrastructure.Email.EmailService>();

        // Background services
        services.AddHostedService<DataRetentionService>();
        services.AddHostedService<MetricsAggregationService>();

        // Storage
        services.AddStorage(configuration);

        return services;
    }

    public static IServiceCollection AddDatabase(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddDbContext<LykeDbContext>(options =>
        {
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                b =>
                {
                    b.MigrationsAssembly(typeof(LykeDbContext).Assembly.FullName);
                    b.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(5),
                        errorCodesToAdd: null);
                    b.CommandTimeout(30);
                }
            );
        });

        // Register abstract DbContext for services that use EF Core generically
        services.AddScoped<DbContext>(sp => sp.GetRequiredService<LykeDbContext>());

        return services;
    }

    public static IServiceCollection AddStorage(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        // Parse blob connection string — Aspire appends ";ContainerName=..." which
        // BlobServiceClient doesn't understand, so strip it and use separately.
        var rawConnectionString = configuration.GetConnectionString("AzureStorage")!;
        var containerName = configuration.GetSection(AzureBlobSettings.SectionName)
            .GetValue("ContainerName", "media");

        var parts = rawConnectionString.Split(';', StringSplitOptions.RemoveEmptyEntries);
        var connStringParts = new List<string>();
        foreach (var part in parts)
        {
            if (part.StartsWith("ContainerName=", StringComparison.OrdinalIgnoreCase))
                containerName = part["ContainerName=".Length..];
            else if (part.StartsWith("QueueName=", StringComparison.OrdinalIgnoreCase))
            {
                // skip — queue name is configured via AzureQueueSettings
            }
            else
                connStringParts.Add(part);
        }

        var cleanConnectionString = string.Join(';', connStringParts);

        AzureBlobSettings blobSettings = new()
        {
            ConnectionString = cleanConnectionString,
            ContainerName = containerName
        };

        services.AddSingleton(blobSettings);

        // Blob + processing services
        services.AddSingleton<IStorageService, AzureBlobStorageService>();
        services.AddSingleton<IImageProcessingService, ImageSharpProcessingService>();
        services.AddSingleton<IVideoProcessingService, FFmpegVideoProcessingService>();

        // Queue settings (same storage account as blob)
        var queueSettings = new AzureQueueSettings();
        configuration.GetSection(AzureQueueSettings.SectionName).Bind(queueSettings);
        services.AddSingleton(queueSettings);

        // Queue service
        services.AddSingleton<IMessageQueue<MediaProcessingMessage>, AzureStorageQueueService<MediaProcessingMessage>>();

        return services;
    }
}
