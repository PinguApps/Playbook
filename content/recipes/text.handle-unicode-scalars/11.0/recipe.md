# Transform text without splitting Unicode scalars

## Goal

A beacon label uses a supplementary-plane marker. One marker is a Unicode scalar but occupies two UTF-16 code units; char-by-char editing can split that pair.

## Older approach

Create a net10.0 / C# 14 console project. Use System.Text, System.Buffers, System.Text.Unicode and System.Text.RegularExpressions; omit imports for namespaces not used by the selected snippet. All snippets belong inside a static class in namespace Playbook.Samples; System, System.IO, System.Linq, System.Collections.Generic and System.Threading.Tasks come from implicit usings.

The older replacement converts each valid Rune into its string representation and uses ordinal replacement. The position loop calls Rune.DecodeFromUtf16 and tracks consumed code units explicitly. Earlier .NET already supplies Rune; the new release removes adaptation work.

{{snippet:older}}

## Newer approach

Create the corresponding net11.0 / C# 15 project, with the same imports and implicit usings.

Use Rune overloads directly for replacement/search/trim/split and RunePosition.EnumerateUtf16 for scalar, StartIndex, Length and WasReplaced. The canonical position sample retains replacement information instead of silently presenting malformed source as valid.

{{snippet:newer}}

## Migration

1. Choose the scalar boundary first, then replace string conversions with matching Rune overloads. Preserve explicit comparison policy and index units. Use StringInfo when the goal requires grapheme boundaries rather than scalar boundaries.
2. Preserve the observable result and error policy; compile with the actual declared target and dependencies. A newer installed SDK does not establish an older runtime route.
3. Run the focused checks described below before adopting the replacement.

## Constraints

A Rune is a scalar, not a user-perceived grapheme: combining sequences and emoji clusters can contain several scalars. StartIndex/Length are UTF-16 code units for EnumerateUtf16; EnumerateUtf8 reports bytes. Invalid sequences are replaced during enumeration; use the validation recipe when rejection is required. An actual U+FFFD is not necessarily malformed input; WasReplaced distinguishes decoder replacement. Specify StringComparison for search; ordinal matching is appropriate for the marker here. TextInfo Rune overloads apply culture-aware casing; ordinal casing helpers have different semantics. StringBuilder/TextWriter Rune overloads avoid ToString adaptation but do not turn a UTF-16 char operation into grapheme editing.

This edition targets .NET 11. It has no rendering mode, external provider, optional workload or experimental opt-in. It uses framework APIs and needs no additional package or package asset. The sample SDK pins 10.0.402 and 11.0.100-rc.2.26504.105 are reproducibility baselines, not mandatory exact consumer SDK installations. RC2 execution is not GA verification. No .NET 10 catalogue edition or conditional older-target package route is advertised.

## Evidence

The original UnicodeText projects compile and run against their declared .NET 10/11 SDKs. Checks cover supplementary replacement, malformed-input position/replacement flags, Turkish comparison, invalid UTF byte/code-unit offsets, scalar search/trim/split, ordinal casing and mixed Unicode newlines including Singleline interaction.

Saved evidence includes exact input hashes, source commit/dirty marker, SDK/runtime host, commands, dependencies and output. No mocks or cross-compilation substitute for execution. Consult the embedded records for what actually ran; no complete-catalogue, native-deployment or product-publication readiness is implied.
