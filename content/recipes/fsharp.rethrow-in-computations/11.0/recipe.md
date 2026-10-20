# Preserve failure stacks in computations (preview)

## Goal

Preserve an original failure stack inside F# async or task handlers with preview reraise.

## Older approach

ExceptionDispatchInfo captures and throws the original exception with its stack. The return after Throw is unreachable and supplies the computation result type. This remains a valid non-preview fallback.

{{snippet:older}}

## Newer approach

Inside the computation handler, return reraise () preserves the failure and original failAtDock frame. The return is needed because these computations produce int; a unit-valued example can have a different spelling.

{{snippet:newer}}

## Migration

1. Enable LangVersion=preview for the newer async/task source and place reraise inside the with handler.
2. Keep necessary logging or cleanup without replacing the exception with raise error, which can reset its stack. Preserve the result type with return reraise () here.
3. Await both paths and inspect the exception at the outer boundary. Retain ExceptionDispatchInfo where preview is unsuitable.

Use .NET 11 SDK, net11.0 and FSharp.Core 11; exact evidence pins are SDK 11.0.100-rc.2.26504.105 and FSharp.Core 11.0.101-rc2.26504.105, selecting lib/net10.0. The comparison pins SDK 10.0.402 and FSharp.Core 10.1.402 (lib/netstandard2.1). Set LangVersion to preview for the newer example and 10.0 for the comparison.

## Constraints

- Experimental compiler behavior; do not move reraise into an arbitrary callback outside its handler.
- Managed checks cover async and task after suspension and assert the originating no-inline frame. Custom computation builders are not compiled here and may need their own builder-specific validation.

No provider, workload or rendering mode is required. This edition supports net11.0 on .NET-supported platforms. The net10.0 source is a historical comparison, not an advertised older-target route for the new feature.

## Evidence

Saved records contain exact build/run commands, actual outputs, resolved packages, authoring host and input hashes for both canonical projects. The real installed-tool harness checks full Markdown and applicability offline. Compiler and external-consumer checks run separately in eng/Test-FSharpBoundaries.ps1. This is actual managed execution on the recorded host, not a mock or cross-compilation. No IDE tooltip, trimmed/Native AOT publish, UI framework, all-platform or GA verification is claimed. RC2 evidence must be reconciled at GA.

Official sources: [Official F# release contract](https://github.com/dotnet/core/blob/main/release-notes/11.0/preview/rc2/fsharp.md); [F# implementation and tests](https://github.com/dotnet/fsharp/pull/20405).
