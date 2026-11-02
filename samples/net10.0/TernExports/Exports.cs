using System.Runtime.InteropServices;

internal sealed class TernSurvey
{
    static TernSurvey() { }
}

internal static class Exports
{
    [UnmanagedCallersOnly(EntryPoint = "tern_root")]
    public static int Root()
    {
#if KEEP
        GC.KeepAlive(new TernSurvey());
#endif
        return 1;
    }

    // snippet:export
    // Build-time pairing is explicit on .NET 10; EntryPoint alone roots an export.
#if KEEP || LEGACY_ALWAYS
    [UnmanagedCallersOnly(EntryPoint = "tern_depth")]
    public static int Depth(int sounding) => sounding + 7;
#endif
    // endsnippet:export

    [UnmanagedCallersOnly]
    public static int Unnamed() => 99;
}
