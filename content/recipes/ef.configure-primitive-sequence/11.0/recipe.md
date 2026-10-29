# Choose scalar or primitive collection storage explicitly

## Goal

A pottery workshop records ordered kiln temperatures, a sequence of sensor byte
codes and a binary seal. The two byte arrays have different meanings: the sensor
codes are separate values, while the seal is one binary value. Tell EF which
meaning is intended before asking the provider to create the schema.

## Choose the mapping

In EF11 use `PrimitiveCollection` for `Steps` and `SensorCodes`, and `Property`
for the binary `Seal`. EF11 fixes collection classification when configuring the
property; `Property` no longer promotes a sequence to a primitive collection at
model finalization. Ordinary primitive collections can still be discovered by
convention, so there is no need to rewrite every convention-only model.

The EF10 comparison deliberately uses `Property` for the integer sequence to
demonstrate its previous promotion behavior. It already uses `PrimitiveCollection`
to choose byte-sequence semantics, because `byte[]` also has a binary scalar
mapping. `PrimitiveCollection` is an existing API, not an EF11 invention, and is
a valid explicit choice on the older baseline too. Both samples store ordered
JSON sequences and a binary scalar. Their separate `PropertyControlContext`
checks the actual older/current classification difference without relying on a
conversion exception or a model-finalization crash.

## Constraints and evidence boundary

The current example requires .NET/SDK 11, C# 15 mode and the pinned EF11 SQLite
packages. The previous example requires .NET/SDK 10, C# 14 and EF10 10.0.13.
No optional workload, rendering mode, account or experimental opt-in is required.
The packaged SQLite native library must match the host OS/architecture. These
consumers ran on Windows x64 using SQLite 3.53.3 (EF10) and 3.53.4 (EF11), with
SQLitePCLRaw.bundle_e_sqlite3 3.0.5 and the complete resolved lock-file graph.
EF11 selects net11.0 EF/Relational assets and net10.0 Microsoft.Data.Sqlite.Core;
the latter asset does not make EF11 itself compatible with .NET 10.

Compilation and execution used SDK 10.0.402 and 11.0.100-rc.2.26504.105 with EF
10.0.13 and 11.0.0-rc.2.26504.105. Saved evidence records identify exact SDKs,
resolved dependencies, input hashes and executed commands. These are actual
SQLite checks, not mocks, cross-compilation or GA proof. Server installation and
offline guidance retrieval establish delivery separately from database behavior.

## Complete original consumers

For each version, create the named files in its own directory. The empty local
build files intentionally prevent inherited repository settings from changing the
standalone example. Do not copy the two versions into the same directory.

## Older approach

Use this complete .NET 10 project with its pinned EF10 provider. It solves the same storage goal on the previous release.

### .NET 10

#### global.json

{{snippet:n10-global-json}}

#### Directory.Build.props

{{snippet:n10-directory-build-props}}

#### Directory.Build.targets

{{snippet:n10-directory-build-targets}}

#### Directory.Packages.props

{{snippet:n10-directory-packages-props}}

#### KilnReadings.csproj

{{snippet:n10-kilnreadings-csproj}}

#### Program.cs

{{snippet:n10-program-cs}}


## Newer approach

Use this complete .NET 11 project with the pinned RC2 provider. The model assertions and actual database round-trips check the intended mapping.

### .NET 11

#### global.json

{{snippet:n11-global-json}}

#### Directory.Build.props

{{snippet:n11-directory-build-props}}

#### Directory.Build.targets

{{snippet:n11-directory-build-targets}}

#### Directory.Packages.props

{{snippet:n11-directory-packages-props}}

#### KilnReadings.csproj

{{snippet:n11-kilnreadings-csproj}}

#### Program.cs

{{snippet:n11-program-cs}}

## Migration

1. Choose the intended scalar/collection or nested complex mapping before editing configuration.
2. Update the relevant fluent calls as described below, retaining the existing column/data intent.
3. Build and run the complete sample on its declared target, then inspect the model and any production migration before applying it.

Find explicit `Property` calls that intended primitive-collection behavior and
replace those calls with `PrimitiveCollection`. Keep genuinely scalar properties
and intentional value converters as scalar mappings. Inspect the resulting model
and generated migration before applying it. Switching an existing byte-array
column from BLOB to JSON text changes its data representation: changing the fluent
call alone does not convert persisted bytes or provide a safe data migration.
Plan and verify that conversion against your actual provider and stored values.

Run either complete project in its own directory with its pinned SDK using
`dotnet run --configuration Release`. Expect `PASS: collection/scalar metadata`
after real SQLite schema creation and writes. The checks require integer and byte
element metadata, JSON array/TEXT sequence storage versus BLOB scalar storage,
fresh-context order/duplicate preservation and persisted collection mutations.
`SensorCodes[1]` changes from 0 to 19; the scalar seal is replaced as a whole.
This does not claim in-place mutation tracking for arbitrary scalar byte arrays.

The two local CA1819 suppressions document why these EF sample properties
intentionally expose arrays. They are not EF experimental opt-ins. Collection
query translation, custom converters, complex collections and other providers'
array/JSON behavior remain separate concerns.

## Evidence

Both canonical projects compiled and ran against actual SQLite with locked dependencies. Their verification-evidence.json records pin source/configuration hashes, SDKs, resolved packages, host and captured PASS output. Installed stdio list/get checks separately verify complete canonical Markdown and applicability. These Windows x64 RC2 results establish neither other-provider behavior nor GA readiness.

## Official references

- [EF11 authoring change](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-11.0/breaking-changes#property-no-longer-configures-primitive-collections).
- [Immutable reviewed documentation](https://github.com/dotnet/EntityFramework.Docs/blob/aa051c4c1cad21b2dc03fa401ac0e925c3644606/entity-framework/core/what-is-new/ef-core-11.0/breaking-changes.md).
