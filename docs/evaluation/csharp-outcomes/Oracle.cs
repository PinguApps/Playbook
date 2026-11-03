using TideSync;

foreach (var (value, expected) in new[] {
    (SyncWorkflow.Apply("r17"), "applied:r17"),
    (SyncWorkflow.Skip("r18"), "unchanged:r18"),
    (SyncWorkflow.Reject("checksum"), "rejected:checksum") })
{
    Require(SyncWorkflow.Describe(value) == expected, "matching " + expected);
    Require(SyncWorkflow.Describe(SyncWorkflow.Deserialize(SyncWorkflow.Serialize(value))) == expected,
        "persistence " + expected);
}
Require(SyncWorkflow.Describe(default) == "absent", "default");
Require(SyncWorkflow.Serialize(default) == "null", "null persistence");
Require(SyncWorkflow.Describe(SyncWorkflow.Deserialize("null")) == "absent", "null restore");
foreach (string json in new[] { "{", "{}", "{\"Unknown\":true}" })
{
    bool rejected = false;
    try { SyncWorkflow.Deserialize(json); }
    catch (System.Text.Json.JsonException) { rejected = true; }
    catch (ArgumentException) { rejected = true; }
    Require(rejected, "invalid JSON rejected: " + json);
}
Console.WriteLine("PASS: three cases, payload persistence, default/null and invalid input");
static void Require(bool condition, string claim)
{
    if (!condition) throw new InvalidOperationException(claim);
}
