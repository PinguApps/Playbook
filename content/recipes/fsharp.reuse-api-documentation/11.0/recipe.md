# Reuse inherited and external API documentation

## Goal

Reuse shared F# API summaries through inherited or external XML documentation.

## Older approach

Explicit summaries are emitted beside each member. Repeating a short summary is valid, although edits must keep related members consistent.

{{snippet:older}}

## Newer approach

The override retains an inheritdoc tag in emitted XML; it does not copy the base summary into that XML. The include tag selects an external summary at compile time and the emitted dockLabel summary contains its text. Editor tooltip inheritance is a separate consumer behavior.

{{snippet:newer}}

Complete external ApiDocs.xml authoring input:

{{snippet:external-doc}}

## Migration

1. Set GenerateDocumentationFile=true in the F# project. Place ApiDocs.xml beside the API source before building; use the file and XPath below.
2. Use inheritdoc for the override and include for the external summary. Publish the generated XML alongside the library; retain the external file in authoring inputs for reproducible builds.
3. Inspect generated FSharpApi.xml: HarbourPolicy.Limit contains inheritdoc and dockLabel contains the expanded summary. Documentation tools may need their own inheritance resolution.

Use .NET 11 SDK, net11.0 and FSharp.Core 11; exact evidence pins are SDK 11.0.100-rc.2.26504.105 and FSharp.Core 11.0.101-rc2.26504.105, selecting lib/net10.0. The comparison pins SDK 10.0.402 and FSharp.Core 10.1.402 (lib/netstandard2.1). Set LangVersion to 11.0 for the newer example and 10.0 for the comparison.

## Constraints

- The compiler leaves inheritdoc unexpanded. Do not promise copied text or verify IDE tooltips merely from the emitted file.
- The include file/path must select the intended nodes at compilation. The sample checks emitted content; no IDE, FCS symbol-tooltip or documentation-site rendering check is claimed.

No provider, workload or rendering mode is required. This edition supports net11.0 on .NET-supported platforms. The net10.0 source is a historical comparison, not an advertised older-target route for the new feature.

## Evidence

Saved records contain exact build/run commands, actual outputs, resolved packages, authoring host and input hashes for both canonical projects. The real installed-tool harness checks full Markdown and applicability offline. Compiler and external-consumer checks run separately in eng/Test-FSharpBoundaries.ps1. This is actual managed execution on the recorded host, not a mock or cross-compilation. No IDE tooltip, trimmed/Native AOT publish, UI framework, all-platform or GA verification is claimed. RC2 evidence must be reconciled at GA.

Official sources: [Official F# release contract](https://github.com/dotnet/core/blob/main/release-notes/11.0/preview/rc1/fsharp.md); [F# implementation and tests](https://github.com/dotnet/fsharp/pull/19186).
