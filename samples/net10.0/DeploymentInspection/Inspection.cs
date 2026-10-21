using System.Reflection;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Playbook.Samples;

// snippet:caller
public readonly record struct CallerReport(string? AssemblyName, string? UnavailableReason);

public static class DeploymentReport
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static CallerReport Caller()
    {
        try
        {
            return new CallerReport(Assembly.GetCallingAssembly().GetName().Name, null);
        }
        catch (NotSupportedException exception)
        {
            // Also handles PlatformNotSupportedException on .NET 10 Native AOT.
            return new CallerReport(null, exception.GetType().Name);
        }
    }

    // IL3000 is intentional: this diagnostic explicitly handles the missing location.
#pragma warning disable IL3000
    [UnconditionalSuppressMessage("SingleFile", "IL3000", Justification = "Reports unavailable location explicitly; never opens the path.")]
    public static string? Location()
    {
        string location = typeof(DeploymentReport).Assembly.Location;
        return string.IsNullOrEmpty(location) ? null : location;
    }
#pragma warning restore IL3000
}
// endsnippet:caller
