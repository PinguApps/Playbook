using System.Buffers;
using System.Text;
using Playbook.Samples;

string manifest = "Beacon Δ / \U0001F680";
byte[] bytes = Encoding.UTF8.GetBytes(manifest);
if (Adapters.ReadMemory(bytes) != manifest || Adapters.ReadText(manifest) != manifest)
    throw new InvalidOperationException("Memory/text reader changed the manifest.");
var first = new Segment(bytes.AsMemory(0, 8));
Segment last = first.Append(bytes.AsMemory(8));
var sequence = new ReadOnlySequence<byte>(first, 0, last, last.Memory.Length);
if (Adapters.ReadSequence(sequence) != manifest)
    throw new InvalidOperationException("Segment boundary changed UTF-8.");
byte[] destination = new byte[bytes.Length + 4];
Adapters.WriteMemory(destination.AsMemory(2, bytes.Length), bytes);
if (!destination.AsSpan(2, bytes.Length).SequenceEqual(bytes) || destination[0] != 0 || destination[^1] != 0)
    throw new InvalidOperationException("Write escaped owned slice.");
var builder = new StringBuilder(4).Append(manifest).Append(new string('x', 9000));
string expected = builder.ToString();
StringBuilder moved = Adapters.Transfer(builder);
builder.Append("next batch");
if (moved.ToString() != expected || builder.ToString() != "next batch")
    throw new InvalidOperationException("Transfer did not isolate the emptied source.");
#if NET11_0_OR_GREATER
using (var borrowed = new ReadOnlyMemoryStream(bytes))
{
    bytes[0] = (byte)'b';
    if (borrowed.ReadByte() != 'b' || !borrowed.CanSeek || borrowed.CanWrite)
        throw new InvalidOperationException("Borrowed memory ownership/capabilities changed.");
}
using (var fixedSize = new WritableMemoryStream(new byte[1]))
{
    fixedSize.WriteByte(1);
    try { fixedSize.WriteByte(2); throw new InvalidOperationException("Unexpected growth."); }
    catch (NotSupportedException) { }
}
#endif
Console.WriteLine("PASS: memory, segmented UTF-8, encoded text, sliced writes and emptied builder ownership.");

sealed class Segment : ReadOnlySequenceSegment<byte>
{
    public Segment(ReadOnlyMemory<byte> memory) => Memory = memory;

    public Segment Append(ReadOnlyMemory<byte> memory)
    {
        var next = new Segment(memory) { RunningIndex = RunningIndex + Memory.Length };
        Next = next;
        return next;
    }

}
