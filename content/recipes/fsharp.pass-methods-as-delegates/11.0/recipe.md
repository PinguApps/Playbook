# Pass compatible methods directly to delegates

## Goal

Pass a compatible F# method to a .NET delegate; retain adapters when arguments need transformation.

## Older approach

An explicit adapter lambda names the parameters and forwards them to the method. This remains useful when arguments need reordering, validation or transformation. The direct spelling is already accepted by F# 10 too, as the second declaration demonstrates; it is not new F# 11 syntax.

{{snippet:older}}

## Newer approach

F# 11 binds the compatible two-argument delegate directly to the curried method. The pinned executable verifies that the bound method belongs to DockMath, whereas the older executable reports a generated Invoke wrapper. Both produce 11 free slots. The release change is compiler lowering of an already-supported spelling; code already using it needs no rewrite. This is not an allocation or performance benchmark.

{{snippet:newer}}

## Migration

1. Open System, declare the method and choose a matching delegate signature, including argument and return types.
2. Use the method directly when its shape matches. Keep an adapter for transformations or unsupported shapes.
3. Invoke the delegate with representative values; the conversion does not change the API result.

Use .NET 11 SDK, net11.0 and FSharp.Core 11; exact evidence pins are SDK 11.0.100-rc.2.26504.105 and FSharp.Core 11.0.101-rc2.26504.105, selecting lib/net10.0. The comparison pins SDK 10.0.402 and FSharp.Core 10.1.402 (lib/netstandard2.1). Set LangVersion to 11.0 for the newer example and 10.0 for the comparison.

## Constraints

- Direct construction is signature-dependent; this example uses a curried static method and Func<int,int,int>. Do not assume every tupled or overloaded method has the same conversion.
- Automatic compiler closure optimizations do not require rewriting clear working code. No allocation benchmark is claimed.
- Direct spelling predates F# 11. The 11.0 introduction label here identifies the newer compiler lowering, not the first availability of passing a method to a delegate.

No provider, workload or rendering mode is required. This edition supports net11.0 on .NET-supported platforms. The net10.0 source is a historical comparison, not an advertised older-target route for the new feature.

## Evidence

Saved records contain exact build/run commands, actual outputs, resolved packages, authoring host and input hashes for both canonical projects. The real installed-tool harness checks full Markdown and applicability offline. Compiler and external-consumer checks run separately in eng/Test-FSharpBoundaries.ps1. This is actual managed execution on the recorded host, not a mock or cross-compilation. No IDE tooltip, trimmed/Native AOT publish, UI framework, all-platform or GA verification is claimed. RC2 evidence must be reconciled at GA.

Official sources: [Official F# release contract](https://github.com/dotnet/core/blob/main/release-notes/11.0/preview/rc1/fsharp.md); [F# implementation and tests](https://github.com/dotnet/fsharp/pull/19993).
