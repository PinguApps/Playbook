// snippet:checks
using CwmLab;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
#if LEGACY_PLATFORM
using PayloadException = Newtonsoft.Json.JsonException;
#else
using PayloadException = System.Text.Json.JsonException;
#endif

var serializer = JsonDataSerializer.Instance;
Console.WriteLine($"runtime={Environment.Version};platform={typeof(Message).Assembly.FullName}");
if (args.Length == 2 && args[0] == "--read")
{
    TypedBridge.Read(File.ReadAllText(args[1])).Check();
    Console.WriteLine("PASS foreign-wire");
    return;
}
foreach (var version in new[] { 1, 2 })
{
    var wire = serializer.SerializePayload(TypedBridge.Completion, LabRun.Completed(), version);
    if (args.Length == 2 && args[0] == "--write") File.WriteAllText($"{args[1]}-v{version}.json", wire);
    TypedBridge.Read(wire).Check();
#if LEGACY_PLATFORM
    LegacyReader.Read(wire, version == 1).Check();
#else
    var message = serializer.DeserializeMessage(wire);
    if (message.Version != (version == 1 ? 0 : version) || message.MessageType != TypedBridge.Completion
        || message.RawMessage != wire || message.ToString() != wire)
        throw new InvalidDataException("Header/raw-message semantics changed.");
#endif
    Console.WriteLine($"PASS protocol={version};typed-platform-graph;batch=vial-204");
}
SurfaceChecks.Verify();
if (serializer.DeserializePayload<LabRun>(null) is not null) throw new InvalidDataException();
#if !LEGACY_PLATFORM
if (serializer.DeserializePayload<LabRun>(new Message()) is not null) throw new InvalidDataException();
#endif

// snippet:failures
foreach (var payload in new[] { "null", "", "123", "{}" })
{
    var wire = "{\"Version\":2,\"MessageType\":\"CwmLab.BatchCompleted\",\"Payload\":" + payload + "}";
    try { TypedBridge.Read(wire); throw new InvalidOperationException("Expected rejection."); }
    catch (Exception e) when (e is PayloadException or InvalidDataException) { Console.WriteLine($"rejected={payload};exception={e.GetType().FullName}"); }
}
// endsnippet:failures

var truncated = "{\"Version\":2,\"MessageType\":\"CwmLab.BatchCompleted\",\"Payload\":";
var header = serializer.DeserializeMessage(truncated);
if (header.MessageType != TypedBridge.Completion) throw new InvalidDataException();
Reject<PayloadException>(() => serializer.DeserializePayload<LabRun>(header));
Console.WriteLine("PASS fast-header-is-not-payload-validation");
Reject<PayloadException>(() => serializer.DeserializeMessage("{"));
Reject<InvalidDataException>(() => TypedBridge.Read("{\"Version\":99,\"MessageType\":\"CwmLab.BatchCompleted\",\"Payload\":{}}"));
Reject<InvalidDataException>(() => TypedBridge.Read(serializer.SerializePayload("OtherLab.Event", LabRun.Completed(), 2)));
Reject<NotSupportedException>(() => serializer.SerializePayload(TypedBridge.Completion, LabRun.Completed(), 99));
var unknown = serializer.DeserializeMessage("{\"Version\":99,\"MessageType\":\"CwmLab.BatchCompleted\",\"Payload\":{}}");
Reject<NotSupportedException>(() => serializer.DeserializePayload<LabRun>(unknown));
// Force the ordinary JSON header parser, rather than the canonical fast layout.
var reordered = serializer.SerializePayload(TypedBridge.Completion, LabRun.Completed(), 2);
TypedBridge.Read(" \n" + reordered).Check();
Console.WriteLine("PASS header-fallback;unknown-kind-and-version-rejected");

// Every surviving public serializer operation is exercised, not inferred from metadata.
serializer.Deserialize<LabRun>(serializer.Serialize(LabRun.Completed(), 2), 2)!.Check();
serializer.Clone(LabRun.Completed())!.Check();
if (serializer.Clone<LabRun>(null) is not null) throw new InvalidDataException();
if (serializer.DeserializeMessage(serializer.SerializeMessage("CwmLab.Ping")).MessageType != "CwmLab.Ping") throw new InvalidDataException();
TypedBridge.Read(serializer.SerializePayload(TypedBridge.Completion, LabRun.Completed())).Check();
Console.WriteLine("PASS retained-serializer-members");

// The platform's supported statistics converter targets ITestRunStatistics.
var statistics = new TestRunStatistics(1, new Dictionary<TestOutcome, long> { [TestOutcome.Passed] = 1 });
var statsWire = serializer.SerializePayload("CwmLab.RunTotals", statistics, 2);
var statsMessage = serializer.DeserializeMessage(statsWire);
var contract = serializer.DeserializePayload<ITestRunStatistics>(statsMessage)!;
if (contract.ExecutedTests != 1 || contract[TestOutcome.Passed] != 1) throw new InvalidDataException();
var concrete = serializer.DeserializePayload<TestRunStatistics>(statsMessage)!;
#if LEGACY_PLATFORM
if (concrete[TestOutcome.Passed] != 1) throw new InvalidDataException();
#else
if (concrete[TestOutcome.Passed] != 0) throw new InvalidDataException("Refresh the documented concrete-type limitation.");
#endif
Console.WriteLine($"PASS statistics-interface;concrete-passed={concrete[TestOutcome.Passed]}");
Console.WriteLine("PASS laboratory-bridge");

static void Reject<T>(Action action) where T : Exception
{
    try { action(); }
    catch (T) { return; }
    throw new InvalidOperationException($"Expected {typeof(T).Name}.");
}
// endsnippet:checks


