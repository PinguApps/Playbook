using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Playbook.Samples;

// snippet:fallback
public static partial class Instructions
{
    private static int[] AddAvxOrScalar(ReadOnlySpan<int> input, int bias)
    {
        int[] output = new int[input.Length];
        int i = 0;
        if (Avx2.IsSupported)
        {
            Vector256<int> adjustment = Vector256.Create(bias);
            for (; i <= input.Length - Vector256<int>.Count; i += Vector256<int>.Count)
            {
                Vector256<int> values = Vector256.LoadUnsafe(ref MemoryMarshal.GetReference(input), (nuint)i);
                Avx2.Add(values, adjustment).StoreUnsafe(ref MemoryMarshal.GetArrayDataReference(output), (nuint)i);
            }
        }
        for (; i < input.Length; i++) output[i] = unchecked(input[i] + bias);
        return output;
    }

    private static int SumAvxOrScalar(ReadOnlySpan<short> left, ReadOnlySpan<short> right)
    {
        int total = 0;
        int i = 0;
        if (Avx2.IsSupported)
        {
            for (; i <= left.Length - Vector256<short>.Count; i += Vector256<short>.Count)
            {
                Vector256<short> a = Vector256.LoadUnsafe(ref MemoryMarshal.GetReference(left), (nuint)i);
                Vector256<short> b = Vector256.LoadUnsafe(ref MemoryMarshal.GetReference(right), (nuint)i);
                total = unchecked(total + Vector256.Sum(Avx2.MultiplyAddAdjacent(a, b)));
            }
        }
        for (; i < left.Length; i++) total = unchecked(total + left[i] * right[i]);
        return total;
    }
}
// endsnippet:fallback
