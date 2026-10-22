// snippet:crash-old
using System.Diagnostics;

namespace Samples;

public static class DispatchCrash
{
    public static void Configure(ProcessStartInfo start, string directory)
    {
        start.Environment["DOTNET_DbgEnableMiniDump"] = "1";
        start.Environment["DOTNET_EnableCrashReportOnly"] = "1";
        start.Environment["DOTNET_DbgMiniDumpName"] = Path.Combine(directory, "dispatch.%p");
    }
}
// endsnippet:crash-old
