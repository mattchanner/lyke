using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Lyke.Infrastructure.Data;

/// <summary>
/// Design-time factory for EF Core migrations.
/// </summary>
public class LykeDbContextFactory : IDesignTimeDbContextFactory<LykeDbContext>
{
    public LykeDbContext CreateDbContext(string[] args)
    {
        var basePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "Lyke.Api");

        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        var optionsBuilder = new DbContextOptionsBuilder<LykeDbContext>();
        optionsBuilder.UseNpgsql(
            connectionString,
            b => b.MigrationsAssembly(typeof(LykeDbContext).Assembly.FullName)
        );

        return new LykeDbContext(optionsBuilder.Options);
    }
}
