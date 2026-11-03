using System.Runtime.InteropServices;

public static class TideWindow
{
    // snippet:export
    [UnmanagedCallersOnly(EntryPoint = "tide_window_minutes")]
    public static int Minutes(int heightCentimetres, int clearanceCentimetres)
        => heightCentimetres >= clearanceCentimetres ? (heightCentimetres - clearanceCentimetres) * 2 : 0;
    // endsnippet:export
}
