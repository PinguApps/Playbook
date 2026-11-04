# Preserve relational automatic-transaction intent

## Goal

Preserve automatic transaction and rollback behavior when replacing an obsolete transaction switch.

## When to use

EF10 AutoTransactionsEnabled=true selects WhenNeeded, not Always (measured by an old-boolean-only single-command save with zero transaction starts); false selects Never. EF11 removes that boolean. AutoTransactionBehavior already exists in EF10 and is a valid migration before upgrading. Never can leave earlier commands committed after a later command fails. WhenNeeded wraps the multi-command save; Always also starts a transaction for a single command. The fixture forces separate commands with MaxBatchSize(1), rejects the second insert with a CHECK constraint and reads persisted IDs through a fresh context. Separate single-command saves measure TransactionStarted. No ambient/explicit transaction, retry strategy, Cosmos mode or distributed atomicity guarantee.

## Older approach

Use the EF10 comparison below. Its original parameterized calls remain valid. The transaction enum and collection mode API can also be adopted before upgrading.

### .NET 10 global.json

{{snippet:n10-global-json}}

### .NET 10 Directory.Build.props

{{snippet:n10-directory-build-props}}

### .NET 10 Directory.Build.targets

{{snippet:n10-directory-build-targets}}

### .NET 10 Directory.Packages.props

{{snippet:n10-directory-packages-props}}

### .NET 10 AlderQueries.csproj

{{snippet:n10-alderqueries-csproj}}

### .NET 10 Program.cs

{{snippet:n10-program-cs}}

## Newer approach

Use the EF11 comparison below. The full AlderQueries projects exercise all three goals; copy each version separately.

### .NET 11 global.json

{{snippet:n11-global-json}}

### .NET 11 Directory.Build.props

{{snippet:n11-directory-build-props}}

### .NET 11 Directory.Build.targets

{{snippet:n11-directory-build-targets}}

### .NET 11 Directory.Packages.props

{{snippet:n11-directory-packages-props}}

### .NET 11 AlderQueries.csproj

{{snippet:n11-alderqueries-csproj}}

### .NET 11 Program.cs

{{snippet:n11-program-cs}}

## Migration

1. Identify the existing configuration/call and required database semantics; keep a before-upgrade result fixture.
2. Replace the obsolete call with the explicit supported API described above, preserving parameter values and transaction intent.
3. Run the complete pinned comparison on disposable databases. Inspect actual SQL, parameters, persisted rows and negative controls.
4. Restore/build your own consumer and verify its provider, compatibility and failure path; reconcile actual GA artifacts separately.

## Constraints

Current supported route: .NET/SDK11, C#15 and matching EF11 packages. Measured SDK11.0.100-rc.2.26504.105, EF11.0.0-rc.2.26504.105 versus SDK10.0.402, C#14, runtime10.0.13 and EF10.0.13. Stable API shapes on RC2 are not GA proof. EF11/Core/Relational select net11.0; direct Sqlite11 selects net10.0. EF10 selects net10.0 and direct Sqlite10 net8.0. SQL Server provider uses SqlClient6.1.7 versus7.1.0; locked graphs pin Pkcs10.0.13. Windows x64 native SQLite3.53.3/SQLitePCLRaw2.1.13 versus3.53.4/3.0.5. Both native hashes/source IDs are recorded. No older-runtime EF11 route, new .NET10 foundation, encryption, other OS/provider, Cosmos transaction or GA claim. No workload, rendering mode or experimental opt-in applies.

EF10 AutoTransactionsEnabled=true selects WhenNeeded, not Always (measured by an old-boolean-only single-command save with zero transaction starts); false selects Never. EF11 removes that boolean. AutoTransactionBehavior already exists in EF10 and is a valid migration before upgrading. Never can leave earlier commands committed after a later command fails. WhenNeeded wraps the multi-command save; Always also starts a transaction for a single command. The fixture forces separate commands with MaxBatchSize(1), rejects the second insert with a CHECK constraint and reads persisted IDs through a fresh context. Separate single-command saves measure TransactionStarted. No ambient/explicit transaction, retry strategy, Cosmos mode or distributed atomicity guarantee.

## Evidence

Run eng/Test-AlderQueries.ps1 -Record with the approved SQL2025 Docker route. It compiles both independent locked graphs, records fresh input hashes/dependencies/SDK/runtime, creates unique restricted databases, runs real SQL and native SQLite checks, and removes owned databases/logins in finally. behavior.evidence.json retains SQL, parameter values, runtime checks and compile-only diagnostics. verification-evidence.json retains compilation. Never execute DiagnosticProbe: it deliberately contains insecure compiler controls. Actual installed offline stdio inspection separately verifies discovery, full canonical body, target11/edition11, applicability and evidence. Unusable retrieval proxies prove proxy independence, not OS network isolation. PIN-959/PIN-965 and existing Human gates remain open.

## Official references

- [Official EF reference](https://github.com/dotnet/dotnet/blob/820230c386db5b0106c5430b3e5aee92a7826bf5/src/efcore/src/EFCore/Infrastructure/DatabaseFacade.cs)
- [Official EF reference](https://learn.microsoft.com/en-us/ef/core/querying/sql-queries)
- [Official EF reference](https://learn.microsoft.com/en-us/ef/core/saving/transactions)


## Compile-only negative diagnostic controls

The probe methods are excluded from normal compilation by #if DIAGNOSTIC_PROBE. The runner defines DIAGNOSTIC_PROBE only during compilation; never execute these intentionally unsafe call forms.

### EF10

{{snippet:n10-diagnostic-probe}}

### EF11

{{snippet:n11-diagnostic-probe}}
