using System;

namespace Playbook.Samples;

// snippet:storage
public static class ManifestBatch
{
    public static void VerifyRetainedParcels()
    {
        scoped Span<int> morning = default;
        scoped Span<int> evening = default;
        foreach (int parcel in new[] { 241, 582 })
        {
            // Each batch owns separate storage, even after this block ends.
            int[] storage = [parcel, parcel + 1];
            if (parcel == 241) morning = storage;
            else evening = storage;
        }
        morning[0] = 900;
        if (morning[1] != 242 || evening[0] != 582 || evening[1] != 583)
            throw new InvalidOperationException("Independent batch storage changed.");

        // Two enclosing declarations also preserve independence without arrays.
        Span<int> firstStorage = [241, 242];
        Span<int> secondStorage = [582, 583];
        scoped Span<int> first = default;
        scoped Span<int> second = default;
        { first = firstStorage; second = secondStorage; }
        first[0] = 900;
        if (first[1] != 242 || second[0] != 582 || second[1] != 583)
            throw new InvalidOperationException("Enclosing storage changed.");

        // Sharing is legal, but gives two views of the SAME final contents.
        Span<int> reusable = [0, 0];
        scoped Span<int> earlierView = default;
        scoped Span<int> laterView = default;
        foreach (int parcel in new[] { 241, 582 })
        {
            reusable[0] = parcel;
            if (parcel == 241) earlierView = reusable;
            else laterView = reusable;
        }
        laterView[1] = 77;
        if (earlierView[0] != 582 || earlierView[1] != 77)
            throw new InvalidOperationException("Shared views stopped aliasing.");
        Console.WriteLine("PASS span: independent arrays/enclosing spans; shared views=582,77");
    }
}
// endsnippet:storage

public static class Program
{
    public static void Main() => ManifestBatch.VerifyRetainedParcels();
}
