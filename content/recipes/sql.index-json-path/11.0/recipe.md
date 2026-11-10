# Maintain a lookup index over a document field

## Goal

A dune specimen archive stores a ridge and tray inside each card's location document. Staff search by ridge. Keep the document shape while declaring the one lookup path in the database model.

## Older approach

EF10 maps the location to text JSON and declares a bounded computed column over JSON_VALUE, then a normal index on that column. This remains a useful choice when native JSON indexing is unavailable. The sample limits the extracted value to nvarchar(80); choose a bound and collation appropriate to real data, and assess truncation before migration. It checks result correctness, not optimizer selection or speed.

## Newer approach

EF11 maps a complex location to native json and puts HasIndex over Location.Ridge. GenerateCreateScript shows the actual CREATE JSON INDEX with the path; EnsureCreated applies it only to this fresh test database. Querying the complex member then returns the matching card. The physical index is inspected through sys.indexes.

{{snippet:json-model}}

## Database transition

Inspect generated DDL before applying a migration to an existing database; EnsureCreated is a sample bootstrap, not a production migration strategy. Move text JSON to native json only after validating existing content. SQL Server requires a clustered primary key for JSON indexes. Confirm release, native type and CREATE JSON INDEX availability on the actual server. Do not infer support from the client package or compatibility level alone. The executed path is SQL2025 CU10, compatibility170; older engines are not executed.

EF11 supports creating and scaffolding JSON indexes. For database-first work, use the matching EF11 tooling to reverse engineer the schema and inspect the resulting path configuration. Reverse-engineering CLI execution is not part of this sample's proof. Do not add uniqueness or mix arbitrary paths into this bounded example. No optimizer-plan or performance claim is made.

## Requirements

Use one project in its own directory. Restore its locked packages with the pinned SDK, then run it only against an empty, exclusively owned disposable `playbook_` database. Supply `PLAYBOOK_SQL_CONNECTION_STRING` privately; do not put it in source or logs. The sample creates schema and synthetic data. The approved `eng/Test-DuneArchive.ps1 -Record` runner supplies this variable, creates separate databases and removes them in finally. It stops the local server afterward.

Current: SDK11.0.100-rc.3.26522.148, C#15, EF SQL Server11.0.0-rc.2.26504.105, SqlClient7.1.0, Pkcs10.0.13. Previous: SDK10.0.403, C#14, EF SQL Server10.0.13, SqlClient6.1.7, Pkcs10.0.13. EF11 needs net11.0; no EF11/net10 route is offered. The previous sample is a comparison, not another .NET10 foundation.

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

The .NET 11 SDK baseline is now 11.0.100 GA with preview selection disabled. The .NET 10 comparisons retain their declared SDKs. Earlier RC audit details below are historical; use the current sample pins, dependency locks and GA verification records for current coverage. Existing native exclusions and pending Human gates still apply.

The locked consumers compile on their declared SDKs and execute supported JSON/index, exact-distance and persisted-projection checks on Windows against the local Linux SQL2025 route. Current vector-index DDL and exact/approximate VectorSearch are SQL-generation checks only. They do not prove application or execution of that branch. Pingu explicitly authorized this unavailable-engine execution exclusion on 2 November 2026. No mock, GA proof, production migration, reverse-engineering CLI execution, other platform or measured approximate recall is claimed. Installed-tool list/get verifies the complete offline body and applicability independently of consumer execution.

## Official references

- [EF Core 11 authoring changes](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-11.0/whatsnew)
- [EF Core scalar and JSON path indexes](https://learn.microsoft.com/en-us/ef/core/modeling/indexes)
- [SQL Server CREATE JSON INDEX prerequisites](https://learn.microsoft.com/en-us/sql/t-sql/statements/create-json-index-transact-sql?view=sql-server-ver17)
- [EF Core SQL Server vector search and loading](https://learn.microsoft.com/en-us/ef/core/providers/sql-server/vector-search)
- [SQL Server VECTOR_SEARCH syntax and availability](https://learn.microsoft.com/en-us/sql/t-sql/functions/vector-search-transact-sql?view=sql-server-ver17)

## Migration

1. Confirm native json and CREATE JSON INDEX support on the exact database engine; compatibility level alone is insufficient.
2. Validate existing text documents before converting them to native json and ensure the table has a clustered primary key.
3. Map the complex document and declare HasIndex over the selected scalar path, then inspect generated DDL before applying a reviewed migration.
4. Check matching records and the physical index on an isolated database. Keep the bounded computed-column index where native JSON indexing is unavailable.

## Constraints

Native JSON indexing depends on actual server feature support and a clustered primary key. The previous computed-column extraction is bounded to nvarchar(80); assess truncation and collation against real data. No optimizer-plan, speed, reverse-engineering CLI execution or actual GA claim is made.

The complete shared sample also demonstrates experimental vector search and suppresses EF9105 for that separate portion. JSON indexing itself does not require that EF diagnostic suppression. The shared vector-search execution exclusion does not replace the successful local JSON-index execution checks.
