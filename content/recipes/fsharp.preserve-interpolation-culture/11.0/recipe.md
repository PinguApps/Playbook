# Keep an explicit display culture in F# diagnostic messages

## Goal

Format dates and numbers with an explicit culture before interpolating them when diagnostic messages must retain a chosen region across F# 10 and F# 11.

## Older approach

Add `open System` and `open System.Globalization` before the definitions in both versions.

Format each value with the intended CultureInfo before constructing the string. This already works on .NET 10 and avoids relying on a mutable thread culture. The older example deliberately makes that dependency explicit.

{{snippet:older}}

## Newer approach

F# 11 string-typed interpolation uses invariant formatting for ordinary holes. Passing a culture directly to each value formatter preserves the chosen date and decimal conventions. The sample returns UK or German text even when CurrentCulture differs. The .NET 10 approach remains valid; the migration prevents implicit regional formatting assumptions from changing output.

{{snippet:newer}}

## Migration

1. Target net11.0 with LangVersion 11.0. Keep access to the intended globalization data; do not enable invariant globalization for regional output.
2. Pass CultureInfo explicitly and format date and decimal values with ToString(format, culture) before their text enters the interpolated message.
3. Check at least two cultures while CurrentCulture differs. Prefer invariant output deliberately for machine-readable identifiers or protocols.

## Constraints

- Ordinary and .NET-formatted interpolation holes use the new lowering; printf-format holes such as %A{value} retain the printf path and must not be used to claim reflection-free behavior.
- Explicit scalar formatting does not give arbitrary nested objects the requested culture. Format their members yourself or provide an appropriate formatter.

No provider, workload or rendering mode is required. The declared samples run on Windows with .NET 10 and .NET 11 RC2; other .NET-supported platforms require their normal runtime and globalization support. No older-target SDK/package route is advertised.

## Evidence

The exact SDK pins, resolved FSharp.Core dependencies, input hashes, build commands and actual execution output accompany this recipe. The saved records cover the .NET 10 and .NET 11 samples. The separate compiler probe confirms stable spreads, preview constructor rejection/acceptance and ordinary culture formatting. Reflection-free diagnostics use their own --reflectionfree executable; ordinary formatting is not substituted for that evidence. No trimmed or Native AOT publish/run, cross-compilation or mocks are claimed. RC2 evidence is not GA verification.

Official sources: [F# 11 interpolation lowering and invariant culture](https://github.com/dotnet/core/blob/main/release-notes/11.0/preview/rc1/fsharp.md); [Culture-aware formatting contract](https://learn.microsoft.com/en-us/dotnet/standard/base-types/formatting-types).
