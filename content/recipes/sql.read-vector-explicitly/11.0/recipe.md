# Read stored vectors explicitly

## Goal

An archive normally displays echo identifiers without their stored signatures. An inspection task occasionally needs the exact persisted vector. Ask for that column explicitly rather than treating an omitted entity value as stored data.

## Older approach

EF10 includes SqlVector columns when materializing an ordinary entity. The old sample checks generated SQL includes the vector, executes the entity query, and separately projects the persisted vector to verify its three components. Existing explicit projection remains valid.

## Newer approach

EF11 omits vector columns from the default entity SELECT. A predicate or ordering can still use the vector without adding it to entity projection. The current sample checks the SQL omission and executes the entity query in a fresh context, then reads Embedding with an explicit scalar projection and compares it with the seeded values.

{{snippet:loading}}

## Loading transition

Audit code that reads vector properties after an ordinary entity query. Use an explicit projection whenever the stored values are needed. Do not interpret an unpopulated value as a stored zero vector, and do not write an omitted value back. These reads use AsNoTracking to keep query behavior separate from an already tracked entity's values. No future auto-load opt-in API is assumed or recommended. Exact VectorDistance ordering and persisted projection require SQL2025 native vector support; they do not require an approximate index or a preview opt-in.

The complete shared sample also contains experimental vector-search demonstration code. Its EF9105 suppression applies to that separate portion; explicit scalar projection itself is not an experimental EF API.

## Requirements

Use one project in its own directory. Restore its locked packages with the pinned SDK, then run it only against an empty, exclusively owned disposable `playbook_` database. Supply `PLAYBOOK_SQL_CONNECTION_STRING` privately; do not put it in source or logs. The sample creates schema and synthetic data. The approved `eng/Test-DuneArchive.ps1 -Record` runner supplies this variable, creates separate databases and removes them in finally. It stops the local server afterward.

Current: SDK11.0.100, C#15, EF SQL Server11.0.0, SqlClient7.1.0, Pkcs10.0.13. Previous: SDK10.0.403, C#14, EF SQL Server10.0.13, SqlClient6.1.7, Pkcs10.0.13. EF11 needs net11.0; no EF11/net10 route is offered. The previous sample is a comparison, not another .NET10 foundation.

EF11 uses lib/net11.0; EF10 uses lib/net10.0; the drivers select compatible net9.0 assets, with Windows runtime assets in these executions. Exact dependency closures are in the project locks and verification manifests. There are no workloads, web rendering modes, embedding services or accounts to configure. The synthetic three-dimensional vectors are test data, not a semantic model.

### .NET10 complete comparison project

{{snippet:n10-global-json}}

{{snippet:n10-directory-build-props}}

{{snippet:n10-directory-build-targets}}

{{snippet:n10-directory-packages-props}}

{{snippet:n10-dunearchive-csproj}}

{{snippet:n10-program-cs}}

### .NET11 complete comparison project

{{snippet:n11-global-json}}

{{snippet:n11-directory-build-props}}

{{snippet:n11-directory-build-targets}}

{{snippet:n11-directory-packages-props}}

{{snippet:n11-dunearchive-csproj}}

{{snippet:n11-program-cs}}

## Evidence

The .NET 11 SDK baseline is now 11.0.100 GA with preview selection disabled. The .NET 10 comparisons retain their declared SDKs. Use the current sample pins, dependency locks and GA verification records for current coverage. Existing native exclusions and pending Human gates still apply.

The locked consumers compile on their declared SDKs and execute supported JSON/index, exact-distance and persisted-projection checks on Windows against the local Linux SQL2025 route. Current vector-index DDL and exact/approximate VectorSearch are SQL-generation checks only. They do not prove application or execution of that branch. Pingu explicitly authorized this unavailable-engine execution exclusion on 2 November 2026. No mock, GA proof, production migration, reverse-engineering CLI execution, other platform or measured approximate recall is claimed. Installed-tool list/get verifies the complete offline body and applicability independently of consumer execution.

## Official references

- [EF Core 11 authoring changes](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-11.0/whatsnew)
- [EF Core scalar and JSON path indexes](https://learn.microsoft.com/en-us/ef/core/modeling/indexes)
- [SQL Server CREATE JSON INDEX prerequisites](https://learn.microsoft.com/en-us/sql/t-sql/statements/create-json-index-transact-sql?view=sql-server-ver17)
- [EF Core SQL Server vector search and loading](https://learn.microsoft.com/en-us/ef/core/providers/sql-server/vector-search)
- [SQL Server VECTOR_SEARCH syntax and availability](https://learn.microsoft.com/en-us/sql/t-sql/functions/vector-search-transact-sql?view=sql-server-ver17)

## Migration

1. Restore the pinned complete project with its declared SDK and locked packages.
2. Inspect its generated SQL and compare the actual database capabilities before changing an application.
3. Replace implicit vector reads with explicit projection where needed. Review schema/data impact before applying production migrations; this sample uses fresh disposable databases.

## Constraints

The supplied SQL2025 CU10 rejects EF11 WITH APPROXIMATE. That execution is explicitly not-tested under the owner-authorized exception. No GA, optimizer-plan or measured recall/performance guarantee is made.
