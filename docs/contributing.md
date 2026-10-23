# Contribute and verify an original recipe

Navigation without inline-handler exceptions uses the original BrackenMenu host/client
comparisons. [PIN-798](verification/PIN-798.md) records both real module paths,
strict-policy controls, older backport and complete returned-body reconstruction.

Unified JavaScript startup options use the original .NET 10/11 ParcelStartup
Server/WASM legacy and Web App consumers. [PIN-789](verification/PIN-789.md)
records effective callbacks, browser culture, responsive controls, retained flat
legacy compatibility and the tested offline Inspector handoff.

Server-configured browser startup uses the original .NET 10/11 TideDesk host/client
pairs. [PIN-788](verification/PIN-788.md) records generated endpoint/component
options, real reconnection, Server/WASM/Auto execution, a bounded separate-request
RC2 sharing probe and complete offline installed retrieval.

Restrictive style CSP uses the original BeaconReadings .NET 10/11 samples.
[PIN-786](verification/PIN-786.md) records actual enforcing headers, visible distant
rows, old exception/strict-negative comparisons and the tested offline Inspector path.

Virtual-list item reveal uses the original .NET 10/11 LanternQueue samples.
[PIN-785](verification/PIN-785.md) records held-provider initial/programmatic
reveal, actual rendered ticket/viewport assertions, bounds and complete offline
installed retrieval. It keeps QuickGrid positioning and list anchoring separate.

Virtualized QuickGrid row positioning uses original HarbourManifest .NET 10/11 samples. [PIN-783](verification/PIN-783.md) records real Chrome initial/programmatic scrolling, identity and Start anchoring, plus the tested offline Inspector path.

Shareable table views use the original .NET 10/11 WorkshopViews samples.
[PIN-781](verification/PIN-781.md) records independent two-grid URL state,
immutable sort keys with separately editable captions, real static links and
interactive history, plus complete installed MCP retrieval.

Changing list anchoring uses the original .NET 10/11 OrchardJournal apps.
[PIN-784](verification/PIN-784.md) records rendered prepend/append/expansion,
provider identity, deliberate overscan and the tested offline Inspector path.

Interactive QuickGrid selection uses original .NET 10/11 MuseumRows apps.
[PIN-782](verification/PIN-782.md) records actual typed row events, accessible
button alternatives, static SSR exclusion and the tested Inspector handoff.

Safe static-subtree caching uses the original .NET 10/11 QuarryLedger samples.
[PIN-780](verification/PIN-780.md) records real cache-provider rendering,
variation/invalidation/isolation checks and the tested offline Inspector path.

Alternative component values use the original .NET 10/11 SeedLabels applications.
[PIN-779](verification/PIN-779.md) records native C# 15 literal/bound parameters,
real JSON interop/prerender persistence and delegate-union rejection, with the
tested Inspector handoff and existing packed-tool harness.

Captured static child content uses the original .NET 10/11 ArchiveCards samples.
[PIN-778](verification/PIN-778.md) records real SSR/Interactive Server execution,
generic/delegate/prerender restrictions and the tested offline Inspector path.

F# array migration, FSI asynchronous cleanup and experimental pull-driven
sequences use the six original `FSharpArrayAot`, `FSharpFsiCleanup` and
`FSharpSequence` samples. [PIN-982–984](verification/PIN-982-984.md) records the
one-machine Windows Native AOT publish/run, real FSI assets, measured RC2 cleanup
limitation and focused `eng/Test-FSharpLifetimes.ps1` diagnostics. Native proof
remains an authoring check reused while its inputs stay unchanged; routine
verification runs the five managed comparisons and consumes the saved native
record without adding a native build matrix.

Runtime-async and compiler-host examples use seven real managed sample paths and
the bounded `eng/Test-RuntimeAsync.ps1` authoring check. See
[PIN-757/980/981](verification/PIN-757-980-981.md) for exact assets, diagnostic
differences, the .NET 10 FCS host route and installed-tool inspection.

TLS, QUIC and runtime evidence uses the bounded authoring checks in
[PIN-752/755/756](verification/PIN-752-755-756.md). Windows checks exercise real TLS
and QUIC; `eng/Test-CrashEvidence.ps1` compiles and crashes only disposable Linux
children to verify report output. No recurring native CI matrix is added.

HTTP capacity and DNS service samples use the actual HttpCapacity and DnsServices
projects, real local TCP/UDP peers and the existing installed-tool harness.
[PIN-753–754](verification/PIN-753-754.md) records experimental eviction, delayed
HTTP/2 settings, Windows resolver restrictions and the independent older package
asset. On Windows the DNS authoring check needs a free UDP 127.0.0.1:53; it never changes
system DNS configuration. No native CI matrix is added.

