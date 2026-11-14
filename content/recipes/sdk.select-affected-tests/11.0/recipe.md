# Select affected tests with an experimental source map

## Goal

Collect local source-to-test maps and select mapped tests while retaining conservative full-suite fallback. **Experimental, private-preview packages.** The verified route keeps a `net11.0` project target but explicitly uses SDK **12.0.100-alpha.1.26509.101**, its preview reference pack and runtime **12.0.0-alpha.1.26509.101**. It is not a GA SDK11 solution or a .NET12 catalogue edition.

**Measured limit:** for a Germination-only source change, Germination selects its two mapped tests, while ColdStore runs its two tests because that changed source is unmapped in ColdStore. All four execute. This is the publisher's conservative behavior, not cross-module test elimination. Pingu accepted this bounded result on 14 November 2026. Do not promise a faster run without measuring the real suite.

## Older approach: keep the full suite

Cwm Seed Bank has independent Germination and ColdStore products and tests. The original rules remain the ordinary older baseline:

{{snippet:rules}}

An ordinary complete test run remains valid on .NET10 and .NET11. Keep its actual passing/failing results. This wrapper clears inherited affected mode in its child and preserves the real exit code:

{{snippet:baseline}}

## Newer approach: supported acquisition and compatibility

Use Microsoft's public `test-tools` feed for AffectedTests and matching CodeCoverage **18.13.0-preview.26519.2**. The exact extension package SHA256 is **8c6a546749559f1a567a68e6b940d97ec68a47f2e8679758b4ea8ff9a7dcc5ff**. Its `PACKAGE.md` identifies default local `.cts/mappings` storage without another provider. Its props register `Microsoft.Testing.Extensions.AffectedTests.TestingPlatformBuilderHook` through the MTP builder-hook mechanism. Generated MTP entry points consume that hook; CodeCoverage alone is insufficient.

The package carries **Microsoft .NET Library** terms in `License.txt`, not the documentation repository's MIT license. Review the exact package terms and third-party notices before redistribution. Public acquisition does not establish stable support. These instructions use local test consumers only and distribute no package binaries.

The real closure uses MSTest **4.6.0-preview.26519.1**, MTP/MSBuild **2.6.0-preview.26519.1**, and the matched affected/coverage preview pair. The package's MTP **2.6.0-preview.26515.10** dependency is a minimum, not the resolved version. Its net10.0 managed asset is selected for these net11 consumers. The win-x64 layout includes affected-test `Cts_x64.config`/`ctsrun64.dll` and the matching coverage profiler/symbol-reader payload. Keep the complete restored/copied native layout; missing symbols can produce identities without useful source associations.

**GA SDK11.0.100 control:** the ordinary four-test baseline passes and collection writes maps, but the SDK rejects the suppressed reporting handshake and returns1. Retain this failure; do not ignore handshakes, normalize exits, set SDK-owned mode markers or allow zero tests globally.

The exact SDK12 alpha is the publisher's documented orchestration route. Its `net11.0` reference directory belongs to the **12.0.0-alpha.1.26509.101** targeting pack; System.Runtime's assembly identity is **11.0.0.0**. The sample pins both the net11 TFM and the preview runtime explicitly. The reference-pack product version, emitted target, SDK and runtime are distinct identities. This conditional composition was actually built and executed; it does not establish general SDK12/net11 or future-package compatibility.

The linked storage documentation still returns404, and the package's stamped source revision is unavailable publicly. The working route below relies on the package's own documented **default local storage**, actual packaged registration hook, publisher change-policy example and observed consumer behavior. It invents no storage JSON or builder method and requires no Azure provider, account or token. Use this complete `global.json` policy:

{{snippet:consumer-global}}

## Complete local consumer

Save these files under a disposable `Consumer` directory. Product/test sources are separate so associations remain attributable. Each test project references only its corresponding product.

`global.json` selects the exact SDK/MTP runner and the publisher's `test.affectedTests.changes` policy. Omit `storage` to use the documented local default. Ignoring `docs/**` is an explicit repository policy, not a universal no-impact heuristic:

Use the `global.json` shown above.

`Directory.Build.props` explicitly separates target and runtime; `Directory.Build.targets` is an empty `<Project />`. `Directory.Packages.props` disables inherited central package management for this version-specific example. Keep these boundaries when reproducing it outside this repository:

{{snippet:consumer-props}}

{{snippet:consumer-packages}}

`NuGet.Config` declares only the reviewed feeds:

{{snippet:consumer-feed}}

`CwmSeedBank.slnx`:

{{snippet:consumer-solution}}

`Germination/Germination.csproj` and `Germination/Germination.cs`:

{{snippet:germination-project}}

{{snippet:germination-product}}

`ColdStore/ColdStore.csproj` and `ColdStore/ColdStore.cs`:

{{snippet:coldstore-project}}

{{snippet:coldstore-product}}

`Germination.Tests/Germination.Tests.csproj` and `Germination.Tests/Tests.cs`:

{{snippet:germination-tests-project}}

{{snippet:germination-tests}}

`ColdStore.Tests/ColdStore.Tests.csproj` and `ColdStore.Tests/Tests.cs`:

{{snippet:coldstore-tests-project}}

{{snippet:coldstore-tests}}

Save this original `Probe.cs` in **each** test project. It supplies independent execution markers and deliberate failure/startup/wait controls for verification. With those environment values unset it changes no test outcome:

{{snippet:test-probe}}

