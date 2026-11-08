# Comprehensive content and evidence gate

Audit checkpoint: 8 November 2026. **Approved pre-GA preview boundary.**
Pingu approved completing PIN-959 with accountable coverage, feasible local checks
and explicitly provisional content. Stable/GA certification remains gated by the
15 named pending editions below. The implementation PR prepares publication; it
does not publish a package.

The [preview registry](../../../content/preview-evidence.json) binds each exception
to exact metadata, Markdown and canonical sample identities, with existing GA
follow-up ownership. Changes invalidate approval until explicitly reconciled.
Source review, compilation and failed execution retain their separate meanings.

## Preview candidate and refreshed proof

- Content revision: `b47547cb95c34791f604087c901e05fcb489594b`.
- Package: `PinguApps.Playbook.0.0.0-preview.959.nupkg`, locally built, unpublished.
- SHA-256: `1a87ee5bdb45e95acde45191505412375087ac1305735e39e89967074aea2d36`.
- All 486 unique installed editions matched full canonical Markdown across 505
  target10/11 list/get pairs, including discovery title/goal, applicability,
  current evidence and text/structured parity.
- The documented Inspector setup/build/pack/example passed from `C:/`, selecting
  this worktree literally. All15 exceptions returned complete provisional guidance.
- Full `eng/Verify.ps1 -ReleaseReady -PackageVersion 0.0.0-preview.959
  -SkipInstalledTests` passed build, portable checks, stale/incomplete-content
  rejection and release-selection fixture packing. The affected content was
  rebuilt/repacked after disclosure corrections; unchanged sample proof is reused.
- Preview guard tests passed approval and rejected stable versions, mixed modes,
  missing exceptions and changed approved inputs. An actual stable `1.0.0` pack
  with `PlaybookReleaseReady=true` rejected pending proof and emitted no package.
- The exact preview passed all51 installed scenarios on each isolated .NET10
  and .NET11 RC Docker host with `--network none` (102 scenario executions).
  The existing suite was rebuilt with current embedded guidance. The consumer
  wrapper's stale46 expectation was corrected to the actual51 scenarios; stale
  runtime-patch and measured discovery-budget assertions were reconciled without
  truncating warnings or changing the served response contract.
- Dependency/renovation policy checks and publishing workflow guard checks passed.
  Native/Maps behavior exclusions and unperformed GA proof remain explicit.

## Historical checkpoint identity

- Source: `e37e09204e0f8c97e06951bd5c4d1a08e3353747`.
- Package: `PinguApps.Playbook.0.0.0-pin959-audit.nupkg`, built locally, unpublished.
- SHA-256: `c85b4114bc8788ff29409c8a6f1634615913f5f8785ffb84d74636de9a0cbc61`.
- Catalogue: 486 recipe editions. This count establishes neither completeness nor
  readiness. The candidate retains the existing source-review metadata and pending
  proof disclosures; the supplemental compilation records below are not yet wired
  into its served metadata.
