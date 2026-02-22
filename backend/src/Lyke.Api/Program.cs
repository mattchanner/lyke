using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Lyke.Api.Endpoints;
using Lyke.Api.Middleware;
using Lyke.Application;
using Lyke.Application.DTOs;
using Lyke.Infrastructure;
using Lyke.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Serilog;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Configuration.AddUserSecrets<Program>();

builder.Host.UseSerilog();

// Aspire defaults
builder.AddServiceDefaults();

builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter())
);

// Add services
builder.Services.AddApplication(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddRedirectToScalarUiMiddleware();

// FluentValidation auto-validation for endpoints
builder.Services.AddFluentValidationAutoValidation();

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey =
    jwtSettings["Secret"] ?? throw new InvalidOperationException("JWT Secret not configured");

builder
    .Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ClockSkew = TimeSpan.Zero,
        };
    });

builder.Services.AddAuthorization(options =>
{
    // Role-based policies using user_type claim from JWT
    options.AddPolicy("CreatorOnly", policy => policy.RequireClaim("user_type", "Creator"));

    options.AddPolicy("AdminOnly", policy => policy.RequireClaim("user_type", "Admin"));

    options.AddPolicy("RetailerOnly", policy => policy.RequireClaim("user_type", "Retailer"));

    // Combined policies for flexibility
    options.AddPolicy(
        "CreatorOrAdmin",
        policy =>
            policy.RequireAssertion(context =>
                context.User.HasClaim(c =>
                    c.Type == "user_type" && (c.Value == "Creator" || c.Value == "Admin")
                )
            )
    );

    options.AddPolicy(
        "RetailerOrAdmin",
        policy =>
            policy.RequireAssertion(context =>
                context.User.HasClaim(c =>
                    c.Type == "user_type" && (c.Value == "Retailer" || c.Value == "Admin")
                )
            )
    );
});

// Controllers
builder.Services.AddControllers();

// Razor Pages for account management (password reset, email verification)
builder.Services.AddRazorPages();

// OpenAPI/Swagger
builder.Services.AddOpenApi("be-lyke_api_v1");

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AllowMobileApp",
        policy =>
        {
            if (builder.Environment.IsDevelopment())
            {
                policy
                    .SetIsOriginAllowed(_ => true)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            }
            else
            {
                policy
                    .WithOrigins(
                        builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                            ?? Array.Empty<string>()
                    )
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            }
        }
    );
});

// Health checks
builder.Services.AddHealthChecks();

// Rate limiting
builder.Services.AddRateLimiter(options =>
{
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.HttpContext.Response.ContentType = "application/json";
        var response = ApiResponse.Fail(
            "RATE_LIMITED",
            "Too many requests. Please try again later."
        );
        await context.HttpContext.Response.WriteAsJsonAsync(response, cancellationToken);
    };

    options.AddFixedWindowLimiter(
        "auth",
        limiter =>
        {
            limiter.PermitLimit = 10;
            limiter.Window = TimeSpan.FromSeconds(60);
        }
    );

    options.AddFixedWindowLimiter(
        "api",
        limiter =>
        {
            limiter.PermitLimit = 100;
            limiter.Window = TimeSpan.FromSeconds(60);
        }
    );

    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromSeconds(60),
            }
        )
    );
});

var app = builder.Build();

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
FileInfo entryAseemblyFile = new(Assembly.GetEntryAssembly()!.Location);

// Apply pending migrations
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<LykeDbContext>();
    await db.Database.MigrateAsync();
}

// Seed development data
if (true || app.Environment.IsDevelopment())
{
    await DataSeeder.SeedAsync(app.Services);
}

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseRedirectToScalarUiMiddleware();
    app.MapScalarApiReference(configure =>
    {
        configure.WithOpenApiRoutePattern("/openapi/{documentName}.json").WithTitle("LYKE API v1");

        configure.ShowSidebar = false;
        configure.Theme = ScalarTheme.Purple;
    });
}

app.UseExceptionHandling();

app.UseSerilogRequestLogging();

app.UseCors("AllowMobileApp");

app.UseRateLimiter();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapRazorPages();
app.MapHealthChecks("/health");

// Minimal API endpoints
app.MapAuthEndpoints();
app.MapProfileEndpoints();
app.MapLookupEndpoints();
app.MapFeedEndpoints();
app.MapPostEndpoints();
app.MapSearchEndpoints();
app.MapCommerceEndpoints();
app.MapCreatorEndpoints();
app.MapMediaEndpoints();
app.MapAdminEndpoints();
app.MapRetailerEndpoints();
app.MapPrivacyEndpoints();
app.MapAnalyticsEndpoints();

await app.RunAsync();

// Make Program class accessible for integration tests
public partial class Program { }
