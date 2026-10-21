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
    public static IEnumerable<Difference> All(IEnumerable<ManifestLine> expected, IEnumerable<ManifestLine> observed) =>
        expected.FullJoin(observed, x => x.Code, x => x.Code,
            (left, right) => new Difference(left, right), StringComparer.OrdinalIgnoreCase);
}
// endsnippet:full

// snippet:pairs
public static partial class Manifests
{
    public static IEnumerable<(ManifestLine? Outer, ManifestLine? Inner)> Pairs(
        IEnumerable<ManifestLine> expected, IEnumerable<ManifestLine> observed) =>
        expected.FullJoin(observed, x => x.Code, x => x.Code, StringComparer.OrdinalIgnoreCase);
}
// endsnippet:pairs
