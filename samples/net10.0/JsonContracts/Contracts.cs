using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using System.Text.Json.Serialization;

namespace Playbook.Samples;

// snippet:conventions
public sealed class FerryEntry
{
    [JsonPropertyName("deck_code")]
    public string DeckCode { get; set; } = "A3";
    [JsonPropertyName("loadCount")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public int LoadCount { get; set; }
    [JsonPropertyName("sailing_name")]
    public string SailingName { get; set; } = "Dawn";
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Notes { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public string? Audit { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int Attempts { get; set; }
}
public static class FerryJson
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
    };
    public static string Write() => JsonSerializer.Serialize(new FerryEntry(), Options);
}
// endsnippet:conventions

// snippet:values
// .NET 10 has no BFloat16/IEEE decimal types to round-trip. These use ordinary
// decimal/float representations; they do not emulate those new numeric formats.
public sealed record CargoMeasure(decimal Mass, float Sensor);
public sealed class CargoManifest
{
    public HashSet<int> Bays { get; set; } = [];
    [JsonExtensionData]
    public Dictionary<string, JsonElement> Extra { get; set; } = [];
}
// endsnippet:values

// snippet:schema
// The sample intentionally exposes binary buffers to compare all three JSON contracts.
#pragma warning disable CA1819
public sealed record SealImage(byte[] Bytes, Memory<byte> Scratch, ReadOnlyMemory<byte> Original);
#pragma warning restore CA1819
public static class ImageSchema
{
    public static JsonNode Export()
    {
        JsonSerializerOptions options = new(JsonSerializerOptions.Default);
        // .NET 10 needs the explicit annotation for binary JSON strings.
        return options.GetJsonSchemaAsNode(typeof(SealImage), new JsonSchemaExporterOptions
        {
            TransformSchemaNode = (context, schema) =>
            {
                Type type = context.TypeInfo.Type;
                if (type == typeof(byte[]) || type == typeof(Memory<byte>) || type == typeof(ReadOnlyMemory<byte>))
                    schema["contentEncoding"] = "base64";
                return schema;
            }
        });
    }
}
// endsnippet:schema
