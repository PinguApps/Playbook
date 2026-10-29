# EF and first-party provider API coverage — PIN-710

Reviewed **26 October 2026**. This is a source/artifact audit, **not delivered
recipes, database execution evidence or permission to publish**. The 31 delivery
drafts below await the existing [PIN-986 review checkpoint][review]; none has
been created as a Linear issue. Unknown extension contracts remain explicit.

## Scope and provenance

[PIN-710][target] owns EF model, query, migrations and public tooling, SQL Server,
SQLite and Cosmos provider surfaces. Primary source: `SPEC.md` at
`0a8989814ea11346354464b845c79c15bb71c5e9`, stories 47, 114–118 and 158–160;
approved proposal `b3778cc`, key `API_EF`. Read the agreed planning README,
implementation/coverage/verification plans, data inventory, glossary, ADRs
0001–0004 and contribution/dependency policies. Root `PLAN.md`, `CONTEXT.md` and
`CONTEXT-MAP.md` are absent. Planning statements that implementation has not
started are historical: the tool, content pipeline and audit conventions exist.

The full scoped overlap query returned **321** issues, including archived work,
in PinguApps team `62f80270-b641-4fa3-b60c-46be4c4df2c8`, Playbook project
`b22380c7-6fde-49f4-8826-e5631dc5df7a`. Both IDs were checked before reading
related issue descriptions. PIN-842–889 are the 48 existing EF/provider delivery
owners; their full criteria remain obligations, not claims of completed content.
PIN-676/677 retain isolated SQL/Cosmos access, PIN-957 retains real Visual Studio
PMC proof, PIN-689 retains global diagnostic-index ownership, PIN-965 retains GA
reconciliation, and PIN-959 retains comprehensive content/evidence closure.
PIN-710 has no native blockers. Shared files and milestone order add none.

Excluded from this bounded artifact comparison: standalone SqlClient/Cosmos SDK
feature inventories, third-party providers and native UI APIs. Dependencies are
inspected only as needed to identify the selected first-party artifacts and read
their metadata. This does not establish a supported restored application graph.

## Reproducible artifact and source boundary

[Inspect-EfApis.ps1](../../../eng/Inspect-EfApis.ps1) reads actual NuGet ZIPs and
assembly metadata without executing inspected code, installing tools, restoring
a project, running SDK setup or changing PATH. [The output](ef.assets.json)
records exact ZIP/assembly/tooling hashes, nuspec repository commits and selected
dependency groups, selected targets, complete public/protected declaration
deltas, whole-surface fingerprints, obsolete/experimental declarations and
shipped PowerShell/MSBuild/runtime-configuration text differences. Package-matched
source differences also cover command definitions and analyzer implementation.
Line-set differences bound review; they do not detect reordering by themselves.
Whole-file hashes retain that distinction.

The selected declaration comparison records **2,227 added and 989 removed raw
lines** (including attribute-only pairs), with **151 previous and 126 current
obsolete/experimental declarations** assigned. It also retains 82 tooling text
comparisons and 55 package-matched source comparisons. Counts describe review
inputs, not numbers of new APIs or recipes. U1 retains 26 annotation/call-shape
lines and U3 retains 653 public/protected extension lines; neither is called closed
semantic migration analysis.

| Baseline | SDK/reference pack | EF and Microsoft.Data.Sqlite packages |
| --- | --- | --- |
| Previous | `10.0.402` / `10.0.13` | `10.0.13` |
| Current | `11.0.100-rc.2.26504.105` / `11.0.0-rc.2.26504.105` | `11.0.0-rc.2.26504.105` |

Twenty selected package IDs per baseline cover Core, Abstractions, Relational,
SQL Server/Abstractions/HierarchyId/NetTopologySuite, SQLite/Core/NetTopologySuite,
Cosmos, Design, Tools, Tasks, Analyzers, Proxies, InMemory, Microsoft.Data.Sqlite/
Core and dotnet-ef. Wrapper packages can have no managed surface. Both Tasks
routes (`net10.0` → `net`, `net472` → `netframework`) are inspected. All other
packaged DLLs are inventoried/hashes retained; their API or execution compatibility
is not inferred from the selected route.

The .NET 10 consumer selects Microsoft.Data.Sqlite's actual `lib/net8.0` asset;
the RC2 package exposes `lib/net10.0`. Their reviewed declarations are identical.
That is metadata evidence, not a runtime check. EF Core itself moves to
`lib/net11.0`; Abstractions and SQL Server Abstractions expose `net10.0` assets.
SQL Server Abstractions, HierarchyId, both spatial plugins, Proxies, direct SQLite
and both Tasks routes have no selected declaration differences. This does not
erase package target/dependency changes or prove unchanged provider behavior.
The new `EFCore.Tools.dll` marker has no public declarations.

