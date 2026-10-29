// snippet:complete
using System.Net;
using System.Text.Json;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using TideSurvey;

RuntimeIdentity.Verify();
var builder = WebApplication.CreateBuilder();
builder.Logging.ClearProviders();
builder.WebHost.ConfigureKestrel(options =>
{
    // Separate ephemeral cleartext listeners avoid TLS trust or HTTP/1.1 negotiation changes.
    options.Listen(IPAddress.Loopback, 0, listen => listen.Protocols = HttpProtocols.Http1);
    options.Listen(IPAddress.Loopback, 0, listen => listen.Protocols = HttpProtocols.Http2);
});
builder.Services.AddGrpc().AddJsonTranscoding();
#if NET10_0
builder.Services.AddGrpcSwagger();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "Tide survey", Version = "v1" });
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "TideSurvey.xml"));
    options.IncludeGrpcXmlComments(Path.Combine(AppContext.BaseDirectory, "TideSurvey.xml"), true);
});
#endif
await using var app = builder.Build();
app.MapGrpcService<SurveyService>();
#if NET10_0
app.UseSwagger();
#else
// A generated BUILD artifact; no replacement runtime adapter or reflection integration.
app.MapGet("/contracts/tides.swagger.json", () => Results.File(
    Path.Combine(AppContext.BaseDirectory, "tides.swagger.json"), "application/json"));
#endif
await app.StartAsync();
try
{
    var addresses = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.ToArray();
    using var http = new HttpClient { BaseAddress = new Uri(addresses[0]), Timeout = TimeSpan.FromSeconds(15) };
    using var channel = GrpcChannel.ForAddress(addresses[1]);
    var client = new TideBoard.TideBoardClient(channel);
    var grpc = await client.ReadTideAsync(new TideRequest { Station = "willow" }, deadline: DateTime.UtcNow.AddSeconds(15));
    using var response = await http.GetAsync(new Uri("/v1/tides/willow", UriKind.Relative));
    response.EnsureSuccessStatusCode();
    var json = await response.Content.ReadAsStringAsync();
    using var payload = JsonDocument.Parse(json);
    Check(grpc.Station == "willow" && grpc.HeightCm == 142 && grpc.ObservedAt == "2026-10-01T06:30:00Z", "gRPC contract");
    Check(payload.RootElement.GetProperty("station").GetString() == grpc.Station &&
        payload.RootElement.GetProperty("heightCm").GetInt32() == grpc.HeightCm &&
        payload.RootElement.GetProperty("observedAt").GetString() == grpc.ObservedAt, "JSON matches actual gRPC values");
    try
    {
        await client.ReadTideAsync(new TideRequest { Station = "unknown" }, deadline: DateTime.UtcNow.AddSeconds(15));
        throw new InvalidOperationException("unknown station accepted");
    }
    catch (RpcException error) when (error.StatusCode == StatusCode.NotFound) { }
    Check((await http.GetAsync(new Uri("/v1/tides/unknown", UriKind.Relative))).StatusCode == HttpStatusCode.NotFound, "transcoded gRPC error");
    Check((await http.PostAsync(new Uri("/v1/tides/willow", UriKind.Relative), new StringContent("{}"))).StatusCode == HttpStatusCode.MethodNotAllowed, "wrong HTTP method");
    Check((await http.GetAsync(new Uri("/v1/tides/willow/extra", UriKind.Relative))).StatusCode == HttpStatusCode.NotFound, "wrong path");
#if NET10_0
    var document = await http.GetStringAsync(new Uri("/swagger/v1/swagger.json", UriKind.Relative));
#else
    var document = await http.GetStringAsync(new Uri("/contracts/tides.swagger.json", UriKind.Relative));
#endif
    using var contract = JsonDocument.Parse(document);
    var root = contract.RootElement;
    var operation = root.GetProperty("paths").GetProperty("/v1/tides/{station}").GetProperty("get");
    Check(root.GetProperty("paths").EnumerateObject().Count() == 1, "nonempty bounded operations");
    Check(operation.GetProperty("summary").GetString()!.Contains("next surveyed tide", StringComparison.Ordinal), "RPC comment");
    var responseSchema = operation.GetProperty("responses").GetProperty("200");
#if NET10_0
    var reference = responseSchema.GetProperty("content").GetProperty("application/json").GetProperty("schema").GetProperty("$ref").GetString()!;
    var schema = root.GetProperty("components").GetProperty("schemas").GetProperty(reference.Split('/')[^1]);
#else
    Check(root.GetProperty("swagger").GetString() == "2.0", "explicit OpenAPI v2 dialect");
    var reference = responseSchema.GetProperty("schema").GetProperty("$ref").GetString()!;
    var schema = root.GetProperty("definitions").GetProperty(reference.Split('/')[^1]);
#endif
    Check(schema.GetProperty("description").GetString()!.Contains("navigation guarantee", StringComparison.Ordinal), "message comment");
    var properties = schema.GetProperty("properties");
    Check(properties.GetProperty("heightCm").GetProperty("description").GetString()!.Contains("local survey marker", StringComparison.Ordinal), "field comment");
    // Validate the actual JSON payload against the documented bounded primitive contract.
    Check(properties.EnumerateObject().Count() == payload.RootElement.EnumerateObject().Count(), "all wire fields documented");
    foreach (var field in payload.RootElement.EnumerateObject())
    {
        var type = properties.GetProperty(field.Name).GetProperty("type").GetString();
        Check(type == (field.Value.ValueKind == JsonValueKind.Number ? "integer" : "string"), "documented wire type " + field.Name);
    }
    Check(properties.GetProperty("heightCm").GetProperty("format").GetString() == "int32", "int32 format");
    Console.WriteLine("ACTUAL-JSON " + json);
    Console.WriteLine("ACTUAL-DOCUMENT " + document);
    Console.WriteLine($"PASS TideSurvey real gRPC/JSON/document/comments/failure checks; runtime={Environment.Version}");
}
finally { await app.StopAsync(); }

static void Check(bool condition, string claim)
{
    if (!condition) throw new InvalidOperationException(claim);
}

internal sealed class SurveyService : TideBoard.TideBoardBase
{
    public override Task<TideReading> ReadTide(TideRequest request, ServerCallContext context)
    {
        if (request.Station != "willow") throw new RpcException(new Status(StatusCode.NotFound, "Station is outside this survey."));
        return Task.FromResult(new TideReading { Station = request.Station, HeightCm = 142, ObservedAt = "2026-10-01T06:30:00Z" });
    }
}
// endsnippet:complete
