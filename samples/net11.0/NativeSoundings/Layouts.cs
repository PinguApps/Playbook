// Aggregate fields are the C ABI contract; this analyzer does not recognize ExtendedLayout.
#pragma warning disable CA1051
using System.Runtime.InteropServices;

namespace Playbook.Samples;

// snippet:native-layout
[ExtendedLayout(ExtendedLayoutKind.CStruct)]
internal struct Packet
{
    internal byte Kind;
    internal double Depth;
    internal int Count;
}
[ExtendedLayout(ExtendedLayoutKind.CUnion)]
internal struct Reading
{
    internal double Depth;
    internal long Code;
}
// endsnippet:native-layout
[ExtendedLayout(ExtendedLayoutKind.CStruct)]
internal struct PacketAlignment { internal byte Prefix; internal Packet Value; }
[ExtendedLayout(ExtendedLayoutKind.CStruct)]
internal struct ReadingAlignment { internal byte Prefix; internal Reading Value; }
