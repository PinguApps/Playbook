// snippet:host
using Playbook.Samples;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
var app = builder.Build();
app.UseAntiforgery();
app.MapStaticAssets();
// endsnippet:host
app.MapGet("/probe/results", () => new { FragmentPersistenceRejections = FragmentProbe.PersistenceRejections });
// snippet:run
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.Run();
// endsnippet:run