Nuspecs identify dotnet/dotnet commits
`4099f83977f4d29604a9aeb971fb0a7bc1d5f699` and
`820230c386db5b0106c5430b3e5aee92a7826bf5`. Metadata resolution additionally
records the pinned SDK's MSBuild assembly hashes and .NET Framework **4.7.2**
reference hashes for full-framework task inspection. The minimum NuGet MSBuild
metadata dependency lacks `IMultiThreadableTask`; resolving the pinned task with
the actual SDK assemblies avoids inventing a successful minimum-version consumer.
No Visual Studio task-host execution was performed.

[The source manifest](ef.sources.json) pins **123 complete official files**:

- dotnet/core `60871330fce2b3da383461be027ebdf3ce30f8fb`: all nine EF Preview
  1–7/RC1/RC2 notes and RC2 `release.json`. No EF/provider files exist in that
  commit's .NET 11 published `api-diff` tree; actual package metadata supplies
the end-to-end boundary.
- dotnet/EntityFramework.Docs `aa051c4c1cad21b2dc03fa401ac0e925c3644606`:
  the **whole** EF11 new-feature, breaking-change (including Microsoft.Data.Sqlite)
  and provider-facing change articles.
- Both package-matched dotnet/dotnet commits: command configuration/implementation,
  dotnet-ef project/startup handling, analyzer releases/resources/implementations,
  model/query/mapping/entry source and Tasks targets. Missing old EF1004 analyzer
  source is a genuine addition, not an unreadable replacement.
- dotnet/docs `5e19883f3aca93e649d9e3fb72b0b89091d7079f`: the complete global
  obsoletion overview, cross-checked against the existing [PIN-689 map](obsolete-index.md).

This boundary is complete at the recorded commits, not a claim about future
documentation, all behavioral changes or final GA artifacts. No external code
example has been republished as an original Playbook sample.

## Accountable dispositions

### Nested scalar and primitive sequence delivery — 29 October 2026

PIN-845 delivers `ef.configure-nested-scalar/11.0`: original EstuaryMarkers
consumers compare EF10 intermediate complex builders with EF11 chained scalar
configuration. Both build the actual nested complex model, verify facets and the
custom column, create SQLite schema and insert/read/update across contexts.
The direct chain was rejected by EF10's model builder during authoring. Complex
properties are explicitly registered; arbitrary collections are not supported by
this recipe. SQLite's maximum-length metadata is not database enforcement.

PIN-848 delivers `ef.configure-primitive-sequence/11.0`: original KilnReadings
consumers verify EF10 `Property` promotion versus EF11 scalar classification,
explicit `PrimitiveCollection` selection, JSON array/TEXT byte-sequence storage
versus BLOB binary storage, and real collection/scalar round-trips. The older
byte-sequence sample already uses the valid `PrimitiveCollection` API. Neither
the method's existence nor all collection discovery is new in EF11. Existing
BLOB-to-JSON data conversion is explicitly outside this disposable demonstration.

Four canonical `samples/net{10,11}.0/{EstuaryMarkers,KilnReadings}` projects carry
locked package graphs and current `verification-evidence.json` records. Actual
Windows x64 execution used SDK 10.0.402 / EF 10.0.13 / SQLite 3.53.3 and SDK
11.0.100-rc.2.26504.105 / EF 11.0.0-rc.2.26504.105 / SQLite 3.53.4. Both resolve
SQLitePCLRaw.bundle_e_sqlite3 3.0.5; no external account or database is needed.
Offline list/get delivery uses the existing packed-tool inspection harness.
No extra foundation, other-provider/native-platform proof, custom-convention or
nested-entry API proof, or actual GA claim is added. The original API audit and
its unresolved extension groups remain unchanged; PIN-965/959 still own their
separate GA/comprehensive gates.

[The rules](ef.dispositions.json) assign every added/removed declaration, including
annotations, defaults, constraints and protected hooks, to a named group. An
unmatched declaration fails the script. Grouping is responsibility for a bounded
subcase, **not** proof that an existing recipe covers it. Residual groups U1–U3
are explicitly unresolved, never a no-change finding or a catch-all delivery ticket.

