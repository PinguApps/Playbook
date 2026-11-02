// snippet:native-consumer
using System.Runtime.InteropServices;

namespace Playbook.Samples;

internal static class SoundingExchange
{
    internal static unsafe Packet Revise(Packet packet, string libraryPath)
    {
        if (!OperatingSystem.IsWindows() || RuntimeInformation.ProcessArchitecture != Architecture.X64)
            throw new PlatformNotSupportedException("This helper is built for MSVC Windows x64.");
        nint library = NativeLibrary.Load(libraryPath);
        try
        {
            var metric = (delegate* unmanaged[Cdecl]<int, nuint>)NativeLibrary.GetExport(library, "metric");
            if (metric(0) != (nuint)sizeof(Packet) || metric(3) != (nuint)((byte*)&packet.Depth - (byte*)&packet))
                throw new InvalidOperationException("Native layout differs; do not exchange the packet.");
            var revise = (delegate* unmanaged[Cdecl]<Packet*, void>)NativeLibrary.GetExport(library, "revise");
            revise(&packet);
            return packet;
        }
        finally { NativeLibrary.Free(library); }
    }
}
// endsnippet:native-consumer
