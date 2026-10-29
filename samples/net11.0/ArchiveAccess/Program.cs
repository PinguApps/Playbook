// snippet:complete
using ArchiveAccess;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.DataProtection;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDataProtection().UseEphemeralDataProtectionProvider();
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<ArchiveIdentity>();
builder.Services.AddScoped<AuthenticationStateProvider>(services => services.GetRequiredService<ArchiveIdentity>());
var app = builder.Build();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
Console.WriteLine("Sample runtime: " + System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription);
app.Run();
// endsnippet:complete
