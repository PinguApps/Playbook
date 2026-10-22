// snippet:crash-new
using System.Diagnostics;

namespace Samples;

public static class DispatchCrash
{
    public static void Configure(ProcessStartInfo start, string directory)
    {
        // Configure the child before CoreCLR starts; do not change machine settings.
        start.Environment["DOTNET_DbgEnableMiniDump"] = "0";
        start.Environment["DOTNET_EnableCrashReportOnly"] = "1";
        start.Environment["DOTNET_CrashReportRootPath"] = directory;
    }
}
// endsnippet:crash-new
