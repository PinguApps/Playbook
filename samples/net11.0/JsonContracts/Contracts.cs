using System.Numerics;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using System.Text.Json.Serialization;

namespace Playbook.Samples;

// snippet:conventions
[JsonNamingPolicy(JsonKnownNamingPolicy.SnakeCaseLower)]
[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
public sealed class FerryEntry
{
    [JsonPropertyName("deck_code")]
    [JsonNamingPolicy(JsonKnownNamingPolicy.KebabCaseLower)]
    public string DeckCode { get; set; } = "A3";
    [JsonNamingPolicy(JsonKnownNamingPolicy.CamelCase)]
    public int LoadCount { get; set; }
    public string SailingName { get; set; } = "Dawn";
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
        PropertyNamingPolicy = JsonNamingPolicy.PascalCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
    };
    public static string Write() => JsonSerializer.Serialize(new FerryEntry(), Options);
}
// endsnippet:conventions

// snippet:values
public sealed record CargoMeasure(Decimal32 Small, Decimal64 Mass, Decimal128 Total, BFloat16 Sensor);
public sealed class CargoManifest
{
    public IReadOnlySet<int> Bays { get; set; } = new HashSet<int>();
    [JsonExtensionData]
    public IReadOnlyDictionary<string, JsonElement> Extra { get; set; } = new Dictionary<string, JsonElement>
    {
        ["retained"] = JsonSerializer.SerializeToElement(8),
        ["revision"] = JsonSerializer.SerializeToElement(1)
    };
}
public sealed class CargoNode
{
    public int Id { get; set; }
    [JsonExtensionData]
    public JsonObject Extra { get; set; } = [];
}
[JsonSerializable(typeof(CargoManifest))]
public partial class CargoContext : JsonSerializerContext;
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
        // Built-in binary contracts now supply contentEncoding: base64.
        return options.GetJsonSchemaAsNode(typeof(SealImage));
    }
}
// endsnippet:schema
