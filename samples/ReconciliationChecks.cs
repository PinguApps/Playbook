using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Playbook.Samples;

ManifestLine[] left = [new("A", 4), new("B", 7), new("B", 9)];
ManifestLine[] right = [new("b", 2), new("B", 3), new("C", 5)];
Check(Manifests.KeepExpected(left, right), 5, 1, 0);
Check(Manifests.KeepObserved(left, right), 5, 0, 1);
Check(Manifests.KeepExpected(left, []), 3, 3, 0);
Check(Manifests.KeepObserved([], right), 3, 0, 3);
#if NET10_0_OR_GREATER
Check(Manifests.All(left, right), 6, 1, 1);
Check(Manifests.All([], right), 3, 0, 3);
Check(Manifests.All(left, []), 3, 3, 0);
Check(Manifests.All([], []), 0, 0, 0);

// Queryable executes only the in-memory EnumerableQuery provider here.
Check(left.AsQueryable().LeftJoin(right, x => x.Code, x => x.Code,
    (a, b) => new Difference(a, b), StringComparer.OrdinalIgnoreCase), 5, 1, 0);
Check(left.AsQueryable().RightJoin(right, x => x.Code, x => x.Code,
    (a, b) => new Difference(a, b), StringComparer.OrdinalIgnoreCase), 5, 0, 1);
Check(await Feed(left).LeftJoin(Feed(right), x => x.Code, x => x.Code,
    (a, b) => new Difference(a, b), StringComparer.OrdinalIgnoreCase).ToArrayAsync(), 5, 1, 0);
Check(await Feed(left).RightJoin(Feed(right), x => x.Code, x => x.Code,
    (a, b) => new Difference(a, b), StringComparer.OrdinalIgnoreCase).ToArrayAsync(), 5, 0, 1);
#endif
#if NET11_0_OR_GREATER
Check(Manifests.Pairs(left, right).Select(pair => new Difference(pair.Outer, pair.Inner)), 6, 1, 1);
Check(left.AsQueryable().FullJoin(right, x => x.Code, x => x.Code,
    (a, b) => new Difference(a, b), StringComparer.OrdinalIgnoreCase), 6, 1, 1);
Check(await Feed(left).FullJoin(Feed(right), x => x.Code, x => x.Code,
    (a, b) => new Difference(a, b), StringComparer.OrdinalIgnoreCase).ToArrayAsync(), 6, 1, 1);
// Tuple-returning overloads: each family is compiled and executed, not inferred.
if (left.Join(right, x => x.Code, x => x.Code, StringComparer.OrdinalIgnoreCase).Count() != 4 ||
    left.GroupJoin(right, x => x.Code, x => x.Code, StringComparer.OrdinalIgnoreCase).Count() != 3 ||
    left.LeftJoin(right, x => x.Code, x => x.Code, StringComparer.OrdinalIgnoreCase).Count() != 5 ||
    left.RightJoin(right, x => x.Code, x => x.Code, StringComparer.OrdinalIgnoreCase).Count() != 5 ||
    left.AsQueryable().Join(right, x => x.Code, x => x.Code, StringComparer.OrdinalIgnoreCase).Count() != 4 ||
    left.AsQueryable().GroupJoin(right, x => x.Code, x => x.Code, StringComparer.OrdinalIgnoreCase).Count() != 3 ||
    left.AsQueryable().LeftJoin(right, x => x.Code, x => x.Code, StringComparer.OrdinalIgnoreCase).Count() != 5 ||
    left.AsQueryable().RightJoin(right, x => x.Code, x => x.Code, StringComparer.OrdinalIgnoreCase).Count() != 5 ||
    left.AsQueryable().FullJoin(right, x => x.Code, x => x.Code, StringComparer.OrdinalIgnoreCase).Count() != 6 ||
    await Feed(left).Join(Feed(right), x => x.Code, x => x.Code, StringComparer.OrdinalIgnoreCase).CountAsync() != 4 ||
    await Feed(left).GroupJoin(Feed(right), x => x.Code, x => x.Code, StringComparer.OrdinalIgnoreCase).CountAsync() != 3 ||
    await Feed(left).LeftJoin(Feed(right), x => x.Code, x => x.Code, StringComparer.OrdinalIgnoreCase).CountAsync() != 5 ||
    await Feed(left).RightJoin(Feed(right), x => x.Code, x => x.Code, StringComparer.OrdinalIgnoreCase).CountAsync() != 5)
{
    throw new InvalidOperationException("Tuple operator cardinality changed.");
}
Check((await Feed(left).FullJoin(Feed(right), x => x.Code, x => x.Code,
    StringComparer.OrdinalIgnoreCase).ToArrayAsync()).Select(pair => new Difference(pair.Outer, pair.Inner)), 6, 1, 1);

using (var cancellation = new CancellationTokenSource())
{
    var rows = Feed(left).FullJoin(Feed(right),
        (x, token) => Key(x, token), (x, token) => Key(x, token),
        (a, b, token) =>
        {
            token.ThrowIfCancellationRequested();
            return ValueTask.FromResult(new Difference(a, b));
        }, StringComparer.OrdinalIgnoreCase);
    await using var iterator = rows.GetAsyncEnumerator(cancellation.Token);
    if (!await iterator.MoveNextAsync()) { throw new InvalidOperationException("Missing first joined row."); }
    await cancellation.CancelAsync();
    try { while (await iterator.MoveNextAsync()) { } throw new InvalidOperationException("Cancellation ignored."); }
    catch (OperationCanceledException) when (cancellation.IsCancellationRequested) { }
}

static ValueTask<string> Key(ManifestLine line, CancellationToken token)
{
    token.ThrowIfCancellationRequested();
    return ValueTask.FromResult(line.Code);
}
#endif
Console.WriteLine($"PASS reconciliation: unmatched sides, empty inputs and duplicate matches; {RuntimeInformation.FrameworkDescription}");
#if NET10_0_OR_GREATER
Console.WriteLine("PASS Enumerable, Queryable (in-memory only) and AsyncEnumerable operators.");

static async IAsyncEnumerable<ManifestLine> Feed(IEnumerable<ManifestLine> lines,
    [EnumeratorCancellation] CancellationToken cancellationToken = default)
{
    foreach (ManifestLine line in lines)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await Task.Yield();
        yield return line;
    }
}
#endif

static void Check(IEnumerable<Difference> source, int total, int leftOnly, int rightOnly)
{
    Difference[] rows = source.ToArray();
    if (rows.Length != total || rows.Count(row => row.Observed is null) != leftOnly ||
        rows.Count(row => row.Expected is null) != rightOnly ||
        rows.Any(row => row.Expected is null && row.Observed is null) ||
        rows.Any(row => row.Expected is not null && row.Observed is not null &&
            !StringComparer.OrdinalIgnoreCase.Equals(row.Expected.Code, row.Observed.Code)))
    {
        throw new InvalidOperationException("Reconciliation lost a side or changed key semantics.");
    }
}
