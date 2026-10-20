# Choose a character comparison policy explicitly

## Goal

A beacon importer compares machine markers ordinally but allows culture-aware comparisons in a human-facing filter. Keep these policies explicit at the comparison site.

## Older approach

Create a net10.0 / C# 14 console project. Use System.Text, System.Buffers, System.Text.Unicode and System.Text.RegularExpressions; omit imports for namespaces not used by the selected snippet. All snippets belong inside a static class in namespace Playbook.Samples; System, System.IO, System.Linq, System.Collections.Generic and System.Threading.Tasks come from implicit usings.

The older version adapts each character/scalar into a string and uses String.Equals with the same StringComparison. It is a correct alternative; the newer overload removes the temporary adaptation.

{{snippet:older}}

## Newer approach

Create the corresponding net11.0 / C# 15 project, with the same imports and implicit usings.

Use char.Equals or Rune.Equals with StringComparison. The Turkish test deliberately distinguishes i/I under ordinal ignore-case from i/İ under Turkish current-culture ignore-case, and checks a supplementary Deseret scalar pair.

{{snippet:newer}}

## Migration

1. Retain the old StringComparison value when removing string conversions. Test language-sensitive pairs and supplementary scalars. For stable identifiers prefer an explicitly designed ordinal policy rather than the ambient culture.
2. Preserve the observable result and error policy; compile with the actual declared target and dependencies. A newer installed SDK does not establish an older runtime route.
3. Run the focused checks described below before adopting the replacement.

## Constraints

A char can be only half of a surrogate pair; choose Rune when supplementary characters are possible. Culture-sensitive collation is not a universal identifier/security policy and can vary with ICU/NLS/platform versions. The recorded Turkish assertions are Windows globalization execution, not proof of every platform or every collation rule. Ordinal casing (ToUpperOrdinal/ToLowerOrdinal on char/string/Rune/spans) is separate from display casing or Unicode normalization; do not replace a culture-aware transform merely because both appear to uppercase ASCII.

This edition targets .NET 11. It has no rendering mode, external provider, optional workload or experimental opt-in. It uses framework APIs and needs no additional package or package asset. The sample SDK pins 10.0.402 and 11.0.100-rc.2.26504.105 are reproducibility baselines, not mandatory exact consumer SDK installations. RC2 execution is not GA verification. No .NET 10 catalogue edition or conditional older-target package route is advertised.

## Evidence

The original UnicodeText projects compile and run against their declared .NET 10/11 SDKs. Checks cover supplementary replacement, malformed-input position/replacement flags, Turkish comparison, invalid UTF byte/code-unit offsets, scalar search/trim/split, ordinal casing and mixed Unicode newlines including Singleline interaction.

Saved evidence includes exact input hashes, source commit/dirty marker, SDK/runtime host, commands, dependencies and output. No mocks or cross-compilation substitute for execution. Consult the embedded records for what actually ran; no complete-catalogue, native-deployment or product-publication readiness is implied.
