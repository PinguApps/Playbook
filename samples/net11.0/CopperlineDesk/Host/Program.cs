// snippet:complete
using CopperlineDesk.Host;

var builder = WebApplication.CreateBuilder(args);
// Enable referenced client assets when running the built sample in Review/Production.
builder.WebHost.UseStaticWebAssets();
builder.Services.AddRazorComponents().AddInteractiveWebAssemblyComponents();
var app = builder.Build();
app.UseStaticFiles();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(CopperlineDesk.Client.BrowserChrome).Assembly);
app.Run();
// endsnippet:complete
