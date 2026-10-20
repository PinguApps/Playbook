# Bound asynchronous work before it starts

## Goal

Bound async workflows or task starts while retaining batch results in input order.

## Older approach

Async.Parallel already accepts maxDegreeOfParallelism. For tasks, a semaphore gates each factory before starting its operation, and Task.WhenAll retains input order. This valid older alternative has more lifetime plumbing.

{{snippet:older}}

## Newer approach

Async.parallelLimit accepts cold workflows. Task.parallelLimit accepts CancellationToken -> Task factories so its bound applies before invocation. Completion order may differ from input order; the returned array keeps the original order.

{{snippet:newer}}

## Migration

1. Open System.Threading and System.Threading.Tasks. Pick a positive concurrency limit based on the constrained resource, not the total batch size.
2. For async use Async.parallelLimit limit workflows and start/await the returned workflow. For tasks pass uninvoked factories to Task.parallelLimit limit cancellationToken.
3. Do not create tasks first and wrap already-running values in factories; that work already escaped the limit. Await completion and handle failure/cancellation at the batch boundary.

Use .NET 11 SDK, net11.0 and FSharp.Core 11; exact evidence pins are SDK 11.0.100-rc.2.26504.105 and FSharp.Core 11.0.101-rc2.26504.105, selecting lib/net10.0. The comparison pins SDK 10.0.402 and FSharp.Core 10.1.402 (lib/netstandard2.1). Set LangVersion to 11.0 for the newer example and 10.0 for the comparison.

## Constraints

- Limits active operations, not total input/result storage; this is not a streaming or rate-per-second API. Start/completion order is arbitrary.
- The task helper supplies a token to each factory; forward it to cancellable I/O. A sibling fault cancels the governing sibling token; multiple failures do not imply an AggregateException containing every failure.
- The older semaphore sample preserves admission/order but does not implement all newer sibling-failure cancellation semantics. Focused execution checks successful admission/order for both async and task paths.

No provider, workload or rendering mode is required. This edition supports net11.0 on .NET-supported platforms. The net10.0 source is a historical comparison, not an advertised older-target route for the new feature.

## Evidence

Saved records contain exact build/run commands, actual outputs, resolved packages, authoring host and input hashes for both canonical projects. The real installed-tool harness checks full Markdown and applicability offline. Compiler and external-consumer checks run separately in eng/Test-FSharpBoundaries.ps1. This is actual managed execution on the recorded host, not a mock or cross-compilation. No IDE tooltip, trimmed/Native AOT publish, UI framework, all-platform or GA verification is claimed. RC2 evidence must be reconciled at GA.

Official sources: [Official F# release contract](https://github.com/dotnet/core/blob/main/release-notes/11.0/preview/rc2/fsharp.md); [F# implementation and tests](https://github.com/dotnet/fsharp/pull/20294).
