using System.Net.Http.Json;
using Lyke.Core.Entities;
using Lyke.Core.Enums;
using Lyke.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Lyke.IntegrationTests.Fixtures;

public class LykeWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = Guid.NewGuid().ToString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Remove the existing DbContext registration
            var descriptor = services.SingleOrDefault(d =>
                d.ServiceType == typeof(DbContextOptions<LykeDbContext>)
            );
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Remove existing DbContext registration
            var dbContextDescriptor = services.SingleOrDefault(d =>
                d.ServiceType == typeof(LykeDbContext)
            );
            if (dbContextDescriptor != null)
            {
                services.Remove(dbContextDescriptor);
            }

            // Add in-memory database for testing
            services.AddDbContext<LykeDbContext>(options =>
            {
                options.UseInMemoryDatabase(_databaseName);
            });

            // Ensure the database is created and seeded
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
        var context = scope.ServiceProvider.GetRequiredService<LykeDbContext>();

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
        var response = await client.PostAsJsonAsync("/api/auth/login", new { email, password });
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadFromJsonAsync<AuthResponse>();
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
