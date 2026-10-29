# SDK, MSBuild, NuGet and testing authoring coverage: PIN-707

Reviewed **22 October 2026**. This is a bounded artifact/API/configuration and official-source audit. It supplements [PIN-688's host/migration audit](sdk-package-hosts.md). It delivers no recipe, validates no proposed consumer, and clears no comprehensive-content or GA publication gate. The 29 delivery drafts below require review before ticket creation or implementation.

## Boundary and provenance

[PIN-707](https://linear.app/pinguapps/issue/PIN-707), planning key **API_BUILD**; `SPEC.md` at `0a8989814ea11346354464b845c79c15bb71c5e9`, stories 47, 104–106 and 158–160; approved proposal `b3778cc`. Current coverage/implementation/verification plans, glossary, ADRs 0001–0004 and contribution/dependency policies constrain the audit. Root PLAN.md, CONTEXT.md and CONTEXT-MAP.md are absent. The planning README's statement that implementation has not begun is historical: actual recipes, evidence and installed-tool checks now exist.

Linear boundary: team **PinguApps**, `62f80270-b641-4fa3-b60c-46be4c4df2c8`; project **Playbook**, `b22380c7-6fde-49f4-8826-e5631dc5df7a`. A paginated, archived-inclusive overlap search returned 317 issues; every returned team/project ID was checked. PIN-707 has no native blocker. Existing SDK delivery, host migration, obsolete-index, deployment and GA owners remain separate; no related issue is modified or declared satisfied here. The connector's shared-notes endpoint returned `McpServerError: Tool get_agent_context not found`; repository evidence supplies the context.

The [source manifest](sdk-build.sources.json) pins **59** full files with immutable commits and SHA-256 hashes:

- `dotnet/core` **ffed591b0fb8f9aab50ecc28f2c765049fb28d8f**: all nine Preview 1–7/RC1/RC2 component indexes, nine SDK notes, nine container notes, seven MSBuild notes and six NuGet notes. The pinned tree has no separate MSBuild Preview 2/3 or NuGet Preview 1/2/3 files; absence is not a no-change claim. Their available indexes and subsequent notes are retained.
- `dotnet/docs` **f0021fde66c59559fa1ba3c9a044148738f12faf**: full cumulative SDK document, complete .NET 11 compatibility index, all **nine** SDK/MSBuild-family compatibility articles, .NET 11 non-default obsolete article and global obsoletion overview. The index explicitly remains incomplete. Inline component breaking-change sections are included even when not indexed there.
- `dotnet/dotnet` **a8dd0577c5c13f43a2f57fe33b4ff11b90f6371e** / **820230c386db5b0106c5430b3e5aee92a7826bf5**: old/new SearchFilter, current V3 search implementation, staging resource and static-state lifecycle source.
- `dotnet/dotnet-docker`: the exact commit/hash recorded for complete image-signature instructions, following the Preview 3 pointer. This is source evidence, not signature verification.

Full files, rather than just heading lists or cumulative headlines, informed the dispositions. The manifest's headings are navigation aids and can include headings in fenced examples. Canonical links: [SDK P1][sdk-p1], [SDK P5][sdk-p5], [SDK P7][sdk-p7], [MSBuild P6][build-p6], [MSBuild P7][build-p7], [MSBuild RC1][build-rc1], [NuGet P4][nuget-p4], [NuGet P6][nuget-p6], [NuGet P7][nuget-p7], [NuGet RC1][nuget-rc1], [NuGet RC2][nuget-rc2], [container P3][container-p3], [container P6][container-p6]. Other complete milestone files are pinned in the manifest.

## Actual artifact comparison

[Inspect-SdkBuildApis.ps1](../../../eng/Inspect-SdkBuildApis.ps1) reads installed **win-x64** SDK **10.0.402** (product build `10.0.402-servicing.26505.111`, VMR `a8dd0577…`) and **11.0.100-rc.2.26504.105** (VMR `820230c3…`). Reference resolution uses actual `Microsoft.NETCore.App.Ref` **10.0.13/net10.0** and **11.0.0-rc.2.26504.105/net11.0**. These reference packs supply metadata dependencies, not application/tooling execution evidence.

The [generated asset report](sdk-build.assets.json) records **52 managed tooling assemblies per release**, exact relative paths, complete file hashes, assembly/product versions, public/protected type/member declarations, custom attributes, generic constraints, parameter/default/modifier contracts, forwarders, old/new declaration differences, and all encountered obsolete declarations. It includes root MSBuild, NuGet, template-engine and VSTest assemblies, current-.NET SDK task assemblies, API compatibility/symbol/package validation and release-metadata libraries, and container tasks. `net10.0` versus `net11.0` task-host directories are normalized only as comparison keys; actual paths stay recorded. Bundled NuGet is **7.9.0-rc.611** versus **8.0.0-rc.605**; MSBuild's assembly version `15.1.0.0` is not its product version. Product versions and source commits are preserved separately. SDK-bundled and independently distributed package builds are not asserted byte-identical; PIN-688 retains its 23-package asset evidence.

The script also reads **230 / 232** `.props`/`.targets` files under the SDK root, `Current`, `Sdks` and `Containers/build`. It hashes entire files and mechanically compares element paths, attributes and leaf values without evaluating/importing projects. This catches properties, task parameters, conditions and manifest-writing inputs beyond release notes. XML ordering, duplicate identical declarations and whitespace-only/IL changes are not semantic comparisons; whole-file hashes retain those changes. No byte-equal hash is inferred from an empty declaration diff.

Scope exclusions: localized duplicates, .NET Framework `net472` task binaries, external/optional workloads, IDE-only assemblies, standalone NuGet.exe and separately versioned MTP extension matrices. Current-.NET public tooling is the bounded surface, not every DLL shipped anywhere in the SDK. WindowsDesktop/WebAssembly/Razor build files encountered mechanically are forwarded to their family owners, not claimed as fully reviewed framework APIs. App runtime libraries and gRPC API reconciliation belong to their own families. No gRPC recipe is invented from this build audit.

MetadataLoadContext reads inspected assemblies without executing their code. Return-parameter defaults are normalized to `<return>` because they are not callable optional arguments. Paired compiler/nullable/state-machine/initialization metadata is counted separately; original declarations are retained. New trimming annotations, obsoletions, signature changes and XML controls still receive owners. The [reviewable disposition rules](sdk-build.dispositions.json) assign every difference; unmatched differences fail replay. A rule is accountability, not proof of behavioral equivalence. The residuals below remain visible rather than being treated as automatic exclusions.

## Canonical source and authored-impact register

| Family / actual change | Disposition and accountable outcome |
| --- | --- |
| File applications: include/ref/project/DLL directives, quoted values, aliases, duplicate/invalid properties, default artifact layout, explicit file selection | Existing **PIN-759** / `sdk.compose-file-apps`; old .NET 10 servicing overlap must remain explicit. No new recipe for cache reuse or AOT up-to-date fixes alone. |
| Solution filters, targeted/hidden-severity formatting, configuration environment default and launch-profile property expansion | Existing **PIN-760**, stable SDK goal IDs retained. Current-directory fallback and launch notices moving to stderr are automatic fixes. |
| Watch environment/port/device/Aspire workflows and preview template installation | Existing **PIN-761**. Hot Reload recovery/deadlock/prompt fixes alone are excluded. Native device proof remains its own obligation. |
| MTP runner/global.json/environment selection, templates, positional/project/module/traversal selection, timeout/failure limits, JSON discovery and artifact layouts | Existing **PIN-762** / configure/choose/create-test-project goals. Protocol reporter improvements are automatic unless authored policy, parsing or artifact options change. Mobile test-in-app contracts remain **PIN-934**. No runner-selection claim substitutes for extension compatibility. |
| Single-file extraction warning/layout, PackAsTool versus custom nuspec | Existing **PIN-763**. RC2 still emits a warning for the custom nuspec route; historical severity or wording is not current behavior proof. Custom native RID orchestration is **B16**, a different outcome. |
| Static asset groups/endpoints, TypeScript integration, scoped CSS/watch/pack task plumbing | Existing **PIN-764**. Public static-asset helper types embedded in Blazor tasks are not a second independent API recipe. Razor/WebAssembly framework questions remain **PIN-708**, WindowsDesktop **PIN-693**. |
| Container engine preference/removal of standalone containerize, SOURCE_DATE_EPOCH/created labels/cache choice, multi-arch Podman | Existing **PIN-765**. Container timestamps and NuGet timestamps stay separate. No new recipe for redundant upload avoidance. Registry realm validation is **B27**. |
| OTLP/CLI telemetry and environment switches; default server/AOT CLI startup | Authored export/opt-out behavior remains **PIN-766**. Native CLI default has no affected public API and recommends no routine code change: **excluded**, retain its explicit fallback as diagnostic context. Installer dedup, SDK size, partial-R2R of inbox tools, shell completion and default build-server speed are **automatic exclusions**. Authored partial-R2R is separately **B15**. |
| TarDirectory/Untar, MSBuildProvideItemGlobs | Existing **PIN-767**. Actual RC2 also exposes ZStandard compression, deterministic timestamps and extraction include/exclude controls beyond RC1's headline; these are recorded API scope, not newly executed evidence or permission to relabel theoretical content. |
| DTD defaults for XmlPeek/XmlPoke/XslTransformation; retired change waves and MSB4272 | Existing **PIN-768**; exact default/mitigation analysis stays with that delivery. Do not blanket-disable DTD restrictions or resurrect retired wave opt-outs. |
| NuGet cooldown/source exceptions, CLI min-publish-age, vulnerable updates with warnings-as-errors | Existing **PIN-769**. The actual API includes exception/provider settings and NU1020. Preserve fixed/floating/explicit-version and unrelated-restore-failure controls; it is not universal restore suppression. |
| Deterministic NuGet timestamps | Existing **PIN-770**. Preview 5's default-on claim was superseded by RC1's explicit opt-in. Mechanical pack/evaluation reuse and filename/nuspec fixes alone are excluded. |
| NuGet client host framework migration | Existing **PIN-771**. SDK 10.0.402 already bundles NuGet 7.9 prerelease tooling; package/framework availability is independently verified, not inferred from SDK or package major numbers. |
| Seven indexed SDK/package host migrations, affected-test availability | **PIN-688 A–I** own dnx/global.json, explicit unsupported Mono launch, NU5052, NU1703, next-best cached restore, template host TFMs, test Newtonsoft ownership, removed JSON communication APIs and experimental affected tests. Do not repeat their analysis or create duplicate drafts. |
| MSBuild typed task parameters/environment/constructors; AOT host registration; partial evaluation; structured build events; renamed server API and file-access records | **B01–B06**. Existing .NET 10 servicing already has multithreaded infrastructure, BinaryLogger parsing, terminal forwarding and full ProjectGraph APIs. Do not invent a newer-only difference for those. Actual task typing and partial-evaluation additions remain new in this comparison. |
| NuGet public staging service, state-reset hook, analyzer asset metadata and new obsolete diagnostic helper | **B07–B10**. Nullability/init/async implementation metadata does not independently demand one recipe per declaration. PIN-688 keeps JSON migration ownership. |
| Removed VSTest test sessions; assembly resolver construction; runsettings task type; collector event setters | **B11–B13/B22**. These actual public API differences are additional to PIN-688's JSON removal. No MTP migration is presumed as the universal replacement. |
| SDK vulnerability/EOL metadata checks, custom native-tool RIDs, no-dgspec restore, referenced symbols, import-tree items | **B14/B16–B19**. Actual build controls change authored projects/scripts. Caches/diagnostics and old-target routes require real evidence. |
| Container signatures and Azure Linux 4 base tags | **B20/B21**. Smaller image layers/compiler packages and bundled PowerShell 7.7 alone are automatic/toolchain updates; independently versioned PowerShell language guidance is out of scope. Azure Linux 4 is labelled Beta in the reviewed notes, not GA support. |
| Injected workload resolver environment, task-level deps inputs, console encoding, bundled MCP scaffolding, custom optimized-publish hooks | **B23–B26/B28**. These are distinct authored integrations or removable workarounds. The automatic implicit System.Net.Http.Json using is excluded as a new recipe: explicit existing imports remain valid. |
| API symbol filter inclusion/exclusion and composite mode | **B29**. Actual ApiSymbolExtensions constructor/factory changes require custom tooling to choose the intended symbol set; API compatibility/validation assemblies themselves have no declaration delta in this comparison. |
| Native deployment runtimeconfig precedence / generated dev-file controls | **PIN-687** retains the assigned runtime configuration finding; **U3** retains direct SDK task API migration questions. Its other NativeAOT-prefix/caller behavior is not re-audited here. |
| NuGet package-type filtering and opt-in System.Text.Json protocol path | Reviewed in complete P6 notes/source. **Already present in the actual .NET 10.0.402 baseline** (including singular SearchFilter.PackageType); not a new old/new API delta here. Raw-search and plugin compatibility remain visible under U5; do not teach plural PackageTypes from superseded previews. |

Other canonical bug-fix/performance bullets were reviewed: resource-name record parsing, item chaining/whitespace, regex property functions, NoWarn composition, callback defaults, metaproject/restore routing, logging, pack evaluation, current-directory/cache fixes, Haiku recognition, artifact consolidation, transport and deterministic metadata repairs need no distinct authored approach by themselves. Removing a documented compatibility workaround is retained only when it has a concrete user goal; the drafts and existing owner map above supply those cases. Uri property functions and BinaryLogger.ParseParameters are already in the serviced older tooling, so they are not claimed as .NET 11-only capabilities.

### Source drift and obsolete-index ownership

- Actual `Microsoft.NET.Publish.targets` uses **PublishReferencesSymbols**, whereas the Preview 1/cumulative heading says **PublishReferenceSymbols**. B18 must test the actual plural property, including the ineffective singular spelling as a control.
- NuGet P7 describes **NuGetProcessState.RegisterResetAction/Reset**. RC2's source/binary exposes **StaticState.BuildEnded/RaiseBuildEnded** instead. B08 must invalidate cached state after a build, rebuild it under the *next* environment, and avoid disposing shared resources with in-flight users. Source presence is not proof a third-party host raises the lifecycle event correctly.
- Template-host TFM/source conflicts and affected-test newer-SDK rollout remain documented by PIN-688. None is silently resolved by an empty API diff.
- The complete custom-obsolete/index sources add no SDK/MSBuild/NuGet-specific SYSLIB/EXTOBS entry. SYSLIB0064/65 belong to cryptography, SYSLIB0066 to Options/PIN-746, and SYSLIB0063 to pipes/PIN-741; PIN-689 owns global enumeration. Framework-specific entries stay with those owners.
- All obsolete declarations in both tooling baselines remain in the asset report, including historical VSTest async/attachment/traits, NuGet raw-search/plugin payload/legacy framework/serialization and MSBuild compatibility APIs. Their mere continued presence does not create .NET 11 backfill work. Newly obsolete **PluginException.CreatePathNotFoundMessage** gets B10. Newly seen generic resolver and cooldown constructors marked `CompilerFeatureRequired("RequiredMembers")` are compiler compatibility guards, not application deprecation recipes; nullable/required initialization must still be respected by actual client examples.

### Retained unknowns and public-surface limits

- **U1 — template CLI helpers:** DotnetHelpAction disappears; HostSpecificTemplateData and Edge metadata change. CLI implementation exposure is not proof of a supported embedding contract. PIN-688 F owns supported template-host migration; PIN-707 retains the narrower external-help-consumer question for PIN-963 reconciliation. No replacement API or supported net9 route is invented.
- **U2 — VSTest internals exposed publicly:** inferred-platform helpers, TestSDKAutoGeneratedCode visibility annotations, parameter-name/nullable changes and proxy internals remain recorded. B11–B13/B22 own demonstrated authored contract breaks. Intended support and practical external use of the remaining implementation helpers are unverified; no automatic-equivalence claim is made.
- **U3 — direct SDK task/workload callers:** inbox task thread-safety/metadata changes are automatic for ordinary projects. Direct `GenerateDepsFile.UserRuntimeAssemblies` callers change from strings to ITaskItems (B24); workload resolver injection is B23. Public PackIdJsonConverter/reader implementation changes do not prove a supported external workload-host contract. Optional workload/framework availability stays with its native family owners.
- **U4 — build XML:** new compiler/SDK version tables, host-directory changes, source-link task initialization and automatic routing have no demonstrated new authored goal. Explicit controls are assigned above. Full-file hashes retain ordering/body differences that the declaration-set comparison cannot explain; this is not an exhaustive MSBuild behavior equivalence proof.
- **U5 — already-serviced NuGet contracts:** package-type filters, raw-search deserialization and plugin payload obsoletions exist before the selected old/new boundary. Precise first-servicing introduction and older package target routes remain unverified. Do not publish a misleading Newtonsoft/JToken or universal AOT compatibility claim from these DLLs.

PIN-963 must compare actual GA sources/assets and these residuals again. Unknown intended external support does not erase a known authored break: the identified breaks above have delivery drafts. Neither this bounded API closure nor a source exclusion waives future recipe/evidence gates.

PIN-765/PIN-767's tracker comments record Pingu's earlier authorization to deliver theoretical content without native tests. This audit preserves that scope and their saved completion state. It neither invents live evidence for those editions nor applies that separate exception to the new drafts.

## Approval-only delivery drafts

**No new issues or recipes created.** Propose these in existing milestone **03 - Build and deployment guidance**, `7b3876e4-5f75-4b54-a375-cc598059679d`, PinguApps/Playbook boundary above, normally Backlog, related to PIN-707 and the PIN-959 publication gate. Native estimate scale is **S=2, M=3, L=5**; each estimate includes original samples/prose, dependency reconciliation, behavior evidence and installed retrieval. B-identifiers are local proposal labels, not allocated tracker numbers. Existing highest prefix is 03-012; recheck all approved and unpublished proposals before assigning numbers (including PIN-688 A–I). No milestone/title changes are proposed.

Every draft requires the following common acceptance, **in addition** to its specific checks:

1. Use exact SDK/runtime/reference baselines above. Direct SDK API examples pin the assembly identities/hashes/product builds in the asset report and a compatible locked package closure if packages replace SDK references. Independent package versions/TFMs must be read from real artifacts, not inferred from bundled identities. Missing distributable assets block delivery. Native/image drafts also record exact tool/image digests and platform access before execution.
2. Write wholly original fictional-domain older/newer consumers and offline prose/metadata with canonical snippets and stable goal IDs. Explain servicing overlap or an unavailable older equivalent honestly. Record actual target/SDK/language/package/platform/opt-in/stability requirements separately from tested baselines; isolate sample build/package boundaries. No blanket suppressions or unverified down-level route.
3. Compile actual old/new consumers, run the listed real checks, record command/output/artifact evidence and refresh input hashes. A source inspection, mock, manually fabricated manifest or successful no-op is insufficient. A missing required environment, contract or runtime proof blocks the delivery PR.
4. Build/pack/install the ordinary Playbook artifact into an isolated local feed/cache; retrieve ID/title/goal and full Markdown/applicability/evidence through real stdio list/get **offline from an unrelated directory**. Update the source register. No public publishing, production resources, credentials, persistent environment edits or recurring native CI matrix is authorized. GA reconciliation remains separate.

### B01. Bind typed parameters in a safe custom build task — L (5)

Proposed `msbuild.bind-typed-task-inputs`. A fictional observatory's task consumes files relative to each project. Compare the old ITaskItem/explicit-path implementation with actual ITaskItem<T>/TaskItem<T>/AbsolutePath and TaskEnvironment binding; include environment-dependent constructor defaults. Run two projects concurrently under actual `-mt`, verify paths/metadata and unchanged process directory/environment, compare the ordinary host, and prove older-host parameterless-constructor fallback. Do not claim the attribute alone makes unsafe code safe or advertise the planned analyzer as shipped. L covers task binding, concurrency and two host versions.

Delivered under **PIN-1118** as [edition 11.0](../../../content/recipes/msbuild.bind-typed-task-inputs/11.0/recipe.md), with original Meridian Ridge paired tasks and fresh `ObservatoryTasks` sample evidence. Exact SDK MSBuild DLL hashes match this audit; compiler, reference and native host/runtime identities are separately pinned and checked. SDK 10's unannotated explicit-path task runs ordinarily and in `-mt` sidecars. SDK 11's typed task runs ordinarily and concurrently under actual `-mt`, with same-process/different-thread rendezvous, per-project paths/content/output metadata, unchanged task process environment/CWD, absent/configured constructor defaults, explicit override and real missing-file failures. One net10 compatibility DLL demonstrates parameterless selection on SDK 10 versus injected selection on SDK 11. This is not older-host generic-item support. The route is exact-artifact Windows x64 RC2, with experimental `-mt` opt-in and explicit child-only server opt-out; no server-reuse/performance or GA claim. L (5) retains its task-binding/concurrency/two-host rationale. PIN-959, PIN-963/actual-GA and unrelated Human gates remain open.

### B02. Evaluate only the project information a tool needs — M (3)

Proposed `msbuild.evaluate-project-stages`. A museum inventory tool requests properties, then items from an original imported project. Verify ProjectInstance stage/value/target counts against full evaluation, full as default, and mutable Project rejection of partial stages. Check real command `-getProperty`/`-getItem` results too; make no universal speed claim. Prerequisite: matching actual Microsoft.Build dependency closure and SDK resolution, not just the DLL's presence.

Delivered under **PIN-1119** as [edition 11.0](../../../content/recipes/msbuild.evaluate-project-stages/11.0/recipe.md). Original Bracken Quay Museum hosts compile/run on both actual SDK/runtime closures with the independently pinned and locked Locator 1.11.2 net8.0 asset. Public `RegisterMSBuildPath` supplies the complete installed SDK context before MSBuild types load; SDK compilation references are non-copying, with no independent Microsoft.Build package substitution. Exact audited MSBuild hashes, compiler/reference/native host/runtime, dependency manifests and loaded SDK helpers/resolvers are pinned separately; evidence records all loaded assemblies and real SDK/import file identities. SDK10 retains full evaluation and lacks the partial API; no older partial route is advertised. SDK11 checks every stage, properties/imports/items/metadata against Full, omitted/default Full, undefined stage and all three mutable Project factories rejecting every partial stage. **Actual RC2 corrects Preview7's zero-count example:** partial `Targets.Count` throws `InvalidOperationException`; only Full exposes a target count. Both actual CLIs return the expected property/item data; the explicit target control alone changes the inventory status. No target execution is inferred from evaluation or target discovery, and no universal speed claim is made. The complete offline recipe is inspected through the ordinary installed tool. M (3) retains its two-host dependency/resolver closure and stage/value/count/CLI comparison rationale. PIN-959, PIN-963/actual-GA and unrelated Human gates remain open.

### B03. Register a closed set of build extensions in an AOT host — L (5)

Proposed `msbuild.register-aot-build-extensions`. An original tool embeds project evaluation and one known task/resolver/logger. Use actual registration APIs and trimming diagnostics, compare a managed host, publish and execute a real native binary with the platform compiler, and exercise a missing-registration failure. Separate AOT-safe evaluation from reflective task execution; resolve required APIs/annotations before authoring. No suppression-based success or copied SDK internals. L covers native toolchain and runtime loading.

Delivered under **PIN-1120** as [edition 11.0](../../../content/recipes/msbuild.register-aot-build-extensions/11.0/recipe.md), qualified only for the original North Quay fixed extension set on Windows x64. SDK10's complete managed host uses Locator1.11.2, directory SDK resolution and reflective UsingTask; actual execution proves its valid alternative and absence of the new registration APIs. SDK11's compiler/runtime/native-host identities match this audit, but its bundled MSBuild18.12.0-1.26504.105 is **not** the independently distributed package18.11.0 (net11.0, product18.11.0-1.26512.6/source1b17a019) used by the current locked consumer. The public package's actual buildTransitive defaults are imported and checked in the real ILC response; no SDK internals are copied. Real platform ILC/link publishing produces an x64 native PE without a CLR directory, then executes resolver registration, generic task input/output binding and direct logger instances. Fresh processes verify evaluation-only and missing task/resolver MSB4283/MSB4282; both managed dependency sets also execute. **IL2026 remains visible** at the public BuildManager.Build RUC boundary: this is measured fixed-set execution, not an analyzer-clean or general MSBuild AOT-safety guarantee. Dynamic extensions, project-cache plugins, generic task-item binding, solutions and actual GA remain unqualified. Exact hashes, native toolchain/libs, commands, locked closure and fresh input evidence are retained through the existing sample workflow; installed offline retrieval verifies complete canonical guidance. L (5) retains its native-toolchain/runtime-loading and managed/native comparison rationale. The eight foundations, PIN-959 qualifying coverage/evidence, PIN-963/actual-GA and unrelated Human gates remain open.

### B04. Consume structured assembly-conflict and server events — M (3)

Proposed `msbuild.inspect-structured-build-events`. A fictional depot build host captures actual conflicting assembly-reference and server-lifecycle events, preserving victor/victim/dependency/search fields and deferred startup messages instead of scraping localized text. Generate a real conflict and reused/new/no-server cases, inspect the binlog and verify message-constructor migration. Keep sensitive paths out of published evidence. Prerequisite: a controlled original reference graph and actual host lifecycle; synthetic event objects alone do not prove emission.

### B05. Migrate a custom MSBuild server client — M (3)

Proposed `msbuild.migrate-server-client`. Compare the old Experimental namespace with current Server types/constructor/results and short-lived node semantics. A disposable child host executes success, build failure and cancellation, then shuts down only its own server; prove exits and no orphan process. Disclose EditorBrowsable/internal-support limitations and require an official supported use decision before publishing an embedding recommendation. Ordinary dotnet build users need no API migration.

### B06. Read experimental build file-access records correctly — M (3)

Proposed `msbuild.inspect-file-access-records`. An original build observer handles changed FileAccessData construction, enumerated patterns and opened-file attributes. Prerequisites: publisher-supported instrumentation/OS route, exact binary/diagnostic opt-ins and disposable inputs. Capture real read/write/enumeration events, compare old/new decoding and cancellation; preserve experimental status. If no accessible real instrumentation route exists, delivery stays blocked rather than using hand-created events as runtime proof.

### B07. Upload packages to a V3 staging endpoint — L (5)

Proposed `nuget.stage-package-uploads`. An original publisher discovers PackageStagingResourceV3 from a real service index and stages an original nupkg/snupkg with optional group metadata. Require an authorized disposable staging service or faithful local HTTP server implementing the pinned protocol; record which one ran. Verify actual multipart payload/routes, status/cancellation/timeout, missing-resource handling and secure transport controls; do not push to nuget.org or print API keys. A local protocol check does not establish any hosted service's support. L includes the new endpoint contract and actual transport evidence.

### B08. Invalidate custom NuGet caches between reused builds — M (3)

Proposed `nuget.reset-reused-host-state`. A fictional package host subscribes to actual StaticState.BuildEnded. Two real builds in one host see different task-specific environment/configuration without stale values; verify reset timing, lazy recreation, unsubscription and an in-flight resource control. The host's real lifecycle must raise the event; manually raising it alone is only a unit check. Keep PATH/persistent settings unchanged and do not use the obsolete note's nonexistent registry API.

### B09. Inspect restore's analyzer assets without assuming execution — M (3)

Proposed `nuget.inspect-analyzer-assets`. Create an original analyzer package with language/compiler-version layout and real net10/net11 consumers. Test RestoreEnableAnalyzerAssets, selected assets metadata and include/exclude/private controls, including mixed targets. Independently verify whether the pinned SDK consumes the metadata and whether the diagnostic actually executes; do not equate an assets entry or `_._` placeholder with compiler enforcement. Prerequisites: actual compatible Roslyn/analyzer packages and a locked closure.

### B10. Remove dependency on an obsolete credential-plugin diagnostic helper — S (2)

Proposed `nuget.own-plugin-launch-diagnostics`. An original custom credential-plugin launcher owns its missing-executable error instead of calling newly obsolete CreatePathNotFoundMessage. Compile old/fixed consumers, capture the actual warning and a real missing-child launch with a useful path/error while redacting secrets. Verify valid launch remains unchanged. No credential-provider installation or real feed credential is required; do not invent a public replacement helper.

### B11. Replace removed VSTest session orchestration — L (5)

Proposed `testing.migrate-vstest-session-host`. An original custom host using Start/StopTestSession/ITestSession migrates to a supported available discovery/run route. Pin actual matching translation-layer/platform/test packages independently of SDK version. Compile the old failure and new host; execute discovery, pass/fail, cancellation and repeated runs with cleanup and correct result counts. Retain session-pool removal and JSON changes as separate facts; do not promise equivalent pooled lifetime or assume MTP is drop-in.

### B12. Construct immutable VSTest assembly-resolution context — S (2)

Proposed `testing.migrate-assembly-resolution-context`. An original resolver supplies RequestingAssembly in the new constructor instead of its removed setter. Real extension loading must resolve the intended assembly and preserve missing/requestor behavior in both platform baselines. Pin the actual PlatformAbstractions closure and a controlled plugin directory. A constructed event object alone is insufficient loading evidence.

### B13. Pass custom VSTest runsettings through the current task contract — S (2)

Proposed `testing.pass-custom-task-runsettings`. Migrate a custom MSBuild invocation from string[] VSTestCLIRunSettings to the current string contract. Use real tests with values containing spaces/separators, inspect effective settings and prove that values do not become switches or lose quoting. Compare old/new hosts; do not alter ordinary runner-selection guidance or confuse XML RunSettings with CLI settings. Prerequisite: actual VSTest task/test-host packages for both builds.

### B14. Check the resolved SDK's vulnerability and EOL status — M (3)

Proposed `sdk.check-sdk-servicing-status`. An original project enables CheckSdkVulnerabilities and owns SDK selection separately from NuGet package audit. Isolate its metadata cache/home, use real cache schema with controlled vulnerability/EOL/clear/missing/stale cases, run actual restore/build and capture NETSDK1236/1237 behavior. Distinguish controlled metadata from a live advisory check and verify no build-time network assumption. Never label a missing cache as a clean security result.

### B15. Publish a selected partial ReadyToRun set — L (5)

Proposed `sdk.choose-partial-readytorun-inputs`. An original command tool selects PublishReadyToRunPartialAssemblies/Crossgen2ContainerFormat using the actual supported target contract, preserving a full/default control. Require matched runtime/crossgen artifacts and the platform execution route. Publish and inspect actual PE/native outputs, run both, verify exclusions/fallback and failure diagnostics. DLL property presence does not prove supported configuration; no startup/performance claim without a bounded measurement.

### B16. Supply a deliberate native-tool package RID matrix — L (5)

Proposed `sdk.customize-native-tool-package-rids`. Implement ComputeToolPackageRuntimeIdentifiersToPack with actual supported native toolchain/RIDs rather than depending on the host's conservative defaults. Pack original real native executables, inspect inner-build counts/package identities/RID graph and install/run on an available declared host; cross-compiled-but-unrun RIDs stay labelled. Test unsupported/missing toolchain failure. Prerequisites include each advertised target's compiler/runtime, not a dummy generated file. Distinct from PIN-763's basic tool layout.

### B17. Avoid writing a disposable CI restore graph — S (2)

Proposed `nuget.skip-disposable-restore-graph`. Compare real isolated restore outputs with RestoreDoNotWriteDependencyGraphSpec on/off and a normal incremental local control. Verify the nupkg closure/assets and build still work while dgspec presence differs; do not claim cache/no-op equivalence or speed savings without measurement. Exact NuGet/SDK pins and a local original feed avoid accidental network/cache success.

### B18. Include referenced symbols in published output — S (2)

Proposed `sdk.publish-referenced-symbols`. An original project-reference application uses the actual **PublishReferencesSymbols** property. Publish true/false/default and incorrect-singular controls, inspect PDB/DLL output for references versus the application's own symbols, and execute the app. Compare the older baseline's explicit file-copy route only if its real artifacts need one. No source-only promise that every PDB or single-file mode behaves identically.

### B19. Inspect the actual build import tree — S (2)

Proposed `msbuild.inspect-import-tree`. An original shared-build project opts into MSBuildProvideImportedProjects and reads paths, ImportingProjectPath and Sdk metadata. Verify real nested/conditional/SDK imports, ordering/duplicate behavior and the unset control; distinguish this from PIN-767's unevaluated item globs. A handwritten XML include list does not prove the evaluated closure.

### B20. Verify the signature of a selected .NET base image — M (3)

Proposed `containers.verify-base-image-signatures`. Pin exact old/current image digests, Notation **1.3.2**, any ORAS helper and the actual Microsoft signing/TSA certificate bytes/trust policy in an isolated store. Run real registry/referrer and signature checks against the digest selected for a fictional worker; reject missing/untrusted/tampered or wrong-scope controls. Inspecting a referrer is not signature/trust verification. Record whether the older image already carries signatures; this is not claimed as exclusive to .NET 11. No system trust-store edits.

### B21. Migrate an authored Azure Linux base-image selection — L (5)

Proposed `containers.migrate-azurelinux-base-images`. Pin actual available Azure Linux 3/4 image digests and the selected app/AOT variant. Build and execute an original worker on supported Linux, verify OS/native-library/globalization behavior and deliberate tag replacement, and test missing native dependency failure. Keep Beta status from the reviewed source unless actual later support evidence changes it. Smaller default layers or GCC replacing Clang alone do not justify a recipe; its authored native/base-image migration must be demonstrated.

### B22. Consume VSTest collector events without mutating removed setters — M (3)

Proposed `testing.migrate-collector-event-inputs`. An original collector/extension stops assigning the removed TestCaseEventArgs and TestCaseEndEventArgs properties and uses supported construction/observation contracts. Compile old/fixed code and load the real collector in a real test run, verifying case identity, child-case flag and outcome. Do not fabricate test outcomes or conflate these events with ordinary application DTOs. Require compatible matching collector/test-host packages.

### B23. Resolve workloads using an explicit host environment — M (3)

Proposed `sdk.resolve-workloads-with-host-environment`. An original workload metadata tool uses WorkloadResolver.Create's new environment delegate with real disposable manifest files. Compare ambient versus injected values, null/default callback, missing manifest and conflicting root inputs without installing workloads or changing machine environment. Record the reader's supported external-use contract before publication; optional native compilation is required only if advertised beyond manifest resolution.

### B24. Migrate custom dependency-file task inputs — M (3)

Proposed `sdk.migrate-custom-dependency-file-inputs`. An original custom build invoking GenerateDepsFile supplies ITaskItem[] instead of the removed string[] UserRuntimeAssemblies contract. Generate actual old/new deps.json from real assemblies, verify library/runtime paths and execute the resulting application. Establish the supported direct-task extension contract and exact target framework before authoring; do not replace this with a manually written dependency file. Runtimeconfig migration remains with PIN-687.

### B25. Choose console encoding for redirected SDK output — M (3)

Proposed `sdk.configure-redirected-output-encoding`. An original Windows script sets DOTNET_CLI_FORCE_UTF8_ENCODING in child processes only. Capture actual non-ASCII diagnostics through a pipe with default/false/true controls and the declared console/code-page conditions; decode bytes using the consumer's explicit policy. Verify exits and unchanged persistent console/environment settings. Never promise the SDK can discover a pipe reader's encoding.

### B26. Create an MCP server from the bundled template — M (3)

Proposed `sdk.scaffold-mcp-server`. Generate an original fictional-domain local stdio server from the actual bundled template, inspect and pin all generated package/SDK dependencies, build it and exercise real initialize/tools/list/tool-call/shutdown. Compare the actual older template availability/install route honestly. Remote transport/AOT/self-contained claims each need their own actual supported build/runtime proof or explicit exclusion; template presence alone proves none. This does not change Playbook's own two-tool protocol.

### B27. Configure a registry authentication challenge that the SDK accepts — M (3)

Proposed `containers.migrate-registry-auth-challenges`. An authorized disposable registry/auth service uses a valid absolute HTTPS realm, with deliberate local insecure exceptions only where the actual SDK permits them. Publish a real original image and test disallowed scheme/private/loopback/mismatched-host cases without sending credentials outside the disposable service. Record actual HTTP exchanges with secrets redacted; a mocked realm validator is insufficient. Do not disable validation to manufacture compatibility.

### B28. Preserve custom hooks in optimized native publishing — M (3)

Proposed `sdk.migrate-optimized-publish-hooks`. An original NativeAOT application's build has an observable generated artifact/hook dependency. Compare actual old/default/new UseOptimizedPublish paths and migrate the hook to the supported publish target so its artifact is present and consumed. Require a real native compiler/publish/run, inspect the binlog and exercise NoBuild/repeat controls; an opt-out is a documented temporary alternative. Automatic AOT publish speed alone remains excluded.

### B29. Select the intended public API comparison surface — M (3)

Proposed `packaging.select-api-comparison-symbols`. An original library validation tool migrates CompositeSymbolFilter's constructor/mode and DocIdSymbolFilter/SymbolFilterFactory inclusion contracts. Pin actual ApiSymbolExtensions, Roslyn and compatibility-library assets. Build old/fixed consumers, compare real original assemblies containing deliberately included/excluded APIs, verify And/Or and empty-list controls and a known breaking change. Record the selected symbols and actual validation result; a hand-authored list alone is insufficient. Do not silently convert an exclusion list to an inclusion list or assume named/params calls remain source-compatible.

## Reproduce and inspect

Prerequisites: PowerShell 7, both exact installed win-x64 SDK roots/reference packs above, and HTTPS access to pinned official sources. No installer, project restore or reviewed assembly execution occurs in the audit script. Use `-Net10Root`/`-Net11Root` for alternate exact installations. From an unrelated directory, this block was executed:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-37e76603'
$pwsh = (Get-Command pwsh -CommandType Application | Select-Object -First 1).Source
& $pwsh -NoProfile -File ./eng/Inspect-SdkBuildApis.ps1 `
    -OutputPath ./artifacts/sdk-build.replay.json `
    -CompareWith ./docs/planning/audits/sdk-build.assets.json
if ($LASTEXITCODE -ne 0) { throw 'SDK audit replay failed' }
Get-Content -LiteralPath ./docs/planning/audits/sdk-build.md |
    Select-String -Pattern '^### B\d+\.'
& $pwsh -NoProfile -File ./eng/Test-Smoke.ps1
if ($LASTEXITCODE -ne 0) { throw 'Installed-tool smoke failed' }
```

Expected: exact installed assembly/XML hashes and declaration differences match, **59 immutable official sources** reproduce, every API/XML difference has an owner, **29 draft headings** appear, then one installed MCP smoke passes with zero failures/skips. A missing/changed SDK input, source hash, owner or comparison fails; the negative baseline check in [PIN-707 verification](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-707.md) verifies comparison rejection. The smoke builds/packs the ordinary artifact and exercises real installed stdio initialize/discovery/list/get/shutdown. No MCP tool or served recipe changed, so no changed-guidance Inspector result is claimed.

Limits: no proposed sample was restored, compiled or executed; no optional workload/native/image/service verification or GA reconciliation ran. The declarations bound review; they do not establish runtime compatibility, supported embedding, complete transitive package closure, all XML/IL semantics, or future source completeness. See U1–U5 and each delivery's prerequisites. The generated dump is excluded from PR Agent review under a single exact `.pragentignore` rule; its generator, disposition/source inputs, this audit and verification remain reviewable. No project/dependency/shared build/workload/CI configuration changed.

[sdk-p1]: https://github.com/dotnet/core/blob/ffed591b0fb8f9aab50ecc28f2c765049fb28d8f/release-notes/11.0/preview/preview1/sdk.md
[sdk-p5]: https://github.com/dotnet/core/blob/ffed591b0fb8f9aab50ecc28f2c765049fb28d8f/release-notes/11.0/preview/preview5/sdk.md
[sdk-p7]: https://github.com/dotnet/core/blob/ffed591b0fb8f9aab50ecc28f2c765049fb28d8f/release-notes/11.0/preview/preview7/sdk.md
[build-p6]: https://github.com/dotnet/core/blob/ffed591b0fb8f9aab50ecc28f2c765049fb28d8f/release-notes/11.0/preview/preview6/msbuild.md
[build-p7]: https://github.com/dotnet/core/blob/ffed591b0fb8f9aab50ecc28f2c765049fb28d8f/release-notes/11.0/preview/preview7/msbuild.md
[build-rc1]: https://github.com/dotnet/core/blob/ffed591b0fb8f9aab50ecc28f2c765049fb28d8f/release-notes/11.0/preview/rc1/msbuild.md
[nuget-p4]: https://github.com/dotnet/core/blob/ffed591b0fb8f9aab50ecc28f2c765049fb28d8f/release-notes/11.0/preview/preview4/nuget.md
[nuget-p6]: https://github.com/dotnet/core/blob/ffed591b0fb8f9aab50ecc28f2c765049fb28d8f/release-notes/11.0/preview/preview6/nuget.md
[nuget-p7]: https://github.com/dotnet/core/blob/ffed591b0fb8f9aab50ecc28f2c765049fb28d8f/release-notes/11.0/preview/preview7/nuget.md
[nuget-rc1]: https://github.com/dotnet/core/blob/ffed591b0fb8f9aab50ecc28f2c765049fb28d8f/release-notes/11.0/preview/rc1/nuget.md
[nuget-rc2]: https://github.com/dotnet/core/blob/ffed591b0fb8f9aab50ecc28f2c765049fb28d8f/release-notes/11.0/preview/rc2/nuget.md
[container-p3]: https://github.com/dotnet/core/blob/ffed591b0fb8f9aab50ecc28f2c765049fb28d8f/release-notes/11.0/preview/preview3/containers.md
[container-p6]: https://github.com/dotnet/core/blob/ffed591b0fb8f9aab50ecc28f2c765049fb28d8f/release-notes/11.0/preview/preview6/containers.md
