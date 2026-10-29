// snippet:host
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.TestPlatform.VsTestConsole.TranslationLayer;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

namespace TernTesting;

internal static class Program
{
    public static void Main(string[] args)
    {
        if (args.Length != 2) throw new ArgumentException("console DLL and test DLL required");
        Console.WriteLine($"runtime={RuntimeInformation.FrameworkDescription};host={Environment.ProcessPath}");
        var wrapper = new VsTestConsoleWrapper(Path.GetFullPath(args[0]));
        try
        {
            // This is the console protocol connection, not the removed pooled test session.
            wrapper.StartSession();
            wrapper.InitializeExtensions([Path.GetFullPath(args[1])]);
            var discovery = new Discovery();
            wrapper.DiscoverTests([Path.GetFullPath(args[1])], "<RunSettings />", discovery);
            discovery.Check();
            var ordinary = discovery.Tests.Where(t => !t.FullyQualifiedName.EndsWith("Cancellation", StringComparison.Ordinal)).ToArray();
            for (var repeat = 0; repeat < 2; repeat++)
            {
                var run = new Run();
                wrapper.RunTests(ordinary, "<RunSettings />", run);
                run.Check(2, 1, false);
                Console.WriteLine($"PASS repeat={repeat};passed=2;failed=1;total=3");
            }
            var cancellation = new Run();
            // Wait for the real adapter's readiness message, not a timing guess.
            var pending = Task.Run(() => wrapper.RunTests(discovery.Tests.Where(t => t.FullyQualifiedName.EndsWith("Cancellation", StringComparison.Ordinal)), "<RunSettings />", cancellation));
            if (!cancellation.Ready.Wait(TimeSpan.FromSeconds(30))) throw new TimeoutException("adapter not ready");
            wrapper.CancelTestRun();
            if (!pending.Wait(TimeSpan.FromSeconds(30))) throw new TimeoutException("cancellation did not complete");
            cancellation.Check(0, 0, true);
            Console.WriteLine("PASS cancellation;total=0");
            var afterCancel = new Run();
            wrapper.RunTests(ordinary, "<RunSettings />", afterCancel);
            afterCancel.Check(2, 1, false);
            Console.WriteLine("PASS after-cancel;passed=2;failed=1;total=3");
        }
        finally
        {
            wrapper.EndSession();
            foreach (var id in Messages.OwnedHosts.Keys)
            {
                try
                {
                    using var child = Process.GetProcessById(id);
                    if (!child.WaitForExit(10000)) throw new InvalidOperationException($"owned testhost {id} survived shutdown");
                }
                catch (ArgumentException) { /* The observed testhost has already exited. */ }
            }
        }
        Console.WriteLine("PASS tern-host;connection-closed");
    }
}

internal abstract class Messages : ITestMessageEventHandler
{
    internal static System.Collections.Concurrent.ConcurrentDictionary<int, byte> OwnedHosts { get; } = new();
    public List<string> Errors { get; } = [];
    public ManualResetEventSlim Ready { get; } = new();
    public void HandleRawMessage(string rawMessage) { }
    public void HandleLogMessage(TestMessageLevel level, string? message)
    {
        if (level == TestMessageLevel.Error) Errors.Add(message ?? "unknown platform error");
        if (message == "TERN_READY") Ready.Set();
        if (message?.StartsWith("TERN_PID:", StringComparison.Ordinal) == true)
            OwnedHosts.TryAdd(int.Parse(message.AsSpan(9), System.Globalization.CultureInfo.InvariantCulture), 0);
        Console.WriteLine($"platform:{level}:{message}");
    }
}

internal sealed class Discovery : Messages, ITestDiscoveryEventsHandler
{
    public List<TestCase> Tests { get; } = [];
    private long total = -1;
    private bool aborted;
    public void HandleDiscoveredTests(IEnumerable<TestCase>? discoveredTestCases) { if (discoveredTestCases is not null) Tests.AddRange(discoveredTestCases); }
    public void HandleDiscoveryComplete(long totalTests, IEnumerable<TestCase>? lastChunk, bool isAborted)
    {
        HandleDiscoveredTests(lastChunk);
        total = totalTests;
        aborted = isAborted;
    }
    public void Check()
    {
        if (aborted || total != 4 || Tests.Count != 4 || Tests.Select(t => t.Id).Distinct().Count() != 4 || Errors.Count != 0)
            throw new InvalidOperationException($"Discovery: total={total};received={Tests.Count};errors={string.Join('|', Errors)}");
        Console.WriteLine("PASS discovery;total=4;distinct=4");
    }
}

internal sealed class Run : Messages, ITestRunEventsHandler
{
    private readonly List<TestResult> results = [];
    private TestRunCompleteEventArgs? complete;
    public void HandleTestRunStatsChange(TestRunChangedEventArgs? testRunChangedArgs)
    {
        if (testRunChangedArgs?.NewTestResults is not null) results.AddRange(testRunChangedArgs.NewTestResults);
    }
    public void HandleTestRunComplete(TestRunCompleteEventArgs testRunCompleteArgs, TestRunChangedEventArgs? lastChunkArgs, ICollection<AttachmentSet>? runContextAttachments, ICollection<string>? executorUris)
    {
        HandleTestRunStatsChange(lastChunkArgs);
        complete = testRunCompleteArgs;
    }
    public int LaunchProcessWithDebuggerAttached(TestProcessStartInfo testProcessStartInfo) => throw new NotSupportedException();
    public void Check(int passed, int failed, bool canceled)
    {
        if (complete is null || complete.IsAborted || complete.Error is not null || complete.IsCanceled != canceled || Errors.Count != 0
            || results.Count != passed + failed || results.Select(r => r.TestCase.Id).Distinct().Count() != results.Count
            || results.Count(r => r.Outcome == TestOutcome.Passed) != passed || results.Count(r => r.Outcome == TestOutcome.Failed) != failed
            || complete.TestRunStatistics?.ExecutedTests != passed + failed
            || (complete.TestRunStatistics?[TestOutcome.Passed] ?? 0) != passed || (complete.TestRunStatistics?[TestOutcome.Failed] ?? 0) != failed)
            throw new InvalidOperationException($"Run totals/status differ;received={results.Count};stats={complete?.TestRunStatistics?.ExecutedTests};cancel={complete?.IsCanceled};errors={string.Join('|', Errors)}");
    }
}
// endsnippet:host
