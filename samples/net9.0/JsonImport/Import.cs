using System.Text.Json;
using System.Text.Json.Serialization;

namespace Playbook.Samples;

// snippet:import
public sealed record BerthImport(string Vessel, int Crates);
public static class BerthReader
{
    private static readonly JsonSerializerOptions Options = new()
    {
        RespectRequiredConstructorParameters = true,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        RespectNullableAnnotations = true
    };

    public static BerthImport Read(string json)
    {
        // Duplicate rejection is explicit on .NET 9. This flat contract is small;
        // the DOM pass is bounded by an application input-size limit.
        using JsonDocument document = JsonDocument.Parse(json);
        if (document.RootElement.ValueKind != JsonValueKind.Object) throw new JsonException("Expected an import object.");
        HashSet<string> names = new(StringComparer.Ordinal);
        foreach (JsonProperty property in document.RootElement.EnumerateObject())
            if (!names.Add(property.Name)) throw new JsonException("Duplicate import field.");
        return JsonSerializer.Deserialize<BerthImport>(json, Options) ?? throw new JsonException("Null import.");
    }
}
// endsnippet:import
