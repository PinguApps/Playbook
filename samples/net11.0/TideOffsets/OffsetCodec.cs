// snippet:implementation
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TideOffsets;

[JsonPolymorphic(TypeClassifier = typeof(OffsetClassifier))]
[JsonDerivedType(typeof(AboveDatum))]
[JsonDerivedType(typeof(BelowDatum))]
public abstract record TideOffset(string Station, int Offset);
public sealed record AboveDatum(string Station, int Offset) : TideOffset(Station, Offset);
public sealed record BelowDatum(string Station, int Offset) : TideOffset(Station, Offset);

public sealed class OffsetClassifier : JsonTypeClassifierFactory<TideOffset>
{
    public override JsonTypeClassifier CreateJsonClassifier(JsonTypeClassifierContext context, JsonSerializerOptions options)
    {
        // Validate the immutable configuration once; capture only registered candidates.
        Type above = context.DerivedTypes.Single(t => t.DerivedType == typeof(AboveDatum)).DerivedType;
        Type below = context.DerivedTypes.Single(t => t.DerivedType == typeof(BelowDatum)).DerivedType;
        return (ref Utf8JsonReader reader) =>
        {
            // The runtime supplies a defensive copy. Another copy makes our lookahead explicit.
            var lookahead = reader;
            using var document = JsonDocument.ParseValue(ref lookahead);
            return OffsetRule.Read(document.RootElement) switch { > 0 => above, < 0 => below, _ => null };
        };
    }
}

public static class OffsetCodec
{
    public static TideOffset? Read(string json) => JsonSerializer.Deserialize(json, OffsetJson.Default.TideOffset);
}

[JsonSerializable(typeof(TideOffset))]
[JsonSerializable(typeof(AboveDatum))]
[JsonSerializable(typeof(BelowDatum))]
public partial class OffsetJson : JsonSerializerContext;
// endsnippet:implementation
