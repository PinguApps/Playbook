# Preserve an embedded sequence while replacing positional ownership

## Goal

A walking-trail editor keeps an ordered list of waypoints inside each route card.
Two visits to the same gate are separate entries, even when their captions and
walking times match. Keep those values and their order when replacing EF's
implicit item identity with a complex collection.

## Choose by meaning

Use `ComplexCollection` with `ToJson` when each item is a value belonging to its
card. The current model gives the collection no owned entity identity. The older
`OwnsMany` mapping uses an EF-generated shadow key containing an ordinal: that
position helps EF track an item, but is not an application identity or a stored
JSON ID. Both implementations below store the same original array.

The EF10 approach is valid on its stated baseline. Complex JSON collections also
exist in EF10; they are not an EF11 invention. What changes in EF11 is the
diagnostic for owned JSON collections using synthetic ordinal keys. Existing
mapping continues to work on the measured RC2 release; this is a deprecation,
not an immediate removal. Owned references are a different case.

If items need durable identities independent of their position, decide that
separately. The official mitigation also mentions a non-shadow CLR key for
owned items. That changes identity semantics and may need new persisted key
values, uniqueness rules and a data backfill. This recipe demonstrates the
complex-collection route only; it establishes no compatible keyed-owned JSON
route for an arbitrary provider. Do not add a `HasKey` call to this example and
assume the provider supports it. A separately mapped child table is another
design when entity identity is essential, with its own schema migration.

## Constraints

Owned JSON collections without key; schema/data migration reviewed separately.
This is a .NET 11 edition addressing the EF11 upgrade warning. The EF10 comparison
does not add a ninth .NET10 foundation or offer EF11 on an older runtime.

The actual previous consumer uses SDK10.0.402, runtime10.0.13, C#14 mode and
EF/SQLite10.0.13. The current consumer uses SDK11.0.100-rc.2.26504.105,
runtime11RC2, C#15 mode and EF/SQLite11.0.0-rc.2.26504.105. Neither sample needs
new language syntax. Current EF/Relational assets target net11.0, while
Microsoft.Data.Sqlite.Core has a net10.0 asset; that driver asset does not make
EF11 compatible with net10.0. Lock files retain the actual resolved graphs.

The real provider check ran on Windows x64. Previous SQLite3.53.3 comes through
SQLitePCLRaw2.1.13; current SQLite3.53.4 comes through SQLitePCLRaw3.0.5 and the
separate SQLite3.53.4 package. Suitable native assets are required on any host.
There is no optional workload, browser/rendering mode, external service, account
or experimental opt-in. Stable GA-targeted API guidance is backed by RC2
execution, not GA evidence. Other providers and platforms require separate proof.

The pinned RC2 diagnostic is
`RelationalEventId.OwnedEntityMappedToJsonCollectionWarning`, in
`Microsoft.EntityFrameworkCore.Diagnostics`. The documentation's suppression
example names `CoreEventId`; that is not the compiled RC2 member used here.
The control context elevates the real event to an exception, then verifies the
complex model initializes without that event. Suppression would only hide the
warning; it would not change mapping or migrate data.

## Complete original consumers

Create a separate directory for each version, containing all the files shown.
The empty local build files isolate the projects from inherited build policies.
The repository lock files and saved evidence supply exact reproducibility;
standalone copies can restore the listed direct pin to generate their own locks.
Dependency restore is online setup; installed Playbook guidance remains offline.

## Older approach

### .NET 10 files

#### global.json

{{snippet:n10-global-json}}

#### Directory.Build.props

{{snippet:n10-directory-build-props}}

#### Directory.Build.targets

{{snippet:n10-directory-build-targets}}

#### Directory.Packages.props

{{snippet:n10-directory-packages-props}}

#### TrailCards.csproj

{{snippet:n10-trailcards-csproj}}

#### Program.cs

{{snippet:n10-program-cs}}

## Newer approach

### .NET 11 files

#### global.json

{{snippet:n11-global-json}}

#### Directory.Build.props

{{snippet:n11-directory-build-props}}

#### Directory.Build.targets

{{snippet:n11-directory-build-targets}}

#### Directory.Packages.props

{{snippet:n11-directory-packages-props}}

#### TrailCards.csproj

{{snippet:n11-trailcards-csproj}}

#### Program.cs

{{snippet:n11-program-cs}}

## Run the comparison

