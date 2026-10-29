// snippet:adapter
using System.Xml.Linq;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

namespace TernTesting;

[FileExtension(".dll")]
[DefaultExecutorUri(Executor.Address)]
public sealed class Discoverer : ITestDiscoverer
{
    internal static IEnumerable<TestCase> Cases(string source)
    {
        foreach (var name in new[] { "Capacity", "DeliberateFailure", "Settings", "Cancellation" })
            yield return new TestCase($"Tern.Reservations.{name}", new Uri(Executor.Address), source);
    }
    public void DiscoverTests(IEnumerable<string> sources, IDiscoveryContext discoveryContext, IMessageLogger logger, ITestCaseDiscoverySink discoverySink)
    {
        foreach (var source in sources.Where(s => Path.GetFileName(s) == "Tern.TestAdapter.dll"))
            foreach (var test in Cases(source)) discoverySink.SendTestCase(test);
    }
}

[ExtensionUri(Address)]
public sealed class Executor : ITestExecutor, IDisposable
{
    public const string Address = "executor://tern/reservations/v1";
    private readonly ManualResetEventSlim canceled = new();
    public void Cancel() => canceled.Set();
    public void Dispose() => canceled.Dispose();
    public void RunTests(IEnumerable<string>? sources, IRunContext? runContext, IFrameworkHandle? frameworkHandle)
    {
        var filter = runContext?.GetTestCaseFilter(["FullyQualifiedName"], _ => TestCaseProperties.FullyQualifiedName);
        var selected = sources?.SelectMany(Discoverer.Cases).Where(test => filter?.MatchTestCase(test, _ => test.FullyQualifiedName) ?? true);
        Execute(selected, runContext, frameworkHandle, requireSettings: true);
    }
    public void RunTests(IEnumerable<TestCase>? tests, IRunContext? runContext, IFrameworkHandle? frameworkHandle)
        => Execute(tests, runContext, frameworkHandle, requireSettings: false);
    private void Execute(IEnumerable<TestCase>? tests, IRunContext? runContext, IFrameworkHandle? frameworkHandle, bool requireSettings)
    {
        ArgumentNullException.ThrowIfNull(tests);
        ArgumentNullException.ThrowIfNull(runContext);
        ArgumentNullException.ThrowIfNull(frameworkHandle);
        frameworkHandle.SendMessage(TestMessageLevel.Informational, $"TERN_PID:{Environment.ProcessId}");
        foreach (var test in tests)
        {
            if (canceled.IsSet) break;
            if (test.FullyQualifiedName.EndsWith("Cancellation", StringComparison.Ordinal))
            {
                frameworkHandle.SendMessage(TestMessageLevel.Informational, "TERN_READY");
                if (!canceled.Wait(TimeSpan.FromSeconds(45))) throw new TimeoutException("Cancel was not delivered");
                break;
            }
            frameworkHandle.RecordStart(test);
            var result = new TestResult(test);
            try
            {
                if (test.FullyQualifiedName.EndsWith("DeliberateFailure", StringComparison.Ordinal))
                    AssertEqual(12, Remaining(20, 7)); // Deliberate failing assertion; actual result is 13.
                else if (test.FullyQualifiedName.EndsWith("Capacity", StringComparison.Ordinal))
                    AssertEqual(13, Remaining(20, 7));
                else
                    CheckSettings(runContext.RunSettings?.SettingsXml, frameworkHandle, requireSettings);
                result.Outcome = TestOutcome.Passed;
            }
            catch (InvalidOperationException ex)
            {
                result.Outcome = TestOutcome.Failed;
                result.ErrorMessage = ex.Message;
            }
            frameworkHandle.RecordResult(result);
            frameworkHandle.RecordEnd(test, result.Outcome);
        }
    }
    private static int Remaining(int capacity, int reserved) => checked(capacity - reserved);
    private static void AssertEqual(int expected, int actual)
    {
        if (expected != actual) throw new InvalidOperationException($"expected={expected};actual={actual}");
    }
    private static void CheckSettings(string? xml, IFrameworkHandle handle, bool required)
    {
        var root = XDocument.Parse(xml ?? "<RunSettings />");
        var config = root.Root?.Element("Tern");
        if (config is null)
        {
            if (required) throw new InvalidOperationException("Tern settings are required for source-based execution.");
            AssertEqual(0, Remaining(7, 7)); // Selected-test session runs have no custom settings.
            return;
        }
        var expected = new Dictionary<string, string>
        {
            ["Harbour"] = "North Quay", ["Pattern"] = @"^Tern\.Bay/West=13$", ["Literal"] = "--listTests=not a switch"
        };
        foreach (var pair in expected)
        {
            var actual = (string?)config.Element(pair.Key);
            if (actual != pair.Value) throw new InvalidOperationException($"{pair.Key}: expected '{pair.Value}', got '{actual}'");
            handle.SendMessage(TestMessageLevel.Informational, $"TERN_EFFECTIVE {pair.Key}={actual}");
        }
        if (config.Element("RejectSemicolon") is { } boundary)
        {
            if (boundary.Value != "east;west") throw new InvalidOperationException($"semicolon value changed: {boundary.Value}");
            handle.SendMessage(TestMessageLevel.Informational, $"TERN_EFFECTIVE RejectSemicolon={boundary.Value}");
        }
    }
}
// endsnippet:adapter
