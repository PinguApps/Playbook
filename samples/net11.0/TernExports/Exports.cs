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
    [UnmanagedCallersOnly(EntryPoint = "tern_depth", AssociatedSourceType = typeof(TernSurvey))]
    public static int Depth(int sounding) => sounding + 7;
    // endsnippet:export

    // Without EntryPoint this is callable by pointer, but has no named export.
    [UnmanagedCallersOnly]
    public static int Unnamed() => 99;
}
