// snippet:codec
using System.Formats.Cbor;

namespace CalibrationGroups;

public static class CalibrationCodec
{
    public static int Read(ReadOnlyMemory<byte> data, int maxDepth = 8)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(maxDepth); // Application policy: explicit finite limit only.
        var reader = new CborReader(data, new CborReaderOptions
        {
            ConformanceMode = CborConformanceMode.Strict,
            AllowMultipleRootLevelValues = false,
            MaxDepth = maxDepth
        });
        return GroupTraversal.Read(reader, int.MaxValue); // Package now enforces depth on entry.
    }

    public static byte[] Write(int depth, bool maps = false, bool indefinite = false, int maxDepth = 8)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(maxDepth);
        var writer = new CborWriter(new CborWriterOptions
        {
            ConformanceMode = CborConformanceMode.Strict,
            AllowMultipleRootLevelValues = false,
            ConvertIndefiniteLengthEncodings = false,
            InitialCapacity = 32,
            MaxDepth = maxDepth
        });
        return GroupTraversal.Write(writer, depth, maps, indefinite, int.MaxValue);
    }
}
// endsnippet:codec
