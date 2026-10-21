# Round-trip floating values and raw BFloat16 payloads

## Goal

Persist finite floating-point values in readable hexadecimal form, or use explicit-endian raw bits when signed zeros and NaN payloads must survive unchanged.

Use an SDK console project, implicit System imports and namespace Playbook.Samples. Import System.Buffers.Binary, System.Globalization and System.Numerics. Older examples target net10.0 / C# 14; newer examples target net11.0 / C# 15. The partial class can be used on its own for this goal.

## Older approach

A fictional calibration archive stores a double's raw 64-bit pattern as exactly sixteen hex digits. This valid .NET 10 representation preserves every bit, including NaN payloads, but is an integer bit encoding rather than a hexadecimal floating-point numeral. BFloat16 has no framework equivalent on .NET 10; do not relabel Half's different encoding as BFloat16.

{{snippet:older}}

## Newer approach

For readable numerical hex, format double with X and parse with NumberStyles.HexFloat plus invariant culture. A calibration value of 19.375 becomes 0X1.36P+4; signed zero becomes -0X0P+0. The same format/parsing family is exercised for float and Half. For BFloat16 transport, the separate raw helpers write/read exactly two big-endian bytes, bypassing arithmetic and textual normalization.

{{snippet:newer}}

## Migration

1. Decide whether the contract is a numeric value or a raw bit pattern. Version any changed persisted format; old sixteen-digit bit text is not the new hex-float grammar.
2. For finite numerical interchange, use X plus HexFloat with invariant culture and check parsing success.
3. For NaN payload identity, use raw bits; BFloat16 helpers explicitly choose big endian and require exactly two bytes.
4. Preserve encoding/byte-order metadata with persisted data. Test signed zero, subnormal/maximum finite values and malformed input.

## Constraints

Do not promise that textual NaN preserves sign or payload. The actual RC2 run changes NaN bits 7FF8000000000037 to FFF8000000000000 through X text while remaining NaN. Raw BFloat16 encoding checks finite 3F88, negative zero 8000, infinity 7F80 and NaN payload 7FC5 unchanged. BitConverter.GetBytes/ToBFloat16 use native byte order; their same-host round trip is not a cross-platform wire contract. The explicit BinaryPrimitives route is portable by construction. Short/extra raw payloads are rejected by the helper; malformed numerical text returns false. Keep existing raw double encoding when every payload bit matters. BFloat16 precision selection and general arithmetic remain a separate goal.

The newer route uses framework-provided managed APIs on .NET 11; no additional package/assets, workload, provider, UI rendering context or experimental opt-in is required. Language mode 15.0 and SDK 11 are the sample configuration, not a claim that this algorithm requires a new language construct. The older comparison is executed on .NET 10, not advertised as a route to the new API. No older-target package route is established.

## Evidence

Real older/newer runs compare double bit patterns for 19.375, negative zero, smallest subnormal, maximum finite and a negative fraction. Newer execution also checks float/Half hex, raw BFloat16 payloads, byte order, native BitConverter pairing, short buffers and observed NaN normalization.

Canonical samples are samples/net10.0/NumericGuidance and samples/net11.0/NumericGuidance with shared NumericGuidanceChecks.cs. Saved evidence records compile-and-run with SDK 10.0.402 / C# 14 and SDK 11.0.100-rc.2.26504.105 / C# 15, on runtimes .NET 10.0.13 and .NET 11.0.0-rc.2.26504.105. Their exact inputs, commands and outputs accompany this recipe. RC2 evidence is not GA proof; no mocks, native devices, providers, AOT or general performance guarantees are claimed.
