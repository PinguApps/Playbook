using Playbook.Samples;

// snippet:startup
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents();
builder.Services.AddValidation();
// endsnippet:startup
builder.Services.AddSingleton<SubmissionCounts>();
var app = builder.Build();
app.UseRequestLocalization(new RequestLocalizationOptions()
    .SetDefaultCulture("en-GB").AddSupportedCultures("en-GB", "fr-FR")
    .AddSupportedUICultures("en-GB", "fr-FR"));
app.Use(async (context, next) =>
{
    if (HttpMethods.IsPost(context.Request.Method))
    {
        context.RequestServices.GetRequiredService<SubmissionCounts>().RecordPost();
    }
    await next(context);
});
app.UseAntiforgery();
app.MapStaticAssets();
app.MapGet("/counts", (SubmissionCounts counts) => new { counts.Posts, counts.Accepted });
app.MapRazorComponents<App>();
app.Run();
