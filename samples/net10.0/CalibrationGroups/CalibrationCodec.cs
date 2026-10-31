// snippet:codec
using System.Formats.Cbor;

namespace CalibrationGroups;

public static class CalibrationCodec
{
    public static int Read(ReadOnlyMemory<byte> data, int maxDepth = 8)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(maxDepth);
        var reader = new CborReader(data, CborConformanceMode.Strict, allowMultipleRootLevelValues: false);
        return GroupTraversal.Read(reader, maxDepth);
    }

    public static byte[] Write(int depth, bool maps = false, bool indefinite = false, int maxDepth = 8)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(maxDepth);
        var writer = new CborWriter(CborConformanceMode.Strict,
            convertIndefiniteLengthEncodings: false, allowMultipleRootLevelValues: false, initialCapacity: 32);
        return GroupTraversal.Write(writer, depth, maps, indefinite, maxDepth);
    }
}
// endsnippet:codec
