using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using Playbook.Samples;

var report = QuarryMaintenance.Inspect();
if (QuarryMaintenance.SelectHelpName("quarry-audit") != "quarry-audit" ||
    QuarryMaintenance.SelectHelpName("../../untrusted.exe") != "quarry-maintenance")
    throw new InvalidOperationException("Bounded help aliases failed.");
if (string.IsNullOrEmpty(report.Invocation)) throw new InvalidOperationException("Missing invocation.");
Console.WriteLine(JsonSerializer.Serialize(new
{
    report.Invocation, report.Executable, report.ManagedFile,
#pragma warning disable IL3000 // Raw empty value retained for deployment evidence.
    RawLocation = Assembly.GetEntryAssembly()?.Location,
#pragma warning restore IL3000
    HelpName = QuarryMaintenance.SelectHelpName(report.Invocation),
    Runtime = RuntimeInformation.FrameworkDescription
}));
