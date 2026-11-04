using System.Text.Json;
using TideSync;

var checks = 0;
void Equal<T>(T expected, T actual)
{
    checks++;
    if (!EqualityComparer<T>.Default.Equals(expected, actual))
        throw new Exception($"Expected {expected}, got {actual}.");
}
void Throws<T>(Action action) where T : Exception
{
    checks++;
    try { action(); }
    catch (T) { return; }
    throw new Exception($"Expected {typeof(T).Name}.");
}

(SyncOutcome Outcome, string Description)[] examples =
[
    (SyncWorkflow.Apply("r17"), "applied:r17"),
    (SyncWorkflow.Skip("r18"), "unchanged:r18"),
    (SyncWorkflow.Reject("station offline"), "rejected:station offline"),
    (default, "absent")
];
foreach (var (outcome, description) in examples)
{
    Equal(description, SyncWorkflow.Describe(outcome));
    var json = SyncWorkflow.Serialize(outcome);
    var restored = SyncWorkflow.Deserialize(json);
    Equal(description, SyncWorkflow.Describe(restored));
    Equal(outcome, restored);
    Equal(json, JsonSerializer.Serialize(restored));
    Console.WriteLine($"{description} -> {json} -> {SyncWorkflow.Describe(restored)}");
}
Equal("null", SyncWorkflow.Serialize(default));
Equal("absent", SyncWorkflow.Describe(SyncWorkflow.Deserialize(" null ")));
Equal("absent", SyncWorkflow.Describe(new SyncOutcome()));

// Check payload preservation, including empty strings and JSON escaping.
foreach (var payload in new[] { "", "quote\" slash\\ newline\n tide \u6f6e \ud83c\udf0a" })
{
    foreach (var outcome in new[] { SyncWorkflow.Apply(payload), SyncWorkflow.Skip(payload), SyncWorkflow.Reject(payload) })
        Equal(outcome, SyncWorkflow.Deserialize(SyncWorkflow.Serialize(outcome)));
}
Equal("applied:r1", SyncWorkflow.Describe(SyncWorkflow.Deserialize("{\"revision\":\"r1\",\"case\":\"applied\"}")));

string[] invalid =
[
    "", " ", "{", "null null", "[]", "true", "42", "\"applied\"", "{}",
    "{\"case\":\"unknown\",\"revision\":\"r1\"}",
    "{\"case\":\"Applied\",\"revision\":\"r1\"}",
    "{\"case\":null,\"revision\":\"r1\"}",
    "{\"case\":1,\"revision\":\"r1\"}",
    "{\"case\":\"applied\"}",
    "{\"revision\":\"r1\"}",
    "{\"case\":\"applied\",\"revision\":null}",
    "{\"case\":\"applied\",\"revision\":5}",
    "{\"case\":\"unchanged\",\"revision\":[]}",
    "{\"case\":\"rejected\",\"reason\":null}",
    "{\"case\":\"rejected\",\"revision\":\"r1\"}",
    "{\"case\":\"applied\",\"reason\":\"failure\"}",
    "{\"case\":\"applied\",\"revision\":\"r1\",\"reason\":\"failure\"}",
    "{\"case\":\"applied\",\"case\":\"rejected\",\"revision\":\"r1\"}",
    "{\"case\":\"applied\",\"revision\":\"r1\",\"revision\":\"r2\"}",
    "{\"case\":\"applied\",\"revision\":\"r1\",\"extra\":true}",
    "{\"case\":\"applied\",\"revision\":\"r1\",}",
    "{/*comment*/\"case\":\"applied\",\"revision\":\"r1\"}"
];
foreach (var json in invalid)
    Throws<JsonException>(() => SyncWorkflow.Deserialize(json));

Throws<ArgumentNullException>(() => SyncWorkflow.Apply(null!));
Throws<ArgumentNullException>(() => SyncWorkflow.Skip(null!));
Throws<ArgumentNullException>(() => SyncWorkflow.Reject(null!));
Throws<ArgumentNullException>(() => { SyncOutcome outcome = (Applied)null!; });
Throws<ArgumentNullException>(() => { SyncOutcome outcome = (Unchanged)null!; });
Throws<ArgumentNullException>(() => { SyncOutcome outcome = (Rejected)null!; });
Throws<ArgumentNullException>(() => examples[0].Outcome.Match(
    _ => "applied", _ => "unchanged", (Func<Rejected, string>)null!, () => "absent"));

// All four handlers are mandatory; rejection and absence have distinct paths.
Equal("failure:blocked", SyncWorkflow.Reject("blocked").Match(
    _ => "success", _ => "unchanged", r => $"failure:{r.Reason}", () => "missing"));
Equal("missing", default(SyncOutcome).Match(
    _ => "success", _ => "unchanged", _ => "failure", () => "missing"));
Console.WriteLine($"Passed {checks} checks, including {invalid.Length} invalid JSON inputs.");
