// snippet:adapter
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

namespace Quay.Tests;

[FileExtension(".dll")]
[DefaultExecutorUri(BerthExecutor.Address)]
public sealed class BerthDiscoverer : ITestDiscoverer
{
    internal static IEnumerable<TestCase> Cases(string source)
    {
        yield return new TestCase("Quay.Berths.AvailableCapacity", new Uri(BerthExecutor.Address), source);
        yield return new TestCase("Quay.Berths.DeliberateFailure", new Uri(BerthExecutor.Address), source);
    }

    public void DiscoverTests(IEnumerable<string> sources, IDiscoveryContext discoveryContext,
        IMessageLogger logger, ITestCaseDiscoverySink discoverySink)
    {
        foreach (var source in sources.Where(path => Path.GetFileName(path) == "Quay.TestAdapter.dll"))
            foreach (var test in Cases(source)) discoverySink.SendTestCase(test);
    }
}

[ExtensionUri(Address)]
public sealed class BerthExecutor : ITestExecutor
{
    public const string Address = "executor://quay/berths/v1";
    public void Cancel() { }
    public void RunTests(IEnumerable<string>? sources, IRunContext? runContext, IFrameworkHandle? frameworkHandle)
        => RunTests(sources?.SelectMany(BerthDiscoverer.Cases), runContext, frameworkHandle);

    public void RunTests(IEnumerable<TestCase>? tests, IRunContext? runContext, IFrameworkHandle? frameworkHandle)
    {
        ArgumentNullException.ThrowIfNull(tests);
        ArgumentNullException.ThrowIfNull(frameworkHandle);
        foreach (var test in tests)
        {
            frameworkHandle.RecordStart(test);
            var result = new TestResult(test);
            try
            {
                var remaining = checked(20 - 7);
                var expected = test.FullyQualifiedName.EndsWith("DeliberateFailure", StringComparison.Ordinal) ? 12 : 13;
                if (remaining != expected) throw new InvalidOperationException($"expected={expected};actual={remaining}");
                result.Outcome = TestOutcome.Passed;
            }
            catch (InvalidOperationException failure)
            {
                result.Outcome = TestOutcome.Failed;
                result.ErrorMessage = failure.Message;
            }
            frameworkHandle.RecordResult(result);
            frameworkHandle.RecordEnd(test, result.Outcome);
        }
    }
}
// endsnippet:adapter
