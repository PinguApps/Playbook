using Microsoft.AspNetCore.Components.Endpoints;
using Playbook.Samples;

// snippet:startup
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".HarbourPermit.Session";
    options.Cookie.HttpOnly = true;
    options.IdleTimeout = TimeSpan.FromMinutes(20);
});
builder.Services.AddRazorComponents(options =>
{
    options.TempDataProviderType = builder.Configuration.GetValue<bool>("SessionTempData")
        ? TempDataProviderType.SessionStorage
        : TempDataProviderType.Cookie;
});
// endsnippet:startup
builder.Services.AddSingleton<InspectionGate>();
var app = builder.Build();
// snippet:pipeline
// Session must surround component endpoints; antiforgery still protects form posts.
if (!builder.Configuration.GetValue<bool>("SkipSession"))
{
    app.UseSession();
}
app.UseAntiforgery();
app.MapRazorComponents<App>();
// endsnippet:pipeline
app.MapPost("/control/arm", (InspectionGate gate) => gate.Arm());
app.MapPost("/control/release", (InspectionGate gate) => gate.Release());
app.MapGet("/late-session", async (HttpContext context) =>
{
    await context.Response.WriteAsync("Headers sent\n");
    await context.Response.Body.FlushAsync();
    try
    {
        context.Session.SetString("late", "cannot establish a new session now");
        await context.Response.WriteAsync("session-written");
    }
    catch (InvalidOperationException)
    {
        await context.Response.WriteAsync("new-session-rejected-after-headers");
    }
});
app.MapGet("/control/runtime", () => System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription);
app.Run();
