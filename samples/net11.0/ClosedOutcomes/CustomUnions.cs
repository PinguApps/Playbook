using System.Runtime.CompilerServices;

// Assertions deliberately exercise compiler lowering, including constant inputs.
#pragma warning disable CA1508

namespace Playbook.Samples;

// snippet:tagged-union
[Union]
public readonly struct ShelfReading : IUnion
{
    private readonly byte _tag;
    private readonly int _number;
    private readonly bool _flag;

    public ShelfReading(int? value) => (_tag, _number, _flag) = (value.HasValue ? (byte)1 : (byte)0, value.GetValueOrDefault(), false);
    public ShelfReading(bool value) => (_tag, _number, _flag) = (2, 0, value);
    public bool HasValue => _tag != 0;
    public object? Value
    {
        get
        {
            ValueReads++;
            return _tag switch { 1 => _number, 2 => _flag, _ => null };
        }
    }
    // Instrumentation proves that matching takes the typed access path.
    public static int ValueReads { get; set; }
    public bool TryGetValue(out int value) { value = _number; return _tag == 1; }
    public bool TryGetValue(out bool value) { value = _flag; return _tag == 2; }
}
// endsnippet:tagged-union

// snippet:provider-union
[Union]
public sealed class ShelfDecision : ShelfDecision.IUnionMembers, IUnion
{
    private readonly object? _value;
    private ShelfDecision(object? value) => _value = value;
    public object? Value => _value;

    public interface IUnionMembers
    {
        public static ShelfDecision Create(ArchiveReceipt? value) => new(value);
        public static ShelfDecision Create(ArchiveRefusal? value) => new(value);
        public object? Value { get; }
    }
}
// endsnippet:provider-union

public union ShelfEnvelope(ShelfReading, string);
public union GenericShelf<T>(T, string);
public union OverlappingShelf(System.IComparable, System.IFormattable);

public union ShelfPattern(ArchiveReceipt, ArchiveRefusal)
{
    public int Length => 1;
    public int this[int index] => index == 0 ? 17 : throw new ArgumentOutOfRangeException(nameof(index));
    public void Deconstruct(out int first, out int second) => (first, second) = (17, 18);
}

[Union]
public readonly struct PriorityShelf : IUnion
{
    public PriorityShelf(int value) => Value = value;
    public object? Value { get; }
    public static PriorityShelf FromInt32(int value) => new(value + 1);
    public static implicit operator PriorityShelf(int value) => FromInt32(value);
}

internal static class CustomUnionChecks
{
    public static void Run()
    {
        // snippet:union-patterns
        ShelfReading empty = default;
        ShelfReading count = (int?)24;
        ShelfReading flag = true;
        ShelfReading absent = (int?)null;
        ShelfReading? noWrapper = null;
        ShelfReading? emptyWrapper = empty;
        Require(empty is { } && empty is null, "{} checks the struct wrapper, null checks its contents");
        Require(count is int number && number == 24, "declaration pattern unwraps contents");
        Require(count is int { } && flag is bool, "typed property/type patterns unwrap contents");
        Require(count is not null && absent is null && noWrapper is null && emptyWrapper is null, "content and wrapper null");
        ShelfReading.ValueReads = 0;
        Require(Describe(count) == "count:24" && Describe(flag) == "flag:True" && Describe(empty) == "empty", "all cases");
        Require(ShelfReading.ValueReads == 0, "typed matching bypasses boxing Value getter");
        // endsnippet:union-patterns
        foreach (ShelfReading reading in new[] { empty, count, flag, absent })
        {
            object? value = reading.Value;
            Require(reading.HasValue == (value is not null), "HasValue agreement");
            Require(reading.TryGetValue(out int n) == (value is int) && (value is not int expected || n == expected), "int agreement");
            Require(reading.TryGetValue(out bool b) == (value is bool) && (value is not bool expectedFlag || b == expectedFlag), "bool agreement");
            Require(value is null or int or bool, "soundness including default");
        }
        Require(new ShelfReading((int?)24).Value is int and 24 && count.Value is int and 24, "direct and implicit creation agree");
        ShelfDecision receipt = new ArchiveReceipt("C-24");
        ShelfDecision refusal = new ArchiveRefusal("humidity");
        ShelfDecision nil = (ArchiveReceipt?)null;
        Require(receipt is ArchiveReceipt { ShelfCode: "C-24" } && refusal is ArchiveRefusal, "provider conversion cases");
        Require(nil is null && ShelfDecision.IUnionMembers.Create((ArchiveRefusal?)null) is null, "provider null equivalence");
        Require(ShelfDecision.IUnionMembers.Create(new ArchiveReceipt("C-24")) is ArchiveReceipt, "direct provider factory");
        PriorityShelf priority = 24;
        Require(priority.Value is int and 25, "operator shadows union conversion");
        ShelfEnvelope nested = count;
        Require(nested.Value is ShelfReading, "nested union retains inner wrapper");
        GenericShelf<int> generic = 24;
        Require(generic is int, "closed generic case");
        ShelfPattern patterned = new ArchiveReceipt("C-24");
        Require(patterned is { Length: 1 } && patterned is (17, 18) && patterned is [17], "untyped property, positional and list patterns use wrapper members");
        ShelfReading? lifted = (int?)24;
        Require(lifted is int and 24, "union conversion to nullable wrapper");
        Console.WriteLine("PASS: custom union contracts, provider factories, nullable cases, typed matching without Value reads, operator precedence and nested storage");
    }

    private static string Describe(ShelfReading value) => value switch
    {
        int count => "count:" + count,
        bool flag => "flag:" + flag,
        null => "empty"
    };

    private static void Require(bool condition, string claim)
    {
        if (!condition) { throw new InvalidOperationException(claim); }
    }
}

