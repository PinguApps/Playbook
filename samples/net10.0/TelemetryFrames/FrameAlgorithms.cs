using System.Buffers.Binary;
using System.IO.Hashing;

// snippet:algorithms
// Original, deliberately simple fallback; parameters use the normal (MSB-first) polynomial.
internal sealed class FrameCrc(int width, ulong polynomial, ulong initial, ulong finalXor, bool reflected)
    : NonCryptographicHashAlgorithm(width / 8)
{
    private readonly ulong mask = width == 64 ? ulong.MaxValue : uint.MaxValue;
    private readonly ulong seed = initial;
    private ulong register = initial;

    public override void Append(ReadOnlySpan<byte> source)
    {
        // Always shift left; reflect each input octet and the final register when requested.
        foreach (byte octet in source)
        {
            ulong input = reflected ? Reverse(octet, 8) : octet;
            register ^= input << (width - 8);
            for (int bit = 0; bit < 8; bit++)
            {
                bool top = (register & (1UL << (width - 1))) != 0;
                register = ((register << 1) ^ (top ? polynomial : 0)) & mask;
            }
        }
    }

    public override void Reset() => register = seed;

    protected override void GetCurrentHashCore(Span<byte> destination)
    {
        ulong value = (reflected ? Reverse(register, width) : register) ^ finalXor;
        if (width == 64) BinaryPrimitives.WriteUInt64LittleEndian(destination, value);
        else if (reflected) BinaryPrimitives.WriteUInt32LittleEndian(destination, (uint)value);
        else BinaryPrimitives.WriteUInt32BigEndian(destination, (uint)value);
    }

    private static ulong Reverse(ulong value, int bits)
    {
        ulong result = 0;
        for (int bit = 0; bit < bits; bit++, value >>= 1) result = (result << 1) | (value & 1);
        return result;
    }
}

internal sealed class FrameAdler() : NonCryptographicHashAlgorithm(4)
{
    private uint sum = 1, weighted;
    public override void Append(ReadOnlySpan<byte> source)
    {
        foreach (byte octet in source)
        {
            sum = (sum + octet) % 65521;
            weighted = (weighted + sum) % 65521;
        }
    }
    public override void Reset() { sum = 1; weighted = 0; }
    protected override void GetCurrentHashCore(Span<byte> destination) =>
        BinaryPrimitives.WriteUInt32BigEndian(destination, (weighted << 16) | sum);
}
// endsnippet:algorithms
