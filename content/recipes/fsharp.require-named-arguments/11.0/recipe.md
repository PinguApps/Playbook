# Require meaningful argument names at F# calls (preview)

## Goal

Require names on selected F# preview calls to prevent same-typed argument swaps.

## Older approach

Named arguments are already available and can be reordered, but callers may still use positional arguments. The older compiler does not enforce the new attribute contract.

{{snippet:older}}

## Newer approach

The annotated method requires names for F# preview callers; the reordered call yields 9. A positional call reports FS3923. Default F# 11 mode still permits positional calls, so this is not a language-independent or runtime validation rule.

{{snippet:newer}}

Complete supplied attribute file (compile before the API):

{{snippet:attribute}}

## Migration

1. The RC2 BCL does not ship RequireNamedArgumentsAttribute. Add the supplied definition below as RequireNamedArguments.fs; do not reference a nonexistent BCL type or add a duplicate if a dependency supplies it.
2. Compile RequireNamedArguments.fs before the API file. Open System.Diagnostics.CodeAnalysis in that file and enable LangVersion=preview in F# callers.
3. Apply the attribute only to selected methods or constructors. Name every affected call argument; verify positional calls fail with FS3923 in preview.

Use .NET 11 SDK, net11.0 and FSharp.Core 11; exact evidence pins are SDK 11.0.100-rc.2.26504.105 and FSharp.Core 11.0.101-rc2.26504.105, selecting lib/net10.0. The comparison pins SDK 10.0.402 and FSharp.Core 10.1.402 (lib/netstandard2.1). Set LangVersion to preview for the newer example and 10.0 for the comparison.

## Constraints

- Experimental: enforcement depends on the calling F# compiler and preview mode; C# callers and runtime invocation are not restricted by this check.
- Preserve parameter names when evolving the API because named callers depend on them. The sample supplies the exact namespace/name and method/constructor AttributeUsage; it is not a shipped framework definition.

No provider, workload or rendering mode is required. This edition supports net11.0 on .NET-supported platforms. The net10.0 source is a historical comparison, not an advertised older-target route for the new feature.

## Evidence

Saved records contain exact build/run commands, actual outputs, resolved packages, authoring host and input hashes for both canonical projects. The real installed-tool harness checks full Markdown and applicability offline. Compiler and external-consumer checks run separately in eng/Test-FSharpBoundaries.ps1. This is actual managed execution on the recorded host, not a mock or cross-compilation. No IDE tooltip, trimmed/Native AOT publish, UI framework, all-platform or GA verification is claimed. RC2 evidence must be reconciled at GA.

Official sources: [Official F# release contract](https://github.com/dotnet/core/blob/main/release-notes/11.0/preview/rc2/fsharp.md); [F# implementation and tests](https://github.com/dotnet/fsharp/pull/20340).
