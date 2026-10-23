// snippet:complete
using CultureHarbour.Host;
using Microsoft.AspNetCore.Localization;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddLocalization();
// Explicit server opt-out wins even though a localizer factory is registered.
if (builder.Configuration.GetValue<bool>("IndependentClient"))
    builder.Configuration["Components:UseCultureFromServer"] = "false";
builder.Services.AddRazorComponents().AddInteractiveWebAssemblyComponents();
var app = builder.Build();
var localization = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("en-GB", "fr-FR"),
    SupportedCultures = [new CultureInfo("en-GB")],
    SupportedUICultures = [new CultureInfo("fr-FR")],
    // This fictional desk has a fixed server policy; browser headers cannot change it.
    RequestCultureProviders = []
};
app.UseRequestLocalization(localization);
app.UseStaticFiles();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(CultureHarbour.Client.Manifest).Assembly);
app.Run();
// endsnippet:complete
