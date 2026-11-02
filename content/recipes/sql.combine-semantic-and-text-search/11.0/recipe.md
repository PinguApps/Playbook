# Combine word matches and vector similarity in one ranking

## Goal

A quarry maintenance search needs one list that balances literal material names with nearby vector directions. Keep both signals in SQL and apply sorting and the result limit there. The controlled three-dimensional vectors establish ranking mechanics, not semantic quality.

The score is textRank/1000 + (1 - cosineDistance/2), with a missing text rank contributing zero. Both terms are roughly 0..1 for these nonzero vectors. This is an equally weighted sum, not reciprocal rank fusion; text ranks vary with the corpus, so evaluate weights with actual searches. The left join preserves a note without the word basalt. Exact scanning avoids preview index/search syntax; it may be costly on a large table.

## Constraints

The full executable requires SQL Server2025 with full-text installed and English language support, compatibility170 and native vector(3). The vector portion uses exact VECTOR_DISTANCE, not VECTOR_SEARCH, WithApproximate or a vector index. It requires no PREVIEW_FEATURES opt-in. EF vector APIs remain experimental EF9105 and the sample explicitly accepts that risk. Full-text modeling and ranked functions themselves are not marked experimental. No renderer or workload is needed.

Current artifacts are SDK11.0.100-rc.2.26504.105 / EF11 RC2 / SqlClient7.1.0. Previous artifacts are SDK10.0.402 / EF10.0.13 / SqlClient6.1.7. Both pin Pkcs10.0.13 rather than accepting the older transitive dependency. EF11 needs net11.0; no EF11-on-net10 route is claimed. Exact package assets and platform limits are in applicability.

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

1. Move explicit full-text schema SQL into EF11 model configuration and scaffold a reviewed migration.
2. Keep the unique key index, language and catalog names consistent. A catalog can serve multiple tables; avoid dropping a shared catalog during a single-table rollback. Both comparisons intentionally own their catalog.
3. EF10 parameterized SQL remains appropriate where upgrading the provider is unnecessary. Predicates remain sufficient when you only need filtering. For a large hybrid corpus, evaluate candidate selection and supported approximate-search syntax separately; this sample establishes neither approximate recall nor index performance.

## Evidence

Both canonical projects compile and execute on Windows x64 against the authorized Linux Docker SQL2025 CU10 17.0.5035.5 instance. They create full-text structures, wait for actual population, execute both ranked functions and one combined query, assert literal and semantic-only results, and remove the schema. The current sample executes generated forward/reverse model migration commands. These are real database checks, not SQL-string-only assertions or mocks. Input-hashed records retain exact SDKs, dependencies and output. Actual older SQL engines, other client platforms and EF11 GA remain unverified.


