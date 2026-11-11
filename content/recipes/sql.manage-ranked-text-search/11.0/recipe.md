# Manage searchable text and retrieve ranked matches

## Goal

Quarry maintenance notes need ranked word matches. Filtering with a Boolean predicate cannot expose the database relevance rank. Configure a full-text catalog and index, then use the ranked table functions and order explicitly. CONTAINSTABLE accepts a full-text search expression; FREETEXTTABLE performs broader linguistic matching. Rank is relative relevance, not a probability or a stable cross-query score. The result key type must match the unique single-column, non-null full-text key index; this sample uses int. The actual GA result is FullTextSearchResult<int>, carrying Key and Rank, rather than an automatically materialized entity. Join the key back to the entity when you need its text.

## Constraints

The full executable requires SQL Server2025 with full-text installed and English language support, compatibility170 and native vector(3). The vector portion uses exact VECTOR_DISTANCE, not VECTOR_SEARCH, WithApproximate or a vector index. It requires no PREVIEW_FEATURES opt-in. EF vector APIs remain experimental EF9105 and the sample explicitly accepts that risk. Full-text modeling and ranked functions themselves are not marked experimental. No renderer or workload is needed.

Current artifacts are SDK11.0.100 / EF11 GA / SqlClient7.1.0. Previous artifacts are SDK10.0.403 / EF10.0.13 / SqlClient6.1.7. Both pin Pkcs10.0.13 rather than accepting the older transitive dependency. EF11 needs net11.0; no EF11-on-net10 route is claimed. Exact package assets and platform limits are in applicability.

Full-text DDL runs outside explicit transactions. Population is asynchronous: the bounded readiness wait checks actual matches. Production code should treat freshness as an operational concern. Use only an empty, exclusively owned database: the executable creates and removes its schema. It is an authoring check, not a startup migration strategy. Do not expose an unrestricted full-text search grammar unless that is your intended user interface; parameterization prevents SQL injection but does not validate search expressions.

Save the six files for each release together in a standalone directory; the local build/package files stop repository inheritance. Restore online once, retain packages.lock.json and build with RestoreLockedMode=true. The guidance itself is embedded offline.

## Older approach — .NET10 / EF10

EF10 uses explicit parameterized SQL for the ranked functions and full-text DDL. This remains a valid server-side approach.

**global.json**

{{snippet:n10-global-json}}

**Directory.Build.props**

{{snippet:n10-directory-build-props}}

**Directory.Build.targets**

{{snippet:n10-directory-build-targets}}

**Directory.Packages.props**

{{snippet:n10-directory-packages-props}}

**QuarrySearch.csproj**

{{snippet:n10-quarrysearch-csproj}}

**Program.cs**

{{snippet:n10-program-cs}}

## Newer approach — .NET11 / EF11

EF11 models the catalog and index, generates migration commands from the model difference, and exposes ranked table functions in LINQ. The runner executes each generated command outside an explicit transaction, as required for full-text DDL; reverse model differences exercise removal. In a maintained application, scaffold and review migrations normally rather than regenerating an initial schema at startup.

**global.json**

{{snippet:n11-global-json}}

**Directory.Build.props**

{{snippet:n11-directory-build-props}}

**Directory.Build.targets**

{{snippet:n11-directory-build-targets}}

**Directory.Packages.props**

{{snippet:n11-directory-packages-props}}

**QuarrySearch.csproj**

{{snippet:n11-quarrysearch-csproj}}

**Program.cs**

{{snippet:n11-program-cs}}

## Migration

1. Move explicit full-text schema SQL into EF11 model configuration and scaffold a reviewed migration. The model-driven DDL uses nvarchar(450) for Text by convention, whereas the older explicit SQL uses nvarchar(max). Configure and verify the intended column type before applying the migration; narrowing an existing column can fail or put longer notes at risk.
2. Keep the unique key index, language and catalog names consistent. A catalog can serve multiple tables; avoid dropping a shared catalog during a single-table rollback. Both comparisons intentionally own their catalog.
3. EF10 parameterized SQL remains appropriate where upgrading the provider is unnecessary. Predicates remain sufficient when you only need filtering. For a large hybrid corpus, evaluate candidate selection and supported approximate-search syntax separately; this sample establishes neither approximate recall nor index performance.

## Evidence

The .NET 11 SDK baseline is now 11.0.100 GA with preview selection disabled. The .NET 10 comparisons retain their declared SDKs. Use the current sample pins, dependency locks and GA verification records for current coverage. Existing native exclusions and pending Human gates still apply.

Both canonical projects compile and execute on Windows x64 against the authorized Linux Docker SQL2025 CU10 17.0.5035.5 instance. They create full-text structures, wait for actual population, execute both ranked functions and one combined query, assert literal and semantic-only results, and remove the schema. The current sample executes generated forward/reverse model migration commands. These are real database checks, not SQL-string-only assertions or mocks. Input-hashed records retain exact SDKs, dependencies and output. Actual older SQL engines, other client platforms and EF11 GA remain unverified.


