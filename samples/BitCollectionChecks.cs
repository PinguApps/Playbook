using System.Collections;
using System.Runtime.InteropServices;
using Playbook.Samples;

static void Require(bool condition, string claim)
{
    if (!condition) throw new InvalidOperationException(claim);
}

static int[] Enabled(BitArray bits) => Enumerable.Range(0, bits.Length).Where(i => bits[i]).ToArray();

Span<byte> wire = stackalloc byte[] { 0xff, 0x24, 0x41, 0xff };
BitArray slots = BitCollections.FromBytes(wire.Slice(1, 2));
Require(Enabled(slots).SequenceEqual([2, 5, 8, 14]), "byte slice uses least-significant-bit-first positions");
Require(BitCollections.Count(slots) == 4, "four reserved slots");
wire[1] = 0;
Require(slots[2] && slots[5], "constructor copies rather than retaining span storage");
slots.Length = 10;
Require(BitCollections.Count(slots) == 3, "truncation excludes removed set bits");
slots.Not();
Require(BitCollections.Count(slots) == 7, "count ignores unused storage bits after inversion");

Span<bool> flags = stackalloc bool[] { false, true, false, true, true };
BitArray switches = BitCollections.FromFlags(flags);
flags.Clear();
Require(Enabled(switches).SequenceEqual([1, 3, 4]), "bool constructor copies logical order");
Span<int> words = stackalloc int[] { unchecked((int)0x80000002), 1 };
BitArray packed = BitCollections.FromWords(words);
Require(Enabled(packed).SequenceEqual([1, 31, 32]), "int constructor preserves word and bit order");
Require(BitCollections.Count(packed) == 3 && BitCollections.Count(new BitArray(35, true)) == 35, "word counts and partial final word");
Require(BitCollections.Count(BitCollections.FromBytes(ReadOnlySpan<byte>.Empty)) == 0, "empty collection count");

Shipment[] arrivals = [new("quay-b", 4), new("QUAY-A", 3), new("quay-a", 1), new(null, 8), new(null, 6)];
IEqualityComparer<Shipment> equality = ShipmentKeys.ByBay();
Require(equality.Equals(arrivals[1], arrivals[2]), "explicit ordinal case-insensitive key equality");
Require(equality.GetHashCode(arrivals[1]) == equality.GetHashCode(arrivals[2]), "equal keys have equal hashes");
Require(!equality.Equals(arrivals[0], arrivals[1]), "distinct bays remain distinct");
Require(equality.Equals(arrivals[3], arrivals[4]) && equality.GetHashCode(arrivals[3]) == 0, "null keys form one equality group");
Require(arrivals.Distinct(equality).Count() == 3, "hash-based matching uses bay rather than full record equality");
Require(!ShipmentKeys.ByExactBay().Equals(arrivals[1], arrivals[2]), "default string key comparer is case-sensitive");
Require(ShipmentKeys.Order(arrivals).Select(x => x.Ticket).SequenceEqual([6, 8, 1, 3, 4]), "null-first bay ordering and explicit ticket tie-break");
Require(ShipmentKeys.Compare(arrivals[1], arrivals[2]) == 0, "key comparison ties do not compare ticket values");
Console.WriteLine($"PASS bit counts/order, copied spans, key equality/hash and ordering; {RuntimeInformation.FrameworkDescription}");
