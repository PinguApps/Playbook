# SDK/package hosts and affected-test availability: PIN-688

Reviewed **21 October 2026**. This is a bounded source and distributable-metadata audit. Seven full compatibility notes and the affected-test claim have accountable dispositions. Nine narrow delivery proposals await backlog approval. No recipe, consumer implementation or release is delivered by closing this audit.

## Scope and provenance

[PIN-688](https://linear.app/pinguapps/issue/PIN-688), `SPEC.md` at `0a8989814ea11346354464b845c79c15bb71c5e9`, stories 47, 104, 105, 158 and 159; approved proposal `b3778cc`, key `SH_Q08`. Assigned inventory rows: `migration.sdk-package-hosts` and `sdk.select-affected-tests`. Read the target, empty comments/attachments, cited spec/proposal, current planning README, implementation/coverage/verification plans, glossary, ADRs 0001–0004, contribution and dependency policies, existing SDK recipes/evidence and adjacent audit conventions. Root PLAN.md, CONTEXT.md and CONTEXT-MAP.md are absent. The historical planning claim that implementation has not begun is stale: the installed-tool harness and SDK content exist.

Linear boundary: PinguApps team `62f80270-b641-4fa3-b60c-46be4c4df2c8`, Playbook project `b22380c7-6fde-49f4-8826-e5631dc5df7a`. The archived-inclusive, paginated overlap search returned **317** issues, all checked against both IDs. No native blocker exists. Full descriptions/comments were read for PIN-707, PIN-963, PIN-761, PIN-762, PIN-763, PIN-769, PIN-771 and PIN-923 after scoped discovery. None owns the additional goals below, and none is changed or treated as satisfied by this audit.

The [32-file source manifest](sdk-package-hosts.sources.json) pins immutable raw URLs and SHA-256 values. It includes the complete cumulative SDK page, RC1/RC2 SDK notes, compatibility index, all seven full mitigation notes, MTP command documentation, SDK orchestration/launch sources, NuGet restore/validation sources, VSTest message/serializer sources and TestFx's current rollout/configuration. The compatibility index remains incomplete. This audit does not replace PIN-707's wider public-tooling/API reconciliation or PIN-963's final GA reconciliation.

The [23-package asset manifest](sdk-package-hosts.assets.json) records actual downloaded ZIP hashes, `lib` framework directories, direct dependency unions, selected entry hashes and a bounded VSTest public-surface inspection. Dependencies are a **union across nuspec groups**, not a resolved consumer closure. PE metadata is read without loading or executing the reviewed assembly. Package presence, reference metadata and source expectations do not establish consumer compilation, native execution, SDK/extension interoperability or GA verification.

## Full-note dispositions

### SDK selection for tool execution

[dnx scripts bypass global.json][dnx-note]: introduced in SDK **10.0.302 / 10.0.400** and .NET 11 Preview 6. The wrapper enumerates installed SDKs and invokes the selected SDK's `dotnet.dll dnx` directly. An ambient `global.json` therefore does not pin that wrapper's SDK. The documented explicit alternative is `dotnet dnx`, which uses ordinary SDK resolution. No affected managed API is listed.

**Included → A.** This is a reusable choice between a version-independent wrapper and reproducible SDK selection. It is not solely a .NET 11 change. The serviced .NET 10 source at VMR `a8dd0577c5c13f43a2f57fe33b4ff11b90f6371e` already bypasses the pin, matching the installed 10.0.402 wrapper inspected here. Older VMR `4099f839...` has the muxer-based wrapper. Do not describe current .NET 10 as always respecting the pin, or promise that any pinned old SDK implements `dnx`. Neither wrapper SDK selection nor a package's major version establishes the executed tool's runtime requirement. PIN-763's package/install goal does not cover this SDK-selection decision.

### Removed automatic Mono launch

[Mono launch target removal][mono-note]: .NET 11 Preview 1 removes the Linux .NET Framework defaults for `RunCommand=mono` and `RunArguments` containing the quoted target plus start arguments. The note explicitly says this automatic scenario is no longer officially supported. It permits authors who still need it to supply those properties themselves. No managed API is affected.

**Included → B** for deliberate, explicitly unsupported legacy launch configuration; **excluded** as a promise to restore Microsoft support for .NET Framework on Linux. The old/new SDK targets confirm default removal, preserve explicit overrides and expose the `_CheckRunCommandSupported` guard. A surviving `dotnet run` command is not proof of supported runtime compatibility. A proposal must exercise real Mono on Linux, quoting, arguments and exits; source review alone cannot establish it. No existing SDK watch or native-platform ticket owns this legacy goal.

### Package ID restrictions

[NU5052 package IDs][id-note]: Preview 6; the note gates the new pack warning at `SdkAnalysisLevel >= 11.0.100`. IDs begin with an ASCII letter, digit or underscore, contain only ASCII letters/digits/dots/dashes/underscores and have at most 100 characters. The reviewed `PackageIdValidator.IsValidPackageId(string,bool)` first applies legacy validation, then its restricted branch rejects adjacent punctuation when **both** characters are dot or dash, including mixed `.-`/`-.`. Underscores remain valid. Pack warning versus feed acceptance and warnings-as-errors are separate concerns.

**Included → C.** Choose a compliant new identity and a deliberate consumer migration/deprecation plan. Do not recommend blanket warning suppression or actually publish/deprecate packages during verification. The documented NoWarn/older analysis-level opt-outs hide a warning; they do not change feed policy or migrate consumers. Public API presence in the pinned NuGet source does not prove its availability on every asset or old NuGet package.

### Deprecated Android package assets

[NU1703 MonoAndroid assets][android-note]: Preview 5. The warning concerns a .NET 11-or-later Android target, SDK analysis level at least 11.0.100 and **selected non-placeholder** MonoAndroid compile/runtime assets. It is not a warning for every package containing a MonoAndroid folder, every Android project or every older-target consumer. The reviewed `MonoAndroidDeprecation.ShouldCheck/ShouldWarn` code supplies those guards, including the empty-folder exclusion.

**Included → D.** Prefer a dependency version supplying modern `net6.0-android` or later assets and prove which files restore actually selects. Package-scoped suppression or an older analysis level is a temporary diagnostic choice, not evidence that an old binary works. PIN-923 owns target/runtime/JDK/manifest migration, not a package-asset replacement exercise; its title alone does not satisfy this requirement. Native compilation prerequisites must be established before delivery. No recurring device CI is added.

### Restore's next-best version search

[Global packages folder search][restore-note]: RC1 changes the search when a nonfloating requested minimum is absent. Only configured package sources supply the next-best version: a cache-only intermediate version must not win over a higher source version. A source without a suitable version now fails NU1102 rather than succeeding from that intermediate cache entry. Selecting a higher source version can still produce NU1603. **An exact requested cached version remains reusable.**

**Included → E.** `nuget11-provider` checks an inclusive nonfloating exact minimum first, then returns no next-best match for global/fallback-folder providers. The resolver itself is not sufficient evidence: the provider bounds the candidates. Do not generalize this to disabling the global cache, all floating/range resolution, vulnerability cooldown or cache cleanup. PIN-769's cooldown/vulnerability selection is a different policy. The delivery needs controlled feeds and isolated caches, including the unchanged exact-hit case.

### Template-engine host compatibility

[Template-engine netstandard removal][template-note]: Preview 4 removes the netstandard2.0 route from eight packages. The full note lists Microsoft.TemplateEngine.Abstractions, Core, Core.Contracts, Edge, Orchestrator.RunnableProjects, Utils, IDE and the localizer. Its localizer link names the actual package **Microsoft.TemplateEngine.TemplateLocalizer.Core**. This is an embedded template-host migration, not a restriction on applications generated by `dotnet new`.

**Included → F.** All eight downloaded **10.0.402** packages contain `lib/netstandard2.0`, `net8.0`, `net10.0` and `net472`. All eight **11.0.100-rc.2.26504.105** packages instead contain **net10.0, net11.0 and net472**. The note's **net9.0** remaining-target recommendation does not match these RC2 ZIPs. Do not advertise a net9 host route from it. net10/net11 or .NET Framework net472 are candidate routes, subject to real restore/build/run evidence and transitive asset selection. Edge's nuspec includes NuGet **8.0.0-rc.605** dependencies; a broad package framework list does not establish a complete compatible closure. The note's unchanged-public-API assertion is not a full binary API diff performed here.

PIN-761's CLI template installation and PIN-771's NuGet-client host are adjacent, distinct goals. This audit neither duplicates their examples nor expands PIN-771 to template embedding.

### VSTest's JSON dependency and public surface

[VSTest Newtonsoft removal][vstest-note]: Preview 4 removes the platform-provided Newtonsoft dependency. Test source using JsonConvert/JToken/JObject needs its own dependency; runtime consumers and adapters/collectors must ship the assets they use. Excluding Newtonsoft runtime assets can no longer rely on the VSTest copy. Modern .NET uses System.Text.Json in the platform; the note identifies JSONite for .NET Framework.

**Included → G** for owning a test/extension dependency and **included → H** for code using removed communication APIs. These are distinct fixes. Adding Newtonsoft cannot restore `Message.Payload`, `VersionedMessage` or the old public serializer/converter contracts. The note names DefaultTestPlatformContractResolver, TestCaseConverter, TestObjectConverter, TestPlatformContractResolver<T>, TestResultConverter and TestRunStatisticsConverter. The actual 18.7.0 TestHost DLL exposes the old named surface; **18.11.0** exposes Message's `MessageType`, `Version`, `RawMessage`, with the old converter/resolver classes absent from that public surface. Some converters remain **internal**; they are not public migration targets. The serializer source provides `DeserializeMessage` and `DeserializePayload<T>` for a possible typed route, which still needs a real consumer test.

The downloaded Microsoft.NET.Test.Sdk **18.7.0 / 18.11.0** nuspecs depend on matching TestHost for net8.0; the old TestHost directly depends on Newtonsoft **13.0.3**, the new one does not. This is actual package evidence, not an assumption that package version 11 corresponds to .NET 11. The installed .NET 11 RC2 SDK's `vstest.console.deps.json` / `testhost.deps.json` use **18.11.0-release-26504-105** platform components without Newtonsoft. Its SDK-root `dotnet.deps.json` still has Newtonsoft **13.0.4** for other tooling. Finding that DLL somewhere in the SDK does not make it a test project's dependency. The downloaded 18.11.0 package and SDK-bundled release-build identity are recorded separately, not asserted byte-identical.

PIN-762's runner/project selection does not deliver either migration, and PR #52's tracker comment is evidence of separate work awaiting review, not content present on this audit's base. No changes from that PR were copied.

## Affected-test availability reconciliation

**Included experimental goal → I; .NET 11 composed-consumer compatibility remains unverified.** The cumulative SDK page and MTP command page describe `DOTNET_CLI_ENABLE_AFFECTED_TESTS=1`, `--collect-test-map` and `--affected-tests`. Canonical RC1 explicitly withheld a complete workflow because the separate extension/storage contract was unavailable; RC2 adds no distribution resolution. SDK RC2 source does contain gated orchestration and forwarded-option validation. Flags, a successful empty run or source presence do not prove correct test selection.

The current official **TestFx** rollout source changes the availability conclusion. It points to the public [test-tools feed](https://pkgs.dev.azure.com/dnceng/public/_packaging/test-tools/nuget/v3/index.json), pins **18.13.0-preview.26519.2** in `eng/Versions.props` and supplies repository-specific `global.json` policy and Azure DevOps storage wiring. Direct nuget.org queries for Microsoft.Testing.Extensions.AffectedTests returned 404, while that feed actually supplied the extension, its AzureDevOps storage provider and matching CodeCoverage ZIPs. A nuget.org-only absence claim would be wrong.

Actual package contents qualify the source's historical **18.13.0-preview.26508.3** net8.0 rollout description:

- The downloaded **26519.2** extension/provider/CodeCoverage packages expose **netstandard2.0 and net10.0**, not net8.0. Their nuspecs require MTP **2.6.0-preview.26515.10**; TestFx separately pins **2.6.0-preview.26519.1**. This audit has not restored the combined closure.
- The extension's packaged `PACKAGE.md` labels it private experimental, says it registers separately from CodeCoverage, supports local **.cts/mappings** without a provider, uses a packaged **Windows x64** collector and does not ship the formerly separate Collector DLL. Both the readme and win-x64 native payload were inspected and hashed. Installing CodeCoverage alone does not enable selection; a public feed is not a stable support promise. License acceptance and supported use must be retained during delivery.
- AzureDevOps storage is optional for a local example. No account, token, pipeline provisioning or Azure resource is required or authorized by this audit. Its provider ZIP confirms actual availability, not successful remote storage behavior.
- TestFx's current known-good rollout deliberately gates on **SDK 12.0.100-alpha.1.26509.101** for collection/empty-selection handshakes. That is not proof of .NET 11 RC2 interoperability. The extension may target an older runtime while requiring a newer SDK; keep those facts separate. Do not backport or suppress handshake/error checks to manufacture .NET 11 support.
- The package's link to `microsoft/codecoverage/docs/cts/affectedteststorage.md` is **404** on both the embedded repository commit and public main. That explicit pointer is unavailable. The packaged readme and pinned TestFx implementation/configuration are available; they do not justify inventing a complete public configuration schema or local fallback contract.

The broad command docs prohibit combinations with devices, parallel modules and minimum-test policies. The pinned SDK parser explicitly rejects collection with parallel/minimum settings; its internal `DOTNET_CLI_TEST_AFFECTED_TESTS_MODE` marker is an SDK/extension contract, **not an application opt-in to set manually**. Keep the publisher's supported restrictions until real consumer evidence settles finer distinctions. TestFx's source records rollout and test evidence; this audit did not execute those tests, download an SDK 12 toolchain or validate the publisher's pipeline.

This resolves **asset discovery** without claiming a complete .NET 11 workflow. I must establish a supported SDK/extension/storage combination and real selection/failure evidence before publication. If only a newer SDK works, validate the .NET 11 target route explicitly and disclose its SDK requirement; otherwise retain the .NET 11 compatibility gap. The accepted experimental goal is not silently dropped or counted as recipe coverage.

## Approval-only delivery proposals

**None created or implemented.** The 317-issue overlap search found no existing owner for these nine narrow goals. Use the same team/project, Backlog, native **S=2 / M=3 / L=5** estimates covering source reconciliation, original samples/prose, actual consumers and installed-tool checks. Existing maximum allocations are **03-012** and **07-038**. Proposed numbers below are not reservations: recheck overlap, unpublished proposals and next free numbers before approved creation. No milestone creation or existing title changes are proposed.

Build proposals A/B/C/E/F/G/H/I belong to existing milestone **03 - Build and deployment guidance**, `7b3876e4-5f75-4b54-a375-cc598059679d`. D belongs to **07 - Mobile and native guidance**, `7526654a-0b01-441f-952d-4952fca5271f`. Link approved work to PIN-688 and the comprehensive publication gate PIN-959; preserve PIN-707/PIN-963's reconciliation duties. Numeric order and shared files create no blockers. Required external/native access must become an explicit prerequisite before implementation.

### Shared delivery acceptance

1. Write wholly original offline recipes and fictional same-goal older/newer examples; preserve stable IDs/editions, exact eight .NET 10 foundations and canonical snippets. Include discovery ID/title/**goal**, full Markdown, migration, constraints, alternatives and official references. Add no historical foundation or extra native CI matrix.
2. Separate runtime/target, SDK, language, package assets, platform, stability and verification baselines. Starting comparison pins are SDK **10.0.402 / 11.0.100-rc.2.26504.105**, runtime **10.0.13 / 11.0.0-rc.2.26504.105**, C# **14 / 15** where relevant. Proposal-specific earlier serviced SDKs, Framework/Mono or SDK 12 are exceptions to verify and disclose, not a release-wide change. Package pins below are audited assets or proposed inputs, not verified consumer closures. Follow the dependency policy for every new project/SDK/package/lock; keep the host workload-free and update policy inventory when needed.
3. Actually compile and run the claimed old/new consumers and focused failures below in isolated feeds/caches/homes. Capture selected assets, exact toolchain/dependencies, commands, output and hashes through the existing evidence system. Source expectations, ZIP metadata, mocks and a successful process exit alone do not prove a recipe. Missing required behavior or compilation blocks its PR.
4. Build/pack the ordinary Playbook tool, install that exact artifact through the existing harness, and retrieve discovery goal plus complete Markdown/applicability/edition/evidence **offline from an unrelated directory**. Check canonical snippets and unsupported targets, then update the register. No account/resource publication, merging or release is authorized. RC and experimental proof never clear the GA/publication gates automatically.

### A. 03-013 - Select the intended SDK when executing a tool — S (2)

Proposed ID `sdk.select-tool-execution-sdk`. A fictional museum runs a local catalogue-check tool under a repository SDK pin. Demonstrate wrapper versus explicit `dotnet dnx` selection with actual distinct compatible SDKs and a local pinned tool package; record selected SDK and tool runtime independently. Cover a pre-dnx pin's failure, a serviced 10.0.402 pin, a newer installed SDK, quoted paths and argument forwarding without external package execution. Verify the SDK servicing boundary rather than labeling this .NET 11-only. No global PATH/environment mutation or system installation. Sources: dnx note, `dnx-cmd`, `dnx-sh`, `sdk10-serviced-dnx`. Related: PIN-763.

### B. 03-014 - Configure an explicit legacy Mono launch — M (3)

Proposed ID `sdk.configure-legacy-mono-launch`. A fictional observatory retains a small Framework command-line importer on Linux. Establish an isolated Linux/Mono authoring route and exact Framework reference assets before starting. Compare actual SDK 10/11 default evaluation and run behavior, then explicit RunCommand/RunArguments with a quoted assembly path, start arguments, working directory and nonzero process exit. Compile and execute the legacy consumer on Mono; prove cleanup. State the publisher's unsupported status prominently and present migration to supported .NET as an alternative. No promise of universal Framework compatibility or Windows-to-Linux emulation. If the necessary runtime cannot be supplied, this proposal remains blocked rather than source-verified. Sources: mono note, `sdk10-run`, `sdk11-run`.

### C. 03-015 - Choose a compliant package identity during migration — M (3)

Proposed ID `nuget.migrate-package-identities`. A fictional wildlife charity changes a restricted legacy package ID. Pack original local packages under SDK 10/11 with explicit analysis levels. Verify NU5052, warnings-as-errors, valid underscores and mixed adjacent punctuation; distinguish legacy invalid-ID rejection from the new warning. Demonstrate old/new consumer references and dependency identity using a local feed, with a documented deprecation/replacement plan that performs no public write. Prove package contents/installability after renaming and avoid assuming feed validation equals pack behavior. Sources: ID note, `nuget-id`.

### D. 07-039 - Replace a dependency selected from legacy Android assets — M (3)

Proposed ID `android.replace-monoandroid-package-assets`. A fictional coastal-trail application consumes original local packages with legacy and modern Android assets. Establish actual Android SDK 10/11 workload/toolchain access, then restore/build real projects and inspect project.assets.json's compile/runtime selection. Trigger NU1703 only with the stated net11-android/analysis guards and nonempty selected assets; show a modern dependency removes the cause. Check an older target and the temporary package-scoped diagnostic choice without claiming suppression repairs compatibility. Record real native compilation and any independently required device proof; no device matrix. Sources: Android note, `nuget-android`. Related: PIN-923, which retains its platform/runtime/JDK scope.

### E. 03-016 - Restore dependencies independently of next-best cache entries — M (3)

Proposed ID `nuget.restore-from-declared-sources`. A fictional research station uses an original local feed and separate cache seeds. Execute SDK 10/11 restores with an absent requested minimum, a cache-only intermediate version and a higher source version; inspect selected assets and NU1603. Remove the higher source version and verify NU1102 on the new route. Then prove an exact cached requested version remains usable. Include a transitive request, explicit source configuration and a separate uncached consumer. Use package versions that actually exist on the intended sources; do not wipe shared caches or broaden claims to floating versions. Sources: restore note, `nuget11-provider`. Related: PIN-769.

### F. 03-017 - Retarget an embedded template-engine host — L (5)

Proposed ID `sdk.retarget-template-engine-hosts`. A fictional archive embeds an original template in a small host. Compare actual **10.0.402 / 11.0.100-rc.2.26504.105** template packages and all eight removed netstandard routes. Prove old netstandard consumption, new incompatible restore, and at least one real supported host that restores/builds/runs template creation with the actual dependency closure. Inspect all advertised routes independently; never infer Framework/net10 compatibility from ZIP directories. Demonstrate generated application TFM is separate from host TFM. Resolve the note's net9-versus-net10 discrepancy using selected assets. An ordinary net10 route is preferred; Framework is not advertised without its own evidence. Sources: template note, `template-edge-project`, sixteen template ZIPs. Related: PIN-761 and PIN-771.

### G. 03-018 - Own JSON dependencies in test projects and extensions — M (3)

Proposed ID `testing.declare-json-dependencies`. A fictional weather station's test project and minimal test extension use independently authored JSON data. Compare real **Microsoft.NET.Test.Sdk/TestHost 18.7.0 / 18.11.0** consumers, showing the old transitive dependency and new compile failure, then an explicit Newtonsoft dependency with runtime assets. Exercise actual test execution plus adapter/collector loading and the missing-runtime failure where taught. Start with Newtonsoft **13.0.3 / 13.0.4** only as comparison candidates; review the final pin and lock. Distinguish SDK-root copies, test output and MTP/VSTest runner choice. Adding a package is not a remedy for removed public APIs. Sources: VSTest note, four test-package ZIPs. Related: PIN-762.

### H. 03-019 - Read VSTest messages through the supported typed contract — M (3)

Proposed ID `testing.migrate-vstest-message-readers`. A fictional laboratory's small test-tool bridge reads a known message payload. Compile against the actual old/new CommunicationUtilities assets and cover every removed public member/type named in the note, grouping converters only when the original same-goal example demonstrates the replacement. Exercise a real serialized message through DeserializeMessage/DeserializePayload<T>, including null/malformed payload and protocol version handling. Do not access remaining internal converters, fabricate a replacement for each old resolver, or assume a direct Newtonsoft reference restores binary compatibility. Verify the supported acquisition/deployment route for these platform assemblies before implementation; if no public replacement supports a subgoal, retain that limitation explicitly. Sources: VSTest note, `vstest11-message`, `vstest11-serializer`, TestHost public metadata. Related: PIN-762 and G; no blocker between G/H solely from adjacency.

### I. 03-020 - Select affected tests with a verified experimental map — L (5)

Preserve proposed stable ID **`sdk.select-affected-tests`**. A fictional seed-bank repository has two independent managed test modules and original product code. First establish the exact permitted package/license, SDK, MTP, Windows x64 collector and **local** storage/registration contract. Starting assets: extension/CodeCoverage **18.13.0-preview.26519.2**, MTP dependency **2.6.0-preview.26515.10**, with TestFx's **2.6.0-preview.26519.1 / SDK 12.0.100-alpha.1.26509.101** known-good rollout as evidence to reconcile. Verify actual acquisition and composed consumers; never invent the missing storage documentation or assume RC2 compatibility. A .NET 11 target under a newer SDK is advertised only after actual proof; no .NET 12 recipe backfill is authorized.

Collect actual source associations and test identities, change one product module, then prove only the correct tests execute; also verify conservative fallback on missing/stale/incompatible maps, an unrelated change, a deliberately failing affected test, valid empty selection, protocol/startup failure and cancellation. Record honest counts: collection is not full-test validation and an unselected module is not a skipped/passed test. Preserve the declared device/parallel/minimum restrictions and SDK authorization marker ownership. Keep an ordinary full-test baseline; no global zero-test-success or ignored-handshake workaround. Use isolated local mappings; remote storage is outside this narrow delivery. If a supported combination or required compatibility cannot be established, leave the recipe/publication gate incomplete with evidence. Sources: RC1/RC2, SDK test sources, TestFx rollout/configuration and three feed ZIPs. Related: PIN-762, PIN-707, PIN-963.

## Reproduce the audit

Requires PowerShell 7 and public network access. Downloads are temporary, with no installation or reviewed-code execution. The script validates the reviewed source bytes, seven full-note section sets, package identities/hashes/frameworks/direct dependency unions/selected entries, bounded old/new VSTest public metadata and nine proposal headings. It displays actual package framework lists, the VSTest public surface and proposals. Read the dispositions for semantic conclusions: hash replay cannot prove those conclusions or run the proposed consumers.

The command also checks four hashed files from the already installed win-x64 RC2 SDK, including its product identity and three dependency manifests. That SDK is present at the literal path below. On another machine, supply an equivalent SDK directory or omit `-Net11SdkDirectory` to replay only public sources/packages; omission does not reverify the saved installed-SDK observation and never installs an SDK.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-c76ebb7d'
& ./eng/Inspect-SdkPackageHosts.ps1 -Net11SdkDirectory 'C:\Users\pingu\.dotnet-playbook-11\sdk\11.0.100-rc.2.26504.105'
```

Expected: **PASS: 32 immutable source hashes, seven full notes, 23 package ZIPs and nine approval-only proposals.** Any unavailable source, changed hash, package metadata/API mismatch or missing proposal fails. This block was tested from an unrelated directory. The [verification ledger](../../verification/PIN-688.md) records actual checks and tracker coverage. MCP tools and served recipes are unchanged; this audit makes no claim of new installed guidance.

[dnx-note]: https://github.com/dotnet/docs/blob/f0021fde66c59559fa1ba3c9a044148738f12faf/docs/core/compatibility/sdk/11/dnx-scripts-bypass-global-json.md
[mono-note]: https://github.com/dotnet/docs/blob/f0021fde66c59559fa1ba3c9a044148738f12faf/docs/core/compatibility/sdk/11/mono-launch-target-removed.md
[id-note]: https://github.com/dotnet/docs/blob/f0021fde66c59559fa1ba3c9a044148738f12faf/docs/core/compatibility/sdk/11/nuget-pack-nu5052-packageid.md
[android-note]: https://github.com/dotnet/docs/blob/f0021fde66c59559fa1ba3c9a044148738f12faf/docs/core/compatibility/sdk/11/nu1703-deprecated-monoandroid-framework.md
[restore-note]: https://github.com/dotnet/docs/blob/f0021fde66c59559fa1ba3c9a044148738f12faf/docs/core/compatibility/sdk/11/restore-global-packages-folder-version-search.md
[template-note]: https://github.com/dotnet/docs/blob/f0021fde66c59559fa1ba3c9a044148738f12faf/docs/core/compatibility/sdk/11/template-engine-netstandard.md
[vstest-note]: https://github.com/dotnet/docs/blob/f0021fde66c59559fa1ba3c9a044148738f12faf/docs/core/compatibility/sdk/11/vstest-removes-newtonsoft-json.md
