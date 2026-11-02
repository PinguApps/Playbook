using System.Runtime.InteropServices;

namespace Playbook.Samples;

// snippet:native-layout
// MSVC Windows x64 contract only: preserve these explicit sizes and offsets.
[StructLayout(LayoutKind.Explicit, Size = 24)]
internal struct Packet
{
    [FieldOffset(0)] internal byte Kind;
    [FieldOffset(8)] internal double Depth;
    [FieldOffset(16)] internal int Count;
}
[StructLayout(LayoutKind.Explicit, Size = 8)]
internal struct Reading
{
    [FieldOffset(0)] internal double Depth;
    [FieldOffset(0)] internal long Code;
}
// endsnippet:native-layout
[StructLayout(LayoutKind.Sequential)]
internal struct PacketAlignment { internal byte Prefix; internal Packet Value; }
[StructLayout(LayoutKind.Sequential)]
internal struct ReadingAlignment { internal byte Prefix; internal Reading Value; }