Key-material and post-quantum guidance uses independently pinned KeyMaterial and
PostQuantum projects. [PIN-750–751](verification/PIN-750-751.md) records real
Windows comparisons and bounded Linux/OpenSSL authoring checks through
`eng/Test-CryptoPlatforms.ps1`, plus complete packed offline retrieval. This
does not add a recurring native CI matrix.

A complete contribution includes original comparison samples, metadata, offline instructions and fresh compilation evidence, plus focused execution evidence for claims that need it. A source audit or inventory entry does not deliver a recipe. Missing compilation blocks build/pack and publication; pending targeted Human proof blocks release readiness. Passing these gates alone does not establish the separate catalogue, client, evaluation or GA launch requirements.

File-application examples use the same capture/freshness pipeline as project samples. Declare TargetFramework and LangVersion explicitly in each evidence entry point, pin global.json, and declare external includes/imports as additionalInputs. `eng/Test-SdkWorkflows.ps1` checks actual CLI behavior; `eng/Test-WatchHosts.ps1` is a bounded Windows authoring check, not a native CI matrix. [PIN-759–761](verification/PIN-759-761.md) records its unresolved mobile proof and combined publication gate.

Runner, tool/single-file and static-assets samples use the bounded Windows authoring check `eng/Test-SdkDelivery.ps1`; its actual SDK/RID/Node requirements and installed inspection are in [PIN-762–764](verification/PIN-762-764.md). Saved compilation and execution evidence is reused by portable package validation; no new native CI matrix is added.

The host remains a workload-free .NET 10 tool. Keep sample SDKs, provider dependencies and native workloads under `samples/` and on suitable authoring machines, outside the host's project and solution. Installed Playbook reads embedded guidance; it neither inspects the consumer nor builds its application.

Shared validation/tracing/logging/HTTP guidance uses actual .NET 10/11 projects
under ValidationRules, TracingLogging and HttpObservations. The authoring check
`eng/Test-Observability.ps1` verifies real SYSLIB1011/CA1873 rejection and restores
the ordinary build. [PIN-748–749](verification/PIN-748-749.md) records the managed
and real loopback checks, package assets and installed offline retrieval.

## Delivery order

1. Establish the reusable goal, official source support, older/newer comparison and actual application requirements.
2. Author the independent samples and claim-specific checks. Pin their verification inputs, compile them and capture evidence on suitable machines.
3. Add the recipe metadata and complete Markdown using canonical snippet references. Reuse an edition only when guidance and feature requirements are unchanged.
4. Validate content, build, run pack rejection checks and pack the ordinary tool. Install that exact artifact and inspect discovery, full Markdown, applicability and evidence.
5. Review the full contribution against its goal and source register. Record what actually ran and any remaining proof. Publish only after all applicable release gates are satisfied; leave merging/releasing to the owner.

The seed workflow below follows these steps using `csharp.validate-property-assignment`. [The verification record](verification/PIN-674.md) records the commands exercised for this guide.

## Quick pull request check

