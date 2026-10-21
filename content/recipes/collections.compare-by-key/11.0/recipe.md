# Match records by a selected key and define ordering separately

## Goal

Group or match records by one chosen field with consistent equality and hashing, while making the separate sort order and tie-break rule explicit.

Use an SDK console project with implicit usings and nullable enabled. Put the supplied declarations in namespace Playbook.Samples. The older comparison targets net10.0 / C# 14; the newer comparison targets net11.0 / C# 15.

## Older approach

A fictional harbour matches shipments by bay name even when ticket numbers differ. A small comparer class uses the same key comparer for equality and hashing. Null bay names form one group with hash zero. This works on .NET 10 and remains useful when a comparer has additional state or rules. The existing two-delegate EqualityComparer.Create overload can also express this, provided both equality and a consistent hash function are supplied.

{{snippet:older}}

## Newer approach

.NET 11's key-selector overload of EqualityComparer<Shipment>.Create supplies equality and hashing from the same key rule. ByBay chooses ordinal case-insensitive matching; ByExactBay omits the key comparer and therefore uses the default string comparer, which is case-sensitive. Neither changes Shipment's own record equality.

{{snippet:newer}}

Distinct with ByBay retains one shipment per bay, including one null-key group. It does not sort the results. Order uses an independent Comparer and ThenBy(Ticket): null bay names first, then ordinal case-insensitive bay order, then ascending tickets within a bay. Two records can compare as equal by bay while their tickets differ; the tie-break makes the final output deterministic for this example.

## Migration

1. Identify the business key and select its equality rule explicitly. Use ordinal comparison for identifiers unless the application deliberately needs cultural semantics.
2. On net11.0, replace the matching comparer class with the key-selector factory. Pass the resulting comparer to HashSet, Dictionary or LINQ operations such as Distinct that actually consume it.
3. Decide null-key behavior. Here a null string key hashes to zero and compares equal to another null key; it remains distinct from an empty string. The sample uses a value-type record, so it has no null record instance. For reference-type records, the selector receives nullable instances: explicitly reject or map them rather than dereferencing blindly.
4. Keep key values and comparer rules stable while values are stored in a hash-based collection. Remove and reinsert changed keys; changing a key in place can make lookup fail.
5. Specify ordering separately using Comparer, OrderBy and any tie-break. A key equality comparer does not implement IComparer or define a sequence's order.

## Constraints

The key-selector factory is a .NET 11 framework contract with no extra package/assets, provider, workload, rendering mode or experimental opt-in. No older-target package route is established. SDK 11 / C# 15 are the canonical configuration, not a requirement for new language syntax. The older two-delegate factory is a different overload: omitting its hash delegate makes GetHashCode throw, so it is unsuitable for hash-based use without that delegate. Selectors should be deterministic and free of side effects; comparisons can invoke them repeatedly. Hash codes are process details, never persisted identifiers. LINQ-to-Objects checks do not establish query-provider translation.

## Evidence

Both actual net10.0 and net11.0 BitCollections projects compile and execute the same checks: case-insensitive matches, default case-sensitive behavior, equal hashes for equal keys, distinct bay groups, null keys, comparison ties and the exact ticket order 6, 8, 1, 3, 4. The record's independent ticket field makes accidental whole-record equality observable. Saved input hashes, commands and output identify SDK 10.0.402 / C# 14 and SDK 11.0.100-rc.2.26504.105 / C# 15. This is real managed behavior, not provider execution, mocks, cross-compilation or GA verification.
