// snippet:frames
namespace Playbook.Samples;

public static class BeaconFrame
{
    public static ushort Decode(ReadOnlySpan<byte> frame)
    {
        if (frame.Length < 2) { throw new ArgumentException("A beacon reading needs two bytes.", nameof(frame)); }
        fixed (byte* address = frame)
        {
            // SAFETY: the length check guarantees two readable bytes in the supplied valid span.
            // The caller retains ownership; fixed pins managed storage until this synchronous read
            // returns. ReadRaw neither retains the pointer nor frees or writes the storage.
            return unsafe(ReadRaw(address));
        }
    }

    /// <summary>Reads a two-byte little-endian beacon reading without validation.</summary>
    /// <param name="address">The start of at least two readable bytes.</param>
    /// <returns>The decoded reading.</returns>
    /// <remarks>
    /// Safety: the caller must keep both bytes allocated and readable for the entire call,
    /// retain ownership (this method never frees, writes or retains the pointer), and prevent
    /// concurrent mutation or release. Managed storage must remain pinned throughout the call;
    /// unmanaged storage must remain alive and stationary. Prefer Decode for a checked span API.
    /// </remarks>
    public static unsafe ushort ReadRaw(byte* address)
    {
        unsafe
        {
            // SAFETY: the caller contract guarantees readable offsets 0 and 1 for this call.
            // Only these bytes are read synchronously; no pointer escapes and ownership stays
            // with the caller, whose pin/lifetime must cover both accesses.
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
    // SAFETY: this standalone ushort overlaps no other field and has no pointer/lifetime invariant.
    public safe ushort Reading;
}
// endsnippet:layout