- Official [release metadata](https://builds.dotnet.microsoft.com/dotnet/release-metadata/11.0/releases.json)
  reported `latest-release: 11.0.0-rc.3`, SDK `11.0.100-rc.3.26522.148` at this
  checkpoint. Final-package proof cannot be inferred from these RC artifacts.

## Coverage reconciliation boundary

The three registers at specification commit
`0a8989814ea11346354464b845c79c15bb71c5e9` contain the original **406** disposition
rows. Current registers contain **455** rows under the same extraction rule:
goal-bearing tables, the explicitly unnamed exclusion rows, and native goal
bullets. The additional 49 rows are register growth, not 49 proven new recipes.
Do not freeze a current audit at the historical 406 count.

The [web register](../inventory/02-web-data.md) retains G01–G25, A01–A06,
A07a–A07d and E01–E03: 38 rows preserving the original 25+7+3 baseline, with A07
split into four distinct goals. A07d still has pending browser/remote-state proof.
The [foundation register](../inventory/01-languages-libraries-sdk.md#exact-eight-approved-net10-foundations-cross-reference-only)
retains exactly eight .NET 10 goals, each with an existing 10.0 edition:

1. `csharp.extend-uneditable-types`
2. `csharp.validate-property-assignment`
3. `aspnet-core.validate-requests`
4. `ef.include-archived-with-tenant-filter`
5. `linq.retain-unmatched-rows`
6. `json.reject-malformed-contracts`
7. `linq.transform-async-stream`
8. `sdk.run-file-utility`

The [coverage ledger](content-readiness.inventory.json) retains every historical/current row, its reason,
source line, direct identity matches and links, plus hashes and ownership references
for all 23 existing Markdown source/API audits. It records correspondence and
unresolved gates, rather than certifying every consumer contract. Keep
grouped goals and unnamed exclusions; do not manufacture a recipe for each row.
The [audit decision record](../audit-proposal-review.md) also retains the seven
withdrawn/unsupported consumer dispositions. Their historical approval totals do
not override those later decisions.

The 26 initially unmatched aliases/aggregate owners have explicit dispositions in
the ledger. Representative mappings follow. These identify existing content,
rather than declaring missing recipes
solely because an earlier proposed ID differs:

| Register goal | Delivered guidance or accountable boundary |
| --- | --- |
| `mime.describe-video-content` | `uri.identify-data-uris` |
| `unsafe.check-accessibility-contract` | `csharp.audit-memory-boundaries` |
| `runtime.inspect-nativeaot-callers` | `reflection.inspect-loaded-metadata` |
| `sdk.configure-file-directives` | `sdk.compose-file-apps` |
| `sdk.analyzer-guided-logging` | `logging.generate-generic-messages` |
| `winforms.present-toggle-options`, `winforms.customize-small-appearance-details` | `winforms.adopt-modern-rendering` |
| `winforms.wire-vb-designer-events`, `winforms.return-intentional-property-values` | `winforms.preserve-designer-roundtripping` |
| `maui.simplify-xaml-namespaces` | `maui.choose-xaml-generation` |
| `maui.bind-xaml-csharp-expressions` | `maui.author-view-local-csharp-in-xaml` |
| `maui.remove-compatibility-package` | `maui.migrate-navigation-handlers` |
| `maui.show-tab-badges`, `maui.control-swipe-action-colors`, `maui.describe-navigation-for-accessibility` | `maui.navigate-with-path-parameters` |
| `maui.request-notification-permission`, `maui.filter-location-updates` | `maui.inject-permission-checks` |
| `android.configure-build-toolchain` | `android.migrate-runtime-and-platform-targets` |
| Historical `mobile.verify-test-template-contracts` | Current `mobile.run-tests-in-app-process`; preserve its native exclusions |
| Compiler/migration/obsolete-index aggregate rows | Their bounded audit ownership maps, not one invented aggregate recipe |
| `mobile.verify-native-original-examples` | Sample-specific compilation/source-review records under the native policy |

Canonical sample lineage and full body retrieval do not establish successful
composition of the supported hosts listed below. Those missing consumer contracts
and final-package results prevent stable certification. The approved preview
exposes those limits; linkage and a Done ticket do not supply execution proof.

## Historical checkpoint and feasible local checks

- `eng/Build-Content.ps1`: passed ordinary metadata, canonical snippet and saved
  evidence/input-freshness validation for the candidate. This does not execute
  every sample or establish originality by itself.
- `eng/Build-Content.ps1 -ReleaseReady`: failed on pending
  `blazor.restore-agent-conversation` proof. Separate enumeration found 15 pending
  editions; first-failure validation is not an exhaustive gap report.
- `eng/Test-Smoke.ps1 -PackageVersion 0.0.0-pin959-audit`: built/packed the ordinary
  tool and passed the one Windows installed-MCP scenario.
- The exact package passed the existing installed-MCP smoke in the existing
  Docker SDK10 image `sha256:de4fe90a98845d75910b9769662465c84de1a934665c431279d1382b2bddc02b`,
  with `--network none`. This is tool/offline evidence, not all recipe execution.
- `eng/Inspect-BlazorMigrationGuidance.ps1` retrieved all eight provisional
  SDK/build recipes listed below from the same artifact: full canonical Markdown,
  ID/title/goal, applicability, current declared evidence and matching structured/
  text responses. It ran from an unrelated directory with isolated installation.
- A persistent installed stdio session traversed both targets: 20 .NET10 and
  485 .NET11 list/get pairs, covering all 486 unique editions. Every returned
  Markdown body matched the compiled canonical catalogue; discovery title/goal,
  selected-target applicability, declared evidence freshness, unexpanded-snippet
  absence and text/structured parity passed. This traversal used unusable proxies,
  not OS network isolation; the separate Docker smoke used `--network none`.
- Actual locked compilation passed for both .NET 10 and .NET 11 projects named
  `BuildArtifacts`, `CliTelemetry`, `ContainerPublishing`, `SeedBankMap` and
  `BuildAccessObserver`. Existing-format `verification-evidence.json` records
  retain exact commands, dependencies, toolchain and input hashes.
- Both CLI opt-out wrappers executed successfully with the absolute SDK path.
  No export branch or collector was used. Successful `--info` is not evidence of
  telemetry absence, precedence or transport delivery.
- The .NET 10 `ArchiveDispatch` helper succeeded; extracted and build-output
  payload SHA-256 both equal
  `40de278996e90f8f57b4200fd389f6e23e6295aef812709d59493c0ff8cb7149`.
- SDK11 RC3 `ArchiveDispatch` failed inside `TarDirectory`: MSB4018/MSB0001,
  `Cannot call Reacquire() ... when Yield() was called on ...`. A retry with
  `-m:1 -nr:false` also failed. No successful newer extraction or incremental
  behavior is claimed.
- Both `ExplainDispatchInputs` targets returned their include/exclude/remove
  descriptions and only `payload/departures.txt` as the expanded input. Extra
  imported-graph, unset, mutation and rollback checks remain unperformed.
- Two actual SDK11 multiarchitecture container publishes, separated by a clean,
  produced local OCI archives. The pinned runtime index was
  `mcr.microsoft.com/dotnet/runtime@sha256:d8235d0f69afc582c94082a622dc31fe60e466ddbc1fef509ebcc8725827b6a3`;
  both used `SOURCE_DATE_EPOCH=1792454400`, empty `ContainerRegistry`,
  `LocalRegistry=Docker`, and distinct `ContainerArchiveOutputPath` values.
  Both image indexes identify
  `sha256:b447b66009a4ceaaaa70e801539c96030967ddae8c27505b98ceee2b07f4b077`;
  child manifest identities match for Linux x64 and Arm64. All 19 regular archive
  entries have equal SHA-256 content hashes across both runs. Archive-file
  hashes differ: archive-byte reproducibility is not established. The SDK wrote
  uncompressed tar despite the requested `.tar.gz` filename. No image was
  pushed to a remote registry or loaded/run in Podman. Archive creation does not
  test local-engine selection or Podman multiarchitecture loading.
- A separate SDK11 publish selected Docker explicitly, loaded an owned Linux x64
  image with a unique `pin959-` tag and ran it with `--network none`. Both expected
  dispatch lines were returned. The owned image/tag was removed. This establishes
  the Docker happy path, not Podman, Arm64 execution or failure/fallback behavior.
- Process/user/machine PATH comparisons passed for the compilation and package/
  Inspector checks. Automatic .NET PATH integration was disabled. No persistent
  environment setting was intentionally changed.
- An additional isolated-home installation of the same candidate succeeded from
  a local-only NuGet configuration. Its first-run output reported installing an
  ASP.NET development certificate. No certificate trust command was run.

Routine logs, Inspector responses and container archives remain under ignored
`artifacts/pin-959/`; they are not release approval artifacts.

The saved checkpoint has a narrow freshness check. From any starting directory,
with PowerShell 7:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-b415b02e'
& ./eng/Test-ContentReadiness.ps1
```

It verifies register/audit hashes, current row locations and recipe destinations,
the exact foundation identities and the pending-gate inventory. Pass the local
candidate's absolute path with `-PackagePath` to also verify its SHA-256. A pass
means the checkpoint still matches its inputs; it does not certify release readiness.

## Remaining evidence gates

| Recipe | Exact canonical sample directories, under both `samples/net10.0/` and `samples/net11.0/` | Remaining proof |
| --- | --- | --- |
| `blazor.restore-agent-conversation` | `ObservatoryLog` | Browser/provider/conversation restoration and final artifact qualification |
| `blazor.synchronize-agent-view-state` | `MuseumCaptions` | Browser/AG-UI/state synchronization and final artifact qualification |
| `ef.configure-generated-model-stage` | `FernGeneration` | Working generated model/query, build/publish/AOT outputs; known RC generation failures remain |
| `ef.materialize-missing-projection` | `PloverQueries` | Successful direct materialization on qualified final packages |
| `hosting.select-direct-tls` | `HarbourTls` | Actual Linux native TLS behavior and final artifact qualification |
| `msbuild.archive-build-artifacts` | `BuildArtifacts` | Official-docs implementation retained by Pingu; newer archive execution and extraction/incremental behavior remain unverified on RC3 |
| `msbuild.inspect-item-globs` | `BuildArtifacts` | Remaining negative/mutation/import checks beyond the observed happy path |
| `numerics.align-tensor-shapes` | `InstrumentBatch` | Final tensor/runtime shape, Any/view and memory-preflighted native-width traversal |
| `numerics.copy-tensor-views-safely` | `GridTransform` | Final tensor/runtime storage and copying execution |
| `sdk.configure-cli-telemetry` | `CliTelemetry` | Measured opt-out/configuration/network/collector behavior |
| `sdk.publish-multiarch-with-podman` | `ContainerPublishing` | Actual Podman load/inspect; no Podman executable was available |
| `sdk.publish-reproducible-containers` | `ContainerPublishing` | Bounded two-run image/object evidence is served; qualify the supported final artifact and default Podman path; do not claim archive-byte equality |
| `sdk.select-affected-tests` | `SeedBankMap` | Supported composed MTP host, local storage/registration contract, mapping and selection/failure behavior; compiled rules alone are insufficient |
| `msbuild.inspect-file-access-records` | `BuildAccessObserver` | Composed real observer host and file-access/failure/cancellation behavior; `.cs.txt` callback is not compiled by the existing project |
| `sdk.select-local-container-engine` | `ContainerPublishing` | Error/fallback checks and Podman remain; actual Docker x64 publish/load/run passed |

No pending status was cleared. Existing unavailable-native exclusions remain
sample-specific `not-tested`; native hardware is not a new prerequisite. Windows
Maps service/rendering stays `not-tested` under Pingu's local-only/no-cost decision:
no resources, credentials or service/tile requests were used. Other required local/provider/Human proof remains pending where explicitly
registered for preview and must be completed before stable publication.

## Preview completion and stable follow-up

The current preview incorporates the ten existing-format compilation records in
served evidence, alongside source-review records. Every named edition discloses
its provisional scope in discovery/applicability and full Markdown. MTP selection
and file-access callbacks remain provisional reference material, not working
composed host claims. The remaining source/API findings retain their bounded
recipe/audit ownership in the ledger; this ticket introduces no new qualifying
unowned corpus finding.

- `PIN-962`: runtime/BCL/tensor final artifacts and actual tensor suites.
- `PIN-963`: SDK/MSBuild/tooling/container GA qualification and supported hosts.
- `PIN-964`: web/Blazor/AI/browser/native TLS final qualification.
- `PIN-965`: EF final packages and generated/provider consumer evidence.

Pingu's decision is to follow official .NET11 documentation where RC3 differs.
The archive implementation is retained without an RC workaround or a separate
failure-only ticket. Its observed failure remains distinct from successful proof.
No pending status or sample-specific native/Maps exclusion was cleared.

The normal release publication path chooses the approved preview gate only for
`X.Y.Z-preview.N`; stable versions still use the strict `-ReleaseReady` gate.
Changing inputs, missing approvals/disclosures or stale evidence rejects preview
packaging. This is a repeatable release decision, not a blanket preview bypass.
Reconcile and retire exceptions with their existing owners once final artifacts
and supported consumer contracts can pass; then run the strict stable gate.
