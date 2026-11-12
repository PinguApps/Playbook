# GA EF and provider reconciliation

## PIN-1311/PIN-1312 delivery update

The historical register below retains its original audit inputs and hashes.
PIN-1311 adds `sqlite.index-json-path/11.0`: independent KestrelCrates
SDK10.0.403/EF10.0.13 and SDK11.0.100/EF11.0.0 consumers apply SQLite
migrations, inspect expression-index schema, and verify scalar/position lookups.
EF11 checks migration per-element ordering, constructor null/empty/mixed sort
semantics, count mismatch and all-elements rejection. SQL Server's unsupported
options remain a separate source-reviewed contract. No performance claim follows.

PIN-1312 reproduces the clean automatic model-build DLL-not-found failure and
withdraws that successful-generation route. The bounded alternative generates
a conventional model with dotnet-ef11.0.0 before publish, then generates query
interceptors with Tasks11.0.0. Both compile and the published Windows x64 SQLite
app verifies compiled-model loading and exact results. Run
`eng/Test-FernGeneration.ps1 -Record` for the input-bound generation receipt and
generated-source hashes; ordinary compilation receipts remain separate.
Normal verification generates in a disposable copy because EF assigns a fresh
model identifier; only `-Record` regenerates the canonical model and receipt.
NativeAOT and other platforms are not-tested. Experimental labels remain.

Only Fern's resolved pending-proof/preview exception is removed. PIN-1313,
PIN-1314, PIN-967, other Human gates and the eight foundations remain unchanged.
Generated model C# and migrations remain reviewable because their mapping/schema
meaning merits review; no new PR Agent exclusion is added.

PIN-965, 12 November 2026. This is a bounded release-decision audit, not
database execution proof or permission to publish. PIN-967 remains open and
the four follow-ons below block its final stable-candidate check.

## Authority and complete family boundary

Scope follows SPEC.md at `0a8989814ea11346354464b845c79c15bb71c5e9`,
proposal `b3778cc` (`GA_DATA`), ADRs 0001–0004 and the release plan.
Root PLAN.md, CONTEXT.md and CONTEXT-MAP.md are absent. The planning statement
that implementation has not started is historical. The existing
[EF audit](ef.md), source manifest and dispositions retain their original scope.
All PIN-965 native delivery prerequisites are Done in PinguApps team
`62f80270-b641-4fa3-b60c-46be4c4df2c8`, Playbook project
`b22380c7-6fde-49f4-8826-e5631dc5df7a`. Done does not establish deferred proof.

The [generated register](ga-ef-contracts.sources.json) binds all **81** prepared
editions selected by `ef-core`, `sql-server`, `sqlite` or `cosmos`: 80 .NET 11
editions and the retained named-filter .NET 10 foundation. Four editions remain
experimental; 77 are labelled stable. IDs, metadata and Markdown hashes bind the
whole family, including extension-author, migrations/tooling and direct SQLite
goals, rather than only recent deliveries. The other seven foundations remain
outside this family. No served content, sample pin, receipt or release gate changes.

## Actual GA versus retained RC evidence

