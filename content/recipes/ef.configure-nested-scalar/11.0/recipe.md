# Configure a scalar inside nested complex values

## Goal

An estuary survey stores each marker's code inside its site and bearing values.
The code still needs one predictable column name, a required value and a maximum
length in the EF model. EF11 lets that configuration name the whole member path
in one lambda, instead of stepping through a builder for each complex value.

## Choose the mapping

`SurveySite` and `Bearing` are explicitly marked `[ComplexType]`. That declaration
is still necessary here: shortening the scalar configuration does not make every
object graph a complex model. The EF10 sample navigates two `ComplexProperty`
builders and configures `Bearing.Code`; the EF11 sample configures
`marker.Site.Bearing.Code` directly. Both produce the same `marker_code` column.
The older builder chain remains valid and useful when configuring several members
at one level. The direct chained expression was rejected by the measured EF10
model builder; compiling the expression alone would not have proved support.

This recipe covers non-collection complex properties. Do not infer support for
arbitrary collection elements, indexed lists, LINQ selectors, methods or entity
navigations from this member path. No JSON mapping, inheritance or index/key
configuration is needed for this example.

## Constraints and evidence boundary

The current example requires .NET/SDK 11, C# 15 mode and the pinned EF11 SQLite
packages. The previous example requires .NET/SDK 10, C# 14 and EF10 10.0.13.
No optional workload, rendering mode, account or experimental opt-in is required.
The packaged SQLite native library must match the host OS/architecture. These
consumers ran on Windows x64. EF10 used native SQLite 3.53.3 from the
SQLitePCLRaw.bundle_e_sqlite3/core/lib.e_sqlite3/provider.e_sqlite3 2.1.13 graph.
EF11 used SQLitePCLRaw.bundle_e_sqlite3/config.e_sqlite3/core/provider.e_sqlite3
3.0.5 with the separate SQLite package 3.53.4. Each complete graph is locked.
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

#### EstuaryMarkers.csproj

{{snippet:n10-estuarymarkers-csproj}}

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

#### EstuaryMarkers.csproj

{{snippet:n11-estuarymarkers-csproj}}

#### Program.cs

{{snippet:n11-program-cs}}

## Migration

1. Choose the intended scalar/collection or nested complex mapping before editing configuration.
2. Update the relevant fluent calls as described below, retaining the existing column/data intent.
3. Build and run the complete sample on its declared target, then inspect the model and any production migration before applying it.

Keep complex-type registration, column names, requiredness and other facets while
replacing the intermediate builder calls with the chained member lambda. Inspect
the resulting model and migration before applying it to an existing database;
this syntax simplification should not by itself rename the column. The sample
uses `EnsureCreated` only for a disposable in-memory database, not for production
migration management. SQLite stores the value as TEXT and does not enforce the
model's maximum length; validate the input or choose suitable database constraints
when enforcement matters. The sample checks `HasMaxLength` as model metadata.

Run either project in its own directory with its pinned SDK using
`dotnet run --configuration Release`. It must print `PASS: nested model facets`,
after checking the real schema, saving `EST-42`, reading it in a new context,
changing it to `EST-43` and reading that persisted update in another context.

## Evidence

Both canonical projects compiled and ran against actual SQLite with locked dependencies. Their verification-evidence.json records pin source/configuration hashes, SDKs, resolved packages, host and captured PASS output. Installed stdio list/get checks separately verify complete canonical Markdown and applicability. These Windows x64 RC2 results establish neither other-provider behavior nor GA readiness.

## Official references

- [EF11 authoring change](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-11.0/whatsnew#configuring-complex-type-properties-via-lambda-chaining).
- [Immutable reviewed documentation](https://github.com/dotnet/EntityFramework.Docs/blob/aa051c4c1cad21b2dc03fa401ac0e925c3644606/entity-framework/core/what-is-new/ef-core-11.0/whatsnew.md).
