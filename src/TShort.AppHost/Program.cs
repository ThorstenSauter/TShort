using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var db = builder.AddSqlServer("sql")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume("tshort-data")
    .AddDatabase("tshort");

var api = builder.AddProject<TShort_Api>("api")
    .WithReference(db)
    .WaitFor(db);

builder.AddProject<TShort_Web>("web")
    .WithReference(api)
    .WaitFor(api);

await builder.Build().RunAsync();
