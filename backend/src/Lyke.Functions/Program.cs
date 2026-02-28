using Lyke.Infrastructure;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices((ctx, services) =>
    {
        services.AddDatabase(ctx.Configuration);
        services.AddStorage(ctx.Configuration);
    })
    .Build();

host.Run();
