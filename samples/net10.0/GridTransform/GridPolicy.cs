using System.Numerics.Tensors;

namespace Playbook.Samples;

// snippet:policy
public static class GridPolicy
{
    public static void CopySnapshot(ReadOnlyTensorSpan<int> source, int[] output)
    {
        // Read every logical input before any output write, even when arrays overlap.
        var snapshot = new int[checked((int)source.FlattenedLength)];
        source.FlattenTo(snapshot);
        ReadOnlyTensorSpan<int> independent = new(snapshot, source.Lengths);
        independent.CopyTo(new TensorSpan<int>(output, source.Lengths));
    }

    public static Tensor<int> AddIndependent(ReadOnlyTensorSpan<int> terrain, ReadOnlyTensorSpan<int> correction)
    {
        // An allocating result keeps output independent of both input views.
        return Tensor.Add(terrain, correction);
    }
}
// endsnippet:policy
