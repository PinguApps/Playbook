using Playbook.Samples;

// snippet:host
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
var app = builder.Build();
app.Use(async (context, next) =>
{
    // Comparison only: the legacy route permits dynamic spacer style attributes.
    string attributes = context.Request.Path == "/legacy" ? "'unsafe-inline'" : "'none'";
    context.Response.Headers.ContentSecurityPolicy =
        "default-src 'self'; base-uri 'self'; object-src 'none'; frame-ancestors 'none'; " +
        "script-src 'self'; connect-src 'self' ws://localhost:*; " +
        $"style-src 'self'; style-src-elem 'self'; style-src-attr {attributes}";
    await next();
});
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.Run();
// endsnippet:host
