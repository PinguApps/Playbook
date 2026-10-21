# Annotate embedded code strings for editor tooling

## Goal

Mark a fictional harbour training tool's source-text parameters with the embedded language. Supporting editors can use the metadata while users author the text; it does not turn that text into executable code or runtime validation.

Use an SDK console project targeting net10.0 / C# 14 for the older comparison or net11.0 / C# 15 for the newer sample. Enable implicit usings and nullable, put declarations in namespace Playbook.Samples and import System.Diagnostics.CodeAnalysis. There are no external packages.

## Older approach

StringSyntaxAttribute already accepts arbitrary syntax identifiers. The .NET 10 sample supplies the language names directly. This remains valid when targeting older frameworks or naming a syntax without a framework constant.

{{snippet:older}}

The identifiers are exactly C#, F# and Visual Basic. "VB" is not the VisualBasic constant's value. A particular editor may only recognise some identifiers or may provide no assistance.

## Newer approach

.NET 11 provides the three named constants, removing the repeated identifier literals while preserving the metadata values:

{{snippet:newer}}

These are C# declarations describing three possible string contents; they are not translated F# or VB applications. StringSyntaxAttribute can annotate parameters, properties and fields. Put the attribute where supporting tooling can see the string's intended syntax. Constants are compiled into the attribute metadata; they do not supply a parser, compiler, sandbox or language service.

## Migration

1. Identify public or internal string inputs that actually contain source text. Avoid labelling ordinary descriptions as code.
2. On net11.0 replace repeated language-name identifiers with the matching constant. Keep the same identifier when retaining the .NET 10 comparison.
3. Confirm support in the editor and language service your users employ. Compilation of this sample does not prove highlighting, completion or diagnostics in an editor.
4. If runtime correctness matters, add a separate parser/compiler and an explicit error contract. Treat execution of supplied code as a separate design; this recipe neither executes it nor provides an isolation boundary.

## Constraints

Supporting editors decide how to use syntax metadata. These methods do not parse, compile or execute source, and the attribute does not provide a runtime validation contract.

## Evidence

The example methods return input unchanged. Actual builds and runtime checks on both targets inspect all three attributes and confirm that deliberately invalid source is returned unchanged. This establishes emitted metadata and lack of runtime validation; no editor UI behavior or specific editor version is claimed or required Human proof.

The constants are framework APIs with no external package/assets, provider, rendering mode, optional workload or experimental opt-in. No unverified older-target package route is advertised, and .NET 10 remains a comparison rather than a new catalogue foundation. The newer artifact is SDK 11.0.100-rc.2.26504.105 / C# 15 / .NET 11 RC2; its exact versions and input hashes are saved separately. RC evidence is not GA verification.
