# Preserve decimal intent across numeric conversions

## Goal

Separate decimal source values from binary measurements when an upgrade changes conversion rounding, and choose an explicit rounding rule for displayed amounts.

Use an SDK console project, implicit System imports and namespace Playbook.Samples. Import System.Globalization and System.Numerics. Older examples target net10.0 / C# 14; newer examples target net11.0 / C# 15. The partial class can be used on its own for this goal.

## Older approach

A fictional import utility receives binary sensor measurements and separately reads invoice text. The same explicit casts, invariant parsing and ties-to-even calls below are valid .NET 10 APIs. On the measured older runtime a double parsed from "2.71" converts to decimal 2.71, reflecting its older significant-digit conversion. Do not mistake that result for evidence that the binary input was exactly decimal 2.71.

{{snippet:older}}

## Newer approach

Keep decimal source text in InvoiceAmount, or start with a decimal literal such as 2.71m. Use FromBinary only when the actual binary source value is the intended input: the measured conversion yields 2.709999999999999964472863212. RoundInvoice specifies two decimal places and ties to even; 8.125 becomes 8.12 and 8.135 becomes 8.14 on both targets. ApproximateCount makes the intentional loss of arbitrary-integer precision visible.

{{snippet:newer}}

## Migration

1. Classify each input as decimal text/literal, binary measurement or exact integer; avoid converting decimal source through double.
2. Retain direct invariant decimal parsing or m-suffixed literals for decimal intent; use a numeric conversion when preserving the binary source is intentional.
3. Specify rounding digits and MidpointRounding at the application boundary. Do not confuse decimal places with significant digits.
4. Re-run saved-value and bit-pattern expectations after both runtime and SDK upgrades, including compiler-folded constants; there is no old-conversion compatibility switch.

## Constraints

Decimal/binary and BigInteger-to-binary conversions in .NET 11 use one rounding of the exact source to the destination. The actual older/newer calculation converts 2^80 + 2^27 + 1 to double: older bits end in 0000, newer bits end in 0001, because the source lies just above a midpoint. Floating destinations can still lose precision. NaN/infinity cannot become System.Decimal and throw OverflowException. Compiler constant evaluation can embed changed conversion results when rebuilding with the newer SDK irrespective of target framework; the supplied runtime inputs are parsed/computed rather than folded casts. If compatibility requires a particular old representation, store that explicit representation or define a deliberate significant-digit policy; rounding to a fixed number of decimal places is not a general recreation of the old algorithm.

The newer route uses framework-provided managed APIs on .NET 11; no additional package/assets, workload, provider, UI rendering context or experimental opt-in is required. Language mode 15.0 and SDK 11 are the sample configuration, not a claim that this algorithm requires a new language construct. The older comparison is executed on .NET 10, not advertised as a route to the new API. No older-target package route is established.

## Evidence

Real target executions assert exact decimal values, both original BigInteger destination bit patterns, ties-to-even, and exceptions for NaN/infinity. This is not a full audit of every conversion direction, Math.Round migration or older-target/newer-SDK route.

Canonical samples are samples/net10.0/NumericGuidance and samples/net11.0/NumericGuidance with shared NumericGuidanceChecks.cs. Saved evidence records compile-and-run with SDK 10.0.402 / C# 14 and SDK 11.0.100-rc.2.26504.105 / C# 15, on runtimes .NET 10.0.13 and .NET 11.0.0-rc.2.26504.105. Their exact inputs, commands and outputs accompany this recipe. RC2 evidence is not GA proof; no mocks, native devices, providers, AOT or general performance guarantees are claimed.
