using Fairline.Api.Endpoints;
using Fairline.Application;
using Fairline.Infrastructure;
using Fairline.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddInfrastructure();
builder.Services.AddApplication();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseCors();
app.MapDefaultEndpoints();
app.MapStatusEndpoints();
app.MapIngestEndpoints();
app.MapModelingEndpoints();
app.MapDashboardEndpoints();
app.MapEdgeEndpoints();

app.Run();

public partial class Program;
