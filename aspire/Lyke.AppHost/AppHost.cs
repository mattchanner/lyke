var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder
    .AddPostgres("postgres")
    .WithDataVolume()
    .WithPgAdmin(x => x.WithHostPort(56652));

var storage = builder
    .AddAzureStorage("storage")
    .RunAsEmulator(azurite =>
    {
        azurite.WithBlobPort(27000).WithQueuePort(27001).WithTablePort(27002);

        azurite.WithLifetime(ContainerLifetime.Persistent);
        azurite.WithDataVolume();
    });

var blob = storage.AddBlobContainer("media");
var queue = storage.AddQueue("media-processing");

var postgresDb = postgres.AddDatabase("postgresdb");

builder
    .AddProject<Projects.Lyke_Api>("Api")
    .PublishAsDockerFile(config =>
    {
        config.WithDockerfile("Dockerfile");
        config.WithEndpoint(port: 5104, name: "http");
    })
    .WaitFor(postgresDb)
    .WithReference(postgresDb, connectionName: "DefaultConnection")
    .WaitFor(blob)
    .WithReference(blob, connectionName: "AzureStorage")
    .WaitFor(queue);

builder
    .AddProject<Projects.Lyke_Functions>("Functions")
    .WaitFor(queue)
    .WaitFor(postgresDb)
    .WithReference(postgresDb, connectionName: "DefaultConnection")
    .WithReference(blob, connectionName: "AzureStorage");

builder.Build().Run();
