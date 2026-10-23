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
// MVC's existing TempData services are a supported bridge for the older sample.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorComponents();
// endsnippet:startup
builder.Services.AddSingleton<InspectionGate>();
var app = builder.Build();
// snippet:pipeline
app.UseSession();
app.UseAntiforgery();
app.MapRazorComponents<App>();
// endsnippet:pipeline
app.MapPost("/control/arm", (InspectionGate gate) => gate.Arm());
app.MapPost("/control/release", (InspectionGate gate) => gate.Release());
app.MapGet("/control/runtime", () => System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription);
app.Run();
