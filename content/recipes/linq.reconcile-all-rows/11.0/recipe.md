# Reconcile records while preserving both unmatched sides

## Goal

Reconcile the harbour's expected crate manifest against observed crates. Report matches, expected crates without observations and observations without expected crates, without silently dropping either unmatched side. Multiple rows sharing a code are retained as all matching pairs; enforcing unique keys is a separate domain rule.

## Older approach

Use a net10.0 / C# 14 console project, Nullable and ImplicitUsings enabled, namespace Playbook.Samples. These supporting declarations are complete and also work on .NET 11:

{{snippet:support}}

Compose the left report with right-only rows. Materialize each input once because this valid composition reuses both inputs; it is suitable for bounded manifests, not an endless feed.

{{snippet:older}}

## Newer approach

Target net11.0 / C# 15 and retain the supporting declarations above. Replace the older All method with this partial class. Framework Enumerable.FullJoin expresses the two-sided result directly:

{{snippet:newer}}

A null Expected is an unexpected observation; a null Observed is a missing crate. Compare Units only after classifying the row. Do not replace a missing record with a zero-valued record, since zero can be legitimate data.

When the next step wants the two records themselves, omit the result selector using the new tuple-returning overload:

{{snippet:pairs}}

The pair names are Outer and Inner. The same selector-free pattern is available for Join, GroupJoin, LeftJoin and RightJoin in .NET 11 across Enumerable, Queryable and AsyncEnumerable. GroupJoin returns an outer row plus its matching group, rather than flattening pairs. Tuple convenience does not change the join's inclusion rules. .NET 10's existing selector overloads remain valid.

## Migration

1. Decide whether the report needs both unmatched sides. Keep LeftJoin/RightJoin for one-sided goals.
2. Preserve non-null key validation, key selectors and the equality policy; this example uses OrdinalIgnoreCase.
3. Replace All's composed joins with FullJoin. Do not union distinct rows: that can discard meaningful duplicates.
4. Preserve both nullable sides in the result projection and test missing, unexpected, matched and empty-input results.
5. Select the tuple overload only when its shape fits the next operation; keep an explicit projection for a domain-specific Difference.
6. Bound the inputs and sort explicitly if a downstream report requires a particular order. Validate database translation separately.

## Constraints

FullJoin is introduced in the .NET 11 framework. Enumerable performs an in-memory join; Queryable builds an expression for a provider; AsyncEnumerable takes asynchronous inputs and has both synchronous and cancellation-aware ValueTask selector overloads. For the latter, pass the enumeration token with WithCancellation and honor the supplied token in asynchronous selectors. Async does not mean no buffering: this implementation builds an inner lookup before producing matched rows, so it is unsuitable for an unbounded inner feed. Cancellation remains cooperative.

Queryable's FullJoin and tuple-returning contracts do not guarantee that an EF/provider version translates them. This recipe tests only EnumerableQuery, never a relational provider. Full outer join SQL support and comparer/tuple projection translation are separate provider/database requirements; no SQLite or blanket EF promise is made. Explicitly moving data into Enumerable changes execution location, memory use and query behavior.

No package, workload, rendering mode or experimental opt-in is required for this framework route. No .NET 10 FullJoin package route, trimming or Native AOT behavior is established. The recipe is stable API guidance prepared with .NET 11 RC2 evidence; final GA reconciliation is separate.

## Evidence

Actual net10.0 and net11.0 projects build and execute on .NET 10.0.13 and .NET 11.0.0-rc.2.26504.105. Checks retain both unmatched sides, four duplicate-key matches and empty-input results. The .NET 11 checks compile and execute FullJoin and every tuple join family for Enumerable, in-memory Queryable and AsyncEnumerable, plus cancellation-aware selectors and canceled enumeration. Those checks establish operator availability and managed results, not provider translation, every overload, cross-compilation or AOT execution. Existing immutable source/asset audit records identify the framework contracts. Real installed stdio retrieval verifies full Markdown, edition 11.0 and target-specific applicability offline. No mocks or required Human proof are used.