1. In PowerShell set `DOTNET_ADD_GLOBAL_TOOLS_TO_PATH` to `0` for each SDK child.
   Choose a **new, disposable, absolute** database filename, and set
   `PLAYBOOK_TRAIL_DATABASE` to it in the shell used for both consumers. Do not
   point either program at an existing application database.
2. In the older directory use the .NET10 executable to run `dotnet restore`,
   `dotnet build -c Release -p:RestoreLockedMode=true`, then
   `dotnet bin/Release/net10.0/TrailCards.dll`. It creates the database once,
   seeds two cards and prints the original JSON. Reseeding an existing fixture
   fails rather than resetting it.
3. In the newer directory use the .NET11 executable for the equivalent restore
   and build, then `dotnet bin/Release/net11.0/TrailCards.dll`. Run SDK commands
   from their matching directories so each `global.json` is selected. The newer
   program requires the seeded file; it does not create a replacement database.
4. Expect the original captions `Gate, Lookout, Gate` and times `4, 12, 4`.
   The fresh complex context keeps the duplicate entries distinct, reads an empty
   second list and verifies that a no-op save leaves JSON unchanged. It then
   removes the first entry, changes the remaining gate's time to 8 and appends
   Bridge/6. A fresh context must read `Lookout, Gate, Bridge` and `12, 8, 6`.
5. Back in the older directory execute
   `dotnet bin/Release/net10.0/TrailCards.dll verify-migrated`. This checks the
   changed values through the original owned mapping. Each program must report
   `PASS`; an exception or nonzero exit is failure. Restore the two environment
   variables to their prior values and delete only your disposable database file
   after the processes exit. No persistent service is started.

## Migration

1. Identify every affected owned JSON collection, including nested ones. Inventory
   its column/JSON names, converters, requiredness, indexes and stored values.
   Back up real data and test on a copy. This example covers the root JSON array,
   mapped fields, ordinary entries and empty arrays; SQL NULL, malformed JSON,
   unknown properties and other document shapes need their own policy and tests.
2. Decide whether entries are values. Remove the owned mapping and configure the
   complex collection, retaining explicit column and member names. Remove
   ownership/key configuration that belongs only to the old entity model. Do
   not keep both configurations on the same member.
3. Generate and inspect the application's migration and SQL against its existing
   snapshot before applying it. Although this fixture preserves the JSON shape
   and reads the old table without DDL, the new model generates `Waypoints TEXT
   NOT NULL`; the old table has `Waypoints TEXT NULL`. Decide how existing NULLs
   should be handled before enforcing requiredness. Never accept a generated
   drop/recreate-column operation that discards the array merely because the
   model is now complex. SQLite constraint changes can require a table rebuild.
4. Test the actual provider and dataset: pre-edit values, order, duplicates,
   empty/null decisions, modified elements, removed/appended elements and a fresh
   reload. Coordinate other writers and any rollout/rollback window separately.
   The backward read in this fixture is evidence for its unchanged JSON shape,
   not a general guarantee that every old application can write safely after a
   schema or data migration.

## Evidence

The canonical EF10 executable creates the actual SQLite file consumed by the
canonical EF11 executable. `eng/Test-TrailCards.ps1 -Record` captures compilation
and execution with exact dependency/input hashes through the existing evidence
recorder. It then checks an EF10 fresh read of the EF11 result. EF11 evidence
also tracks the older project's source inputs and the orchestration script.
No mock, cross-compilation, Cosmos/SQL Server execution or deployed production
migration is represented as proof. `GenerateCreateScript` checks the different
model nullability; it is not an executed migration or a migrations-diff test.

Packed stdio list/get separately verifies the complete expanded Markdown,
applicability and target11/edition11 from an isolated local feed/cache and an
unrelated working directory. Unusable proxies establish proxy-independent
retrieval, not OS-level network isolation. GA and comprehensive release gates
remain open; this guidance neither provisions resources nor publishes packages.

## Official references

- [EF11 owned JSON collection diagnostic and mitigations](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-11.0/breaking-changes#owned-json-collections-obsolete).
- [Reviewed immutable documentation baseline](https://github.com/dotnet/EntityFramework.Docs/blob/aa051c4c1cad21b2dc03fa401ac0e925c3644606/entity-framework/core/what-is-new/ef-core-11.0/breaking-changes.md).
- [Package-matched RC2 relational validator](https://github.com/dotnet/dotnet/blob/820230c386db5b0106c5430b3e5aee92a7826bf5/src/efcore/src/EFCore.Relational/Infrastructure/RelationalModelValidator.cs).
- [EF10 complex types and JSON](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-10.0/whatsnew#complex-types).
