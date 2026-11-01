# Fetch the whole record at a minimum or maximum key

## Goal

Moorland Dispatch needs the lightest and heaviest complete load, including its label. A scalar Min or Max gives only a value. Choose the row on the database, and make the tie policy part of the query: lowest Id among loads with the fewest crates, highest Id among those with the most crates.

EF11 translates MinBy, MaxBy, MinByAsync and MaxByAsync. The pinned SQL provider rejects an anonymous composite key. Instead, filter to crate counts matching a scalar MIN/MAX subquery and select the lowest/highest unique Id with MinBy/MaxBy. This preserves the declared tie policy in a single database command, at the cost of a subquery. A plain crate-count key cannot select a deterministic tied row. Do not rely on a preceding OrderBy surviving the new operator.

## Older approach — .NET10 / EF10

EF10 uses OrderBy/ThenBy and FirstOrDefault, with descending order for the maximum. That remains a clear, valid choice on EF11, especially for mixed-direction tie rules. In the pinned EF10 consumer, direct MinBy does not translate; the sample checks that failure rather than fetching the table into memory.

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

The EF11 sample uses a MIN/MAX subquery to retain tied candidates, then selects the unique Id with a scalar MinBy/MaxBy key. It returns the same records as the older ordered queries. The existing ordered form remains simpler for general tie rules; do not replace it solely for brevity or speed. Four actual commands materialize four whole records; two execute through the async command API. Both sync/async minimum return Fern/11 and maximum return Heather/22. Empty entity queries return null. Await each operation before reusing a DbContext.

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
3. Choose the row-selection policy. Keep ordered FirstOrDefault for mixed-direction ties, or use the tested EF11 aggregate-filter plus scalar-key form to resolve tied values in one command. Do not materialize the table to bypass translation.
4. Restore the selected standalone project with its pinned SDK using `dotnet restore MoorlandDispatch.csproj --use-lock-file`. Review the resulting lock file, then build with `dotnet build MoorlandDispatch.csproj -c Release -p:RestoreLockedMode=true`.
5. Provide PLAYBOOK_SQL_CONNECTION_STRING privately for a new empty disposable playbook_ database. Run `dotnet run --project MoorlandDispatch.csproj -c Release --no-build`. This sample creates a table and changes only that database to150 then170; it intentionally requires a supporting SQL2025 test server and database-owner rights. Remove the disposable database afterwards. For older actual engines, adapt the test levels to supported values rather than running the170 phase.
6. In the checkout, use `eng/Test-SqlQueryGuidance.ps1 -Record` to refresh both real compilation/execution records through the authorized isolated route after relevant sample/package changes. Rebuild and pack Playbook, then retrieve this edition through the installed stdio harness.

## Constraints

- SQL Server2025 at compatibility150 is not SQL Server2019 execution. The provider configuration is source-supported for older/lower databases, but those older engines were not executed here. A newer engine may still accept a function at a lower database level; that does not show an older engine can execute it.
- This consumer changes compatibility only to create controlled test configurations. UseCompatibilityLevel itself sends no database-upgrade command. It controls SQL generation and cannot add engine capabilities.
- Query ordering, collation and null semantics belong to the provider. The demonstrated keys are non-null integers. Anonymous composite MinBy keys fail on the pinned RC2 provider, despite a suggestive implementation comment. The sample checks this rejection. Scalar MinBy/MaxBy is not a general custom-comparer or every-provider guarantee. Empty reference-entity results are null; non-nullable scalar/value projections have different empty-sequence behavior.
- Both extrema directions are deterministic because crate counts are filtered to the extreme and the remaining Id key is unique. A one-column tied key leaves the winner unspecified. No performance improvement or transaction-wide snapshot claim is made.
- Stable APIs are verified with prerelease EF11/.NET11 RC2 artifacts. No experimental opt-in is required. No EF11 downlevel runtime route or extra .NET10 foundation is added. No browser/rendering/workload requirement applies.

## Evidence

The paired original MoorlandDispatch projects compile and execute against their declared SDKs: SDK10.0.402/EF10.0.13/SqlClient6.1.7 and SDK11.0.100-rc.2.26504.105/EF11.0.0-rc.2.26504.105/SqlClient7.1.0, both with Pkcs10.0.13. Each locked transitive closure is retained in verification.json and packages.lock.json; generated compilation/execution records hash the actual sources and inherited inputs.

`eng/Test-SqlQueryGuidance.ps1 -Record` creates one unique restricted database per release on SQL Server2025 17.0.5035.5, runs actual commands at150 and170, and removes the databases/logins in finally. It checks selected IDs [11,12], nested TOP at provider150, release-default nested TOP versus LEAST, unchanged database compatibility, real TOP/ORDER BY commands, whole labels, deterministic tied IDs, sync/async command paths and empty entity results. EF10 direct MinBy translation rejection is the red control. These are real database executions, not mocks or SQL-string-only claims.

Official references and exact sample evidence accompany the structured response. Installed-package inspection compares the entire canonical Markdown, applicability and edition offline. It does not establish another database engine, provider, OS or actual GA release. PIN-959 comprehensive coverage and PIN-965 GA provider reconciliation remain separate gates.
