# Compose record fields without repeating declarations and copies

## Goal

Use record spreads when a new named or anonymous F# record reuses existing fields and needs explicit additions or replacements.

## Older approach

Declare every field in the destination type and copy each value. Copy-and-update remains a good choice for updating the same record type; it does not flatten several record shapes into a new type.

{{snippet:older}}

## Newer approach

F# 11 spreads flatten the Parcel fields into Dispatch. The explicit Dock value replaces the spread value without mutating dispatch. The anonymous summary also copies the same values. Spreads are stable in F# 11; this sample needs no preview mode.

{{snippet:newer}}

## Migration

1. Target net11.0 with the .NET 11 SDK and LangVersion 11.0. Reference FSharp.Core 11.0.101-rc2.26504.105 for this reproducible baseline.
2. Replace repeated destination declarations with ...Parcel and copied expressions with ...parcel. Keep explicit additions and replacements visible.
3. Check all copied fields, the resulting type and any overridden values. Preserve the original record when making a new value.

## Constraints

- Spreads copy record fields, not class inheritance or object identity.
- Keep field types compatible and resolve conflicting names explicitly. This recipe demonstrates one source and explicit overrides, not an arbitrary conflict-resolution policy.

No provider, workload or rendering mode is required. The declared samples run on Windows with .NET 10 and .NET 11 RC2; other .NET-supported platforms require their normal runtime and globalization support. No older-target SDK/package route is advertised.

## Evidence

The exact SDK pins, resolved FSharp.Core dependencies, input hashes, build commands and actual execution output accompany this recipe. The saved records cover the .NET 10 and .NET 11 samples. The separate compiler probe confirms stable spreads, preview constructor rejection/acceptance and ordinary culture formatting. Reflection-free diagnostics use their own --reflectionfree executable; ordinary formatting is not substituted for that evidence. No trimmed or Native AOT publish/run, cross-compilation or mocks are claimed. RC2 evidence is not GA verification.

Official sources: [F# 11 record spreads and stabilization](https://github.com/dotnet/core/blob/main/release-notes/11.0/preview/rc1/fsharp.md); [Record spread implementation](https://github.com/dotnet/fsharp/pull/18927).
