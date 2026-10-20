using System.Text.Json;
using Playbook.Samples;

Outcome receipt = OutcomeWorkflow.Receipt("C-24");
Outcome refusal = OutcomeWorkflow.Refusal("humidity");
foreach ((Outcome value, string expected) in new[] { (receipt, "stored:C-24"), (refusal, "refused:humidity") })
{
    Require(OutcomeWorkflow.Describe(value) == expected, "matching preserves the case");
    string json = OutcomeWorkflow.Serialize(value);
    Require(OutcomeWorkflow.Describe(OutcomeWorkflow.Deserialize(json)) == expected, "JSON preserves the case and payload");
    Console.WriteLine($"{expected} -> {json}");
}

Require(OutcomeWorkflow.Describe(default) == "unassigned", "default is explicitly handled");
Require(OutcomeWorkflow.Serialize(default) == "null", "default serializes as null");
Require(OutcomeWorkflow.Describe(OutcomeWorkflow.Deserialize("null")) == "unassigned", "JSON null returns the empty outcome");
ExpectJsonFailure("{}");
ExpectJsonFailure("{\"ShelfCode\":\"C-24\",\"ReasonCode\":\"humidity\"}");
ExpectJsonFailure("{\"Unknown\":true}");

foreach (IntakeState state in new IntakeState[] { new Waiting(17), new Catalogued("C-24") })
{
    string expected = StateWorkflow.Describe(state);
    string json = StateWorkflow.Serialize(state);
    Require(StateWorkflow.Describe(StateWorkflow.Deserialize(json)) == expected, "inferred/registered hierarchy round trip");
    Console.WriteLine($"{expected} -> {json}");
}

Require(StateWorkflow.Describe(null) == "absent", "null hierarchy is explicitly handled");
Require(StateWorkflow.Serialize(null) == "null", "null hierarchy serializes as null");
Require(StateWorkflow.Deserialize("null") is null, "null hierarchy round trip");
#if NET11_0_OR_GREATER
RestrictionChecks.Run();
#endif
Console.WriteLine($"PASS: outcome matching, JSON cases, null/default, ambiguity and hierarchy checks; runtime {Environment.Version}");

static void Require(bool condition, string claim)
{
    if (!condition) { throw new InvalidOperationException(claim); }
}

static void ExpectJsonFailure(string json)
{
    try { OutcomeWorkflow.Deserialize(json); }
    catch (JsonException) { return; }
    throw new InvalidOperationException($"Ambiguous/unknown input accepted: {json}");
}
