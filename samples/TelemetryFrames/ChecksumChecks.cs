using System.IO.Hashing;

byte[] vector = "123456789"u8.ToArray();
// CRC RevEng check values are independently catalogued for ASCII 123456789.
// Adler is independently evaluated from RFC1950: sum=478; weighted sum=2334.
var vectors = new[]
{
    (FrameChecksum.Adler, 0x091E01DEUL, "091E01DE"),
    (FrameChecksum.Castagnoli, 0xE3069283UL, "839206E3"),
    (FrameChecksum.Nvme, 0xAE8B14860A799888UL, "8898790A86148BAE"),
    (FrameChecksum.Mpeg2, 0x0376E6E7UL, "0376E6E7")
};
foreach (var (kind, expected, wire) in vectors)
{
    Equal(expected, FrameChecksums.Value(vector, kind));
    Equal(wire, Convert.ToHexString(FrameChecksums.Hash(vector, kind)));
    foreach (byte[] payload in new[] { Array.Empty<byte>(), vector, "Fenwatch/temperature=18;sequence=412"u8.ToArray(), Enumerable.Range(0, 20000).Select(i => (byte)i).ToArray() })
    {
        byte[] once = FrameChecksums.Hash(payload, kind);
        var hash = FrameChecksums.Create(kind);
        foreach (int split in new[] { 0, payload.Length / 2, payload.Length })
        {
            hash.Reset();
            hash.Append(payload.AsSpan(0, split));
            hash.Append(payload.AsSpan(split));
            Equal(Convert.ToHexString(once), Convert.ToHexString(hash.GetCurrentHash()));
            // Neither reading the hash nor a failed TryGetHashAndReset may destroy state.
            byte[] shortBuffer = Enumerable.Repeat((byte)0xA5, hash.HashLengthInBytes - 1).ToArray();
            Equal(false, hash.TryGetHashAndReset(shortBuffer, out int written));
            Equal(0, written);
            Equal(true, shortBuffer.All(b => b == 0xA5));
            Throws<ArgumentException>(() => hash.GetCurrentHash(shortBuffer));
            Equal(Convert.ToHexString(once), Convert.ToHexString(hash.GetHashAndReset()));
            Equal(Convert.ToHexString(FrameChecksums.Hash([], kind)), Convert.ToHexString(hash.GetCurrentHash()));
            hash.Append(vector);
            hash.Reset();
            hash.Append(payload);
            Equal(Convert.ToHexString(once), Convert.ToHexString(hash.GetCurrentHash()));
        }
    }
    Console.WriteLine($"{kind}: input=ASCII123456789; value={expected:X}; format bytes={wire}; segmented/reset/short destinations PASS");
}
#if NET11_0_OR_GREATER
byte[] short32 = new byte[3], short64 = new byte[7];
Equal(false, Adler32.TryHash(vector, short32, out int adlerWritten)); Equal(0, adlerWritten);
Equal(false, Crc32.TryHash(Crc32ParameterSet.Crc32C, vector, short32, out int crcWritten)); Equal(0, crcWritten);
Equal(false, Crc64.TryHash(Crc64ParameterSet.Nvme, vector, short64, out int nvmeWritten)); Equal(0, nvmeWritten);
Throws<ArgumentException>(() => Adler32.Hash(vector, short32));
Throws<ArgumentException>(() => Crc32.Hash(Crc32ParameterSet.Crc32C, vector, short32));
Throws<ArgumentException>(() => Crc64.Hash(Crc64ParameterSet.Nvme, vector, short64));
Equal(0x1EDC6F41u, Crc32ParameterSet.Crc32C.Polynomial);
Equal(0xAD93D23594C93659UL, Crc64ParameterSet.Nvme.Polynomial);
#else
// Existing 10.0.13 defaults are legitimate, but are different format variants.
Equal(0xCBF43926u, Crc32.HashToUInt32(vector));
Equal(0x6C40DF5F0B497347UL, Crc64.HashToUInt64(vector));
#endif
Console.WriteLine($"PASS TelemetryFrames: independent vectors, empty/fictional/20KB payloads, format byte order, one-shot/Append/reset/short destinations. Runtime={Environment.Version}; Hashing={typeof(Crc32).Assembly.Location}; version={typeof(Crc32).Assembly.GetName().Version}");

static void Equal<T>(T expected, T actual)
{
    if (!EqualityComparer<T>.Default.Equals(expected, actual)) throw new InvalidOperationException($"Expected {expected}, got {actual}");
}
static void Throws<T>(Action action) where T : Exception
{
    try { action(); }
    catch (T) { return; }
    throw new InvalidOperationException($"Expected {typeof(T).Name}");
}