The official [release feed](https://builds.dotnet.microsoft.com/dotnet/release-metadata/11.0/releases.json)
and pinned GA release notes identify SDK 11.0.100 / release 11.0.0.
The actual NuGet ZIPs are downloaded and inspected by
[Inspect-EfApis.ps1](../../../eng/Inspect-EfApis.ps1), not inferred from a
release-note title. The generated register records package hashes, nuspec source
commits, dependency groups and the actual report hash. Twenty first-party package
IDs include EF Core, first-party providers/plugins, Design, Tools, Tasks, analyzers,
dotnet-ef and Microsoft.Data.Sqlite; metadata-only dependency resolution adds
their required references. This is not an application restore compatibility test.

The repository's `ef.assets.json` has GA SDK labels but still records RC2 EF
packages. It is preserved. Comparing fresh GA output with that file **fails**, as
expected for changed packages/dependencies/API declarations; it is not a successful
baseline reproduction. The explicit immutable RC report from commit
`b7b6c2edc489fe264efe0d559a526a676dca7686` additionally preserves SDK RC2,
so a later SDK-only refresh cannot conceal the RC-to-GA boundary.

All **123** retained official source hashes are independently checked before
comparison: 90 files are byte-identical and 33 differ. All 55 package-matched
RC2 source files in the retained manifest are unchanged at GA. Changed files
are the older-control source comparisons and the global obsoletion overview;
these are not newly introduced GA authoring goals. Complete EF11 what's-new,
breaking-change and provider-facing articles are unchanged at the pinned final
documentation revision. Additional package-matched JSON-index source files and
the GA release notes are hashed separately, because unchanged selected RC files
do not establish whole-source equality.

Three public-API delta groups introduce JSON-index changes. Twelve tooling-text
groups retain packaged PowerShell/dependency-text differences, including old
control and package-version changes. They are review inputs, not twelve new goals.
The older comparison changes SDK/reference packs; SDK/ref and text differences
must not be attributed to new GA EF behavior without supporting source/artifacts.

## Availability, provider requirements and dispositions

| Surface | Final artifact decision | Owner |
| --- | --- | --- |
| SQLite JSON indexes | GA migrations generate JSON-path expressions and per-element DESC; all-elements paths are rejected. This is outside PIN-855's SQL Server goal. New SQLite generator override and validator/diagnostic declarations account for this capability. | [PIN-1311](https://linear.app/pinguapps/issue/PIN-1311), L (5) |
| Relational JSON-index metadata | Constructor gains optional sort-list argument; IsDescending and IsElementDescending carry element-level rather than deduplicated column-level order. Null, empty and mixed lists require real consumer controls. The new mismatch diagnostic is validation, not another standalone goal. | PIN-1311; existing PIN-1205 remains its delivered structural-mapping scope |
| SQL Server JSON index validation | New Internal diagnostic rejects unsupported index options; an Internal resource accessor is not a supported application API. Preserve PIN-855's native JSON/service limits; do not assume SQLite ordering is available on SQL Server. | PIN-1311's provider contrast, PIN-1314's applicability reconciliation |
| Generated model/query stages | Fern still records unsuccessful GA generation and pending generated-consumer proof. Keep experimental labels and the explicit limitation. Compilation with generation disabled does not satisfy generation. | [PIN-1312](https://linear.app/pinguapps/issue/PIN-1312), L (5), related to PIN-887 |
| Missing non-entity projection | Plover records GA direct-projection failures and a working nullable-marker fallback. Documentation intent is not successful shipped materialization. | [PIN-1313](https://linear.app/pinguapps/issue/PIN-1313), M (3), related to PIN-1213 |
| Stale requirements/source labels | DuneArchive prose still says SqlClient7.1.0 while its actual GA lock uses7.1.1. Direct SQLite recipes call RC2 NuGet URLs GA assets; time-component guidance still calls PIN-965 outstanding. Correct source/applicability wording without relabelling historical receipts. | [PIN-1314](https://linear.app/pinguapps/issue/PIN-1314), M (3), related to PIN-855/863/864/1210 |
| Remaining catalogue | Retain existing behavior evidence and source dispositions; unchanged source/metadata does not rerun or upgrade any receipt. No additional authored goal is identified by this bounded comparison. | PIN-967 final candidate/freshness gate |

EF11 selects `net11.0`; direct Microsoft.Data.Sqlite11 retains its distinct
lower-target route. SQLServer11 requires SqlClient7.1.1 and Pkcs10.0.13;
Cosmos11 requires SDK3.63.2 rather than RC's3.63.1. SQLitePCLRaw core/bundle3.0.5
and SQLite3.53.4 remain separate dependencies. Metadata inspection does not
establish server compatibility, credentials, authentication, emulator capability,
native loading, SQL generation execution or a supported complete restored graph.
Existing sample locks already contain GA dependency updates; their receipts remain
limited to their saved commands/inputs. PIN-1314 reconciles stale prose against them.

Preserve four experimental labels: `sql.search-vector-index`,
`sql.query-json-membership`, `sql.combine-semantic-and-text-search` and
`ef.configure-generated-model-stage`. Neither an independent database feature's
stability nor MSBuild/precompilation experimental status is graduated by host GA.
The approved unavailable vector-search execution exclusions remain unchanged;
this audit creates no new cloud or hardware obligation.

The four follow-ons are Todo in current cycle 5 (9–16 November 2026), explicitly
chosen by Pingu, milestone 05. They relate to PIN-965 and block PIN-967.
Estimates include implementation, focused real checks, evidence and installed
offline guidance. Existing completed deliveries remain completed. PIN-965 retains
L (5) for the source/artifact reconciliation. Stable publication must wait for
these follow-ons and PIN-967's final freshness/behavior requirements.

## Reproduce

Run with PowerShell 7 and the actual installed SDK roots below. These commands
download/inspect metadata and official source; they do not execute samples.
The older portable SDK supplies the pinned metadata reader/reference assemblies.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-67fb7898-6f79a125e29d0840'
$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
$older = 'V:\_worktrees\Playbook\t3-1ead8027\artifacts\dotnet10'
$ga = 'C:\Users\pingu\AppData\Local\dotnetup\dotnet'
& ./eng/Inspect-EfApis.ps1 -Net10Root $older -Net11Root $ga `
  -OutputPath ./artifacts/ga-ef.assets.json
& ./eng/Inspect-GaEfContracts.ps1
```

Expect 123 source comparisons, 81 catalogue editions and 15 metadata delta groups
on the recorded inputs. Default output is disposable. Select the committed register
explicitly only when refreshing this release decision. Do not pass the retained
RC-package `ef.assets.json` as an expected identical GA output. Missing artifacts,
source-hash mismatch and network failures fail the audit; no wait is hidden as GA
success. No application/database execution or stable readiness is claimed here.
