#pragma warning disable CA1861 // Each check owns its tensor storage.
using System.Buffers;
using System.Numerics.Tensors;
using System.Runtime.InteropServices;
using Playbook.Samples;

Console.WriteLine($"runtime={RuntimeInformation.FrameworkDescription}; arch={RuntimeInformation.ProcessArchitecture}; os={RuntimeInformation.OSDescription}");
var asset = typeof(Tensor).Assembly.Location;
Console.WriteLine($"TENSOR ASSET {asset}; sha256={Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(asset)))}");
foreach (var type in new[] { typeof(Tensor), typeof(Tensor<>), typeof(TensorSpan<>), typeof(ReadOnlyTensorSpan<>) })
    Console.WriteLine($"API {type.FullName}; experimental={string.Join(',', type.CustomAttributes.Where(a => a.AttributeType.Name == "ExperimentalAttribute"))}");

Policy();
#if NET11_0
// These are final-release expectations, NOT claimed successful RC2 execution.
EquivalentShapes();
Empties();
Combine();
Destination();
Views();
Any();
if (Environment.GetEnvironmentVariable("PLAYBOOK_TENSOR_NATIVE_CHECKS") == "1")
    NativeChecks.Run();
else Console.WriteLine("PENDING native-width checks: use Record-InstrumentBatch.ps1 -RunReleaseChecks on a suitable Windows x64 machine.");
Console.WriteLine("PASS final-release small tensor checks");
#else
Console.WriteLine("PASS .NET10 explicit three-channel normalization policy; no new tensor shape contract claimed");
#endif

static void Check(bool condition, string name)
{
    if (!condition) throw new InvalidOperationException(name);
    Console.WriteLine($"PASS {name}");
}
static void Policy()
{
    ReadOnlyTensorSpan<int> left = new(new[] { 4, 7, 9 }, [3]);
    ReadOnlyTensorSpan<int> right = new(new[] { 2, 5, 8 }, [1, 3]);
    var storage = new int[3];
    var output = new TensorSpan<int>(storage, [1, 1, 3]);
    BatchPolicy.AddAligned(left, right, output);
    Check(storage.SequenceEqual(new[] { 6, 12, 17 }), "instrument sum values");
    Check(output.Lengths.SequenceEqual((ReadOnlySpan<nint>)[1, 1, 3]), "instrument destination retains metadata");
#if NET11_0
    try { BatchPolicy.RequireExact(output.Lengths, [3]); throw new InvalidOperationException("lost exact-shape policy"); }
    catch (ArgumentException) { Console.WriteLine("PASS exact stored-shape policy rejects equivalent padding"); }
#endif
}

