using System.IO.Hashing;

// snippet:checksum
internal static class FrameChecksums
{
    // Explicit custom set: CRC-32/MPEG-2. Polynomial is normal, not bit-reversed.
    private static readonly Crc32ParameterSet Mpeg2 = Crc32ParameterSet.Create(
        polynomial: 0x04C11DB7, initialValue: uint.MaxValue, finalXorValue: 0, reflectValues: false);

    public static NonCryptographicHashAlgorithm Create(FrameChecksum kind) => kind switch
    {
        FrameChecksum.Adler => new Adler32(),
        FrameChecksum.Castagnoli => new Crc32(Crc32ParameterSet.Crc32C),
        FrameChecksum.Nvme => new Crc64(Crc64ParameterSet.Nvme),
        FrameChecksum.Mpeg2 => new Crc32(Mpeg2),
        _ => throw new ArgumentOutOfRangeException(nameof(kind))
    };

    public static byte[] Hash(ReadOnlySpan<byte> payload, FrameChecksum kind) => kind switch
    {
        FrameChecksum.Adler => Adler32.Hash(payload),
        FrameChecksum.Castagnoli => Crc32.Hash(Crc32ParameterSet.Crc32C, payload),
        FrameChecksum.Nvme => Crc64.Hash(Crc64ParameterSet.Nvme, payload),
        FrameChecksum.Mpeg2 => Crc32.Hash(Mpeg2, payload),
        _ => throw new ArgumentOutOfRangeException(nameof(kind))
    };

    public static ulong Value(ReadOnlySpan<byte> payload, FrameChecksum kind) => kind switch
    {
        FrameChecksum.Adler => Adler32.HashToUInt32(payload),
        FrameChecksum.Castagnoli => Crc32.HashToUInt32(Crc32ParameterSet.Crc32C, payload),
        FrameChecksum.Nvme => Crc64.HashToUInt64(Crc64ParameterSet.Nvme, payload),
        FrameChecksum.Mpeg2 => Crc32.HashToUInt32(Mpeg2, payload),
        _ => throw new ArgumentOutOfRangeException(nameof(kind))
    };
}
// endsnippet:checksum
