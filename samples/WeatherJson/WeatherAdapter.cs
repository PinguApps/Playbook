using System.Reflection;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using Newtonsoft.Json.Linq;

namespace WeatherStation;

// snippet:adapter
[FileExtension(".dll")]
[DefaultExecutorUri(WeatherExecutor.ExecutorUri)]
public sealed class WeatherDiscoverer : ITestDiscoverer
{
    public void DiscoverTests(IEnumerable<string> sources, IDiscoveryContext discoveryContext,
        IMessageLogger logger, ITestCaseDiscoverySink discoverySink)
    {
        // Independent extension configuration, unrelated to the test's rainfall input.
        JObject settings = JObject.Parse("""{"suite":"weather","schema":2}""");
        if ((int?)settings["schema"] != 2) { throw new InvalidOperationException("Wrong schema."); }
        logger.SendMessage(TestMessageLevel.Informational, "weather-adapter=loaded;schema=2");
        foreach (string source in sources.Where(path => Path.GetFileName(path) == "WeatherTests.dll"))
        {
            discoverySink.SendTestCase(new TestCase("WeatherStation.WeatherChecks.RainfallTotal",
                new Uri(WeatherExecutor.ExecutorUri), source));
        }
    }
}

[ExtensionUri(ExecutorUri)]
public sealed class WeatherExecutor : ITestExecutor
{
    public const string ExecutorUri = "executor://weatherstation/json/v1";
    private volatile bool cancelled;

    public void Cancel() => cancelled = true;

    public void RunTests(IEnumerable<string>? sources, IRunContext? runContext, IFrameworkHandle? frameworkHandle)
    {
        ArgumentNullException.ThrowIfNull(sources);
        ArgumentNullException.ThrowIfNull(frameworkHandle);
        RunTests(sources.Where(path => Path.GetFileName(path) == "WeatherTests.dll")
            .Select(path => new TestCase("WeatherStation.WeatherChecks.RainfallTotal",
                new Uri(ExecutorUri), path)), runContext, frameworkHandle);
    }

    public void RunTests(IEnumerable<TestCase>? tests, IRunContext? runContext, IFrameworkHandle? frameworkHandle)
    {
        ArgumentNullException.ThrowIfNull(tests);
        ArgumentNullException.ThrowIfNull(frameworkHandle);
        foreach (TestCase test in tests)
        {
            if (cancelled) { break; }
            frameworkHandle.RecordStart(test);
            TestResult result = new(test);
            try
            {
                Assembly.LoadFrom(test.Source).GetType("WeatherStation.WeatherChecks", true)!
                    .GetMethod("RainfallTotal")!.Invoke(null, null);
                result.Outcome = TestOutcome.Passed;
            }
            catch (Exception error)
            {
                result.Outcome = TestOutcome.Failed;
                result.ErrorMessage = error.ToString();
            }
            frameworkHandle.RecordResult(result);
            frameworkHandle.RecordEnd(test, result.Outcome);
        }
    }
}
// endsnippet:adapter
