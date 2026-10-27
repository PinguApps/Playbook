// snippet:complete
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using FenContracts;

await using var app = CreateApplication(args);
await app.StartAsync();
try
{
    var address = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();
    await ContractChecks.RunAsync(address);
    Console.WriteLine($"PASS: FenContracts net10.0; runtime {Environment.Version}; OpenAPI {typeof(OpenApiDocument).Assembly.GetName().Version}");
}
finally
{
    await app.StopAsync();
}

static WebApplication CreateApplication(string[] args)
{
    var builder = WebApplication.CreateBuilder(args);
    builder.WebHost.UseUrls("http://127.0.0.1:0");
    builder.Logging.ClearProviders();
    builder.Services.AddOpenApi("raw", options => Configure(options, false, false));
    builder.Services.AddOpenApi("v1", options => Configure(options, true, false));
    builder.Services.AddOpenApi("slash", options => Configure(options, true, true));
    var app = builder.Build();
    app.UsePathBase("/fen");
    app.MapOpenApi();
    // snippet:declarations
#pragma warning disable CS0618 // Intentionally retain the published retiring contract.
    app.MapGet("/legacy-readings", ReadingRoutes.ReadLegacy);
#pragma warning restore CS0618
    app.MapGet("/readings", () => new Reading { Station = "Alder-7", DepthMetres = 2.4 });
    // endsnippet:declarations
    return app;
}

// snippet:registration
static void Configure(OpenApiOptions options, bool legacyDeprecation, bool preserveSlash)
{
    options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_1;
    options.AddDocumentTransformer<FenDocumentTransformer>();
    options.AddOperationTransformer<FenOperationTransformer>();
    options.AddSchemaTransformer<FenSchemaTransformer>();
    if (legacyDeprecation)
    {
        options.AddSchemaTransformer<LegacyDeprecationTransformer>();
        options.AddOperationTransformer<LegacyDeprecationTransformer>();
    }
    if (preserveSlash)
    {
        options.AddDocumentTransformer((document, context, cancellationToken) =>
        {
            // Keep only the empty-PathBase historical convention, not /fen/.
            if (document.Servers?.All(server => new Uri(server.Url!).AbsolutePath == "/") == true)
            {
                foreach (var server in document.Servers ?? [])
                {
                    server.Url = server.Url!.TrimEnd('/') + "/";
                }
            }
            return Task.CompletedTask;
        });
    }
}
// endsnippet:registration
// endsnippet:complete
