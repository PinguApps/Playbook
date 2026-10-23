using Playbook.Samples;

// snippet:host
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
var app = builder.Build();
app.UseAntiforgery();
app.MapStaticAssets();
// endsnippet:host
// Local diagnostic observes invocation independently of the captured DOM.
app.MapGet("/control/callbacks", () => BoundaryProbe.CallbackCount);
// snippet:run
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.Run();
// endsnippet:run
