using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace Playbook.Samples;

internal static class CallChecks
{
    private static int AdjustLoads(int loads, int adjustment) => loads * 7 - adjustment;

    internal static unsafe void Run()
    {
        // This recipe deliberately demonstrates the Windows MulDiv ABI.
        if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException("Windows MulDiv is required for this consumer.");
        CallEmitter.RequireDynamicCode(RuntimeFeature.IsDynamicCodeSupported);
        Check(QuarryAdapter.Evaluate(6, 5, 7, 2) == (37, 21), "canonical consumer");
        delegate* managed<int, int, int> managed = &AdjustLoads;
        var adjusted = CallEmitter.CreateManaged();
        Check(adjusted(6, 5, (nint)managed) == 37, "managed arguments/result");
        Check(adjusted(-4, 9, (nint)managed) == -37, "managed signed arguments");

        string libraryPath = Path.Combine(Environment.SystemDirectory, "kernel32.dll");
        nint library = NativeLibrary.Load(libraryPath);
        try
        {
            nint address = NativeLibrary.GetExport(library, "MulDiv");
            delegate* unmanaged[Stdcall]<int, int, int, int> native = (delegate* unmanaged[Stdcall]<int, int, int, int>)address;
            var scaled = CallEmitter.CreateNative();
            foreach (var input in new[] { (6, 7, 2, 21), (7, 5, 2, 18), (-7, 5, 2, -18), (3, 4, 0, -1), (int.MaxValue, 2, 1, -1) })
            {
                int emitted = scaled(input.Item1, input.Item2, input.Item3, address);
                int direct = native(input.Item1, input.Item2, input.Item3);
                Check(emitted == input.Item4 && emitted == direct, "native arguments/result/rounding/failure");
            }
            // Windows x64 has a unified native ABI: this does not distinguish x86 conventions.
            Console.WriteLine("CHECK native: real Windows MulDiv export; Stdcall signature; positive/negative rounding, zero-divisor and overflow sentinel; direct typed pointer agrees");
            foreach (ProcessModule module in Process.GetCurrentProcess().Modules)
            {
                if (module.ModuleName.Equals("kernel32.dll", StringComparison.OrdinalIgnoreCase)
                    || module.ModuleName.Equals("kernelbase.dll", StringComparison.OrdinalIgnoreCase))
                    Console.WriteLine($"NATIVE {module.FileName} version={module.FileVersionInfo.ProductVersion} sha256={Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(module.FileName)))}");
            }
        }
        finally { NativeLibrary.Free(library); }
    }

    internal static void Check(bool condition, string scope)
    {
        if (!condition) throw new InvalidOperationException(scope);
    }
}
