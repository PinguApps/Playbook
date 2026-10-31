using System.Buffers.Binary;
using System.IO.Hashing;

// snippet:checksum
internal static class FrameChecksums
{
    public static NonCryptographicHashAlgorithm Create(FrameChecksum kind) => kind switch
    {
        FrameChecksum.Adler => new FrameAdler(),
        FrameChecksum.Castagnoli => new FrameCrc(32, 0x1EDC6F41, uint.MaxValue, uint.MaxValue, true),
        FrameChecksum.Nvme => new FrameCrc(64, 0xAD93D23594C93659, ulong.MaxValue, ulong.MaxValue, true),
        FrameChecksum.Mpeg2 => new FrameCrc(32, 0x04C11DB7, uint.MaxValue, 0, false),
        _ => throw new ArgumentOutOfRangeException(nameof(kind))
    };

    public static byte[] Hash(ReadOnlySpan<byte> payload, FrameChecksum kind)
    {
        var hash = Create(kind);
        hash.Append(payload);
        return hash.GetCurrentHash();
    }

    public static ulong Value(ReadOnlySpan<byte> payload, FrameChecksum kind)
    {
        byte[] bytes = Hash(payload, kind);
        return kind switch
        {
            FrameChecksum.Nvme => BinaryPrimitives.ReadUInt64LittleEndian(bytes),
            FrameChecksum.Castagnoli => BinaryPrimitives.ReadUInt32LittleEndian(bytes),
            _ => BinaryPrimitives.ReadUInt32BigEndian(bytes)
        };
    }
}
// endsnippet:checksum
