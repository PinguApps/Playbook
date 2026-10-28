// snippet:complete
using ReadingRoom;
using Microsoft.AspNetCore.DataProtection;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDataProtection().UseEphemeralDataProtectionProvider();
builder.Services.AddSingleton<ReadingAudit>();
builder.Services.AddRazorComponents().AddInteractiveServerComponents(options =>
{
    // A short, local demonstration window makes disconnected teardown observable.
    // Choose production retention independently from this evidence control.
    options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromSeconds(1);
});
var app = builder.Build();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapGet("/audit", (ReadingAudit audit) => audit);
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
Console.WriteLine("Sample runtime: " + System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription);
app.Run();
// endsnippet:complete
