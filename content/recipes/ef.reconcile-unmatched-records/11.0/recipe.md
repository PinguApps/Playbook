# Preserve both sides of a database reconciliation

## Goal

Compare planned and observed records with translated FullJoin while retaining matched, left-only and right-only rows. The fictional MarshSurvey compares field readings and planned surveys without hiding missing records.

## Older approach

Combine a left join with only the unmatched observed rows. Concat preserves multiplicity; using Union could remove legitimate duplicates. The two branches remain one translated query. FullJoin itself is unavailable on .NET10.

{{snippet:n10-query}}

## Newer approach

FullJoin expresses the two-sided reconciliation directly. Null-check both entity inputs before selecting their fields: either side can be absent. The sample uses non-null codes; nullable matching keys and collation need a separately agreed business rule.

{{snippet:n11-query}}

## Migration

1. Choose the previous or current standalone project below. Keep its package boundary files beside the project so repository-wide dependency settings cannot change its pins.
2. Install its exact SDK/runtime and restore MarshSurvey.csproj with the lock file. Run `dotnet restore MarshSurvey.csproj --use-lock-file`, then `dotnet build MarshSurvey.csproj -c Release -p:RestoreLockedMode=true`, using the selected pinned SDK. Package restoration can use the network; Playbook retrieval does not.
3. Supply PLAYBOOK_SQL_CONNECTION_STRING privately to an empty disposable playbook_ SQL Server database. Execute `dotnet run --project MarshSurvey.csproj -c Release --no-build`. Never use a database containing application data: this sample creates its own schema and seed rows.
4. Confirm winner IDs12/21, station names North reedbed/South lagoon and reconciliation pairs A/null, B/B, null/C. The output includes the translated SQL only after successful execution; a SQL string alone is not behavior proof.
5. Remove the owned test database. Repository authors run eng/Test-MarshSurvey.ps1 -Record to create/remove unique restricted databases and capture fresh evidence after source, dependency or pin changes. Rebuild/pack Playbook and inspect both recipes with the installed stdio harness.

### Complete previous project

global.json

{{snippet:n10-global-json}}

Directory.Build.props

{{snippet:n10-directory-build-props}}

Directory.Build.targets

{{snippet:n10-directory-build-targets}}

Directory.Packages.props

{{snippet:n10-directory-packages-props}}

MarshSurvey.csproj

{{snippet:n10-marshsurvey-csproj}}

Program.cs

{{snippet:n10-program-cs}}

### Complete current project


global.json

{{snippet:n11-global-json}}

Directory.Build.props

{{snippet:n11-directory-build-props}}

Directory.Build.targets

{{snippet:n11-directory-build-targets}}

Directory.Packages.props

{{snippet:n11-directory-packages-props}}

MarshSurvey.csproj

{{snippet:n11-marshsurvey-csproj}}

Program.cs

{{snippet:n11-program-cs}}

## Constraints

EF Core11 requires .NET11 and a supporting relational translation. These exact shapes are verified on SQL Server2025; neither SQLite nor every other provider is promised. FULL OUTER JOIN must be supported by the server and translated by the provider. No browser, rendering mode or workload is involved. The provider assets are EF10 lib/net10.0 and EF11 lib/net11.0; SqlClient uses its compatible net9.0 assets as recorded in applicability and locks.

Both query styles select results in the database. The comparison is about composition and clarity, with no throughput claim. Integer scores and unique Id tie breakers define the winner. Seed codes are unique and non-null: duplicate keys in a real full join produce every matching pair, and nullable keys/collations require their own business rules. This sample makes no multi-query snapshot or concurrent-write guarantee. Non-entity missing projections belong to the separate materialization guidance.

The stable API contract is prepared from GA evidence with no experimental opt-in. GA checks are limited to the recorded scope. The historical .NET10 comparison adds no target10 edition or foundation.

## Evidence

The .NET 11 SDK baseline is now 11.0.100 GA with preview selection disabled. The .NET 10 comparisons retain their declared SDKs. Use the current sample pins, dependency locks and GA verification records for current coverage. Existing native exclusions and pending Human gates still apply.

The original paired MarshSurvey projects declare SDK10.0.403/EF10.0.13/SqlClient6.1.7 and SDK11.0.100/EF11.0.0/SqlClient7.1.1; both pin Pkcs10.0.13. Their locks/manifests retain the complete dependency closures. Saved compile-and-run records hash the actual sources and effective inputs.

eng/Test-MarshSurvey.ps1 -Record executes each standalone consumer on Windows x64 against unique SQL Server2025 17.0.5035.5 databases in the authorized local Docker route, then removes its databases/logins and stops the server. Assertions compare actual winner records and related names plus all three reconciliation outcomes. These are provider executions, not mocks, cross-compilation or SQL-only checks. Installed packed-tool inspection compares full canonical Markdown, applicability and selected edition with unusable retrieval proxies; proxy independence is not operating-system network isolation. No other provider, platform, GA release or comprehensive catalogue closure is established.
