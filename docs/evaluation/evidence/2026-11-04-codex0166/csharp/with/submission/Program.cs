using System.Text.Json;
using TideSync;

static void Equal(string expected, string actual)
{
    if (actual != expected) throw new Exception($"Expected {expected}; got {actual}.");
}

foreach ((SyncOutcome outcome, string expected) in new (SyncOutcome, string)[]
{
    (SyncWorkflow.Apply("r17"), "applied:r17"),
    (SyncWorkflow.Skip("r17"), "unchanged:r17"),
    (SyncWorkflow.Reject("sensor offline"), "rejected:sensor offline"),
    (default, "absent")
})
{
    Equal(expected, SyncWorkflow.Describe(outcome));
    string json = SyncWorkflow.Serialize(outcome);
    Equal(expected, SyncWorkflow.Describe(SyncWorkflow.Deserialize(json)));
    Equal(json, SyncWorkflow.Serialize(SyncWorkflow.Deserialize(json)));
    Console.WriteLine($"{expected} -> {json}");
}
Equal("null", SyncWorkflow.Serialize(default));
Equal("absent", SyncWorkflow.Describe(SyncWorkflow.Deserialize(" \n null \t")));

// Include escaped, Unicode and empty payloads; there is no nonempty domain constraint.
foreach (string payload in new[] { "", "tide \"A\"\n\\", "marée 🌊" })
{
    foreach (SyncOutcome outcome in new[] { SyncWorkflow.Apply(payload), SyncWorkflow.Skip(payload), SyncWorkflow.Reject(payload) })
        Equal(SyncWorkflow.Describe(outcome), SyncWorkflow.Describe(SyncWorkflow.Deserialize(SyncWorkflow.Serialize(outcome))));
}

string[] invalid =
[
    "", "{", "{}", "[]", "42", "true", "\"applied\"", "null null",
    """{"revision":"r17"}""",
    """{"case":"unknown","revision":"r17"}""",
    """{"case":"Applied","revision":"r17"}""",
    """{"case":"applied"}""",
    """{"case":null,"revision":"r17"}""",
    """{"case":"applied","revision":null}""",
    """{"case":"unchanged","revision":17}""",
    """{"case":"rejected","reason":{}}""",
    """{"case":"rejected","revision":"r17"}""",
    """{"case":"applied","revision":"r17","reason":"failed"}""",
    """{"case":"applied","case":"rejected","reason":"failed"}""",
    """{"case":"applied","revision":"r17","revision":"r18"}""",
    """{"case":"applied","revision":"r17","extra":true}""",
    """{"case":"applied","revision":"r17",}"""
];
foreach (string json in invalid)
{
    try { SyncWorkflow.Deserialize(json); }
    catch (JsonException) { continue; }
    throw new Exception("Invalid persisted outcome was accepted: " + json);
}

// This SDK accepts the missing null arm even though default union storage is possible.
static string MissingAbsence(SyncOutcome outcome) => outcome switch
{
    Applied applied => applied.Revision,
    Unchanged unchanged => unchanged.Revision,
    Rejected rejected => rejected.Reason
};
bool absenceThrew = false;
try { MissingAbsence(default); }
catch (System.Runtime.CompilerServices.SwitchExpressionException)
{
    absenceThrew = true;
    Console.WriteLine("Missing absence arm: SwitchExpressionException (no compiler diagnostic).");
}
if (!absenceThrew) throw new Exception("Missing absence arm unexpectedly succeeded.");

// A catch-all suppresses exhaustiveness diagnostics; fail loudly if a caller uses one.
static string IncompleteCaller(SyncOutcome outcome) => outcome switch
{
    Applied applied => applied.Revision,
    Unchanged unchanged => unchanged.Revision,
    null => "absent",
    _ => throw new InvalidOperationException("Caller omitted an outcome case.")
};
try { IncompleteCaller(SyncWorkflow.Reject("failed")); }
catch (InvalidOperationException exception)
{
    Console.WriteLine("Missing-case handling: " + exception.Message);
    Console.WriteLine($"PASS: cases, payloads, absence, JSON round trips and {invalid.Length} invalid inputs.");
    return;
}
throw new Exception("An incomplete caller silently accepted rejection.");
