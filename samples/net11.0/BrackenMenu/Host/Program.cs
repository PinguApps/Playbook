// snippet:host
using System.Security.Cryptography;
using System.Text;
using BrackenMenu.Host;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents().AddInteractiveServerComponents().AddInteractiveWebAssemblyComponents();
var app = builder.Build();
app.UsePathBase("/bracken");
app.UseRouting();
app.Use(async (context, next) =>
{
    const string handler = "this.closest('bracken-menu').querySelector('input').checked = false";
    var attributePolicy = builder.Configuration.GetValue<bool>("AllowInlineMenu")
        ? "'unsafe-hashes' 'sha256-" + Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(handler))) + "'"
        : "'none'";
    context.Response.Headers.ContentSecurityPolicy =
        "default-src 'self'; base-uri 'self'; object-src 'none'; frame-ancestors 'none'; " +
        "script-src 'self' 'wasm-unsafe-eval'; script-src-attr " + attributePolicy + "; " +
        "style-src 'self'; connect-src 'self' ws://localhost:*";
    await next();
});
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode().AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(BrackenMenu.Client.Routes).Assembly);
app.Run();
// endsnippet:host
