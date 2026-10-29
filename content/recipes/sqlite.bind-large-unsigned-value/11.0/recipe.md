# Bind a full-width unsigned integer without custom encoding

## Goal

MarshCounters stores accumulated sensor counters that can exceed both signed and
unsigned 64-bit ranges. Use Microsoft.Data.Sqlite 11 to pass a `UInt128` as a
parameter value. The provider writes an invariant, zero-padded 39-digit decimal
TEXT value. With consistent encoding, TEXT affinity and BINARY ordering, textual
comparison follows unsigned numeric order all the way to `UInt128.MaxValue`.

## Constraints

This is direct `Microsoft.Data.Sqlite` binding, not EF property mapping. Provider11
exposes `Microsoft.Data.Sqlite.Core` lib/net10.0, so both the primary net11.0
consumer and the optional newer-package net10.0 consumer below are verified.
The older comparison uses provider10.0.13's lib/net8.0 on net10.0. A .NET10 app
does not need EF11 or the .NET11 runtime to take the optional direct-provider route.
This retained 11.0 edition adds a conditional package route, not a ninth foundation.

Use a `TEXT COLLATE BINARY` column and the same padded representation for stored
values and comparison parameters. INTEGER/NUMERIC affinity may convert large
decimal strings to REAL and lose precision; REAL is not a full-width UInt128 store.
Unpadded or mixed existing text does not have this ordering guarantee. Review and
migrate existing data separately before switching comparison parameters. This
sample does not migrate production rows, perform UInt128 SQL arithmetic or handle
arbitrary externally written values.

**The data reader does not support UInt128 retrieval.** Read `GetString`, then
explicitly parse that string in application code if a number is needed. This is
not a provider `GetUInt128`/`GetFieldValue<UInt128>` round-trip claim. Do not cast
through Int64, UInt64 or double. Parameter binding remains parameterized; do not
interpolate the number into SQL.

The tested old SDK is 10.0.402/C#14; current net11.0 is SDK
11.0.100-rc.2.26504.105/C#15. The conditional net10.0/current-package project uses
SDK10.0.402/C#14. No C#15-only syntax or experimental opt-in is required.
All current package pins are 11.0.0-rc.2.26504.105; these are RC2 results, not GA
verification. Native Windows x64 SQLite is 3.53.3 with provider10 and 3.53.4 with
provider11. Other native OS/architecture routes need suitable assets and their own
checks. No web framework, renderer, workload, account or external database is used.

## Older approach

Provider10 does not bind a UInt128 directly; the control verifies that rejection.
Explicit invariant `D39` formatting is a valid older solution and preserves the
same storage contract. This full .NET10 project measures its stored representation,
ordering and range comparison rather than substituting a narrower integer.

#### global.json

{{snippet:n10-global-json}}

#### Directory.Build.props

{{snippet:n10-directory-build-props}}

#### Directory.Build.targets

{{snippet:n10-directory-build-targets}}

#### Directory.Packages.props

{{snippet:n10-directory-packages-props}}

#### MarshCounters.csproj

{{snippet:n10-marshcounters-csproj}}

#### Program.cs

{{snippet:n10-program-cs}}

## Newer approach

Pass the `UInt128` directly to `AddWithValue`. Provider11 owns the same formatting;
the schema and expected ordering are unchanged. All six files form a standalone
net11.0 project. The current sample includes zero, 9/10, 99/100, values beyond
Int64 and UInt64, and both maximum UInt128 values.

#### global.json

{{snippet:n11-global-json}}

#### Directory.Build.props

{{snippet:n11-directory-build-props}}

#### Directory.Build.targets

{{snippet:n11-directory-build-targets}}

#### Directory.Packages.props

{{snippet:n11-directory-packages-props}}

#### MarshCounters.csproj

{{snippet:n11-marshcounters-csproj}}

#### Program.cs

{{snippet:n11-program-cs}}

## Optional .NET10 target with provider11

This complete standalone project uses the same direct binding, actual provider11
net10.0 asset and native bundle on .NET10. It compiled and ran with SDK10.0.402.
It is an optional dependency upgrade, not evidence that EF Core11 runs on .NET10.

#### global.json

{{snippet:n10-current-global-json}}

#### Directory.Build.props

{{snippet:n10-current-directory-build-props}}

#### Directory.Build.targets

{{snippet:n10-current-directory-build-targets}}

#### Directory.Packages.props

{{snippet:n10-current-directory-packages-props}}

#### MarshCounters11.csproj

{{snippet:n10-current-marshcounters11-csproj}}

#### Program.cs

{{snippet:n10-current-program-cs}}

## Migration and running

1. Inspect the declared column affinity, collation and existing representation.
   Retain TEXT/BINARY and uniformly padded decimal values if relying on ordering.
   Schema/data conversion for another encoding needs a separate reviewed migration.
2. Choose the primary .NET11 project or conditional .NET10/provider11 project.
   Use the complete CPM and project files above; the current dependency and native
   package graph is distinct from provider10 and remains author-maintained.
3. Replace application `ToString("D39", CultureInfo.InvariantCulture)` binding
   with the UInt128 value itself. Update range parameters too. Keep string reading
   and explicit parsing, because provider reader UInt128 retrieval is unsupported.
4. Put the selected project's six files in one standalone directory. With its SDK
   installed, run `dotnet run --configuration Release`; initial dependency restore
   needs nuget.org. The sample opens only a transient in-memory database, removed
   when its connection is disposed.
5. Expect nine ascending 39-character lines, ending in
   `340282366920938463463374607431768211455`, then `PASS: UInt128 D39 TEXT storage`.
   SQL `typeof`/`length` and exact expected text assertions must pass for every row;
   the parameterized range at UInt64.MaxValue+1 must return three rows.

## Evidence

All three actual SDK/target/provider graphs compiled and ran against real SQLite
on Windows x64. Each sample's saved evidence records input hashes, exact resolved
locked dependencies, commands, runtime/native version and observed PASS output.
The old explicit-format and new direct-binding consumers check identical storage
and ordering. No mock, metadata-only or reader UInt128 claim substitutes for this
execution. Installed stdio inspection separately checks the complete canonical
body, 11.0 edition and route metadata offline. Actual GA and other native
platforms remain unverified; no Human or external-account prerequisite is missing.

## Official references

- [Official EF11 release guidance](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-11.0/whatsnew#uint128-support).
- [Immutable reviewed documentation baseline](https://github.com/dotnet/EntityFramework.Docs/blob/aa051c4c1cad21b2dc03fa401ac0e925c3644606/entity-framework/core/what-is-new/ef-core-11.0/whatsnew.md).
- [SQLite storage classes and column affinity](https://learn.microsoft.com/en-us/dotnet/standard/data/sqlite/types).
