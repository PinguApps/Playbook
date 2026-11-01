// snippet:call-consumer
using System.Runtime.InteropServices;

namespace Playbook.Samples;

internal static class QuarryAdapter
{
    private static int AdjustLoads(int loads, int adjustment) => loads * 7 - adjustment;

    internal static unsafe (int Adjusted, int Scaled) Evaluate(int loads, int adjustment, int multiplier, int divisor)
    {
        if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException("Requires Windows MulDiv.");
        var adjust = CallEmitter.CreateManaged();
        var scale = CallEmitter.CreateNative();
        delegate* managed<int, int, int> managed = &AdjustLoads;
        int adjusted = adjust(loads, adjustment, (nint)managed);
        nint library = NativeLibrary.Load(Path.Combine(Environment.SystemDirectory, "kernel32.dll"));
        try
        {
            nint address = NativeLibrary.GetExport(library, "MulDiv");
            return (adjusted, scale(loads, multiplier, divisor, address));
        }
        finally { NativeLibrary.Free(library); }
    }
}
// endsnippet:call-consumer
