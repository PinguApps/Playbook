using System.Buffers.Text;
using System.Formats.Cbor;

namespace Playbook.Samples;

public static class Telemetry
{
    // snippet:base64
    public static string Encode(ReadOnlySpan<byte> payload) => Base64.EncodeToString(payload);

    public static byte[] Decode(string text) => Base64.DecodeFromChars(text);

    public static byte[] EncodeUtf8(ReadOnlySpan<byte> payload) => Base64.EncodeToUtf8(payload);
    // endsnippet:base64

    // snippet:cbor
    public static int[] ReadMeasurements(IEnumerable<ReadOnlyMemory<byte>> chunks)
    {
        using IEnumerator<ReadOnlyMemory<byte>> incoming = chunks.GetEnumerator();
        // Retain only unconsumed token bytes across boundaries, not the whole document.
        var reader = new CborReader(ReadOnlyMemory<byte>.Empty,
            new CborReaderOptions { ConformanceMode = CborConformanceMode.Lax }, isFinalBlock: false);
        ReadOnlyMemory<byte> current = ReadOnlyMemory<byte>.Empty;
        var values = new List<int>();
        bool started = false;
        while (true)
        {
            CborReaderState state = reader.PeekState();
            if (state == CborReaderState.NeedsMoreData)
            {
                ReadOnlyMemory<byte> remaining = current[^reader.BytesRemaining..];
                bool hasChunk = incoming.MoveNext();
                byte[] next = new byte[remaining.Length + (hasChunk ? incoming.Current.Length : 0)];
                remaining.Span.CopyTo(next);
                if (hasChunk) incoming.Current.Span.CopyTo(next.AsSpan(remaining.Length));
                current = next;
                reader.SlideData(current, isFinalBlock: !hasChunk);
            }
            else if (!started && state == CborReaderState.StartArray)
            {
                reader.ReadStartArray();
                started = true;
            }
            else if (state == CborReaderState.UnsignedInteger || state == CborReaderState.NegativeInteger)
                values.Add(reader.ReadInt32());
            else if (state == CborReaderState.EndArray)
            {
                reader.ReadEndArray();
            }
            else if (state == CborReaderState.Finished && started)
            {
                if (reader.BytesRemaining != 0) throw new FormatException("Unexpected trailing record.");
                while (incoming.MoveNext())
                    if (!incoming.Current.IsEmpty) throw new FormatException("Unexpected trailing record.");
                return values.ToArray();
            }
            else
                throw new FormatException("Expected one measurement array.");
        }
    }
    // endsnippet:cbor
}
