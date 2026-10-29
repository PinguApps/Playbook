// snippet:complete
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Microsoft.AspNetCore.JsonPatch.SystemTextJson;
using Microsoft.AspNetCore.JsonPatch.SystemTextJson.Operations;

using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(30));
var resolver = new DefaultJsonTypeInfoResolver();
resolver.Modifiers.Add(type =>
{
    if (!typeof(OperationBase).IsAssignableFrom(type.Type)) return;
    foreach (var property in type.Properties)
    {
        if (property.Name == "from") property.ShouldSerialize = (value, _) => NeedsFrom((OperationBase)value);
        if (property.Name == "value") property.ShouldSerialize = (value, _) =>
            ((OperationBase)value).OperationType is OperationType.Add or OperationType.Replace or OperationType.Test;
    }
});
var json = new JsonSerializerOptions(JsonSerializerDefaults.Web) { TypeInfoResolver = resolver };
var builder = WebApplication.CreateBuilder();
builder.Logging.ClearProviders();
builder.WebHost.UseUrls("http://127.0.0.1:0");
await using var app = builder.Build();
app.MapPatch("/garden", async context =>
{
    // Loopback fixture only. Authenticate and authorize the owner BEFORE this point in a real app.
    // Cap the body and operation count; allow-list BOTH path and from before applying.
    if (context.Request.ContentLength is null or > 4096) { context.Response.StatusCode = 413; return; }
    JsonPatchDocument<GardenRecord>? patch;
    try { patch = await JsonSerializer.DeserializeAsync<JsonPatchDocument<GardenRecord>>(context.Request.Body, json, context.RequestAborted); }
    catch (JsonException) { context.Response.StatusCode = 400; return; }
    string[] allowed = ["/label", "/note", "/backup", "/seeds"];
    if (patch is null || patch.Operations.Count is 0 or > 8 || patch.Operations.Any(operation =>
        !allowed.Contains(operation.path, StringComparer.Ordinal) ||
        (NeedsFrom(operation) && !allowed.Contains(operation.from, StringComparer.Ordinal)) ||
        operation.OperationType == OperationType.Invalid))
    { context.Response.StatusCode = 400; return; }
    // Apply to an isolated working copy. Never persist a partially failed patch.
    var garden = new GardenRecord();
    var failures = new List<string>();
    patch.ApplyTo(garden, error => failures.Add(error.ErrorMessage));
    if (failures.Count > 0) { context.Response.StatusCode = 400; return; }
    await context.Response.WriteAsJsonAsync(garden, json, context.RequestAborted);
});
await app.StartAsync(deadline.Token);
using var client = new HttpClient(new SocketsHttpHandler { UseProxy = false }) { BaseAddress = new Uri(app.Urls.Single()) };
var operations = new List<Operation<GardenRecord>>
{
    new("test", "/label", null!, "fern"),
    new("add", "/note", null!, "shade"),
    new("copy", "/backup", "/note"),
    new("move", "/label", "/backup"),
    new("replace", "/note", null!, null!),
    new("remove", "/seeds", null!)
};
var document = new JsonPatchDocument<GardenRecord>(operations, json);
Console.WriteLine("DEFAULT-WIRE " + JsonSerializer.Serialize(document, new JsonSerializerOptions(JsonSerializerDefaults.Web)));
string wire = JsonSerializer.Serialize(document, json);
Console.WriteLine("WIRE " + wire);
string spurious = JsonSerializer.Serialize(new Operation<GardenRecord>("add", "/note", "/backup", "sprout"), json);
using (var parsed = JsonDocument.Parse(spurious))
    Require(!parsed.RootElement.TryGetProperty("from", out _), "Older operation-aware modifier did not filter spurious from");
