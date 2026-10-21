# Parse numeric cells while retaining the consumed boundary

## Goal

Read semicolon-delimited numeric values from character or UTF-8 spans while rejecting malformed prefixes and preserving the next-field offset.

Use an SDK console project, implicit System imports and namespace Playbook.Samples. Import System.Globalization and System.Numerics. Older examples target net10.0 / C# 14; newer examples target net11.0 / C# 15. The partial class can be used on its own for this goal.

## Older approach

A fictional instrument export uses invariant numbers separated by semicolons, without quoting. Find the delimiter and parse the bounded span. This already avoids substring allocation and remains a clear .NET 10 solution. The returned offset excludes the delimiter, so the caller can validate or advance it.

{{snippet:older}}

## Newer approach

TryParsePartial reports how far numeric parsing progressed. Accept the value only if the next input element is a semicolon or the input ended, and reject nonfinite values. `TryReadCell<decimal>`(" -14.25;tail", out value, out consumed) gives -14.25 and offset 7. The UTF-8 form parses "6.25e1;tail" as 62.5 with six bytes consumed. Use only the output value/offset on true.

{{snippet:newer}}

## Migration

1. Declare the numeric grammar and culture. Here NumberStyles.Float permits signs, decimal point, exponent and whitespace, with invariant decimal point and no thousands separators.
2. Use the appropriate character or UTF-8 helper for the input encoding; keep consumed offsets in those units.
3. On true, process the number; if consumed is less than input length, skip the known one-element semicolon before parsing the next field.
4. On false, report or reject the entire cell. Do not accept a numeric prefix followed by arbitrary junk, and do not continue a loop without progress.

## Constraints

NumberStyles.Float intentionally excludes grouped thousands and culture-specific comma decimals. Check the delimiter yourself: TryParsePartial alone may accept a valid prefix in malformed text. Both helpers reject NaN/infinity and overflow-to-infinity for floating types. Failed output values are not usable. The sample constrains T to a value type implementing `INumberBase<T>` and executes int, decimal and double. The interface's default partial implementation falls back to full TryParse; a custom type need not stop at a delimiter, so verify its override before relying on this optimization. Invalid style combinations may throw ArgumentException rather than return false; this helper fixes the style instead of accepting arbitrary caller flags. This is neither quoted CSV nor an incremental decoder for incomplete UTF-8 buffers.

The newer route uses framework-provided managed APIs on .NET 11; no additional package/assets, workload, provider, UI rendering context or experimental opt-in is required. Language mode 15.0 and SDK 11 are the sample configuration, not a claim that this algorithm requires a new language construct. The older comparison is executed on .NET 10, not advertised as a route to the new API. No older-target package route is established.

## Evidence

Both targets execute character/UTF-8 paths, final fields, signs/exponents, integer overflow, and rejection of empty fields, incomplete exponents, junk suffixes, comma formats, NaN/infinity and excessive exponents.

Canonical samples are samples/net10.0/NumericGuidance and samples/net11.0/NumericGuidance with shared NumericGuidanceChecks.cs. Saved evidence records compile-and-run with SDK 10.0.402 / C# 14 and SDK 11.0.100-rc.2.26504.105 / C# 15, on runtimes .NET 10.0.13 and .NET 11.0.0-rc.2.26504.105. Their exact inputs, commands and outputs accompany this recipe. RC2 evidence is not GA proof; no mocks, native devices, providers, AOT or general performance guarantees are claimed.
