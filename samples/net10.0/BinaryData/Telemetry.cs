using System.Buffers.Text;
using System.Formats.Cbor;

namespace Playbook.Samples;

public static class Telemetry
{
    // snippet:base64
    public static string Encode(ReadOnlySpan<byte> payload) => Convert.ToBase64String(payload);

    public static byte[] Decode(string text) => Convert.FromBase64String(text);

    public static byte[] EncodeUtf8(ReadOnlySpan<byte> payload)
    {
        byte[] result = new byte[Base64.GetMaxEncodedToUtf8Length(payload.Length)];
        if (Base64.EncodeToUtf8(payload, result, out int consumed, out int written) != System.Buffers.OperationStatus.Done || consumed != payload.Length)
            throw new InvalidOperationException("Incomplete Base64 encoding.");
        return result[..written];
    }
    // endsnippet:base64

    // snippet:cbor
    public static int[] ReadMeasurements(IEnumerable<ReadOnlyMemory<byte>> chunks)
    {
        // A valid .NET 10 approach buffers the complete finite document first.
        using var buffer = new MemoryStream();
        foreach (ReadOnlyMemory<byte> chunk in chunks)
            buffer.Write(chunk.Span);
        var reader = new CborReader(buffer.ToArray(), CborConformanceMode.Lax);
        reader.ReadStartArray();
        var values = new List<int>();
        while (reader.PeekState() != CborReaderState.EndArray)
            values.Add(reader.ReadInt32());
        reader.ReadEndArray();
        if (reader.PeekState() != CborReaderState.Finished || reader.BytesRemaining != 0)
            throw new FormatException("Unexpected trailing record.");
        return values.ToArray();
    }
    // endsnippet:cbor
}