| Surface or canonical source section | Impact and accountable owner |
| --- | --- |
| Complex types/JSON with TPT/TPC; chained configuration and nested entry access | Authored model/access code: PIN-844/845. Ordinary stability fixes need no second recipe. |
| Complex-path keys, alternate keys, indexes and JSON index paths | Authored model/index code: PIN-846/855. Custom metadata inspection is E17; public metadata churn is not automatic application compatibility. |
| GroupBy winner/navigation composition, FullJoin, MinBy/MaxBy and async variants | PIN-849/850/851. Provider execution and empty/tied/unmatched results remain required; in-memory LINQ proof is insufficient. |
| JSON path existence, containment and indexing | PIN-853/854/855. SQL Server 2022 versus 2025, actual database compatibility and nullable JSON_CONTAINS fallback remain distinct prerequisites. |
| Vector search/index/approximation/default projection and full-text catalogs/indexes/ranking/hybrid results | PIN-856/857/858. Keep experimental SQL/API status. E4 concerns scalar **write** load-state; it does not assume the promised future fluent vector opt-in exists. |
| SQL temporal CLR period properties, visibility, date/time translation and DateTrunc | PIN-859/860. `LocalDateTime` is server-local; hidden-column configuration belongs with row-validity authoring. |
| Ordered SQLite Join/Concat aggregation, UInt128 binding | PIN-861/862. Aggregation needs SQLite **3.44+**. UInt128 is padded 39-digit text binding; no UInt128 reader claim. |
| Direct SQLite target, retired native bundles, unsupported UWP/Xamarin routes | PIN-863/864. Native initialization/encryption/platform proof remains their requirement; no paid vendor selected. |
| Cosmos complex types; automatic/composite/path/vector/full-text indexing | PIN-865/866. Actual account capabilities and stored nested values/policy required. |
| Cosmos Auto/Never/Always batches; bulk writes; token consistency | PIN-867/868/869. Container/partition limits, partial completion and token ownership remain explicit. Automatic retry/performance improvements do not create another goal by themselves. |
| Cosmos removed __jObject/unmapped-field round-trip; id escaping/collision; undefined projection; empty owned collection; numeric truncation; discriminator model name | PIN-870/871/872/873/874/876. Stored `$type` is not renamed. Legacy id switch preserves the collision defect. E9/E10/E29/E30 cover separate uncovered authored contracts. |
| Cosmos removed synchronous I/O | PIN-875. No switch restores it; unsupported internal synchronous members do not form another application recipe. |
| Migration snapshot latest ID, scaffold-and-apply, offline remove, omitted FK constraint, empty migration assembly, command config, PMC NoBuild, direct Design reference, wildcard contexts, clean stdout, generation stages | PIN-877–887 respectively (constraint omission PIN-880; empty assembly PIN-881). Real PMC proof stays PIN-957. E15/E19/E26–28 are separate goals, not silent additions to these tickets. |
| Owned JSON ordinal-key warning; primitive collection creation-time semantics; split-query concurrency exception | PIN-847/848/888. Scope is collection/non-shadow-key specific. Snapshot/serializable/single-query advice needs actual provider guarantees; the source's blanket single-query consistency wording is not a general isolation guarantee. |
| SqlClient 7/Azure-auth dependency split and SQL compatibility default 160 | PIN-842/843. Their existing source-finding and real identity/database acceptance is not repeated by this audit. |
| Named filter foundation | PIN-889; retain the exact EF9/10 comparison and tenant-filter limits. No ninth foundation inferred from removed metadata helpers. |
| GetEntriesForState, registration/options replacement, pooled factory overload, scalar IsLoaded | E1–4: authored code beyond the cumulative summary. Defaults and change-detection/pool/write semantics must be demonstrated. |
| Removed AutoTransactionsEnabled, collection-mode helpers, raw SQL obsoletions/arguments, UseAzureSqlDefaults, singular partition-key helpers | E5–9: authored migrations; supported replacements can already exist in EF10. Do not portray them as newly invented EF11 capabilities. |
| ContentResponseOnWriteEnabled nullable/default change and new obsoletion | E10. Source-backed warning says enabling it currently brings EF no benefit; not a statement about all direct Cosmos SDK uses. |
| Generic type mapping/default comparer, structured JSON update paths, value generators, materializer/query-root hooks, constructor binding, metadata traversal, ignore conventions, scaffolding selector | E11–14, E16–18, E31. These are extension-author consumers, with exact actual compile/behavior prerequisites, not imaginary application-level features. Residual hooks remain U3. |
| Better join/order pruning, redundant CAST/NULLIF/null-check removal, automatic retry paths, generated file-scoped namespaces, index DROP_EXISTING and computed/TPC sequence ordering fixes, WinRT/Wine initialization and RC1/RC2 fixes | Excluded as independent goals when only upgrading fixes unchanged code/output. Existing feature owners still test affected shapes. New authored query forms are E20–25/E30; an actual removed workaround must be demonstrated before claiming one. |
| Moved IOperationReporter/ISnapshotModelProcessor, query SQL visitor/collation/type-mapping extension changes and provider specification-test reorganization/xUnit v3 | Public/provider extension obligations retained in U3, E11/12/17/31. Not erased by release notes calling them plumbing. Test-package compatibility remains unverified; do not copy a new runner command without a real provider test project. |
| Diagnostic strings/resources, public `.Internal` support, compiler state-machine annotations | No standalone recipe from implementation scaffolding or compiler-generated state type names. Full raw deltas remain. U1 requires exact annotation/call-shape adjudication; EF1001 warns about unsupported internals, not permission to ignore documented public/protected extension changes. |

