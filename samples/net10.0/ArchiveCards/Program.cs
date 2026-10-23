using Playbook.Samples;

// snippet:host
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
var app = builder.Build();
app.UseAntiforgery();
app.MapStaticAssets();
// endsnippet:host
// snippet:run
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.Run();
// endsnippet:run
