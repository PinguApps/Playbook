using System.Diagnostics;
using System.Numerics.Tensors;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Playbook.Samples;

// Opt-in authoring proof, never part of the ordinary sample/CI run.
internal static partial class NativeChecks
{
    public static unsafe void Run()
    {
        if (!OperatingSystem.IsWindows() || RuntimeInformation.ProcessArchitecture != Architecture.X64)
            throw new PlatformNotSupportedException("The measured-memory native proof requires Windows x64.");
        long logicalCount = (long)int.MaxValue + 9;
        nint count = checked((nint)logicalCount);
        nint late = count - 1;
        ulong bytes = checked((ulong)count * 3);
        const ulong headroom = 2UL * 1024 * 1024 * 1024;
        RequireMemory(bytes, headroom);
        using var process = Process.GetCurrentProcess();
        var timer = Stopwatch.StartNew();
        byte* a = null;
        byte* b = null;
        byte* output = null;
        try
        {
            a = (byte*)NativeMemory.Alloc((nuint)count);
            b = (byte*)NativeMemory.Alloc((nuint)count);
            output = (byte*)NativeMemory.Alloc((nuint)count);
            if (a == null || b == null || output == null) throw new OutOfMemoryException();
            NativeMemory.Fill(a, (nuint)count, 1);
            NativeMemory.Fill(b, (nuint)count, 2);
            ReadOnlyTensorSpan<byte> source = new(a, count), peers = new(b, count);
            var destination = new TensorSpan<byte>(output, count);
            a[late] = 3;
            b[late] = 3;
            Require(Tensor.EqualsAny(source, (byte)3) && Tensor.EqualsAny(source, peers), "EqualsAny scalar/pairwise beyond int");
            Require(Tensor.GreaterThanOrEqualAny(source, (byte)3) && Tensor.GreaterThanOrEqualAny(source, peers), "GreaterThanOrEqualAny scalar/pairwise beyond int");
            b[late] = 2;
            Require(Tensor.GreaterThanAny(source, (byte)2) && Tensor.GreaterThanAny(source, peers), "GreaterThanAny scalar/pairwise beyond int");
            NativeMemory.Fill(a, (nuint)count, 3);
            a[late] = 1;
            Require(Tensor.LessThanAny(source, (byte)2) && Tensor.LessThanAny(source, peers), "LessThanAny scalar/pairwise beyond int");
            b[late] = 1;
            Require(Tensor.LessThanOrEqualAny(source, (byte)1) && Tensor.LessThanOrEqualAny(source, peers), "LessThanOrEqualAny scalar/pairwise beyond int");
            source.CopyTo(destination);
            Require(output[0] == 3 && output[late] == 1 && Tensor.SequenceEqual(source, (ReadOnlyTensorSpan<byte>)destination), "native-width copy and equality");
            output[late] = 0;
            Require(!Tensor.SequenceEqual(source, (ReadOnlyTensorSpan<byte>)destination), "native-width equality late mismatch");
            Tensor.Reverse(source, destination);
            Require(output[0] == 1 && output[late] == 3, "native-width reversal");
            var last = source.Slice((ReadOnlySpan<nint>)[late]);
            Require(last.FlattenedLength == 1 && last[0] == 1, "native-width view offset");
            var reshaped = Tensor.Reshape(source, [2, count / 2]);
            Require(reshaped[1, count / 2 - 1] == 1, "native-width multidimensional indexing");
            a[late] = 9;
            Require(Tensor.IndexOfMax(source) == late && Tensor.IndexOfMaxMagnitude(source) == late, "native-width maximum indices");
            a[late] = 0;
            Require(Tensor.IndexOfMin(source) == late && Tensor.IndexOfMinMagnitude(source) == late, "native-width minimum indices");
            Console.WriteLine($"PASS native proof: count={count}; allocatedBytes={bytes}; elapsed={timer.Elapsed}; peakWorkingSetBytes={process.PeakWorkingSet64}");
        }
        finally
        {
            NativeMemory.Free(output);
            NativeMemory.Free(b);
            NativeMemory.Free(a);
        }
    }

    private static void Require(bool condition, string name)
    {
        if (!condition) throw new InvalidOperationException(name);
        Console.WriteLine($"PASS {name}");
    }

    [SupportedOSPlatform("windows")]
    private static void RequireMemory(ulong bytes, ulong headroom)
    {
        var memory = new MemoryStatus { Length = (uint)Marshal.SizeOf<MemoryStatus>() };
        if (!GlobalMemoryStatusEx(ref memory)) throw new InvalidOperationException("Cannot measure available memory.");
        Console.WriteLine($"NATIVE PREFLIGHT allocatedBytes={bytes}; headroomBytes={headroom}; availablePhysicalBytes={memory.AvailablePhysical}; availableCommitBytes={memory.AvailablePageFile}");
        if (memory.AvailablePhysical < bytes + headroom || memory.AvailablePageFile < bytes + headroom)
            throw new InvalidOperationException("Insufficient free physical/commit memory; native proof remains incomplete.");
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MemoryStatus
    {
        public uint Length;
        public uint MemoryLoad;
        public ulong TotalPhysical;
        public ulong AvailablePhysical;
        public ulong TotalPageFile;
        public ulong AvailablePageFile;
        public ulong TotalVirtual;
        public ulong AvailableVirtual;
        public ulong AvailableExtendedVirtual;
    }

    [LibraryImport("kernel32.dll")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GlobalMemoryStatusEx(ref MemoryStatus buffer);
}
