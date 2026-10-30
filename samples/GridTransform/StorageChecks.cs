using System.Numerics.Tensors;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Playbook.Samples;

Console.WriteLine($"runtime={RuntimeInformation.FrameworkDescription}; arch={RuntimeInformation.ProcessArchitecture}; os={RuntimeInformation.OSDescription}");
var asset = typeof(Tensor).Assembly.Location;
Console.WriteLine($"PACKAGE ASSET {asset}; sha256={Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(asset)))}");
foreach (var type in new[] { typeof(Tensor), typeof(Tensor<>), typeof(TensorSpan<>), typeof(ReadOnlyTensorSpan<>) })
    Console.WriteLine($"API {type.FullName}; experimental={string.Join(',', type.CustomAttributes.Where(a => a.AttributeType.Name == "ExperimentalAttribute"))}");
Policy();
Capacity();
if (args.Contains("--policy-only", StringComparer.Ordinal))
{
    Console.WriteLine("PASS ownership policy and Try false controls only; release storage contract NOT exercised");
    return;
}
#if NET11_0
// Expected release contract: published RC2/RC3 fail these checks. Never call that successful proof.
SupportedOverlap();
RejectedLayouts();
OutputLayouts();
ArrayStorage();
Console.WriteLine("PASS release tensor storage contract");
#else
Console.WriteLine("PASS previous grid ownership policy and Try capacity/shape controls");
#endif