### Obsolete and public-tooling ownership

PIN-689's global SYSLIB/EXTOBS index introduces **no EF-specific custom diagnostic**.
That does not cover default CS0618/CS0619 or removed EF APIs. The artifact keeps
every old/current obsolete declaration rather than checking only custom IDs.

- Removed application/configuration helpers map to E5–9; removed metadata,
  value-generator, binding and selector helpers map to E11–18/E31 or U3.
- New FromSqlInterpolated/ExecuteSqlInterpolated warnings map to E7. New
  ContentResponseOnWriteEnabled warning maps to E10. JsonTypeMapping's structural
  replacement maps to E11. Element-type mutation/configuration-source obsoletions
  retain PIN-848, with custom convention consequences visible in U3.
- Existing formatter-based exception constructors retain SYSLIB0051/PIN-704;
  unchanged EventSource-to-metrics advice is historical, not a new .NET11 goal.
  Existing table-scoped check/comment/index-name advice remains historical where
  its declaration survives unchanged. Removed counterparts stay U3 until their
  exact supported replacement is adjudicated. Compiler compatibility sentinels
  for required members/byref-like types are not runtime deprecation recipes.
- Analyzer release records retain EF1000 retired, EF1001 internal-API warning,
  EF1002/EF1003 raw-query security warnings and add **EF1004**. EF1004 belongs to
  PIN-852's async adapter goal. Expanded EF1003 string.Format/string.Concat
  recognition belongs to E7's parameter-preserving raw SQL migration. Descriptor
  source/hashes do not establish compiler diagnostics or safe execution.
- Actual Tasks properties/targets show removal of EFOptimizeContext and retained
  stage controls: PIN-887. Tool/Tasks dependency removal stays PIN-884. Shipped
  PMC scripts expose NoBuild, connection/offline and add/apply parameters; retain
  PIN-878/879/883 and actual interactive PIN-957 proof.
- dotnet-ef command/startup source adds file/startup-file selection (E19), config
  defaults (PIN-882), supported context wildcards (PIN-885), and startup output
  metadata handling (E27). Runtimeconfig/tool folders change from net8/net10 to
  net10/net; this is not proof that EF11 runs on net10.

### Source and artifact drift that must survive delivery

1. The cumulative feature page omits several Preview 3–7 authored APIs and
   translations. All nine complete component notes and package deltas were used.
2. Early full-text/vector builder names and SqlServerFullTextSearchResult
   namespaces changed during previews. Existing owners must compile the RC2
   signatures, not teach a preview-to-preview migration as a stable-release goal.
3. The provider article calls `IColumnModification.JsonPath` a new `JsonPath`
   object, while the actual property type is **StructuredJsonPath**. E12 uses the
   actual asset. Source-name disagreement is retained rather than papered over.
4. Preview 6 says the default SQLite dependency changed to SQLite3MC; the complete
   breaking article also describes bundle_e_sqlite3 2.1.12. Actual RC2 wrappers
   depend on **SQLitePCLRaw.bundle_e_sqlite3/core 3.0.5**. PIN-864 must choose and
   execute its explicit native route; no package identity implies encryption
   compatibility.
5. Documentation promises a later vector auto-load mechanism. RC2 exposes
   PropertyEntry.IsLoaded and mutable metadata flags, but that alone establishes
   no supported fluent loader. PIN-856's explicit projection stays valid; E4 must
   prove its separate excluded-write behavior. Runtime results remain unmeasured.
6. SQL Server's actual RC2 dependency is **Microsoft.Data.SqlClient 7.1.0**, not
   the feature article's 7.0 headline. Cosmos depends on **Microsoft.Azure.Cosmos
   3.63.1** and still directly lists **Newtonsoft.Json 13.0.4**, despite prose
   saying the provider no longer depends on Newtonsoft. PIN-842/870 must verify
   their actual consumers; do not remove a package reference or infer raw-document
   behavior from that prose alone. These artifact facts refine prerequisites,
   without redoing those owners' authentication or data-loss analysis.
7. Cumulative wildcard guidance mentions migration bundles; package-matched
   MigrationsBundleCommand explicitly rejects `--context '*'` during bundle
   creation. Distinguish creation from execution and prove the advertised route
   before PIN-885/E26 documentation claims multi-context bundle support.

