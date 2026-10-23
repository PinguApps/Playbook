// snippet:complete
using BreakwaterLab;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddScoped<SessionStamp>();
builder.Services.AddSingleton(new StationName("West breakwater"));
if (!args.Contains("--missing-key"))
{
    builder.Services.AddKeyedSingleton<Scale>("depth", new Scale(3));
}
builder.Services.AddScoped<StationServices>();
WebApplication app = builder.Build();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.Run();
// endsnippet:complete
