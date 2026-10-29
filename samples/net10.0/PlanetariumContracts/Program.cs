// snippet:complete
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi;
using Planetarium;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
string scenario = builder.Configuration["PLANETARIUM_CASE"] ?? "normal";
// .NET 10: materialize the configured names at startup and register each one.
foreach (string audience in scenario == "missing" ? new[] { "public" } : new[] { "public", "staff" })
{
    string name = audience;
    builder.Services.AddOpenApi(name, options =>
    {
        options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_0;
        options.ShouldInclude = description => description.GroupName == name;
        options.AddDocumentTransformer((document, context, token) =>
        {
            document.Info.Title = "Planetarium " + name;
            return Task.CompletedTask;
        });
    });
}
WebApplication app = builder.Build();
app.MapOpenApi();
app.MapGet("/planetarium/shows", () => new { Show = "Winter sky", Seats = 24 }).WithGroupName("public");
app.MapGet("/planetarium/projector", () => new { LampHours = 312 }).WithGroupName("staff");
await ContractChecks.RunAsync(app, scenario, newer: false);
// endsnippet:complete
