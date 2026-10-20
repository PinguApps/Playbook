# Match mixed newline conventions deliberately

## Goal

A beacon log combines records from several platforms. Its line-matching pattern should recognize each separator without leaving a stray carriage return in a record.

## Older approach

Create a net10.0 / C# 14 console project. Use System.Text, System.Buffers, System.Text.Unicode and System.Text.RegularExpressions; omit imports for namespaces not used by the selected snippet. All snippets belong inside a static class in namespace Playbook.Samples; System, System.IO, System.Linq, System.Collections.Generic and System.Threading.Tasks come from implicit usings.

The older version normalizes CRLF first, then CR/NEL/line-separator/paragraph-separator to LF and uses ordinary multiline matching. This remains useful when downstream consumers also require normalized content, but changes source offsets.

{{snippet:older}}

## Newer approach

Create the corresponding net11.0 / C# 15 project, with the same imports and implicit usings.

Apply AnyNewLine together with Multiline to ^.+$. The newer version recognizes mixed line endings directly without first rewriting the source. The example skips empty records deliberately because it uses + rather than *.

{{snippet:newer}}

## Migration

1. Decide whether to normalize the data or just change matching. Add AnyNewLine explicitly where its anchor/dot semantics match the input contract, then test CRLF as one boundary and each Unicode separator.
2. Preserve the observable result and error policy; compile with the actual declared target and dependencies. A newer installed SDK does not establish an older runtime route.
3. Run the focused checks described below before adopting the replacement.

## Constraints

AnyNewLine changes authored regex newline handling; generator/code-fixer improvements are not extra APIs or automatic fixes to existing patterns. Multiline controls anchors and Singleline controls dot: Singleline still allows dot to consume line separators. The test checks this interaction and seven mixed-ending records. Preserve timeout/security policy for untrusted complex expressions; this small fixed pattern is not a general log parser. Normalization and direct matching can produce equal line values while retaining different source offsets. Do not promise the older normalized offsets equal the original positions.

This edition targets .NET 11. It has no rendering mode, external provider, optional workload or experimental opt-in. It uses framework APIs and needs no additional package or package asset. The sample SDK pins 10.0.402 and 11.0.100-rc.2.26504.105 are reproducibility baselines, not mandatory exact consumer SDK installations. RC2 execution is not GA verification. No .NET 10 catalogue edition or conditional older-target package route is advertised.

## Evidence

The original UnicodeText projects compile and run against their declared .NET 10/11 SDKs. Checks cover supplementary replacement, malformed-input position/replacement flags, Turkish comparison, invalid UTF byte/code-unit offsets, scalar search/trim/split, ordinal casing and mixed Unicode newlines including Singleline interaction.

Saved evidence includes exact input hashes, source commit/dirty marker, SDK/runtime host, commands, dependencies and output. No mocks or cross-compilation substitute for execution. Consult the embedded records for what actually ran; no complete-catalogue, native-deployment or product-publication readiness is implied.
