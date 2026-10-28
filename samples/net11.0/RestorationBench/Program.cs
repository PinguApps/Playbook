// snippet:complete
using RestorationBench;
using Microsoft.AspNetCore.DataProtection;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDataProtection().UseEphemeralDataProtectionProvider();
builder.Services.AddSingleton<StatusFeed>();
builder.Services.AddRazorComponents().AddInteractiveServerComponents(options =>
    options.DetailedErrors = builder.Environment.IsDevelopment()); // Local proof only; never expose in production.
var app = builder.Build();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapGet("/audit", (StatusFeed feed) => feed.Snapshot());
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
Console.WriteLine("Sample runtime: " + System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription);
app.Run();
// endsnippet:complete
