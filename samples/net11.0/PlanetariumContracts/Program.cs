// snippet:complete
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using Planetarium;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
string scenario = builder.Configuration["PLANETARIUM_CASE"] ?? "normal";
// snippet:dynamic
builder.Services.AddOpenApiCore();
builder.Services.AddSingleton<IAdditionalOpenApiDocumentNameResolver>(new AudienceNames(scenario));
foreach (string audience in new[] { "public", "staff" })
{
    string name = audience;
    builder.Services.Configure<OpenApiOptions>(name, options =>
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
// endsnippet:dynamic
// Control: explicit names and additional names must coexist without duplication.
if (scenario == "explicit")
{
    builder.Services.AddOpenApi("public", options => options.AddDocumentTransformer((document, context, token) =>
    {
        document.Info.Title += " (registered)";
        return Task.CompletedTask;
    }));
}
WebApplication app = builder.Build();
app.MapOpenApi();
app.MapGet("/planetarium/shows", () => new { Show = "Winter sky", Seats = 24 }).WithGroupName("public");
app.MapGet("/planetarium/projector", () => new { LampHours = 312 }).WithGroupName("staff");
await ContractChecks.RunAsync(app, scenario, newer: true);

internal sealed class AudienceNames(string scenario) : IAdditionalOpenApiDocumentNameResolver
{
    public IEnumerable<string> ResolveDocumentNames() => scenario switch
    {
        "missing" => ["public"],
        "empty" => [],
        "duplicate" => ["public", "staff", "public", "staff"],
        "explicit" => ["staff", "staff"], // public comes only from AddOpenApi.
        "case" => ["public", "staff", "PUBLIC"],
        "null-name" => [null!], // Deliberate invalid-input control, never production configuration.
        _ => ["public", "staff"]
    };
}
// endsnippet:complete
