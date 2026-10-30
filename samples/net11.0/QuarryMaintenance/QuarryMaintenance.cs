using System.Reflection;

namespace Playbook.Samples;

public static class QuarryMaintenance
{
    // snippet:locations
    public static (string Invocation, string? Executable, string? ManagedFile) Inspect()
    {
        string invocation = Environment.GetCommandLineArgs()[0];
        string? executable = Environment.ProcessPath;
#pragma warning disable IL3000 // Empty bundled Location is handled deliberately.
        string? managed = Assembly.GetEntryAssembly()?.Location;
#pragma warning restore IL3000
        return (invocation, executable, string.IsNullOrEmpty(managed) ? null : managed);
    }
    // endsnippet:locations

    // snippet:alias
    public static string SelectHelpName(string invocation)
    {
        // Display/dispatch only. Never execute a path reconstructed from argv[0].
        string name = Path.GetFileNameWithoutExtension(invocation);
        return name == "quarry-audit" ? "quarry-audit" : "quarry-maintenance";
    }
    // endsnippet:alias
}
