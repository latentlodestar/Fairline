using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Fairline.Abstractions.Contracts;
using Fairline.Abstractions.Interfaces;
using Fairline.Infrastructure.Persistence;
using NSubstitute;

namespace Fairline.Api.Tests.Endpoints;

public sealed class IngestEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public IngestEndpointTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ConnectionStrings:fairlinedb", "Host=localhost;Database=test");
            builder.ConfigureServices(services =>
            {
                RemoveDbContextServices<IngestDbContext>(services);
                RemoveDbContextServices<ModelingDbContext>(services);

                services.AddDbContext<IngestDbContext>(opts => opts.UseInMemoryDatabase("TestIngest2"));
                services.AddDbContext<ModelingDbContext>(opts => opts.UseInMemoryDatabase("TestModeling2"));

                var checker = Substitute.For<IDbContextChecker>();
                checker.CanConnectAsync(Arg.Any<CancellationToken>()).Returns(true);
                services.RemoveAll<IDbContextChecker>();
                services.AddScoped(_ => checker);
            });
        }).CreateClient();
    }

    [Fact]
    public async Task GetProviders_ReturnsOkWithEmptyList()
    {
        var response = await _client.GetAsync("/api/ingest/providers");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var providers = await response.Content.ReadFromJsonAsync<List<ProviderInfo>>();
        providers.Should().NotBeNull();
        providers.Should().BeEmpty();
    }

    private static void RemoveDbContextServices<TContext>(IServiceCollection services) where TContext : DbContext
    {
        var contextName = typeof(TContext).FullName!;
        var toRemove = services.Where(d =>
            d.ServiceType.FullName?.Contains(contextName) == true ||
            d.ImplementationType?.FullName?.Contains(contextName) == true ||
            (d.ServiceType.IsGenericType && d.ServiceType.GenericTypeArguments.Any(t => t == typeof(TContext))))
            .ToList();
        foreach (var descriptor in toRemove)
            services.Remove(descriptor);
    }
}
