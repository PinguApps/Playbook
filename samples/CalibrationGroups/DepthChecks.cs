using System.Formats.Cbor;
using System.Runtime.InteropServices;
using CalibrationGroups;

Console.WriteLine($"RUNTIME {RuntimeInformation.FrameworkDescription} {RuntimeInformation.OSArchitecture}");
Console.WriteLine($"CBOR {typeof(CborReader).Assembly.FullName}");
foreach (bool maps in new[] { false, true })
foreach (bool indefinite in new[] { false, true })
{
    foreach (int depth in new[] { 0, 1, 7, 8 })
    {
        byte[] bytes = CalibrationCodec.Write(depth, maps, indefinite);
        Check(CalibrationCodec.Read(bytes) == 17, "complete accepted calibration value");
        Console.WriteLine($"ACCEPT maps={maps} indefinite={indefinite} depth={depth} value=17 bytes={Convert.ToHexString(bytes)}");
    }
    byte[] tooDeep = CalibrationCodec.Write(9, maps, indefinite, maxDepth: 9);
    Reject<CborContentException>(() => CalibrationCodec.Read(tooDeep), $"READER depth9 maps={maps} indefinite={indefinite}");
    Reject<InvalidOperationException>(() => CalibrationCodec.Write(9, maps, indefinite), $"WRITER depth9 maps={maps} indefinite={indefinite}");
    Check(CalibrationCodec.Read(CalibrationCodec.Write(0, maps, indefinite, 0), 0) == 17, "zero permits scalar");
    Reject<CborContentException>(() => CalibrationCodec.Read(CalibrationCodec.Write(1, maps, indefinite), 0), "READER zero forbids nesting");
    Reject<InvalidOperationException>(() => CalibrationCodec.Write(1, maps, indefinite, 0), "WRITER zero forbids nesting");
}
Reject<CborContentException>(() => CalibrationCodec.Read(new byte[] { 0x81 }), "READER truncated array");
Reject<CborContentException>(() => CalibrationCodec.Read(new byte[] { 0x82, 17, 18 }), "READER extra array member");
Reject<CborContentException>(() => CalibrationCodec.Read(new byte[] { 17, 18 }), "READER trailing root");
Reject<ArgumentOutOfRangeException>(() => CalibrationCodec.Read(new byte[] { 17 }, -1), "application finite-only reader policy");
Reject<ArgumentOutOfRangeException>(() => CalibrationCodec.Write(1, maxDepth: -1), "application finite-only writer policy");

#if NET11_0_OR_GREATER
Check(new CborReaderOptions().MaxDepth == -1 && new CborWriterOptions().MaxDepth == -1, "option sentinel default");
foreach (int? setting in new int?[] { null, -1 })
{
    var reader = setting is null ? new CborReader(Arrays(65)) : new CborReader(Arrays(65), new CborReaderOptions { MaxDepth = setting.Value });
    Check(reader.MaxDepth == 64, "reader default64");
    for (int i = 0; i < 64; i++) reader.ReadStartArray();
    Reject<CborContentException>(() => reader.ReadStartArray(), $"READER default/sentinel={setting} permits64 rejects65");
    Check(GroupTraversal.Read(new CborReader(Arrays(64)), int.MaxValue) == 17, "complete default64 reader control");
    var writer = setting is null ? new CborWriter() : new CborWriter(new CborWriterOptions { MaxDepth = setting.Value });
    Check(writer.MaxDepth == 1000, "writer default1000");
    for (int i = 0; i < 1000; i++) writer.WriteStartArray(1);
    Reject<InvalidOperationException>(() => writer.WriteStartArray(1), $"WRITER default/sentinel={setting} permits1000 rejects1001");
    // A rejected extra container has not opened: close the accepted ones and check the entire encoding.
    writer.WriteInt32(17);
    for (int i = 0; i < 1000; i++) writer.WriteEndArray();
    Check(writer.Encode().SequenceEqual(Arrays(1000)), "complete writer1000 control");
}
Reject<ArgumentOutOfRangeException>(() => new CborReaderOptions { MaxDepth = -2 }, "READER option below-1");
Reject<ArgumentOutOfRangeException>(() => new CborWriterOptions { MaxDepth = -2 }, "WRITER option below-1");
var zeroReader = new CborReader(new byte[] { 17 }, new CborReaderOptions { MaxDepth = 0 });
Check(zeroReader.ReadInt32() == 17 && zeroReader.MaxDepth == 0, "raw zero reader scalar");
Reject<CborContentException>(() => new CborReader(Arrays(1), new CborReaderOptions { MaxDepth = 0 }).ReadStartArray(), "raw zero reader container");
var zeroWriter = new CborWriter(new CborWriterOptions { MaxDepth = 0 });
zeroWriter.WriteInt32(17);
Check(zeroWriter.Encode().SequenceEqual(new byte[] { 17 }), "raw zero writer scalar");
Reject<InvalidOperationException>(() => new CborWriter(new CborWriterOptions { MaxDepth = 0 }).WriteStartArray(1), "raw zero writer container");
#else
var rawReader = new CborReader(Arrays(65));
for (int i = 0; i < 64; i++) rawReader.ReadStartArray();
Reject<CborContentException>(() => rawReader.ReadStartArray(), "OLDER reader10.0.13 backport permits64 rejects65");
var rawWriter = new CborWriter();
for (int i = 0; i < 1001; i++) rawWriter.WriteStartArray(1);
rawWriter.WriteInt32(17);
for (int i = 0; i < 1001; i++) rawWriter.WriteEndArray();
Check(rawWriter.Encode().SequenceEqual(Arrays(1001)), "old raw writer exceeds new default, exact full output");
Console.WriteLine("OLDER reader10.0.13 already has default64; writer1001 succeeds; application budget8 rejects9 before opening container.");
#endif
Console.WriteLine("PASS CalibrationGroups: separate reader/writer, finite limits, zero, sentinel/defaults, invalid settings and complete values.");

static byte[] Arrays(int depth) => Enumerable.Repeat((byte)0x81, depth).Append((byte)17).ToArray();
static void Check(bool condition, string message)
{
    if (!condition) throw new InvalidOperationException(message);
}
static void Reject<T>(Action action, string label) where T : Exception
{
    try { action(); }
    catch (T ex) { Console.WriteLine($"{label}: {ex.GetType().Name}"); return; }
    throw new InvalidOperationException($"Expected {typeof(T).Name}: {label}");
}
