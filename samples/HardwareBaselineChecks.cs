using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
#if NET11_0_OR_GREATER
using DispatchHardware = Samples.Current.DispatchHardware;
#else
using Samples;
#endif

if (!DispatchHardware.MeetsBaseline()) { throw new PlatformNotSupportedException("CPU does not meet the declared x86/x64 runtime baseline."); }
var (_, _, ecx, edx) = X86Base.CpuId(1, 0);
if (!Samples.Current.DispatchHardware.MeetsBaseline()) { throw new InvalidOperationException("Pre-upgrade x86-64-v2 check failed."); }
for (int bit = 0; bit < 32; bit++)
{
    if ((new[] { 0, 9, 13, 19, 20, 23 }).Contains(bit) && Samples.Current.DispatchHardware.MeetsBaseline(ecx & ~(1 << bit), edx))
    {
        throw new InvalidOperationException($"Missing required CPUID bit {bit} was accepted by the preflight policy.");
    }
}
Console.WriteLine($"PASS: x86/x64 baseline CPUID ECX={ecx:X8}, EDX={edx:X8}; {RuntimeInformation.ProcessArchitecture}; {RuntimeInformation.FrameworkDescription}; masked-bit cases are policy checks, not execution on unsupported CPUs.");
