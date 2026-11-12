# GA desktop and mobile reconciliation

PIN-966, 12 November 2026. Source/artifact reconciliation is separate from
compilation, native execution and permission to publish. PIN-967 remains open.

## Delivered A/B corrections

PIN-1315/PIN-1316 retain the original audit/RC receipts at `cb318c7`.
The current register binds the updated catalogue. All44 native editions name
workload set11.0.100.1 (package11.100.1), Android37.2.31, Apple27.2.11039 and
MAUI11.0.0; seven desktop editions retain their inputs. Both complete18-entry
maps are retained in the linked generated register. Apple requires Xcode27.2/macOS Tahoe26.6+; the upstream16.6+
host conflict stays explicit. Android uses JDK21/25 and matching API37 SDK/build
tools, with Gradle9.1+ when invoked using JDK25. Binding levels and app deployment
floors remain distinct. NativeAOT, Hot Reload, AI and backend limitations remain.

Served Apple applicability replaces stale RC packs; AppleRoutes/AppleAuthoring
source reviews now pin GA34b3b25d. Their existing runners refresh reference
compilation, binding generation and real plist/strip task output. Five affected
.NET10 comparisons are rebuilt. Scene guidance explains MT7199 merged-plist
validation and its SDK27+ iOS/Catalyst, extension/framework and dynamic-config
boundaries. MT7199 is source-reviewed here; native launch is not tested.

`eng/Test-AndroidGaContracts.ps1 -Record` verifies SHA256-pinned actual37.2.31
packages: CS0619 for direct CreateDelegate calls; current generator-produced
SafeInvokeFunc callbacks compile; actual defaults/diagnostic tasks emit XA1044
for explicit RunAOTCompilation=false. Corrected PublishReadyToRun=false and
MonoVM controls pass. This is isolated task/reference proof, not an Android
workload build. Java.Interop's System.Runtime10 reference produces CS1701;
generated nullable warnings remain visible. Neither implies runtime proof.
The canonical negative/corrected inputs extend the two existing recipe goals.
CoastalProbe sources/artifacts retain historical comparison identities; the new
runner independently pins and exercises GA37.2.31. Retained Android RC template
sources in mobile diagnostics/testing document historical implementation review,
not the selected GA workload composition or GA device execution.

Current receipts retain exact sample/input identities. Historical RC results,
native exclusions, feasible Human/provider gates and PIN-967 remain intact.
The generated register retains the full GA API comparison, platform-specific availability and
experimental dispositions. No new platform support is inferred from a base
workload. E–F, resource provisioning, maps requests and release remain excluded.

## Delivered C/D corrections

PIN-1317/1318 preserve both recipe IDs. GA XAML source review removes consent while retaining SourceGen, ambiguity and async-lambda limits; Incremental Hot Reload stays preview. AI discovery withdraws the stable 11.0.0 acquisition claim and makes net11 applicability conditional. The independently published preview retains MAUIAI0001; text chat and status APIs are source-reviewed. The nuspec declares AI.Abstractions10.4.1 and DI/Hosting/Logging.Abstractions10.0.5 for Apple. An isolated real net11 neutral restore selected the net10.0 asset and resolved AI.Abstractions10.4.1 plus the preview package; that neutral selection proves acquisition only, not Apple dependency closure. Apple resources.zip contains EssentialsAIiOS.xcframework with device/simulator bridge binaries. Source project builds that bridge using XcodeProject and AppleBindings.targets. Native deployment and model execution remain not-tested. PIN-967 still owns final readiness; historical RC receipts are not GA execution.

## Authority and complete boundary

Scope follows SPEC.md at `0a8989814ea11346354464b845c79c15bb71c5e9`,
stories 41–44, 47, 93–95, 130, 132–133, 146 and 158–160; proposal `b3778cc`
(`GA_NATIVE`), ADRs 0001–0004 and the release plan. Root PLAN.md, CONTEXT.md
and CONTEXT-MAP.md are absent. Explicit provenance resolves. All listed native
delivery prerequisites are Done in PinguApps team
`62f80270-b641-4fa3-b60c-46be4c4df2c8`, Playbook project
`b22380c7-6fde-49f4-8826-e5631dc5df7a`. Done is not deferred execution proof.

The [generated register](ga-native-contracts.sources.json) binds **51 prepared
11.0 editions**: 44 mobile/native and seven desktop editions. Selection includes
the `mobile` diagnostic/testing recipes through their Android/Apple areas.
Every edition retains its ID, stability, applicability, metadata/Markdown hashes,
saved evidence and Human boundary. Current labels are 47 stable, two preview
and two experimental; these are observations, not endorsements. The eight
historical foundations remain unchanged. PIN-966 itself changed no served content or samples; the A/B delivery above updates them.

The retained [desktop](windowsdesktop.md), [mobile API](mobile-api-gaps.md) and
[composition](mobile-baselines.md) audits remain RC2 records. Their hashes,
source decisions and deliberately experimental contracts are not relabelled GA.
The current register downloads actual archives and compares all 142 shipped and
unshipped MAUI PublicAPI files plus 18 supporting implementation/test sources
between RC2 `d32b7992` and actual GA package source `4e6582f7`. It checks the
complete GA source-tree file set, so added/deleted API files cannot silently vanish.