## Delivery drafts for PIN-986 review

Each numbered item is a **proposed Agent ticket**, not an approved/new issue.
Use milestone `05 - Data access recipes`, existing `Agent`/feature labels and
native T-shirt values **S (2), M (3), L (5)**. Prefix/title allocation and native
relations happen only after approval. Each draft's Source is PIN-710 plus the
specific named manifest entry/API and this proposal ID. Existing estimates are
not resized. Suggested dependency edges below are requirements of the proposed
work, not new blockers on this audit.

**Common acceptance for every draft:** wholly original fictional-domain EF10/11
samples and complete offline recipe metadata/Markdown; exact SDK/package/target/
provider/server/native pins; actual older/current compilation; the stated real
behavior check including failure controls; saved freshness/compilation evidence;
ordinary packed-tool install and real stdio discovery/full body/applicability
from an isolated feed/cache and unrelated directory. Preserve experimental labels,
safe disposable data/cleanup and honest older alternatives. All require the
existing tool foundation PIN-668. SQL/Cosmos access dependencies are PIN-676/677
where named; existing SQLite native-route delivery PIN-864 is required when used.
Source review or translated SQL alone cannot meet these acceptance criteria.

### E1. Select tracked entries without an extra detection pass — M (3)

As a developer, I want to inspect known tracked states without triggering another
scan. Proposed `ef.select-known-tracked-states`; GetEntriesForState generic and
non-generic overloads. Prerequisites: pinned Core, SQLite native route. Compare
Entries filtering with explicit DetectChanges plus state selection; mutate a
tracked object before/after detection, test every Boolean default (all false),
and persist the intended changes. M includes state/correctness and consumer proof.

### E2. Replace a configured database provider deliberately — M (3)

As a test author, I want a separate database configuration without retaining the
production provider. Proposed `ef.replace-context-provider`; RemoveDbContext,
removeConfigurationOnly and WithoutExtension/RemoveExtension. Use isolated DI
containers and SQLite/InMemory; inspect resolved options/services, run actual
CRUD and reject mixed/retained configuration. No production connection. M covers
registration lifetimes and failure controls; InMemory is not relational proof.

### E3. Reuse shared configuration in a pooled factory — M (3)

As a developer, I want pooled contexts to use the configuration I already
registered. Proposed `ef.reuse-pooled-factory-configuration`; new no-options-action
overload. SQLite route; prove ConfigureDbContext options are consumed, contexts
are disposed/reused and request-specific state does not leak. Do not put setup in
OnConfiguring or infer pooling safety from registration alone. M includes lifecycle.

### E4. Avoid saving a scalar value that was not loaded — M (3)

As a developer, I want partial records to preserve database values I did not read.
Proposed `ef.preserve-unloaded-scalar-values`; PropertyEntry.IsLoaded. SQLite
route, actual SELECT/UPDATE/save/reload with loaded/unloaded/default-valued
controls; prove the write omission and re-enabling behavior. Reproduce the older
IsModified-based alternative. M covers persistence and state hazards; no vector
fluent-loader claim, security or general partial-entity guarantee.

### E5. Preserve relational automatic-transaction intent — M (3)

As a maintainer, I want an upgrade to preserve when a save creates a transaction.
Proposed `ef.replace-transaction-boolean`; removed AutoTransactionsEnabled to
AutoTransactionBehavior. SQL access; compare Never/WhenNeeded/Always against the
actual old property semantics, including a forced mid-save failure and persisted
rows. M includes rollback evidence. Cosmos transaction modes stay PIN-867.

### E6. Preserve collection-parameter translation configuration — M (3)

As a maintainer, I want collection query settings to keep their intended SQL form.
Proposed `ef.migrate-collection-parameter-mode`; removed two TranslateParameterized
helpers to existing UseParameterizedCollectionMode. SQL access, seeded actual
Contains queries, constants/parameter controls and result equivalence. Inspect
executed SQL, without a query-plan/performance guarantee. M includes provider modes.

### E7. Migrate raw SQL calls while preserving parameters — M (3)

As a developer, I want to replace outdated query calls without changing how input
is passed to the database. Proposed `ef.retain-parameterized-raw-sql`; new warnings,
FromSql/ExecuteSql replacements and EF1003 expanded recognition. SQLite route;
compile actual old/new warning diagnostics, inspect command parameters, run
metacharacter-bearing values and cancelled async calls without concatenating
untrusted text. M includes negative security controls; no claim that a rename
sanitizes arbitrary raw SQL.

### E8. Replace the removed Azure SQL defaults helper — M (3)