Ordinary PRs build/pack the tool and run one installed MCP smoke scenario. With PowerShell 7 and .NET SDK 10.0.402:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-6e51df7e'
& ./eng/Test-Smoke.ps1
```

Expect one passed scenario and a final `PASS` message. This checks installation, initialization, tool discovery, recipe list/get and clean shutdown. Build/pack still validates recipe metadata and saved evidence freshness. Adding a recipe does not require adding another PR smoke scenario. Run the relevant samples when authoring or changing their behaviour; capture fresh evidence for changed inputs. The broader checks below remain available locally and run before release or through manual `Run Tests` dispatch. See [automation](automation.md) for the coverage tradeoff.

## Full verification

Build prerequisites: PowerShell 7, .NET SDK **10.0.402**, **9.0.319** and **11.0.100-rc.2.26504.105**. The .NET 9 SDK is needed only for the historical comparison, never for installing or running Playbook. The .NET 11 SDK verifies that target and the optional older-target route of the shared edition. No optional workloads are used.

```powershell
Set-Location <path-to-Playbook>
pwsh -NoProfile -File ./eng/Verify.ps1
```

If .NET 9 or 11 is installed separately, pass its executable with `-Net9Dotnet <path-to-dotnet>` or `-Net11Dotnet <path-to-dotnet>`. The script rejects the focused invalid-content fixture, compiles and executes the real .NET 9/10/11 samples, including a .NET 11 SDK build of a .NET 10 target, then builds the solution with analyzers/warnings as errors and checks real `pack --no-build` rejection of incomplete/stale content. It packs the ordinary artifact once and tests it; its SHA-256 identifies that package. A second package under `artifacts/release-fixture/` contains contract-only identities and is never published. Fixture packing restores ordinary build output afterwards.

The `Rejected ...` and `Pack --no-build rejected ...` messages are expected passing checks. Temporary fixtures deliberately contain invalid content or stale/missing evidence; these messages confirm the guards refuse them. They do not mean your authored recipe or final package was rejected. A successful run ends with all 35 scenarios passing, zero failures/skips and a package SHA-256. `NETSDK1057` is informational for the pinned RC SDK; a TRX overwrite warning replaces the previous report. An unhandled error, nonzero script exit or failed test indicates failure.

Thirty-four focused Reqnroll scenarios install from a local-only feed and new cache under unrelated temporary directories. They exercise bare/help/version, stdio initialization/instructions, exactly two read-only tools and output schemas, required nonnullable target schemas, omitted/null/blank rejection on both tools, deliberate catalogue-only `latest`, independent list/get selection, complete structured/JSON-text equality, canonical snippets, aliases, coverage, unchanged edition reuse, retained changed editions, conditional routes, response-size budgets, empty-search recovery and actionable errors. The fixture scenario tests newest-compatible selection when editions overlap, a known ID without an applicable edition, and a pre-introduction conditional route. Generation also rejects that route without its conditional flag. Synthetic fixture introduction metadata is not API introduction or release coverage evidence. All scenarios require a clean shutdown and stderr diagnostics; none project-reference production code. Temporary installs are deleted afterwards.

Fresh `DOTNET_CLI_HOME` directories trigger SDK first-run setup. Every harness child sets `DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=0` so the SDK cannot register temporary `.dotnet/tools` directories in the user's PATH, even when installation uses `--tool-path`. The obsolete `DOTNET_SKIP_FIRST_TIME_EXPERIENCE` setting does not prevent that write. The harness launches the installed tool by its absolute path and checks that process PATH, plus saved user/machine PATH on Windows, remain unchanged across each real install. An unexpected difference fails the scenario; the harness never restores shared environment settings from a snapshot.

Local checks set unusable HTTP proxies, which establishes proxy-independent operation but is **not** network isolation. On Linux use `pwsh -NoProfile -File ./eng/Verify.ps1 -IsolateNetwork`: after build/pack, the installed-consumer tests run under `sudo unshare --net` with no external network routes. Only the local package feed is available. SDK downloads/restores happen before network isolation.

With Docker available, run `pwsh -NoProfile -File ./eng/Test-Offline.ps1 -RuntimeMajor 10` and `pwsh -NoProfile -File ./eng/Test-Offline.ps1 -RuntimeMajor 11` after verification. Each reruns all 35 Reqnroll scenarios against the exact package with `--network none`. Only package artifacts, test binaries and the artifact-consumer script are mounted; no recipe or sample source is accessible. Canonical sample text is embedded independently in the test assembly for comparison. Both images are digest-pinned, with only runtime 10.0.12 or 11.0.0-rc.2.26504.105 available; pulling them is an online preparation step. Host traces assert the installed server's actual CoreCLR path. The .NET 11 lane explicitly permits prerelease selection and verifies the package's own major roll-forward policy. CI also downloads the same package and compiled suite for one brief Windows and one brief macOS smoke. See [packed-host evidence and commands](verification/PIN-673.md).

## Inspect the development package

From the repository root after verification:

```powershell
Set-Location <path-to-Playbook>
# Use fresh directories; if these exist from an earlier run, choose new names.
New-Item -ItemType Directory ./artifacts/manual-feed | Out-Null
Copy-Item ./artifacts/PinguApps.Playbook.1.0.0-preview.1.nupkg ./artifacts/manual-feed/
$feed = (Resolve-Path ./artifacts/manual-feed).Path
$config = Join-Path $feed 'NuGet.Config'
'<configuration><packageSources><clear/><add key="local" value="{0}"/></packageSources></configuration>' -f [Security.SecurityElement]::Escape($feed) | Set-Content $config
$previousPackages = $env:NUGET_PACKAGES
try {
    $env:NUGET_PACKAGES = Join-Path $feed 'cache'
    dotnet tool install PinguApps.Playbook --version 1.0.0-preview.1 --tool-path ./artifacts/manual-tool --configfile $config --no-cache
}
finally { $env:NUGET_PACKAGES = $previousPackages }
./artifacts/manual-tool/playbook --help
./artifacts/manual-tool/playbook --version
```

Help must show `playbook mcp`; version must show `1.0.0-preview.1`. The fresh local-only feed/cache ensures the installation consumes this artifact instead of a cached or remote package. This installs locally, without publishing or changing global tools. Delete `artifacts/manual-tool` and `artifacts/manual-feed` when finished. To see the actual recipe body and constraints yourself, use [the MCP Inspector walkthrough](inspect-guidance.md). The Reqnroll checks verify real MCP responses and lifecycle results; TRX evidence is under `artifacts/TestResults`.

## Author a recipe edition

### Write original comparisons and preserve attribution

Choose the developer outcome first. Write the discovery `title` and `goal` around the reusable capability and when to apply it; keep fictional domains in the worked example. The seed's warehouse bay codes demonstrate property validation, while its discovery wording explains normalization and validation with `field`.

Write prose and examples independently from scratch. Changing names in a guide's demonstration is not an original example. Official documentation, release notes, language proposals and API/package artifacts establish facts; they are not a supply of recipe implementations or explanations to copy. Both implementations must solve the same realistic goal with the same observable rules. Compare .NET 11 with .NET 10 normally, and the [exact eight .NET 10 foundations](planning/coverage.md#approved-initial-eight-recipes) with .NET 9. .NET 9 is comparison-only, not a catalogue target. Explain a genuinely unavailable older equivalent without inventing one, and describe valid older approaches fairly.

Use C# for shared framework/library examples, F# for F# capabilities and VB for qualifying VB changes. Add translations only when authoring differs materially. Include the essential configuration, code, numbered migration instructions, constraints and useful alternatives offline. An external link cannot supply a missing implementation step.

Add official HTTPS references to `sources` with meaningful titles, and connect route conditions through `sourceUrls`. Check the actual API and target-specific package assets, not just a release headline. If substantial reference-source implementation is legitimately reused, preserve its license, copyright and attribution separately; do not present it as an original recipe example. Shared explanations should have one canonical source with framework integrations linked rather than copied.

Update the relevant [source-disposition register](planning/inventory/README.md): source, introduction, authoring impact, applicability, included recipe/grouping, exclusion reason or unresolved fact. Keep unresolved facts visible. A reviewed inventory row, synthetic contract fixture or API audit is not evidence of a completed or compiled recipe. Keep `content/coverage.json` honest; one seed recipe does not fulfil comprehensive coverage.

### Assemble the authoring files

A deliberately theoretical edition may carry only `official-review` records when
`humanProof.status` is `pending`. Record the reviewed sources and actual input
hashes; identify SDK/language values as intended sample pins, and state that no
build, execution or artifact inspection occurred. Development builds retain
`kind: official-review` and `role: source-review`, and automatically disclose the
missing compilation/execution in discovery and detailed applicability. Mocks and
execution-only records cannot use this authoring path. `-ReleaseReady` still
rejects pending proof. This permits source review without inventing evidence;
it does not make the recipe verified or ready for publication.

Use the [seed metadata](../content/recipes/csharp.validate-property-assignment/10.0/recipe.json), [seed Markdown](../content/recipes/csharp.validate-property-assignment/10.0/recipe.md) and [schema](../eng/recipe.schema.json) as a structural reference. Author the new content independently; do not copy the seed's claims, baselines or successful evidence into another goal.

- **Identity/discovery:** `id`, `edition`, `title`, `goal`, `areas`, `exampleLanguages`, `tags`, `apis`, `introducedIn`, `supportedTargets` and `stability`. Preserve published stable IDs. `edition` identifies guidance; `introducedIn` identifies the feature; neither automatically establishes compatibility.
- **Applicability:** each route declares `target`, `usage`, `conditional`, `requirements`, `selectionNotes`, `sourceUrls` and `evidenceSamples`. Separate primary and optional routes. List runtime, SDK, language, framework, packages/assets, provider, platform, rendering mode, workloads, opt-ins and stability limits explicitly, using empty arrays where none apply. The rules below cover shared areas and neutral languages.
- **Provenance/proof:** `sources`, per-sample `evidence` entries with exact `kind`, `sample`, `target`, `sdk`, `language` and bounded `scope`, plus `humanProof.status` and `scope`. Optional `record` selects a separate saved record. Every declared sample needs fresh compilation; every target route needs exact-target evidence.
- **Canonical content:** `snippets` maps each Markdown token to a repository-relative sample `path`, named `region` and language. Add `recipe.md`, independently pinned sample projects, `global.json`, `verification.json` and captured `verification-evidence.json`. Dependency samples also need a reviewed `packages.lock.json`.

The following rules explain the supported metadata and current validation boundary. Structural validation helps catch omissions; it cannot establish originality, factual support or that arbitrary prose achieves the goal.

### Reuse unchanged guidance across verified targets

Keep one edition when its canonical snippets, migration and feature requirements are unchanged. Adding a supported application target is not itself a new edition: extend `supportedTargets`, add that target's explicit applicability route and preserve its exact-target sample evidence. Keep `introducedIn` at the feature's original release. Never infer target compatibility from introduction, an edition number, a newer SDK or a successful build alone.

The property recipe uses edition `10.0` for targets `10.0` and `11.0`, retaining its .NET 10 / C# 14 introduction. Both serve identical offline Markdown and snippets. .NET 10 keeps its primary route and optional SDK 11 alternative; .NET 11 has its own runtime/SDK route and RC disclosure. Distinct baselines stay linked separately even when they compile the same source. Existing evidence freshness and stale-pack gates still apply to every sample and to changed metadata/Markdown.

Add a newer edition only when guidance or applicable feature requirements genuinely change; retain the historical edition and its verified targets. Discovery and detail select the numerically newest edition explicitly declaring the requested target. Overlapping targets are allowed: a changed edition may supersede earlier guidance for one target while the earlier edition still serves another. Do not duplicate unchanged content just to label a new target. The unpublished `fixture.changed-guidance` tests this with an additional migration check; it is not another shipped recipe or an API claim. See the [tested two-target walkthrough](verification/PIN-973.md).

### Separate requirements from test baselines

Author each route's application conditions inside `requirements`, following [the contract](planning/tool-contract.md#requirements-and-verification-baselines). Runtime/SDK/language and each package/workload condition use `name`, `version`, `match` (`release-family` or `exact`) and a source-supported `reason`. Use major.minor for a release family; it does not mean an open-ended minimum. Preserve a necessary exact pin only when official source/artifact evidence establishes it. One successful test at a pinned SDK does not justify either an exact installation requirement or a minimum. Keep assets, providers, platforms, rendering restrictions, opt-ins and stability limitations in the same requirements object.

List the normal route first with `usage: primary`; label another choice `optional-alternative`. Requirements on alternatives are not cumulative. Keep `conditional` for release-routing conditions. Link each route through `evidenceSamples` and declared `sourceUrls`; preserve exact sample pins and complete saved evidence using the existing [freshness workflow](verification/PIN-672.md). Generation labels those records `verification-baseline` without changing their identity or hashes. Exact measured runtime output, SDK/language, resolved dependencies, workloads/native versions and commands describe tests, not consumer upgrades. Keep RC-only/GA-pending limitations explicit even for stable syntax.

The property recipe's normal route needs .NET 10 / C# 14, not SDK 10.0.402 specifically. Its optional .NET 11 SDK route retains `net10.0` and C# 14. Both choices have distinct evidence links. The schema and real build/pack gates reject old prose requirements, malformed family pins and contradictory SDK/language baselines; author review must still establish supported compatibility. See the [tested inspection example](verification/PIN-972.md).

Use the canonical Draft-07 dialect identifier `http://json-schema.org/draft-07/schema#` in the local schemas. PowerShell 7.6.7 resolves that registered identifier without external fetching; the HTTPS spelling is not treated as the same identifier. Do not enable schema downloading for offline validation. See [the reproduced build fix](verification/PIN-974.md#powershell-767-validation-correction).