Console.WriteLine("NONNULL-FROM " + spurious);
using (var parsed = JsonDocument.Parse(wire))
{
    for (int index = 0; index < operations.Count; index++)
    {
        var item = parsed.RootElement[index];
        bool needsValue = operations[index].OperationType is OperationType.Add or OperationType.Replace or OperationType.Test;
        Require(item.TryGetProperty("from", out _) == NeedsFrom(operations[index]), "Incorrect from presence");
        Require(item.TryGetProperty("value", out _) == needsValue, "Incorrect value presence");
    }
    Require(parsed.RootElement[4].GetProperty("value").ValueKind == JsonValueKind.Null, "Required null value omitted");
}
var roundTrip = JsonSerializer.Deserialize<JsonPatchDocument<GardenRecord>>(wire, json)!;
Require(roundTrip.Operations.Count == 6 && JsonSerializer.Serialize(roundTrip, json) == wire, "Round trip drift");
using (var response = await SendAsync(wire))
{
    Require(response.StatusCode == HttpStatusCode.OK, "Six-operation patch failed");
    var result = JsonSerializer.Deserialize<GardenRecord>(await response.Content.ReadAsStringAsync(deadline.Token), json)!;
    Require(result.Label == "shade" && result.Note is null && result.Backup is null && result.Seeds == 0, "Incorrect applied state");
}
// The measured serializer does not enforce RFC member presence on incoming JSON.
// Check raw presence separately when the application requires stricter validation.
foreach (string input in new[]
{
    "[{\"op\":\"replace\",\"path\":\"/note\",\"value\":null}]",
    "[{\"op\":\"replace\",\"path\":\"/note\"}]"
})
{
    using var response = await SendAsync(input);
    Console.WriteLine($"PRESENCE {input} => {(int)response.StatusCode} {await response.Content.ReadAsStringAsync(deadline.Token)}");
    Require(response.StatusCode == HttpStatusCode.OK, "Revisit missing/null measured behavior");
}
foreach (string input in new[]
{
    "[{\"op\":\"replace\",\"path\":\"/owner\",\"value\":\"intruder\"}]",
    "[{\"op\":\"copy\",\"path\":\"/note\",\"from\":\"/owner\"}]",
    "[{\"op\":\"move\",\"path\":\"/note\"}]",
    "[{\"op\":\"test\",\"path\":\"/label\",\"value\":\"wrong\"}]",
    "[{\"op\":\"add\",\"path\":\"/note\",\"value\":\"discard\"},{\"op\":\"test\",\"path\":\"/label\",\"value\":\"wrong\"}]",
    "[{\"op\":\"replace\",\"path\":\"/seeds\",\"value\":{}}]"
})
{
    using var response = await SendAsync(input);
    Require(response.StatusCode == HttpStatusCode.BadRequest, "Unsafe/invalid patch accepted: " + input);
    Console.WriteLine("REJECT " + input);
}
foreach (var property in new[] { typeof(OperationBase).GetProperty("from")!, typeof(Operation).GetProperty("value")! })
    Console.WriteLine($"METADATA {property.DeclaringType!.Name}.{property.Name}: {string.Join(";", property.GetCustomAttributesData())}");
await app.StopAsync(deadline.Token);
Console.WriteLine($"PASS GlasshousePatch {System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}: all six operations; actual wire from/value/null, deserialize/apply HTTP controls and rejected failures. Newtonsoft path is separate and unchanged.");

Task<HttpResponseMessage> SendAsync(string body) => client.SendAsync(new HttpRequestMessage(HttpMethod.Patch, new Uri(client.BaseAddress!, "/garden"))
{
    Content = new StringContent(body, Encoding.UTF8, "application/json-patch+json")
}, deadline.Token);

static bool NeedsFrom(OperationBase operation) => operation.ShouldSerializeFrom();
static void Require(bool condition, string message) { if (!condition) throw new Exception(message); }

sealed class GardenRecord
{
    public string? Label { get; set; } = "fern";
    public string? Note { get; set; } = "initial";
    public string? Backup { get; set; }
    public int Seeds { get; set; } = 12;
}
// endsnippet:complete
