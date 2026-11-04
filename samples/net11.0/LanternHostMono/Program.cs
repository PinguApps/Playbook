// snippet:managed-host
using System.Runtime.InteropServices.JavaScript;
using System.Diagnostics.Tracing;

namespace Playbook.Samples;

internal static partial class Lantern
{
    private static string report = "not-started";
    private static int Main(string[] args)
    {
        report = $"{Environment.GetEnvironmentVariable("LANTERN_ROUTE")}:{string.Join('|', args)}";
        Console.WriteLine($"PASS Lantern {report} {System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}");
        return 17;
    }

    [JSExport]
    internal static bool Emit()
    {
        LanternEvents.Log.Reading(29);
        return LanternEvents.Log.IsEnabled();
    }

    [JSExport]
    internal static string Read() => report;
}

[EventSource(Name = "Lantern-Station")]
internal sealed class LanternEvents : EventSource
{
    internal static readonly LanternEvents Log = new();
    [Event(1, Level = EventLevel.Informational)]
    public void Reading(int value) => WriteEvent(1, value);
}
// endsnippet:managed-host
