// snippet:implementation
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TideOffsets;

[JsonConverter(typeof(OffsetConverter))]
public abstract record TideOffset(string Station, int Offset);
public sealed record AboveDatum(string Station, int Offset) : TideOffset(Station, Offset);
public sealed record BelowDatum(string Station, int Offset) : TideOffset(Station, Offset);

public sealed class OffsetConverter : JsonConverter<TideOffset>
{
    public override TideOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var lookahead = reader;
        using var document = JsonDocument.ParseValue(ref lookahead);
        int? offset = OffsetRule.Read(document.RootElement);
        return offset switch
        {
            > 0 => JsonSerializer.Deserialize(ref reader, OffsetJson.Default.AboveDatum)!,
            < 0 => JsonSerializer.Deserialize(ref reader, OffsetJson.Default.BelowDatum)!,
            _ => throw new JsonException("Expected a nonzero Int32 Offset.")
        };
    }

    public override void Write(Utf8JsonWriter writer, TideOffset value, JsonSerializerOptions options) =>
        throw new NotSupportedException("This import-only converter does not write messages.");
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
