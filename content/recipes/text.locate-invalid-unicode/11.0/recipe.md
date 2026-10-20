# Report the first malformed Unicode subsequence

## Goal

A beacon import reports where a malformed record begins instead of replacing it silently. Preserve the original encoding and its offset units for a useful diagnostic.

## Older approach

Create a net10.0 / C# 14 console project. Use System.Text, System.Buffers, System.Text.Unicode and System.Text.RegularExpressions; omit imports for namespaces not used by the selected snippet. All snippets belong inside a static class in namespace Playbook.Samples; System, System.IO, System.Linq, System.Collections.Generic and System.Threading.Tasks come from implicit usings.

The older decoder loops advance by each successfully consumed scalar. A non-Done status marks the first invalid or incomplete subsequence for this complete-input contract.

{{snippet:older}}

## Newer approach

Create the corresponding net11.0 / C# 15 project, with the same imports and implicit usings.

Use Utf8.IndexOfInvalidSubsequence for bytes and Utf16.IndexOfInvalidSubsequence for UTF-16 code units; -1 means valid complete input. Utf16.IsValid supplies a Boolean-only check when a position is unnecessary. Import System.Text.Unicode.

{{snippet:newer}}

## Migration

1. Validate the original bytes/chars before a lossy decoder changes them. Keep the offset’s encoding in the diagnostic and do not label it a character index. Add the record’s own starting offset only in the same units.
2. Preserve the observable result and error policy; compile with the actual declared target and dependencies. A newer installed SDK does not establish an older runtime route.
3. Run the focused checks described below before adopting the replacement.

## Constraints

A trailing partial UTF-8 sequence or unmatched high surrogate is invalid when the input is complete. For streaming input retain incomplete trailing data until the next block instead of treating every block as a complete document. These functions validate encoding, not normalization, grapheme boundaries, application allowlists or display safety. Return the offset in its original units: the test places an invalid byte at 5 after a four-byte scalar, and an unmatched surrogate at UTF-16 offset 3 after a surrogate pair.

This edition targets .NET 11. It has no rendering mode, external provider, optional workload or experimental opt-in. It uses framework APIs and needs no additional package or package asset. The sample SDK pins 10.0.402 and 11.0.100-rc.2.26504.105 are reproducibility baselines, not mandatory exact consumer SDK installations. RC2 execution is not GA verification. No .NET 10 catalogue edition or conditional older-target package route is advertised.

## Evidence

The original UnicodeText projects compile and run against their declared .NET 10/11 SDKs. Checks cover supplementary replacement, malformed-input position/replacement flags, Turkish comparison, invalid UTF byte/code-unit offsets, scalar search/trim/split, ordinal casing and mixed Unicode newlines including Singleline interaction.

Saved evidence includes exact input hashes, source commit/dirty marker, SDK/runtime host, commands, dependencies and output. No mocks or cross-compilation substitute for execution. Consult the embedded records for what actually ran; no complete-catalogue, native-deployment or product-publication readiness is implied.
