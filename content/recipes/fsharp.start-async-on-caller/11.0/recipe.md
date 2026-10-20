# Start an asynchronous computation on the caller

## Goal

Start a blocking async prefix on the caller, outside a SynchronizationContext.

## Older approach

An explicit synchronous prefix runs before entering Async.RunSynchronously. This makes its placement clear; the ordinary runner already may run inline on a thread-pool thread without an ambient context or timeout.

{{snippet:older}}

## Newer approach

The new runner starts the computation on the caller until its first suspension and blocks until completion. The wrapper rejects an ambient SynchronizationContext. RC2 measurement found a continuation posted to that context despite release notes saying it is disregarded; do not rely on that source claim for UI safety.

{{snippet:newer}}

## Migration

1. Open System and System.Threading. Use this runner from a blocking console/test boundary without an ambient SynchronizationContext; keep UI and request paths asynchronous.
2. Check the context before entering, then pass the computation. The example captures thread IDs on the caller, at entry and after Async.Sleep.
3. Await asynchronously instead when blocking is unsuitable. RunSynchronouslyImmediate has no timeout overload; use a token for cooperative cancellation, or the ordinary runner where a timeout is needed.

Use .NET 11 SDK, net11.0 and FSharp.Core 11; exact evidence pins are SDK 11.0.100-rc.2.26504.105 and FSharp.Core 11.0.101-rc2.26504.105, selecting lib/net10.0. The comparison pins SDK 10.0.402 and FSharp.Core 10.1.402 (lib/netstandard2.1). Set LangVersion to 11.0 for the newer example and 10.0 for the comparison.

## Constraints

- Blocks the caller for the whole operation; never use it to make a UI responsive. Context-affine continuations can deadlock a blocked thread.
- Actual pinned RC2 execution posts to a supplied recording context after suspension. The context probe and guard address this source/artifact mismatch; they do not establish safety for every UI framework.
- Only the prefix before suspension is guaranteed to start on the caller. Later execution can move threads.

No provider, workload or rendering mode is required. This edition supports net11.0 on .NET-supported platforms. The net10.0 source is a historical comparison, not an advertised older-target route for the new feature.

## Evidence

Saved records contain exact build/run commands, actual outputs, resolved packages, authoring host and input hashes for both canonical projects. The real installed-tool harness checks full Markdown and applicability offline. Compiler and external-consumer checks run separately in eng/Test-FSharpBoundaries.ps1. This is actual managed execution on the recorded host, not a mock or cross-compilation. No IDE tooltip, trimmed/Native AOT publish, UI framework, all-platform or GA verification is claimed. RC2 evidence must be reconciled at GA.

Official sources: [Official F# release contract](https://github.com/dotnet/core/blob/main/release-notes/11.0/preview/rc1/fsharp.md); [F# implementation and tests](https://github.com/dotnet/fsharp/pull/19804).
