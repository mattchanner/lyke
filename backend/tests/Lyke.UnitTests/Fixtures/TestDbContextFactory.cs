using Lyke.Core.Entities;
using Lyke.Core.Enums;
using Lyke.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Lyke.UnitTests.Fixtures;

public static class TestDbContextFactory
{
    public static LykeDbContext Create(string? dbName = null)
    {
        dbName ??= Guid.NewGuid().ToString();

        var options = new DbContextOptionsBuilder<LykeDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        var context = new LykeDbContext(options);
        context.Database.EnsureCreated();

        return context;
    }

    public static async Task<LykeDbContext> CreateWithSeedDataAsync(string? dbName = null)
    {
        var context = Create(dbName);
        await SeedTestDataAsync(context);
        return context;
    }

    public static async Task SeedTestDataAsync(LykeDbContext context)
    {
        // Seed body types (matching production data)
        if (!context.BodyTypes.Any())
        {
            context.BodyTypes.AddRange(
                new BodyType
                {
                    Id = 1,
                    Name = "Hourglass",
                    Description = "Balanced bust and hips with defined waist",
                    DisplayOrder = 1,
                },
                new BodyType
                {
                    Id = 2,
                    Name = "Pear",
                    Description = "Hips wider than shoulders",
                    DisplayOrder = 2,
                },
                new BodyType
                {
                    Id = 3,
                    Name = "Apple",
                    Description = "Fuller midsection with slimmer legs",
                    DisplayOrder = 3,
                },
                new BodyType
                {
                    Id = 4,
                    Name = "Rectangle",
                    Description = "Balanced proportions, less waist definition",
                    DisplayOrder = 4,
                },
                new BodyType
                {
                    Id = 5,
                    Name = "Inverted Triangle",
                    Description = "Shoulders wider than hips",
                    DisplayOrder = 5,
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
                    Name = "True to Size",
                    Category = "Fit",
                },
                new FitTag
                {
                    Id = 2,
                    Name = "Runs Small",
                    Category = "Fit",
                },
                new FitTag
                {
                    Id = 3,
                    Name = "Runs Large",
                    Category = "Fit",
                },
                new FitTag
                {
                    Id = 4,
                    Name = "Stretchy",
                    Category = "Material",
                },
                new FitTag
                {
                    Id = 5,
                    Name = "Comfortable",
                    Category = "Feel",
                }
            );
        }

        await context.SaveChangesAsync();
    }

    public static User CreateTestUser(
        Guid? id = null,
        string? email = null,
        UserType userType = UserType.Shopper,
        bool isActive = true
    )
    {
        id ??= Guid.NewGuid();
        email ??= $"test-{id}@test.com";

        return new User
        {
            Id = id.Value,
            Email = email,
            UserName = email,
            NormalizedEmail = email.ToUpperInvariant(),
            NormalizedUserName = email.ToUpperInvariant(),
            UserType = userType,
            IsActive = isActive,
            CreatedAt = DateTime.UtcNow,
            SecurityStamp = Guid.NewGuid().ToString(),
        };
    }

    public static Creator CreateTestCreator(
        Guid? id = null,
        Guid? userId = null,
        string displayName = "Test Creator",
        bool isVerified = false
    )
    {
        return new Creator
        {
            Id = id ?? Guid.NewGuid(),
            UserId = userId ?? Guid.NewGuid(),
            DisplayName = displayName,
            Bio = "Test bio",
            IsVerified = isVerified,
        };
    }

    public static Retailer CreateTestRetailer(
        Guid? id = null,
        string name = "Test Retailer",
        bool isActive = true
    )
    {
        return new Retailer
        {
            Id = id ?? Guid.NewGuid(),
            Name = name,
            WebsiteUrl = "https://test-retailer.com",
            IsActive = isActive,
        };
    }

    public static Product CreateTestProduct(
        Guid? id = null,
        Guid? retailerId = null,
        string name = "Test Product",
        decimal price = 99.99m,
        bool isActive = true
    )
    {
        return new Product
        {
            Id = id ?? Guid.NewGuid(),
            RetailerId = retailerId ?? Guid.NewGuid(),
            ExternalSku = $"SKU-{Guid.NewGuid():N}".Substring(0, 20),
            Name = name,
            Description = "Test product description",
            Category = "Tops",
            Price = price,
            Currency = "USD",
            ProductUrl = "https://test-retailer.com/product/1",
            IsActive = isActive,
        };
    }

    public static Post CreateTestPost(
        Guid? id = null,
        Guid? creatorId = null,
        PostStatus status = PostStatus.Draft,
        string? title = null
    )
    {
        return new Post
        {
            Id = id ?? Guid.NewGuid(),
            CreatorId = creatorId ?? Guid.NewGuid(),
            Title = title ?? "Test Post",
            Description = "Test post description",
            MediaType = MediaType.Image,
            MediaUrls = "[\"https://example.com/image1.jpg\"]",
            Status = status,
            PublishedAt = status == PostStatus.Published ? DateTime.UtcNow : null,
        };
    }

    public static PostProduct CreateTestPostProduct(
        Guid? id = null,
        Guid? postId = null,
        Guid? productId = null,
        string sizeWorn = "M"
    )
    {
        return new PostProduct
        {
            Id = id ?? Guid.NewGuid(),
            PostId = postId ?? Guid.NewGuid(),
            ProductId = productId ?? Guid.NewGuid(),
            SizeWorn = sizeWorn,
            FitRating = FitRating.TrueToSize,
            FitNotes = "Fits well",
        };
    }

    public static BodyProfile CreateTestBodyProfile(
        Guid? id = null,
        Guid? userId = null,
        int heightCm = 170,
        decimal weightKg = 70m,
        int bodyTypeId = 1
    )
    {
        return new BodyProfile
        {
            Id = id ?? Guid.NewGuid(),
            UserId = userId ?? Guid.NewGuid(),
            HeightCm = heightCm,
            WeightKg = weightKg,
            BodyTypeId = bodyTypeId,
            FitPreference = FitPreference.Regular,
        };
    }
}
