# Filter SQLite rows by time components

## Goal

A wetland reserve schedules audible signals at its observation hides. Operators
need the signals whose stored time has a chosen hour, minute or second. Keep that
selection in SQLite and return only matching IDs. Component matching repeats
across the day: minute 15 means every hour's minute 15, unless an hour is also
specified. It is not an elapsed interval or a timezone conversion.

EF11 translates `TimeOnly.Hour`, `Minute` and `Second` to integer casts of SQLite
`strftime` using `%H`, `%M` and `%S`. EF10 can store the same values but rejects
these members in a predicate. Parameterized SQL remains a working older approach.
The improvement is provider translation, not new CLR properties or a new SQLite
date/time type. This edition targets .NET11; its .NET10 comparison does not add
another approved foundation or change existing recipe identities.

## Older approach

Use `FromSqlInterpolated` directly on the `DbSet`, with fixed identifiers and
format strings. Interpolated values become database parameters; keep the argument
as a `FormattableString`, rather than first building a `string` for a raw SQL API.
The helper uses -1 to omit a component filter, since valid time components are
nonnegative. Its caller supplies known component values. Compose the station
filter, ordering and ID projection before `ToArray`. No entity rows need to be
materialized to apply the application filters.

The complete older fixture also attempts each direct member predicate. All three
must fail translation before a command reaches SQLite. Calling `AsEnumerable` or
materializing the table first would defeat this goal. The separate full-table
round-trip in the fixture verifies storage only; it is not the application route.

Copy these six canonical files into a separate standalone directory. Empty local
build files prevent inherited repository settings; the local central package file
pins the one direct provider dependency. The repository's lock file preserves the
exact transitive graph used for the saved evidence.

### global.json

{{snippet:n10-global-json}}

### Directory.Build.props

{{snippet:n10-directory-build-props}}

### Directory.Build.targets

{{snippet:n10-directory-build-targets}}

### Directory.Packages.props

{{snippet:n10-directory-packages-props}}

### HeronSignals.csproj

{{snippet:n10-heronsignals-csproj}}

### Program.cs

{{snippet:n10-program-cs}}

## Newer approach

Keep the predicate as `IQueryable` and use the three members directly. Each
specified component adds a server predicate. The command interceptor observes
the actual executing command, including parameters; `ToQueryString` alone would
not establish execution. Each application check requires one command, the
expected `strftime` formats in SQL, and exact matching IDs. A station containing
SQL punctuation is sent as a parameter and matches no row.

The current fixture rejects a `Millisecond` predicate before execution. The three
supported members do not imply support for every `TimeOnly` member. No internal
translator class is called or registered by application code.

Use these files in another directory; do not mix the older/current projects.

### global.json

{{snippet:n11-global-json}}

### Directory.Build.props

{{snippet:n11-directory-build-props}}

### Directory.Build.targets

{{snippet:n11-directory-build-targets}}

### Directory.Packages.props

{{snippet:n11-directory-packages-props}}

### HeronSignals.csproj

{{snippet:n11-heronsignals-csproj}}

### Program.cs

{{snippet:n11-program-cs}}

## Migration

1. Inspect the application's actual target, EF/provider packages and native
   SQLite assets. EF11 needs .NET11. Direct Microsoft.Data.Sqlite11's net10 asset
   does not make EF11 a supported net10 consumer. Retain the EF10 SQL alternative
   when upgrading the framework/provider is not appropriate.
2. Confirm the mapped column uses the demonstrated default `TimeOnly` TEXT
   representation. Inspect actual stored values before adopting these queries;
   custom converters, numeric storage or legacy text require separate analysis.
   Define how null times and independent versus combined components should behave.
3. In each standalone directory, acquire its pinned SDK and packages. For exact
   repository reproduction, use the committed `packages.lock.json` and run
   `dotnet build HeronSignals.csproj --configuration Release -p:RestoreLockedMode=true`.
   If reconstructing only the six offline files, first run
   `dotnet restore HeronSignals.csproj --use-lock-file` and review the resulting
   transitive graph; it may differ from the measured lock. Initial acquisition
   needs the packages; offline recipe retrieval does not supply a package feed.
4. Run `dotnet ./bin/Release/net10.0/HeronSignals.dll` with runtime10 for the older
   project, or `dotnet ./bin/Release/net11.0/HeronSignals.dll` with runtime11 for the
   current project. Keep the in-memory connection open across the contexts, and
   inspect the printed native version, stored values, executed SQL and results.
5. Replace the older parameterized component SQL with the current member
   predicates only after the real provider produces the same results. Keep the
   station value parameterized and the projection after filtering. In both
   versions, perform database filtering before any enumeration or materialization.
6. Require the result sets below and the version-specific translation-failure
   controls to pass. Dispose readers/contexts before disposing the connection;
   the database then disappears. Adapt only against isolated test data first.

## Constraints

The actual artifacts are SDK10.0.402, reference/runtime10.0.13, C#14 and
EF/Sqlite10.0.13 versus SDK11.0.100-rc.2.26504.105,
reference/runtime11.0.0-rc.2.26504.105, C#15 and matching EF/Sqlite RC2 packages.
These are verification pins, separate from the current route's .NET/SDK/EF11
release-family prerequisites. No C#15-specific feature is needed. The API is not
publisher-designated experimental and needs no suppression/opt-in. RC2 execution
is not final GA verification; PIN-965 retains that reconciliation.

