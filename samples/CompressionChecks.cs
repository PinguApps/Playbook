using System.Buffers;
using System.IO.Compression;
using System.Text;
using Playbook.Samples;

byte[][] messages = [[], Encoding.UTF8.GetBytes("Lighthouse survey: 27/14m"), Encoding.UTF8.GetBytes(new string('b', 8192)), [], [9, 6, 3]];
foreach (string format in new[] { "deflate", "zlib", "gzip" })
{
    byte[][] frames = Compression.EncodeMessages(messages, format);
    for (int i = 0; i < messages.Length; i++)
        if (!Compression.Restore(frames[i], format).SequenceEqual(messages[i])) throw new Exception($"{format} round trip failed.");
    Console.WriteLine($"PASS: {format} empty/nonempty independent message round trips.");
}
foreach (bool gzip in new[] { false, true })
{
    byte[] empty = Compression.CompressEmpty(gzip);
#if NET11_0_OR_GREATER
    if (empty.Length == 0) throw new Exception("Expected a finalised empty compressed frame.");
#else
    if (empty.Length != 0) throw new Exception("Expected .NET 10's no-write empty output.");
#endif
    if (Compression.Restore(empty, gzip ? "gzip" : "deflate").Length != 0) throw new Exception("Empty output failed.");
    Console.WriteLine($"PASS: no-write {(gzip ? "gzip" : "deflate")} output length {empty.Length}.");
}
#if NET11_0_OR_GREATER
foreach (byte[] message in messages)
{
    byte[] frame = Compression.EncodeZstandard(message);
    if (!Compression.Restore(frame, "zstd").SequenceEqual(message)) throw new Exception("Zstandard stream round trip failed.");
}
byte[][] zstdFrames = Compression.EncodeZstandardMessages(messages);
for (int i = 0; i < messages.Length; i++)
    if (!Compression.Restore(zstdFrames[i], "zstd").SequenceEqual(messages[i])) throw new Exception("Zstandard reset failed.");
using (GZipEncoder encoder = new())
{
    if (encoder.Compress(messages[2], [], out _, out _, isFinalBlock: true) != OperationStatus.DestinationTooSmall)
        throw new Exception("Short compression destination was not reported.");
    encoder.Reset();
}
using (GZipDecoder decoder = new())
{
    byte[] frame = Compression.EncodeGZipMessages([messages[2]])[0];
    if (decoder.Decompress(frame, new byte[1], out _, out _) != OperationStatus.DestinationTooSmall)
        throw new Exception("Short decompression destination was not reported.");
    decoder.Reset();
    if (decoder.Decompress(frame.AsSpan(0, 1), new byte[9000], out _, out _) != OperationStatus.NeedMoreData)
        throw new Exception("Incomplete compressed input was not reported.");
}
Console.WriteLine("PASS: Deflate/ZLib/GZip/Zstandard Reset, bounded destinations and partial input status.");
#else
try { Compression.EncodeZstandard(messages[1]); throw new Exception("Unavailable codec silently substituted."); }
catch (NotSupportedException) { }
#endif
Console.WriteLine($"PASS: compression format/lifecycle checks; runtime {Environment.Version}.");
