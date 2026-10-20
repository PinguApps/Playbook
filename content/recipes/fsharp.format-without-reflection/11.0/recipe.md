# Render record and union diagnostics without reflection formatting

## Goal

Use generated record and union ToString implementations with --reflectionfree when diagnostics need field values in a trimming-conscious F# application.

## Older approach

Add `open System.Globalization` before the older definitions.

Provide explicit ToString overrides for the record and union when reflection formatting is unsuitable. They render this diagnostic without relying on the older generated reflection printer. That remains a valid choice when exact output is part of a contract.

{{snippet:older}}

## Newer approach

Compile with OtherFlags=--reflectionfree. F# 11 generates simple ToString implementations for records, anonymous records and union cases. The nested Ready value includes its record fields; Paused keeps its case name. String conversion uses those methods instead of the reflection-oriented %A printer.

{{snippet:newer}}

## Migration

1. Target net11.0, use LangVersion 11.0 and add --reflectionfree to OtherFlags for the assembly declaring the types. Reference FSharp.Core 11; the sample pins RC2 for reproducibility.
2. Use string, ToString or ordinary interpolation for these diagnostics. Remove reflection-oriented %A formatting from the intended deployment path.
3. Check the actual nested field types and output. Keep a custom override when output layout, escaping or culture must be controlled exactly.

## Constraints

- Contained values must also have trimming/AOT-compatible ToString implementations. This flag is not a whole-application safety certificate.
- Simple output is not F# source or a serialization format: strings lack quotes, arrays can show a type name, and null-represented values can print null. StructuredFormatDisplay is not honored by simple formatting.
- This contribution separately compiles and executes --reflectionfree diagnostics and ordinary scalar formatting. It does not claim a trimmed or Native AOT application publish/run.

No provider, workload or rendering mode is required. The declared samples run on Windows with .NET 10 and .NET 11 RC2; other .NET-supported platforms require their normal runtime and globalization support. No older-target SDK/package route is advertised.

## Evidence

The exact SDK pins, resolved FSharp.Core dependencies, input hashes, build commands and actual execution output accompany this recipe. The saved records cover the .NET 10 and .NET 11 samples. The separate compiler probe confirms stable spreads, preview constructor rejection/acceptance and ordinary culture formatting. Reflection-free diagnostics use their own --reflectionfree executable; ordinary formatting is not substituted for that evidence. No trimmed or Native AOT publish/run, cross-compilation or mocks are claimed. RC2 evidence is not GA verification.

Official sources: [F# 11 reflection-free record and union formatting](https://github.com/dotnet/core/blob/main/release-notes/11.0/preview/rc1/fsharp.md); [Compiler guide: simple versus reflection printing](https://fsharp.github.io/fsharp-compiler-docs/reflectionfree-printing.html); [Reflection-free generated ToString implementation](https://github.com/dotnet/fsharp/pull/19976).
