# Select affected tests with an experimental source map

## Goal

Use this when a large managed test suite needs a faster change-validation lane while retaining an ordinary full-test baseline. **Experimental, documentation-based guidance.** The original examples and command shapes below have not been compiled or executed as a composed affected-test consumer. The local registration/configuration documentation is unavailable; do not treat this as a turnkey setup or a verified .NET 11 RC2 route. Pingu authorized documentation-based delivery on 3 November 2026 while unreleased execution proof remains pending.

## Older approach: run the complete suite

Cwm Seed Bank has independent `Germination` and `ColdStore` product projects, each referenced by its own managed MTP test module. Keep product sources separate too: a single shared source file would confound a source-to-test isolation exercise. These original rules show the two product behaviours; split them into their respective projects when composing the solution:

{{snippet:rules}}

The original test bodies belong in `Germination.Tests` and `ColdStore.Tests` respectively. They illustrate framework assertions, not extension registration. Adapt imports and package pins to the repository's supported MTP framework. The checked-in evidence projects contain only the rules; the `.cs.txt` file is an illustrative host integration input, not a compiled test host.

{{snippet:tests}}

On .NET 10, the dependable baseline is an ordinary complete test run with the repository's existing supported runner configuration. Do not claim that .NET 10 has a validated equivalent of this experimental SDK workflow. This wrapper clears affected-mode inheritance only in its child and propagates the actual exit code:

{{snippet:baseline}}

## Newer approach: collect, then select

The .NET 11 MTP command documentation introduces separate `--collect-test-map` and `--affected-tests` operations at RC1. Enable them with `DOTNET_CLI_ENABLE_AFFECTED_TESTS=1`; never combine them. Select MTP through `global.json`'s `test.runner` value `Microsoft.Testing.Platform`. Every selected module must be an MTP application and register the separately distributed extension. Installing CodeCoverage alone does not register affected-test selection.

{{snippet:affected}}

These are commands for an **already configured** repository. The wrapper does not install packages, configure change detection, register the builder hook or create storage. It changes no persistent environment variables or PATH. Pass an absolute SDK executable and a disposable absolute repository path. The SDK-owned `DOTNET_CLI_TEST_AFFECTED_TESTS_MODE` marker must not be set by repository code or pipeline scripts.

### Artifact and support boundary

The source audit inspected `Microsoft.Testing.Extensions.AffectedTests` and matching CodeCoverage **18.13.0-preview.26519.2** from Microsoft's public `test-tools` feed. The extension package re-downloaded for this work matches SHA-256 `8c6a546749559f1a567a68e6b940d97ec68a47f2e8679758b4ea8ff9a7dcc5ff`. Its readme labels it **private experimental**. Its license is Microsoft .NET Library terms, not the documentation repository's MIT license; retain and review the exact package terms and third-party notices before use or redistribution. Public downloadability is not a stable support promise.

Audited assets are `netstandard2.0` and `net10.0`, with an MTP dependency minimum **2.6.0-preview.26515.10**, and a packaged **Windows x64** collector. TestFx separately pinned MTP **2.6.0-preview.26519.1**. Runtime/TFM compatibility, the native collector layout and SDK orchestration compatibility are separate requirements; a package's TFM does not establish a complete working solution.

The packaged readme documents local `.cts/mappings` storage without an additional provider. Its linked `docs/cts/affectedteststorage.md` returns 404. The exact local storage configuration, change-policy schema and registration hook are therefore **unresolved here**. Do not invent a JSON property or builder method. Consult matching publisher documentation when available; until then use the ordinary full-test baseline. No Azure provider, account, token or remote storage is needed or authorized by this local-only recipe.

The pinned TestFx rollout documents SDK **12.0.100-alpha.1.26509.101** for collection and empty-selection handshake fixes. Its newer moving documentation advances affected-test packages to **18.13.0-preview.26530.1**; that is source drift, not this recipe's reviewed package pin. Neither their Azure DevOps rollout nor SDK 11 RC2's command flags prove this local consumer works. This is a target-11 documentation edition, not a .NET 12 recipe or an advertised SDK12/net11 compatibility result. The sample SDK11 pin identifies intended inputs only.

## Migration

