using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.X86;
using System.Runtime.InteropServices;

namespace Playbook.Samples;

// snippet:dispatch
// SVE contracts are experimental. This opt-in is scoped to this original sample.
#pragma warning disable SYSLIB5003
public static partial class Instructions
{
    public static string BiasPath => Sve.IsSupported ? "SVE" : Avx2.IsSupported ? "AVX2" : "scalar";
    public static string ProductPath => AvxVnni.V512.IsSupported ? "AVX-VNNI-512" : Avx2.IsSupported ? "AVX2" : "scalar";
    public static string Features() => $"arch={RuntimeInformation.ProcessArchitecture}; SVE={Sve.IsSupported}; SVE2={Sve2.IsSupported}; SVE_SHA3={SveSha3.IsSupported}; SVE_SM4={SveSm4.IsSupported}; AVX2={Avx2.IsSupported}; AVX-VNNI-512={AvxVnni.V512.IsSupported}";

    public static int[] AddBias(ReadOnlySpan<int> input, int bias) =>
        Sve.IsSupported ? AddSve(input, bias) : AddAvxOrScalar(input, bias);

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static unsafe int[] AddSve(ReadOnlySpan<int> input, int bias)
    {
        int[] output = new int[input.Length];
        fixed (int* source = input)
        fixed (int* destination = output)
        {
            int width = checked((int)Sve.Count32BitElements(SveMaskPattern.All));
            for (long i = 0; i < input.Length; i += width)
            {
                // Signed Int32 lanes; exclusive logical end masks the final partial vector.
                Vector<int> active = Sve.CreateWhileLessThanMaskInt32(i, (long)input.Length);
                Vector<int> values = Sve.LoadVector(active, source + i);
                Sve.StoreAndZip(active, destination + i, Sve.Add(values, new Vector<int>(bias)));
            }
        }
        return output;
    }

    public static int SumProducts(ReadOnlySpan<short> left, ReadOnlySpan<short> right)
    {
        if (left.Length != right.Length) throw new ArgumentException("Product spans must have equal lengths.");
        return AvxVnni.V512.IsSupported ? SumVnni512(left, right) : SumAvxOrScalar(left, right);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int SumVnni512(ReadOnlySpan<short> left, ReadOnlySpan<short> right)
    {
        Vector512<int> sums = Vector512<int>.Zero;
        int i = 0;
        for (; i <= left.Length - Vector512<short>.Count; i += Vector512<short>.Count)
        {
            Vector512<short> a = Vector512.LoadUnsafe(ref MemoryMarshal.GetReference(left), (nuint)i);
            Vector512<short> b = Vector512.LoadUnsafe(ref MemoryMarshal.GetReference(right), (nuint)i);
            sums = AvxVnni.V512.MultiplyWideningAndAdd(sums, a, b);
        }
        int total = Vector512.Sum(sums);
        for (; i < left.Length; i++) total = unchecked(total + left[i] * right[i]);
        return total;
    }
}
#pragma warning restore SYSLIB5003
// endsnippet:dispatch
