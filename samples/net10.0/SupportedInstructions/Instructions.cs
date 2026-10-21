using System.Runtime.Intrinsics.X86;

namespace Playbook.Samples;

// snippet:dispatch
public static partial class Instructions
{
    public static string BiasPath => Avx2.IsSupported ? "AVX2" : "scalar";
    public static string ProductPath => BiasPath;
    public static string Features() => $"AVX2={Avx2.IsSupported}; .NET 10 comparison uses AVX2 or scalar";
    public static int[] AddBias(ReadOnlySpan<int> input, int bias) => AddAvxOrScalar(input, bias);

    public static int SumProducts(ReadOnlySpan<short> left, ReadOnlySpan<short> right)
    {
        if (left.Length != right.Length) throw new ArgumentException("Product spans must have equal lengths.");
        return SumAvxOrScalar(left, right);
    }
}
// endsnippet:dispatch
