using Lyke.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Lyke.Api.Workers;

public class MigrateDatabaseBackgroundWorker(
    IServiceProvider provider,
    ILogger<MigrateDatabaseBackgroundWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = provider.CreateScope();
        LykeDbContext context =
            scope.ServiceProvider.GetRequiredService<LykeDbContext>();


        if (true || context.Database.HasPendingModelChanges())
        {
            logger.LogInformation("Migrating database to the latest version");
            await context.Database.MigrateAsync(stoppingToken).ConfigureAwait(false);
        }
        else
        {
            logger.LogInformation("Database is already at the latest version");
        }
    }
}
