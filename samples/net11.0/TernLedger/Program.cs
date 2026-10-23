using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using TernLedger;

// snippet:program
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents();
var app = builder.Build();
var requests = new ConcurrentQueue<object>();
app.Use(async (context, next) =>
{
    var id = Guid.NewGuid().ToString("N");
    context.Response.Headers["X-Tern-Request"] = id;
    requests.Enqueue(new
    {
        id,
        at = DateTimeOffset.UtcNow,
        path = context.Request.Path + context.Request.QueryString,
        accept = context.Request.Headers.Accept.ToString(),
        enhanced = context.Request.Headers.Accept.ToString().Contains("blazor-enhanced-nav=on", StringComparison.Ordinal),
        destination = context.Request.Headers["Sec-Fetch-Dest"].ToString()
    });
    while (requests.Count > 128) { requests.TryDequeue(out _); }
    if (context.Request.Path.StartsWithSegments("/ledger-data.json", StringComparison.Ordinal) ||
        context.Request.Path.StartsWithSegments("/ledger-module.js", StringComparison.Ordinal))
    {
        context.Response.Headers.CacheControl = "public, max-age=600";
    }
    await next(context);
});
app.UseStaticFiles();
app.UseRouting();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapGet("/proof", () => new { runtime = RuntimeInformation.FrameworkDescription, requests = requests.ToArray() });
app.MapRazorComponents<App>();
app.Run();
// endsnippet:program
