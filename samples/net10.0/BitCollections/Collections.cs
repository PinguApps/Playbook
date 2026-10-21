using System.Collections;

namespace Playbook.Samples;

// snippet:bits
public static class BitCollections
{
    public static BitArray FromBytes(ReadOnlySpan<byte> bytes) => new(bytes.ToArray());
    public static BitArray FromFlags(ReadOnlySpan<bool> flags) => new(flags.ToArray());
    public static BitArray FromWords(ReadOnlySpan<int> words) => new(words.ToArray());

    public static int Count(BitArray bits)
    {
        int count = 0;
        foreach (bool enabled in bits)
        {
            if (enabled) count++;
        }
        return count;
    }
}
// endsnippet:bits

// snippet:keys
public readonly record struct Shipment(string? Bay, int Ticket);

public static class ShipmentKeys
{
    public static IEqualityComparer<Shipment> ByBay() => new BayEquality(StringComparer.OrdinalIgnoreCase);
    public static IEqualityComparer<Shipment> ByExactBay() => new BayEquality(EqualityComparer<string>.Default);

    private sealed class BayEquality(IEqualityComparer<string> keys) : IEqualityComparer<Shipment>
    {
        public bool Equals(Shipment x, Shipment y) => keys.Equals(x.Bay, y.Bay);
        public int GetHashCode(Shipment value) => value.Bay is null ? 0 : keys.GetHashCode(value.Bay);
    }

    // Ordering is a separate contract. Equal bays tie even when ticket values differ.
    public static int Compare(Shipment left, Shipment right) => StringComparer.OrdinalIgnoreCase.Compare(left.Bay, right.Bay);
    public static Shipment[] Order(IEnumerable<Shipment> arrivals) => arrivals
        .OrderBy(x => x, Comparer<Shipment>.Create(Compare)).ThenBy(x => x.Ticket).ToArray();
}
// endsnippet:keys
