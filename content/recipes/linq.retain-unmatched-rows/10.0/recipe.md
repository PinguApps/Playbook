# Preserve unmatched records on the required side of a join

## Goal

Compare a fictional harbour's expected crate manifest with observed crates. Keep a missing observation visible when the expected manifest is authoritative; keep an unexpected observation visible when the scan feed is authoritative. Neither operation alone is a full reconciliation.

## Older approach

Use a net9.0 / C# 13 console project with Nullable and ImplicitUsings enabled. The following declarations use namespace Playbook.Samples. GroupJoin gathers each preserved row's matches; DefaultIfEmpty supplies the absent side. Reversing the inputs implements the other direction. This remains a valid approach on older releases.

{{snippet:older}}

## Newer approach

Target net10.0 / C# 14. Framework System.Linq supplies LeftJoin and RightJoin, so no package is needed. Keep the same key comparison and nullable missing-side handling.

{{snippet:newer}}

Call Manifests.KeepExpected(expected, observed) for a complete expected-side report, or KeepObserved for a complete observed-side report. In each Difference, a null Observed means missing and a null Expected means unexpected. Read Units through the non-null record; zero units is real data, not an absence marker. These reference records deliberately distinguish absence from an ordinary value-type default.

## Migration

1. Choose which input must survive without a match; do not swap LeftJoin and RightJoin accidentally.
2. Preserve the key selectors and comparer. This example uses non-null codes and OrdinalIgnoreCase; validate input contracts separately.
3. Replace the matching GroupJoin/SelectMany/DefaultIfEmpty composition with the corresponding operator.
4. Handle a null missing side before comparing values. Duplicate keys produce every matching pair rather than one arbitrarily chosen record.
5. If both unmatched sides matter, retrieve linq.reconcile-all-rows for .NET 11, or combine a left report with right-only records on .NET 10.
6. Test the consumer's actual database/provider query separately before replacing a server-side query.

## Constraints

Enumerable operates in memory. Queryable also declares result-selector LeftJoin/RightJoin in .NET 10, but constructs provider expressions: API availability is not SQL translation support. EF Core 10 documents these operators; the exact EF version, relational provider and database must support the consumer's query shape. The comparer used here is an in-memory policy, not a portable database collation instruction. No database behavior is claimed or required by this route.

AsyncEnumerable also provides these result-selector operators in the .NET 10 framework. Use them with IAsyncEnumerable inputs and enumerate with WithCancellation(token). A join may build a lookup of the opposite input; asynchronous input does not make it a constant-memory transform. These calls do not turn IQueryable database execution into asynchronous client streaming.

The shared edition is verified unchanged on .NET 11 / C# 15. .NET 9 is comparison-only, not a served target. No package asset, optional workload, rendering mode or experimental opt-in is required. No trimming, Native AOT or older-target package route is established.

## Evidence

Actual SDK 9.0.319, 10.0.402 and 11.0.100-rc.2.26504.105 builds execute against their own .NET 9.0.21, 10.0.13 and 11 RC2 runtimes. Checks retain a left-only A, a right-only C and all four pairs from two B rows on each side, including empty-input cases. .NET 10/11 checks also execute Queryable using EnumerableQuery and real asynchronous feeds. That is managed execution, not a mock or database integration check. Installed stdio list/get compares this entire body and canonical snippets offline. RC2 is not GA proof; no Human action is required for these bounded claims.
