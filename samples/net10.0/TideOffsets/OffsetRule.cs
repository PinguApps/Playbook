// snippet:rule
using System.Text.Json;

namespace TideOffsets;

// Shared strict value rule: both editions accept precisely the same payloads.
public static class OffsetRule
{
    public static int? Read(JsonElement root)
    {
        if (root.ValueKind != JsonValueKind.Object) return null;
        int? result = null;
        foreach (var property in root.EnumerateObject())
        {
            if (property.Name != "Offset") continue;
            if (result.HasValue || property.Value.ValueKind != JsonValueKind.Number || !property.Value.TryGetInt32(out int value)) return null;
            result = value;
        }
        return result;
    }
}
// endsnippet:rule
