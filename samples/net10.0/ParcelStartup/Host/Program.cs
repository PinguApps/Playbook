// snippet:complete
using ParcelStartup.Host;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages();
builder.Services.AddRazorComponents().AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();
var app = builder.Build();
app.UseStaticFiles();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorPages();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(ParcelStartup.Client.ParcelCounter).Assembly);
app.Run();
// endsnippet:complete
