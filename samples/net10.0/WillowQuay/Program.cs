// snippet:complete
using WillowQuay;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddSingleton<PanelAudit>();
WebApplication app = builder.Build();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapGet("/audit", (PanelAudit audit) => new { audit.TimerFired, audit.DisposedPanels, runtime = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription });
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.Run();
// endsnippet:complete
