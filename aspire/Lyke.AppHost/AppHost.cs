var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithDataVolume()
    .WithPgAdmin(x => x.WithHostPort(56652)); 

var postgresDb = postgres.AddDatabase("postgresdb");

builder.AddProject<Projects.Lyke_Api>("lyke-api")
    .WaitFor(postgresDb)
    .WithReference(postgresDb, connectionName: "DefaultConnection");

builder.Build().Run();
