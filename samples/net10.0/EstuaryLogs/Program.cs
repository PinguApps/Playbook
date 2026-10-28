// snippet:complete
using EstuaryLogs;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents().AddInteractiveServerComponents();

// Migration fixture only: these obsolete options no longer control transfer.
#pragma warning disable CS0618
builder.Services.Configure<Microsoft.AspNetCore.Components.Forms.RemoteBrowserFileStreamOptions>(options =>
{
    options.MaxSegmentSize = 20 * 1024;
    options.MaxBufferSize = 1024 * 1024;
    options.SegmentFetchTimeout = TimeSpan.FromMinutes(1);
});
#pragma warning restore CS0618

var app = builder.Build();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.Run();
// endsnippet:complete
