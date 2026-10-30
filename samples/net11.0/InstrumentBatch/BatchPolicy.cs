using System.Numerics.Tensors;

namespace Playbook.Samples;

// snippet:policy
public static class BatchPolicy
{
    // The final .NET 11 package aligns equivalent leading-singleton shapes.
    // The caller's destination keeps its requested dimensions.
    public static void AddAligned(ReadOnlyTensorSpan<int> left,
        ReadOnlyTensorSpan<int> right, in TensorSpan<int> destination)
    {
        Tensor.Add(left, right, destination);
    }

    // Use this before the operation when the instrument protocol requires
    // an exact stored shape, rather than the library's shape equivalence.
    public static void RequireExact(ReadOnlySpan<nint> actual, ReadOnlySpan<nint> expected)
    {
        if (!actual.SequenceEqual(expected))
            throw new ArgumentException("The instrument requires the exact stored dimensions.");
    }
}
// endsnippet:policy
