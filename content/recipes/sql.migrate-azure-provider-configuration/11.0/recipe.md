# Replace Azure SQL defaults configuration

## Goal

Replace the removed Azure SQL defaults helper while preserving retry settings and database access.

## When to use

EF10 UseSqlServer(..., sql => sql.UseAzureSqlDefaults()) is replaced by UseAzureSql(connection). UseAzureSql already exists in EF10: adopt it before upgrading. EF11 removes the defaults helper. Both paths use six retries with a 30-second maximum delay. Preserve intentional retry and compatibility overrides; these defaults do not change the database compatibility level. Local SQL2025 compatibility170 round-trips are approved evidence for this bounded configuration migration. No Azure outage, retry scheduling, Entra authentication or cloud service parity is established.

## Older approach

The EF10 comparison preserves the previous configuration and real persistence controls. The supported replacements can also be adopted before upgrading.

### .NET 10 global.json

{{snippet:n10-global-json}}

### .NET 10 Directory.Build.props

{{snippet:n10-directory-build-props}}

### .NET 10 Directory.Build.targets

{{snippet:n10-directory-build-targets}}

### .NET 10 Directory.Packages.props

{{snippet:n10-directory-packages-props}}

### .NET 10 EstuarySql.csproj

{{snippet:n10-estuarysql-csproj}}

### .NET 10 Program.cs

{{snippet:n10-program-cs}}

## Newer approach

UseAzureSql selects the supported entry point.

### .NET 11 global.json

{{snippet:n11-global-json}}

### .NET 11 Directory.Build.props

{{snippet:n11-directory-build-props}}

### .NET 11 Directory.Build.targets

{{snippet:n11-directory-build-targets}}

### .NET 11 Directory.Packages.props

{{snippet:n11-directory-packages-props}}

### .NET 11 EstuarySql.csproj

{{snippet:n11-estuarysql-csproj}}

### .NET 11 Program.cs

{{snippet:n11-program-cs}}

## Migration

1. Record the existing UseSqlServer/UseAzureSql configuration, connection/database, explicit retry and compatibility overrides, and table rows to compare.
2. Replace the defaults helper with UseAzureSql, preserving the connection and intentional retry/compatibility overrides.
3. Run both pinned consumers on isolated data. Compare fresh reads, updates and the negative controls before accepting the change.
4. Remove obsolete comparison controls from application code. Verify your actual provider and authentication route; reconcile GA packages separately.

## Constraints

SDK10.0.403 / C#14 / EF10.0.13 versus SDK11.0.100-rc.3.26522.148 / C#15 / EF11.0.0-rc.2.26504.105. Both EF consumers select net10.0 versus net11.0 assets. No EF11 on an older runtime or extra .NET10 foundation is offered. RC3 evidence is not actual GA proof. No workload, rendering mode or experimental opt-in applies. SqlClient6.1.7 versus7.1.0, with Pkcs10.0.13; SQL2025 17.0.5035.5 compatibility170; SQL authentication is injected through the authorized local route.

EF10 UseSqlServer(..., sql => sql.UseAzureSqlDefaults()) is replaced by UseAzureSql(connection). UseAzureSql already exists in EF10: adopt it before upgrading. EF11 removes the defaults helper. Both paths use six retries with a 30-second maximum delay. Preserve intentional retry and compatibility overrides; these defaults do not change the database compatibility level. Local SQL2025 compatibility170 round-trips are approved evidence for this bounded configuration migration. No Azure outage, retry scheduling, Entra authentication or cloud service parity is established.

## Evidence

The sample-evidence workflow captures each exact locked graph, SDK/runtime, inputs and fresh compilation separately from real provider execution. Run `eng/Test-EstuaryProviders.ps1 -Record`. SQL checks insert and query through fresh contexts and inspect retry defaults. The sibling Cosmos recipes own their separate document and concurrency checks. The installed local package is inspected with real stdio list/get from an unrelated directory. PIN-959 comprehensive readiness and PIN-965 GA reconciliation remain open.

## References

- [Official EF provider contract](https://learn.microsoft.com/en-us/ef/core/providers/sql-server/)
- [Official EF provider contract](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-11.0/breaking-changes)
