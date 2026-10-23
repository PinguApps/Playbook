// snippet:host
using Playbook.Samples;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.Circuits;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddSingleton<PauseJournal>();
builder.Services.AddScoped<CircuitLease>();
builder.Services.AddSingleton<MaintenanceRegistry>();
builder.Services.AddScoped<CircuitHandler, WorksheetCircuit>();
builder.Services.AddHostedService<MaintenanceShutdown>();
builder.Services.Configure<HostOptions>(options => options.ShutdownTimeout = TimeSpan.FromSeconds(20));
var app = builder.Build();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode()
.WithBrowserOptions(options => options.AddAutoPause(p =>
{
    p.Enabled = builder.Configuration.GetValue("AutoPause", true);
    p.HiddenDelay = TimeSpan.FromMilliseconds(builder.Configuration.GetValue("HiddenDelay", 120000));
}));
// endsnippet:host
// Loopback-only fixture controls; do not expose these endpoints in a production app.
if (builder.Configuration.GetValue("Verification", false))
{
    app.MapGet("/proof/events", (PauseJournal journal) => journal.Snapshot());
    app.MapPost("/proof/stop", () =>
    {
        _ = Task.Run(async () => { await Task.Delay(300); await app.StopAsync(); });
        return Results.Accepted();
    });
}
app.Run();