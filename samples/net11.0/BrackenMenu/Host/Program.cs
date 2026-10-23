// snippet:host
using BrackenMenu.Host;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents().AddInteractiveServerComponents().AddInteractiveWebAssemblyComponents();
var app = builder.Build();
app.UsePathBase("/bracken");
app.UseRouting();
app.Use(async (context, next) =>
{
    context.Response.Headers.ContentSecurityPolicy =
        "default-src 'self'; base-uri 'self'; object-src 'none'; frame-ancestors 'none'; " +
        "script-src 'self' 'wasm-unsafe-eval'; script-src-attr 'none'; " +
        "style-src 'self'; connect-src 'self' ws://localhost:*";
    await next();
});
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode().AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(BrackenMenu.Client.Routes).Assembly);
app.Run();
// endsnippet:host
