var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithDataVolume()
    .WithPgAdmin(x => x.WithHostPort(56652));

var storage = builder.AddAzureStorage("azure-storage").RunAsEmulator().AddBlobContainer("media");

var postgresDb = postgres.AddDatabase("postgresdb");

builder.AddProject<Projects.Lyke_Api>("lyke-api")
    .WaitFor(postgresDb)
    .WithReference(postgresDb, connectionName: "DefaultConnection")
    .WaitFor(storage)
    .WithReference(storage, connectionName: "AzureBlob:ConnectionString");

builder.Build().Run();
