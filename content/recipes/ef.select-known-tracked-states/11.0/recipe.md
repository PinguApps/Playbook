# Select tracked entries without an extra detection pass

## Goal

Inspect known tracked states after deliberate change detection without triggering another scan.

## When to use

Both GetEntriesForState overloads default added, modified, deleted and unchanged to false: the argument-free result is empty. Property assignments can leave state stale until DetectChanges. Detect once at the intended boundary, then name the desired flags. Entries normally detects again when AutoDetectChangesEnabled is true. Materialize before modifying entry state during iteration. No timing or speedup benchmark is claimed.

## Older approach

EF10 lacks GetEntriesForState. The original SelectStates helper temporarily disables AutoDetectChangesEnabled, materializes Entries filtering and restores the previous setting in finally. Explicit detection remains the caller responsibility; disabling it globally risks missed changes. Both generic and non-generic helpers execute.

### .NET 10 global.json

{{snippet:n10-global-json}}

### .NET 10 Directory.Build.props

{{snippet:n10-directory-build-props}}

### .NET 10 Directory.Build.targets

{{snippet:n10-directory-build-targets}}

### .NET 10 Directory.Packages.props

{{snippet:n10-directory-packages-props}}

### .NET 10 WrenTracking.csproj

{{snippet:n10-wrentracking-csproj}}

### .NET 10 Program.cs

{{snippet:n10-program-cs}}

## Newer approach

Use the pinned EF11 project below. Both complete WrenTracking projects execute all three bounded comparisons against disposable SQLite. Copy each version into its own directory; local empty build files prevent repository build inheritance.

### .NET 11 global.json

{{snippet:n11-global-json}}

### .NET 11 Directory.Build.props

{{snippet:n11-directory-build-props}}

### .NET 11 Directory.Build.targets

{{snippet:n11-directory-build-targets}}

### .NET 11 Directory.Packages.props

{{snippet:n11-directory-packages-props}}

### .NET 11 WrenTracking.csproj

{{snippet:n11-wrentracking-csproj}}

### .NET 11 Program.cs

{{snippet:n11-program-cs}}

## Migration

1. Identify the boundary where object mutations must become tracked states, then call DetectChanges.
2. Replace repeated Entries filtering with GetEntriesForState and name every desired flag.
3. Materialize selections before changing state; preserve SaveChanges detection unless you take responsibility for it.
4. Run the comparison and check stale/current results, all flags, scan counts and persisted insert/update/delete results.

## Constraints

Supported current route requires .NET/SDK11 and matching EF11 SQLite packages. Measured: SDK11.0.100-rc.3.26522.148, runtime11 RC3, C#15 and EF11.0.0-rc.2.26504.105; previous: SDK10.0.403, runtime10.0.14, C#14 and EF10.0.13. Stable API shapes measured on RC2 are not GA verification. No experimental opt-in or C#15-specific syntax is required. EF11 requires net11.0 although direct SQLite11 selects net10.0; no older-runtime EF11 route or extra .NET10 foundation is offered. Windows x64 executes SQLite3.53.3/SQLitePCLRaw2.1.13 versus SQLite3.53.4/SQLitePCLRaw3.0.5 using normal e_sqlite3 initialization. Full independent graphs and content hashes are locked. No encryption, retired bundle, other provider/platform or production guarantee. No workload or browser rendering mode applies. The fixture owns its open in-memory connection until all contexts and service providers are disposed. Never share a context concurrently.

## Evidence

The .NET 11 SDK baseline is now 11.0.100 GA with preview selection disabled. The .NET 10 comparisons retain their declared SDKs. Earlier RC audit details below are historical; use the current sample pins, dependency locks and GA verification records for current coverage. Existing native exclusions and pending Human gates still apply.

Run eng/Test-WrenTracking.ps1 to execute both locked graphs; -Record refreshes sample verification-evidence.json. Records contain exact commands, SDK/runtime details, resolved dependencies, input hashes, host, SQLite version/source ID, executed SQL and PASS checks. This is actual compilation and SQLite execution, not mocks, metadata or cross-compilation. State checks cover stale/current states, all flags/defaults, scan counts and persisted insert/update/delete. Scalar checks cover partial SELECT, UPDATE omission, fresh reload, re-enable/default write and loaded update. Pool checks cover shared configuration, disposal/reuse, EF tracking and explicit RequestLabel reset, unsaved-mutation exclusion and persisted save. Other application fields and async interaction are not independently exercised. Ordinary installed-tool inspection separately checks ID/title/goal, complete canonical Markdown, target11/edition11, applicability and evidence over real stdio from an unrelated directory with unusable proxies. That proves embedded retrieval, not OS network isolation. PIN-959 comprehensive coverage, PIN-965 actual GA and existing Human gates remain open. Each M(3) retains its focused correctness/lifecycle and consumer proof scope; no recurring platform/client matrix is added.

## Official references

- [Package-matched EF11 source](https://github.com/dotnet/dotnet/blob/820230c386db5b0106c5430b3e5aee92a7826bf5/src/efcore/src/EFCore/ChangeTracking/ChangeTracker.cs)
- [EF change tracking](https://learn.microsoft.com/en-us/ef/core/change-tracking/)
- [Context pooling and state ownership](https://learn.microsoft.com/en-us/ef/core/performance/advanced-performance-topics)
