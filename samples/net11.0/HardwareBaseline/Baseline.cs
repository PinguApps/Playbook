// snippet:hardware-new
using System.Runtime.Intrinsics.X86;

namespace Samples.Current;

public static class DispatchHardware
{
    public static bool MeetsBaseline()
    {
        if (!X86Base.IsSupported) { throw new PlatformNotSupportedException("This probe is for x86/x64 only."); }
        var (_, _, ecx, edx) = X86Base.CpuId(1, 0);
        return MeetsBaseline(ecx, edx);
    }

    public static bool MeetsBaseline(int ecx, int edx)
    {
        int requiredEdx = (1 << 8) | (1 << 15) | (1 << 25) | (1 << 26);
        int requiredEcx = (1 << 0) | (1 << 9) | (1 << 13) | (1 << 19) | (1 << 20) | (1 << 23);
        // SSE3, SSSE3, CX16, SSE4.1, SSE4.2, POPCNT, in addition to the previous baseline.
        return (edx & requiredEdx) == requiredEdx && (ecx & requiredEcx) == requiredEcx;
    }
}
// endsnippet:hardware-new
