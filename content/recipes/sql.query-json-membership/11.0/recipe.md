# Search JSON content with experimental JsonContains

## Goal

Fen survey notes store a measured depth and searchable tags. Search the selected document path on the database instead of downloading every document. Equality and LIKE search modes answer different questions. JSON_CONTAINS cannot search for SQL null; nullable LINQ collection membership can therefore keep OPENJSON to preserve null matching.

## Constraints

Use .NET11/EF11 for the current approach. The exact proof uses SDK11.0.100 and matching EF11 GA. The previous comparison uses SDK10.0.403/EF10.0.13. SQL path existence needs SQL2022+; containment needs SQL2025 and provider/database compatibility170. The combined executable exercises both on SQL2025 using native json columns. That complete sample requires SQL2025 even when demonstrating the SQL2022 path-existence function. A presence-only application can use textual JSON on SQL2022; actual2022 execution is not claimed. Changing provider compatibility does not upgrade a database.

JsonPathExists is not marked experimental. JsonContains in the pinned EF11 GA package is experimental (EF9106); the sample explicitly accepts this risk with a local pragma. Its API may change or be removed. The current SQL JSON_CONTAINS reference carries no preview label; this does not stabilize JSON indexes or vector features, which are outside this recipe. Pin SqlClient6.1.7/7.1.0 and Pkcs10.0.13 as shown. The latter replaces an older transitive cryptography dependency. No renderer, workload or browser is needed. Exact assets and client/server limitations are in applicability.

## Older approach — .NET10 / EF10

EF10 has no built-in JsonPathExists or JsonContains API. Mapping the existing SQL built-ins with HasDbFunction remains a valid server-side route on a compatible server. These throwing CLR markers must only appear inside translated queries; they are not client implementations. The explicit containment mapping uses search mode zero for equality. Ordinary collection Contains retains OPENJSON. This comparison does not claim containment existed on older SQL engines.

Save all six files together in a standalone FenDocuments directory. Local build/package files deliberately stop repository inheritance. Restore online once, retain the generated packages.lock.json, then build with RestoreLockedMode=true. Serving this guidance requires no network.

**global.json**

{{snippet:n10-global-json}}

**Directory.Build.props**

{{snippet:n10-directory-build-props}}

**Directory.Build.targets**

{{snippet:n10-directory-build-targets}}

**Directory.Packages.props**

{{snippet:n10-directory-packages-props}}

**FenDocuments.csproj**

{{snippet:n10-fendocuments-csproj}}

**Program.cs**

{{snippet:n10-program-cs}}

## Newer approach — .NET11 / EF11

EF11 provides built-in methods, making the custom mappings unnecessary for these operations. JsonPathExists returns a Boolean predicate. The pinned JsonContains API returns int despite SQL permitting NULL. Use it in a predicate compared with 1; do not materialize a potentially NULL result into int. The separate raw SQL assertion is verification of SQL NULL behavior, not a suggested query implementation. Keep the path explicit: the current SQL reference still lists a required path and an array wildcard requirement despite documenting optional syntax. The scalar paths here avoid that ambiguity.

Save all six files together in a standalone FenDocuments directory. Local build/package files deliberately stop repository inheritance. Restore online once, retain the generated packages.lock.json, then build with RestoreLockedMode=true. Serving this guidance requires no network.

**global.json**

{{snippet:n11-global-json}}

**Directory.Build.props**

{{snippet:n11-directory-build-props}}

**Directory.Build.targets**

{{snippet:n11-directory-build-targets}}

**Directory.Packages.props**

{{snippet:n11-directory-packages-props}}

**FenDocuments.csproj**

{{snippet:n11-fendocuments-csproj}}

**Program.cs**

{{snippet:n11-program-cs}}

## Migration

1. Confirm the real engine version and database compatibility before selecting the provider level. Use a disposable database for these examples.
2. Copy the full current project, restore its pinned packages, and preserve the lock file. Keep the previous project separately for comparison.
3. Replace custom SQL function markers with JsonPathExists or JsonContains. Explicitly accept experimental EF9106 only for JsonContains. Use a predicate compared with 1; do not project a possible SQL NULL into int.
4. Map the containment document to native json. Configure primitive collection columns through PrimitiveCollection and retain nullable membership semantics instead of forcing JSON_CONTAINS.
5. Run the executable and check returned IDs against absent/null/empty and matching/nonmatching cases before adapting the fictional survey model.

## Run and interpret the result

Supply a private connection through PLAYBOOK_SQL_CONNECTION_STRING to an empty, exclusively owned disposable playbook_ database. The sample creates FieldNotes and changes that database to compatibility170; never use an existing/shared database. Run `dotnet run -c Release -p:RestoreLockedMode=true` with the release-specific executable. The repository runner creates/removes a unique restricted database per release using the approved local SQL route and stops the container afterward. External readers must arrange their own isolated server; no account or database is provisioned by Playbook.

Presence returns IDs1/2/3: text, JSON null and empty text all have the path. ID4 omits the field and ID5 has SQL NULL. Depth7 returns1/3; LIKE re% returns1; a missing containment path matches no rows when compared with 1, and the separate SQL projection check returns NULL for all five rows. The older nullable custom mapping directly tests IS NULL. JSON null is different from an absent path and from SQL NULL. Ordinary non-null tag membership returns1/4. Nullable element membership using a nullable Wanted column returns1/2/4, including null matches; EF11 retains OPENJSON for that query. Do not replace that fallback with JsonContains when null matching is intended.

The supplied SQL2025 build rejected JSON_CONTAINS over nvarchar(max) with SQL8116, although the current official reference accepts character input. These samples therefore map Document and both collections to native json columns. In EF11 use PrimitiveCollection when configuring the collection column type, preserving its element mapping; scalar Property configuration is not the demonstrated route. Do not infer textual-column containment support from the source alone.

## Evidence and limitations

The .NET 11 SDK baseline is now 11.0.100 GA with preview selection disabled. The .NET 10 comparisons retain their declared SDKs. Use the current sample pins, dependency locks and GA verification records for current coverage. Existing native exclusions and pending Human gates still apply.

`eng/Test-JsonDocuments.ps1 -Record` compiles and executes both locked consumers against real SQL2025/170. Each query checks actual returned IDs as well as translation shape. Saved records identify runtime, SDK, dependency closure and source hashes. No mocks, older-engine execution, cross-compilation, throughput or actual-GA result is claimed. SQL2022 support for presence is official-source evidence; the executable was run on SQL2025. Complete installed stdio list/get checks prove offline content delivery separately from database behavior.

## Official references

- https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-11.0/whatsnew#effunctionsjsonpathexists
- https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-11.0/whatsnew#contains-operations-using-json_contains
- https://learn.microsoft.com/en-us/sql/t-sql/functions/json-path-exists-transact-sql?view=sql-server-ver17
- https://learn.microsoft.com/en-us/sql/t-sql/functions/json-contains-transact-sql?view=sql-server-ver17
- https://learn.microsoft.com/en-us/ef/core/querying/user-defined-function-mapping
