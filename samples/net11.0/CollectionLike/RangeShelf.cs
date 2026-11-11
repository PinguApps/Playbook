namespace Playbook.Samples;

// snippet:ranges
#pragma warning disable CA1819 // Mutable backing array makes the copy-isolation check observable.
public sealed record ArchiveShelf(string[] Labels);
#pragma warning restore CA1819

public static class ArchiveShelfAccess
{
    extension(ArchiveShelf shelf)
    {
        public int Count => shelf.Labels.Length;
        [System.Runtime.CompilerServices.IndexerName("Label")]
        public string this[int slot] => shelf.Labels[slot];
        public ArchiveShelf Slice(int start, int length)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(start);
            ArgumentOutOfRangeException.ThrowIfNegative(length);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(start, shelf.Labels.Length - length);
            return new ArchiveShelf(shelf.Labels.AsSpan(start, length).ToArray());
        }
    }
}

public static class RangeShelfWorkflow
{
    [System.Runtime.CompilerServices.ModuleInitializer]
    public static void Check()
    {
        ArchiveShelf shelf = new(["shelf-c", "shelf-a", "shelf-b"]);
        if (shelf[^1] != "shelf-b" || shelf[1..^1].Labels is not ["shelf-a"] ||
            shelf is not ["shelf-c", .. var tail] || tail.Labels is not ["shelf-a", "shelf-b"] ||
            ArchiveShelfAccess.get_Label(shelf, 1) != "shelf-a")
        {
            throw new InvalidOperationException("Range/list/accessor contract changed.");
        }
        foreach (Action rejected in new Action[] { () => _ = shelf[^0], () => _ = shelf[4..], () => _ = shelf[2..1] })
        {
            try { rejected(); throw new InvalidOperationException("Expected bounds rejection."); }
            catch (ArgumentOutOfRangeException) { }
            catch (IndexOutOfRangeException) { }
        }
        ArchiveShelf copy = shelf[..];
        copy.Labels[0] = "copy-only";
        if (shelf.Labels is not ["shelf-c", "shelf-a", "shelf-b"])
        {
            throw new InvalidOperationException("Slicing changed the source.");
        }
        Console.WriteLine("PASS: from-end, ranges, list patterns, named accessor, bounds and independent copy");
        if (new InstanceShelf()[^1] != 2 || new CountShelf()[^1] != 2)
        {
            throw new InvalidOperationException("Instance implicit indexing or Count lookup lost precedence.");
        }
    }
}
// endsnippet:ranges

public sealed class InstanceShelf
{
    public int Length => 3;
    public int this[int slot] => slot;
}

public sealed class CountShelf
{
    public int Count => 3;
}

public static class LookupAccess
{
    extension(InstanceShelf shelf)
    {
        public int this[Index slot] => -1;
    }
    extension(CountShelf shelf)
    {
        public int Length => 100;
        public int this[int slot] => slot;
    }
}
