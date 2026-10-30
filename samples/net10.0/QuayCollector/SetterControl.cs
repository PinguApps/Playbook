// Compile-only API direction control. Never called on production events.
using Microsoft.VisualStudio.TestPlatform.ObjectModel.DataCollection;
internal static class SetterControl
{
    public static void Assign(TestCaseEndEventArgs input)
    {
        input.TestCaseId = Guid.Empty;
        input.TestCaseName = "compile-only fixture";
        input.IsChildTestCase = true;
        input.TestElement = null;
        input.TestOutcome = Microsoft.VisualStudio.TestPlatform.ObjectModel.TestOutcome.None;
    }
}
