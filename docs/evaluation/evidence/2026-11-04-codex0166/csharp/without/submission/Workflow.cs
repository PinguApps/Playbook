using System.Text.Json;
using System.Text.Json.Serialization;

namespace TideSync;

public sealed record Applied(string Revision);
public sealed record Unchanged(string Revision);
public sealed record Rejected(string Reason);

[JsonConverter(typeof(SyncOutcomeJsonConverter))]
public readonly struct SyncOutcome
{
    private readonly Applied? applied;
    private readonly Unchanged? unchanged;
    private readonly Rejected? rejected;

    private SyncOutcome(Applied value) => applied = value;
    private SyncOutcome(Unchanged value) => unchanged = value;
    private SyncOutcome(Rejected value) => rejected = value;

    public static implicit operator SyncOutcome(Applied value)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(value.Revision);
        return new(value);
    }

    public static implicit operator SyncOutcome(Unchanged value)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(value.Revision);
        return new(value);
    }

    public static implicit operator SyncOutcome(Rejected value)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(value.Reason);
        return new(value);
    }

    // Every handler is required, including absence. No public constructor can
    // create multiple active cases or admit an unrelated value.
    public T Match<T>(Func<Applied, T> onApplied, Func<Unchanged, T> onUnchanged,
        Func<Rejected, T> onRejected, Func<T> onAbsent)
    {
        ArgumentNullException.ThrowIfNull(onApplied);
        ArgumentNullException.ThrowIfNull(onUnchanged);
        ArgumentNullException.ThrowIfNull(onRejected);
        ArgumentNullException.ThrowIfNull(onAbsent);

        if (applied is not null) return onApplied(applied);
        if (unchanged is not null) return onUnchanged(unchanged);
        if (rejected is not null) return onRejected(rejected);
        return onAbsent();
    }
}

public sealed class SyncOutcomeJsonConverter : JsonConverter<SyncOutcome>
{
    public override bool HandleNull => true;

    public override SyncOutcome Read(ref Utf8JsonReader reader, Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null) return default;
        using var document = JsonDocument.ParseValue(ref reader);
        if (document.RootElement.ValueKind != JsonValueKind.Object)
            throw new JsonException("Expected an outcome object or null.");

        var properties = new Dictionary<string, JsonElement>(StringComparer.Ordinal);
        foreach (var property in document.RootElement.EnumerateObject())
        {
            if (!properties.TryAdd(property.Name, property.Value))
                throw new JsonException($"Duplicate property: {property.Name}.");
        }

        if (properties.Count != 2 || !properties.TryGetValue("case", out var caseElement)
            || caseElement.ValueKind != JsonValueKind.String)
            throw new JsonException("Expected exactly a case and its payload.");

        var caseName = caseElement.GetString();
        var payloadName = caseName switch
        {
            "applied" or "unchanged" => "revision",
            "rejected" => "reason",
            _ => throw new JsonException("Unknown outcome case.")
        };
        if (!properties.TryGetValue(payloadName, out var payload)
            || payload.ValueKind != JsonValueKind.String)
            throw new JsonException($"Expected a string {payloadName}.");

        var value = payload.GetString()!;
        return caseName switch
        {
            "applied" => new Applied(value),
            "unchanged" => new Unchanged(value),
            "rejected" => new Rejected(value),
            _ => throw new JsonException("Unknown outcome case.")
        };
    }

    public override void Write(Utf8JsonWriter writer, SyncOutcome value,
        JsonSerializerOptions options)
    {
        value.Match(
            applied => WriteCase(writer, "applied", "revision", applied.Revision),
            unchanged => WriteCase(writer, "unchanged", "revision", unchanged.Revision),
            rejected => WriteCase(writer, "rejected", "reason", rejected.Reason),
            () => { writer.WriteNullValue(); return true; });
    }

    private static bool WriteCase(Utf8JsonWriter writer, string caseName,
        string payloadName, string payload)
    {
        writer.WriteStartObject();
        writer.WriteString("case", caseName);
        writer.WriteString(payloadName, payload);
        writer.WriteEndObject();
        return true;
    }
}

public static class SyncWorkflow
{
    public static SyncOutcome Apply(string revision) => new Applied(revision);
    public static SyncOutcome Skip(string revision) => new Unchanged(revision);
    public static SyncOutcome Reject(string reason) => new Rejected(reason);
    public static string Describe(SyncOutcome result) => result.Match(
        applied => $"applied:{applied.Revision}",
        unchanged => $"unchanged:{unchanged.Revision}",
        rejected => $"rejected:{rejected.Reason}",
        () => "absent");
    public static string Serialize(SyncOutcome result) => JsonSerializer.Serialize(result);
    public static SyncOutcome Deserialize(string json) => JsonSerializer.Deserialize<SyncOutcome>(json);
}
