using Playbook.Samples;

// snippet:host
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents(options => options.DisableClientValidation = builder.Configuration.GetValue<bool>("DisableClientValidation"));
builder.Services.AddSingleton<SubmissionCounts>();
var app = builder.Build();
app.Use(async (context, next) =>
{
    if (HttpMethods.IsPost(context.Request.Method))
        context.RequestServices.GetRequiredService<SubmissionCounts>().Post();
    await next(context);
});
app.UseAntiforgery();
app.MapStaticAssets();
app.MapGet("/counts", (SubmissionCounts counts) => new { counts.Posts, counts.Accepted });
app.MapRazorComponents<App>();
app.Run();
// endsnippet:host

