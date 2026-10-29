// snippet:payload
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;

namespace CwmLab;

public sealed class LabRun
{
    public string Batch { get; set; } = "";
    public TestCase? Selected { get; set; }
    public TestResult? Result { get; set; }
    public ITestRunStatistics? Statistics { get; set; }

    public static LabRun Completed()
    {
        var test = new TestCase("CwmLab.Spectrometer.Calibration", new Uri("executor://cwmlab"), "CwmLab.Tests.dll")
        {
            DisplayName = "Reference vial stays within tolerance"
        };
        test.Traits.Add(new Trait("Bench", "Swansea-4"));
        var result = new TestResult(test) { Outcome = TestOutcome.Passed, Duration = TimeSpan.FromMilliseconds(42) };
        return new LabRun
        {
            Batch = "vial-204",
            Selected = test,
            Result = result,
            Statistics = new TestRunStatistics(1, new Dictionary<TestOutcome, long> { [TestOutcome.Passed] = 1 })
        };
    }

    public void Check()
    {
        if (Batch != "vial-204" || Selected?.FullyQualifiedName != "CwmLab.Spectrometer.Calibration"
            || Selected.DisplayName != "Reference vial stays within tolerance"
            || Selected.Traits.Single().Value != "Swansea-4" || Result?.Outcome != TestOutcome.Passed
            || Result.TestCase.FullyQualifiedName != Selected.FullyQualifiedName
            || Result.Duration != TimeSpan.FromMilliseconds(42)
            || Statistics?.ExecutedTests != 1 || Statistics[TestOutcome.Passed] != 1)
        {
            throw new InvalidDataException("The laboratory completion message lost required data.");
        }
    }
}
// endsnippet:payload

