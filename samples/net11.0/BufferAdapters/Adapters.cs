using System.Buffers;
using System.Text;

namespace Playbook.Samples;

public static class Adapters
{
    // snippet:streams
    public static string ReadMemory(ReadOnlyMemory<byte> bytes)
    {
        using var stream = new ReadOnlyMemoryStream(bytes);
        using var reader = new StreamReader(stream, Encoding.UTF8);
        return reader.ReadToEnd();
    }

    public static string ReadSequence(ReadOnlySequence<byte> bytes)
    {
        using var stream = new ReadOnlySequenceStream(bytes);
        using var reader = new StreamReader(stream, Encoding.UTF8);
        return reader.ReadToEnd();
    }

    public static string ReadText(string text)
    {
        using var stream = new StringStream(text, Encoding.UTF8);
        using var reader = new StreamReader(stream, Encoding.UTF8);
        return reader.ReadToEnd();
    }

    public static void WriteMemory(Memory<byte> destination, ReadOnlySpan<byte> source)
    {
        using var stream = new WritableMemoryStream(destination);
        stream.Write(source);
    }
    // endsnippet:streams

    // snippet:transfer
    public static StringBuilder Transfer(StringBuilder source) => StringBuilder.MoveChunks(source);
    // endsnippet:transfer
}
