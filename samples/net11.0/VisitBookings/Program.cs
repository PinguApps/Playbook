using Playbook.Samples;

var builder = WebApplication.CreateBuilder(args);
// snippet:startup
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddLocalization();
builder.Services.AddValidation();
// endsnippet:startup
builder.Services.AddSingleton<RuleJournal>();
builder.Services.AddHttpClient<VisitRule>(client => client.BaseAddress =
    new Uri(builder.Configuration["RuleBaseUri"] ?? "http://localhost:5285"));
var app = builder.Build();
// snippet:culture
app.UseRequestLocalization(new RequestLocalizationOptions()
    .SetDefaultCulture("en-GB").AddSupportedCultures("en-GB", "fr-FR")
    .AddSupportedUICultures("en-GB", "fr-FR"));
// endsnippet:culture
// snippet:culture-cookie
app.MapGet("/culture/{culture}", (string culture, HttpContext context) =>
{
    if (culture is not ("en-GB" or "fr-FR")) return Results.BadRequest();
    context.Response.Cookies.Append(Microsoft.AspNetCore.Localization.CookieRequestCultureProvider.DefaultCookieName,
        Microsoft.AspNetCore.Localization.CookieRequestCultureProvider.MakeCookieValue(
            new Microsoft.AspNetCore.Localization.RequestCulture(culture)));
    return Results.Redirect("/interactive");
});
// endsnippet:culture-cookie
app.UseAntiforgery();
app.MapStaticAssets();
// snippet:endpoint
app.MapPost("/visits", (VisitRequest request, RuleJournal journal) =>
{
    journal.Accept(request.Pass);
    return Results.NoContent();
});
// endsnippet:endpoint
// Local sample controls make awaiting/cancellation observable without an external service.
app.MapPost("/control/arm/{code}", (string code, RuleJournal journal) => journal.Arm(code));
app.MapPost("/control/release/{code}", (string code, RuleJournal journal) => journal.Release(code));
app.MapGet("/control/{code}", (string code, RuleJournal journal) => journal.Snapshot(code));
app.MapGet("/rules/{code}", async (string code, RuleJournal journal, CancellationToken token) =>
    await journal.CheckAsync(code, token));
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.Run();
