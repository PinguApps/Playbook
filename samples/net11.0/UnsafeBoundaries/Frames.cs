// snippet:frames
namespace Playbook.Samples;

public static class BeaconFrame
{
    public static ushort Decode(ReadOnlySpan<byte> frame)
    {
        if (frame.Length < 2) { throw new ArgumentException("A beacon reading needs two bytes.", nameof(frame)); }
        fixed (byte* address = frame)
        {
            return unsafe(ReadRaw(address));
        }
    }

    // Caller contract: address points to at least two readable bytes for this call.
    public static unsafe ushort ReadRaw(byte* address)
    {
        unsafe
        {
            return (ushort)(address[0] | (address[1] << 8));
        }
    }
}
// endsnippet:frames

// snippet:layout
[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit)]
public struct BeaconStatus
{
    [System.Runtime.InteropServices.FieldOffset(0)]
    public safe ushort Reading;
}
// endsnippet:layout