Resolved EF10/Core/Relational/Sqlite.Core assets are net10; direct Sqlite.Core10
uses net8. EF11/Core/Relational/Sqlite.Core use net11, while Abstractions11 and
Sqlite.Core11 use net10. The independently locked native graphs execute SQLite
3.53.3 (SQLitePCLRaw bundle/core/lib/provider2.1.13) and 3.53.4 (SQLitePCLRaw
bundle/config/core/provider3.0.5 plus SQLite3.53.4). This is the normal packaged
`e_sqlite3` route, initialized by the provider bundle. The audit's Preview6
SQLite3MC headline does not override these actual RC2 artifacts. Retired bundles,
system/custom/encrypted engines and paid providers are separate choices.

Only Windows x64 was executed. Matching native assets are required on any other
host, and these runs do not prove those platforms. No workload, external service,
browser/rendering mode, account or Human interaction is needed for this console
path. Sample dependencies remain author-maintained under the dependency policy.

The default EF mapping stored whole seconds without a fractional suffix and
nonzero fractions with seven digits. Both consumers preserved every tick on
readback. In both native versions, the tested unmodified time text produced the
same whole components as the CLR, including .9995000 and .9999999 boundaries.
That observation does not make `strftime` a general seven-digit arithmetic or
fraction-extraction contract. Adding modifiers or converting representations is
outside this check. SQLite has no dedicated time type and does not enforce CLR
`TimeOnly` invariants on arbitrary SQL writes. This recipe excludes malformed
legacy strings, `24:00`, alternative REAL mappings, timezone/date conversion,
microseconds and date bucketing; SQL Server guidance is not SQLite proof.

Filtering in SQLite avoids transferring every row into the application. It does
not guarantee an index seek or a faster query: component expressions may still
scan the table inside SQLite. For a single time interval, consider a separately
validated range predicate rather than treating a component as an interval.

## Evidence

Both real native engines stored and read the following original fixture. Null
components remain SQL NULL and are excluded by the explicit non-null predicate.
The three `%H`/`%M`/`%S` outputs were checked against the listed whole time.

| ID | Actual stored TEXT (or NULL) | Native H:M:S |
| --- | --- | --- |
| 1 | 00:00:00 | 00:00:00 |
| 2 | 07:14:59.9994000 | 07:14:59 |
| 3 | 07:14:59.9995000 | 07:14:59 |
| 4 | 07:14:59.9999999 | 07:14:59 |
| 5 | 07:15:00 | 07:15:00 |
| 6 | 07:15:42.1234567 | 07:15:42 |
| 7 | 07:15:42.9999999 | 07:15:42 |
| 8 | 23:59:59.9999999 | 23:59:59 |
| 9 | NULL | NULL:NULL:NULL |
| 10 | 23:59:59.9994000 | 23:59:59 |
| 11 | 08:15:42 | 08:15:42 |

| Executed filter, station reed-hide | Exact IDs in both consumers |
| --- | --- |
| Hour 7 | 2, 3, 4, 5, 6, 7 |
| Minute 15 | 5, 6, 7, 11 |
| Second 42 | 6, 7, 11 |
| Hour 7 AND minute 15 AND second 42 | 6, 7 |
| Hour 0 AND minute 0 AND second 0 | 1 |
| Hour 23 AND minute 59 AND second 59 | 8, 10 |
| Combined filter with station `reed-hide' OR 1=1 --` | Empty |

The two `verification-evidence.json` records capture fresh locked builds and
execution, exact commands, input hashes, resolved dependencies, runtime/native
versions, native source IDs/DLL hashes, stored values, intercepted SQL/parameters
and assertions. EF10 rejects each direct member filter before a database read;
EF11 executes the supported filters and rejects `Millisecond`. No InMemory,
mock, metadata-only result or cross-compilation substitutes for these runs.

The ordinary packed tool is separately installed in a fresh local-only feed/cache
and queried over real stdio from an unrelated directory. That check verifies
ID/title/goal, explicit target11/edition11, full expanded canonical Markdown,
applicability and current evidence, using unusable proxies. It establishes offline
retrieval independent of proxies, not network isolation or fresh SQLite execution
on the reader's machine. PIN-959 comprehensive coverage and all existing Human
gates remain open. E22's M(3) covers this bounded pair, stored-boundary results and
actual translation evidence; no recurring native/provider matrix is added.

## Official references

- [Official EF11 SQLite TimeOnly introduction](https://github.com/dotnet/core/blob/60871330fce2b3da383461be027ebdf3ce30f8fb/release-notes/11.0/preview/preview6/efcore.md).
- [Package-matched EF11 formats and conversion](https://github.com/dotnet/dotnet/blob/820230c386db5b0106c5430b3e5aee92a7826bf5/src/efcore/src/EFCore.Sqlite.Core/Query/Internal/Translators/SqliteTimeOnlyMemberTranslator.cs) and [older translator registration](https://github.com/dotnet/dotnet/blob/4099f83977f4d29604a9aeb971fb0a7bc1d5f699/src/efcore/src/EFCore.Sqlite.Core/Query/Internal/SqliteMemberTranslatorProvider.cs). These explain provider behavior, not a supported application extension point.
- [Parameterized EF SQL queries](https://learn.microsoft.com/en-us/ef/core/querying/sql-queries) and [Microsoft.Data.Sqlite storage mappings](https://learn.microsoft.com/en-us/dotnet/standard/data/sqlite/types).
- [SQLite date/time function contract](https://www.sqlite.org/lang_datefunc.html).
