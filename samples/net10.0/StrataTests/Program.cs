// snippet:complete
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StrataApi;

[assembly: WebApplicationFactoryContentRoot("StrataApi, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", ".", "StrataApi.dll", "-100")]

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
// Entrypoint Run() and the factory can overlap teardown. Observe completion
// explicitly instead of assuming a returned factory call completed every task.
await catalog.DisposalCompleted.Task.WaitAsync(deadline.Token);
Require(catalog.Disposed, "Asynchronous service disposal did not finish");

await using (var production = new WebApplicationFactory<StrataApi.Program>())
{
    using var client = production.CreateClient();
    var result = await client.GetFromJsonAsync<Reading>(new Uri("/strata", UriKind.Relative), deadline.Token);
    Require(result == new Reading("production", "production", "production-core"), "Test configuration leaked into production control");
}
// ConfigureWebHost runs later. It can change request-time config, not a value
// already captured by entrypoint code immediately after CreateBuilder.
await using (var late = new WebApplicationFactory<StrataApi.Program>().WithWebHostBuilder(builder =>
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

sealed class StrataFactory : WebApplicationFactory<StrataApi.Program>
{
    public int Calls { get; private set; }
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
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
