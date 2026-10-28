// snippet:complete
using QuayArchive;
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
builder.Services.AddControllers();
await using var app = builder.Build();
app.MapControllers();
await app.StartAsync();
try
{
    var address = app.Services.GetRequiredService<IServer>()
        .Features.Get<IServerAddressesFeature>()!.Addresses.Single();
    using var client = new HttpClient { BaseAddress = new Uri(address) };
    var body = await client.GetStringAsync(new Uri("/archive/crates/27", UriKind.Relative));
    if (body != "Crate 27 is ready") throw new InvalidOperationException(body);
    Console.WriteLine($"PASS: MVC startup and representative controller request; {System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}; ASP.NET Core {typeof(WebApplication).Assembly.GetName().Version}");
}
finally
{
    await app.StopAsync();
}
// endsnippet:complete
