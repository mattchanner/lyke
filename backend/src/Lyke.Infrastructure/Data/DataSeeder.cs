using Lyke.Core.Entities;
using Lyke.Core.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Lyke.Infrastructure.Data;

public static class DataSeeder
{
    private const string DefaultPassword = "Test1234!";

    // Deterministic GUIDs for reproducible seed data
    // Admin
    private static readonly Guid AdminUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    // Shoppers
    private static readonly Guid Shopper1Id = Guid.Parse("00000000-0000-0000-0000-000000000010");
    private static readonly Guid Shopper2Id = Guid.Parse("00000000-0000-0000-0000-000000000011");
    private static readonly Guid Shopper3Id = Guid.Parse("00000000-0000-0000-0000-000000000012");
    private static readonly Guid Shopper4Id = Guid.Parse("00000000-0000-0000-0000-000000000013");
    private static readonly Guid Shopper5Id = Guid.Parse("00000000-0000-0000-0000-000000000014");

    // Creator users
    private static readonly Guid CreatorUser1Id = Guid.Parse(
        "00000000-0000-0000-0000-000000000020"
    );
    private static readonly Guid CreatorUser2Id = Guid.Parse(
        "00000000-0000-0000-0000-000000000021"
    );
    private static readonly Guid CreatorUser3Id = Guid.Parse(
        "00000000-0000-0000-0000-000000000022"
    );
    private static readonly Guid CreatorUser4Id = Guid.Parse(
        "00000000-0000-0000-0000-000000000023"
    );

    // Creator entities
    private static readonly Guid Creator1Id = Guid.Parse("00000000-0000-0000-0000-000000000030");
    private static readonly Guid Creator2Id = Guid.Parse("00000000-0000-0000-0000-000000000031");
    private static readonly Guid Creator3Id = Guid.Parse("00000000-0000-0000-0000-000000000032");
    private static readonly Guid Creator4Id = Guid.Parse("00000000-0000-0000-0000-000000000033");

    // Retailer users
    private static readonly Guid RetailerUser1Id = Guid.Parse(
        "00000000-0000-0000-0000-000000000040"
    );
    private static readonly Guid RetailerUser2Id = Guid.Parse(
        "00000000-0000-0000-0000-000000000041"
    );
    private static readonly Guid RetailerUser3Id = Guid.Parse(
        "00000000-0000-0000-0000-000000000042"
    );

    // Retailer entities
    private static readonly Guid Retailer1Id = Guid.Parse("00000000-0000-0000-0000-000000000050");
    private static readonly Guid Retailer2Id = Guid.Parse("00000000-0000-0000-0000-000000000051");
    private static readonly Guid Retailer3Id = Guid.Parse("00000000-0000-0000-0000-000000000052");

    // Products (6 per retailer = 18 total)
    private static readonly Guid Product1Id = Guid.Parse("00000000-0000-0000-0000-000000000100");
    private static readonly Guid Product2Id = Guid.Parse("00000000-0000-0000-0000-000000000101");
    private static readonly Guid Product3Id = Guid.Parse("00000000-0000-0000-0000-000000000102");
    private static readonly Guid Product4Id = Guid.Parse("00000000-0000-0000-0000-000000000103");
    private static readonly Guid Product5Id = Guid.Parse("00000000-0000-0000-0000-000000000104");
    private static readonly Guid Product6Id = Guid.Parse("00000000-0000-0000-0000-000000000105");
    private static readonly Guid Product7Id = Guid.Parse("00000000-0000-0000-0000-000000000106");
    private static readonly Guid Product8Id = Guid.Parse("00000000-0000-0000-0000-000000000107");
    private static readonly Guid Product9Id = Guid.Parse("00000000-0000-0000-0000-000000000108");
    private static readonly Guid Product10Id = Guid.Parse("00000000-0000-0000-0000-000000000109");
    private static readonly Guid Product11Id = Guid.Parse("00000000-0000-0000-0000-000000000110");
    private static readonly Guid Product12Id = Guid.Parse("00000000-0000-0000-0000-000000000111");
    private static readonly Guid Product13Id = Guid.Parse("00000000-0000-0000-0000-000000000112");
    private static readonly Guid Product14Id = Guid.Parse("00000000-0000-0000-0000-000000000113");
    private static readonly Guid Product15Id = Guid.Parse("00000000-0000-0000-0000-000000000114");
    private static readonly Guid Product16Id = Guid.Parse("00000000-0000-0000-0000-000000000115");
    private static readonly Guid Product17Id = Guid.Parse("00000000-0000-0000-0000-000000000116");
    private static readonly Guid Product18Id = Guid.Parse("00000000-0000-0000-0000-000000000117");

    // Posts (3 per creator = 12 total)
    private static readonly Guid Post1Id = Guid.Parse("00000000-0000-0000-0000-000000000200");
    private static readonly Guid Post2Id = Guid.Parse("00000000-0000-0000-0000-000000000201");
    private static readonly Guid Post3Id = Guid.Parse("00000000-0000-0000-0000-000000000202");
    private static readonly Guid Post4Id = Guid.Parse("00000000-0000-0000-0000-000000000203");
    private static readonly Guid Post5Id = Guid.Parse("00000000-0000-0000-0000-000000000204");
    private static readonly Guid Post6Id = Guid.Parse("00000000-0000-0000-0000-000000000205");
    private static readonly Guid Post7Id = Guid.Parse("00000000-0000-0000-0000-000000000206");
    private static readonly Guid Post8Id = Guid.Parse("00000000-0000-0000-0000-000000000207");
    private static readonly Guid Post9Id = Guid.Parse("00000000-0000-0000-0000-000000000208");
    private static readonly Guid Post10Id = Guid.Parse("00000000-0000-0000-0000-000000000209");
    private static readonly Guid Post11Id = Guid.Parse("00000000-0000-0000-0000-000000000210");
    private static readonly Guid Post12Id = Guid.Parse("00000000-0000-0000-0000-000000000211");

    // PostProducts (2 per post = 24 total)
    private static readonly Guid PostProduct1Id = Guid.Parse(
        "00000000-0000-0000-0000-000000000300"
    );
    private static readonly Guid PostProduct2Id = Guid.Parse(
        "00000000-0000-0000-0000-000000000301"
    );
    private static readonly Guid PostProduct3Id = Guid.Parse(
        "00000000-0000-0000-0000-000000000302"
    );
    private static readonly Guid PostProduct4Id = Guid.Parse(
        "00000000-0000-0000-0000-000000000303"
    );
    private static readonly Guid PostProduct5Id = Guid.Parse(
        "00000000-0000-0000-0000-000000000304"
    );
    private static readonly Guid PostProduct6Id = Guid.Parse(
        "00000000-0000-0000-0000-000000000305"
    );
    private static readonly Guid PostProduct7Id = Guid.Parse(
        "00000000-0000-0000-0000-000000000306"
    );
    private static readonly Guid PostProduct8Id = Guid.Parse(
        "00000000-0000-0000-0000-000000000307"
    );
    private static readonly Guid PostProduct9Id = Guid.Parse(
        "00000000-0000-0000-0000-000000000308"
    );
    private static readonly Guid PostProduct10Id = Guid.Parse(
        "00000000-0000-0000-0000-000000000309"
    );
    private static readonly Guid PostProduct11Id = Guid.Parse(
        "00000000-0000-0000-0000-000000000310"
    );
    private static readonly Guid PostProduct12Id = Guid.Parse(
        "00000000-0000-0000-0000-000000000311"
    );
    private static readonly Guid PostProduct13Id = Guid.Parse(
        "00000000-0000-0000-0000-000000000312"
    );
    private static readonly Guid PostProduct14Id = Guid.Parse(
        "00000000-0000-0000-0000-000000000313"
    );
    private static readonly Guid PostProduct15Id = Guid.Parse(
        "00000000-0000-0000-0000-000000000314"
    );
    private static readonly Guid PostProduct16Id = Guid.Parse(
        "00000000-0000-0000-0000-000000000315"
    );
    private static readonly Guid PostProduct17Id = Guid.Parse(
        "00000000-0000-0000-0000-000000000316"
    );
    private static readonly Guid PostProduct18Id = Guid.Parse(
        "00000000-0000-0000-0000-000000000317"
    );
    private static readonly Guid PostProduct19Id = Guid.Parse(
        "00000000-0000-0000-0000-000000000318"
    );
    private static readonly Guid PostProduct20Id = Guid.Parse(
        "00000000-0000-0000-0000-000000000319"
    );
    private static readonly Guid PostProduct21Id = Guid.Parse(
        "00000000-0000-0000-0000-000000000320"
    );
    private static readonly Guid PostProduct22Id = Guid.Parse(
        "00000000-0000-0000-0000-000000000321"
    );
    private static readonly Guid PostProduct23Id = Guid.Parse(
        "00000000-0000-0000-0000-000000000322"
    );
    private static readonly Guid PostProduct24Id = Guid.Parse(
        "00000000-0000-0000-0000-000000000323"
    );

    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<LykeDbContext>>();
        var context = services.GetRequiredService<LykeDbContext>();
        var userManager = services.GetRequiredService<UserManager<User>>();

