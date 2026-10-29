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
    Require(result == new Reading("fixture", "fixture", "test-core"), "Early configuration/service was not visible to startup");
    Require(factory.Calls == 1, "Hook ran more than once");
    catalog = factory.Services.GetRequiredService<CoreCatalog>();
    Require(!catalog.Disposed, "Service disposed before request");
    Console.WriteLine("PASS early builder: fixture configuration seen during startup and request; observable test service resolved.");
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
foreach (bool webHost in new[] { false, true })
{
    await using var unsupported = new UnsupportedFactory(webHost);
    try { using var client = unsupported.CreateClient(); throw new Exception("Hook unexpectedly accepted a legacy builder path"); }
    catch (InvalidOperationException exception) when (exception.Message.Contains("ConfigureWebApplicationBuilder", StringComparison.Ordinal))
    { Console.WriteLine("PASS rejected early hook with " + (webHost ? "IWebHostBuilder" : "IHostBuilder") + ": " + exception.Message); }
}
Console.WriteLine($"PASS StrataTests {System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}: real HTTP startup/config order, production control, unsupported paths and awaited asynchronous disposal.");

static void Require(bool condition, string message) { if (!condition) throw new Exception(message); }
sealed record Reading(string StartupSource, string CurrentSource, string Catalog);

sealed class StrataFactory : WebApplicationFactory<StrataApi.Program>
{
    public int Calls { get; private set; }
    protected override void ConfigureWebApplicationBuilder(IHostApplicationBuilder hostApplicationBuilder)
    {
        Calls++;
        hostApplicationBuilder.Configuration.AddInMemoryCollection(new Dictionary<string, string?> { ["Strata:Source"] = "fixture" });
        // Register a factory so the host owns/disposes the service it creates.
        hostApplicationBuilder.Services.AddSingleton(_ => new CoreCatalog("test-core"));
    }
}

sealed class UnsupportedFactory(bool webHost) : WebApplicationFactory<StrataApi.Program>
{
    protected override void ConfigureWebApplicationBuilder(IHostApplicationBuilder hostApplicationBuilder) { }
    protected override IHostBuilder? CreateHostBuilder() => webHost ? null : Host.CreateDefaultBuilder().ConfigureWebHostDefaults(builder => builder.Configure(_ => { }));
    #pragma warning disable ASPDEPR004 // Deliberately exercise the unsupported historical entry path.
    protected override IWebHostBuilder? CreateWebHostBuilder() => webHost ? new WebHostBuilder().Configure(_ => { }) : null;
    #pragma warning restore ASPDEPR004
}
// endsnippet:complete
