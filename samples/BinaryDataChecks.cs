using System.Buffers;
using System.Buffers.Text;
using System.Formats.Cbor;
using System.Text;
using Playbook.Samples;

byte[] payload = [0x00, 0xFF, 0x7E, 0x80, 0x21];
string encoded = Telemetry.Encode(payload);
if (encoded != "AP9+gCE=" || !Telemetry.Decode(encoded).SequenceEqual(payload) || Encoding.ASCII.GetString(Telemetry.EncodeUtf8(payload)) != encoded)
    throw new InvalidOperationException("Standard padded Base64 round trip failed.");
#if NET11_0_OR_GREATER
Span<char> chars = stackalloc char[8];
Span<byte> decoded = stackalloc byte[5];
if (!Base64.TryEncodeToChars(payload, chars, out int charsWritten) || charsWritten != 8 ||
    !Base64.TryDecodeFromChars(chars, decoded, out int bytesWritten) || bytesWritten != 5 || !decoded.SequenceEqual(payload) ||
    Base64.DecodeFromChars("!bad", decoded, out _, out _) != OperationStatus.InvalidData || Base64.TryEncodeToChars(payload, chars[..1], out _))
    throw new InvalidOperationException("Base64 Try/destination contract failed.");
if (Base64.EncodeToChars(payload.AsSpan(0, 2), chars, out int consumed, out int written, isFinalBlock: false) != OperationStatus.NeedMoreData || consumed != 0 || written != 0)
    throw new InvalidOperationException("Non-final Base64 tail was consumed.");
try { Base64.TryDecodeFromChars("!bad", decoded, out _); throw new InvalidOperationException("RC2 malformed Try input did not throw."); }
catch (FormatException) { }
try { _ = new CborReader(ReadOnlyMemory<byte>.Empty, new CborReaderOptions { ConformanceMode = CborConformanceMode.Strict }, false); throw new InvalidOperationException("Accepted strict incremental mode."); }
catch (ArgumentException) { }
#endif
// Array [31, -8, 1024], split inside the integer header and its two-byte body.
byte[] record = [0x83, 0x18, 0x1F, 0x27, 0x19, 0x04, 0x00];
for (int split = 0; split <= record.Length; split++)
{
    if (!Telemetry.ReadMeasurements(new ReadOnlyMemory<byte>[] { record.AsMemory(0, split), record.AsMemory(split) }).SequenceEqual(new[] { 31, -8, 1024 }))
        throw new InvalidOperationException("CBOR boundary failed at " + split);
}
if (!Telemetry.ReadMeasurements(record.Select(value => (ReadOnlyMemory<byte>)new byte[] { value })).SequenceEqual(new[] { 31, -8, 1024 }))
    throw new InvalidOperationException("One-byte CBOR chunks failed.");
try { _ = Telemetry.ReadMeasurements(new[] { (ReadOnlyMemory<byte>)record[..^1] }); throw new InvalidOperationException("Accepted truncated final CBOR token."); }
catch (CborContentException) { }
try { _ = Telemetry.ReadMeasurements(new[] { (ReadOnlyMemory<byte>)record, (ReadOnlyMemory<byte>)new byte[] { 0 } }); throw new InvalidOperationException("Accepted a second CBOR root."); }
catch (FormatException) { }
catch (CborContentException) { }
Console.WriteLine("PASS: padded character/UTF-8 Base64 round trips, every CBOR split, one-byte chunks and truncated-final rejection.");
