# Choose decimal precision and special-value behavior

## Goal

Choose System.Decimal or IEEE decimal floating point when accumulating base-ten measurements with an explicit precision and nonfinite-value policy.

Use an SDK console project, implicit System imports and namespace Playbook.Samples. Import System.Numerics. Older examples target net10.0 / C# 14; newer examples target net11.0 / C# 15. The partial class can be used on its own for this goal.

## Older approach

A fictional weighbridge totals delivery measurements with System.Decimal. This remains appropriate for finite base-ten arithmetic within its roughly 28–29 significant digits and scale 0–28. It throws on decimal division by zero instead of carrying infinity. .NET 10 has no framework Decimal32/64/128 equivalent; this comparison does not emulate IEEE decimal behavior.

{{snippet:older}}

## Newer approach

Use the same accumulation algorithm with a deliberately selected IEEE type. Call `Accumulate<Decimal32>`([(Decimal32)10_000_000, (Decimal32)1]) and the result is 10_000_000: seven significant digits cannot retain the increment. Decimal64 retains 10_000_001. Decimal128 retains a unit added to 10^32. For decimal fractions, parse source text with Decimal64.Parse("0.07", CultureInfo.InvariantCulture), or convert exact integers; do not route a base-ten source through double.

{{snippet:newer}}

## Migration

1. Keep System.Decimal when its finite range, precision and existing persistence contracts meet the goal.
2. For IEEE special values or a different precision/range, select Decimal32, Decimal64 or Decimal128; instantiate the generic helper with that type.
3. Parse base-ten text directly into the chosen type with an explicit culture. Treat conversion to a narrower representation as a rounding boundary.
4. Decide whether NaN/infinity are valid measurements; reject or carry them intentionally, and update storage/serialization contracts separately.

## Constraints

Decimal32/64/128 are 32/64/128-bit IEEE decimal formats with 7/16/34 significant decimal digits, respectively. They have different exponent ranges from System.Decimal and from one another; a larger storage width does not make conversion exact in every direction. Arithmetic can round at each operation. One divided by zero yields infinity, and zero divided by zero yields NaN; System.Decimal has neither. This helper permits them and is not a validated financial accumulator. The new types participate in generic math, including `IFloatingPointIeee754<T>`; this example selects decimal types rather than assuming that interface means binary. No older-target package route or storage-format equivalence is established.

The newer route uses framework-provided managed APIs on .NET 11; no additional package/assets, workload, provider, UI rendering context or experimental opt-in is required. Language mode 15.0 and SDK 11 are the sample configuration, not a claim that this algorithm requires a new language construct. The older comparison is executed on .NET 10, not advertised as a route to the new API. No older-target package route is established.

## Evidence

Real executions check a lost Decimal32 increment, retained Decimal64 and Decimal128 increments, 0.07 + 0.13, IEEE infinity/NaN and older decimal division failure.

Canonical samples are samples/net10.0/NumericGuidance and samples/net11.0/NumericGuidance with shared NumericGuidanceChecks.cs. Saved evidence records compile-and-run with SDK 10.0.402 / C# 14 and SDK 11.0.100-rc.2.26504.105 / C# 15, on runtimes .NET 10.0.13 and .NET 11.0.0-rc.2.26504.105. Their exact inputs, commands and outputs accompany this recipe. RC2 evidence is not GA proof; no mocks, native devices, providers, AOT or general performance guarantees are claimed.
