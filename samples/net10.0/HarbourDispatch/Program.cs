// snippet:complete
using HarbourDispatch;
using Microsoft.AspNetCore.DataProtection;
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseStaticWebAssets();
builder.Services.AddDataProtection().UseEphemeralDataProtectionProvider();
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddScoped<DispatchClient>();
WebApplication app = builder.Build();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.Run();
// endsnippet:complete
