// snippet:builder
using System.Collections;
using System.Runtime.CompilerServices;

namespace Playbook.Samples;

[CollectionBuilder(typeof(CrateLabelBuilder), nameof(CrateLabelBuilder.Create))]
public sealed class CrateLabels : IEnumerable<string>
{
    private readonly HashSet<string> _labels;

    internal CrateLabels(IEqualityComparer<string> comparer, ReadOnlySpan<string> labels)
    {
        _labels = new HashSet<string>(comparer);
        foreach (string label in labels) { _labels.Add(label); }
    }

    public int Count => _labels.Count;
    public bool Contains(string label) => _labels.Contains(label);
    public IEnumerator<string> GetEnumerator() => _labels.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public static class CrateLabelBuilder
{
    public static CrateLabels Create(ReadOnlySpan<string> labels) => new(StringComparer.Ordinal, labels);
    public static CrateLabels Create(IEqualityComparer<string> comparer, ReadOnlySpan<string> labels) => new(comparer, labels);
}
// endsnippet:builder
