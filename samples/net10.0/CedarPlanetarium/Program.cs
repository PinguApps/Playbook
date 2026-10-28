// snippet:complete
using CedarPlanetarium;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped<VisitState>();
// Disposable local sample: no persistent keys or production resources.
builder.Services.AddDataProtection().UseEphemeralDataProtectionProvider();
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
var app = builder.Build();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
Console.WriteLine(System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription);
app.Run();
// endsnippet:complete
