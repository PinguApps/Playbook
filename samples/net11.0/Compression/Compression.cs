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

    public static byte[] Restore(byte[] bytes, string format)
    {
        using MemoryStream input = new(bytes);
        using Stream decoder = format switch
        {
            "deflate" => new DeflateStream(input, CompressionMode.Decompress),
            "zlib" => new ZLibStream(input, CompressionMode.Decompress),
            "gzip" => new GZipStream(input, CompressionMode.Decompress),
            "zstd" => new ZstandardStream(input, CompressionMode.Decompress),
            _ => throw new ArgumentException("Unknown format.", nameof(format)),
        };
        using MemoryStream output = new();
        decoder.CopyTo(output);
        return output.ToArray();
    }
    // snippet:buffers
    public static byte[][] EncodeDeflateMessages(byte[][] messages)
    {
        using DeflateEncoder encoder = new();
        using DeflateDecoder decoder = new();
        List<byte[]> encoded = [];
        foreach (byte[] message in messages)
        {
            encoder.Reset();
            decoder.Reset();
            byte[] buffer = new byte[checked((int)DeflateEncoder.GetMaxCompressedLength(message.Length))];
            OperationStatus status = encoder.Compress(message, buffer, out int consumed, out int written, isFinalBlock: true);
            if (status != OperationStatus.Done || consumed != message.Length)
                throw new InvalidDataException("Compression did not finish within the allocated bound.");
            byte[] frame = buffer[..written];
            byte[] restored = new byte[message.Length + 1];
            status = decoder.Decompress(frame, restored, out consumed, out written);
            if (status != OperationStatus.Done || consumed != frame.Length || !restored.AsSpan(0, written).SequenceEqual(message))
                throw new InvalidDataException("Independent message failed validation.");
            encoded.Add(frame);
        }
        return encoded.ToArray();
    }

    public static byte[][] EncodeZLibMessages(byte[][] messages)
    {
        using ZLibEncoder encoder = new();
        using ZLibDecoder decoder = new();
        List<byte[]> encoded = [];
        foreach (byte[] message in messages)
        {
            encoder.Reset();
            decoder.Reset();
            byte[] buffer = new byte[checked((int)ZLibEncoder.GetMaxCompressedLength(message.Length))];
            OperationStatus status = encoder.Compress(message, buffer, out int consumed, out int written, isFinalBlock: true);
            if (status != OperationStatus.Done || consumed != message.Length)
                throw new InvalidDataException("Compression did not finish within the allocated bound.");
            byte[] frame = buffer[..written];
            byte[] restored = new byte[message.Length + 1];
            status = decoder.Decompress(frame, restored, out consumed, out written);
            if (status != OperationStatus.Done || consumed != frame.Length || !restored.AsSpan(0, written).SequenceEqual(message))
                throw new InvalidDataException("Independent message failed validation.");
            encoded.Add(frame);
        }
        return encoded.ToArray();
    }

    public static byte[][] EncodeGZipMessages(byte[][] messages)
    {
        using GZipEncoder encoder = new();
        using GZipDecoder decoder = new();
        List<byte[]> encoded = [];
        foreach (byte[] message in messages)
        {
            encoder.Reset();
            decoder.Reset();
            byte[] buffer = new byte[checked((int)GZipEncoder.GetMaxCompressedLength(message.Length))];
            OperationStatus status = encoder.Compress(message, buffer, out int consumed, out int written, isFinalBlock: true);
            if (status != OperationStatus.Done || consumed != message.Length)
                throw new InvalidDataException("Compression did not finish within the allocated bound.");
            byte[] frame = buffer[..written];
            byte[] restored = new byte[message.Length + 1];
            status = decoder.Decompress(frame, restored, out consumed, out written);
            if (status != OperationStatus.Done || consumed != frame.Length || !restored.AsSpan(0, written).SequenceEqual(message))
                throw new InvalidDataException("Independent message failed validation.");
            encoded.Add(frame);
        }
        return encoded.ToArray();
    }

    // endsnippet:buffers

    public static byte[][] EncodeMessages(byte[][] messages, string format) => format switch
    {
        "deflate" => EncodeDeflateMessages(messages),
        "zlib" => EncodeZLibMessages(messages),
        "gzip" => EncodeGZipMessages(messages),
        _ => throw new ArgumentException("Unknown format.", nameof(format)),
    };

    // snippet:zstandard
    public static byte[] EncodeZstandard(byte[] survey)
    {
        using MemoryStream output = new();
        ZstandardCompressionOptions options = new() { Quality = 3, WindowLog2 = 20, AppendChecksum = true };
        using (ZstandardStream compressor = new(output, options, leaveOpen: true))
            compressor.Write(survey);
        return output.ToArray();
    }

    public static byte[][] EncodeZstandardMessages(byte[][] messages)
    {
        using ZstandardEncoder encoder = new(new ZstandardCompressionOptions { Quality = 3, WindowLog2 = 20, AppendChecksum = true });
        using ZstandardDecoder decoder = new(new ZstandardDecompressionOptions { MaxWindowLog2 = 20 });
        List<byte[]> frames = [];
        foreach (byte[] message in messages)
        {
            encoder.Reset();
            decoder.Reset();
            byte[] output = new byte[checked((int)ZstandardEncoder.GetMaxCompressedLength(message.Length))];
            OperationStatus status = encoder.Compress(message, output, out int consumed, out int written, isFinalBlock: true);
            if (status != OperationStatus.Done || consumed != message.Length) throw new InvalidDataException("Incomplete Zstandard frame.");
            byte[] frame = output[..written];
            byte[] restored = new byte[message.Length + 1];
            status = decoder.Decompress(frame, restored, out consumed, out written);
            if (status != OperationStatus.Done || consumed != frame.Length || !restored.AsSpan(0, written).SequenceEqual(message))
                throw new InvalidDataException("Invalid Zstandard frame.");
            frames.Add(frame);
        }
        return frames.ToArray();
    }
    // endsnippet:zstandard
}
