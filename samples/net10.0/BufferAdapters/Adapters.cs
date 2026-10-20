using System.Buffers;
using System.Text;

namespace Playbook.Samples;

public static class Adapters
{
    // snippet:streams
    public static string ReadMemory(ReadOnlyMemory<byte> bytes)
    {
        using var stream = new MemoryStream(bytes.ToArray(), writable: false);
        using var reader = new StreamReader(stream, Encoding.UTF8);
        return reader.ReadToEnd();
    }

    public static string ReadSequence(ReadOnlySequence<byte> bytes) => ReadMemory(bytes.ToArray());

    public static string ReadText(string text) => ReadMemory(Encoding.UTF8.GetBytes(text));

    public static void WriteMemory(Memory<byte> destination, ReadOnlySpan<byte> source)
    {
        // Array-backed memory can already use MemoryStream without copying on .NET 10.
        using var stream = new MemoryStream(destination.Length);
        stream.Write(source);
        stream.GetBuffer().AsSpan(0, source.Length).CopyTo(destination.Span);
    }
    // endsnippet:streams

    // snippet:transfer
    public static StringBuilder Transfer(StringBuilder source)
    {
        var result = new StringBuilder(source.Length);
        foreach (ReadOnlyMemory<char> chunk in source.GetChunks())
        {
            result.Append(chunk.Span);
        }

        source.Clear();
        return result;
    }
    // endsnippet:transfer
}
