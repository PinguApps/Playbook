using ManifestEvaluation;

int failures = 0;
Check("matched, left-only, right-only and real zero",
    [new("Kite", 0), new("Anchor", 7), new("Same", 3)],
    [new("Buoy", 4), new("Same", 3), new("Kite", 2)],
    [new("Anchor", 7, null), new("Buoy", null, 4), new("Kite", 0, 2), new("Same", 3, 3)]);
Check("both empty", [], [], []);
Check("left empty", [], [new("B", 0), new("A", 1)], [new("A", null, 1), new("B", null, 0)]);
Check("right empty", [new("B", 0), new("A", 1)], [], [new("A", 1, null), new("B", 0, null)]);
Check("ordinal case and whitespace", [new("a", 1), new(" A", 2)], [new("A", 3)],
    [new(" A", 2, null), new("A", null, 3), new("a", 1, null)]);
Check("permuted inputs", [new("Same", 3), new("Anchor", 7), new("Kite", 0)],
    [new("Kite", 2), new("Same", 3), new("Buoy", 4)],
    [new("Anchor", 7, null), new("Buoy", null, 4), new("Kite", 0, 2), new("Same", 3, 3)]);
Reject("duplicate left matched", [new("A", 1), new("A", 2)], [new("A", 3)]);
Reject("duplicate right matched", [new("A", 1)], [new("A", 2), new("A", 3)]);
Reject("identical unmatched left duplicates", [new("A", 1), new("A", 1)], []);
Reject("identical unmatched right duplicates", [], [new("A", 1), new("A", 1)]);
Console.WriteLine($"Failed assertions: {failures}");
return failures == 0 ? 0 : 1;

void Check(string name, Entry[] left, Entry[] right, Row[] wanted)
{
    try { Report(name, Reconciliation.Compare(left, right).SequenceEqual(wanted)); }
    catch (Exception ex) { Console.WriteLine(ex.GetType().Name); Report(name, false); }
}
void Reject(string name, Entry[] left, Entry[] right)
{
    try { _ = Reconciliation.Compare(left, right); Report(name, false); }
    catch (ArgumentException) { Report(name, true); }
    catch (Exception ex) { Console.WriteLine(ex.GetType().Name); Report(name, false); }
}
void Report(string name, bool passed)
{
    Console.WriteLine($"{(passed ? "PASS" : "FAIL")}: {name}");
    if (!passed) failures++;
}
