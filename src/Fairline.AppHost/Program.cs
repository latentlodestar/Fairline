var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithHostPort(5439)
    .WithDataVolume();

var db = postgres.AddDatabase("fairlinedb");

var migrator = builder.AddProject<Projects.Fairline_Migrator>("migrator")
    .WithReference(db)
    .WaitFor(db);

var api = builder.AddProject<Projects.Fairline_Api>("api")
    .WithReference(db)
    .WaitForCompletion(migrator);

builder.AddViteApp("web", "../Fairline.Web")
    .WithReference(api)
    .WaitFor(api);

builder.Build().Run();