static int[] Cells() => [13, 29, 47, 61, 83, 97];
static void Check(bool condition, string name)
{
    if (!condition) throw new InvalidOperationException(name);
    Console.WriteLine($"PASS {name}");
}
static void Values(ReadOnlyTensorSpan<int> view, int[] expected, string name)
{
    var actual = new int[checked((int)view.FlattenedLength)];
    view.FlattenTo(actual);
    Check(actual.SequenceEqual(expected), name);
    Console.WriteLine($"VALUES {name}: {string.Join(',', actual)}");
}
static void Policy()
{
    var cells = Cells();
    ReadOnlyTensorSpan<int> input = new(cells, [2, 2], [3, 1]);
    GridPolicy.CopySnapshot(input, cells);
    Check(cells.SequenceEqual(new[] { 13, 29, 61, 83, 83, 97 }), "snapshot preserves original logical grid values");
    cells = Cells();
    ReadOnlyTensorSpan<int> terrain = new(cells.AsSpan(0, 4), [2, 2]);
    ReadOnlyTensorSpan<int> correction = new(cells.AsSpan(1, 4), [2, 2]);
    var result = GridPolicy.AddIndependent(terrain, correction);
    Values(result.AsReadOnlyTensorSpan(), [42, 76, 108, 144], "independent sum of both overlapping inputs");
    Check(cells.SequenceEqual(Cells()), "allocating sum preserves shared inputs");
    // When caller-owned output is needed, snapshot EACH input before writing it.
    int[] left = new int[4], right = new int[4];
    terrain.FlattenTo(left);
    correction.FlattenTo(right);
    Tensor.Add(new ReadOnlyTensorSpan<int>(left, [2, 2]), new ReadOnlyTensorSpan<int>(right, [2, 2]), new TensorSpan<int>(cells.AsSpan(2, 4), [2, 2]));
    Check(cells.SequenceEqual(new[] { 13, 29, 42, 76, 108, 144 }), "both snapshots retain intended values before shared output");
}
static void Capacity()
{
    var cells = Cells();
    var before = cells.ToArray();
    ReadOnlyTensorSpan<int> input = new(cells, [2, 2], [3, 1]);
    Check(!input.TryCopyTo(new TensorSpan<int>(cells.AsSpan(0, 3), [3])), "TryCopyTo insufficient capacity false");
    Check(cells.SequenceEqual(before), "capacity failure writes nothing");
    Check(!input.TryCopyTo(new TensorSpan<int>(cells.AsSpan(0, 4), [1, 4])), "TryCopyTo incompatible shape false");
    Check(!input.TryFlattenTo(cells.AsSpan(0, 3)), "TryFlattenTo insufficient capacity false");
    Check(cells.SequenceEqual(before), "all Try false controls write nothing");
}
#if NET11_0
static void SupportedOverlap()
{
    foreach (var method in new[] { "CopyTo", "TryCopyTo", "FlattenTo", "TryFlattenTo" })
    {
        var cells = Cells();
        ReadOnlyTensorSpan<int> source = new(cells.AsSpan(0, 4), [2, 2]);
        var destination = new TensorSpan<int>(cells.AsSpan(1, 4), [2, 2]);
        switch (method)
        {
            case "CopyTo": source.CopyTo(destination); break;
            case "TryCopyTo": Check(source.TryCopyTo(destination), "dense TryCopyTo succeeds"); break;
            case "FlattenTo": source.FlattenTo(cells.AsSpan(1, 4)); break;
            case "TryFlattenTo": Check(source.TryFlattenTo(cells.AsSpan(1, 4)), "dense TryFlattenTo succeeds"); break;
        }
        Check(cells.SequenceEqual(new[] { 13, 13, 29, 47, 61, 97 }), $"dense shifted {method} is overlap-safe");
    }
    var storage = Cells();
    var strided = new TensorSpan<int>(storage, [2, 2], [3, 1]);
    ReadOnlyTensorSpan<int> same = strided;
    same.CopyTo(strided);
    Check(same.TryCopyTo(strided) && storage.SequenceEqual(Cells()), "identical strided copy is permitted");
    Tensor.Add(same, same, strided);
    Check(storage.SequenceEqual(new[] { 26, 58, 47, 122, 166, 97 }), "identical nonrepeating binary elementwise output");
    Tensor.Negate((ReadOnlyTensorSpan<int>)strided, strided);
    Tensor.Add((ReadOnlyTensorSpan<int>)strided, 1, strided);
    Check(storage.SequenceEqual(new[] { -25, -57, 47, -121, -165, 97 }), "identical unary and scalar output");
    storage = Cells();
    Tensor.ResizeTo(new ReadOnlyTensorSpan<int>(storage.AsSpan(0, 3)), new TensorSpan<int>(storage.AsSpan(1, 5)));
    Check(storage.SequenceEqual(new[] { 13, 13, 29, 47, 0, 0 }), "dense overlapping resize clears added elements");
    var dense = new TensorSpan<int>(storage);
    Tensor.Reverse((ReadOnlyTensorSpan<int>)dense, dense);
    Check(storage.SequenceEqual(new[] { 0, 0, 47, 29, 13, 13 }), "dense in-place reverse");
    Tensor.ReverseDimension((ReadOnlyTensorSpan<int>)dense, dense, 0);
    Check(storage.SequenceEqual(new[] { 13, 13, 29, 47, 0, 0 }), "dense in-place reverse dimension");
    var noOp = new TensorSpan<int>(storage, [1, 3], [0, 2]);
    var before = storage.ToArray();
    Tensor.ReverseDimension((ReadOnlyTensorSpan<int>)noOp, noOp, 0);
    Check(storage.SequenceEqual(before), "singleton-axis strided reversal is a no-op");
}
static void RejectedLayouts()
{
    // Each operation owns fresh storage. Assert all backing cells, including gaps, are unchanged.
    foreach (var wrapper in new[] { "readonly", "writable", "tensor", "interface" })
    foreach (var method in new[] { "CopyTo", "TryCopyTo", "FlattenTo", "TryFlattenTo" })
    {
        var cells = Cells();
        Reject(() => CopyOverlap(cells, wrapper, method), cells, $"strided {wrapper}.{method}");
    }
    foreach (var operation in new[] { "binary", "unary", "scalar", "broadcast", "resize", "resize-writable", "resize-tensor", "reverse", "reverse-dimension", "concatenate" })
    {
        var cells = Cells();
        Reject(() => InvalidOperation(cells, operation), cells, operation);
    }
}
static void CopyOverlap(int[] cells, string wrapper, string method)
{
    var writable = new TensorSpan<int>(cells, [2, 2], [3, 1]);
    ReadOnlyTensorSpan<int> source = writable;
    var tensor = Tensor.Create(cells, [2, 2], [3, 1]);
    var destination = new TensorSpan<int>(cells, [2, 2]);
    if (wrapper == "interface") { CopyInterface(tensor, destination, cells.AsSpan(0, 4), method); return; }
    switch (method)
    {
        case "CopyTo": if (wrapper == "tensor") tensor.CopyTo(destination); else if (wrapper == "writable") writable.CopyTo(destination); else source.CopyTo(destination); break;
        case "TryCopyTo": if (wrapper == "tensor") _ = tensor.TryCopyTo(destination); else if (wrapper == "writable") _ = writable.TryCopyTo(destination); else _ = source.TryCopyTo(destination); break;
        case "FlattenTo": if (wrapper == "tensor") tensor.FlattenTo(cells.AsSpan(0, 4)); else if (wrapper == "writable") writable.FlattenTo(cells.AsSpan(0, 4)); else source.FlattenTo(cells.AsSpan(0, 4)); break;
        case "TryFlattenTo": if (wrapper == "tensor") _ = tensor.TryFlattenTo(cells.AsSpan(0, 4)); else if (wrapper == "writable") _ = writable.TryFlattenTo(cells.AsSpan(0, 4)); else _ = source.TryFlattenTo(cells.AsSpan(0, 4)); break;
    }
}
static void CopyInterface<TTensor>(TTensor source, TensorSpan<int> destination, Span<int> flat, string method) where TTensor : IReadOnlyTensor<TTensor, int>, allows ref struct
{
    switch (method)
    {
        case "CopyTo": source.CopyTo(destination); break;
        case "TryCopyTo": _ = source.TryCopyTo(destination); break;
        case "FlattenTo": source.FlattenTo(flat); break;
        case "TryFlattenTo": _ = source.TryFlattenTo(flat); break;
    }
}
static void InvalidOperation(int[] cells, string operation)
{
    ReadOnlyTensorSpan<int> source = new(cells.AsSpan(0, 4), [2, 2]);
    var shifted = new TensorSpan<int>(cells.AsSpan(1, 4), [2, 2]);
    switch (operation)
    {
        case "binary": Tensor.Add(source, source, shifted); break;
        case "unary": Tensor.Negate(source, shifted); break;
        case "scalar": Tensor.Add(source, 7, shifted); break;
        case "broadcast": Tensor.Add(new ReadOnlyTensorSpan<int>(cells.AsSpan(0, 2), [1, 2]), source, new TensorSpan<int>(cells.AsSpan(0, 4), [2, 2])); break;
        case "resize": Tensor.ResizeTo(new ReadOnlyTensorSpan<int>(cells, [2, 2], [3, 1]), new TensorSpan<int>(cells, [2, 2])); break;
        case "resize-writable": Tensor.ResizeTo(new TensorSpan<int>(cells, [2, 2], [3, 1]), new TensorSpan<int>(cells, [2, 2])); break;
        case "resize-tensor": Tensor.ResizeTo(Tensor.Create(cells, [2, 2], [3, 1]), new TensorSpan<int>(cells, [2, 2])); break;
        case "reverse": var view = new TensorSpan<int>(cells, [2, 2], [3, 1]); Tensor.Reverse((ReadOnlyTensorSpan<int>)view, view); break;
        case "reverse-dimension": var axisView = new TensorSpan<int>(cells, [2, 2], [3, 1]); Tensor.ReverseDimension((ReadOnlyTensorSpan<int>)axisView, axisView, 1); break;
        case "concatenate":
            int[] independent = [107, 109];
            Tensor.Concatenate<int>([Tensor.Create(cells, [2], [2]), Tensor.Create(independent)], new TensorSpan<int>(cells.AsSpan(0, 4)));
            break;
    }
}
static void Reject(Action operation, int[] storage, string name)
{
    var before = storage.ToArray();
    try { operation(); throw new InvalidOperationException($"Expected ArgumentException: {name}"); }
    catch (ArgumentException) { Check(storage.SequenceEqual(before), $"{name} rejects before any write"); }
}
static void OutputLayouts()
{
    foreach (var combine in new[] { false, true })
    {
        int[] storage = [101];
        Reject(() => WriteOutput(storage, [2], [0], combine), storage, $"repeated output combine={combine}");
        WriteOutput(storage, [1], [0], combine);
        Check(storage[0] == 23, $"singleton zero stride permitted combine={combine}");
        storage[0] = 101;
        WriteOutput(storage, [0], [0], combine);
        Check(storage[0] == 101, $"empty output permitted combine={combine}");
    }
}
static void WriteOutput(int[] storage, nint[] lengths, nint[] strides, bool combine)
{
    var destination = new TensorSpan<int>(storage, lengths, strides);
    var count = checked((int)destination.FlattenedLength);
    var input = Tensor.Create(Enumerable.Repeat(23, count).ToArray(), lengths);
    if (combine) Tensor.Concatenate<int>([input], destination);
    else Tensor.ResizeTo(input, destination);
}
static void ArrayStorage()
{
    foreach (var shaped in new[] { false, true })
    {
        Array incompatible = new long[] { 7, 8 };
        Array covariance = new string[] { "moor", "marsh" };
        try { _ = shaped ? new TensorSpan<int>(incompatible, [0], [2], [1]) : new TensorSpan<int>(incompatible); throw new InvalidOperationException("writable incompatible array accepted"); }
        catch (ArrayTypeMismatchException) { Console.WriteLine($"PASS writable incompatible Array shaped={shaped}"); }
        try { _ = shaped ? new ReadOnlyTensorSpan<int>(incompatible, [0], [2], [1]) : new ReadOnlyTensorSpan<int>(incompatible); throw new InvalidOperationException("readonly incompatible array accepted"); }
        catch (ArrayTypeMismatchException) { Console.WriteLine($"PASS readonly incompatible Array shaped={shaped}"); }
        try { _ = shaped ? new TensorSpan<object>(covariance, [0], [2], [1]) : new TensorSpan<object>(covariance); throw new InvalidOperationException("writable covariance accepted"); }
        catch (ArrayTypeMismatchException) { Console.WriteLine($"PASS writable covariance rejected shaped={shaped}"); }
        var readOnly = shaped ? new ReadOnlyTensorSpan<object>(covariance, [0], [2], [1]) : new ReadOnlyTensorSpan<object>(covariance);
        Check((string)readOnly[0] == "moor" && (string)readOnly[1] == "marsh", $"read-only compatible covariance shaped={shaped}");
    }
}
#endif
