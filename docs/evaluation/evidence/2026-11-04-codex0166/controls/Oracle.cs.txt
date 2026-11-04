using System.Text.Json;
using TideSync;

var failures = new List<string>();
Check("matching applied", () => SyncWorkflow.Describe(SyncWorkflow.Apply("r17")) == "applied:r17");
Check("matching unchanged", () => SyncWorkflow.Describe(SyncWorkflow.Skip("r18")) == "unchanged:r18");
Check("matching rejected", () => SyncWorkflow.Describe(SyncWorkflow.Reject("checksum")) == "rejected:checksum");
Check("persistence applied", () => SyncWorkflow.Describe(SyncWorkflow.Deserialize(SyncWorkflow.Serialize(SyncWorkflow.Apply("r17")))) == "applied:r17");
Check("persistence unchanged", () => SyncWorkflow.Describe(SyncWorkflow.Deserialize(SyncWorkflow.Serialize(SyncWorkflow.Skip("r18")))) == "unchanged:r18");
Check("persistence rejected", () => SyncWorkflow.Describe(SyncWorkflow.Deserialize(SyncWorkflow.Serialize(SyncWorkflow.Reject("checksum")))) == "rejected:checksum");
Check("default", () => SyncWorkflow.Describe(default!) == "absent");
Check("null persistence", () => {
    using var json = JsonDocument.Parse(SyncWorkflow.Serialize(default!));
    return json.RootElement.ValueKind == JsonValueKind.Null;
});
Check("null restore", () => SyncWorkflow.Describe(SyncWorkflow.Deserialize("null")) == "absent");
foreach (string json in new[] { "{", "{}", "{\"Unknown\":true}" })
    Check("invalid JSON rejected: " + json, () => Rejects(json));
// Supply the submission's format-specific ambiguous document as one argument.
if (args.Length == 1) Check("ambiguous JSON rejected", () => Rejects(args[0]));
Console.WriteLine($"Oracle: {failures.Count} failed");
Environment.ExitCode = failures.Count == 0 ? 0 : 1;

bool Rejects(string json)
{
    try { return SyncWorkflow.Describe(SyncWorkflow.Deserialize(json)).StartsWith("rejected:", StringComparison.Ordinal); }
    catch (Exception) { return true; }
}
void Check(string claim, Func<bool> test)
{
    try {
        if (!test()) throw new InvalidOperationException("Unexpected result");
        Console.WriteLine("PASS: " + claim);
    }
    catch (Exception error) {
        failures.Add(claim);
        Console.WriteLine($"FAIL: {claim}: {error.GetType().Name}: {error.Message}");
    }
}
