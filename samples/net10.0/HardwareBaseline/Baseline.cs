// snippet:hardware-old
using System.Runtime.Intrinsics.X86;

namespace Samples;

public static class DispatchHardware
{
    public static bool MeetsBaseline()
    {
        if (!X86Base.IsSupported) { throw new PlatformNotSupportedException("This probe is for x86/x64 only."); }
        var (_, _, _, edx) = X86Base.CpuId(1, 0);
        int required = (1 << 8) | (1 << 15) | (1 << 25) | (1 << 26); // CX8, CMOV, SSE, SSE2
        return (edx & required) == required;
    }
}
// endsnippet:hardware-old
