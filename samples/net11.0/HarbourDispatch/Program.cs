// snippet:complete
using HarbourDispatch;
using Microsoft.AspNetCore.DataProtection;
if (args.Contains("--check", StringComparer.Ordinal))
{
    await BehaviorChecks.RunAsync();
    await LoopbackChecks.RunAsync();
    return;
}
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseStaticWebAssets();
builder.Services.AddDataProtection().UseEphemeralDataProtectionProvider();
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddScoped<DispatchClient>();
WebApplication app = builder.Build();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
await app.RunAsync();
// endsnippet:complete