As a maintainer, I want Azure SQL setup to use the supported entry point.
Proposed `sql.migrate-azure-provider-configuration`; UseAzureSqlDefaults to
UseAzureSql. SQL access must identify an authorized Azure-compatible test route,
exact retry/compatibility settings and actual connection/query evidence. Entra
identity uses existing PIN-679/958 if that route needs it; otherwise use the
authorized authentication mode. M covers configuration, not new infrastructure.

### E9. Preserve partition keys when replacing singular Cosmos helpers — M (3)

As a maintainer, I want updated model code to locate the same documents.
Proposed `cosmos.migrate-partition-metadata`; singular getters/configuration to
supported plural metadata/builder APIs. Cosmos access; compile old/new model code,
inspect key order, seed/query/update the same isolated documents and reject an
incorrect key. No data repartitioning is implied. M includes identity/persistence.

### E10. Remove ineffective EF write-content configuration — S (2)

As a maintainer, I want to remove an obsolete option without changing saved data.
Proposed `cosmos.remove-write-content-option`; ContentResponseOnWriteEnabled.
Cosmos access; compile old/current warning/default/null cases and execute
save/reload/concurrency-token checks before/after removal. S is one option with
service proof; no direct-SDK-wide performance claim.

### E11. Migrate a custom scalar type mapping and comparer — L (5)

As an extension author, I want custom values to survive the changed mapping
contracts. Proposed `ef.migrate-generic-type-mapping`; generic Core/Relational
mapping, Clone signature, ValueComparer defaults and structural JSON distinction.
SQL access; one original scalar mapping, actual model/query/save/round-trip and
mutation/equality/null controls. Establish supported extension and constructor
contracts before authoring. L includes both compiled versions and provider behavior;
NativeAOT claims require separate actual native publish/run, not inferred support.

### E12. Generate partial JSON updates using structured paths — L (5)

As an extension author, I want nested updates to address the intended JSON value.
Proposed `ef.migrate-structured-json-update-path`; actual StructuredJsonPath in
IColumnModification/ColumnModificationParameters. SQL access with supported JSON;
compile an original bounded update-generator extension, execute root/nested/array/
escaped-name updates and verify untouched siblings. L includes extension/migration
risk. Resolve the source type-name disagreement using the pinned assembly.

### E13. Replace removed value-generator selectors — M (3)

As an extension author, I want custom generated values to keep working after an
upgrade. Proposed `ef.migrate-generator-selection`; Select/Create to TrySelect/
TryCreate, including no-generator failure. SQLite route; compile/register one
original generator, save multiple entities and verify generated keys and failure
behavior. M covers selection and persistence; no broad provider rewrite.

### E14. Replace legacy materializer/query-root extension hooks — L (5)

As an extension author, I want my custom materialization hook to use the supported
structural contracts. Proposed `ef.migrate-structural-materializer`; entity-only
interfaces/parameters, query parameter root and collection-result counterparts.
SQLite route; first identify the exact callable old hook removed/changed, then
compile a bounded original extension and materialize entity/nested value/null
results. Never revive an already error-obsolete API in the comparison. L covers
extension validity; if no supported consumer exists, return that concrete evidence
to PIN-986 rather than inventing an example.

### E15. Retain rows whose modeled principal may be missing — M (3)

As a developer, I want reads to retain dependent records in an intentionally
unenforced relationship. Proposed `ef.query-unconstrained-relationship`;
IsConstrained(false), distinct from ExcludeForeignKeyFromMigrations. SQL access;
inspect generated constraints/indexes and execute queries with a missing principal,
comparing required-inner versus unconstrained-left joins. M covers schema/query
semantics. Keep the integrity responsibility explicit; do not resize PIN-880.

### E16. Migrate a custom constructor-binding consumer — M (3)

As an extension author, I want my constructor selection to use the current binding
contract. Proposed `ef.migrate-constructor-binding`; removed TryBindConstructor
to GetBindings. SQLite route; compile the actual old/new supported extension and
materialize an original type with constructor/service parameters plus an invalid
binding control. M includes extension support and execution, not a generic mapper.

### E17. Inspect mapped columns across entity and complex paths — L (5)

As a tooling author, I want metadata inspection to report the correct nested key,
index and store column. Proposed `ef.inspect-structural-model-mappings`; removed
DeclaringEntityType/column-base/container-mapping methods and broadened property
lists. SQLite route for entity/complex/table/view inspection, SQL access for any
advertised native JSON-index route. Compile old/new inspectors and verify names,
ordering, store mappings and executed lookups. L covers changed metadata shapes;
no claim to migrate every convention/provider hook in U3.

### E18. Migrate a custom type-ignore convention — M (3)

