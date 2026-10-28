using Playbook.Samples;

// snippet:host
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents();
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
Console.WriteLine($"Sample runtime: {System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}");
app.Run();
// endsnippet:host
