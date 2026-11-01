# Keep generated SQL within the database compatibility level

## Goal

Moorland Dispatch limits a load list by both a caller request and a dispatch ceiling. After an EF upgrade, the same chained Take query may use a SQL function an older engine does not understand. Preserve an intentional provider level instead of treating a package upgrade as a database upgrade. Inspect both server version and the database's configured compatibility before choosing a level.

EF10 defaults to provider150. EF11 defaults to160. An explicit UseCompatibilityLevel(150) keeps the demonstrated query on its TOP-based translation in both releases. It is appropriate for SQL2019 or lower configured compatibility; choose the actual supported level for an older engine rather than assuming150 fits every SQL installation. It does not change database settings or emulate every newer SQL feature.

## Older approach — .NET10 / EF10

EF10 already supports explicit provider compatibility. Leaving its default150 can work, but retaining the explicit setting makes the database boundary visible during upgrade. The original ordered extrema queries remain valid and return the same records.

Save the following six files together in a standalone MoorlandDispatch directory. Local Directory.Build files deliberately stop repository inheritance; the central package file pins the complete direct dependency set. Pkcs10.0.13 replaces the vulnerable older transitive default observed in the SQL authoring baseline. Keep the generated packages.lock.json after restoring.

**global.json**

{{snippet:n10-global-json}}

**Directory.Build.props**

{{snippet:n10-directory-build-props}}

**Directory.Build.targets**

{{snippet:n10-directory-build-targets}}

**Directory.Packages.props**

{{snippet:n10-directory-packages-props}}

**MoorlandDispatch.csproj**

{{snippet:n10-moorlanddispatch-csproj}}

**Program.cs**

{{snippet:n10-program-cs}}

## Newer approach — .NET11 / EF11

EF11 needs an explicit lower provider setting when its new default160 exceeds the database capability. The sample runs the explicit150 query and the release default side by side on database150 and170; the same load IDs must be returned, and the database compatibility must remain unchanged after configuring and querying through both providers.

Save the following six files together in a standalone MoorlandDispatch directory. Local Directory.Build files deliberately stop repository inheritance; the central package file pins the complete direct dependency set. Pkcs10.0.13 replaces the vulnerable older transitive default observed in the SQL authoring baseline. Keep the generated packages.lock.json after restoring.

**global.json**

{{snippet:n11-global-json}}

**Directory.Build.props**

{{snippet:n11-directory-build-props}}

**Directory.Build.targets**

{{snippet:n11-directory-build-targets}}

**Directory.Packages.props**

{{snippet:n11-directory-packages-props}}

**MoorlandDispatch.csproj**

{{snippet:n11-moorlanddispatch-csproj}}

**Program.cs**

{{snippet:n11-program-cs}}

## Migration

1. Inspect the installed EF/provider package, runtime, actual SQL engine version and database compatibility. EF11 needs SDK/runtime11; the older sample uses SDK/runtime10.
2. Select the provider level the database supports. The sample explicitly uses150 and never raises it because the engine happens to be newer. Changing database compatibility is a separate administrative decision.
3. Keep the production query shape unchanged while pinning provider compatibility. This shared test consumer also exercises whole-record extrema; use the companion ef.select-record-at-extreme recipe when deliberately changing row selection.
4. Restore the selected standalone project with its pinned SDK using `dotnet restore MoorlandDispatch.csproj --use-lock-file`. Review the resulting lock file, then build with `dotnet build MoorlandDispatch.csproj -c Release -p:RestoreLockedMode=true`.
5. Provide PLAYBOOK_SQL_CONNECTION_STRING privately for a new empty disposable playbook_ database. Run `dotnet run --project MoorlandDispatch.csproj -c Release --no-build`. This sample creates a table and changes only that database to150 then170; it intentionally requires a supporting SQL2025 test server and database-owner rights. Remove the disposable database afterwards. For older actual engines, adapt the test levels to supported values rather than running the170 phase.
6. In the checkout, use `eng/Test-SqlQueryGuidance.ps1 -Record` to refresh both real compilation/execution records through the authorized isolated route after relevant sample/package changes. Rebuild and pack Playbook, then retrieve this edition through the installed stdio harness.

## Constraints

- SQL Server2025 at compatibility150 is not SQL Server2019 execution. The provider configuration is source-supported for older/lower databases, but those older engines were not executed here. A newer engine may still accept a function at a lower database level; that does not show an older engine can execute it.
- This consumer changes compatibility only to create controlled test configurations. UseCompatibilityLevel itself sends no database-upgrade command. It controls SQL generation and cannot add engine capabilities.
- The shared consumer also checks extrema independently of compatibility configuration. Query ordering, collation and null semantics belong to the provider. The demonstrated keys are non-null integers. Anonymous composite MinBy keys fail on the pinned RC2 provider, despite a suggestive implementation comment. The sample checks this rejection. Scalar MinBy/MaxBy is not a general custom-comparer or every-provider guarantee. Empty reference-entity results are null; non-nullable scalar/value projections have different empty-sequence behavior.
- In that companion check, both extrema directions are deterministic because crate counts are filtered to the extreme and the remaining Id key is unique. A one-column tied key leaves the winner unspecified. No performance improvement or transaction-wide snapshot claim is made.
- Stable APIs are verified with prerelease EF11/.NET11 RC2 artifacts. No experimental opt-in is required. No EF11 downlevel runtime route or extra .NET10 foundation is added. No browser/rendering/workload requirement applies.

## Evidence

The paired original MoorlandDispatch projects compile and execute against their declared SDKs: SDK10.0.402/EF10.0.13/SqlClient6.1.7 and SDK11.0.100-rc.2.26504.105/EF11.0.0-rc.2.26504.105/SqlClient7.1.0, both with Pkcs10.0.13. Each locked transitive closure is retained in verification.json and packages.lock.json; generated compilation/execution records hash the actual sources and inherited inputs.

`eng/Test-SqlQueryGuidance.ps1 -Record` creates one unique restricted database per release on SQL Server2025 17.0.5035.5, runs actual commands at150 and170, and removes the databases/logins in finally. It checks selected IDs [11,12], TOP-based at provider150, release-default TOP-based versus LEAST, unchanged database compatibility, real TOP/ORDER BY commands, whole labels, deterministic tied IDs, sync/async command paths and empty entity results. EF10 direct MinBy translation rejection is the red control. These are real database executions, not mocks or SQL-string-only claims.

Official references and exact sample evidence accompany the structured response. Installed-package inspection compares the entire canonical Markdown, applicability and edition offline. It does not establish another database engine, provider, OS or actual GA release. PIN-959 comprehensive coverage and PIN-965 GA provider reconciliation remain separate gates.
