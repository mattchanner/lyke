using System.Text;
using Lyke.Api.Endpoints;
using Lyke.Api.Middleware;
using Lyke.Application;
using Lyke.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Add services
builder.Services.AddApplication(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);

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

builder.Services.AddAuthorization();

// Controllers
builder.Services.AddControllers();

// OpenAPI/Swagger
builder.Services.AddOpenApi();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AllowMobileApp",
        policy =>
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
    );
});

// Health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseExceptionHandling();

app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

app.UseCors("AllowMobileApp");

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

app.Run();

// Make Program class accessible for integration tests
public partial class Program { }
