using Lyke.Api.Endpoints;
using Lyke.Api.Middleware;
using Lyke.Application;
using Lyke.Infrastructure;
using Lyke.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Serilog;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

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

var app = builder.Build();

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
FileInfo entryAseemblyFile = new(Assembly.GetEntryAssembly()!.Location);

// Seed development data
if (app.Environment.IsDevelopment())
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
        configure
            .WithOpenApiRoutePattern("/openapi/{documentName}.json")
            .WithTitle("Be-Lyke API v1");

        configure.ShowSidebar = false;
        configure.Theme = ScalarTheme.Purple;
    });
}

app.UseExceptionHandling();

app.UseSerilogRequestLogging();

app.UseCors("AllowMobileApp");

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
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

await app.RunAsync();

// Make Program class accessible for integration tests
public partial class Program { }
