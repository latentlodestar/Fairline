using Fairline.Infrastructure;
using Fairline.Migrator;
using Fairline.ServiceDefaults;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();
builder.AddInfrastructure();

builder.Services.AddHostedService<MigrationWorker>();
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing.AddSource(MigrationWorker.ActivitySourceName));

var host = builder.Build();
host.Run();
