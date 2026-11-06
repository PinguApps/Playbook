// snippet:complete
using System.Net;

// A fictional estuary survey service. Certificate trust is a separate owner action.
var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(options => options.Listen(IPAddress.Loopback, 56485,
    listener => listener.UseHttps()));
var app = builder.Build();
app.MapGet("/survey", () => "Estuary survey ready");
await app.RunAsync();
// endsnippet:complete
