using System.Collections;

namespace Playbook.Samples;

// snippet:bits
public static class BitCollections
{
    public static BitArray FromBytes(ReadOnlySpan<byte> bytes) => new(bytes);
    public static BitArray FromFlags(ReadOnlySpan<bool> flags) => new(flags);
    public static BitArray FromWords(ReadOnlySpan<int> words) => new(words);
    public static int Count(BitArray bits) => bits.PopCount();
}
// endsnippet:bits

// snippet:keys
public readonly record struct Shipment(string? Bay, int Ticket);

public static class ShipmentKeys
{
    public static IEqualityComparer<Shipment> ByBay() =>
        EqualityComparer<Shipment>.Create(x => x.Bay, StringComparer.OrdinalIgnoreCase);

    public static IEqualityComparer<Shipment> ByExactBay() =>
        EqualityComparer<Shipment>.Create(x => x.Bay);

    // Equality does not define sort order. Keep an explicit ordering comparer and tie-break.
    public static int Compare(Shipment left, Shipment right) => StringComparer.OrdinalIgnoreCase.Compare(left.Bay, right.Bay);
    public static Shipment[] Order(IEnumerable<Shipment> arrivals) => arrivals
        .OrderBy(x => x, Comparer<Shipment>.Create(Compare)).ThenBy(x => x.Ticket).ToArray();
}
// endsnippet:keys
