// snippet:complete
using CedarPlanetarium;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped<VisitState>();
// Disposable local sample: no persistent keys or production resources.
builder.Services.AddDataProtection().UseEphemeralDataProtectionProvider();
var trace = new MetadataTrace();
builder.Services.AddSingleton(trace);
builder.Services.AddRazorComponents().AddInteractiveServerComponents(options =>
{
    // First resolver owns ShowTicket. The second rejects that type if reached.
#pragma warning disable ASPNETCORE9004 // Experimental application-payload metadata only.
    options.JsonTypeInfoResolvers.Add(new TicketResolver("first", true, trace));
    options.JsonTypeInfoResolvers.Add(new TicketResolver("second", false, trace));
#pragma warning restore ASPNETCORE9004
});
var app = builder.Build();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
Console.WriteLine(System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription);
app.Run();
// endsnippet:complete
