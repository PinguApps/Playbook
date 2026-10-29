// snippet:complete
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StrataApi;

using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(30));
var factory = new StrataFactory();
CoreCatalog catalog;
await using (factory)
{
    using var client = factory.CreateClient(); // Actual entry assembly, startup and TestServer.
    var result = await client.GetFromJsonAsync<Reading>(new Uri("/strata", UriKind.Relative), deadline.Token);
    Require(result == new Reading("production", "fixture", "test-core"), "Late configuration/service not visible to request");
    Require(factory.Calls == 1, "Hook ran more than once");
    catalog = factory.Services.GetRequiredService<CoreCatalog>();
    Require(!catalog.Disposed, "Service disposed before request");
    Console.WriteLine("PASS older ConfigureWebHost: fixture config/service visible to request; startup captured production value.");
}
Require(catalog.Disposed, "Factory.DisposeAsync did not await service disposal");

await using (var production = new RootedFactory())
{
    using var client = production.CreateClient();
    var result = await client.GetFromJsonAsync<Reading>(new Uri("/strata", UriKind.Relative), deadline.Token);
    Require(result == new Reading("production", "production", "production-core"), "Test configuration leaked into production control");
}
// ConfigureWebHost runs later. It can change request-time config, not a value
// already captured by entrypoint code immediately after CreateBuilder.
await using (var late = new RootedFactory().WithWebHostBuilder(builder =>
    builder.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(
        new Dictionary<string, string?> { ["Strata:Source"] = "late" }))))
{
    using var client = late.CreateClient();
    var result = await client.GetFromJsonAsync<Reading>(new Uri("/strata", UriKind.Relative), deadline.Token);
    Require(result == new Reading("production", "late", "production-core"), "Revisit configuration order");
}
Console.WriteLine($"PASS StrataTests {System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}: real HTTP startup/config order, production control and awaited asynchronous disposal.");

static void Require(bool condition, string message) { if (!condition) throw new Exception(message); }
sealed record Reading(string StartupSource, string CurrentSource, string Catalog);

class RootedFactory : WebApplicationFactory<StrataApi.Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder) => builder.UseContentRoot(AppContext.BaseDirectory);
}

sealed class StrataFactory : RootedFactory
{
    public int Calls { get; private set; }
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        Calls++;
        builder.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(new Dictionary<string, string?> { ["Strata:Source"] = "fixture" }));
        // Register a factory so the host owns/disposes the service it creates.
        builder.ConfigureServices(services =>
        {
            Microsoft.Extensions.DependencyInjection.Extensions.ServiceCollectionDescriptorExtensions.RemoveAll<CoreCatalog>(services);
            services.AddSingleton(_ => new CoreCatalog("test-core"));
        });
    }
}

// endsnippet:complete