        // Idempotent — skip if data already exists
        if (await context.Creators.AnyAsync())
        {
            logger.LogInformation("Seed data already exists, skipping");
            return;
        }

        logger.LogInformation("Seeding development data...");

        await SeedUsersAsync(userManager, logger);
        await SeedBodyProfilesAsync(context);
        await SeedCreatorsAsync(context);
        await SeedRetailersAsync(context);
        await SeedProductsAsync(context);
        await SeedPostsAsync(context);
        await SeedPostProductsAsync(context);
        await SeedPostFitTagsAsync(context);
        await SeedEngagementsAsync(context);
        await SeedClickEventsAsync(context);
        await SeedCreatorEarningsAsync(context);

        logger.LogInformation("Development data seeded successfully");
    }

    private static async Task SeedUsersAsync(UserManager<User> userManager, ILogger logger)
    {
        var users = new (Guid id, string userName, string email, UserType userType)[]
        {
            (AdminUserId, "admin", "admin@lyke.app", UserType.Admin),
            (Shopper1Id, "emma.shopper", "emma@example.com", UserType.Shopper),
            (Shopper2Id, "olivia.shopper", "olivia@example.com", UserType.Shopper),
            (Shopper3Id, "ava.shopper", "ava@example.com", UserType.Shopper),
            (Shopper4Id, "mia.shopper", "mia@example.com", UserType.Shopper),
            (Shopper5Id, "sophia.shopper", "sophia@example.com", UserType.Shopper),
            (CreatorUser1Id, "jessica.creates", "jessica@example.com", UserType.Creator),
            (CreatorUser2Id, "maya.creates", "maya@example.com", UserType.Creator),
            (CreatorUser3Id, "taylor.creates", "taylor@example.com", UserType.Creator),
            (CreatorUser4Id, "priya.creates", "priya@example.com", UserType.Creator),
            (RetailerUser1Id, "zara.retail", "contact@zara-example.com", UserType.Retailer),
            (RetailerUser2Id, "hm.retail", "contact@hm-example.com", UserType.Retailer),
            (RetailerUser3Id, "asos.retail", "contact@asos-example.com", UserType.Retailer),
        };

        // Profile images for creator accounts
        var profileImages = new Dictionary<Guid, string>
        {
            [CreatorUser1Id] =
                "https://images.unsplash.com/photo-1494790108377-be9c29b29330?w=200&h=200&fit=crop", // Jessica Style
            [CreatorUser2Id] =
                "https://images.unsplash.com/photo-1517841905240-472988babdf9?w=200&h=200&fit=crop", // Maya Fits
            [CreatorUser3Id] =
                "https://images.unsplash.com/photo-1438761681033-6461ffad8d80?w=200&h=200&fit=crop", // Taylor Petite
            [CreatorUser4Id] =
                "https://images.unsplash.com/photo-1534528741775-53994a69daeb?w=200&h=200&fit=crop", // Priya Looks
        };

        foreach (var (id, userName, email, userType) in users)
        {
            var user = new User
            {
                Id = id,
                UserName = userName,
                Email = email,
                EmailConfirmed = true,
                UserType = userType,
                IsActive = true,
                ProfileImageUrl = profileImages.GetValueOrDefault(id),
            };

            var result = await userManager.CreateAsync(user, DefaultPassword);
            if (!result.Succeeded)
            {
                logger.LogWarning(
                    "Failed to create user {UserName}: {Errors}",
                    userName,
                    string.Join(", ", result.Errors.Select(e => e.Description))
                );
            }
        }
    }

    private static async Task SeedBodyProfilesAsync(LykeDbContext context)
    {
        // New body type IDs: 1=Hourglass, 2=Pear, 3=Apple, 4=Rectangle, 5=Inverted Triangle
        // Frame size IDs: 1=Petite, 2=Average, 3=Tall, 4=Plus
        var profiles = new BodyProfile[]
        {
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000060"),
                UserId = Shopper1Id,
                HeightCm = 165,
                WeightKg = 58m,
                BodyTypeId = 1, // Hourglass
                FrameSizeId = 2, // Average
            },
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000061"),
                UserId = Shopper2Id,
                HeightCm = 170,
                WeightKg = 63m,
                BodyTypeId = 5, // Inverted Triangle (was Athletic)
                FrameSizeId = 2, // Average
            },
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000062"),
                UserId = Shopper3Id,
                HeightCm = 155,
                WeightKg = 50m,
                BodyTypeId = 4, // Rectangle (was Petite — shape now separate)
                FrameSizeId = 1, // Petite
            },
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000063"),
                UserId = Shopper4Id,
                HeightCm = 175,
                WeightKg = 72m,
                BodyTypeId = 2, // Pear
                FrameSizeId = 3, // Tall
            },
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000064"),
                UserId = Shopper5Id,
                HeightCm = 160,
                WeightKg = 55m,
                BodyTypeId = 4, // Rectangle (was Slim — shape now separate)
                FrameSizeId = 2, // Average
            },
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000065"),
                UserId = CreatorUser1Id,
                HeightCm = 168,
                WeightKg = 60m,
                BodyTypeId = 1, // Hourglass
                FrameSizeId = 2, // Average
            },
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000066"),
                UserId = CreatorUser2Id,
                HeightCm = 172,
                WeightKg = 65m,
                BodyTypeId = 5, // Inverted Triangle (was Athletic)
                FrameSizeId = 3, // Tall
            },
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000067"),
                UserId = CreatorUser3Id,
                HeightCm = 158,
                WeightKg = 52m,
                BodyTypeId = 1, // Hourglass (was Petite — shape now separate)
                FrameSizeId = 1, // Petite
            },
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000068"),
                UserId = CreatorUser4Id,
                HeightCm = 163,
                WeightKg = 57m,
                BodyTypeId = 4, // Rectangle
                FrameSizeId = 2, // Average
            },
        };

        context.BodyProfiles.AddRange(profiles);
        await context.SaveChangesAsync();

        // Seed fit preferences (multi-select)
        var fitPreferences = new BodyProfileFitPreference[]
        {
            new()
            {
                BodyProfileId = Guid.Parse("00000000-0000-0000-0000-000000000060"),
                FitPreference = FitPreference.Regular,
            },
            new()
            {
                BodyProfileId = Guid.Parse("00000000-0000-0000-0000-000000000061"),
                FitPreference = FitPreference.Relaxed,
            },
            new()
            {
                BodyProfileId = Guid.Parse("00000000-0000-0000-0000-000000000062"),
                FitPreference = FitPreference.Fitted,
            },
            new()
            {
                BodyProfileId = Guid.Parse("00000000-0000-0000-0000-000000000063"),
                FitPreference = FitPreference.Regular,
            },
            new()
            {
                BodyProfileId = Guid.Parse("00000000-0000-0000-0000-000000000063"),
                FitPreference = FitPreference.Relaxed,
            },
            new()
            {
                BodyProfileId = Guid.Parse("00000000-0000-0000-0000-000000000064"),
                FitPreference = FitPreference.Fitted,
            },
            new()
            {
                BodyProfileId = Guid.Parse("00000000-0000-0000-0000-000000000064"),
                FitPreference = FitPreference.Regular,
            },
            new()
            {
                BodyProfileId = Guid.Parse("00000000-0000-0000-0000-000000000065"),
                FitPreference = FitPreference.Regular,
            },
            new()
            {
                BodyProfileId = Guid.Parse("00000000-0000-0000-0000-000000000066"),
                FitPreference = FitPreference.Relaxed,
            },
            new()
            {
                BodyProfileId = Guid.Parse("00000000-0000-0000-0000-000000000067"),
                FitPreference = FitPreference.Fitted,
            },
            new()
            {
                BodyProfileId = Guid.Parse("00000000-0000-0000-0000-000000000068"),
                FitPreference = FitPreference.Regular,
            },
        };

        context.BodyProfileFitPreferences.AddRange(fitPreferences);
        await context.SaveChangesAsync();
    }

    private static async Task SeedCreatorsAsync(LykeDbContext context)
    {
        var creators = new Creator[]
        {
            new()
            {
                Id = Creator1Id,
                UserId = CreatorUser1Id,
                DisplayName = "Jessica Style",
                Bio = "Fashion lover sharing everyday outfit inspiration for curvy girls",
                IsVerified = true,
                VerificationStatus = VerificationStatus.Approved,
                SocialLinks =
                    "[{\"platform\":\"instagram\",\"url\":\"https://instagram.com/jessicastyle\"}]",
            },
            new()
            {
                Id = Creator2Id,
                UserId = CreatorUser2Id,
                DisplayName = "Maya Fits",
                Bio = "Athletic build, sporty-chic outfits. Size M advocate",
                IsVerified = true,
                VerificationStatus = VerificationStatus.Approved,
                SocialLinks =
                    "[{\"platform\":\"tiktok\",\"url\":\"https://tiktok.com/@mayafits\"}]",
            },
            new()
            {
                Id = Creator3Id,
                UserId = CreatorUser3Id,
                DisplayName = "Taylor Petite",
                Bio = "Petite fashion under 5'3\". Proving style has no height requirement",
                IsVerified = true,
                VerificationStatus = VerificationStatus.Approved,
                SocialLinks =
                    "[{\"platform\":\"instagram\",\"url\":\"https://instagram.com/taylorpetite\"}]",
            },
            new()
            {
                Id = Creator4Id,
                UserId = CreatorUser4Id,
                DisplayName = "Priya Looks",
                Bio = "Mixing South Asian and Western fashion. Bold colors, modern cuts",
                IsVerified = false,
                VerificationStatus = VerificationStatus.Pending,
                VerificationRequestedAt = new DateTime(2026, 1, 20, 0, 0, 0, DateTimeKind.Utc),
            },
        };

        context.Creators.AddRange(creators);
        await context.SaveChangesAsync();
    }

    private static async Task SeedRetailersAsync(LykeDbContext context)
    {
        var retailers = new Retailer[]
        {
            new()
            {
                Id = Retailer1Id,
                UserId = RetailerUser1Id,
                Name = "Zara",
                LogoUrl =
                    "https://images.unsplash.com/photo-1441986300917-64674bd600d8?w=200&h=200&fit=crop",
                WebsiteUrl = "https://www.zara.com",
                ContactEmail = "partners@zara-example.com",
                AffiliateConfig =
                    "{\"commissionRate\":0.08,\"cookieDays\":30,\"network\":\"rakuten\"}",
                IsActive = true,
            },
            new()
            {
                Id = Retailer2Id,
                UserId = RetailerUser2Id,
                Name = "H&M",
                LogoUrl =
                    "https://images.unsplash.com/photo-1441984904996-e0b6ba687e04?w=200&h=200&fit=crop",
                WebsiteUrl = "https://www.hm.com",
                ContactEmail = "partners@hm-example.com",
                AffiliateConfig =
                    "{\"commissionRate\":0.07,\"cookieDays\":14,\"network\":\"awin\"}",
                IsActive = true,
            },
            new()
            {
                Id = Retailer3Id,
                UserId = RetailerUser3Id,
                Name = "ASOS",
                LogoUrl =
                    "https://images.unsplash.com/photo-1472851294608-062f824d29cc?w=200&h=200&fit=crop",
                WebsiteUrl = "https://www.asos.com",
                ContactEmail = "partners@asos-example.com",
                AffiliateConfig =
                    "{\"commissionRate\":0.10,\"cookieDays\":45,\"network\":\"partnerize\"}",
                IsActive = true,
            },
        };

        context.Retailers.AddRange(retailers);
        await context.SaveChangesAsync();
    }

    private static async Task SeedProductsAsync(LykeDbContext context)
    {
        var products = new Product[]
        {
            // Zara products (6)
            new()
            {
                Id = Product1Id,
                RetailerId = Retailer1Id,
                ExternalSku = "ZARA-001",
                Name = "Fitted Blazer",
                Description = "Structured single-breasted blazer with padded shoulders",
                Category = "Outerwear",
                SubCategory = "Blazers",
                ImageUrls =
                    "[\"https://images.unsplash.com/photo-1591047139829-d91aecb6caea?w=400&h=500&fit=crop\"]",
                ProductUrl = "https://www.zara.com/example/blazer",
                Price = 89.99m,
                Currency = "GBP",
                IsActive = true,
            },
            new()
            {
                Id = Product2Id,
                RetailerId = Retailer1Id,
                ExternalSku = "ZARA-002",
                Name = "High-Waist Wide Leg Trousers",
                Description = "Flowing wide-leg trousers with front pleats",
                Category = "Bottoms",
                SubCategory = "Trousers",
                ImageUrls =
                    "[\"https://images.unsplash.com/photo-1594938298603-c8148c4dae35?w=400&h=500&fit=crop\"]",
                ProductUrl = "https://www.zara.com/example/trousers",
                Price = 49.99m,
                Currency = "GBP",
                IsActive = true,
            },
            new()
            {
                Id = Product3Id,
                RetailerId = Retailer1Id,
                ExternalSku = "ZARA-003",
                Name = "Satin Midi Skirt",
                Description = "A-line satin skirt with elastic waistband",
                Category = "Bottoms",
                SubCategory = "Skirts",
                ImageUrls =
                    "[\"https://images.unsplash.com/photo-1583496661160-fb5886a0uj7h?w=400&h=500&fit=crop\"]",
                ProductUrl = "https://www.zara.com/example/skirt",
                Price = 35.99m,
                Currency = "GBP",
                IsActive = true,
            },
            new()
            {
                Id = Product4Id,
                RetailerId = Retailer1Id,
                ExternalSku = "ZARA-004",
                Name = "Ribbed Knit Top",
                Description = "Slim fit ribbed knit top with round neck",
                Category = "Tops",
                SubCategory = "Knitwear",
                ImageUrls =
                    "[\"https://images.unsplash.com/photo-1576566588028-4147f3842f27?w=400&h=500&fit=crop\"]",
                ProductUrl = "https://www.zara.com/example/knit",
                Price = 25.99m,
                Currency = "GBP",
                IsActive = true,
            },
            new()
            {
                Id = Product5Id,
                RetailerId = Retailer1Id,
                ExternalSku = "ZARA-005",
                Name = "Floral Print Dress",
                Description = "V-neck midi dress with floral print and ruched detail",
                Category = "Dresses",
                SubCategory = "Midi",
                ImageUrls =
                    "[\"https://images.unsplash.com/photo-1572804013309-59a88b7e92f1?w=400&h=500&fit=crop\"]",
                ProductUrl = "https://www.zara.com/example/dress",
                Price = 59.99m,
                Currency = "GBP",
                IsActive = true,
            },
            new()
            {
                Id = Product6Id,
                RetailerId = Retailer1Id,
                ExternalSku = "ZARA-006",
                Name = "Leather Belt Bag",
                Description = "Compact leather belt bag with adjustable strap",
                Category = "Accessories",
                SubCategory = "Bags",
                ImageUrls =
                    "[\"https://images.unsplash.com/photo-1548036328-c9fa89d128fa?w=400&h=500&fit=crop\"]",
                ProductUrl = "https://www.zara.com/example/bag",
                Price = 29.99m,
                Currency = "GBP",
                IsActive = true,
            },
            // H&M products (6)
            new()
            {
                Id = Product7Id,
                RetailerId = Retailer2Id,
                ExternalSku = "HM-001",
                Name = "Oversized Hoodie",
                Description = "Relaxed fit cotton hoodie with kangaroo pocket",
                Category = "Tops",
                SubCategory = "Hoodies",
                ImageUrls =
                    "[\"https://images.unsplash.com/photo-1556821840-3a63f95609a7?w=400&h=500&fit=crop\"]",
                ProductUrl = "https://www.hm.com/example/hoodie",
                Price = 24.99m,
                Currency = "GBP",
                IsActive = true,
            },
            new()
            {
                Id = Product8Id,
                RetailerId = Retailer2Id,
                ExternalSku = "HM-002",
                Name = "Straight Leg Jeans",
                Description = "Classic straight leg jeans in medium wash",
                Category = "Bottoms",
                SubCategory = "Jeans",
                ImageUrls =
                    "[\"https://images.unsplash.com/photo-1541099649105-f69ad21f3246?w=400&h=500&fit=crop\"]",
                ProductUrl = "https://www.hm.com/example/jeans",
                Price = 34.99m,
                Currency = "GBP",
                IsActive = true,
            },
            new()
            {
                Id = Product9Id,
                RetailerId = Retailer2Id,
                ExternalSku = "HM-003",
                Name = "Cropped Cardigan",
                Description = "Soft-knit cropped cardigan with pearl buttons",
                Category = "Tops",
                SubCategory = "Knitwear",
                ImageUrls =
                    "[\"https://images.unsplash.com/photo-1434389677669-e08b4cda3a41?w=400&h=500&fit=crop\"]",
                ProductUrl = "https://www.hm.com/example/cardigan",
                Price = 19.99m,
                Currency = "GBP",
                IsActive = true,
            },
            new()
            {
                Id = Product10Id,
                RetailerId = Retailer2Id,
                ExternalSku = "HM-004",
                Name = "Puffer Jacket",
                Description = "Lightweight quilted puffer jacket with stand collar",
                Category = "Outerwear",
                SubCategory = "Jackets",
                ImageUrls =
                    "[\"https://images.unsplash.com/photo-1544923246-77307dd270cb?w=400&h=500&fit=crop\"]",
                ProductUrl = "https://www.hm.com/example/puffer",
                Price = 44.99m,
                Currency = "GBP",
                IsActive = true,
            },
            new()
            {
                Id = Product11Id,
                RetailerId = Retailer2Id,
                ExternalSku = "HM-005",
                Name = "Linen Blend Shorts",
                Description = "High-waist linen blend shorts with belt",
                Category = "Bottoms",
                SubCategory = "Shorts",
                ImageUrls =
                    "[\"https://images.unsplash.com/photo-1591195853828-11db59a44f6b?w=400&h=500&fit=crop\"]",
                ProductUrl = "https://www.hm.com/example/shorts",
                Price = 22.99m,
                Currency = "GBP",
                IsActive = true,
            },
            new()
            {
                Id = Product12Id,
                RetailerId = Retailer2Id,
                ExternalSku = "HM-006",
                Name = "Cotton T-Shirt Dress",
                Description = "Relaxed t-shirt dress in organic cotton",
                Category = "Dresses",
                SubCategory = "Mini",
                ImageUrls =
                    "[\"https://images.unsplash.com/photo-1515372039744-b8f02a3ae446?w=400&h=500&fit=crop\"]",
                ProductUrl = "https://www.hm.com/example/tshirtdress",
                Price = 17.99m,
                Currency = "GBP",
                IsActive = true,
            },
            // ASOS products (6)
            new()
            {
                Id = Product13Id,
                RetailerId = Retailer3Id,
                ExternalSku = "ASOS-001",
                Name = "Wrap Maxi Dress",
                Description = "Jersey wrap maxi dress with long sleeves",
                Category = "Dresses",
                SubCategory = "Maxi",
                ImageUrls =
                    "[\"https://images.unsplash.com/photo-1495385794356-15371f348c31?w=400&h=500&fit=crop\"]",
                ProductUrl = "https://www.asos.com/example/maxidress",
                Price = 42.00m,
                Currency = "GBP",
                IsActive = true,
            },
            new()
            {
                Id = Product14Id,
                RetailerId = Retailer3Id,
                ExternalSku = "ASOS-002",
                Name = "Tailored Trousers",
                Description = "Slim-fit tailored trousers with pressed crease",
                Category = "Bottoms",
                SubCategory = "Trousers",
                ImageUrls =
                    "[\"https://images.unsplash.com/photo-1506629082955-511b1aa562c8?w=400&h=500&fit=crop\"]",
                ProductUrl = "https://www.asos.com/example/trousers",
                Price = 36.00m,
                Currency = "GBP",
                IsActive = true,
            },
            new()
            {
                Id = Product15Id,
                RetailerId = Retailer3Id,
                ExternalSku = "ASOS-003",
                Name = "Mesh Bodysuit",
                Description = "Fitted mesh bodysuit with high neck",
                Category = "Tops",
                SubCategory = "Bodysuits",
                ImageUrls =
                    "[\"https://images.unsplash.com/photo-1515886657613-9f3515b0c78f?w=400&h=500&fit=crop\"]",
                ProductUrl = "https://www.asos.com/example/bodysuit",
                Price = 18.00m,
                Currency = "GBP",
                IsActive = true,
            },
            new()
            {
                Id = Product16Id,
                RetailerId = Retailer3Id,
                ExternalSku = "ASOS-004",
                Name = "Denim Jacket",
                Description = "Classic denim jacket in mid-wash blue",
                Category = "Outerwear",
                SubCategory = "Jackets",
                ImageUrls =
                    "[\"https://images.unsplash.com/photo-1578587018452-892bacefd3f2?w=400&h=500&fit=crop\"]",
                ProductUrl = "https://www.asos.com/example/denim",
                Price = 45.00m,
                Currency = "GBP",
                IsActive = true,
            },
            new()
            {
                Id = Product17Id,
                RetailerId = Retailer3Id,
                ExternalSku = "ASOS-005",
                Name = "Pleated Midi Skirt",
                Description = "Satin pleated midi skirt in emerald green",
                Category = "Bottoms",
                SubCategory = "Skirts",
                ImageUrls =
                    "[\"https://images.unsplash.com/photo-1583496661160-fb5886a0aaef?w=400&h=500&fit=crop\"]",
                ProductUrl = "https://www.asos.com/example/pleated",
                Price = 32.00m,
                Currency = "GBP",
                IsActive = true,
            },
            new()
            {
                Id = Product18Id,
                RetailerId = Retailer3Id,
                ExternalSku = "ASOS-006",
                Name = "Chunky Trainers",
                Description = "Platform chunky trainers in white leather",
                Category = "Footwear",
                SubCategory = "Trainers",
                ImageUrls =
                    "[\"https://images.unsplash.com/photo-1595950653106-6c9ebd614d3a?w=400&h=500&fit=crop\"]",
                ProductUrl = "https://www.asos.com/example/trainers",
                Price = 55.00m,
                Currency = "GBP",
                IsActive = true,
            },
        };

        context.Products.AddRange(products);
        await context.SaveChangesAsync();
    }

    private static async Task SeedPostsAsync(LykeDbContext context)
    {
        var baseDate = new DateTime(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc);

        var posts = new Post[]
        {
            // Creator 1 - Jessica Style (hourglass, curvy fashion)
            new()
            {
                Id = Post1Id,
                CreatorId = Creator1Id,
                Title = "Office Power Look",
                Description =
                    "My go-to blazer and trouser combo for important meetings. The blazer nips in at the waist perfectly!",
                MediaType = MediaType.Image,
                MediaUrls =
                    "[\"https://images.unsplash.com/photo-1487222477894-8943e31ef7b2?w=400&h=600&fit=crop\",\"https://images.unsplash.com/photo-1496747611176-843222e1e57c?w=400&h=600&fit=crop\"]",
                Status = PostStatus.Published,
                PublishedAt = baseDate,
            },
            new()
            {
                Id = Post2Id,
                CreatorId = Creator1Id,
                Title = "Date Night Outfit",
                Description =
                    "Satin skirt with a knit top — elevated but comfy. The elastic waist is so forgiving after dinner!",
                MediaType = MediaType.Image,
                MediaUrls =
                    "[\"https://images.unsplash.com/photo-1509631179647-0177331693ae?w=400&h=600&fit=crop\",\"https://images.unsplash.com/photo-1485968579580-b6d095142e6e?w=400&h=600&fit=crop\"]",
                Status = PostStatus.Published,
                PublishedAt = baseDate.AddDays(2),
            },
            new()
            {
                Id = Post3Id,
                CreatorId = Creator1Id,
                Title = "Weekend Brunch Fit",
                Description =
                    "Floral dress season is here! This one is amazing for hourglass shapes — wraps right at the waist",
                MediaType = MediaType.Image,
                MediaUrls =
                    "[\"https://images.unsplash.com/photo-1490481651871-ab68de25d43d?w=400&h=600&fit=crop\"]",
                Status = PostStatus.Published,
                PublishedAt = baseDate.AddDays(5),
            },
            // Creator 2 - Maya Fits (athletic, sporty-chic)
            new()
            {
                Id = Post4Id,
                CreatorId = Creator2Id,
                Title = "Athleisure to Brunch",
                Description =
                    "Hoodie and jeans but make it fashion. These straight leg jeans are perfect for athletic thighs",
                MediaType = MediaType.Image,
                MediaUrls =
                    "[\"https://images.unsplash.com/photo-1552374196-1ab2a1c593e8?w=400&h=600&fit=crop\",\"https://images.unsplash.com/photo-1515886657613-9f3515b0c78f?w=400&h=600&fit=crop\"]",
                Status = PostStatus.Published,
                PublishedAt = baseDate.AddDays(1),
            },
            new()
            {
                Id = Post5Id,
                CreatorId = Creator2Id,
                Title = "Gym to Street",
                Description =
                    "Puffer jacket over workout gear — instant outfit upgrade. This puffer fits broad shoulders great",
                MediaType = MediaType.Image,
                MediaUrls =
                    "[\"https://images.unsplash.com/photo-1515886657613-9f3515b0c78f?w=400&h=600&fit=crop\"]",
                Status = PostStatus.Published,
                PublishedAt = baseDate.AddDays(3),
            },
            new()
            {
                Id = Post6Id,
                CreatorId = Creator2Id,
                Title = "Summer Active Look",
                Description =
                    "Linen shorts and a cropped cardigan for warm weather workouts and coffee runs",
                MediaType = MediaType.Image,
                MediaUrls =
                    "[\"https://images.unsplash.com/photo-1515372039744-b8f02a3ae446?w=400&h=600&fit=crop\"]",
                Status = PostStatus.Published,
                PublishedAt = baseDate.AddDays(7),
            },
            // Creator 3 - Taylor Petite (petite fashion)
            new()
            {
                Id = Post7Id,
                CreatorId = Creator3Id,
                Title = "Petite-Friendly Maxi",
                Description =
                    "Yes, short girls CAN wear maxi dresses! This wrap style doesn't drown me at 5'2\"",
                MediaType = MediaType.Image,
                MediaUrls =
                    "[\"https://images.unsplash.com/photo-1495385794356-15371f348c31?w=400&h=600&fit=crop\"]",
                Status = PostStatus.Published,
                PublishedAt = baseDate.AddDays(2),
            },
            new()
            {
                Id = Post8Id,
                CreatorId = Creator3Id,
                Title = "Cropped Everything",
                Description =
                    "Cropped cardigan is a petite girl's best friend — no awkward length issues!",
                MediaType = MediaType.Image,
                MediaUrls =
                    "[\"https://images.unsplash.com/photo-1469334031218-e382a71b716b?w=400&h=600&fit=crop\"]",
                Status = PostStatus.Published,
                PublishedAt = baseDate.AddDays(4),
            },
            new()
            {
                Id = Post9Id,
                CreatorId = Creator3Id,
                Title = "Mini Skirt Moment",
                Description =
                    "Pleated skirts hit at the perfect length on petite frames. Paired with a bodysuit underneath",
                MediaType = MediaType.Image,
                MediaUrls =
                    "[\"https://images.unsplash.com/photo-1515886657613-9f3515b0c78f?w=400&h=600&fit=crop\"]",
                Status = PostStatus.Published,
                PublishedAt = baseDate.AddDays(6),
            },
            // Creator 4 - Priya Looks (bold colors, modern cuts)
            new()
            {
                Id = Post10Id,
                CreatorId = Creator4Id,
                Title = "Bold Colour Blocking",
                Description =
                    "Emerald green skirt with a mesh top — turning heads at every party this season",
                MediaType = MediaType.Image,
                MediaUrls =
                    "[\"https://images.unsplash.com/photo-1515886657613-9f3515b0c78f?w=400&h=600&fit=crop\"]",
                Status = PostStatus.Published,
                PublishedAt = baseDate.AddDays(3),
            },
            new()
            {
                Id = Post11Id,
                CreatorId = Creator4Id,
                Title = "Denim on Denim",
                Description =
                    "Double denim is BACK. Denim jacket with tailored trousers — casual but put together",
                MediaType = MediaType.Image,
                MediaUrls =
                    "[\"https://images.unsplash.com/photo-1578587018452-892bacefd3f2?w=400&h=600&fit=crop\"]",
                Status = PostStatus.Published,
                PublishedAt = baseDate.AddDays(5),
            },
            new()
            {
                Id = Post12Id,
                CreatorId = Creator4Id,
                Title = "Chunky Shoe Era",
                Description =
                    "Styled these chunky trainers with a t-shirt dress. Comfort meets cool",
                MediaType = MediaType.Image,
                MediaUrls =
                    "[\"https://images.unsplash.com/photo-1595950653106-6c9ebd614d3a?w=400&h=600&fit=crop\",\"https://images.unsplash.com/photo-1515372039744-b8f02a3ae446?w=400&h=600&fit=crop\"]",
                Status = PostStatus.Published,
                PublishedAt = baseDate.AddDays(8),
            },
        };

        context.Posts.AddRange(posts);
        await context.SaveChangesAsync();
    }

    private static async Task SeedPostProductsAsync(LykeDbContext context)
    {
        var postProducts = new PostProduct[]
        {
            // Post 1 - Office Power Look (Jessica): Blazer + Trousers
            new()
            {
                Id = PostProduct1Id,
                PostId = Post1Id,
                ProductId = Product1Id, // Zara Blazer
                SizeWorn = "M",
                FitNotes = "Fits perfectly at the waist, slightly snug on bust",
                FitRating = FitRating.TrueToSize,
            },
            new()
            {
                Id = PostProduct2Id,
                PostId = Post1Id,
                ProductId = Product2Id, // Zara Trousers
                SizeWorn = "M",
                FitNotes = "High waist sits perfectly, generous in the hip",
                FitRating = FitRating.TrueToSize,
            },
            // Post 2 - Date Night (Jessica): Skirt + Knit Top
            new()
            {
                Id = PostProduct3Id,
                PostId = Post2Id,
                ProductId = Product3Id, // Zara Satin Skirt
                SizeWorn = "M",
                FitNotes = "Elastic waist is very forgiving, beautiful drape",
                FitRating = FitRating.TrueToSize,
            },
            new()
            {
                Id = PostProduct4Id,
                PostId = Post2Id,
                ProductId = Product4Id, // Zara Knit Top
                SizeWorn = "M",
                FitNotes = "Ribbed material hugs curves nicely, size up if busty",
                FitRating = FitRating.SlightlySmall,
            },
            // Post 3 - Weekend Brunch (Jessica): Floral Dress + Belt Bag
            new()
            {
                Id = PostProduct5Id,
                PostId = Post3Id,
                ProductId = Product5Id, // Zara Floral Dress
                SizeWorn = "M",
                FitNotes = "V-neck is flattering, ruching hides tummy",
                FitRating = FitRating.TrueToSize,
            },
            new()
            {
                Id = PostProduct6Id,
                PostId = Post3Id,
                ProductId = Product6Id, // Zara Belt Bag
                SizeWorn = "OS",
                FitNotes = "Adjustable strap fits all sizes",
                FitRating = FitRating.TrueToSize,
            },
            // Post 4 - Athleisure (Maya): Hoodie + Jeans
            new()
            {
                Id = PostProduct7Id,
                PostId = Post4Id,
                ProductId = Product7Id, // H&M Hoodie
                SizeWorn = "L",
                FitNotes = "Sized up for that oversized look, great on broad shoulders",
                FitRating = FitRating.TrueToSize,
            },
            new()
            {
                Id = PostProduct8Id,
                PostId = Post4Id,
                ProductId = Product8Id, // H&M Jeans
                SizeWorn = "L",
                FitNotes = "Straight leg gives room for athletic thighs, no stretch",
                FitRating = FitRating.SlightlySmall,
            },
            // Post 5 - Gym to Street (Maya): Puffer Jacket + T-Shirt Dress
            new()
            {
                Id = PostProduct9Id,
                PostId = Post5Id,
                ProductId = Product10Id, // H&M Puffer
                SizeWorn = "M",
                FitNotes = "Fits shoulders well without being too boxy",
                FitRating = FitRating.TrueToSize,
            },
            new()
            {
                Id = PostProduct10Id,
                PostId = Post5Id,
                ProductId = Product12Id, // H&M T-Shirt Dress
                SizeWorn = "M",
                FitNotes = "Relaxed fit, great for layering under jacket",
                FitRating = FitRating.SlightlyLarge,
            },
            // Post 6 - Summer Active (Maya): Shorts + Cardigan
            new()
            {
                Id = PostProduct11Id,
                PostId = Post6Id,
                ProductId = Product11Id, // H&M Shorts
                SizeWorn = "M",
                FitNotes = "Belt helps define waist, good length on taller frame",
                FitRating = FitRating.TrueToSize,
            },
            new()
            {
                Id = PostProduct12Id,
                PostId = Post6Id,
                ProductId = Product9Id, // H&M Cardigan
                SizeWorn = "M",
                FitNotes = "Cropped length hits at the perfect spot",
                FitRating = FitRating.TrueToSize,
            },
            // Post 7 - Petite-Friendly Maxi (Taylor): Wrap Dress + Trainers
            new()
            {
                Id = PostProduct13Id,
                PostId = Post7Id,
                ProductId = Product13Id, // ASOS Wrap Maxi
                SizeWorn = "XS",
                FitNotes = "Wrap style creates waist definition, hemmed 2 inches",
                FitRating = FitRating.SlightlyLarge,
            },
            new()
            {
                Id = PostProduct14Id,
                PostId = Post7Id,
                ProductId = Product18Id, // ASOS Trainers
                SizeWorn = "UK 4",
                FitNotes = "Platform adds height! True to size",
                FitRating = FitRating.TrueToSize,
            },
            // Post 8 - Cropped Everything (Taylor): Cardigan + Jeans
            new()
            {
                Id = PostProduct15Id,
                PostId = Post8Id,
                ProductId = Product9Id, // H&M Cardigan
                SizeWorn = "XS",
                FitNotes = "Cropped length is perfect for petite — no awkward bunching",
                FitRating = FitRating.TrueToSize,
            },
            new()
            {
                Id = PostProduct16Id,
                PostId = Post8Id,
                ProductId = Product8Id, // H&M Jeans
                SizeWorn = "XS",
                FitNotes = "Had to hem these but the fit through hips is great",
                FitRating = FitRating.SlightlyLarge,
            },
            // Post 9 - Mini Skirt Moment (Taylor): Pleated Skirt + Bodysuit
            new()
            {
                Id = PostProduct17Id,
                PostId = Post9Id,
                ProductId = Product17Id, // ASOS Pleated Skirt
                SizeWorn = "XS",
                FitNotes =
                    "Hits right above the knee on my petite frame — midi on me is like a maxi",
                FitRating = FitRating.TrueToSize,
            },
            new()
            {
                Id = PostProduct18Id,
                PostId = Post9Id,
                ProductId = Product15Id, // ASOS Bodysuit
                SizeWorn = "XS",
                FitNotes = "High neck is flattering, mesh is stretchy enough",
                FitRating = FitRating.TrueToSize,
            },
            // Post 10 - Bold Colour Blocking (Priya): Pleated Skirt + Bodysuit
            new()
            {
                Id = PostProduct19Id,
                PostId = Post10Id,
                ProductId = Product17Id, // ASOS Pleated Skirt
                SizeWorn = "S",
                FitNotes = "Emerald green is stunning, waistband sits comfortably",
                FitRating = FitRating.TrueToSize,
            },
            new()
            {
                Id = PostProduct20Id,
                PostId = Post10Id,
                ProductId = Product15Id, // ASOS Bodysuit
                SizeWorn = "S",
                FitNotes = "Layered under the skirt, mesh adds a cool edge",
                FitRating = FitRating.TrueToSize,
            },
            // Post 11 - Denim on Denim (Priya): Denim Jacket + Tailored Trousers
            new()
            {
                Id = PostProduct21Id,
                PostId = Post11Id,
                ProductId = Product16Id, // ASOS Denim Jacket
                SizeWorn = "S",
                FitNotes = "Classic fit, works well for layering",
                FitRating = FitRating.TrueToSize,
            },
            new()
            {
                Id = PostProduct22Id,
                PostId = Post11Id,
                ProductId = Product14Id, // ASOS Tailored Trousers
                SizeWorn = "S",
                FitNotes = "Slim fit through the leg, pressed crease is sharp",
                FitRating = FitRating.TrueToSize,
            },
            // Post 12 - Chunky Shoe Era (Priya): Trainers + T-Shirt Dress
            new()
            {
                Id = PostProduct23Id,
                PostId = Post12Id,
                ProductId = Product18Id, // ASOS Trainers
                SizeWorn = "UK 6",
                FitNotes = "These run slightly wide but the platform is everything",
                FitRating = FitRating.SlightlyLarge,
            },
            new()
            {
                Id = PostProduct24Id,
                PostId = Post12Id,
                ProductId = Product12Id, // H&M T-Shirt Dress
                SizeWorn = "S",
                FitNotes = "Relaxed and comfy, paired with chunky shoes for contrast",
                FitRating = FitRating.TrueToSize,
            },
        };

        context.PostProducts.AddRange(postProducts);
        await context.SaveChangesAsync();
    }

    private static async Task EnsureFitTagsExistAsync(LykeDbContext context)
    {
        if (await context.Set<FitTag>().AnyAsync())
            return;

        // FitTags are normally seeded via HasData in the migration, but if missing we insert them here
        context
            .Set<FitTag>()
            .AddRange(
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
                    Category = "Fit",
                    IsActive = true,
                },
                new FitTag
                {
                    Id = 5,
                    Name = "Tight on bust",
                    Category = "Fit",
                    IsActive = true,
                },
                new FitTag
                {
                    Id = 6,
                    Name = "Loose on waist",
                    Category = "Fit",
                    IsActive = true,
                },
                new FitTag
                {
                    Id = 7,
                    Name = "Long in arms",
                    Category = "Length",
                    IsActive = true,
                },
                new FitTag
                {
                    Id = 8,
                    Name = "Short in arms",
                    Category = "Length",
                    IsActive = true,
                },
                new FitTag
                {
                    Id = 9,
                    Name = "Long in torso",
                    Category = "Length",
                    IsActive = true,
                },
                new FitTag
                {
                    Id = 10,
                    Name = "Short in torso",
                    Category = "Length",
                    IsActive = true,
                },
                new FitTag
                {
                    Id = 11,
                    Name = "Stretchy material",
                    Category = "Material",
                    IsActive = true,
                },
                new FitTag
                {
                    Id = 12,
                    Name = "Not stretchy",
                    Category = "Material",
                    IsActive = true,
                }
            );
        await context.SaveChangesAsync();
    }

    private static async Task SeedPostFitTagsAsync(LykeDbContext context)
    {
        await EnsureFitTagsExistAsync(context);

        // FitTag IDs: 1=True to size, 2=Runs small, 3=Runs large,
        // 4=Tight on hips, 5=Tight on bust, 6=Loose on waist, 7=Long in arms,
        // 8=Short in arms, 9=Long in torso, 10=Short in torso, 11=Stretchy, 12=Not stretchy
        var fitTags = new PostFitTag[]
        {
            // Post 1 products
            new() { PostProductId = PostProduct1Id, FitTagId = 1 }, // Blazer: True to size
            new() { PostProductId = PostProduct1Id, FitTagId = 5 }, // Blazer: Tight on bust
            new() { PostProductId = PostProduct2Id, FitTagId = 1 }, // Trousers: True to size
            // Post 2 products
            new() { PostProductId = PostProduct3Id, FitTagId = 1 }, // Skirt: True to size
            new() { PostProductId = PostProduct3Id, FitTagId = 11 }, // Skirt: Stretchy
            new() { PostProductId = PostProduct4Id, FitTagId = 2 }, // Knit Top: Runs small
            new() { PostProductId = PostProduct4Id, FitTagId = 5 }, // Knit Top: Tight on bust
            // Post 3 products
            new() { PostProductId = PostProduct5Id, FitTagId = 1 }, // Dress: True to size
            new() { PostProductId = PostProduct5Id, FitTagId = 11 }, // Dress: Stretchy
            new() { PostProductId = PostProduct6Id, FitTagId = 1 }, // Belt Bag: True to size
            // Post 4 products
            new() { PostProductId = PostProduct7Id, FitTagId = 1 }, // Hoodie: True to size
            new() { PostProductId = PostProduct7Id, FitTagId = 11 }, // Hoodie: Stretchy
            new() { PostProductId = PostProduct8Id, FitTagId = 2 }, // Jeans: Runs small
            new() { PostProductId = PostProduct8Id, FitTagId = 4 }, // Jeans: Tight on hips
            new() { PostProductId = PostProduct8Id, FitTagId = 12 }, // Jeans: Not stretchy
            // Post 5 products
            new() { PostProductId = PostProduct9Id, FitTagId = 1 }, // Puffer: True to size
            new() { PostProductId = PostProduct10Id, FitTagId = 3 }, // T-Shirt Dress: Runs large
            // Post 6 products
            new() { PostProductId = PostProduct11Id, FitTagId = 1 }, // Shorts: True to size
            new() { PostProductId = PostProduct12Id, FitTagId = 1 }, // Cardigan: True to size
            new() { PostProductId = PostProduct12Id, FitTagId = 11 }, // Cardigan: Stretchy
            // Post 7 products
            new() { PostProductId = PostProduct13Id, FitTagId = 3 }, // Maxi: Runs large
            new() { PostProductId = PostProduct13Id, FitTagId = 9 }, // Maxi: Long in torso
            new() { PostProductId = PostProduct14Id, FitTagId = 1 }, // Trainers: True to size
            // Post 8 products
            new() { PostProductId = PostProduct15Id, FitTagId = 1 }, // Cardigan: True to size
            new() { PostProductId = PostProduct16Id, FitTagId = 3 }, // Jeans: Runs large
            new() { PostProductId = PostProduct16Id, FitTagId = 9 }, // Jeans: Long in torso
            // Post 9 products
            new() { PostProductId = PostProduct17Id, FitTagId = 1 }, // Pleated Skirt: True to size
            new() { PostProductId = PostProduct18Id, FitTagId = 1 }, // Bodysuit: True to size
            new() { PostProductId = PostProduct18Id, FitTagId = 11 }, // Bodysuit: Stretchy
            // Post 10 products
            new() { PostProductId = PostProduct19Id, FitTagId = 1 }, // Pleated Skirt: True to size
            new() { PostProductId = PostProduct20Id, FitTagId = 1 }, // Bodysuit: True to size
            new() { PostProductId = PostProduct20Id, FitTagId = 11 }, // Bodysuit: Stretchy
            // Post 11 products
            new() { PostProductId = PostProduct21Id, FitTagId = 1 }, // Denim Jacket: True to size
            new() { PostProductId = PostProduct21Id, FitTagId = 12 }, // Denim Jacket: Not stretchy
            new() { PostProductId = PostProduct22Id, FitTagId = 1 }, // Tailored Trousers: True to size
            // Post 12 products
            new() { PostProductId = PostProduct23Id, FitTagId = 3 }, // Trainers: Runs large
            new() { PostProductId = PostProduct24Id, FitTagId = 1 }, // T-Shirt Dress: True to size
            new() { PostProductId = PostProduct24Id, FitTagId = 11 }, // T-Shirt Dress: Stretchy
        };

        context.PostFitTags.AddRange(fitTags);
        await context.SaveChangesAsync();
    }

    private static async Task SeedEngagementsAsync(LykeDbContext context)
    {
        var engagements = new List<Engagement>();
        var baseDate = new DateTime(2026, 1, 16, 10, 0, 0, DateTimeKind.Utc);
        var shopperIds = new[] { Shopper1Id, Shopper2Id, Shopper3Id, Shopper4Id, Shopper5Id };
        var postIds = new[]
        {
            Post1Id,
            Post2Id,
            Post3Id,
            Post4Id,
            Post5Id,
            Post6Id,
            Post7Id,
            Post8Id,
            Post9Id,
            Post10Id,
            Post11Id,
            Post12Id,
        };

        var guidCounter = 0x400;

        // Every shopper views every post
        foreach (var shopperId in shopperIds)
        {
            foreach (var postId in postIds)
            {
                engagements.Add(
                    new Engagement
                    {
                        Id = Guid.Parse($"00000000-0000-0000-0000-000000000{guidCounter++:X3}"),
                        UserId = shopperId,
                        PostId = postId,
                        Type = EngagementType.View,
                        CreatedAt = baseDate.AddHours(guidCounter % 48),
                    }
                );
            }
        }

        // Likes — each shopper likes ~4-6 random posts
        var likePatterns = new (int shopperIdx, int[] postIndices)[]
        {
            (0, [0, 1, 2, 4, 6]), // Emma likes Jessica's + Maya's + Taylor's
            (1, [3, 4, 5, 9, 10]), // Olivia likes Maya's + Priya's
            (2, [0, 6, 7, 8, 11]), // Ava likes Jessica's + Taylor's + Priya's
            (3, [1, 3, 7, 10, 11]), // Mia likes mixed
            (4, [2, 5, 8, 9]), // Sophia likes mixed
        };

        foreach (var (shopperIdx, postIndices) in likePatterns)
        {
            foreach (var postIdx in postIndices)
            {
                engagements.Add(
                    new Engagement
                    {
                        Id = Guid.Parse($"00000000-0000-0000-0000-000000000{guidCounter++:X3}"),
                        UserId = shopperIds[shopperIdx],
                        PostId = postIds[postIdx],
                        Type = EngagementType.Like,
                        CreatedAt = baseDate.AddDays(1).AddHours(guidCounter % 24),
                    }
                );
            }
        }

        // Saves — fewer than likes
        var savePatterns = new (int shopperIdx, int[] postIndices)[]
        {
            (0, [0, 6]), // Emma saves office look + petite maxi
            (1, [3, 4]), // Olivia saves athleisure + gym
            (2, [7, 8]), // Ava saves cropped + mini skirt
            (3, [1, 10]), // Mia saves date night + denim
            (4, [9, 11]), // Sophia saves colour blocking + chunky shoes
        };

        foreach (var (shopperIdx, postIndices) in savePatterns)
        {
            foreach (var postIdx in postIndices)
            {
                engagements.Add(
                    new Engagement
                    {
                        Id = Guid.Parse($"00000000-0000-0000-0000-000000000{guidCounter++:X3}"),
                        UserId = shopperIds[shopperIdx],
                        PostId = postIds[postIdx],
                        Type = EngagementType.Save,
                        CreatedAt = baseDate.AddDays(2).AddHours(guidCounter % 24),
                    }
                );
            }
        }

        context.Engagements.AddRange(engagements);
        await context.SaveChangesAsync();
    }

    private static async Task SeedClickEventsAsync(LykeDbContext context)
    {
        var baseDate = new DateTime(2026, 1, 18, 14, 0, 0, DateTimeKind.Utc);
        var shopperIds = new[] { Shopper1Id, Shopper2Id, Shopper3Id, Shopper4Id, Shopper5Id };

        var clickEvents = new ClickEvent[]
        {
            // Clicks on Jessica's posts
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000500"),
                UserId = shopperIds[0],
                PostId = Post1Id,
                PostProductId = PostProduct1Id, // Blazer
                SessionId = "sess-001",
                CreatedAt = baseDate,
                ConvertedAt = baseDate.AddHours(2),
            },
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000501"),
                UserId = shopperIds[2],
                PostId = Post1Id,
                PostProductId = PostProduct2Id, // Trousers
                SessionId = "sess-002",
                CreatedAt = baseDate.AddHours(3),
            },
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000502"),
                UserId = shopperIds[1],
                PostId = Post2Id,
                PostProductId = PostProduct3Id, // Skirt
                SessionId = "sess-003",
                CreatedAt = baseDate.AddDays(1),
                ConvertedAt = baseDate.AddDays(1).AddHours(5),
            },
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000503"),
                UserId = shopperIds[3],
                PostId = Post3Id,
                PostProductId = PostProduct5Id, // Floral Dress
                SessionId = "sess-004",
                CreatedAt = baseDate.AddDays(2),
            },
            // Clicks on Maya's posts
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000504"),
                UserId = shopperIds[1],
                PostId = Post4Id,
                PostProductId = PostProduct7Id, // Hoodie
                SessionId = "sess-005",
                CreatedAt = baseDate.AddDays(1).AddHours(2),
                ConvertedAt = baseDate.AddDays(1).AddHours(8),
            },
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000505"),
                UserId = shopperIds[4],
                PostId = Post4Id,
                PostProductId = PostProduct8Id, // Jeans
                SessionId = "sess-006",
                CreatedAt = baseDate.AddDays(2).AddHours(1),
            },
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000506"),
                UserId = shopperIds[0],
                PostId = Post5Id,
                PostProductId = PostProduct9Id, // Puffer
                SessionId = "sess-007",
                CreatedAt = baseDate.AddDays(3),
                ConvertedAt = baseDate.AddDays(3).AddHours(4),
            },
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000507"),
                UserId = shopperIds[3],
                PostId = Post6Id,
                PostProductId = PostProduct11Id, // Shorts
                SessionId = "sess-008",
                CreatedAt = baseDate.AddDays(4),
            },
            // Clicks on Taylor's posts
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000508"),
                UserId = shopperIds[2],
                PostId = Post7Id,
                PostProductId = PostProduct13Id, // Wrap Maxi
                SessionId = "sess-009",
                CreatedAt = baseDate.AddDays(2).AddHours(5),
                ConvertedAt = baseDate.AddDays(3).AddHours(1),
            },
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000509"),
                UserId = shopperIds[4],
                PostId = Post7Id,
                PostProductId = PostProduct14Id, // Trainers
                SessionId = "sess-010",
                CreatedAt = baseDate.AddDays(3).AddHours(2),
            },
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000510"),
                UserId = shopperIds[0],
                PostId = Post8Id,
                PostProductId = PostProduct15Id, // Cardigan
                SessionId = "sess-011",
                CreatedAt = baseDate.AddDays(4).AddHours(1),
            },
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000511"),
                UserId = shopperIds[1],
                PostId = Post9Id,
                PostProductId = PostProduct17Id, // Pleated Skirt
                SessionId = "sess-012",
                CreatedAt = baseDate.AddDays(4).AddHours(6),
                ConvertedAt = baseDate.AddDays(5).AddHours(2),
            },
            // Clicks on Priya's posts
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000512"),
                UserId = shopperIds[3],
                PostId = Post10Id,
                PostProductId = PostProduct19Id, // Pleated Skirt
                SessionId = "sess-013",
                CreatedAt = baseDate.AddDays(3).AddHours(4),
                ConvertedAt = baseDate.AddDays(4).AddHours(1),
            },
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000513"),
                UserId = shopperIds[4],
                PostId = Post10Id,
                PostProductId = PostProduct20Id, // Bodysuit
                SessionId = "sess-014",
                CreatedAt = baseDate.AddDays(4).AddHours(2),
            },
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000514"),
                UserId = shopperIds[0],
                PostId = Post11Id,
                PostProductId = PostProduct21Id, // Denim Jacket
                SessionId = "sess-015",
                CreatedAt = baseDate.AddDays(5),
            },
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000515"),
                UserId = shopperIds[2],
                PostId = Post11Id,
                PostProductId = PostProduct22Id, // Tailored Trousers
                SessionId = "sess-016",
                CreatedAt = baseDate.AddDays(5).AddHours(3),
                ConvertedAt = baseDate.AddDays(6),
            },
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000516"),
                UserId = shopperIds[1],
                PostId = Post12Id,
                PostProductId = PostProduct23Id, // Trainers
                SessionId = "sess-017",
                CreatedAt = baseDate.AddDays(5).AddHours(5),
            },
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000517"),
                UserId = shopperIds[3],
                PostId = Post12Id,
                PostProductId = PostProduct24Id, // T-Shirt Dress
                SessionId = "sess-018",
                CreatedAt = baseDate.AddDays(6),
                ConvertedAt = baseDate.AddDays(6).AddHours(3),
            },
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000518"),
                UserId = shopperIds[4],
                PostId = Post3Id,
                PostProductId = PostProduct6Id, // Belt Bag
                SessionId = "sess-019",
                CreatedAt = baseDate.AddDays(6).AddHours(2),
            },
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000519"),
                UserId = shopperIds[2],
                PostId = Post6Id,
                PostProductId = PostProduct12Id, // Cardigan
                SessionId = "sess-020",
                CreatedAt = baseDate.AddDays(7),
                ConvertedAt = baseDate.AddDays(7).AddHours(4),
            },
        };

        context.ClickEvents.AddRange(clickEvents);
        await context.SaveChangesAsync();
    }

    private static async Task SeedCreatorEarningsAsync(LykeDbContext context)
    {
        var baseDate = new DateTime(2026, 1, 20, 0, 0, 0, DateTimeKind.Utc);

        // Earnings tied to converted click events
        var earnings = new CreatorEarning[]
        {
            // Jessica's earnings (Creator1) — clicks 500, 502
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000600"),
                CreatorId = Creator1Id,
                ClickEventId = Guid.Parse("00000000-0000-0000-0000-000000000500"),
                EarningType = EarningType.Affiliate,
                Amount = 7.20m,
                Currency = "GBP",
                Status = EarningStatus.Paid,
                PaidAt = baseDate.AddDays(14),
                CreatedAt = baseDate,
            },
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000601"),
                CreatorId = Creator1Id,
                ClickEventId = Guid.Parse("00000000-0000-0000-0000-000000000502"),
                EarningType = EarningType.Affiliate,
                Amount = 2.88m,
                Currency = "GBP",
                Status = EarningStatus.Paid,
                PaidAt = baseDate.AddDays(14),
                CreatedAt = baseDate.AddDays(1),
            },
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000602"),
                CreatorId = Creator1Id,
                ClickEventId = Guid.Parse("00000000-0000-0000-0000-000000000503"),
                EarningType = EarningType.Sponsored,
                Amount = 15.00m,
                Currency = "GBP",
                Status = EarningStatus.Confirmed,
                CreatedAt = baseDate.AddDays(3),
            },
            // Maya's earnings (Creator2) — clicks 504, 506
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000603"),
                CreatorId = Creator2Id,
                ClickEventId = Guid.Parse("00000000-0000-0000-0000-000000000504"),
                EarningType = EarningType.Affiliate,
                Amount = 1.75m,
                Currency = "GBP",
                Status = EarningStatus.Paid,
                PaidAt = baseDate.AddDays(14),
                CreatedAt = baseDate.AddDays(2),
            },
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000604"),
                CreatorId = Creator2Id,
                ClickEventId = Guid.Parse("00000000-0000-0000-0000-000000000506"),
                EarningType = EarningType.Affiliate,
                Amount = 3.15m,
                Currency = "GBP",
                Status = EarningStatus.Confirmed,
                CreatedAt = baseDate.AddDays(4),
            },
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000605"),
                CreatorId = Creator2Id,
                ClickEventId = Guid.Parse("00000000-0000-0000-0000-000000000507"),
                EarningType = EarningType.Sponsored,
                Amount = 10.00m,
                Currency = "GBP",
                Status = EarningStatus.Pending,
                CreatedAt = baseDate.AddDays(5),
            },
            // Taylor's earnings (Creator3) — clicks 508, 511
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000606"),
                CreatorId = Creator3Id,
                ClickEventId = Guid.Parse("00000000-0000-0000-0000-000000000508"),
                EarningType = EarningType.Affiliate,
                Amount = 4.20m,
                Currency = "GBP",
                Status = EarningStatus.Paid,
                PaidAt = baseDate.AddDays(14),
                CreatedAt = baseDate.AddDays(3),
            },
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000607"),
                CreatorId = Creator3Id,
                ClickEventId = Guid.Parse("00000000-0000-0000-0000-000000000511"),
                EarningType = EarningType.Affiliate,
                Amount = 3.20m,
                Currency = "GBP",
                Status = EarningStatus.Confirmed,
                CreatedAt = baseDate.AddDays(5),
            },
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000608"),
                CreatorId = Creator3Id,
                ClickEventId = Guid.Parse("00000000-0000-0000-0000-000000000510"),
                EarningType = EarningType.Sponsored,
                Amount = 12.50m,
                Currency = "GBP",
                Status = EarningStatus.Pending,
                CreatedAt = baseDate.AddDays(6),
            },
            // Priya's earnings (Creator4) — clicks 512, 515, 517
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000609"),
                CreatorId = Creator4Id,
                ClickEventId = Guid.Parse("00000000-0000-0000-0000-000000000512"),
                EarningType = EarningType.Affiliate,
                Amount = 3.20m,
                Currency = "GBP",
                Status = EarningStatus.Confirmed,
                CreatedAt = baseDate.AddDays(4),
            },
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000610"),
                CreatorId = Creator4Id,
                ClickEventId = Guid.Parse("00000000-0000-0000-0000-000000000515"),
                EarningType = EarningType.Affiliate,
                Amount = 3.60m,
                Currency = "GBP",
                Status = EarningStatus.Pending,
                CreatedAt = baseDate.AddDays(6),
            },
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000611"),
                CreatorId = Creator4Id,
                ClickEventId = Guid.Parse("00000000-0000-0000-0000-000000000517"),
                EarningType = EarningType.Affiliate,
                Amount = 1.44m,
                Currency = "GBP",
                Status = EarningStatus.Pending,
                CreatedAt = baseDate.AddDays(7),
            },
        };

        context.CreatorEarnings.AddRange(earnings);
        await context.SaveChangesAsync();
    }
}
