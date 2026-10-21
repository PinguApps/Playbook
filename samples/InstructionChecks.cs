using System.Runtime.InteropServices;
using Playbook.Samples;

foreach (int length in new[] { 0, 1, 7, 8, 9, 31, 32, 33, 79 })
{
    int[] values = Enumerable.Range(0, length).Select(i => i % 3 == 0 ? int.MaxValue : i * 17 - 200).ToArray();
    int[] actual = Instructions.AddBias(values, 23);
    int[] scalar = values.Select(x => unchecked(x + 23)).ToArray();
    if (!actual.SequenceEqual(scalar)) throw new InvalidOperationException($"Bias result mismatch at length {length}");
    short[] left = Enumerable.Range(0, length).Select(i => (short)(i % 2 == 0 ? short.MinValue : 123)).ToArray();
    short[] right = Enumerable.Range(0, length).Select(i => (short)(i % 3 == 0 ? short.MinValue : -57)).ToArray();
    int expected = 0;
    for (int i = 0; i < length; i++) expected = unchecked(expected + left[i] * right[i]);
    if (Instructions.SumProducts(left, right) != expected) throw new InvalidOperationException($"Wrapped dot product mismatch at length {length}");
}

bool rejected = false;
try { Instructions.SumProducts(new short[2], new short[1]); }
catch (ArgumentException) { rejected = true; }
if (!rejected) throw new InvalidOperationException("Mismatched product spans must be rejected");

Console.WriteLine(Instructions.Features());
Console.WriteLine($"PASS scalar agreement at empty, partial, exact and multiple vector lengths; bias={Instructions.BiasPath}, products={Instructions.ProductPath}; {RuntimeInformation.FrameworkDescription}");
#if NET11_0_OR_GREATER
if (args.Contains("--require-sve") && Instructions.BiasPath != "SVE") throw new InvalidOperationException("INCOMPLETE: SVE execution required but unavailable");
if (args.Contains("--require-vnni512") && Instructions.ProductPath != "AVX-VNNI-512") throw new InvalidOperationException("INCOMPLETE: AVX-VNNI-512 execution required but unavailable");
#endif
