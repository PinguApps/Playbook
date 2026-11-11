# Migrate raw SQL calls while preserving parameters

## Goal

Replace obsolete raw SQL entry points while retaining separate value parameters and cancellation.

## When to use

EF10 FromSqlInterpolated/ExecuteSqlInterpolated/ExecuteSqlInterpolatedAsync remain working parameterized APIs; EF11 marks them obsolete in favor of FromSql/ExecuteSql/ExecuteSqlAsync, which already exist in EF10. Keep an inline interpolation or FormattableString; assigning to string before a Raw call loses parameterization. Parameters represent values, not identifiers or arbitrary SQL syntax. Use fixed/allowlisted identifiers. Real SQLite query/update commands keep a quote/semicolon/comment-bearing label out of command text and inside parameters; fresh reads confirm two rows and no canceled write. Already-canceled async query/update throw OperationCanceledException; this does not prove interruption of a running database command. A fixed authored OR 1=1 negative control demonstrates changed predicate semantics without building SQL from untrusted input. Compile-only controls observe EF1002 interpolation on both versions and EF1003 Format/Concat recognition only on EF11, plus new CS0618 warnings. Diagnostics are neither sanitizers nor a comprehensive security analysis.

## Older approach

Use the EF10 comparison below. Its original parameterized calls remain valid.

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

Current supported route: .NET/SDK11, C#15 and matching EF11 packages. Measured SDK11.0.100, EF11.0.0 versus SDK10.0.403, C#14, runtime10.0.14 and EF10.0.13. Stable API shapes on GA cover only the recorded checks. EF11/Core/Relational select net11.0; direct Sqlite11 selects net10.0. EF10 selects net10.0 and direct Sqlite10 net8.0. SQL Server provider uses SqlClient6.1.7 versus7.1.1; locked graphs pin Pkcs10.0.13. Windows x64 native SQLite3.53.3/SQLitePCLRaw2.1.13 versus3.53.4/3.0.5. Both native hashes/source IDs are recorded. No older-runtime EF11 route, new .NET10 foundation, encryption, other OS/provider, Cosmos transaction or GA claim. No workload, rendering mode or experimental opt-in applies.

EF10 FromSqlInterpolated/ExecuteSqlInterpolated/ExecuteSqlInterpolatedAsync remain working parameterized APIs; EF11 marks them obsolete in favor of FromSql/ExecuteSql/ExecuteSqlAsync, which already exist in EF10. Keep an inline interpolation or FormattableString; assigning to string before a Raw call loses parameterization. Parameters represent values, not identifiers or arbitrary SQL syntax. Use fixed/allowlisted identifiers. Real SQLite query/update commands keep a quote/semicolon/comment-bearing label out of command text and inside parameters; fresh reads confirm two rows and no canceled write. Already-canceled async query/update throw OperationCanceledException; this does not prove interruption of a running database command. A fixed authored OR 1=1 negative control demonstrates changed predicate semantics without building SQL from untrusted input. Compile-only controls observe EF1002 interpolation on both versions and EF1003 Format/Concat recognition only on EF11, plus new CS0618 warnings. Diagnostics are neither sanitizers nor a comprehensive security analysis.

## Evidence

The .NET 11 SDK baseline is now 11.0.100 GA with preview selection disabled. The .NET 10 comparisons retain their declared SDKs. Use the current sample pins, dependency locks and GA verification records for current coverage. Existing native exclusions and pending Human gates still apply.

Run eng/Test-AlderQueries.ps1 -Record with the approved SQL2025 Docker route. It compiles both independent locked graphs, records fresh input hashes/dependencies/SDK/runtime, creates unique restricted databases, runs real SQL and native SQLite checks, and removes owned databases/logins in finally. behavior.evidence.json retains SQL, parameter values, runtime checks and compile-only diagnostics. verification-evidence.json retains compilation. Never execute DiagnosticProbe: it deliberately contains insecure compiler controls. Actual installed offline stdio inspection separately verifies discovery, full canonical body, target11/edition11, applicability and evidence. Unusable retrieval proxies prove proxy independence, not OS network isolation. PIN-959/PIN-965 and existing Human gates remain open.

## Official references

- [Official EF reference](https://github.com/dotnet/dotnet/blob/820230c386db5b0106c5430b3e5aee92a7826bf5/src/efcore/src/EFCore.Relational/Extensions/RelationalDatabaseFacadeExtensions.cs)
- [Official EF reference](https://learn.microsoft.com/en-us/ef/core/querying/sql-queries)
- [Official EF reference](https://learn.microsoft.com/en-us/ef/core/saving/transactions)


## Compile-only negative diagnostic controls

The probe methods are excluded from normal compilation by #if DIAGNOSTIC_PROBE. The runner defines DIAGNOSTIC_PROBE only during compilation; never execute these intentionally unsafe call forms.

### EF10

{{snippet:n10-diagnostic-probe}}

### EF11

{{snippet:n11-diagnostic-probe}}
