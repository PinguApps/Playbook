using System.Text.Json;
using System.Text.Json.Serialization;

namespace Playbook.Samples;

// snippet:outcomes
public sealed record ArchiveReceipt([property: JsonRequired] string ShelfCode);
public sealed record ArchiveRefusal([property: JsonRequired] string ReasonCode);

[JsonConverter(typeof(OutcomeConverter))]
public readonly struct Outcome
{
    public Outcome(ArchiveReceipt receipt) => Value = receipt;
    public Outcome(ArchiveRefusal refusal) => Value = refusal;
    public object? Value { get; }
}

public static class OutcomeWorkflow
{
    public static Outcome Receipt(string shelf) => new(new ArchiveReceipt(shelf));
    public static Outcome Refusal(string reason) => new(new ArchiveRefusal(reason));
    public static string Describe(Outcome outcome) => outcome.Value switch
    {
        ArchiveReceipt receipt => "stored:" + receipt.ShelfCode,
        ArchiveRefusal refusal => "refused:" + refusal.ReasonCode,
        null => "unassigned",
        _ => throw new InvalidOperationException("Unexpected outcome type.")
    };

    public static string Serialize(Outcome outcome) => JsonSerializer.Serialize(outcome, OutcomeJson.Default.Outcome);
    public static Outcome Deserialize(string json) => JsonSerializer.Deserialize(json, OutcomeJson.Default.Outcome);
}
// endsnippet:outcomes

// snippet:converter
public sealed class OutcomeConverter : JsonConverter<Outcome>
{
    public override Outcome Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null) { return default; }
        using JsonDocument document = JsonDocument.ParseValue(ref reader);
        JsonElement root = document.RootElement;
        if (root.ValueKind != JsonValueKind.Object) { throw new JsonException("Expected an archive outcome object."); }
        bool receipt = root.TryGetProperty("ShelfCode", out _);
        bool refusal = root.TryGetProperty("ReasonCode", out _);
        if (receipt == refusal) { throw new JsonException("Expected exactly one archive outcome case."); }
        return receipt
            ? new Outcome(root.Deserialize(OutcomeJson.Default.ArchiveReceipt) ?? throw new JsonException())
            : new Outcome(root.Deserialize(OutcomeJson.Default.ArchiveRefusal) ?? throw new JsonException());
    }

    public override void Write(Utf8JsonWriter writer, Outcome value, JsonSerializerOptions options)
    {
        switch (value.Value)
        {
            case ArchiveReceipt receipt: JsonSerializer.Serialize(writer, receipt, OutcomeJson.Default.ArchiveReceipt); break;
            case ArchiveRefusal refusal: JsonSerializer.Serialize(writer, refusal, OutcomeJson.Default.ArchiveRefusal); break;
            case null: writer.WriteNullValue(); break;
            default: throw new JsonException("Unexpected outcome type.");
        }
    }
}
// endsnippet:converter

// snippet:hierarchy
[JsonDerivedType(typeof(Waiting), "waiting")]
[JsonDerivedType(typeof(Catalogued), "catalogued")]
public abstract record IntakeState
{
    private protected IntakeState() { }
}
public sealed record Waiting(int QueueNumber) : IntakeState;
public sealed record Catalogued(string ShelfCode) : IntakeState;

public static class StateWorkflow
{
    public static string Describe(IntakeState? state) => state switch
    {
        Waiting waiting => "queued:" + waiting.QueueNumber,
        Catalogued catalogued => "catalogued:" + catalogued.ShelfCode,
        null => "absent",
        _ => throw new InvalidOperationException("Unexpected intake state.")
    };
    public static string Serialize(IntakeState? state) => JsonSerializer.Serialize(state, StateJson.Default.IntakeState);
    public static IntakeState? Deserialize(string json) => JsonSerializer.Deserialize(json, StateJson.Default.IntakeState);
}
// endsnippet:hierarchy

// snippet:outcome-context
[JsonSerializable(typeof(Outcome))]
[JsonSerializable(typeof(ArchiveReceipt))]
[JsonSerializable(typeof(ArchiveRefusal))]
internal partial class OutcomeJson : JsonSerializerContext;
// endsnippet:outcome-context

// snippet:state-context
[JsonSerializable(typeof(IntakeState))]
internal partial class StateJson : JsonSerializerContext;
// endsnippet:state-context
