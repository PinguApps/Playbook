using System.Text.Json;
using System.Text.Json.Serialization;

namespace TideSync;

public sealed record Applied(string Revision);
public sealed record Unchanged(string Revision);
public sealed record Rejected(string Reason);

[JsonConverter(typeof(SyncOutcomeConverter))]
public union SyncOutcome(Applied, Unchanged, Rejected);

public static class SyncWorkflow
{
    public static SyncOutcome Apply(string revision) => new Applied(RequirePayload(revision));
    public static SyncOutcome Skip(string revision) => new Unchanged(RequirePayload(revision));
    public static SyncOutcome Reject(string reason) => new Rejected(RequirePayload(reason));

    public static string Describe(SyncOutcome result) => result switch
    {
        Applied applied => "applied:" + RequirePayload(applied.Revision),
        Unchanged unchanged => "unchanged:" + RequirePayload(unchanged.Revision),
        Rejected rejected => "rejected:" + RequirePayload(rejected.Reason),
        null => "absent"
    };

    public static string Serialize(SyncOutcome result) => JsonSerializer.Serialize(result);
    public static SyncOutcome Deserialize(string json) => JsonSerializer.Deserialize<SyncOutcome>(json);

    internal static string RequirePayload(string payload) =>
        payload ?? throw new ArgumentNullException(nameof(payload));
}

// A tag is essential: Applied and Unchanged both have a Revision property.
// Keep validation here so direct JsonSerializer calls use the same strict contract.
public sealed class SyncOutcomeConverter : JsonConverter<SyncOutcome>
{
    public override SyncOutcome Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null) return default;
        using JsonDocument document = JsonDocument.ParseValue(ref reader);
        JsonElement root = document.RootElement;
        if (root.ValueKind != JsonValueKind.Object) throw new JsonException("Expected an outcome object or null.");

        var properties = new Dictionary<string, JsonElement>(StringComparer.Ordinal);
        foreach (JsonProperty property in root.EnumerateObject())
        {
            if (!properties.TryAdd(property.Name, property.Value))
                throw new JsonException("Duplicate outcome property.");
        }

        if (properties.Count != 2 || !properties.TryGetValue("case", out JsonElement tag) ||
            tag.ValueKind != JsonValueKind.String)
            throw new JsonException("Expected exactly a case tag and its payload.");

        string payloadName = tag.GetString() switch
        {
            "applied" or "unchanged" => "revision",
            "rejected" => "reason",
            _ => throw new JsonException("Unknown outcome case.")
        };
        if (!properties.TryGetValue(payloadName, out JsonElement payload) || payload.ValueKind != JsonValueKind.String)
            throw new JsonException("Expected a non-null string payload for the selected case.");

        return tag.GetString() switch
        {
            "applied" => new Applied(payload.GetString()!),
            "unchanged" => new Unchanged(payload.GetString()!),
            "rejected" => new Rejected(payload.GetString()!),
            _ => throw new JsonException("Unknown outcome case.")
        };
    }

    public override void Write(Utf8JsonWriter writer, SyncOutcome value, JsonSerializerOptions options)
    {
        // An expression makes additions to the union visible to exhaustiveness analysis.
        (string Tag, string Name, string Payload)? fields = value switch
        {
            Applied applied => ("applied", "revision", SyncWorkflow.RequirePayload(applied.Revision)),
            Unchanged unchanged => ("unchanged", "revision", SyncWorkflow.RequirePayload(unchanged.Revision)),
            Rejected rejected => ("rejected", "reason", SyncWorkflow.RequirePayload(rejected.Reason)),
            null => null
        };
        if (fields is not { } active)
        {
            writer.WriteNullValue();
            return;
        }
        writer.WriteStartObject();
        writer.WriteString("case", active.Tag);
        writer.WriteString(active.Name, active.Payload);
        writer.WriteEndObject();
    }
}
