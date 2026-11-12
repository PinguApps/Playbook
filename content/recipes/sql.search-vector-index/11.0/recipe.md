# Find similar records with an indexed vector query

## Goal

A dune archive compares a requested measurement signature with stored echo signatures. Exact ranking computes every distance; approximate retrieval uses a matching vector index and may miss some closest rows. Choose deliberately based on the application's recall needs.

## Older approach

The EF10 sample orders by EF.Functions.VectorDistance and returns the nearest ID. It is an exact scan over the stored vectors, a valid baseline for a small archive. EF10 does not provide the EF11 VectorSearch/HasVectorIndex authoring surface. Hand-authored SQL is a separate database-specific option, not an equivalent EF10 LINQ API.

## Newer approach

Declare the index with the same cosine metric as the query. The APIs require explicit EF9105 suppression, visible in the complete source. SQL Server2025 vector indexes and search are preview and need PREVIEW_FEATURES in the disposable database. The .NET target does not make the database feature stable.

{{snippet:vector-model}}

Order by Distance, then Take, then WithApproximate. Read the entity from Value and its distance from Distance. Without WithApproximate, VectorSearch requests exact neighbors. With it, the generated TOP clause requests approximate search. Results can differ from exact ranking; a finite ordered distance and a small result count do not establish recall or performance.

{{snippet:search}}

## Server compatibility and migration

The supplied Docker SQL2025 CU10 server supports the legacy TOP_N vector-search form but rejects WITH APPROXIMATE (SQL error102). EF11 GA emits the newer form. Microsoft's current documentation places the latest index implementation in Azure SQL Database, Fabric SQL and Managed Instance with the Always-up-to-date policy. Do not treat SQL Server2025 alone as sufficient for this EF11 query. Check the exact engine/index implementation and generated SQL before adoption. No cloud resources are provisioned by this guidance.

The complete sample prints both SQL shapes and vector-index DDL without executing them by default. On an independently supplied compatible engine, run with --execute-search to create the actual index and exercise both queries. That optional branch is compiled but NOT TESTED here. It seeds before index creation because older preview indexes can restrict writes. Review actual database restrictions and migration impact before deploying any index. This recipe makes no measured latency or recall claim and does not silently replace EF11 SQL with legacy raw SQL.

## Requirements

Use one project in its own directory. Restore its locked packages with the pinned SDK, then run it only against an empty, exclusively owned disposable `playbook_` database. Supply `PLAYBOOK_SQL_CONNECTION_STRING` privately; do not put it in source or logs. The sample creates schema and synthetic data. The approved `eng/Test-DuneArchive.ps1 -Record` runner supplies this variable, creates separate databases and removes them in finally. It stops the local server afterward.

Current: SDK11.0.100, C#15, EF SQL Server11.0.0, SqlClient7.1.1, Pkcs10.0.13. Previous: SDK10.0.403, C#14, EF SQL Server10.0.13, SqlClient6.1.7, Pkcs10.0.13. EF11 needs net11.0; no EF11/net10 route is offered. The previous sample is a comparison, not another .NET10 foundation.

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

1. Confirm the exact engine and vector-index implementation support the generated EF11 query syntax; SQL2025 alone is insufficient.
2. Choose the metric, review index creation/backfill and write restrictions, and seed the isolated sample before creating its index. Keep exact VectorDistance as a baseline.
3. On a compatible engine, exercise exact and approximate queries with realistic data and measure recall and regressions before adopting approximate search; the optional --execute-search branch remains untested here.

## Constraints

The supplied SQL2025 CU10 rejects EF11 WITH APPROXIMATE. That execution is explicitly not-tested under the owner-authorized exception. No GA, optimizer-plan or measured recall/performance guarantee is made.
