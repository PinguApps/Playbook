// snippet:host
using Playbook.Samples;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddSingleton<QueueJournal>();
var app = builder.Build();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapGet("/journal/{run}", (string run, QueueJournal journal) => journal.Read(run));
app.MapPost("/control/{run}/hold", (string run, QueueJournal journal) => journal.Hold(run));
app.MapPost("/control/{run}/release", (string run, QueueJournal journal) => journal.Release(run));
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.Run();
// endsnippet:host
