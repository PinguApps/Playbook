// snippet:complete
using System.Net.ServerSentEvents;
using System.Runtime.CompilerServices;
using Microsoft.OpenApi;
using SkyLedger;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
// snippet:versions
builder.Services.AddOpenApi("v1"); // .NET 11 defaults to OpenAPI 3.2.
builder.Services.AddOpenApi("v31", options => options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_1);
builder.Services.AddOpenApi("v30", options => options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_0);
builder.Services.AddOpenApi("client", options =>
{
    options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_0;
    options.ShouldInclude = description => description.RelativePath == "observatory/status";
});
// endsnippet:versions
// snippet:environment
builder.Services.AddOpenApi("environment", options => options.AddDocumentTransformer((document, context, token) =>
{
    document.Info.Title = "Sky ledger: " + builder.Environment.EnvironmentName;
    return Task.CompletedTask;
}));
WebApplication app = builder.Build();
app.MapOpenApi();
app.MapGet("/observatory/status", () => new DomeStatus("West dome", 7)).WithName("ReadDome");
if (app.Environment.IsDevelopment())
{
    app.MapGet("/observatory/calibration", () => new { Offset = 2 });
}
// endsnippet:environment
// snippet:stream
app.MapGet("/observatory/events", (CancellationToken token) => TypedResults.ServerSentEvents(Readings(token)));
// The same sequence returned directly is JSON, without SSE framing.
app.MapGet("/observatory/raw", (CancellationToken token) => Readings(token));

static async IAsyncEnumerable<SseItem<DomeStatus>> Readings([EnumeratorCancellation] CancellationToken token)
{
    yield return new SseItem<DomeStatus>(new("West dome", 7), "alignment") { EventId = "scan-41" };
    await Task.Delay(20, token);
    yield return new SseItem<DomeStatus>(new("West dome", 8), "alignment") { EventId = "scan-42" };
}
// endsnippet:stream
await ContractChecks.RunAsync(app, newer: true);

namespace SkyLedger
{
    public sealed record DomeStatus(string Dome, int Exposure);
}
// endsnippet:complete
