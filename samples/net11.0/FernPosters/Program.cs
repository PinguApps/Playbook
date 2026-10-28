// snippet:complete
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = Directory.GetCurrentDirectory()
});
builder.Logging.ClearProviders();
builder.WebHost.UseUrls("http://127.0.0.1:0");
builder.Services.AddControllersWithViews();
await using var app = builder.Build();
app.MapControllers();
await app.StartAsync();
try
{
    var address = app.Services.GetRequiredService<IServer>()
        .Features.Get<IServerAddressesFeature>()!.Addresses.Single();
    using var client = new HttpClient { BaseAddress = new Uri(address) };
    var html = await client.GetStringAsync(new Uri("/posters", UriKind.Relative));
    if (!html.Contains("src=\"/posters/fern.svg?v=", StringComparison.Ordinal) ||
        !html.Contains("data-collection=\"Fern &amp; Moss\"", StringComparison.Ordinal) ||
        !html.Contains("alt=\"Fern study\"", StringComparison.Ordinal) ||
        html.Contains("asp-append-version", StringComparison.Ordinal) ||
        html.Contains("fern-poster", StringComparison.Ordinal))
        throw new InvalidOperationException(html);
    Console.WriteLine(html.Trim());
    Console.WriteLine($"PASS: Razor activates derived ImageTagHelper and emits versioned, encoded markup; {System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}; ASP.NET Core {typeof(WebApplication).Assembly.GetName().Version}");
}
finally
{
    await app.StopAsync();
}
// endsnippet:complete
