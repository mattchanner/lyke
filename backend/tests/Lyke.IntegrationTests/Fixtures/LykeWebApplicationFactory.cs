using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Lyke.Application.DTOs.Media;
using Lyke.Application.Interfaces;
using Lyke.Core.Entities;
using Lyke.Core.Enums;
using Lyke.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Lyke.IntegrationTests.Fixtures;

public class LykeWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = Guid.NewGuid().ToString();

    /// <summary>
    /// JSON serializer options matching the API configuration (uses string enums).
    /// </summary>
    public static JsonSerializerOptions JsonOptions { get; } = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Remove the MigrateDatabaseBackgroundWorker as it requires relational database
            var backgroundWorkerDescriptor = services.FirstOrDefault(d =>
                d.ImplementationType?.Name == "MigrateDatabaseBackgroundWorker");
            if (backgroundWorkerDescriptor != null)
            {
                services.Remove(backgroundWorkerDescriptor);
            }


            // Remove ALL DbContext-related registrations to avoid provider conflicts
            var descriptorsToRemove = services.Where(d =>
                d.ServiceType == typeof(DbContextOptions<LykeDbContext>) ||
                d.ServiceType == typeof(DbContextOptions) ||
                d.ServiceType.FullName?.Contains("Npgsql") == true ||
                d.ImplementationType?.FullName?.Contains("Npgsql") == true ||
                d.ServiceType.FullName?.Contains("EntityFrameworkCore") == true
            ).ToList();

            foreach (var descriptor in descriptorsToRemove)
            {
                services.Remove(descriptor);
            }

            // Remove LykeDbContext registration
            services.RemoveAll<LykeDbContext>();

            // Add in-memory database for testing
            services.AddDbContext<LykeDbContext>((sp, options) =>
            {
                options.UseInMemoryDatabase(_databaseName);
            });

            // Register DbContext as an alias for LykeDbContext (some services depend on the base type)
            services.AddScoped<DbContext>(sp => sp.GetRequiredService<LykeDbContext>());

            // Replace storage service with mock for testing
            services.RemoveAll<IStorageService>();
            services.AddSingleton<IStorageService, MockStorageService>();

            // Build service provider and seed data
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<LykeDbContext>();

            db.Database.EnsureCreated();
            SeedTestData(db);
        });
    }

    private static void SeedTestData(LykeDbContext context)
    {
        // Seed body types
        if (!context.BodyTypes.Any())
        {
            context.BodyTypes.AddRange(
                new BodyType
                {
                    Id = 1,
                    Name = "Petite",
                    Description = "Under 5'4\" with small frame",
                    DisplayOrder = 1,
                },
                new BodyType
                {
                    Id = 2,
                    Name = "Athletic",
                    Description = "Toned and muscular build",
                    DisplayOrder = 2,
                },
                new BodyType
                {
                    Id = 3,
                    Name = "Curvy",
                    Description = "Fuller figure with defined curves",
                    DisplayOrder = 3,
                },
                new BodyType
                {
                    Id = 4,
                    Name = "Tall",
                    Description = "Over 5'9\" with long proportions",
                    DisplayOrder = 4,
                },
                new BodyType
                {
                    Id = 5,
                    Name = "Plus Size",
                    Description = "Fuller figure, larger frame",
                    DisplayOrder = 5,
                },
                new BodyType
                {
                    Id = 6,
                    Name = "Straight",
                    Description = "Balanced proportions, less defined waist",
                    DisplayOrder = 6,
                },
                new BodyType
                {
                    Id = 7,
                    Name = "Hourglass",
                    Description = "Balanced bust and hips with defined waist",
                    DisplayOrder = 7,
                },
                new BodyType
                {
                    Id = 8,
                    Name = "Pear",
                    Description = "Hips wider than shoulders",
                    DisplayOrder = 8,
                }
            );
        }

        // Seed fit tags
        if (!context.FitTags.Any())
        {
            context.FitTags.AddRange(
                new FitTag
                {
                    Id = 1,
                    Name = "True to size",
                    Category = "General",
                    IsActive = true,
                },
                new FitTag
                {
                    Id = 2,
                    Name = "Runs small",
                    Category = "General",
                    IsActive = true,
                },
                new FitTag
                {
                    Id = 3,
                    Name = "Runs large",
                    Category = "General",
                    IsActive = true,
                },
                new FitTag
                {
                    Id = 4,
                    Name = "Tight on hips",
                    Category = "Fit Issue",
                    IsActive = true,
                },
                new FitTag
                {
                    Id = 5,
                    Name = "Long in arms",
                    Category = "Fit Issue",
                    IsActive = true,
                },
                new FitTag
                {
                    Id = 6,
                    Name = "Short in torso",
                    Category = "Fit Issue",
                    IsActive = true,
                }
            );
        }

        context.SaveChanges();
    }

    public async Task<string> CreateTestUserAndGetTokenAsync(
        string email = "test@example.com",
        string password = "Test123!",
        UserType userType = UserType.Shopper
    )
    {
        using var scope = Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

        var existingUser = await userManager.FindByEmailAsync(email);
        if (existingUser != null)
        {
            await userManager.DeleteAsync(existingUser);
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            UserName = email,
            UserType = userType,
            IsActive = true,
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow,
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            throw new Exception(
                $"Failed to create test user: {string.Join(", ", result.Errors.Select(e => e.Description))}"
            );
        }

        // Get token via login endpoint
        var client = CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/v1/login", new { email, password });
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions);
        return content?.Data?.AccessToken ?? throw new Exception("Failed to get token");
    }

    public async Task<User> CreateTestUserAsync(
        string email = "test@example.com",
        string password = "Test123!",
        UserType userType = UserType.Shopper
    )
    {
        using var scope = Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

        var existingUser = await userManager.FindByEmailAsync(email);
        if (existingUser != null)
        {
            return existingUser;
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            UserName = email,
            UserType = userType,
            IsActive = true,
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow,
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            throw new Exception(
                $"Failed to create test user: {string.Join(", ", result.Errors.Select(e => e.Description))}"
            );
        }

        return user;
    }

    public LykeDbContext GetDbContext()
    {
        var scope = Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<LykeDbContext>();
    }

    private record AuthResponse(bool Success, AuthData? Data);

    private record AuthData(string AccessToken, string RefreshToken, DateTime ExpiresAt);
}

/// <summary>
/// Mock storage service for integration tests.
/// </summary>
internal class MockStorageService : IStorageService
{
    private readonly HashSet<string> _uploadedFiles = new();

    public Task<StorageUploadResult> UploadAsync(
        Stream content,
        string blobPath,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        _uploadedFiles.Add(blobPath);
        return Task.FromResult(new StorageUploadResult(
            Url: $"https://mockstorage.example.com/{blobPath}",
            BlobName: blobPath,
            SizeBytes: content.Length,
            ContentType: contentType
        ));
    }

    public Task DeleteAsync(string blobPath, CancellationToken cancellationToken = default)
    {
        _uploadedFiles.Remove(blobPath);
        return Task.CompletedTask;
    }

    public Task DeleteManyAsync(IEnumerable<string> blobPaths, CancellationToken cancellationToken = default)
    {
        foreach (var path in blobPaths)
        {
            _uploadedFiles.Remove(path);
        }
        return Task.CompletedTask;
    }

    public Task<string> GenerateSasUrlAsync(
        string blobPath,
        TimeSpan? expiry = null,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult($"https://mockstorage.example.com/{blobPath}?sas=mock-token");
    }

    public Task<bool> ExistsAsync(string blobPath, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_uploadedFiles.Contains(blobPath));
    }
}
