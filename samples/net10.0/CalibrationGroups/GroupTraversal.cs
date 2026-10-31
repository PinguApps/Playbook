// snippet:traversal
using System.Formats.Cbor;

namespace CalibrationGroups;

// One finite observatory value wrapped in single-child arrays or maps keyed "calibration".
public static class GroupTraversal
{
    public static int Read(CborReader reader, int depthLimit)
    {
        var maps = new List<bool>();
        while (reader.PeekState() is CborReaderState.StartArray or CborReaderState.StartMap)
        {
            if (reader.CurrentDepth >= depthLimit) throw new CborContentException("Calibration depth exceeded.");
            bool map = reader.PeekState() == CborReaderState.StartMap;
            if (map)
            {
                reader.ReadStartMap();
                if (reader.ReadTextString() != "calibration") throw new CborContentException("Unknown calibration key.");
            }
            else reader.ReadStartArray();
            maps.Add(map);
        }
        int value = reader.ReadInt32();
        for (int i = maps.Count - 1; i >= 0; i--)
        {
            var expected = maps[i] ? CborReaderState.EndMap : CborReaderState.EndArray;
            if (reader.PeekState() != expected) throw new CborContentException("Expected one calibration child.");
            if (maps[i]) reader.ReadEndMap();
            else reader.ReadEndArray();
        }
        if (reader.BytesRemaining != 0) throw new CborContentException("Trailing calibration data.");
        return value;
    }

    public static byte[] Write(CborWriter writer, int depth, bool maps, bool indefinite, int depthLimit)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(depth);
        for (int i = 0; i < depth; i++)
        {
            if (writer.CurrentDepth >= depthLimit) throw new InvalidOperationException("Calibration depth exceeded.");
            if (maps)
            {
                writer.WriteStartMap(indefinite ? null : 1);
                writer.WriteTextString("calibration");
            }
            else writer.WriteStartArray(indefinite ? null : 1);
        }
        writer.WriteInt32(17);
        for (int i = 0; i < depth; i++)
        {
            if (maps) writer.WriteEndMap();
            else writer.WriteEndArray();
        }
        return writer.Encode(); // Only return a complete document after every container closes.
    }
}
// endsnippet:traversal
