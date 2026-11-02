using System.Runtime.InteropServices;

namespace Playbook.Samples;

internal static class Checks
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "CA1508", Justification = "Native function pointers mutate the addressed fields; the analyzer does not model those writes.")]
    internal static unsafe void Run()
    {
        RuntimeIdentity.Assert();
        if (!OperatingSystem.IsWindows() || RuntimeInformation.ProcessArchitecture != Architecture.X64)
            throw new PlatformNotSupportedException("This measured helper is Windows x64 MSVC only.");
        nint library = NativeLibrary.Load(Path.Combine(AppContext.BaseDirectory, "soundings.dll"));
        try
        {
            var metric = (delegate* unmanaged[Cdecl]<int, nuint>)NativeLibrary.GetExport(library, "metric");
            Packet packet = default;
            Reading reading = default;
            PacketAlignment packetAlignment = default;
            ReadingAlignment readingAlignment = default;
            nuint[] managed = [(nuint)sizeof(Packet), (nuint)((byte*)&packetAlignment.Value - (byte*)&packetAlignment),
                (nuint)((byte*)&packet.Kind - (byte*)&packet), (nuint)((byte*)&packet.Depth - (byte*)&packet),
                (nuint)((byte*)&packet.Count - (byte*)&packet), (nuint)sizeof(Reading),
                (nuint)((byte*)&readingAlignment.Value - (byte*)&readingAlignment),
                (nuint)((byte*)&reading.Depth - (byte*)&reading), (nuint)((byte*)&reading.Code - (byte*)&reading),
                (nuint)((byte*)&packetAlignment.Value - (byte*)&packetAlignment),
                (nuint)((byte*)&readingAlignment.Value - (byte*)&readingAlignment)];
            for (int i = 0; i < managed.Length; i++)
                Require(managed[i] == metric(i), $"native metric {i}: managed={managed[i]} native={metric(i)}");
            Require((nuint)sizeof(WrongPacket) != metric(0), "packed negative control must be rejected before native invocation");
            var revise = (delegate* unmanaged[Cdecl]<Packet*, void>)NativeLibrary.GetExport(library, "revise");
            packet = new Packet { Kind = 3, Depth = -12.25, Count = 9 };
            revise(&packet);
            Require(packet.Kind == 4 && packet.Depth == -11.75 && packet.Count == 11, "struct round trip");
            Packet canonical = SoundingExchange.Revise(packet, Path.Combine(AppContext.BaseDirectory, "soundings.dll"));
            Require(canonical.Kind == 5 && canonical.Depth == -11.25 && canonical.Count == 13, "canonical consumer");
            var reviseDepth = (delegate* unmanaged[Cdecl]<Reading*, void>)NativeLibrary.GetExport(library, "revise_depth");
            reading.Depth = 4.5;
            reviseDepth(&reading);
            Require(reading.Depth == 5.75, "union double round trip");
            var reviseCode = (delegate* unmanaged[Cdecl]<Reading*, void>)NativeLibrary.GetExport(library, "revise_code");
            reading.Code = -100;
            reviseCode(&reading);
            Require(reading.Code == -93, "union integer round trip");
            Console.WriteLine($"PASS NativeSoundings: Windows x64 sizeof/alignment/offsets={string.Join(',', managed)}; struct/union round trips; rejected packed layout");
        }
        finally { NativeLibrary.Free(library); }
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct WrongPacket { public byte Kind; public double Depth; public int Count; }
}
