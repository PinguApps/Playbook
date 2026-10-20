using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Playbook.Samples;

#region outcomes
public sealed record ArchiveReceipt([property: JsonRequired] string ShelfCode);
public sealed record ArchiveRefusal([property: JsonRequired] string ReasonCode);

[JsonUnion(TypeClassifier = typeof(JsonUnionTypeStructuralClassifier))]
public union Outcome(ArchiveReceipt, ArchiveRefusal);

public static class OutcomeWorkflow
{
    public static Outcome Receipt(string shelf) => new ArchiveReceipt(shelf);
    public static Outcome Refusal(string reason) => new ArchiveRefusal(reason);
    public static string Describe(Outcome outcome) => outcome switch
    {
        ArchiveReceipt receipt => "stored:" + receipt.ShelfCode,
        ArchiveRefusal refusal => "refused:" + refusal.ReasonCode,
        null => "unassigned"
    };

    public static string Serialize(Outcome outcome) => JsonSerializer.Serialize(outcome, ArchiveJson.Default.Outcome);
    public static Outcome Deserialize(string json) => JsonSerializer.Deserialize(json, ArchiveJson.Default.Outcome);
}
#endregion

#region hierarchy
[JsonPolymorphic(InferClosedTypePolymorphism = true)]
public closed record IntakeState;
public sealed record Waiting(int QueueNumber) : IntakeState;
public sealed record Catalogued(string ShelfCode) : IntakeState;

public static class StateWorkflow
{
    public static string Describe(IntakeState? state) => state switch
    {
        Waiting waiting => "queued:" + waiting.QueueNumber,
        Catalogued catalogued => "catalogued:" + catalogued.ShelfCode,
        null => "absent"
    };
    public static string Serialize(IntakeState? state) => JsonSerializer.Serialize(state, ArchiveJson.Default.IntakeState);
    public static IntakeState? Deserialize(string json) => JsonSerializer.Deserialize(json, ArchiveJson.Default.IntakeState);
}
#endregion

internal static class RestrictionChecks
{
    private static readonly JsonSerializerOptions RuntimeOnly = new()
    {
        InferClosedTypePolymorphism = true,
        TypeInfoResolver = MissingInferenceJson.Default
    };

    public static void Run()
    {
        // Explicit registrations replace inference, rather than adding to it.
        var options = new JsonSerializerOptions { TypeInfoResolver = new DefaultJsonTypeInfoResolver() };
        JsonTypeInfo info = options.GetTypeInfo(typeof(RegisteredState));
        if (info.PolymorphismOptions?.DerivedTypes.Count != 1 ||
            info.PolymorphismOptions.DerivedTypes[0].DerivedType != typeof(RegisteredWaiting))
        {
            throw new InvalidOperationException("Explicit registration did not replace inference.");
        }

        // A runtime opt-in cannot repair source-generated metadata that omitted inference.
        try { JsonSerializer.Serialize<UnconfiguredState>(new UnconfiguredWaiting(17), RuntimeOnly); }
        catch (InvalidOperationException) { return; }
        throw new InvalidOperationException("Incomplete generated metadata was accepted.");
    }
}

#region context
[JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Metadata,
    InferClosedTypePolymorphism = true)]
[JsonSerializable(typeof(Outcome))]
[JsonSerializable(typeof(IntakeState))]
internal partial class ArchiveJson : JsonSerializerContext;
#endregion

[JsonPolymorphic(InferClosedTypePolymorphism = true)]
[JsonDerivedType(typeof(RegisteredWaiting), "waiting")]
public closed record RegisteredState;
public sealed record RegisteredWaiting : RegisteredState;
public sealed record RegisteredCatalogued : RegisteredState;

public closed record UnconfiguredState;
public sealed record UnconfiguredWaiting(int QueueNumber) : UnconfiguredState;
[JsonSerializable(typeof(UnconfiguredState))]
internal partial class MissingInferenceJson : JsonSerializerContext;
