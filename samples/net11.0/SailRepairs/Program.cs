using Playbook.Samples;

// snippet:startup
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddScoped<MaterialCatalogue>();
var app = builder.Build();
// endsnippet:startup
if (args.Contains("--verify", StringComparer.Ordinal))
{
    await Checks.RunAsync(app.Services);
    return;
}
// snippet:hosting
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
await app.RunAsync();
// endsnippet:hosting