#if NET11_0
static void EquivalentShapes()
{
    ReadOnlyTensorSpan<int> flat = new(new[] { 4, 7, 9 }, [3]);
    ReadOnlyTensorSpan<int> leading = new(new[] { 4, 7, 9 }, [1, 1, 3]);
    Check(Tensor.SequenceEqual(flat, leading), "leading singleton equivalence [3]/[1,1,3]");
    ReadOnlyTensorSpan<int> column = new(new[] { 4, 7 }, [2, 1]);
    ReadOnlyTensorSpan<int> row = new(new[] { 4, 7 }, [1, 2]);
    Check(!Tensor.SequenceEqual(column, row), "internal singleton significant");
    var sum = Tensor.Add(column, row);
    Check(sum.Lengths.SequenceEqual((ReadOnlySpan<nint>)[2, 2]) && sum.AsReadOnlyTensorSpan()[1, 1] == 14,
        "broadcasting differs from shape equality");
}
static void Empties()
{
    ReadOnlyTensorSpan<int> empty = default;
    ReadOnlyTensorSpan<int> vector = new(Array.Empty<int>(), [0]);
    ReadOnlyTensorSpan<int> ranked = new(Array.Empty<int>(), [2, 0]);
    ReadOnlyTensorSpan<int> padded = new(Array.Empty<int>(), [1, 0]);
    ReadOnlyTensorSpan<int> incompatible = new(Array.Empty<int>(), [0, 2]);
    Check(empty.Rank == 0 && empty.Lengths.IsEmpty && empty.Strides.IsEmpty, "default metadata rank zero");
    Check(Tensor<int>.Empty.Rank == 0 && Tensor.SequenceEqual(empty, vector), "default effective [0]");
    Check(Tensor.SequenceEqual(vector, padded) && !Tensor.SequenceEqual(vector, ranked)
        && !Tensor.SequenceEqual(ranked, incompatible), "zero dimensions retained in equality");
    var result = Tensor.Add(empty, ranked);
    Check(result.Lengths.SequenceEqual((ReadOnlySpan<nint>)[2, 0]), "default/[2,0] binary success");
    var broadcast = Tensor.Broadcast(empty, [2, 0]);
    Check(broadcast.Lengths.SequenceEqual((ReadOnlySpan<nint>)[2, 0]), "explicit empty Broadcast");
    var destination = new TensorSpan<int>(Array.Empty<int>(), [2, 0]);
    Tensor.BroadcastTo(empty, destination);
    Check(Tensor.TryBroadcastTo(empty, destination), "empty BroadcastTo and TryBroadcastTo");
    Check(destination.Lengths.SequenceEqual((ReadOnlySpan<nint>)[2, 0]) && empty.Rank == 0 && ranked.Rank == 2,
        "empty source and destination metadata retained");
    try { _ = Tensor.Add(vector, incompatible); throw new InvalidOperationException("accepted incompatible zeros"); }
    catch (ArgumentException) { Console.WriteLine("PASS [0]/[0,2] binary rejection"); }
    var wrong = new TensorSpan<int>(Array.Empty<int>(), [0, 2]);
    Check(!Tensor.TryBroadcastTo(vector, wrong), "incompatible empty TryBroadcastTo false");
}
static void Combine()
{
    var first = Tensor.Create(new[] { 4, 7, 9 }, [1, 3]);
    var second = Tensor.Create(new[] { 2, 5, 8 }, [3]);
    var joined = Tensor.ConcatenateOnDimension<int>(1, [first, second]);
    var stacked = Tensor.StackAlongDimension<int>(1, [first, second]);
    Check(joined.Lengths.SequenceEqual((ReadOnlySpan<nint>)[1, 6]) && joined.AsReadOnlyTensorSpan()[0, 5] == 8,
        "concatenate axis1 uses first [1,3]");
    Check(stacked.Lengths.SequenceEqual((ReadOnlySpan<nint>)[1, 2, 3]) && stacked.AsReadOnlyTensorSpan()[0, 1, 2] == 8,
        "stack axis1 uses first [1,3]");
    Check(Tensor.Concatenate<int>([first, second]).Lengths.SequenceEqual((ReadOnlySpan<nint>)[2, 3])
        && Tensor.Stack<int>([first, second]).Lengths.SequenceEqual((ReadOnlySpan<nint>)[2, 1, 3]), "default combine axes");
    Check(Tensor.ConcatenateOnDimension<int>(0, [second, first]).Lengths.SequenceEqual((ReadOnlySpan<nint>)[6])
        && Tensor.StackAlongDimension<int>(1, [second, first]).Lengths.SequenceEqual((ReadOnlySpan<nint>)[3, 2]),
        "reversing inputs changes axis interpretation");
    try { _ = Tensor.ConcatenateOnDimension<int>(1, [second, first]); throw new InvalidOperationException("accepted invalid first axis"); }
    catch (ArgumentException) { Console.WriteLine("PASS first-rank axis rejection"); }
}
static void Destination()
{
    ReadOnlyTensorSpan<int> source = new(new[] { 4, 7, 9 }, [3]);
    var storage = new int[3];
    var output = new TensorSpan<int>(storage, [1, 1, 3]);
    Tensor.Add(source, source, output);
    Check(storage.SequenceEqual(new[] { 8, 14, 18 }), "elementwise destination values");
    Check(source.TryCopyTo(output) && storage.SequenceEqual(new[] { 4, 7, 9 }), "aligned copy values");
    Check(output.Lengths.SequenceEqual((ReadOnlySpan<nint>)[1, 1, 3]), "aligned destination metadata retained");
}
static void Views()
{
    var storage = new[] { 4, -7, 2, 9 };
    ReadOnlyTensorSpan<int> source = new(storage, [2, 2]);
    Check(Tensor.IndexOfMax(source) == 3 && Tensor.IndexOfMin(source) == 1
        && Tensor.IndexOfMaxMagnitude(source) == 3 && Tensor.IndexOfMinMagnitude(source) == 2, "all index extrema");
    var reshaped = Tensor.Reshape(source, [1, 4]);
    Check(reshaped.Lengths.SequenceEqual((ReadOnlySpan<nint>)[1, 4]) && reshaped[0, 3] == 9, "reshape metadata and values");
    Check(Tensor.SqueezeDimension(reshaped, 0).Rank == 1 && Tensor.Unsqueeze(source, 0).Rank == 3, "squeeze unsqueeze metadata");
    var tensor = Tensor.Create(storage, [2, 2]);
    Check(Tensor.PermuteDimensions(tensor, [1, 0]).AsReadOnlyTensorSpan()[0, 1] == 2, "permuted logical values");
    var split = Tensor.Split(source, 2, 0);
    Check(split.Length == 2 && split[1].AsReadOnlyTensorSpan()[0, 1] == 9, "split logical values");
    Check(Tensor.Reverse(source).AsReadOnlyTensorSpan()[0, 0] == 9
        && Tensor.ReverseDimension(source, 1).AsReadOnlyTensorSpan()[0, 0] == -7, "reverse logical values");
    var output = new TensorSpan<int>(new int[4], [1, 4]);
    Tensor.ResizeTo(source, output);
    Check(output[0, 3] == 9 && output.Lengths.SequenceEqual((ReadOnlySpan<nint>)[1, 4]), "resize destination metadata");
    var slice = source.Slice((ReadOnlySpan<NRange>)[1..2, 0..2]);
    Check(slice[0, 0] == 2 && slice.Lengths.SequenceEqual((ReadOnlySpan<nint>)[1, 2]), "slice values and metadata");
    var empty = source.Slice((ReadOnlySpan<NRange>)[2..2, 2..2]);
    var expected = new ReadOnlyTensorSpan<int>(storage, 0, [0, 0], [2, 1]);
    Check(empty == expected, "empty view keeps source storage origin");
    var writable = new TensorSpan<int>(new int[4], [2, 2]);
    ReadOnlyTensorSpan<int> row = new(new[] { 5, 6 }, [1, 2]);
    Tensor.SetSlice(writable, row, (ReadOnlySpan<NRange>)[1..2, 0..2]);
    Check(writable[1, 0] == 5 && writable[1, 1] == 6 && writable[0, 0] == 0, "SetSlice logical destination");
}
static void Any()
{
    ReadOnlyTensorSpan<int> empty = default;
    ReadOnlyTensorSpan<int> ranked = new(Array.Empty<int>(), [2, 0]);
    Check(!Tensor.EqualsAny(empty, 0) && !Tensor.GreaterThanAny(empty, 0) && !Tensor.GreaterThanOrEqualAny(empty, 0)
        && !Tensor.LessThanAny(empty, 0) && !Tensor.LessThanOrEqualAny(empty, 0), "all five scalar Any default empties");
    Check(!Tensor.EqualsAny(ranked, ranked) && !Tensor.GreaterThanAny(ranked, ranked) && !Tensor.GreaterThanOrEqualAny(ranked, ranked)
        && !Tensor.LessThanAny(ranked, ranked) && !Tensor.LessThanOrEqualAny(ranked, ranked), "all five pairwise Any ranked empties");
    var highValues = Enumerable.Repeat(0, 17).ToArray(); highValues[^1] = 7;
    var lowValues = Enumerable.Repeat(3, 17).ToArray(); lowValues[^1] = 1;
    var peerValues = Enumerable.Repeat(2, 17).ToArray();
    var equalityValues = Enumerable.Repeat(2, 17).ToArray(); equalityValues[^1] = 7;
    ReadOnlyTensorSpan<int> high = new(highValues), low = new(lowValues), peers = new(peerValues), equality = new(equalityValues);
    Check(Tensor.EqualsAny(high, 7) && Tensor.GreaterThanAny(high, 6) && Tensor.GreaterThanOrEqualAny(high, 7)
        && Tensor.LessThanAny(low, 2) && Tensor.LessThanOrEqualAny(low, 1), "all five scalar Any late matches");
    Check(Tensor.EqualsAny(high, equality) && Tensor.GreaterThanAny(high, peers) && Tensor.GreaterThanOrEqualAny(high, equality)
        && Tensor.LessThanAny(low, peers) && Tensor.LessThanOrEqualAny(low, peers), "all five pairwise Any late matches");
    Check(!Tensor.EqualsAny(high, 99) && !Tensor.GreaterThanAny(high, 7) && !Tensor.GreaterThanOrEqualAny(high, 8)
        && !Tensor.LessThanAny(low, 1) && !Tensor.LessThanOrEqualAny(low, 0), "all five Any no-match controls");
}
#endif
