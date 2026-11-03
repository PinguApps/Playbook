# Choose a contract for IEEE-decimal function results

## Goal

Use this when calculated measurements survive a runtime upgrade and corrected numerical algorithms can invalidate stored expected outputs.

## Older approach

.NET 10 has no framework Decimal32/64/128 transcendental equivalent. A Math/double calculation can serve the same application goal with a different binary representation; it does not emulate older IEEE-decimal results. System.Decimal has not acquired these functions.

{{snippet:older}}

## Newer approach

Use framework Decimal32/64/128 on .NET 11, with explicit finite-value and comparison policy. Parse decimal source text directly using invariant culture. These functions are **not guaranteed correctly rounded**; neither an exact output string nor a universal epsilon is an accuracy contract.

{{snippet:newer}}

## Migration

1. Preserve exact input identity separately from numeric comparison. Store source text/units, operation, representation and algorithm version. Numeric equality deliberately equates signed zero; bit identity or source spelling is a separate requirement.
2. Decide whether to preserve a historical output or recompute it. Historical output remains valid as a historical record even when a newer algorithm produces a different value.
3. For the bounded observatory near-one Acos case choose absolute tolerance 1e-49 and relative tolerance 1e-32, accepting error at most max(absolute, relative * max(abs(actual), abs(expected))). These values belong only to this Decimal128 case. The .NET 10 binary example instead uses 1e-15/1e-12.
4. Reject NaN and infinity at this finite-measurement boundary. Validate tolerances as finite and nonnegative. An unrepresentable tolerance bound fails closed. This helper is intended for bounded near-unit inputs/results; choosing a different comparison algorithm for extreme ranges needs a separate contract.
5. On disposable data propose a new version beside the unchanged original, validate against independently justified mathematical bounds, and deliberately reject a stale expected result and unacceptable drift before committing the proposal.
6. Promote the new version only by an explicit application migration decision. Roll back by selecting the unchanged historical version. Never silently rewrite production records or use approximate matching for exact identifiers.

## Constraints

For d=1e-34, acos(1-d)=sqrt(2d)*(1+d/12+O(d^2)). The leading term is 1.414213562373095048801688724209698e-17; the omitted correction is below 2e-52. The rounded 34-digit sqrt(2) constant and this bound independently justify the selected absolute tolerance. This is a bounded mathematical reference, not an assertion that RC2 is always correctly rounded.

The same actual consumer executed with RC1 and RC2 observes Acos(1-1e-34) change from 1.414250203437893161687265742266675e-17 to 1.414213562373095048801688724209698e-17. Log changes from -1.000051818962061097404402609041826e-34 to -1e-34; SinPi changes from 3.141755447660321030278879777191235e-34 to 3.141592653589793238462643383279503e-34. No new signature or switch restoring the previous algorithm is established.

## Evidence

Current route: net11.0 framework, SDK 11 and C# 15 sample mode; import System.Numerics. Framework-managed functions need no extra package, provider, workload, native platform or rendering context and no experimental opt-in. Older comparison: independent net10.0 SDK 10/C# 14 consumer using Math/double. Exact SDK/runtime pins identify measured artifacts rather than consumer minimums.

Canonical projects are samples/net10.0/ObservatoryResults and samples/net11.0/ObservatoryResults. Both compile/run through Record-Evidence.ps1. Current checks include cancellation-sensitive Acos, alternate 0.5/0.5000 inputs, ordinary Decimal32/64 controls, explicit-base Log, signed zero/nonfinite policy, rejected stale expectation and versioned disposable rollback. RC1 observation runs the same compiled consumer with an explicitly selected actual RC1 runtime, not a separate RC1 compilation claim. RC1 download hash, runtime identities and comparison output accompany the release evidence. GA reconciliation remains separate.
