# Construct record values with positional or named arguments (preview)

## Goal

Use preview record constructor syntax when an F# record call benefits from positional arguments or reordered named arguments; retain record expressions when preview is unsuitable.

## Older approach

Record expressions construct immutable values directly and allow any field order. They remain fully valid and avoid a preview language requirement.

{{snippet:older}}

## Newer approach

The compiler exposes the synthesized all-fields constructor to F# calls. The compact crate uses declaration-order positional arguments; spacious names its fields in a different order. Both contain the same intended values as the older record expressions. This syntax is experimental: RC2 still requires LangVersion=preview.

{{snippet:newer}}

## Migration

1. Use the .NET 11 SDK, target net11.0 and opt in with LangVersion=preview. Reference FSharp.Core 11; the sample pins RC2 for reproducibility.
2. Replace selected record expressions with the type name and all constructor arguments. Use names when positional order would obscure meaning.
3. Retain ordinary record expressions if preview is unacceptable. Recompile and compare every field before adopting the new spelling.

## Constraints

- The constructor follows record representation accessibility; it does not bypass a private representation or add business validation.
- Supply all fields. Positional arguments follow declaration order; record expressions remain clearer for many-field models.

No provider, workload or rendering mode is required. The declared samples run on Windows with .NET 10 and .NET 11 RC2; other .NET-supported platforms require their normal runtime and globalization support. No older-target SDK/package route is advertised.

## Evidence

The exact SDK pins, resolved FSharp.Core dependencies, input hashes, build commands and actual execution output accompany this recipe. The saved records cover the .NET 10 and .NET 11 samples. The separate compiler probe confirms stable spreads, preview constructor rejection/acceptance and ordinary culture formatting. Reflection-free diagnostics use their own --reflectionfree executable; ordinary formatting is not substituted for that evidence. No trimmed or Native AOT publish/run, cross-compilation or mocks are claimed. RC2 evidence is not GA verification.

Official sources: [F# record constructor preview gate](https://github.com/dotnet/core/blob/main/release-notes/11.0/preview/rc1/fsharp.md); [Synthesized all-fields constructor implementation](https://github.com/dotnet/fsharp/pull/19974).
