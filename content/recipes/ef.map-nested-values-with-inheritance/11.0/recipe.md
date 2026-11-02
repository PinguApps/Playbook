# Persist nested values across separate inheritance tables

## Goal

A fen archive lends and stores survey equipment. Its bay and shelf belong
together as a location value, even when different equipment types use separate
tables. TPT keeps base fields in a shared table; TPC repeats inherited fields in
each concrete table. Choose that table layout separately from scalar versus JSON
storage.

The current consumer checks four mappings: Plank uses TPT with base scalar
complex columns; Reed uses TPT with a base JSON complex value; Moss uses TPC
with inherited scalar complex columns; Sedge uses TPC with JSON declared on its
concrete Loan subtype. Each stores, reloads, edits and reloads a location.
Queries target the concrete loan type. The second concrete tables exist in the
model; their own row round-trips are not claimed.

Putting Sedge's JSON value on its base type instead throws
`Could not find JSON container column 'Location'` during materialization on the
pinned RC2 build. Base and concrete queries both failed for that inherited
mapping. Moving its declaration and configuration to the concrete subtype
works in this consumer. This bounded route does not establish that arbitrary
inherited TPC JSON mappings are safe.

## Requirements

EF Core 11 requires the .NET 11 runtime and SDK. The current consumer pins SDK
11.0.100-rc.2.26504.105, C# 15 and EF SQL Server 11.0.0-rc.2.26504.105.
The older consumer pins SDK 10.0.402, C# 14 and EF SQL Server 10.0.13.
EF11 selects net11.0 EF/Relational/provider assets; net10.0 abstractions do not
make the full provider compatible with a .NET10 application. Explicit SqlClient
and Pkcs pins plus the lock file retain the resolved graph. No workload, web
framework or rendering mode is required.

Execution used Windows x64 against local Linux SQL Server 2025 Developer
17.0.5035.5. Provider and database compatibility are 170. Native json and
CREATE JSON INDEX support are prerequisites for the current index path,
including the clustered primary key. Other providers, engines and platforms
require their own checks. This is RC2 execution, not actual GA proof.
No experimental EF API suppression or database preview opt-in is used here.

Use an empty exclusively owned disposable database. EnsureCreated creates
the schema; it is not a production migration strategy. Supply
PLAYBOOK_SQL_CONNECTION_STRING privately to the child process and never log it.
Restore consumer dependencies online before running. The complete served
guidance itself remains available offline.

## Older approach

The EF10 consumer preserves the same nested location data using a converter
that serializes each location to a string column. Its ValueComparer snapshots
both fields, so mutating Shelf causes an update. That remains a valid storage
choice when server-side predicates inside the converted value are unnecessary.
It does not pretend to provide EF11's inherited complex/JSON route.

The older label uses flat CodeValue and SlotBay columns while Zone remains a
complex JSON value. A persisted computed column extracts a bounded JSON bay,
then a regular index covers that computed column. This solves lookup without
claiming EF10 supports nested scalar keys or EF11 JSON path indexes. The old
and new schemas differ; adoption needs a reviewed data migration.

### .NET 10

#### global.json

{{snippet:n10-global-json}}

#### Directory.Build.props

{{snippet:n10-directory-build-props}}

#### Directory.Build.targets

{{snippet:n10-directory-build-targets}}

#### Directory.Packages.props

{{snippet:n10-directory-packages-props}}

#### FenStore.csproj

{{snippet:n10-fenstore-csproj}}

#### Program.cs

{{snippet:n10-program-cs}}

## Newer approach

Use complex values and nested paths as configured below. Keep the documented
RC2 restrictions when adapting this example. The shared consumer also executes
the companion inheritance/key/index checks so its provider evidence is concrete.

### .NET 11

#### global.json

{{snippet:n11-global-json}}

#### Directory.Build.props

{{snippet:n11-directory-build-props}}

#### Directory.Build.targets

{{snippet:n11-directory-build-targets}}

#### Directory.Packages.props

{{snippet:n11-directory-packages-props}}

#### FenStore.csproj

{{snippet:n11-fenstore-csproj}}

#### Program.cs

{{snippet:n11-program-cs}}

## Migration

1. Choose a shared base table (TPT) or independent concrete tables (TPC).
   Register every complex value explicitly. Keep IDs unique across a TPC
   hierarchy; this disposable example supplies distinct IDs itself.
2. Decide which values need scalar constraints/predicates and which need JSON.
   Inspect generated DDL and actual provider/server support before adoption.
3. Keep required, bounded scalar key paths separate from JSON lookup paths.
   Use explicit LINQ identity lookup for the demonstrated RC2 nested key.
4. On this RC2 baseline declare TPC JSON on the concrete subtype as shown.
   Do not move it to the base type without new successful execution evidence.
5. Review migrations against a copy of existing data before production use.
   EnsureCreated is only disposable setup; never use it to upgrade existing data.

Create every file of one version in a standalone directory. Empty local build
files isolate the consumer from inherited repository settings. With its pinned
SDK/runtime and a private connection to an empty test database, run
`dotnet run --configuration Release`. The project creates a dependency lock on
its first restore; the checked repository runner uses its committed lock with
RestoreLockedMode=true.
Repository authors can run `eng/Test-FenStore.ps1 -Record` to execute both
releases with separate owned databases, capture evidence and clean up.
The helper stops the server afterwards; coordinate with other container users.

Expect PASS messages for physical keys/indexes, persisted location updates,
identity resolution, scalar and JSON label lookups, and duplicate rejection.
Generated DDL is displayed. The current JSON index and older computed index
establish distinct actual provider paths, with no optimizer or timing guarantee.

## Constraints

Keys exclude complex collections, nullable complex paths and JSON properties.
SQL Server JSON indexes require native json and a clustered primary key.
On RC2 use concrete-subtype TPC JSON and explicit LINQ identity predicates;
inherited-base TPC JSON and nested-key FindAsync remain failing paths.
No other provider, optimizer plan, throughput or actual GA claim is made.

## Evidence

Both canonical projects compile and execute against real SQL Server. Saved
verification-evidence.json records retain source/configuration hashes, exact
SDKs, resolved dependencies, commands and output. This is neither mocked
execution nor cross-compilation. Actual installed stdio list/get checks
separately verify discovery, complete canonical Markdown, applicability and the
11.0 edition offline. GA reconciliation remains separate. Failing inherited
TPC JSON and nested-key FindAsync paths are not counted as passing behavior.

## Official references

- [EF11 authoring changes](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-11.0/whatsnew).
- [Complex values](https://learn.microsoft.com/en-us/ef/core/modeling/complex-types).
- [Keys](https://learn.microsoft.com/en-us/ef/core/modeling/keys).
- [Indexes](https://learn.microsoft.com/en-us/ef/core/modeling/indexes).
- [SQL Server JSON index requirements](https://learn.microsoft.com/en-us/sql/t-sql/statements/create-json-index-transact-sql?view=sql-server-ver17).
- [Immutable reviewed EF11 documentation](https://github.com/dotnet/EntityFramework.Docs/blob/aa051c4c1cad21b2dc03fa401ac0e925c3644606/entity-framework/core/what-is-new/ef-core-11.0/whatsnew.md).