As an extension author, I want my ignore rule to handle the current type callback.
Proposed `ef.migrate-type-ignore-convention`; IEntityTypeIgnoredConvention to
ITypeIgnoredConvention and removed RemoveAnnotation to HasNoAnnotation where used.
SQLite route; compile/register the actual convention and assert included/ignored
model types plus persisted expected schema. M includes convention-order controls.

### E19. Run EF design-time commands for a file-based app — M (3)

As a developer, I want database tooling to target my single-file app correctly.
Proposed `ef.target-file-based-app`; --file/--startup-file and mutually exclusive
project selection. Pinned SDK/tool/Design packages, SQLite route; actual file app
build and migrations add/list/script/update, separate startup-file and invalid
argument controls. M includes generated artifacts, not only --help; shared
file-app authoring PIN-759 does not prove EF design-time behavior.

### E20. Keep collection-existence predicates on the database — M (3)

As a developer, I want supported list/binary predicates to execute where the data
lives. Proposed `ef.translate-collection-existence`; List.Exists and byte[].Any
translations from Preview 5/6. SQLite route plus SQL access for each advertised
provider. Run actual populated/empty/null cases, inspect executed commands and
compare older Any/rewrite alternatives. M covers bounded query shapes; do not
repeat preview notes' blanket client-evaluation wording for EF10 predicates.

### E21. Parse stored numeric text inside a SQL query — M (3)

As a developer, I want numeric text parsed by the database without fetching every
row first. Proposed `sql.translate-numeric-text-parse`; supported single-string
Parse overloads to CAST. SQL access; compile/run original old/new server queries,
test invalid text, null, culture/overflow and filtering controls. M includes
server-versus-CLR differences, not a promise that SQL accepts every .NET format.

### E22. Filter SQLite rows by time components — M (3)

As a developer, I want to query time components without reading all rows into
memory. Proposed `sqlite.query-time-components`; TimeOnly.Hour/Minute/Second.
SQLite route, exact native version; actual stored boundary/precision values,
executed strftime queries and old supported alternative. M includes result and
translation evidence; SQL Server date guidance PIN-860 is not SQLite coverage.

### E23. Store half-precision values in SQLite — M (3)

As a developer, I want smaller-precision measurements stored and read deliberately.
Proposed `sqlite.persist-half-values`; low-level binder/reader and EF mapping,
including JSON only if actually supported. SQLite route; old/current direct and EF
samples, actual round-trip/precision/null/range controls and exact storage type.
M includes native execution. UInt128 binding remains PIN-862 and is not Half proof.

### E24. Compose database queries over tuple-valued projections — M (3)

As a developer, I want grouped tuple keys to remain usable in later query steps.
Proposed `ef.compose-tuple-projections`; ValueTuple/Tuple constructor-member fold.
SQLite route; actual grouped/joined keys, nested Rest and arity controls, compare
older anonymous-type alternative. M covers bounded translation shapes; arbitrary
record constructors are explicitly excluded by the source.

### E25. Preserve missing non-entity join results — M (3)

As a developer, I want an unmatched join to return the intended null/default result.
Proposed `ef.materialize-missing-projection`; DTO/anonymous/nullable struct inner
projections, not just entity FullJoin or top-per-group navigation. SQLite route;
seed matched/unmatched rows, execute actual joins and subsequent composition,
verify null/default and compare the old explicit-marker workaround. M includes
materialization failures. PIN-849/850 remain their existing distinct goals.

### E26. Produce a migration bundle with central package management — L (5)

As a maintainer, I want a deployable migration bundle without conflicting package
versions. Proposed `ef.bundle-centrally-managed-migrations`; Preview 7 NU1008 fix.
Pinned SDK/tool/Design/CPM and SQLite route; real bundle generation and execution
on an isolated file database, inspect temporary project's package selection and
non-CPM control. L includes deployable artifact/runtime proof; automatic template
fix alone does not deliver guidance or establish a removed workaround.

### E27. Run design-time tooling with customized output names — M (3)

As a maintainer, I want database tooling to start the output my build actually
produces. Proposed `ef.use-custom-startup-output-metadata`; ProjectDepsFileName /
ProjectRuntimeConfigFileName rather than AssemblyName guesses. SQLite route;
build real startup/target projects with deliberate output-name differences, run
migrations/list/update and prove the selected artifacts. M includes build/design
time integration and missing-file failure; no hand-written fake deps file.

### E28. Reconcile a snapshot made by an older EF version — M (3)

As a maintainer, I want to distinguish an old snapshot from a real schema change.
Proposed `ef.reconcile-old-snapshot-version`; OldMigrationVersionWarning.
SQLite route; actual old/current snapshot, matching model and genuine-change
control, run migration validation and verify deliberate regenerated artifacts.
M covers diagnostic cause and schema outcome. Branch-divergence/latest-ID handling
stays PIN-877; never suppress all pending-model checks.