Declare `selectionNotes` on each route: concise, complete free-text conditions that affect whether to select it. Preserve essential restrictions from detailed prerequisites/constraints here; do not copy implementation steps or omit a restriction just to meet a size budget. An empty array is valid only when structured requirements and warnings fully describe selection. Review notes whenever requirements change. Discovery automatically retains all version conditions (without justifications), frameworks, packages/workloads, assets, providers, platforms, rendering modes, opt-ins and stability limitations. It keeps each alternative separate. Full reasons, prerequisites, constraints, route sources and evidence links remain in detail. The server does not truncate any field. See [the contract and migration](planning/tool-contract.md#concise-discovery-pin-974) and [measured budgets](verification/PIN-974.md).

Discovery filters use ordinal, case-sensitive area/language identifiers advertised by the installed catalogue. Queries require all whitespace-separated terms, case-insensitively, across title/goal/tags/API names. Areas and languages each use OR; filter types use AND. Results select the newest compatible edition before filtering, then page in ordinal stable-ID order (default 20, maximum 50), with `total`, `offset`, `limit` and nullable `nextOffset`. Keep the same filters when following `nextOffset`; an offset beyond the total succeeds empty. Unknown filters and invalid ranges are tool errors rather than ignored input.

Routes may declare `sharedAreas` (default empty): explicit relevant areas where the route's framework/provider/platform/rendering prerequisites allow this shared guidance. Do not infer relevance from free-text platform descriptions or broadly include every shared recipe. `includeShared=false` disables these extra area matches; omitted/empty areas still means all. Discovery retains only routes matching the target and the area/language combination together. Detail keeps all routes for its target so agents can inspect alternatives.

Programming recipes declare demonstrated `exampleLanguages`, independently of broader API usability. Genuinely neutral configuration/build recipes explicitly set `languageNeutral=true`, use an empty `exampleLanguages`, and declare applicable `neutralLanguages` on every route. This boolean labels discovery and detail; it is never inferred from XML/JSON alone. Language-specific settings/directives (such as C# `LangVersion`) remain non-neutral and declare their programming language. Generation rejects programming snippets mislabelled neutral and missing neutral-language declarations; author review must still establish that configuration is genuinely neutral. Filter vocabulary includes explicitly supported neutral languages as well as demonstrated languages, under the existing `supportedExampleLanguages` field.

Canonical XML snippets use `<!-- snippet:<name> -->` / `<!-- endsnippet:<name> -->` markers. Existing `// snippet:<name>` markers remain supported. The unpublished fixture uses the actual sample project's generic `TargetFramework` setting as neutral XML and its C# `LangVersion` setting as non-neutral XML. These are contract cases, not additional shipped recipes or evidence of new APIs. The 53 synthetic paging identities also stay in the fixture package.

Add `content/recipes/<stable-id>/<edition>/recipe.json` and `recipe.md`, following `eng/recipe.schema.json`. Keep IDs goal-based and release-independent, with nonempty dot-separated names. Declare every supported target explicitly; no future-target compatibility is inferred. Every target needs an applicability route with source-supported requirements, declared source URLs and evidence for that target. An edition cannot predate introduction. A target older than `introducedIn` also requires `conditional: true`; compilation alone does not establish supported language/runtime combinations. Declare framework, package assets, providers, platforms and rendering restrictions explicitly. Keep `content/coverage.json` honest about scope/completion. Aliases, release resolution and JSON tool errors are implemented; combined discovery filters and paging are implemented.

Experimental recipes use `stability: experimental`; every route must provide nonempty `optIns` and `stabilityLimitations`, including the actual directives/configuration and versioned package requirements where applicable. These fields reach both discovery and detail. Stable recipes may omit them; generation supplies empty arrays. Validate publisher-designated experimental status and package versions during source review: structural checks cannot infer API stability from arbitrary prose.

Add independently pinned original comparison samples under `samples/`, targeting the actual previous/current releases. Include exact SDK/language/target evidence and official HTTPS references. Mark canonical source regions with `// snippet:<name>` and `// endsnippet:<name>`, and reference them in Markdown with `{{snippet:<key>}}`. Every snippet must belong to a declared evidence sample directory and contain exactly one nonempty matching region. Do not paste fenced implementations into Markdown: code/configuration blocks must come from canonical sample references. The build validates metadata, paths, evidence baselines and snippet references; it embeds generated JSON and rendered Markdown into the tool assembly. Discovery projects that same embedded edition metadata. No runtime files or fetches are needed.

Include exactly one nonempty Goal, Older approach, Newer approach, Migration, Constraints and Evidence section (descriptive heading suffixes are allowed). Newer approach requires a canonical snippet; Migration requires numbered offline implementation steps. A missing, empty or link-only section is rejected. Explain an unavailable older equivalent honestly. Review the full recipe for essential configuration and instructions: structural validation cannot prove that arbitrary prose implements its stated goal.

Content validation also runs during `dotnet pack --no-build`. It compares the freshly generated content with the catalogue recorded during compilation and rejects changes since that build, even when the changed metadata is otherwise valid. Rebuild before packing modified content. Run `pwsh -NoProfile -File ./eng/Test-Content.ps1` for the small negative fixture, and after a Release build run `pwsh -NoProfile -File ./eng/Test-Content.ps1 -Pack` to check both incomplete and stale content through the real pack target. These use temporary copies and never edit authored recipes or publish packages.

Build metadata checks do not perform compilation or execution. Each declared sample needs saved compilation evidence; follow the capture and refresh workflow below. Add portable checks to `eng/Verify.ps1` where useful. Native samples are compiled on suitable authoring machines, with their records committed; do not add a recurring native CI matrix. Include essential configuration and instructions offline. Keep comparisons fair and original. Extending content with a new stable ID/edition requires no host, transport or tool-registration changes.

## Capture and refresh sample evidence

### Declare inputs, then run the real commands

Give each sample its own exact SDK pin in `global.json` and explicit target/language mode in its project. Application requirements remain separate from these test baselines. Follow [the manifest schema](../eng/verification.schema.json) for `verification.json`:

- `additionalInputs`: linked sources, imported configuration, referenced-project input directories and other verification inputs outside the sample, including the capture scripts. Include transitive inputs explicitly. Dynamic project includes are rejected rather than guessed.
- `buildArguments`: explicit `-p:Name=Value` properties, including required native target/RID configuration.
- `dependencies`: sorted exact resolved `name/version` entries, including transitive packages/projects. Generate and review `packages.lock.json` before capture when dependencies exist; the helper builds with locked restore.
- `workloads`: required `id/manifest-version` entries measured by `dotnet workload list`, not every installed workload.
- `nativeToolchains`: required measured `name/version` entries. Keep the native version commands/results in a linked report; the helper checks the supplied declaration, not the tool's authenticity.

Run from the repository root in PowerShell 7. These seed commands use the normal .NET 10 executable and the separate .NET 9/11 installations on this authoring machine. Substitute your actual executables, or `dotnet` when the pins are available in one installation. Do not change persistent PATH or environment variables.

```powershell
Set-Location <path-to-Playbook>
$net9 = 'C:\Users\pingu\.dotnet-playbook-9\dotnet.exe'
$net11 = 'C:\Users\pingu\.dotnet-playbook-11\dotnet.exe'
pwsh -NoProfile -File ./eng/Record-Evidence.ps1 -Sample samples/net9.0/PropertyAssignment/PropertyAssignment.csproj -Dotnet $net9
pwsh -NoProfile -File ./eng/Record-Evidence.ps1 -Sample samples/net10.0/PropertyAssignment/PropertyAssignment.csproj
pwsh -NoProfile -File ./eng/Record-Evidence.ps1 -Sample samples/net10.0/PropertyAssignmentSdk11/PropertyAssignment.csproj -Dotnet $net11 -RuntimeDotnet dotnet
pwsh -NoProfile -File ./eng/Record-Evidence.ps1 -Sample samples/net11.0/PropertyAssignment/PropertyAssignment.csproj -Dotnet $net11
```

Expect four successful builds and executions of the same property behavior checks: construction, normalization, null/blank/length rejection and retention after a rejected update. The SDK 11 / `net10.0` path executes on .NET 10. The actual runtime is recorded in program output; the SDK host runtime alone is not proof of the sample's runtime. .NET 11 RC execution is not GA verification.

The helper writes `verification-evidence.json` only after successful commands, exact SDK/dependency/workload checks and unchanged before/after inputs. It records source commit/dirty state, per-input SHA-256, sample/target/language, commands/output, dependencies, workloads/native versions, host and SDK/workload inventory. Review and commit the record with the sample. Capture changes embedded content, so rebuild before `pack --no-build`. `eng/Verify.ps1` reruns portable checks but does **not** refresh saved records.

Use `-Kind compile` for build-only proof and `-Kind cross-compile` for an actual cross-compilation. Neither executes the DLL. Default `compile-and-run` runs the built DLL; it is suitable for these console samples, not a generic browser, database, UI or device test runner. Separate records selected by `record` may use `official-review`, `mock` or `execution`; those classifications do not replace compilation. See [the full evidence contract and capture limits](verification/PIN-672.md).

### Decide when proof must be refreshed

Reuse unchanged evidence. Freshness compares exact input bytes and the input set, not the age of a record or the current Git HEAD. It covers the sample directory, declared external inputs and nearest effective `Directory.Build.props`, `Directory.Build.targets`, `Directory.Packages.props`, `NuGet.Config` and `global.json`. New files or nearer overrides invalidate proof; declare additional imported ancestors/nonstandard configuration yourself. `bin`, `obj` and evidence records are excluded. Preserve LF files so checkout line endings do not change hashes.

Refresh every affected record after changing sample/shared source, target, language, SDK pin, resolved dependencies/lock, workload/native declarations or build inputs. Changing the actual authoring SDK/workload/native baseline also requires measured recapture and any appropriate pin/declaration updates; build validation does not inspect installed tool versions. Do not edit hashes or relabel old output to make new inputs appear verified. A different source commit or unrelated documentation edit alone needs no recompilation. Recipe prose/metadata changes still need content review and rebuild; rerun the claim-specific check if the assertion changes even when sample hashes do not.

Run `pwsh -NoProfile -File ./eng/Test-Evidence.ps1` to check unchanged reuse and rejection of changed/missing proof in temporary copies. After a Release build, run `pwsh -NoProfile -File ./eng/Test-Evidence.ps1 -Pack` to prove real pack rejection. Expect `PASS` messages; a missing record reports `Missing compilation evidence`, a changed input reports stale evidence, and no invalid package may be produced. `freshness: current` in an installed response means the inputs matched at package build time; it neither checks the consumer nor guarantees current upstream APIs.

### Hand off provider and native checks

For the current Windows x64 and Docker authoring routes, run the
[native capability preflight](verification/PIN-678.md). Its measured inventory
and explicit owner handoffs distinguish available access from missing Arm/SVE,
CoreCLR WebAssembly relinking and Podman proof. It does not replace recipe evidence.

Before handing work to another authoring machine or a Human, agree the exact recipe/edition/route, source commit and input hashes, samples, pins, commands and observable claim. Record who will capture and refresh proof when those inputs change, the required access and any unmet check. Return the saved compilation record, measured versions, focused execution report and limitations; review them before linking them into recipe metadata. Keep credentials in the appropriate secret store, outside the repository and reports.

For database claims, use an isolated real provider/service at the stated package, target asset, server version and configuration. Record the setup and cleanup, request/query inputs, actual execution and expected result. A translated SQL string or mocked repository does not establish provider execution, transactions or persistence. Match browser/request checks to rendering modes, authentication and other claimed conditions; a successful build or HTTP response alone does not prove an interactive browser claim.

For native claims, use a suitable authoring OS and installed workloads/native SDKs; affected Apple builds require a Mac with matching Xcode. Measure and declare native versions, then pass matching values with `-NativeToolchains` when capturing compilation. Cross-compiling establishes build output only. UI, device/emulator, permissions and lifecycle claims need their focused real checks and reports; do not call a workload-free tool smoke native behavior evidence. If the helper cannot perform the required deployment/run, capture compilation separately and link the actual execution report rather than pretending DLL execution covers it.

Use `humanProof.status: pending` with an exact scope for targeted Human checks still owed; set `verified` only after recording traceable results, or `not-required` with a defensible reason. Pending proof may be served in incremental packages but fails `pwsh -NoProfile -File ./eng/Verify.ps1 -ReleaseReady` and `dotnet pack -p:PlaybookReleaseReady=true`. Missing compilation blocks ordinary build/pack too, except for the explicitly pending source-only authoring path above. That path does not satisfy compilation or release readiness. Report access or toolchain blockers explicitly; audits, mocks and unavailable-machine placeholders cannot satisfy verified delivery.

Commit native authoring evidence for routine validation to consume without installing native workloads. Keep broad native builds, UI/device/emulator suites and recurring native matrices out of CI, including separate manual-dispatch matrices. Routine Windows/macOS checks remain tiny installed-tool smokes; the Linux/core lane remains portable. Reuse unchanged native proof and refresh it on the appropriate authoring machine when relevant inputs change.

## Review the delivered artifact

After capture or an edit, follow `eng/Verify.ps1` above: content/evidence fixtures, seventy real portable sample paths plus the focused SDK workflow commands, Release solution build, real pack rejection checks, one ordinary package and the separate unpublished contract fixture, then installed stdio MCP scenarios. For manual build/pack separation:

```powershell
Set-Location <path-to-Playbook>
pwsh -NoProfile -File ./eng/Test-Content.ps1
pwsh -NoProfile -File ./eng/Test-Evidence.ps1
dotnet build PinguApps.Playbook.slnx -c Release
pwsh -NoProfile -File ./eng/Test-Content.ps1 -Pack
pwsh -NoProfile -File ./eng/Test-Evidence.ps1 -Pack
dotnet pack ./src/PinguApps.Playbook -c Release --no-build -p:PackageVersion=1.0.0-preview.1 --output ./artifacts
```

Use the installed-package scenarios from `Verify.ps1` for the final artifact check, and the [Inspector walkthrough](inspect-guidance.md) to read ID/title/goal, full rendered Markdown, selected applicability and evidence yourself. Do not substitute `dotnet run` or synthetic fixtures for the actual ordinary package. `pack --no-build` rejects content changed since compilation; rebuild and repeat affected checks. Keep the tested package hash and publish that artifact without repacking when publication is separately authorized.

Before handoff, account for each recipe claim with official source support, current compilation and the specific execution check it needs. Check original authorship, fair previous-release behavior, complete offline steps, snippet identity, stable IDs/editions, requirements versus test pins, target-specific routes, experimental labels, register disposition and unresolved gaps. Passing schema or tool tests does not establish native/provider behavior or comprehensive release readiness. Report exact commands, versions, actual results and unverified scope.

Shared style and analyzer configuration adapts the MIT-licensed PinguApps Template's generated-app configuration. The [reference map](planning/reference-repositories.md) records the layout, packaging and scenario conventions used. Recipe prose and samples were authored independently; no guide example implementation was reused.

The [PIN-787 circuit pause proof](verification/PIN-787.md) records original
TideCharts .NET 10/RC2 compilation, native hidden-tab resource/state recovery,
real host lifecycle maintenance/cancellation and complete packed offline retrieval.