The checked-in lock files pin the full reviewed closure, including incidental legacy VSTest dependencies of the MSTest metapackage. Those dependencies are not the affected-test collector. Preserve both test locks; product locks contain an empty `net11.0` dependency object:

{{snippet:germination-lock}}

{{snippet:coldstore-lock}}

## Collection and selection

First initialize and commit the disposable repository, build it and run the ordinary full baseline. Generated `bin/`, `obj/`, `TestResults/` and `.cts/` must be ignored. Collection requires a clean repository and records per-module maps tied to that baseline, runtime, configuration and test metadata.

This wrapper runs one affected mode at a time. Supply the absolute SDK12 executable and disposable repository path; it preserves child exit behavior:

{{snippet:affected}}

Collection inventories test UIDs/source associations; its suppressed ordinary results are **not passing-test counts**. Use independent markers plus actual maps to prove collection work. Selection on unchanged inputs is a valid empty result: zero executed tests, no manufactured passes or skips. A module that does not launch is not a skipped test.

## Migration

1. Preserve an ordinary full-test lane and its failing-test control. Inventory actual module identities before changing selection.
2. Acquire the exact SDK, packages and complete win-x64 native layout. Keep compiler, reference-pack product, assembly identity, target and runtime distinct. Retain the GA11 handshake failure as a compatibility limitation.
3. Save the complete independent consumer above. Use the generated MTP entry points and packaged registration hook. Omit the storage property for default local mappings; do not invent provider/configuration APIs.
4. Commit a clean disposable repository, collect maps, and verify each module has its two exact discovered UIDs and real associations to its own product source.
5. Change Germination only. Verify its mapped identities execute **and ColdStore conservatively falls back to its full suite**. Do not call this cross-module elimination. Unknown source associations require conservative treatment.
6. Check unchanged/ignored-document empty selection, missing/stale/incompatible maps, intentional test failure, startup failure and active cancellation. Keep original process exits and truthful counters. Never suppress SDK protocol failures.
7. Refresh affected evidence after meaningful input/toolchain changes. Remove only resolved preview exceptions; the exact eight foundations, historical RC receipts and PIN-967 final release gate remain separate.

## Repeat the real checks

Save `Host/CancelSelection.cs` beside the Consumer directory. It creates a private hidden console and sends a real Ctrl+C after a test's marker proves active work. It records the SDK's nonzero exit and does not reinterpret cancellation as success:

{{snippet:cancel-selection}}

Save this as `samples/net11.0/SeedBankMap/Get-SelectionSdk.ps1`. SDK setup downloads the exact reviewed archive into a private directory and checks its recorded SHA512. It does not install globally or change PATH/shell profiles. The checkout's corresponding `eng/Get-SeedSelectionSdk.ps1` is a thin wrapper:

{{snippet:sdk-setup}}

Save this as `samples/net11.0/SeedBankMap/Test-Selection.ps1` and invoke it directly, or use the checkout's thin `eng/Test-SeedBankSelection.ps1` wrapper. It creates an isolated original Git repository, package cache, CLI home and local maps under `artifacts/seed-selection/`, then executes the complete consumer, including real failure and cancellation controls. `-Record` additionally requires the checkout's existing evidence recorder; ordinary verification needs only the complete files above:

{{snippet:selection-check}}

## Constraints

Do not combine collection and selection, device testing, parallel-module options or minimum-test policies. The SDK owns `DOTNET_CLI_TEST_AFFECTED_TESTS_MODE`; repository code must not manufacture it. Unknown associations cause conservative module fallback, not inferred no-impact. The exact preview SDK/reference-pack/runtime composition is required.

## Evidence and limitations

Observed: four ordinary baseline tests; two complete maps with real source associations; zero tests for unchanged inputs and ignored documentation; Germination's two mapped identities plus ColdStore's two-test conservative fallback; full fallback for missing/stale/incompatible maps; failing test exit2; startup/protocol failure remains nonzero; selection and collection cancellation remain nonzero. The cancelled collection publishes no reusable Germination map; another completed module's map is distinct.

Do not combine collection and selection, device testing, parallel-module options or minimum-test policies. The SDK owns `DOTNET_CLI_TEST_AFFECTED_TESTS_MODE`; repository code must not manufacture it. The test harness removes inherited markers and uses only `DOTNET_CLI_ENABLE_AFFECTED_TESTS=1` to request the SDK workflow.

Only Windows x64 and this precise private-preview composition are qualified. No remote storage, other native platform, performance saving, universal fallback behavior or stable SDK11 affected-test support is claimed. Experimental labels remain. PIN-967 still owns global candidate readiness.

## Official references

- [Documented MTP modes/restrictions](https://github.com/dotnet/docs/blob/f0021fde66c59559fa1ba3c9a044148738f12faf/docs/core/tools/dotnet-test-mtp.md)
- [Publisher rollout, including unmapped-module fallback](https://github.com/microsoft/testfx/blob/e3d67c4458315461201f6620374af1fbb9c4a722/docs/affected-test-selection.md)
- [Matching publisher SDK and change policy](https://github.com/microsoft/testfx/blob/e3d67c4458315461201f6620374af1fbb9c4a722/global.json)
- [Matching package pins](https://github.com/microsoft/testfx/blob/e3d67c4458315461201f6620374af1fbb9c4a722/eng/Versions.props)
- [Public test-tools feed](https://pkgs.dev.azure.com/dnceng/public/_packaging/test-tools/nuget/v3/index.json)
- [Original unavailable storage pointer](https://github.com/microsoft/codecoverage/blob/main/docs/cts/affectedteststorage.md)