### E29. Keep keys in Cosmos identity-resolution projections — M (3)

As a developer, I want nested projections to retain the identity data they need.
Proposed `cosmos.project-identity-resolution-keys`; Preview 7's changed owned
projection requirement. Cosmos access; actual old/current owned projection with
AsNoTrackingWithIdentityResolution, omitted-key failure and explicit-key success,
duplicates and stored-document controls. M includes service materialization;
complex value mappings PIN-865 and raw fields PIN-870 do not imply this coverage.

### E30. Compose Cosmos numeric conversions in server queries — M (3)

As a developer, I want supported converted values to be usable in document queries.
Proposed `cosmos.translate-converted-expressions`; Preview 6 Convert/math/string
composition. Cosmos access; actual bounded conversion/concatenation query, null,
invalid/range cases and older explicit client alternative. M includes service
semantics; rounding/truncation migration stays PIN-874, without duplicating it.

### E31. Select generated model code using explicit options — M (3)

As a tooling author, I want model code generation to use the supported selector
contract. Proposed `ef.migrate-model-code-selector`; IModelCodeGeneratorSelector
removed overload to ModelCodeGenerationOptions. Actual Design consumer, SQLite
route and pinned code-generation dependencies; compile old/new extension, scaffold
an original schema, compile generated C# and query it. M includes real generated
consumer validation; generation-stage choices remain PIN-887.

## Retained unknowns and publication boundary

- **U1 — annotation and call-shape residuals.** The artifact keeps every exact
  added/removed line. Compiler state-machine names and async implementation
  attributes can differ without removing a callable API; nullability, trimming,
  unsafe/default/argument contracts must not be normalized away. PIN-710 owns this
  audit classification; PIN-986 must reconcile any qualifying consumer found
  before PIN-959 can claim comprehensive closure.
- **U2 — direct SQLite differences.** The actual selected net8/net10 declarations
  are identical. Half/UInt128 support changes bind/map behavior without a new
  direct public method. The source-backed E23/PIN-862 obligations remain;
  identical API shape is not execution evidence or a no-content conclusion.
- **U3 — residual extension contracts.** The finite raw lines/counts retain
  ModelValidator/dependency/member-classifier changes; remaining model/convention
  and JSON relational-element metadata; query SQL visitors/collation quoting,
  SQL-expression and command builder hooks; scaffold/snapshot generator parameters;
  reporter/snapshot-processor moves and runtime model services. These are neither
  blanket exclusions nor one unknown-sized implementation ticket. PIN-710 owns
  identification; PIN-986 must select supported original consumer goals, split
  them into bounded estimated delivery work and retain any unavailable contract.
  Provider test-package/xUnit3 compatibility additionally needs actual packages
  and a real provider test project. No minimum-version, provider-plugin or AOT
  compatibility assertion is made from these metadata dependencies.

The listed proposals are source/metadata-backed plans for **fully verified future
recipes**; they have not been compiled or executed. Audit completion cannot
satisfy them. PIN-986 approval/ownership, all qualifying delivery/evidence, the
remaining existing EF corpus, targeted Human proofs and PIN-959 remain publication
requirements. PIN-965 must compare actual final GA sources/assets rather than
relabeling this RC2 observation. No related Linear issue was changed by this audit.

## Reproduce and inspect

Prerequisites: PowerShell 7, HTTPS access to the pinned NuGet/official source URLs,
both exact SDK/reference roots above and the installed .NET Framework 4.7.2
reference assemblies. Parameters allow alternate roots containing the **same**
pinned versions. The command does not install them or run inspected assemblies.
Executed from an unrelated directory:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-3a778fef'
& ./eng/Inspect-EfApis.ps1 `
  -OutputPath ./artifacts/ef.recheck.json `
  -CompareWith ./docs/planning/audits/ef.assets.json
$audit = Get-Content -LiteralPath ./artifacts/ef.recheck.json -Raw | ConvertFrom-Json
$audit.publicApiDiff | ForEach-Object {
  '{0}: +{1} -{2}' -f $_.asset, $_.diff.added.Count, $_.diff.removed.Count
}
$audit.dispositions | Format-Table asset, side, group, count
```

Expected: exact package/tooling/public-delta ownership and 123 immutable source
hashes reproduce, with no unowned declarations. A changed source/artifact or
ownership rule fails the comparison. This is not a build, service query, Visual
Studio session, native bundle run or installed recipe test. Those are deliberate
future acceptance requirements above, not unavailable prerequisites for this
source-only audit. No application/database settings changed; disposable output is
under ignored `artifacts/`.

[target]: https://linear.app/pinguapps/issue/PIN-710
[review]: https://linear.app/pinguapps/issue/PIN-986
