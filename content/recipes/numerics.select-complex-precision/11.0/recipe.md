# Choose the scalar representation for complex arithmetic

## Goal

Rotate complex measurements using a chosen floating-point representation instead of always storing real and imaginary parts as double.

Use an SDK console project, implicit System imports and namespace Playbook.Samples. Import System.Numerics. Older examples target net10.0 / C# 14; newer examples target net11.0 / C# 15. The partial class can be used on its own for this goal.

## Older approach

A fictional signal recorder rotates an observation by a quarter turn. The existing System.Numerics.Complex stores both components as double and remains a valid choice. It cannot retain float or IEEE decimal components as their own scalar representation. There is no generic `Complex<T>` framework equivalent on .NET 10.

{{snippet:older}}

## Newer approach

Multiply by the imaginary unit using the selected scalar's Zero and One. Rotate(new `Complex<float>`(4, -3)) yields real 3 and imaginary 4; the executed `Complex<Decimal64>` route gives the same exact integral result using decimal components. These deliberately exact inputs verify representation and arithmetic, not a universal error bound.

{{snippet:newer}}

## Migration

1. Keep Complex for a double-based calculation that already meets its precision budget.
2. To retain a different scalar, choose a type satisfying both generic constraints and replace construction, constants and surrounding storage consistently.
3. Use the generic rotation helper; check representative calculations and tolerances for the chosen scalar.
4. Audit nonfinite input expectations separately during a runtime upgrade, even when retaining nongeneric Complex.

## Constraints

Supported scalar families include double, float, Half, BFloat16 and IEEE Decimal32/64/128; the published constraints are authoritative for a custom type. System.Decimal lacks IEEE NaN/infinity support and cannot be substituted here. Narrowing components can round, overflow or underflow, and complex functions introduce their own error; this rotation proves no accuracy/performance claim about transcendental functions. The existing Complex implementation also adopts special-value handling through `Complex<double>` on .NET 11: the tested (3,-4)/zero case changes from two NaNs to positive/negative infinity. That runtime migration is independent of choosing generic storage; broader Annex G audit remains separate.

The newer route uses framework-provided managed APIs on .NET 11; no additional package/assets, workload, provider, UI rendering context or experimental opt-in is required. Language mode 15.0 and SDK 11 are the sample configuration, not a claim that this algorithm requires a new language construct. The older comparison is executed on .NET 10, not advertised as a route to the new API. No older-target package route is established.

## Evidence

Both real targets rotate (4,-3) to (3,4); the newer target executes float and Decimal64 components. The nongeneric zero-divisor check records the distinct runtime special-value results.

Canonical samples are samples/net10.0/NumericGuidance and samples/net11.0/NumericGuidance with shared NumericGuidanceChecks.cs. Saved evidence records compile-and-run with SDK 10.0.402 / C# 14 and SDK 11.0.100-rc.2.26504.105 / C# 15, on runtimes .NET 10.0.13 and .NET 11.0.0-rc.2.26504.105. Their exact inputs, commands and outputs accompany this recipe. RC2 evidence is not GA proof; no mocks, native devices, providers, AOT or general performance guarantees are claimed.