1. Preserve a full-test lane and its passing/failing results. Inventory the two independent product/test modules, source paths and stable test identities before changing selection. Keep the ordinary baseline available even after adopting a map.
2. Reconcile SDK, runtime, framework runner, MTP, extension, CodeCoverage, package license and win-x64 native files as one pinned consumer set. Use isolated homes/caches and disposable inputs. Preserve shared SDK installations, PATH and shell profiles. An SDK installation or version listing alone is not compatibility evidence.
3. Obtain the matching local storage/registration contract and explicitly configure the repository change policy. This prerequisite is pending, not an invitation to guess. Keep mappings local to the disposable seed-bank repository; do not reuse another repository's maps.
4. Once configured, use the collection command independently of ordinary validation. Verify it records both test identities and actual product-source associations for each module. Native instrumentation that yields identities without source paths is not a useful map. Collection counts describe map inventory, not passing tests.
5. Change only Germination's product source, rebuild as required by the chosen host and run selection. Inspect actual test identities: only the Germination tests should be selected. An unlaunched ColdStore module is **not selected**, not passed or skipped. Retain actual test outcomes and process exits.
6. Exercise the checks below before trusting the faster lane. If mapping validity or configuration cannot be established, deliberately run the full suite and report why. Treat unknown completeness conservatively; do not claim the undocumented extension automatically covers every fallback case.
7. Refresh maps after the supported change-policy/format/dependency conditions require it. Restore an ordinary full run when rolling back; remove affected-mode configuration from that lane. Record exact inputs, dependencies and executed commands through the existing sample-evidence workflow before changing this recipe's pending status.

## Constraints

These are **acceptance checks to perform**, not observed results:

| Scenario | Required observation |
| --- | --- |
| Full baseline | Both modules' four assertions execute; a deliberate failure fails the run. |
| Collection | Both modules have stable test UIDs and real product-source associations; no invented passed-test count. |
| Germination-only change | Actual Germination identities execute; ColdStore is reported as not selected. |
| Missing, stale or incompatible maps | Establish the documented fallback; otherwise explicitly run the full baseline. Never silently omit tests. |
| Unrelated change | Distinguish a supported no-impact decision from missing associations; use a full run when uncertain. |
| Failing affected test | Nonzero failure remains visible, with the failing identity and actual counters. |
| Valid empty selection | Explicitly report no selected tests; do not manufacture passes/skips or globally allow zero tests. |
| Startup/protocol failure | Nonzero exit or required-handshake failure remains failure, even in affected mode. |
| Cancellation | Stop the run, preserve cancellation status and avoid treating partial mappings as complete. |

Do not combine affected-test operations with device testing, parallel test modules or minimum-test policies. Do not ignore handshake failures, normalize child failures, or use a global zero-test-success switch. Do not infer test counts for an unselected module. No timing/performance improvement is claimed until measured against the full baseline.

## Evidence and limitations

Official command documentation, pinned TestFx rollout/configuration and the source audit/package readme support this provisional guidance. Source-review records contain original input hashes; they are not resolved dependency closures, compiler output, native execution or selection evidence. Both example TFMs remain uncompiled. Installed Playbook inspection verifies delivery of this text and its warnings, not the described experimental behaviour. Rendering is not applicable; no UI, provider or recurring platform matrix is added.

Release-ready proof remains pending. The exact eight .NET 10 foundations are unchanged; .NET 10 examples are comparisons only. PIN-959 comprehensive content/evidence and PIN-963 actual-GA reconciliation remain open. All unavailable subgoals above remain visible instead of being counted as verified coverage.

## Official references

- [.NET 11 MTP command documentation](https://github.com/dotnet/docs/blob/f0021fde66c59559fa1ba3c9a044148738f12faf/docs/core/tools/dotnet-test-mtp.md)
- [Pinned TestFx affected-test rollout](https://github.com/microsoft/testfx/blob/e3d67c4458315461201f6620374af1fbb9c4a722/docs/affected-test-selection.md)
- [Pinned TestFx versions](https://github.com/microsoft/testfx/blob/e3d67c4458315461201f6620374af1fbb9c4a722/eng/Versions.props)
- [Public test-tools feed](https://pkgs.dev.azure.com/dnceng/public/_packaging/test-tools/nuget/v3/index.json)
- [Publisher's local-storage pointer — unavailable at review](https://github.com/microsoft/codecoverage/blob/main/docs/cts/affectedteststorage.md)