## Actual composition and conflicting prose

The official [release feed](https://builds.dotnet.microsoft.com/dotnet/release-metadata/11.0/releases.json)
identifies SDK 11.0.100/runtime 11.0.0. Actual package ZIP identities, nuspec
provenance/dependencies and shipped asset hashes are recorded by
[Inspect-GaNativeContracts.ps1](../../../eng/Inspect-GaNativeContracts.ps1).
No workload is installed and no downloaded code is executed.

| Artifact | Actual inspected selection and disposition |
| --- | --- |
| Workload set CLI `11.0.100`, package `Microsoft.NET.Workloads.11.0.100/11.100.0` | All 18 entries inspected. MAUI 11.0.0, Android 37.2.29, Apple 27.2.11039. This is not the retained RC2 map. |
| Workload set CLI `11.0.100.1`, package `11.100.1` | Separately inspected complete map; Android 37.2.31. Do not mix its Android manifest into the original set without naming the changed composition. |
| Apple manifests and four actual reference packs | 27.2.11039, package source `34b3b25d`. iOS/tvOS/macOS/Mac Catalyst base workloads do not imply MAUI supports all four as app backends. |
| MAUI manifest and libraries | 11.0.0, package source `4e6582f7`; the manifest retains older-target packs separately. Binding/asset TFMs are not application deployment floors. |
| WindowsDesktop references/analyzers | Actual 10.0.14, RC2 11.0.0-rc.2.26504.105 and GA 11.0.0 ZIPs. RC2→GA: **zero added/removed public/protected declaration lines**. Analyzer strings are presence evidence, not compiler execution. |
| Independent MAUI AI | The recipe's exact public `Microsoft.Maui.Essentials.AI/11.0.0` URL returns **404**. Actual `11.0.0-preview.8.26555.2`, source `1e710638`, retains net10-labelled assets and declared dependency groups. A host GA version does not create a stable AI package or prove net11 native support. |

The [GA mobile note](https://github.com/dotnet/core/blob/33918e8affbb517bf654c15559ea6f813c08bbae/release-notes/11.0/11.0.0/dotnetmaui.md)
still calls itself a draft awaiting release-owner confirmation. Resolve its
composition through the actual manifests above. The Android release's example
workload-list output still names RC2; it does not override the actual 37.2.31
manifest. The [Apple release](https://github.com/dotnet/macios/releases/tag/dotnet-11.0.1xx-xcode27.2.11039)
requires Xcode 27.2 but says macOS **16.6+**. Apple's own
[system requirements](https://developer.apple.com/xcode/system-requirements)
say macOS **Tahoe 26.6+**. Preserve this source conflict explicitly; use Apple's
host requirement rather than copying the apparent typo.

## Catalogue dispositions

These groups account for every prepared edition; the generated register supplies
the full ID/edition list and exact inputs. Follow-ons below own delivery, not this audit.

| Family | Decision and required correction |
| --- | --- |
| Seven `winforms.*`/`wpf.*` editions | Retain existing clipboard, rendering, startup/VB, batching, fullscreen and Designer goals. Actual RC2→GA declarations are unchanged; no invented desktop API migration follows. Preserve each saved execution/owner limit. The separate Designer extension SDK remains independently versioned, not a framework GA dependency. |
| Eight `android.*` editions | Retain target/runtime, JNI loading, HTTP, R8, predictive-back, target-SDK and legacy-asset goals. Add current source-call obsoletion and CoreCLR property rejection to existing migration slices (B below). Automatic R8 cache/field-retention and assembly-store repairs remain excluded as standalone goals. |
| Seven `apple.*` editions | Retain scene lifecycle, deployment/RID/runtime, proxy, plist/framework, failable-initializer and absent-result guidance. Reconcile 27.2 source/ref/host prerequisites and MT7199 merged-scene-manifest validation (A). New OS binding surface alone does not justify an exhaustive native API catalogue. Registrar/build/signing invalidation repairs remain automatic exclusions. |
| XAML expressions | PIN-1317 corrects `maui.author-view-local-csharp-in-xaml` to stable GA expressions without EnablePreviewFeatures/MAUIX2012. SourceGen is still required; GA source/artifact review is distinct from excluded native compilation. This does not graduate Incremental Hot Reload. |
| Other MAUI and two `mobile.*` editions | Retain existing navigation, handlers, bindings, resources, assets, maps, permissions, activations, capture, templates, backend contracts, diagnostics and in-app tests. Source checks retain the previously reviewed converter/nullability/backend/Color decisions. A owns stale workload/host prerequisites and affected source evidence. No browser check substitutes for native execution. |
| HybridWebView, Hot Reload, Avalonia template output and AI | Keep independent experimental/preview qualifications until contract-specific evidence establishes a change. C only changes XAML expression consent. PIN-1318 pins independent AI preview.8.26555.2 and qualifies net11 native compatibility as unverified; its Apple assets are net10.0-ios26.0, with a bundled native bridge. Unavailable devices/models remain explicit not-tested exclusions. |

Moving declarations from Unshipped to Shipped is bookkeeping, not a new feature.
Deleting stale `*REMOVED*` analyzer records is not an additional callable removal.
The actual new authoring residuals include ShareTextRequest.PreviewImage and
DataPackageOperation.Move (E/F). Android Shell's OnResume/inactive-page construction
change is framework behavior; document an affected lifecycle assumption only if
an existing authored example depends on eager construction. No blanket new Shell recipe.

## Prepared narrow follow-ons

Each is one PR boundary in milestone 07, executor Agent, related to PIN-966 and
a prerequisite of PIN-967. Estimates use the observed S=2/M=3/L=5 scale and
include implementation, focused validation and evidence. Cycle selection is
pending Pingu's explicit choice; these are drafts, not created Linear issues.

1. **A — Reconcile mobile GA prerequisites and affected evidence — L (5).**
   Correct stale RC2 Apple packs/Xcode27.0 assumptions across the bound catalogue;
   choose and name a complete actual workload set, JDK/Android tools and Apple's
   host pairing. Reconcile platform-specific availability and MT7199 scene checks.
   Refresh only affected source reviews and feasible local consumer evidence;
   retain exact sample/input identities and sample-specific unavailable-native
   exclusions. Keep prior RC receipts historical. Broad surface, bounded claims.
2. **B — Migrate rejected Android AOT and JNI source calls — M (3).**
   Extend `android.migrate-runtime-and-platform-targets` (PIN-923) and
   `android.migrate-java-peer-binding` (PIN-924). Show CoreCLR XA1044 for explicit
   RunAOTCompilation=false and the intentional PublishReadyToRun=false choice;
   distinguish Mono. Demonstrate CS0619 for JNINativeWrapper.CreateDelegate and
   current generator-produced callbacks; existing compiled bindings retain the
   public implementation. Use actual package/source negative and corrected controls
   where feasible; excluded Android execution stays not-tested.
3. **C — Reconcile GA XAML expression consent — M (3).**
   Update the existing PIN-900 recipe, applicability, sample configuration and
   discovery status using actual GA generator assets. Remove the obsolete preview
   requirement/MAUIX2012 claim, preserve SourceGen and async/ambiguity limitations;
   leave Incremental Hot Reload's separate preview boundary intact. Refresh affected
   feasible build/source evidence and installed list/get output.
4. **D — Repair independent MAUI AI acquisition — M (3).**
   Reconcile PIN-922's recipe against the actual available independent package.
   Inspect assembly experimental metadata, target assets, dependency resolution,
   deployment/native bridge and supported features before claiming a route. Replace
   nonexistent 11.0.0 acquisition; preserve MAUIAI0001 and unavailable model/device
   exclusions. If no supported route exists, qualify/withdraw that route with an
   honest alternative; never relabel a placeholder reference as execution.
5. **E — Share text with an image preview — M (3).**
   Add an original previous/current sample and discoverable goal using
   ShareTextRequest.PreviewImage/ShareFile. Explain Android API29+, iOS/Mac Catalyst,
   ignored platforms, unset behavior, file lifetime and sharing cancellation/error
   handling. Verify actual API/source and feasible local consumer path; mark
   unavailable native share-sheet behavior not-tested. This is distinct from saving
   captured media, map markers and app icons.
6. **F — Request drag moves without losing application data — M (3).**
   Add an original copy/move decision using DragEventArgs.AcceptedOperation and
   DataPackageOperation.Move. Explain native source/session permission, copy
   precedence, unchanged Android implementation and app-owned data mutation only
   after a successful drop. No automatic view move/CollectionView reordering claim.
   Check actual API/source, data-update controls and feasible consumer evidence;
   exclude unavailable native gestures explicitly.

No existing sharing/drag delivery issue was found in scoped searches. Existing
PIN-923/924/900/922 refine B/C/D; PIN-699 fixes the testing boundary and PIN-967
retains final readiness. Existing issues are not renamed, reopened or marked complete
by these findings. No release, credentials, cloud maps, new environment or CI matrix
is authorized. AI and native behavior are not verified by this audit.

## Reproduce and interpret

PowerShell 7, HTTPS; verified commands must start in this task's actual worktree:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-6976e50c-f24bb12c6cdac2ff'
& ./eng/Inspect-GaNativeContracts.ps1 `
    -CompareWith ./docs/planning/audits/ga-native-contracts.sources.json
```

Expected: actual package identities/hashes, all 18-entry maps, complete 142-file
MAUI API path set, RC2/GA source comparisons, desktop metadata comparison and 51
bound editions. The specifically recorded AI11.0.0 404 is expected; an unexpected
availability change, failed download, source/baseline hash or catalogue mismatch
fails. Output is disposable under artifacts. This is metadata/source proof only.
Unchanged evidence can be reused only while its actual relevant inputs remain valid.
Global stable readiness requires the delivered corrections and PIN-967's final checks.
