// snippet:host
using TideDesk;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents().AddInteractiveWebAssemblyComponents();
var app = builder.Build();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode().AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(TideDesk.Client.Desk).Assembly)
    .WithBrowserOptions(options =>
    {
        options.InteractiveServer.ReconnectionMaxRetries = 5;
        options.InteractiveServer.ReconnectionRetryInterval = TimeSpan.FromMilliseconds(3000);
        options.InteractiveServer.ReconnectionDialogId = "tide-reconnect";
        options.StaticServer.PreserveDom = false;
        options.StaticServer.CircuitInactivityTimeout = TimeSpan.FromMinutes(2);
        options.InteractiveWebAssembly.EnvironmentName = "TideDesk";
    });
app.Run();
// endsnippet:host
