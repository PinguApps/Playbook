# Adapt existing types to generic operations (preview)

## Goal

Use preview SRTP resolution to adapt an existing type to a generic F# operation.

## Older approach

A projection supplies the operation explicitly. It works without preview or changing Uri and remains appropriate when callers should select behavior directly.

{{snippet:older}}

## Newer approach

The public extension supplies ManifestWidth to a statically resolved type parameter (SRTP). The inline algorithm calls that operation for a Uri and returns 12 for moss.example. Opening the extension module is part of the call-site contract.

{{snippet:newer}}

## Migration

1. Open System and enable LangVersion=preview in each F# consumer that resolves the extension.
2. Declare a public extension in a named module, open it where the inline algorithm is defined, then declare the member constraint.
3. For another assembly, reference the library and open Playbook.Samples.ApiPreview.HarbourExtensions at the consuming call site before Manifest.width. The external-consumer check compiles and executes this route.

Use .NET 11 SDK, net11.0 and FSharp.Core 11; exact evidence pins are SDK 11.0.100-rc.2.26504.105 and FSharp.Core 11.0.101-rc2.26504.105, selecting lib/net10.0. The comparison pins SDK 10.0.402 and FSharp.Core 10.1.402 (lib/netstandard2.1). Set LangVersion to preview for the newer example and 10.0 for the comparison.

## Constraints

- Experimental preview resolution can change. Only public extensions satisfy this contract; ordinary extension calls do not themselves require preview.
- The defining module must be in scope. Real or built-in members take precedence; avoid conflicting extensions. This recipe verifies an instance method, not every operator or overload-resolution case.

No provider, workload or rendering mode is required. This edition supports net11.0 on .NET-supported platforms. The net10.0 source is a historical comparison, not an advertised older-target route for the new feature.

## Evidence

Saved records contain exact build/run commands, actual outputs, resolved packages, authoring host and input hashes for both canonical projects. The real installed-tool harness checks full Markdown and applicability offline. Compiler and external-consumer checks run separately in eng/Test-FSharpBoundaries.ps1. This is actual managed execution on the recorded host, not a mock or cross-compilation. No IDE tooltip, trimmed/Native AOT publish, UI framework, all-platform or GA verification is claimed. RC2 evidence must be reconciled at GA.

Official sources: [Official F# release contract](https://github.com/dotnet/core/blob/main/release-notes/11.0/preview/rc2/fsharp.md); [F# implementation and tests](https://github.com/dotnet/fsharp/pull/19602).
