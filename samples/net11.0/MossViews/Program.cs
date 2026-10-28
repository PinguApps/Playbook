// snippet:complete
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
bool serve = args.Contains("--serve", StringComparer.Ordinal);
builder.WebHost.UseUrls(serve ? "http://127.0.0.1:5187" : "http://127.0.0.1:0");
// snippet:registration
// The Razor SDK compiles .cshtml at build/publish time by default.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
// endsnippet:registration
await using var app = builder.Build();
app.MapControllers();
app.MapRazorPages();
if (serve) { await app.RunAsync(); return; }
await app.StartAsync();
try
{
    string address = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();
    using var client = new HttpClient(new SocketsHttpHandler { UseProxy = false }) { BaseAddress = new Uri(address), Timeout = TimeSpan.FromSeconds(10) };
    foreach ((string path, string expected) in new[] { ("/sightings", "Moss observatory: MVC"), ("/night", "Moss observatory: Razor Pages") })
    {
        using var response = await client.GetAsync(new Uri(path, UriKind.Relative));
        string html = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode || !html.Contains(expected, StringComparison.Ordinal)
            || !html.Contains("&lt;meteor&gt;", StringComparison.Ordinal))
        {
            throw new Exception($"Rendering/HTML encoding failed for {path}: {response.StatusCode} {html}");
        }
    }
    Console.WriteLine($"PASS {System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}: MVC and Razor Pages render original compiled views with encoded model data; 2 real HTTP requests. No browser or Hot Reload execution claim.");
}
finally { await app.StopAsync(); }
// endsnippet:complete
