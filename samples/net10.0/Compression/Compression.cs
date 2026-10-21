using System.Buffers;
using System.IO.Compression;

namespace Playbook.Samples;

public static class Compression
{
    // snippet:empty
    public static byte[] CompressEmpty(bool gzip)
    {
        using MemoryStream output = new();
        using (Stream compressor = gzip
            ? new GZipStream(output, CompressionLevel.Optimal, leaveOpen: true)
            : new DeflateStream(output, CompressionLevel.Optimal, leaveOpen: true))
        {
            // Deliberately write nothing; disposal finalises this independent message.
        }
        return output.ToArray();
    }
    // endsnippet:empty

    // snippet:restore
    public static byte[] Restore(byte[] bytes, string format)
    {
        using MemoryStream input = new(bytes);
        using Stream decoder = format switch
        {
            "deflate" => new DeflateStream(input, CompressionMode.Decompress),
            "zlib" => new ZLibStream(input, CompressionMode.Decompress),
            "gzip" => new GZipStream(input, CompressionMode.Decompress),
            _ => throw new ArgumentException("Unknown format.", nameof(format)),
        };
        using MemoryStream output = new();
        decoder.CopyTo(output);
        return output.ToArray();
    }
    // endsnippet:restore

    // snippet:buffers
    public static byte[][] EncodeMessages(byte[][] messages, string format)
    {
        return messages.Select(message =>
        {
            using MemoryStream output = new();
            using (Stream compressor = format switch
            {
                "deflate" => new DeflateStream(output, CompressionLevel.Optimal, leaveOpen: true),
                "zlib" => new ZLibStream(output, CompressionLevel.Optimal, leaveOpen: true),
                "gzip" => new GZipStream(output, CompressionLevel.Optimal, leaveOpen: true),
                _ => throw new ArgumentException("Unknown format.", nameof(format)),
            })
            {
                compressor.Write(message); // Each independent message gets a fresh stream.
            }
            return output.ToArray();
        }).ToArray();
    }
    // endsnippet:buffers

    // snippet:zstandard
    public static byte[] EncodeZstandard(byte[] survey)
    {
        throw new NotSupportedException(".NET 10 has no built-in Zstandard codec. GZip is a different wire format, not a Zstandard replacement.");
    }
    // endsnippet:zstandard
}
