// snippet:complete
using System.Buffers;
using System.Security.Cryptography;

namespace HarbourMuseum;

// Exclusive access to an existing array for one synchronous operation at a time.
// The caller must not use aliases until Dispose has cleared the entire array.
public sealed class OwnedBuffer(byte[] storage) : IBufferWriter<byte>, IDisposable
{
    private int written;
    private bool disposed;
    public void ThrowIfOverlaps(ReadOnlySpan<byte> input)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        if (input.Overlaps(storage))
            throw new ArgumentException("Input must not overlap the museum token destination.", nameof(input));
    }
    public ReadOnlySpan<byte> WrittenSpan
    {
        get { ObjectDisposedException.ThrowIf(disposed, this); return storage.AsSpan(0, written); }
    }
    public void Clear()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        CryptographicOperations.ZeroMemory(storage);
        written = 0;
    }
    public void Advance(int count)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(count, storage.Length - written);
        written += count;
    }
    public Memory<byte> GetMemory(int sizeHint = 0)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        ArgumentOutOfRangeException.ThrowIfNegative(sizeHint);
        if (Math.Max(1, sizeHint) > storage.Length - written)
            throw new InvalidOperationException("Museum token buffer is too small.");
        return storage.AsMemory(written);
    }
    public Span<byte> GetSpan(int sizeHint = 0) => GetMemory(sizeHint).Span;
    public void Dispose()
    {
        if (disposed) return;
        Clear(); // Includes bytes written through GetSpan but never advanced.
        disposed = true;
    }
}
// endsnippet:complete
