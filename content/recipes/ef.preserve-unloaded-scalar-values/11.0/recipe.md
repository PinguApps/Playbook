# Avoid saving a scalar value that was not loaded

## Goal

Preserve an unread scalar during partial-record updates and deliberately re-enable writes for intentional replacement values.

## When to use

Project Id and Name only, then attach a new object. Ordinary attach does not infer which columns were read: IsLoaded initially remains true. Mark Count.IsLoaded=false before setting the entity Modified. The UPDATE omits Count and a fresh context still reads 42. IsLoaded=true does not fetch data. The sample explicitly marks the scalar modified and writes the CLR default 0, then loads it normally and writes 8. Default-valued and unread values are different. This scalar update omission mechanism is not authorization, concurrency protection, a general partial-entity guarantee or a vector fluent-loader contract. Validate keys, inserts, relationships and concurrency tokens separately; use edit models with explicitly allowed writable fields.

## Older approach

EF10 has no scalar IsLoaded. After attaching and marking the entity Modified, clear the unread scalar IsModified before saving. This fixture leaves the default-valued scalar and its snapshot untouched, so detection does not mark it modified again. Later value or entity-state changes can invalidate that selection; apply deliberate write selection after detection. IsModified is not durable load-state metadata.

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

1. Project only intended fields and retain the real key; attach the partial object in a short-lived context.
2. On EF11 mark known-unread scalars IsLoaded=false before broad modification; on EF10 clear IsModified after marking the entity modified.
3. Inspect the actual UPDATE and reload in a fresh context to prove the database value survived.
4. Re-enable writes only with an intentional replacement; test a loaded default and normal loaded update.

## Constraints

Supported current route requires .NET/SDK11 and matching EF11 SQLite packages. Measured: SDK11.0.100, runtime11 GA, C#15 and EF11.0.0; previous: SDK10.0.403, runtime10.0.14, C#14 and EF10.0.13. Stable API shapes measured on GA cover only the recorded checks. No experimental opt-in or C#15-specific syntax is required. EF11 requires net11.0 although direct SQLite11 selects net10.0; no older-runtime EF11 route or extra .NET10 foundation is offered. Windows x64 executes SQLite3.53.3/SQLitePCLRaw2.1.13 versus SQLite3.53.4/SQLitePCLRaw3.0.5 using normal e_sqlite3 initialization. Full independent graphs and content hashes are locked. No encryption, retired bundle, other provider/platform or production guarantee. No workload or browser rendering mode applies. The fixture owns its open in-memory connection until all contexts and service providers are disposed. Never share a context concurrently.

## Evidence

The .NET 11 SDK baseline is now 11.0.100 GA with preview selection disabled. The .NET 10 comparisons retain their declared SDKs. Use the current sample pins, dependency locks and GA verification records for current coverage. Existing native exclusions and pending Human gates still apply.

Run eng/Test-WrenTracking.ps1 to execute both locked graphs; -Record refreshes sample verification-evidence.json. Records contain exact commands, SDK/runtime details, resolved dependencies, input hashes, host, SQLite version/source ID, executed SQL and PASS checks. This is actual compilation and SQLite execution, not mocks, metadata or cross-compilation. State checks cover stale/current states, all flags/defaults, scan counts and persisted insert/update/delete. Scalar checks cover partial SELECT, UPDATE omission, fresh reload, re-enable/default write and loaded update. Pool checks cover shared configuration, disposal/reuse, EF tracking and explicit RequestLabel reset, unsaved-mutation exclusion and persisted save. Other application fields and async interaction are not independently exercised. Ordinary installed-tool inspection separately checks ID/title/goal, complete canonical Markdown, target11/edition11, applicability and evidence over real stdio from an unrelated directory with unusable proxies. That proves embedded retrieval, not OS network isolation. PIN-959 comprehensive coverage, PIN-967 actual GA and existing Human gates remain open. Each M(3) retains its focused correctness/lifecycle and consumer proof scope; no recurring platform/client matrix is added.

## Official references

- [Package-matched EF11 source](https://github.com/dotnet/dotnet/blob/820230c386db5b0106c5430b3e5aee92a7826bf5/src/efcore/src/EFCore/ChangeTracking/PropertyEntry.cs)
- [EF change tracking](https://learn.microsoft.com/en-us/ef/core/change-tracking/)
- [Context pooling and state ownership](https://learn.microsoft.com/en-us/ef/core/performance/advanced-performance-topics)
