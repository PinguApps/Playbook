namespace Playbook.Samples;

// snippet:outer
public sealed record ManifestLine(string Code, int Units);
public sealed record Difference(ManifestLine? Expected, ManifestLine? Observed);

public static class Manifests
{
    public static IEnumerable<Difference> KeepExpected(IEnumerable<ManifestLine> expected, IEnumerable<ManifestLine> observed) =>
        expected.GroupJoin(observed, x => x.Code, x => x.Code,
                (left, matches) => new { left, matches }, StringComparer.OrdinalIgnoreCase)
            .SelectMany(group => group.matches.DefaultIfEmpty(), (group, right) => new Difference(group.left, right));

    public static IEnumerable<Difference> KeepObserved(IEnumerable<ManifestLine> expected, IEnumerable<ManifestLine> observed) =>
        observed.GroupJoin(expected, x => x.Code, x => x.Code,
                (right, matches) => new { right, matches }, StringComparer.OrdinalIgnoreCase)
            .SelectMany(group => group.matches.DefaultIfEmpty(), (group, left) => new Difference(left, group.right));
}
// endsnippet:outer
