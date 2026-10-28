// snippet:complete
using System.ComponentModel.DataAnnotations;
using MossObservatory;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.WebHost.UseUrls("http://127.0.0.1:0");
// snippet:registration
builder.Services.AddValidation();
// endsnippet:registration
await using var app = builder.Build();
int handlerCalls = 0;
// snippet:endpoint
app.MapPost("/watches", (WatchRequest request) =>
{
    Interlocked.Increment(ref handlerCalls);
    // This is a business decision, separate from input-shape validation.
    if (request.Station == "closed") { return Results.Conflict(new { reason = "Station is closed" }); }
    return Results.Ok(new { accepted = request.Station });
});
// endsnippet:endpoint
await MossRequestChecks.RunAsync(app, () => Volatile.Read(ref handlerCalls));
// endsnippet:complete
