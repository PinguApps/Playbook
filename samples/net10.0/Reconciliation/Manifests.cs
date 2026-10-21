namespace Playbook.Samples;

// snippet:outer
public sealed record ManifestLine(string Code, int Units);
public sealed record Difference(ManifestLine? Expected, ManifestLine? Observed);

public static partial class Manifests
{
    public static IEnumerable<Difference> KeepExpected(IEnumerable<ManifestLine> expected, IEnumerable<ManifestLine> observed) =>
        expected.LeftJoin(observed, x => x.Code, x => x.Code,
            (left, right) => new Difference(left, right), StringComparer.OrdinalIgnoreCase);

    public static IEnumerable<Difference> KeepObserved(IEnumerable<ManifestLine> expected, IEnumerable<ManifestLine> observed) =>
        expected.RightJoin(observed, x => x.Code, x => x.Code,
            (left, right) => new Difference(left, right), StringComparer.OrdinalIgnoreCase);
}
// endsnippet:outer

// snippet:full
public static partial class Manifests
{
    // Materialize once because this composition reads both inputs more than once.
    public static IEnumerable<Difference> All(IEnumerable<ManifestLine> expected, IEnumerable<ManifestLine> observed)
    {
        ManifestLine[] left = expected.ToArray();
        ManifestLine[] right = observed.ToArray();
        return KeepExpected(left, right)
            .Concat(KeepObserved(left, right).Where(row => row.Expected is null));
    }
}
// endsnippet:full
