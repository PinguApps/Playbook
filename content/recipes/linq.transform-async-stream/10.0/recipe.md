# Filter and enrich an asynchronous stream as records arrive

## Goal

Read a fictional embankment sensor feed, discard invalid or low readings, and resolve a station label only for a retained alert. Return each enriched result before the source finishes. The caller pulls results and can stop or cancel while either the source or label lookup is pending.

## Older approach

Use a net9.0 / C# 13 console project with Nullable and ImplicitUsings enabled, namespace Playbook.Samples, and import System.Runtime.CompilerServices. An async iterator filters and awaits enrichment directly. It needs no package and remains useful when the loop needs custom branching.

{{snippet:older}}

## Newer approach

In net10.0 / C# 14, framework System.Linq.AsyncEnumerable provides Where and cancellation-aware Select over IAsyncEnumerable. Keep ImplicitUsings enabled and the same namespace. No System.Linq.Async package is needed.

{{snippet:newer}}

Enumerate with await foreach over Readings.Transform(source, lookup).WithCancellation(cancellationToken). The lookup delegate takes a station code and CancellationToken and returns ValueTask<string>; an async method can supply it. The enumeration token reaches both the producer and the selected lookup. Consume each Alert inside the loop rather than collecting the entire output. Creating the pipeline does not consume input, and each requested result waits for its own label. Select awaits these enrichments sequentially; this is not a parallel work queue.

## Migration

1. Keep the input as IAsyncEnumerable and preserve the filter before enrichment, so rejected readings do not perform a lookup.
2. On .NET 10, use framework Where and Select; retain the manual iterator on .NET 9 if no package is wanted.
3. Pass the caller's token via WithCancellation. Forward the selector token into the actual I/O operation; do not create an unrelated token.
4. Await each result before requesting another. Stop with break or dispose the enumerator when finished; await foreach handles disposal.
5. Remove a direct System.Linq.Async reference when the consumer no longer needs it. Its 6.0.1 compile assets conflict with framework operators such as Where on .NET 10.
6. If another dependency still needs System.Linq.Async at runtime, retain that dependency while excluding its compile assets. The tested reference is PackageReference Include="System.Linq.Async" ExcludeAssets="compile", with a central PackageVersion of 6.0.1. Do not exclude all assets without checking runtime consumers.
7. Rename applicable legacy SelectAwaitWithCancellation calls to Select. Explicit parameter types such as (int value, CancellationToken token) disambiguate the cancellation-aware selector from an indexed selector returning ValueTask as an ordinary value. Recompile each migrated call; a mechanical name replacement is insufficient.

## Constraints

Where and Select do not collect the complete feed; this sample keeps one pending enrichment at a time. Producer and lookup implementations can still buffer internally, and one record can be large. This establishes incremental processing, not a fixed memory-size or latency bound. ToArrayAsync, grouping, joins and sorting can require buffering and are separate choices.

Cancellation is cooperative: the producer and lookup must honor their tokens. Awaiting a non-cancellable operation does not make it cancellable. A failure propagates to the consumer, and results already consumed are not rolled back. Early disposal unwinds the producer, but does not imply arbitrary external side effects are reversed.

These are client-side async operators, not IQueryable/EF translation. No provider, package asset, workload, rendering mode or experimental flag is required. The unchanged edition is also executed on .NET 11 / C# 15. .NET 9 is comparison-only. The manual .NET 9 route has no dependencies; the separate legacy migration probe uses actual System.Linq.Async 6.0.1 and Microsoft.Bcl.AsyncInterfaces 6.0.0. It establishes that specific conflict/exclusion route, not compatibility of every community operator, a 7.0 migration, or an older-target System.Linq.AsyncEnumerable package route. F# experimental sequence builders are a separate goal.

## Evidence

Real SDK 9.0.319, 10.0.402 and 11.0.100-rc.2.26504.105 builds and executions check deferred startup, filtering before lookup, exact enriched values, early disposal, cancellation during a genuinely pending producer and cancellation during a genuinely pending lookup. A gated producer cannot finish until the first alert is consumed, proving incremental observation without timing guesses. A ten-second cancellation bound fails a stuck check.

The migration probe restores declared actual packages, builds with locked restore, runs the legacy .NET 9 call, reproduces .NET 10 CS0121 on Where, and executes the compile-excluded .NET 10 replacement. Installed stdio list/get verifies this complete canonical comparison, retained edition and applicability offline. These are actual managed checks, not mocks, database execution or cross-compilation. RC2 execution is not GA evidence. No Human action is required for this bounded route.
