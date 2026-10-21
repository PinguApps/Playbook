# Generate bounded random values for generic numeric calculations

## Goal

Generate simulation integers and binary fractions for supported numeric types, with explicit range bounds and a separate cryptographic alternative.

Use an SDK console project, implicit System imports and namespace Playbook.Samples. Import System.Numerics and System.Security.Cryptography. Older examples target net10.0 / C# 14; newer examples target net11.0 / C# 15. The partial class can be used on its own for this goal.

## Older approach

A fictional depot simulation chooses an integer bay and a fractional load using Random.Next and NextDouble. Type-specific methods are concise and remain valid .NET 10 choices. They do not directly provide the new generic integer/binary-float surface; wrapping or converting a double for every destination would introduce additional precision and endpoint decisions.

{{snippet:older}}

## Newer approach

`PickBay<T>` delegates the interval to `NextInteger<T>`(minimum, maximum). `UnitFraction<T>` asks the numeric implementation for a binary fraction directly rather than narrowing a double that could round to one. Call PickBay(random, (byte)3, (byte)13), PickBay(random, Int128.MinValue, Int128.MaxValue) or `UnitFraction<Half>`(random). SecurityIndex deliberately uses RandomNumberGenerator.GetInt32 instead of either simulation helper.

{{snippet:newer}}

## Migration

1. Keep the type-specific older methods when they meet the calculation's needs; introduce generics where the caller must select the numeric type.
2. Constrain bounded integers to `IBinaryInteger<T>`, and binary fractions to `IBinaryFloatingPointIeee754<T>`. IEEE decimal types and System.Decimal do not meet the binary-float constraint.
3. Treat integer lower bounds as inclusive and upper bounds as exclusive. Equal bounds return that bound; reversed bounds throw.
4. Use RandomNumberGenerator.GetInt32 for a security-sensitive bounded integer and its byte-generation APIs for secret bytes. Do not use a seeded Random for credentials, tokens or keys.

## Constraints

The parameterless `NextInteger<T>()` additionally requires `IMinMaxValue<T>` and includes T.MaxValue in its nonnegative range; it is not identical to Random.Next()'s exclusive int.MaxValue bound. The single-upper-bound overload returns zero for maxValue zero and throws for a negative maximum. Bounded integer overloads also support sufficiently conforming custom types; the implementation assumes two's-complement-like representability. `NextBinaryFloat<T>()` returns values in [0,1), including for Half/BFloat16, without implying a continuous distribution or guaranteed coverage of every representable value. Random is noncryptographic; seeds make a bounded test repeatable on a baseline, not a portable sequence contract across releases or types. Checks demonstrate range behavior, not statistical randomness or cryptographic quality.

The newer route uses framework-provided managed APIs on .NET 11; no additional package/assets, workload, provider, UI rendering context or experimental opt-in is required. Language mode 15.0 and SDK 11 are the sample configuration, not a claim that this algorithm requires a new language construct. The older comparison is executed on .NET 10, not advertised as a route to the new API. No older-target package route is established.

## Evidence

Both targets execute 4,096 simulation iterations, equal/reversed-bound checks and the real cryptographic bounded alternative. RC2 additionally exercises byte and full-width signed Int128 bounds, Half/BFloat16 fractions, double, zero/negative maxima and nonnegative parameterless integers.

Canonical samples are samples/net10.0/NumericGuidance and samples/net11.0/NumericGuidance with shared NumericGuidanceChecks.cs. Saved evidence records compile-and-run with SDK 10.0.402 / C# 14 and SDK 11.0.100-rc.2.26504.105 / C# 15, on runtimes .NET 10.0.13 and .NET 11.0.0-rc.2.26504.105. Their exact inputs, commands and outputs accompany this recipe. RC2 evidence is not GA proof; no mocks, native devices, providers, AOT or general performance guarantees are claimed.
