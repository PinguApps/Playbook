# Inspect the guidance an agent receives

## Android runtime, manifest, Java peers and JNI loading

PIN-923/PIN-924 add original CoastalProbe comparisons. The .NET10 Release build
compiles the owned peer library and app using SDK10.0.402, Android36.1.118,
JDK21.0.8 and build-tools36.0.0. Current RC3 native compilation fails NETSDK1147;
RC2 source/manifest/reference artifacts are reviewed separately. Device behavior,
current typemap generation and the optional JNI binary/load ordering are explicitly
not-tested under PIN-699. No GA verification is claimed.

Run from any directory; this selects the task worktree, builds/packs the ordinary
tool, installs it into an isolated feed/cache and retrieves actual stdio discovery,
full Markdown, edition and applicability with unusable proxies for offline calls:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-61fb9621'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('android.migrate-runtime-and-platform-targets','android.declare-target-sdk-intentionally','android.migrate-java-peer-binding','android.control-native-library-preloading') `
  -Query 'coastal-probe' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -RecipeEdition '11.0' -PackageVersion '0.0.0-pin923-924' -BuildAndPack `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' -OutputDirectory './artifacts/android/inspection'
```

Expect four edition11.0 recipes, complete canonical snippets and explicit native
limits. Target10 has no applicable edition; its samples are comparisons. This is
proxy-independent offline delivery, not OS network isolation or native execution.
`eng/Test-CoastalProbe.ps1` rechecks exact sources/artifacts and actual JavaFinalize
obsolete metadata without installing workloads or executing downloaded assemblies.

## Linux TLS binding and Windows-authentication fallback

PIN-1039/PIN-1103 add original Gannet comparisons using actual .NET10.0.14 and
.NET11RC3. The Linux MIT Kerberos route validates mismatches but accepts clients
that omit bindings; this limitation appears in discovery, applicability and the
complete offline body. It must not be used for mandatory client-binding policies.
The Windows loopback Kestrel fixture uses the existing Windows identity, with no
account provisioning, and records the actual HTTP/2 challenge/HTTP/1.1 retry.

This block was checked from C:\, outside the task worktree. It builds/packs the
ordinary tool, installs that exact artifact through an isolated feed/cache, and
prints real stdio discovery ID/title/goal, full Markdown, edition and applicability:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-3a4dc15e'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('networking.bind-linux-negotiate-to-tls','http.remove-manual-windows-auth-downgrade') `
  -Query 'gannet-authentication' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -RecipeEdition '11.0' -PackageVersion '0.0.0-pin1039-1103' -BuildAndPack `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory './artifacts/authentication/inspection'
```

Expect edition11.0 for both, complete canonical snippets and fresh evidence.
Target10 returns no applicable edition; its consumers are comparisons, preserving
the existing eight foundations. Offline retrieval uses unusable proxies, not OS
network isolation. No public package publication or final-GA proof is performed.
eng/Test-GannetGateway.ps1 -Record -VerifySources and
eng/Test-GannetBinding.ps1 -Record capture focused real behavior. The binding
runner's optional -RequireMissingBindingRejection is intentionally failing on the
measured RC3/MIT route and remains an explicit stronger qualification gate.

## Native passkeys and experimental Apple summaries

PIN-916/PIN-922 add Heron guidance. The Windows passkey adapter compiles against
the exact RC2 asset. Its actual native-host build fails NETSDK1147 (maui-tizen);
both Apple samples fail NETSDK1147 (ios). Native ceremonies and model generation
are explicitly not-tested under PIN-699. PIN-936 remains open and has not run;
PIN-937 is canceled. No device approval, server interoperability or GA execution
is claimed. No new hardware, workload installation or service is required.

This block is checked from an unrelated directory. It builds/packs, installs into
an isolated feed/cache, and calls the actual installed server over stdio. It
displays ID/title/goal, complete rendered Markdown, edition and applicability.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-9a8dc444'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('maui.authenticate-with-platform-passkeys','maui.use-apple-intelligence-abstractions') `
  -Query 'heron-guidance' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -RecipeEdition '11.0' -PackageVersion '0.0.0-pin916-922' -BuildAndPack `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' -OutputDirectory './artifacts/heron/inspection'
```

Expect both edition 11.0 recipes, the server verification boundary and experimental
MAUIAI0001 prerequisites. Target 10 has no applicable edition: its Apple sample
is comparison-only. Unusable proxies test proxy-independent offline retrieval,
not OS network isolation or native execution. Nothing is published.

## Windows activation, permissions and themed assets

PIN-919/PIN-920/PIN-921 add three original Tern comparisons. The .NET10
Windows activation app and both Android apps compile. Separate Windows processes
verify launch redirection and synthetic protocol callback delivery; this does not
verify OAuth or WebAuthenticator sign-in. Temporary protocol registration is
removed. SDK11 RC3 native builds fail NETSDK1147, and Android devices/Apple Xcode
are unavailable: those sample-specific native actions are explicitly not-tested
under PIN-699. Source/artifact review is separate from native execution and GA.

From an unrelated directory, this block builds and installs the actual package
using an isolated feed/cache, then displays ID/title/goal, complete Markdown,
edition and applicability through real stdio list/get:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-b4d842e5'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('maui.route-single-instance-activations','maui.inject-permission-checks','maui.author-themed-app-assets') `
  -Query 'tern-guidance' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -RecipeEdition '11.0' -PackageVersion '0.0.0-pin919-921' -BuildAndPack `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory './artifacts/tern/inspection'
```

Expect edition11.0 for all three recipes, complete canonical source and explicit
native limitations. Target10 returns no applicable edition: its samples are
comparisons, not extra foundation editions or supported older-target routes.
The harness uses unusable proxies for offline retrieval; this is not OS network
isolation or native behavior proof. No package publication is performed.

## Dense native maps and the supported Windows surface

PIN-914/PIN-915 add three Quay recipes. Exact MAUI10.0.110 artifact metadata
confirms that Windows maps already exist while Region/clustering/custom-pin and
new overlay APIs do not. Both releases compile the linked canonical registration
against actual Windows assets; the native .NET10 map/pin app also compiles with
SDK10.0.402. It was not launched against the service. SDK11 RC3 Windows MAUI
fails NETSDK1147 and Mac/Xcode is unavailable: only those unavailable native
sample actions use PIN-699 exclusions. No map/service execution is claimed.
PIN-935 remains open and has not run; no credentials/resources were acquired.
The RC2 Windows camera-dispatch mismatch is disclosed; it does not apply to10.0.110.

This exact block is tested from an unrelated directory. It builds/installs the
actual package through an isolated local feed/cache and displays ID/title/goal,
full Markdown, edition and applicability through real stdio list/get.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-59d42d49'
$inspection = @{
  Query = 'quay-maps'
  RecipeEdition = '11.0'
  PackageVersion = '0.0.0-pin914-915'
  Dotnet = 'C:/Users/pingu/AppData/Local/dotnetup/dotnet/dotnet.exe'
  Node = 'C:/Program Files/nodejs/node.exe'
}
& ./eng/Inspect-BlazorMigrationGuidance.ps1 @inspection -BuildAndPack `
  -RecipeId @('maui.compose-maps-declaratively','maui.interact-with-dense-map-markers') `
  -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -OutputDirectory './artifacts/maps/inspection/11-dense'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 @inspection `
  -RecipeId @('maui.enable-windows-maps') -TargetVersion '11.0' `
  -OutputDirectory './artifacts/maps/inspection/11-windows'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 @inspection `
  -RecipeId @('maui.enable-windows-maps') -TargetVersion '10.0' `
  -OutputDirectory './artifacts/maps/inspection/10-windows'
```

Expect edition11 for all three recipes on target11. Target10 has no applicable
dense-map edition; Windows maps resolve edition11 through an explicit conditional
10.0.110 compatibility route backed by native compilation and source/artifact
review. No extra .NET10 foundation edition is added. Unusable network proxies
establish proxy-independent offline retrieval, not OS network isolation or native
map execution. No package is published.

## Cosmos conversion composition and model-code selection

PIN-1218/PIN-1219 add original Bittern comparisons. Cosmos checks actual server
math/concatenation, explicit older client evaluation, null, unsupported string
conversion and range semantics. SQLite checks the selector extension through real
scaffolding, compilation of generated C# and queries on engines3.53.3/3.53.4.

From any directory, select this checkout and inspect the exact installed package:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-dbdc9acd'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('cosmos.translate-converted-expressions','ef.migrate-model-code-selector') `
  -Query 'bittern-guidance' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin1218-1219' -BuildAndPack `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory './artifacts/bittern/inspection'
```

Expect ID/title/goal, complete canonical Markdown, edition11 applicability and
fresh evidence through real stdio list/get. Target10 reports no applicable edition.
The harness uses an isolated local feed/cache, an unrelated directory and unusable
network proxies. It establishes offline retrieval, not OS network isolation or GA.
No package is published. Rerun the two Test-Bittern scripts for real provider proof.

## Current SDK baseline and PMC proof

Active sample SDKs are 10.0.403 and 11.0.100-rc.3.26522.148. Package
versions remain independently pinned; CanalDesign uses EF11 RC2. Pingu verified
actual Visual Studio PMC migration creation, stale-build behavior and SQLite
application on SDK11 RC3. Windows contrast-theme text remained readable; its
red decorative accent is intentionally absent in high contrast.

Build and inspect the real installed tool from an unrelated directory:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('ef.reuse-current-build-in-pmc') `
  -Query 'canal-design' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin957-rc3' -BuildAndPack `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory './artifacts/pin957-inspection'
```

Expect discovery ID/title/goal, complete Markdown and applicability for edition11,
with the verified mixed SDK/EF baseline and the need to rebuild after source
changes. Target10 returns `no_applicable_edition`. The harness installs a local
package in a disposable feed/cache and calls the real stdio server with unusable
proxies. This verifies embedded offline retrieval; it does not repeat Visual
Studio PMC or establish OS-level network isolation. No package is published.

## Android Material customization and generated HybridWebView calls

PIN-909/PIN-910 add the MarshEntry and MarshBridge comparisons. MarshEntry's
.NET 10 Android app builds a signed APK. MarshDispatch exercises the actual
neutral MAUI assemblies and packaged interceptor in a Windows x64 NativeAOT
executable, including request and return JSON. This does not establish Android
interaction, iOS compilation or iOS WebView/NativeAOT behavior; those checks are
explicitly not-tested under PIN-699. The SDK 11 native Android workload is absent.
The historical RC2 ILCompiler emits NU1903; the pinned check is preparation
evidence, not a production toolchain recommendation.

From an unrelated directory, build and inspect the actual installed package:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('maui.customize-material-handlers','maui.call-dotnet-from-hybrid-js-with-aot') `
  -Query 'marsh-survey' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin909-910' -BuildAndPack `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory './artifacts/marsh/inspection'
```

Expect ID/title/goal, complete expanded Markdown and applicability for both
edition 11 recipes, with experimental Apple publishing and exact evidence limits.
Target 10 returns `no_applicable_edition`. The existing harness installs from an
isolated feed/cache outside the checkout and retrieves with unusable proxies;
this proves offline content delivery, not OS network isolation. Nothing is
published. Run `eng/Test-MarshInterop.ps1` for pinned source/package contracts and
`eng/Test-MarshDispatch.ps1 -Dotnet <absolute SDK11 dotnet.exe>` for the actual
neutral NativeAOT dispatch check.

`playbook mcp` speaks protocol over stdin/stdout. Use MCP Inspector as a client; it initializes and closes each connection automatically. `eng/Inspect-Guidance.ps1` builds the real server, installs pinned Inspector 2.11.0 under ignored `artifacts/` if needed, calls both tools, and prints **ID/title/goal, full expanded Markdown and applicability**. Responses default to `artifacts/inspection/<id>.json`.

Requires PowerShell 7, the repository's .NET SDK 10.0.403 and Node 22.19+. Setup may restore/download packages online; guidance itself is embedded and offline. The helper invokes the resolved executables by absolute path without changing PATH or global tools. It disables .NET first-run PATH integration. No native workloads or account are required for server inspection.

## Routed navigation and Android Shell migration

PIN-905/PIN-906 add HarbourNavigation and HarbourAndroid comparisons. Older
Windows and Android samples compile; the controlled Windows probe verifies
forward/query delivery/back navigation. Current native compilation lacks the
SDK11 MAUI SDK pack. Current native interaction, Android device and Apple checks
remain explicitly not-tested under PIN-699. No new native matrix is introduced.

From any directory, select this exact worktree, build/pack and inspect the actual
installed tool. Requires PowerShell7, SDK10.0.403 and Node22.19+; setup restores
packages. The existing harness uses an isolated feed/cache and unrelated working
directory, and prints ID/title/goal, full expanded Markdown and applicability.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('maui.navigate-with-path-parameters','maui.migrate-navigation-handlers') `
  -Query 'harbour-navigation' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin905-906' -BuildAndPack `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory './artifacts/navigation/inspection'
```

Expect both edition11 recipes with complete canonical files, fresh saved evidence,
absolute-template constraints, badge/tint/accessibility exceptions and the Android
package/type distinction. Target10 returns no_applicable_edition. Retrieval uses
unusable proxies, proving proxy independence rather than OS network isolation or
native behavior. No package is published. For the source/artifact assertions use
`eng/Test-HarbourNavigation.ps1`; add `-Windows` after building the older Windows
sample to run its controlled native probe. `-Record` refreshes current source-review
evidence, never compilation evidence.

## Schema ownership and design-time command defaults

PIN-881/PIN-882/PIN-883 add original CanalDesign comparisons. Both locked EF10
and EF11 RC2 consumers compile and run. Real SQLite checks distinguish empty
migration assemblies from externally owned schemas. Real dotnet-ef checks cover
nested-directory defaults, config-relative projects and explicit context overrides.
Actual Visual Studio PMC NoBuild execution was performed by Pingu for PIN-957 on
5 November 2026: VS18.11.0, PMC7.11.0.24, PowerShell5.1.26100.9711, SDK11 RC3
with EF11 RC2. The linked PMC record covers generated migrations, stale/rebuilt
model behavior and actual SQLite schema/history. This is not EF RC3 or GA proof.

From any unrelated directory, in PowerShell7 with SDK10.0.403 and Node22.19+:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('ef.handle-empty-migration-assembly','ef.configure-migration-commands','ef.reuse-current-build-in-pmc') `
  -Query 'canal-design' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin881-883' -BuildAndPack `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory './artifacts/canal/inspection'
```

Expect all three IDs/titles/goals, full expanded Markdown, applicability and
edition11 with fresh compilation/runtime records and verified PMC NoBuild/stale-build proof on SDK11 RC3 with EF11 RC2.
Target10 returns no_applicable_edition. The existing harness installs the exact
local artifact in an isolated feed/cache and calls real stdio list/get from an
unrelated directory with unusable proxies. This proves proxy independence, not
OS network isolation or interactive PMC behavior. Setup can download packages;
retrieved guidance is embedded and needs no account or network service.

## Visual resources and cancellable holds

PIN-903/PIN-904 add original BeaconBoard comparisons. The .NET10 Windows sample
compiles; current Windows compilation lacks its MAUI SDK pack. Apple/Android
toolchains and devices are unavailable. Native rendering and gesture/animation
interaction remain explicitly `not-tested`; no additional hardware is requested.
The full live long-press lifecycle is documented on Apple; Android/Windows provide
completion-only notifications in the reviewed implementation.

From any directory, build/pack and inspect the actual installed tool:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('maui.reapply-mutated-style-state','maui.handle-long-press') `
  -Query 'beaconboard' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin903-904' -BuildAndPack `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory './artifacts/beacon/inspection'
```

Expect both IDs/titles/goals, complete expanded Markdown and applicability,
edition11, fresh compilation/source-review records and explicit native exclusions.
Target10 returns `no_applicable_edition`. The isolated local feed/cache and unrelated
working directory exercise real stdio list/get with unusable proxies; this proves
proxy independence, not OS network isolation or native behavior.

## Trim-safe MAUI bindings and native CLI reload

PIN-901/PIN-902 add SignalDeck comparisons and complete offline .NET11 recipes.
The reload engine is preview. The .NET10 Windows example compiled; current native
compilation, trimming and live reload remain explicit sample-specific exclusions
under PIN-699. Retrieval does not prove native behavior.

Run from any directory with PowerShell7, SDK10.0.403 and Node22.19+. The existing
harness builds and packs, installs that exact artifact with an isolated local
feed/cache, then calls real stdio list/get from an unrelated directory using
unusable proxies. This proves proxy independence, not network isolation.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('maui.author-trim-safe-relative-bindings','maui.iterate-with-native-hot-reload') `
  -Query 'signaldeck' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin901-902' -BuildAndPack `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory './artifacts/signaldeck/inspection'
```

Expect both IDs/titles/goals, full canonical Markdown, complete applicability,
edition11, preview reload status and current compilation/source-review records.
Target10 must return no_applicable_edition. The helper verifies the full body,
JSON-text equality, constraints and input freshness; it also checks saved PATH
values without changing shared settings. It installs no native workload.

## Agent state, completed-turn restoration and direct TLS

PIN-807/PIN-1170/PIN-1181 provide complete documentation-based recipes authorized
by Pingu on 3 November 2026. MuseumCaptions and ObservatoryLog compile against
their actual .NET10/11 dependency sets; the older HarbourTls socket host compiles.
The current documented DirectTlsEndpoint configuration reproduces CS0246 against
the inspected RC1 package and is source-reviewed only. No sample was executed.
Browser/AG-UI/provider, renderer/storage, Linux/OpenSSL TLS and GA checks remain
deferred; pending proof still blocks broader release-ready packaging.

From an unrelated directory, this selects the task worktree, builds and packs the
ordinary tool, then installs that exact artifact into an isolated local feed/cache
and calls real stdio list/get from another directory. Requires PowerShell7,
SDK10.0.403 and Node22.19+. Setup may use network; retrieval uses unusable proxies,
which proves proxy independence rather than network isolation.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('blazor.synchronize-agent-view-state','blazor.restore-agent-conversation','hosting.select-direct-tls') `
  -Query 'agent-state-guidance' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin807-1170-1181' -BuildAndPack `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory './artifacts/agent-guidance/inspection'
```

Expect all three IDs/titles/goals, complete canonical Markdown, applicability,
edition11, experimental labels, fresh compile/source-review records and explicit
pending behaviour proof. Target10 must return no_applicable_edition. The TLS
recipe must disclose its missing endpoint API, never a successful current build.
No new platform matrix, provisioning or public publication is involved.

## Experimental affected tests and file-access observation

PIN-1074/PIN-1123 provide documentation-based experimental guidance authorized by
Pingu on 3 November 2026. Their original samples remain source-reviewed, with
compilation and experimental behaviour pending. Local affected-test registration
documentation is unavailable; no configuration contract is invented. The decoder
has no real event-stream proof. These limitations remain visible in discovery and
detail, and release-ready packaging stays gated.

From an unrelated directory, this selects the verified task worktree, builds and
packs the ordinary tool, installs the exact artifact using an isolated local
feed/cache, and calls real stdio list/get from another directory. Requires
PowerShell 7, SDK10.0.403 and Node22.19+. Online setup is permitted; retrieval uses
unusable proxies to establish proxy independence, not OS-level network isolation.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('sdk.select-affected-tests','msbuild.inspect-file-access-records') `
  -Query 'experimental-build-guidance' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin1074-1123' -BuildAndPack `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory './artifacts/experimental-discovery/inspection'
```

Expect both IDs/titles/goals, complete expanded Markdown and canonical snippets,
target/edition11, experimental applicability and current source-review input
records with pending proof. Target10 returns no applicable edition. This validates
the served documentation, not collection/selection, native event capture,
cancellation, sample compilation or GA support. The exact eight .NET 10
foundations and PIN-959/PIN-963 gates remain unchanged.

## Experimental frontend tools and approval continuation

The paired HarbourDispatch consumers demonstrate declaration-only frontend tools,
an app-defined progress block and an approval decision enforced by the scripted
provider before changing its ledger. Components.AI remains experimental throughout
.NET 11. Run from the pinned sample directory; the older comparison uses a manual
AI.Abstractions loop because the checked Components.AI package has only net11.0.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
Push-Location -LiteralPath './samples/net11.0/HarbourDispatch'
try {
  & 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' run -c Release -- --check
} finally { Pop-Location }
```

Expect actual Components.AI declaration/single-invocation/continuation checks and
real loopback HTTP/SSE AGUI.Client approval/denial checks: denial leaves zero
dispatches, approval records one. The endpoint is scripted; remote model/provider,
multi-turn remote rejection history, production authorization and GA are unverified.
The UI runs with `dotnet run -c Release -- --urls http://127.0.0.1:6072` in that
directory. Send berth, select it, then send dispatch and deny/approve separate
requests. The .NET 10 comparison offers Request berth/Request dispatch buttons.

The following builds/packs the actual tool, installs it into an isolated
feed/cache and calls real stdio list/get from an unrelated directory. It prints
ID/title/goal, full expanded Markdown, applicability and edition 11.0. The harness
checks the entire canonical body and rejects a .NET 10 target for these editions.
Unusable proxies establish proxy independence, not OS-level network isolation.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('blazor.render-agent-tools','blazor.approve-agent-action') `
  -Query 'harbour-dispatch' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin805-806' -BuildAndPack `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory './artifacts/agent-api/inspection'
```

## MAUI SourceGen and preview page logic

QuayBoard compares .NET 10 XamlC/code-behind with SourceGen and preview page logic.
The older Windows page compiled with SDK 10.0.403 and Controls 10.0.110.
The current page is source-reviewed only: SDK 11 RC3 lacks Microsoft.Maui.Sdk
locally and fails NETSDK1147. PIN-699 excludes its native compilation/execution.
The actual Controls 11.0.0-rc.2.26505.5 Windows dependency group targets 20348.
No native execution, performance, Android/Apple or GA proof is claimed.

This block is exercised from unrelated C:\. Requires PowerShell 7, the existing
SDK 10 and Node 22.19+. It builds/packs, installs the exact artifact through an
isolated local feed/cache and calls real stdio from an unrelated directory.
Setup can use network; retrieval uses unusable proxies, not OS network isolation.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Test-Smoke.ps1 -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' -PackageVersion '0.0.0-pin899-900'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 -RecipeId @('maui.choose-xaml-generation','maui.author-view-local-csharp-in-xaml') -Query 'quay-board' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' -PackageVersion '0.0.0-pin899-900' -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' -Node 'C:\Program Files\nodejs\node.exe' -OutputDirectory './artifacts/quay-inspection'
```

Expect the installed smoke, both discovery IDs/titles/goals, complete canonical
Markdown, applicability and fresh paired evidence to pass. Edition 11/target 11,
preview labels and source-only/native exclusion warnings remain visible.
Structured/text results agree; target 10 returns no_applicable_edition.
The older page is a comparison, not an additional .NET 10 foundation.
## Unix library filenames and explicit Mono launch

The local authoring runner uses disposable Linux x64 Docker containers, Mono
reference assemblies, Clang/linker/zlib and SDK10.0.403/11 RC2. It checks shared
library default/opt-out names, symbols, C calls, packaging failures, legacy launch
defaults and explicit quoted paths/arguments, working directory and exit7.
It removes its container and preserves PATH. macOS execution is not-tested;
static libraries remain unsupported/unresolved. Mono launch remains unsupported
by Microsoft. Network is needed for isolated setup; no public publication occurs.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Test-TideAndMono.ps1 -Record
```

Build/pack the ordinary tool, install that exact artifact in an isolated local
feed/cache and retrieve both recipes through real installed stdio from an
unrelated directory. Expect IDs/titles/goals, complete canonical Markdown,
target/edition11, applicability and current evidence; target10 has no edition.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('deployment.consume-native-library-artifacts','sdk.configure-legacy-mono-launch') `
  -Query 'tide-mono' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin1065-1067' -BuildAndPack `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory './artifacts/tide-mono/inspection'
```

The SDK pins and RC evidence do not establish actual GA. The eight historical
foundations, PIN-959 comprehensive gate and PIN-963 reconciliation stay separate.

## Temporal bounds and server time reports

Run the paired original consumers against the authorized isolated SQL2025 container.
Requires existing SDK10.0.403 and SDK11 RC3, Docker Desktop and restored locked packages.
Each run creates/removes its own restricted database and stops the container.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Test-BeaconHistory.ps1 -Record
```

Expect generation/history/update-rejection checks and actual server time query checks
for both releases. RC2 is not GA; older engines and named-zone DST rules are unverified.
The following builds/packs the ordinary tool and retrieves both recipes through the
existing isolated-feed/cache installed stdio harness from an unrelated directory.
It prints ID/title/goal, full canonical Markdown, applicability and edition11.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('sql.expose-temporal-boundaries','sql.query-offset-and-date-parts') `
  -Query 'beacon-history' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin859-860' -BuildAndPack `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory './artifacts/beacon-history/inspection'
```

Expect two complete entries and exact-artifact PASS; target10 has no applicable edition.
The historical projects add no .NET10 foundations. PIN-959/PIN-965 remain separate gates.

## SQL JSON presence and membership

The original paired consumers need the existing SDK10.0.403 and SDK11 RC3,
their locked EF/SqlClient packages, and the approved local Docker SQL2025 route.
The runner creates and removes separate restricted databases; it stops the server
afterward. It changes no PATH or persistent environment setting.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Test-JsonDocuments.ps1 -Record
```

Expect both actual consumers to pass missing/null/empty path, numeric and LIKE
containment, primitive membership and nullable OPENJSON fallback checks. EF11's
JsonContains is experimental EF9106; the current sample explicitly opts in.
Native json columns are required by this measured containment route. SQL2022
path-existence support is source-backed, not actual2022 engine execution.

This block builds/packs the ordinary tool and installs the exact artifact using
an isolated feed/cache and unrelated working directory. Real installed stdio
list/get prints ID/title/goal, full canonical Markdown, target/edition11,
applicability and fresh sample evidence. Unusable proxies establish proxy
independence, not OS network isolation.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('sql.test-json-path','sql.query-json-membership') `
  -Query 'json-documents' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin853-854' -BuildAndPack `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory './artifacts/json-documents/inspection'
```

Expect two complete entries and exact-artifact PASS; target10 returns
no_applicable_edition. Older samples are comparisons, not extra foundations.
Actual EF11 GA, other client platforms and older SQL engines remain unverified.

## Native export retention, Unix mutex scope and x86 heap limits

Requires the existing SDK10.0.403/runtime10.0.14 and separate SDK11 RC3,
PowerShell7, MSVC/linker and Windows SDK libraries, Windows x86 .NET10 runtime,
Docker Linux and Node22.19+. The Windows runner downloads the exact portable
RC2 x86 runtime under ignored artifacts when needed; the Unix runner downloads
RC2/10.0.12 Linux runtimes and uses the existing SDK10.0.403 image. No host
installation, PATH integration, production resource or recurring matrix is added.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Test-NativeBoundaries.ps1 -Record
```

Expect retained/trimmed AOT export tables and independent C calls, real same-session
Unix UID/scope/storage controls (including historical10.0.12), and actual x86
heap-limit/OOM controls. The older runtime ignores HeapHardLimit; the current
runtime enforces32MiB. RSS is separately measured. Source discrepancies and
unverified other platforms/GA remain explicit in the complete offline recipes.

The following block is verified from an unrelated starting directory. It builds
and packs the ordinary tool, installs that exact artifact through an isolated
local feed/cache, then prints discovery ID/title/goal, complete canonical Markdown,
target11/edition11, applicability and fresh evidence via real installed stdio.
Unusable retrieval proxies demonstrate proxy independence, not OS network isolation.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('interop.retain-native-exports-with-types','threading.choose-unix-mutex-scope','runtime.bound-a-32-bit-gc-heap') `
  -Query 'native-boundaries' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin1098-1109-1110' -BuildAndPack `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory './artifacts/native-boundaries/inspection'
```

Expect three complete entries and the exact-artifact PASS. Target10 returns
no_applicable_edition; older samples are comparisons, not additional foundations.
PIN-959/PIN-962 and existing Human/actual-GA gates remain open.

## Native C layouts and browser float buffers

Requires PowerShell7, the existing SDK10.0.403/runtime10.0.13 and separate
SDK11.0.100-rc.2.26504.105/runtime11RC2, MSVC19.51.36260.0/link14.51.36260.0,
Node24.21.0, Playwright and Chromium. The runner parameters select absolute
tool paths without changing PATH. Matching Mono browser runtime/SDK assets are
restored; interpreted publish needs no installed wasm-tools workload, AOT or
native relink. The C helper needs no Windows SDK headers or CRT libraries.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Test-InteropSoundings.ps1 -Record -VerifySources
```

Expect both native consumers to pass sizeof/alignment/offset comparisons,
struct/union round trips and rejected packed layout; both real published browser
consumers pass arrays, generated exports, span/segment offsets, NaN/infinities,
span expiry and retained-segment disposal. Saved compilation and browser execution
receipts identify exact dependencies, compiler/reference/generator/native/wasm
artifacts and input hashes. Other ABIs/browsers, CoreCLR-browser, AOT and GA remain
unverified; the .NET10 foundations and PIN-959/PIN-962 gates are unchanged.

This block is exercised from an unrelated starting directory. It builds/packs the
ordinary tool, installs that exact artifact through a local-only isolated feed/cache
and calls real installed stdio list/get from a temporary unrelated directory.
Retrieval uses unusable proxies; that proves proxy independence, not OS network isolation.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('interop.match-native-c-layout','interop.exchange-float-buffers-with-javascript') `
  -Query 'interop-soundings' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin1095-1097' -BuildAndPack `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory './artifacts/interop-soundings/inspection'
```

Expect ID/title/goal for both recipes, complete canonical Markdown, target11/edition11,
full applicability and fresh compilation/execution evidence. Target10 returns
no_applicable_edition; the older snippets are comparisons inside edition11, not new
.NET10 foundations. Missing/stale evidence or canonical/applicability drift fails.

## SQL compatibility and whole-record extrema

PIN-843/PIN-851 use the existing authorized local SQL route. Refresh the paired
real consumers with `eng/Test-SqlQueryGuidance.ps1 -Record`. It creates/removes
unique restricted databases and stops the server. Both declared SDKs and pinned
packages must be installed/restorable; no external account is used. SQL2025 at150
and170 is not actual SQL2019/older-engine proof. RC2 rejects composite MinBy keys;
the working query uses a MIN/MAX candidate subquery and scalar unique-Id selection.

The following block was exercised from an unrelated starting directory. It builds
and packs the ordinary tool, installs that exact artifact through an isolated
local-only feed/cache, and calls real stdio list/get from a temporary unrelated
directory. Setup may restore online; unusable retrieval proxies establish proxy
independence, not operating-system network isolation. The existing packed Reqnroll
SqlQueryGuidance scenario additionally runs with Docker `--network none`.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('ef.select-record-at-extreme','sql.set-supported-compatibility') `
  -Query 'sql-query-guidance' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin843-851' -BuildAndPack `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory './artifacts/sql-query-guidance/inspection'
```

Expect two ID/title/goal discovery entries, full canonical Markdown, complete
applicability and current paired evidence at edition/target11; target10 must
return no_applicable_edition. Missing/stale proof, snippets or applicability drift
fail. No extra .NET10 foundation or actual-GA/comprehensive readiness is claimed.

## Custom validation metadata and trace randomness

Requires PowerShell7, Node22.19+, SDK10.0.403/runtime10.0.13 and the separate
SDK11.0.100-rc.2.26504.105/runtime11RC2 installation. ReefMaintenance pins actual
Validation/DI packages; ArchiveStages uses framework APIs. No web host, workload,
device, collector, account or persistent environment change is required.

Refresh the four original consumers and verify immutable official source hashes:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Test-ExtensionMetadata.ps1 -Record -VerifySources
```

Expect valid/invalid/nested metadata checks, exact current structured errors,
services/completion/failure/cancellation, generated/localized metadata and expected
old-base/net10/ASP0033/ASP0035/ASP0038 compile failures with successful canonical
controls. Trace checks cover all four flag combinations, parse/format, both
recording decisions, inherited context/string/direct parents, default/custom roots
and cleanup. Package/generator/reference/compiler/runtime identities match saved
artifacts; PATH and persistent settings must remain unchanged.

This block is tested from an unrelated starting directory. It builds/packs the
ordinary tool, installs that exact artifact through an isolated local-only
feed/cache, and calls real installed stdio list/get from an unrelated temporary
directory. It prints discovery ID/title/goal, complete canonical Markdown,
target11/edition11, applicability and fresh paired evidence:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('validation.migrate-custom-metadata','tracing.preserve-randomness-metadata') `
  -Query 'extension-metadata' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin1115-1117' -BuildAndPack `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory './artifacts/extension-metadata/inspection'
```

Expect two complete entries and final exact-artifact PASS. Target10 returns
no_applicable_edition. Missing/stale evidence, canonical snippet/body drift or
applicability changes fail. Unusable proxies during retrieval prove proxy
independence, not OS-level network isolation. No endpoint/collector, random-quality,
AOT/other-platform or final-GA proof is claimed. Exact eight foundations,
existing Human gates and PIN-959/PIN-962 remain unchanged/open.

## Dual-stack connections and endpoint buffers

Requires PowerShell7, Node22.19+, SDK10.0.403/runtime10.0.13 and the separate
SDK11.0.100-rc.2.26504.105/runtime11RC2 installation. The samples have no package
dependencies or workloads. RelayDial requires OS IPv4/IPv6, localhost resolving
only to both loopback families and available ephemeral TCP ports; it owns all
listeners/attempts. EndpointLedger performs no network I/O. No system DNS/routes,
global tools, PATH or persistent environment changes are required.

Refresh the four sample records and verify nine immutable official sources:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Test-NetworkBuffers.ps1 -Record -VerifySources
```

Expect Default/Parallel to pass both/either/neither-family listener cases,
selected-peer payload, event completion and cancellation/draining. The older
scheduler checks its own success/fallback, payload and drained-cancellation paths.
A saturated owned accept queue holds current TCP connections pending for real
cancellation. All sockets are disposed. Endpoint checks cover numeric IPv4/IPv6,
ports/defaults, malformed/UTF8 failures, exact/large/short output and unchanged
buffer boundaries. Current char/generic interfaces execute too. PATH/persistent
environment snapshots must agree; saved records include actual loaded runtime and
audit-matching references/compiler/native identities. No fixed latency, failed-write
interior rollback, other-platform, allocation benchmark or GA claim is made.

This block is tested from an unrelated starting directory. It builds/packs the
ordinary tool, installs that exact artifact using the existing isolated local-only
feed/cache, then calls actual installed stdio list/get from an unrelated temporary
directory. It prints both discovery IDs/titles/goals, full canonical Markdown,
explicit target11/edition11, applicability and fresh paired evidence. Setup can
restore online; unusable proxies during retrieval prove proxy independence, not
OS-level network isolation.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('sockets.race-dual-stack-connections','networking.parse-endpoint-buffers') `
  -Query 'network-buffers' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin1090-1091' -BuildAndPack `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory './artifacts/network-buffers/inspection'
```

Expect two complete entries and final exact-artifact PASS. Target10 returns
no_applicable_edition. Missing/stale evidence, snippet/body drift or applicability
changes fail. Existing eight foundations, Human gates, PIN-959 and PIN-962 remain
unchanged/open. RC2 preparation is not final-GA verification.

## Registered certificate identifiers and typed DNS records

Requires PowerShell 7, Node 22.19+, SDK10.0.403/runtime10.0.13 and the separate
SDK11.0.100-rc.2.26504.105/runtime11RC2 installation. The older DNS consumer pins
DnsClient1.8.0 and executes its net8.0 asset; other consumers use framework APIs.
Certificate proof needs the available Windows ECDSA P-256 provider. DNS proof
needs free loopback UDP53 on127.0.0.1/127.0.0.2 and the existing Linux Docker
runtime images. No account, workload, public zone or system DNS edit is required.

Refresh affected sample/source evidence separately:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Test-IdentityRecords.ps1 -Record -VerifySources
```

Expect four passing actual consumers and nine immutable source hashes. Certificate
checks export/reload SAN bytes, primitive tags, multiple/large OIDs, unrelated DNS,
manual/new equality, malformed/null inputs, failed append and explicit criticality.
DNS checks execute seven record kinds, fields/TTLs, NODATA/NXDOMAIN, SOA120/minimum45
and absent SOA, two-server override and pre/in-flight cancellation. Windows RC2
returns negative TTL0 and rejects custom ports/mixed families; separate network-none
Linux runs return45 and use custom ports. Read-only mounts/private tmpfs disappear
with the containers. PATH and persistent settings are checked unchanged.

This block is exercised from an unrelated starting directory. It builds/packs
the ordinary tool, installs that exact artifact through an isolated local-only
feed/cache, then calls real installed stdio list/get from a temporary unrelated
directory. Setup may restore online; unusable proxies during retrieval prove
proxy independence, not OS network isolation.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Test-Smoke.ps1 -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -PackageVersion '0.0.0-pin1088-1089'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('certificates.encode-registered-identifiers','dns.inspect-typed-records') `
  -Query 'identity-records' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin1088-1089' `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory './artifacts/identity-records/inspection'
& ./eng/Test-IdentityRecordSnippets.ps1
```

Expect one passing installed smoke, both discovery IDs/titles/explanatory goals,
full canonical Markdown, target11/edition11, applicability and fresh paired
compilation plus separate Linux execution evidence. Structured/text results agree;
target10 returns no_applicable_edition. Missing/stale evidence, a changed snippet/body
or a nonzero exit fails. Inspection consumes saved proof; it does not rerun DNS or
certificate checks. No trust, network identity validation, public resolver,
other-platform/native matrix or GA verification is claimed. The string signature
corrects the audit draft's Oid spelling. SRV selection stays in the existing recipe;
eight foundations, PIN-959/PIN-962 and existing Human gates remain unchanged/open.

The final snippet check copies only the installed Markdown code blocks into four
fresh pinned console consumers, one source file per block, and runs certificate
export/read-back or a real local DNS query. It supplies no missing imports to the
copied snippets; all required imports and namespaces are inside their canonical
regions. The temporary projects/sockets are removed. Source evidence separately
prints the loaded consumer CoreLib path/version/hash and rejects runtime baseline
drift before saving; verification pins are not additional application requirements.

## Value-based JSON classification and message authenticators

This builds/packs the ordinary tool, installs that exact artifact using the
existing isolated local-only feed/cache and calls real stdio list/get from an
unrelated temporary directory. It displays ID/title/goal, complete expanded
canonical Markdown, target11/edition11, applicability and fresh paired evidence.
Tested from an unrelated starting directory:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Test-ClassifierAuthentication.ps1
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('json.classify-payload-values','crypto.verify-message-authenticators') `
  -Query 'classifier-authentication' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin1086-1087' -BuildAndPack `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory './artifacts/classifier-auth/inspection'
```

Requires PowerShell7, Node22.19+, SDK10.0.403/ref-runtime10.0.13 and complete
SDK11.0.100-rc.2.26504.105/ref-runtime11RC2. No extra sample packages or workloads.
Positive KMAC checks require a supported provider (measured Windows x64 CNG);
the sample records native DLL versions/hashes. No account or external service is
used. Setup can restore tool/Inspector2.11.0 online; unusable proxies during
installed retrieval prove proxy independence, not OS network isolation.

Expect four successful builds/runs, complete selected JSON payloads in either
property order, ten rejected JSON cases, reference null, registration precedence,
global no-match/unregistered failure contracts and generated metadata without
reflection fallback. Authenticators verify independent known answers, failures,
actual cancellation after12 bytes, incremental preserve/reset and KMAC
customization/support. Raw HMAC invalid lengths throw; raw KMAC has variable
output length, while the protocol enforces32 bytes. Final exact-artifact inspection
reports PASS; target10 returns no_applicable_edition. `-Record -VerifySources`
refreshes affected proof and verifies immutable source hashes. Stale inputs,
canonical-content/target/applicability drift or nonzero exit fail. Union custom
null, AOT, timing, other-platform/provider and final-GA claims remain unverified;
eight foundations and PIN-959/PIN-962/Human gates stay open. No new matrix.

## Text cancellation, IDN buffers and directory diagnostics

This builds/packs the ordinary tool, installs that exact artifact through the
existing isolated local feed/cache, and calls real stdio list/get from an unrelated
temporary directory. It prints ID/title/goal, complete canonical Markdown,
target11/edition11, applicability and fresh paired evidence. Tested from an
unrelated starting directory:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Test-TextBoundaries.ps1
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('io.cancel-text-output','text.convert-domain-labels','io.report-directory-failures') `
  -Query 'text-boundaries' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin1081-1083' -BuildAndPack `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory './artifacts/text-boundaries/inspection'
```

Requires PowerShell7, Node22.19+, SDK10.0.403/ref-runtime10.0.13 and complete
SDK11.0.100-rc.2.26504.105/ref-runtime11RC2. Samples execute on Windows x64;
IDN explicitly uses non-invariant system ICU (measured 72.1.0.4), with the loaded
DLL hash/data identity captured. No account, DNS/service, global tools or native
workload. Setup can restore tool/Inspector2.11.0 online; retrieval with unusable
proxies establishes proxy independence, not OS network isolation.

Expect six successful sample builds/runs: real StreamWriter cancellation retains
a 128-byte prefix in the controlled 8192-character case; IDN round trips and
exact/short/invalid/policy checks pass; actual Windows missing-directory operations
populate the current property. Constructor/fallback checks do not replace those
operations. Target10 returns no_applicable_edition and the final exact-artifact
inspection reports PASS. `-Record -VerifySources` on Test-TextBoundaries refreshes
affected proof and rechecks pinned official sources; ordinary runs preserve it.
Stale evidence, canonical-content drift or a nonzero exit is failure. Other-platform,
NLS/invariant IDNA, arbitrary-writer cancellation, atomic rollback, performance and
final-GA claims remain unverified; exactly eight foundations and PIN-959/PIN-962
and existing Human gates remain intact. No native/client matrix is added.

## Pipe handles, mobile locking, native WebAssembly and HKDF inputs

This block builds/packs the ordinary tool, installs that exact package through
the existing isolated local feed/cache, and calls real stdio list/get from an
unrelated temporary directory. It prints ID/title/goal, full canonical Markdown,
explicit target11/edition11, applicability and fresh evidence. Tested from an
unrelated starting directory:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Test-Smoke.ps1 -PackageVersion '0.0.0-pin741-758-1059' `
  -Dotnet 'C:/Users/pingu/AppData/Local/dotnetup/dotnet/dotnet.exe'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('io.use-pipe-handles','io.migrate-mobile-locking',
    'runtime.link-native-wasm-code','crypto.bound-hkdf-inputs') `
  -Query 'native-boundaries' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin741-758-1059' `
  -Dotnet 'C:/Users/pingu/AppData/Local/dotnetup/dotnet/dotnet.exe' `
  -Node 'C:/Program Files/nodejs/node.exe' `
  -OutputDirectory './artifacts/native-boundaries/inspection'
```

Requires PowerShell7, Node22.19+ and SDK10.0.403. Setup can restore the tool and
Inspector2.11.0 online; installed calls use unusable proxies, establishing proxy
independence rather than OS network isolation. Expect four entries and final
exact-artifact PASS; target10 returns no_applicable_edition. Missing canonical
content, stale evidence or a changed target/applicability fails. Apple mobile
behavior and the experimental CoreCLR WebAssembly native sample are explicitly
sample-specific not-tested; source review and intended Emscripten6.0.2 are not
compiled/linked/invoked proof. No new native environment, CI matrix or ninth
foundation is added; comprehensive and GA gates remain open.

To refresh the affected sample evidence, run `& ./eng/Record-NativeBoundaries.ps1
-VerifySources` after selecting this same worktree. That separate command needs
the installed paired SDK10.0.403/runtime10.0.14 and SDK11RC3/runtime11RC3, plus
the existing Docker runtime images. It runs actual Windows HKDF boundaries,
Windows/Linux pipe contracts and portable journal checks, records exact native
assets/input hashes, and retains unavailable-native source-review classifications.
It changes no global tools, PATH or persistent environment variables.

## Integer division, magnitude and format checksums

This block builds/packs the ordinary tool once, installs that exact artifact
through the existing isolated local feed/cache and runs actual stdio list/get
from an unrelated temporary directory. It prints all three IDs/titles/goals,
complete canonical Markdown, explicit target11/edition11, applicability and fresh
paired evidence. Target10 returns `no_applicable_edition`; the older comparisons
add no ninth foundation. Tested from an unrelated starting directory:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Test-IntegerFormats.ps1
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('numerics.choose-integer-division','numerics.measure-integer-magnitude','io.compute-format-checksums') `
  -Query '' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin1076-1078' -BuildAndPack `
  -Dotnet 'C:/Users/pingu/AppData/Local/dotnetup/dotnet/dotnet.exe' `
  -Node 'C:/Program Files/nodejs/node.exe' `
  -OutputDirectory './artifacts/integer-formats/inspection'
```

Requires PowerShell7, Node22.19+, SDK10.0.403 and complete
SDK11.0.100-rc.2.26504.105 with matching runtime/reference10.0.13/11RC2 for the
paired checks. Locked Hashing packages are10.0.13/11RC2, lib/net10/lib/net11.
Setup can restore packages/Inspector online; installed retrieval with unusable
proxies establishes proxy independence, not OS network isolation. No service,
account, native workload, global tool install or persistent environment change.

Expect five signed division modes and invalid/zero/overflow checks, exact integer
magnitude boundaries through10^38 and both 128-bit maxima, four independent
checksum vectors with wire bytes, segmented/reset/short-buffer checks and final
exact-artifact PASS. `-Record -VerifySources` on Test-IntegerFormats refreshes
affected evidence and rechecks pinned official sources; ordinary runs preserve
saved records. Missing/stale evidence, wrong assets, changed canonical content,
wrong target or a nonzero exit fails. RC2, other-platform execution and optional
RC2-package-on-net10 are not GA/compatibility claims; PIN-959/PIN-962 remain open.

## Explicit untested native-platform checks

The [native testing policy](contributing.md#native-testing-policy) keeps existing
Windows/Linux proof while disclosing unavailable Mac, device and CPU checks as
`humanProof.status: not-tested`, with exact `untestedSamples` and scope. These
declared exclusions do not block publication. Other genuinely pending Human
actions and missing/stale locally required compilation remain gated.

This block builds/packs the ordinary tool, installs that exact package using the
existing isolated local feed/cache, and calls real stdio list/get from an unrelated
directory. It prints each ID/title/goal, complete canonical Markdown, applicability
and evidence. It needs PowerShell 7, SDK 10.0.403 and Node 22.19+; setup may restore
packages and Inspector 2.11.0 online. It changes no global tool, PATH or native
environment. The block was exercised from an unrelated starting directory.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('console.color-piped-output','process.capture-output',
    'process.control-lifetime','process.control-native-handles',
    'process.inspect-termination','process.prepare-before-start',
    'sdk.watch-app-hosts-and-devices','simd.select-supported-instructions') `
  -Query '' -TargetVersion '11.0' -PackageVersion '0.0.0-pin699' -BuildAndPack `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory './artifacts/native-testing/inspection'
```

Expect all eight entries, `Not tested:` selection warnings, sample-specific
`not-tested` detail and a final exact-artifact `PASS`. Actual compilation/execution
records retain their kinds and input freshness. An absent warning, lost sample
identity, stale record, changed canonical body or nonzero command is a failure.
This does not execute the excluded platforms, establish source-only compilation,
or assert full-catalogue release readiness.

## Replace positional identity in embedded JSON sequences

This block builds/packs the ordinary tool, then installs that exact artifact
through the existing isolated local feed/cache. Real stdio list/get runs from an
unrelated temporary directory and prints ID/title/goal, complete canonical
Markdown, applicability, target11/edition11 and fresh paired evidence. Target10
returns `no_applicable_edition`; the older comparison adds no ninth foundation.

Requires PowerShell7, Node22.19+, SDK10.0.403 and runtime10.0.13 for inspection.
Consumer recapture also requires SDK11.0.100-rc.2.26504.105/runtime11RC2 and
Windows x64 native SQLite assets. Restoring dependencies and Inspector can use
the network. Retrieval with unusable proxies proves proxy independence, not
OS-level network isolation. No account, external resource, global tool install
or persistent environment change is required. Tested from an unrelated directory:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Test-TrailCards.ps1
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId 'ef.replace-ordinal-json-ownership' `
  -Query '' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin847' -BuildAndPack `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory ./artifacts/pin847/inspection
```

Expect EF10/11 builds and provider PASS results, then discovery title
"Preserve an embedded sequence while replacing positional ownership", its
explanatory goal and the final exact-artifact inspection PASS. The actual EF10
SQLite file is read/edited by EF11 and read back by EF10. Original order,
duplicates and an empty array survive; removal/edit/append reload successfully.
The old mapping triggers the RC2 relational diagnostic. The old table remains
nullable while the new model generates NOT NULL: production schema/data
migration still needs separate review. Use `Test-TrailCards.ps1 -Record` only
to recapture changed sample proof. No other-provider, production migration or GA
proof follows. Missing/stale evidence, body/applicability drift or nonzero exit
is failure; disposable database cleanup is automatic.

## Compiler conversions, declared layout and F# callback adaptation

This builds/packs the ordinary tool once, installs that exact artifact through
the existing isolated local feed/cache and calls real stdio list/get from an
unrelated temporary directory. It displays every ID/title/goal, complete canonical
Markdown, target11/edition11, applicability and fresh paired evidence. Target10
returns `no_applicable_edition`; the original older consumers remain comparisons,
preserving exactly eight foundations. The VB-native source also verifies VB
comment-marker expansion and canonical identity.

Prerequisites: PowerShell7, Node22.19+, SDK10.0.403. Recapturing consumer proof
also needs complete SDK11.0.100-rc.2.26504.105 and runtime/reference10.0.13/11RC2.
Roslyn5.12.0-1.26504.105 restores from Microsoft's public dotnet-tools feed;
FSharp.Core pins are10.1.402/11.0.101-rc2.26504.105. Setup may restore online;
retrieval with unusable proxies proves proxy independence, not network isolation.
No global install, persistent environment change, provider or native workload.
This literal block was tested from an unrelated starting directory:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('languages.inspect-coalescing-conversions','languages.inspect-type-layout','fsharp.adapt-opaque-callbacks') `
  -Query '' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin1035-1037' -BuildAndPack -CheckDemonstratedLanguages `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory ./artifacts/compiler-inspection/installed
```

Expect "Inspect null-coalescing conversions in C# and VB", "Inspect declared
type layout through compiler symbols", "Adapt an opaque curried callback once"
and their explanatory goals, with complete expanded files and current evidence.
`eng/Test-CompilerInspections.ps1 -Record` refreshes changed consumer proof;
without `-Record` it runs the paired actual consumers, IL and diagnostic checks
without replacing saved records. C#/VB check numeric/reference/identity flags
and null/wrong-language failures; layout checks Sequential/2/24, Explicit/1/32,
Auto/0/0 as declarations, not runtime object layout. F# separate consumers check
results, ordered effects, exception propagation and a hoisted Adapt versus the
InvokeFast control; actual FS3350/FS3916 and newer-compiler F#8-mode imports are
distinct from old-compiler or older-target compatibility. RC2 is not GA proof;
coverage, GA and Human gates stay open. A stale/missing record, changed body,
unexpanded snippet, wrong target or nonzero exit fails the check.

## ZIP initialization and invocation locations

This block was verified from an unrelated directory. It builds/packs the ordinary
tool and installs that exact artifact through a fresh local-only feed/cache.
Real stdio list/get displays both discovery IDs, titles and explanatory goals,
complete canonical Markdown, target11/edition11, applicability and fresh evidence.
Target10 returns `no_applicable_edition`; the comparisons add no ninth foundation.

Use PowerShell7, Node22.19+, SDK10.0.403 and runtime10.0.13 for tool inspection.
Sample recapture also needs complete SDK11.0.100-rc.2.26504.105/ref/runtime11RC2,
Windows x64/NTFS and actual symlink permission. Source replay/setup can use the
network; retrieval with unusable proxies establishes proxy independence, not
OS-level network isolation. No account, global tool installation or PATH change.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Record-WeatherMaintenance.ps1 -VerifySources
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('archive.handle-async-initialization','hosting.distinguish-invocation-paths') `
  -Query '' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin1042-1043' -BuildAndPack `
  -Dotnet 'C:/Users/pingu/AppData/Local/dotnetup/dotnet/dotnet.exe' -Node 'C:/Program Files/nodejs/node.exe' `
  -OutputDirectory ./artifacts/weather-maintenance/inspection
```

Expect paired ZIP PASS results, fourteen actual ordinary/single-file child launches,
and the final exact-artifact installed inspection PASS. Both malformed central
directories fail at Entries on .NET10 and at the factory on RC2; missing end records
fail at both factories. RC2 Entries performs zero reads after async initialization.
Successful directory discovery is separate from damaged-payload EOF/CRC rejection.
Successful leaveOpen and caller cleanup after failed initialization are checked.
Launch records show exact invocation/ProcessPath/entry Location; real single-file
Location is empty. Older single-file resolved executable names may retain an alias
basename. No Unix/every-launcher/AOT/GA claim follows; PIN-959/PIN-962 stay open.
A stale record, missing body/snippet/applicability or nonzero command is a failure.

## Date parser settings and numeric representation

This block builds/packs the ordinary tool and installs that exact artifact from a
local-only feed with a fresh cache. Real stdio list/get runs from an unrelated
temporary directory and prints ID/title/goal, complete canonical Markdown,
target11/edition11, applicability and current evidence. Target10 returns
`no_applicable_edition`: the .NET10 comparisons and SDK11/net10 rebuild experiment
remain inside the target11 edition, preserving exactly eight foundations.

Prerequisites: PowerShell7, Node22.19+, SDK10.0.403 and, for recapture, a complete
SDK11.0.100-rc.2.26504.105 installation plus runtime/ref10.0.13 and11RC2. The source hash replay requires internet access; setup can
restore online. Retrieval with unusable proxies proves proxy independence, not
network isolation. No global tool install, PATH change, credential or provider.
The literal block was tested from an unrelated starting directory:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Record-BookingArchive.ps1 -VerifySources `
  -Net10Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Net11Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('globalization.validate-date-parsing-options','numerics.preserve-conversion-intent') `
  -Query '' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin1045-1046' -BuildAndPack `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory ./artifacts/booking-archive/inspection
```

Expect both explanatory discovery entries and the final exact-artifact PASS.
The recorder verifies six pinned official source hashes, both booking executions,
both numeric executions and the SDK11/net10 constant consumer on actual runtime10;
it compares embedded constants separately from runtime-input casts. Compiler,
selected targeting pack and actual runtime hashes accompany each saved record.
Date styles throw ArgumentException on RC2, but the measured format cases require
FormatException/false qualifications. The original allow-list reader rejects
configuration before rows; malformed data remains false. G7/G15 policy checks
cover signs, ties, exponents, small/large values, range/nonfinite rejection and
representative conversion families. RC2's extremely close Decimal-to-Half midpoint
limitation remains explicit; BigInteger-to-Half controls do not invent a changed
finite result, and BFloat16 runs only on net11. No universal conversion, native,
browser or actual-GA claim follows. PIN-959/PIN-962 and Human gates stay open.

A nonzero exit, stale evidence, absent ID, unexpanded snippet or changed
structured/text response is a failed check.

## Span storage and dynamic logical binding

This block selects the actual task worktree and was checked starting from an
unrelated directory. It builds/packs once, installs that exact ordinary artifact
through the existing isolated local feed/cache, then uses real installed stdio
list/get to display both IDs, titles, explanatory goals, complete canonical
Markdown, explicit target11/edition11, applicability and fresh evidence.
Each target10 request must return `no_applicable_edition`; direct retrieval of the
excluded readonly-metadata goal must return `unknown_recipe`.

Run this block in **PowerShell 7 (`pwsh`)**. Windows PowerShell 5.1 changes native
argument/error handling and is outside this helper's prerequisites.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('csharp.preserve-span-storage','csharp.bind-dynamic-conditions') `
  -RemovedRecipeId @('csharp.resolve-readonly-metadata') -Query '' `
  -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin1032-1034' -BuildAndPack `
  -Dotnet 'C:/Users/pingu/AppData/Local/dotnetup/dotnet/dotnet.exe' -Node 'C:/Program Files/nodejs/node.exe' `
  -OutputDirectory ./artifacts/compiler-migrations/inspection
```

Expect **Preserve span storage across nested scopes** and **Preserve dynamic
logical operator binding** with their discovery descriptions and actual paired
evidence. Setup requires PowerShell7, SDK10.0.403/runtime10.0.13 and Node22.19+;
package/Inspector setup may restore online. Retrieval with unusable proxies proves
proxy independence, not OS-level network isolation. The exact eight foundations
and comprehensive/actual-GA/Human gates remain unchanged.

For sample reproduction, also provide SDK11.0.100-rc.2.26504.105/ref/runtime11 RC2
and run `eng/Test-CompilerMigrations.ps1 -SetupBaseline -Record`; supply
`-Net11Dotnet` with the absolute `dotnet` executable path when installed elsewhere. On Windows x64
that helper can download/extract the official SHA512-checked portable SDK10.0.100
historical reproducer under ignored `artifacts/`; no installer or PATH integration
runs. The helper checks real Span/ReadOnlySpan old acceptance versus CS8352,
both old dynamic binder exceptions versus CS7083, paired corrected storage/aliasing
and all16 logical truth-table/invocation-count cases. Ordinary readonly
method-group/lambda/local calls and emitted InAttribute modifiers succeed in both
supported sets. PIN-1033 uses its explicit evidenced-exclusion allowance: no
supported deficient reference closure/CS0518 repair is claimed or taught.
See the [bounded source reconciliation](planning/audits/compiler-contracts.md#compiler-migration-delivery-reconciliation).

## Selected base-image trust, Azure Linux migration and collector observation

This block was checked starting outside the checkout. It builds/packs the ordinary
tool and installs that exact artifact in
the Inspector's isolated local feed/cache. Real stdio list/get display each ID,
title, **goal**, complete canonical Markdown, target11/edition11, applicability and
fresh evidence. Both target10 gets must return `no_applicable_edition`; the paired
older workers add no ninth foundation.

Setup requires PowerShell7, SDK10.0.403/runtime10.0.13, Node 22.19+ and online package
setup. For sample reproduction, also supply SDK11.0.100-rc.2.26504.105 and a working
Linux amd64 Docker engine. Notation1.3.2/ORAS1.3.6 archives, exact Microsoft root
certificates and image digests are checked by the authoring helpers. Signature
verification needs public registry/certificate access; embedded guidance retrieval
uses unusable proxies, establishing proxy independence rather than OS-level network
isolation. Worker runs use Docker `--network none`. No system trust store, production
registry or persistent environment setting is changed by the sample checks.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
$previousCertificateSetup = $env:DOTNET_GENERATE_ASPNET_CERTIFICATE
$env:DOTNET_GENERATE_ASPNET_CERTIFICATE = 'false'
try {
    & ./eng/Inspect-BlazorMigrationGuidance.ps1 `
        -RecipeId @('containers.verify-base-image-signatures','containers.migrate-azurelinux-base-images','testing.migrate-collector-event-inputs') `
        -Query 'quay' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
        -PackageVersion '0.0.0-pin1137-1139' -BuildAndPack `
        -Dotnet 'C:/Users/pingu/AppData/Local/dotnetup/dotnet/dotnet.exe' -Node 'C:/Program Files/nodejs/node.exe' `
        -OutputDirectory ./artifacts/quay-containers/inspection
}
finally { $env:DOTNET_GENERATE_ASPNET_CERTIFICATE = $previousCertificateSetup }
```

Expect **Verify the signature of a selected .NET base image**, **Migrate an
authored Azure Linux base-image selection** and **Observe VSTest collector events
without rewriting outcomes**, with their explanatory discovery goals.
The first records real strict index/platform verification, existing .NET10 signing,
wrong-root/scope and missing/tampered envelope rejection. The second records actual
managed worker execution on digest-pinned Azure Linux3/4, OS/runtime/architecture,
zlib export/call, ICU fr-FR formatting and absent configured-library exit23.
`eng/Test-QuayWorker.ps1 -Record` then `eng/Test-QuaySignatures.ps1 -Record`
reproduce those checks and refresh the existing sample records. Azure Linux4 stays
**Beta**. NativeAOT, other variants/architectures, performance, arbitrary ABI and GA
support remain unqualified. `eng/Test-QuayCollector.ps1 -Record` additionally
restores/builds paired locked18.7/18.11 package closures and loads the real
collector in matching test runs. Four real start/end events match TRX identity,
name, IsChild=false and outcome; collection preserves the disabled run's one
passing and one deliberate failing assertion. Separate constructor fixtures and
a compile-only assignment control confirm public setters were added, not removed.
No child=true/nested-framework transport, MTP or arbitrary collector compatibility
is claimed. PIN-959,
PIN-963 and unrelated Human gates remain open. No recurring native matrix is added.

## Disposable restore graphs, referenced symbols and evaluated imports

This block was checked from an unrelated directory. It builds/packs the ordinary
tool once, installs that exact artifact through the existing isolated local
feed/cache and calls real installed stdio list/get. It prints each ID/title/goal,
complete canonical Markdown, target11/edition11, applicability and current paired
evidence. Every target10 get must return `no_applicable_edition`; the real older
comparisons add no ninth foundation.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('nuget.skip-disposable-restore-graph','sdk.publish-referenced-symbols','msbuild.inspect-import-tree') `
  -Query 'beacon-build' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin1134-1136' -BuildAndPack `
  -Dotnet 'C:/Users/pingu/AppData/Local/dotnetup/dotnet/dotnet.exe' -Node 'C:/Program Files/nodejs/node.exe' `
  -OutputDirectory ./artifacts/beacon/inspection
```

Expect the reusable titles **Avoid writing a disposable CI restore graph**,
**Include referenced symbols in published output** and **Inspect the actual build
import tree**, with explanatory goals and fresh actual evidence. Separately,
`eng/Test-BeaconBuild.ps1 -Record` recreates the paired local-package/locked-restore,
publish and evaluated-import controls with the exact SDK10/11 executable defaults
declared in that helper. It builds/runs both actual consumers and every publish;
checks true/false/default dgspec outputs, exact package hashes/equal selected assets,
normal local repeat and missing-package failure; verifies plural versus ineffective
singular symbol policy and own/reference/snupkg boundaries; and evaluates nested,
conditional, SDK, unset/false, ordering and duplicate/parent import controls.

Setup needs PowerShell7, SDK10.0.403/runtime/ref10.0.13, Node22.19+ and, to reproduce
samples, complete SDK11.0.100-rc.2.26504.105/runtime/ref11RC2. Tool/Inspector setup
may restore online; sample dependencies are original and local-only. Retrieval
with unusable proxies proves proxy independence, not network isolation. Actual
SDK10 already honours graph suppression/import items and includes the reference
PDB. SDK11's plural symbol property defaults true; false excludes that PDB, while
the app's own symbol setting remains separate. No redundant older copy workaround,
cache/no-op equivalence, speed saving, single-file/debugger, arbitrary import order,
other-platform or GA claim is made. PATH/persistent settings, the exact eight
foundations and PIN-959/PIN-963/Human gates remain unchanged.

## Token-binding removal and published F# MVC views

These two 11.0 editions use the existing actual-package Inspector with an isolated
local feed/cache and unrelated working directory. Requires PowerShell7,
SDK10.0.403/runtime10.0.13, SDK11.0.100-rc.2.26504.105/runtime11 RC2 and Node22.19+.
Sample restore and Inspector setup may use the network; retrieved guidance is
embedded. No trust-store change, production identity provider or native workload.

The block was exercised starting outside the checkout. The first command runs
paired real HTTPS/cookie checks, reproduces ASPDEPR010, compiles/renders F# views,
publishes to a clean source-free directory and exercises the missing-view control.
It refreshes the existing sample records. The second command builds/packs once,
installs that exact ordinary artifact and prints what an agent receives.

Use PowerShell 7 (`pwsh`): Windows PowerShell 5.1 changes native JSON-argument
quoting. The fixture retains `artifacts/web-retirement-<guid>` as disposable
published-consumer output for inspection.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Test-WebRetirement.ps1 -RecordEvidence
& ./eng/Inspect-BlazorMigrationGuidance.ps1 -BuildAndPack `
  -Dotnet 'C:/Users/pingu/AppData/Local/dotnetup/dotnet/dotnet.exe' `
  -PackageVersion '0.0.0-pin1185-1186' -Query 'web-retirement' `
  -RecipeId @('authentication.remove-token-binding-assumption','mvc.compile-fsharp-views') `
  -UnsupportedTargetVersion '10.0' -OutputDirectory ./artifacts/web-retirement/inspection
```

Expect both discovery IDs, titles and explanatory goals, complete expanded
Markdown, explicit11.0 applicability and current evidence, with text/structured
equality and unsupported10.0 rejection. The two comparisons add no .NET10
foundation. The F# current consumer prints the actual .Views.dll path and matching
owner load context; its negative publish builds but returns the specific missing
view500. Token binding is RFC8471, not RFC5929 channel binding; removal preserves
the fixture's real authorization/HTTPS policy and supplies no replacement binding
security. Proxy-blocked retrieval is not network-isolation proof. RC2 Windows x64
execution is not actual-GA, arbitrary loader/trimming, browser or Hot Reload proof.
PIN-959/PIN-964 and unrelated Human gates stay open.

## HTTP/3 budgets, JSON Patch intent and early integration-test setup

This block builds/packs the ordinary tool once and installs that exact artifact
through the existing isolated local feed/cache. Real installed stdio list/get run
from an unrelated directory and display ID/title/goal, complete canonical Markdown,
target11/edition11, full applicability and current paired sample evidence. Each
target10 get must return `no_applicable_edition`; these comparisons do not add
another .NET10 foundation. The last command reconstructs all six consumers solely
from the returned Markdown in a temporary directory, without remote NuGet feeds.

Setup needs PowerShell7, Node22.19+, both exact SDK/runtime sets below, Windows
x64 with TLS1.3 and working MsQuic2.5.11.159415436. The first command restores
the locked packages, compares audited ZIP/reference/compile assets and records
compiler/runtime/native identities plus actual requests. No certificate trust,
production resource or persistent environment change is made. Prepared packages
are reused for the returned-body consumers. Retrieval uses unusable proxies;
loopback is bypassed for actual requests. This proves proxy independence, not
network isolation. No recurring native/client/OS matrix is added.

The complete block was checked starting from an unrelated directory:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Test-WebResourceRecipes.ps1 `
  -Dotnet 'C:/Users/pingu/AppData/Local/dotnetup/dotnet/dotnet.exe' `
  -Net11Dotnet 'C:/Users/pingu/AppData/Local/dotnetup/dotnet/dotnet.exe' -RecordEvidence
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('hosting.configure-http3-budgets', 'json-patch.serialize-operation-intent', 'testing.configure-web-application-builder') `
  -Query 'web-resource-recipes' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin1182-batch' -BuildAndPack `
  -Dotnet 'C:/Users/pingu/AppData/Local/dotnetup/dotnet/dotnet.exe' -Node 'C:/Program Files/nodejs/node.exe' `
  -OutputDirectory ./artifacts/web-resources/inspection
& ./eng/Test-WebResourceConsumers.ps1 `
  -Dotnet 'C:/Users/pingu/AppData/Local/dotnetup/dotnet/dotnet.exe' `
  -Net11Dotnet 'C:/Users/pingu/AppData/Local/dotnetup/dotnet/dotnet.exe'
```

Expect these discoveries and observations:

- `hosting.configure-http3-budgets`: **Configure HTTP/3 connection budgets without
  preview scaffolding**. Goal: set per-connection stream/buffer budgets and retain
  backpressure. Both real HTTP/3 consumers queue a third request behind two active
  requests on one connection, exhaust inbound unidirectional credit and pause/
  resume 16MiB uploads/downloads. The old CA2252 control warns; newer builds need
  no preview acknowledgement. No exact process-memory or application quota claim.
- `json-patch.serialize-operation-intent`: **Preserve JSON Patch operation
  serialization intent**. Goal: preserve required from/value intent and nulls,
  checking actual updates before removing custom rules. Six-operation round trips,
  wire members and HTTP failures pass. The current direct method call warns CS0618.
  New from metadata omits null, but retains a spurious non-null from. Both readers
  accept missing replace value as null; stricter validation remains app policy.
- `testing.configure-web-application-builder`: **Configure a web integration test
  through the application builder**. Goal: isolate early startup configuration/
  services and await fixture disposal. Actual separate entry assemblies/TestServer
  requests verify early/late order, production controls and async disposal; the
  newer hook rejects explicit IHostBuilder/IWebHostBuilder paths. Content-root
  attributes make the separate consumers independent of solution discovery; explicit
  service completion handles overlapping teardown. .NET10 keeps
  ConfigureWebHost; only older .NET11 preview fixtures used the renamed spelling.

Actual Windows x64 RC2 proof does not establish other OS/native lanes, strict
JSON Patch validation/security, physical TestServer network behavior or GA.
PIN-959/PIN-964, the eight foundations and unrelated Human gates stay unchanged.

## SDK servicing, partial ReadyToRun and native tool RIDs

The following block was verified starting from an unrelated directory. Setup
requires PowerShell7, Node22.19+, both exact SDK/runtime distributions below,
public NuGet access, MSVC14.51.36231/link14.51.36260.0 and Windows SDK10.0.26100.0
libraries. The focused check runs controlled advisory-cache cases (not a live
security verdict), full/selected PE publishing with JIT fallback, and real native
packing/install/run plus unsupported/missing-toolchain failures. It preserves
process/user/machine PATH and uses disposable homes/caches.

The Inspector builds/packs the ordinary tool once, installs that exact artifact
through an isolated local-only feed/cache, then calls real installed stdio list/get
from an unrelated directory. It displays ID/title/**goal**, full expanded Markdown,
target11/edition11, applicability and current paired sample evidence. Target10 must
return no_applicable_edition; these comparisons add no historical foundations.
Unusable proxies prove proxy independence, not OS-level network isolation.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
& 'C:/Users/pingu/AppData/Local/dotnetup/dotnet/dotnet.exe' build PinguApps.Playbook.slnx -c Release -p:Version=0.0.0-pin1131-batch
& 'C:/Users/pingu/AppData/Local/dotnetup/dotnet/dotnet.exe' pack ./src/PinguApps.Playbook -c Release --no-build -p:PackageVersion=0.0.0-pin1131-batch --output ./artifacts
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('sdk.check-sdk-servicing-status', 'sdk.choose-partial-readytorun-inputs', 'sdk.customize-native-tool-package-rids') `
  -Query 'skylark-build' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin1131-batch' `
  -Dotnet 'C:/Users/pingu/AppData/Local/dotnetup/dotnet/dotnet.exe' -Node 'C:/Program Files/nodejs/node.exe' `
  -OutputDirectory ./artifacts/skylark-build/inspection
```

Expect all six consumers to pass. Current controlled vulnerability/stale and EOL
metadata warn NETSDK1238 and NETSDK1239; missing/corrupt data silently skips.
ReadyToRun checks inspect actual headers, partial selection and exclusions, including
zero unprofiled partial native methods versus a nonempty full control, then
execute direct=21/late=35/cold=SK-7 normally and with forced JIT. Native commands
create outer plus one win-x64 inner package, install locally and report crates=21,
architecture=X64, dynamic=False from a real native payload; unsupported/missing
toolchains fail. Candidate Arm64 defaults are evaluated separately, never advertised
as a delivered payload. No live advisory freshness, performance, other-platform,
GA, production resource or public-publication claim is made. PIN-959/PIN-963 and
unrelated Human gates remain open.

## Read a recipe

### VSTest host, resolver and custom task migrations

This block was exercised from an unrelated directory. Prerequisites: PowerShell7,
the two exact SDK/runtime installations below, Node22.19+ and public NuGet access
for setup. The first command captures the real old/current platform consumers,
compiler controls, test counts/cancellation/cleanup, extension loading and effective
settings, including the scalar semicolon failure and XML alternative. It preserves
process/user/machine PATH and uses isolated homes/caches.

The Inspector then builds/packs the ordinary tool, installs that exact artifact
through a fresh local-only feed/cache and calls real installed stdio list/get from
an unrelated directory. Both targets are explicit. It displays ID/title/**goal**,
complete canonical Markdown, edition11, applicability and current sample evidence;
target10 is a separately executed conditional public-package route. Unusable
proxies qualify proxy independence, not network isolation.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Test-TernTesting.ps1 `
  -Net10Dotnet 'C:/Users/pingu/AppData/Local/dotnetup/dotnet/dotnet.exe' `
  -Net11Dotnet 'C:/Users/pingu/AppData/Local/dotnetup/dotnet/dotnet.exe' -Record
$recipes = @(
  'testing.migrate-vstest-session-host',
  'testing.migrate-assembly-resolution-context',
  'testing.pass-custom-task-runsettings'
)
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId $recipes -Query 'tern-testing' -TargetVersion '11.0' `
  -PackageVersion '0.0.0-pin1128-vstest' -BuildAndPack `
  -Dotnet 'C:/Users/pingu/AppData/Local/dotnetup/dotnet/dotnet.exe' -Node 'C:/Program Files/nodejs/node.exe' `
  -OutputDirectory ./artifacts/tern-testing/inspection11
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId $recipes -Query 'tern-testing' -TargetVersion '10.0' `
  -PackageVersion '0.0.0-pin1128-vstest' `
  -Dotnet 'C:/Users/pingu/AppData/Local/dotnetup/dotnet/dotnet.exe' -Node 'C:/Program Files/nodejs/node.exe' `
  -OutputDirectory ./artifacts/tern-testing/inspection10
```

Expect four distinct discoveries; each ordinary/post-cancel host run has two
passes and one deliberate failure, cancellation has zero results, and observed
owned testhosts exit. Actual plugin resolution returns west:13 with its real
requestor, while the platform's .NET ALC path has a null requestor. Each custom
task selects/passes exactly one settings test with spaces, backslashes, separators
and switch-looking values unchanged. Old escaped literal semicolons survive;
the current scalar rejects them and its XML-file alternative passes.
Missing Tern settings must fail one selected test; XML verification also checks
the effective literal-semicolon value in the real TRX.

The resolver's old setter belongs to the audited SDK10 binary, **not** public18.7;
public18.9 already has the constructor. The current package consumer works on10/11.
No pooled-lifetime equivalence, MTP drop-in, general plugin security, VSTestTask2
runtime, Unix backslash behavior, arbitrary adapters/collectors or actual-GA claim
is made. PIN-959/PIN-963/Human gates and all eight foundations remain open.

The following example was checked from an unrelated directory. Change `-RecipeId` and `-TargetVersion` for the affected guidance; both list and get receive the same explicit target.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Inspect-Guidance.ps1 `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -TargetVersion '11.0' `
  -RecipeId 'blazor.configure-browser-from-server'
```

Expect `blazor.configure-browser-from-server`, its reusable title and goal, complete original .NET 10/11 TideDesk setup, and full target-11 requirements. There must be no unresolved `{{snippet:...}}` token. Compilation, browser execution and historical RC2 limits remain separate. Use `-NoBuild` only after a current Release build. Use `-OutputDirectory` for another disposable output location.

To inspect several changed recipes, pass an array to `-RecipeId`. Discovery follows all returned pages with unchanged filters before selecting those IDs. A missing ID, failed command or incomplete body fails the helper. The file output is the actual MCP response, including `result.structuredContent.recipe`; it can feed existing returned-body consumer checks:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Test-TideDeskConsumer.ps1
```

That consumer uses the inspection response above, reconstructs both actual project pairs outside the checkout, compiles them with their declared SDKs, and checks real RC2 HTTP options. It requires both declared SDK installations. The report defaults to ignored `artifacts/tide-desk/consumer.json`; `-ResponsePath` and `-OutputPath` select alternatives. It does not repeat native browser reconnection.

## Identity UI, encoded routes and gRPC documents

These three original 11.0 editions share one ordinary package inspection. Setup
requires both exact sample SDK/runtime sets, PowerShell7, Node22.19+ and authenticated
GitHub CLI access to the public pinned generator release. The first command restores
the actual locked packages, verifies/acquires the Windows x64 generator, exercises
the paired samples/publishes and refreshes saved sample records. It does not create
fresh browser evidence. Existing browser observations remain valid only while their
relevant page/layout/CSS/package inputs remain unchanged.

The block was exercised from an unrelated directory. The Inspector builds/packs
once, installs that exact artifact through a fresh local feed/cache, and performs
real installed stdio list/get for **each** ID. It displays ID/title/goal, complete
canonical Markdown, target11/edition11, applicability and fresh evidence, compares
text/structured results, and checks target10's `no_applicable_edition` response.
The final command reconstructs all six original consumers solely from those
returned bodies. Prepared packages are reused with no remote NuGet feeds; remote
proxies are unusable and loopback is explicitly bypassed for the real requests.
This establishes proxy independence, not network isolation.

```powershell
Set-Location -LiteralPath 'V:/_worktrees/Playbook/agent-pin-1178-04-103-move-customized-identity-ui-off-bootstrap-4'
& ./eng/Test-WebMigrationBatch.ps1 `
  -Dotnet 'C:/Users/pingu/AppData/Local/dotnetup/dotnet/dotnet.exe' `
  -Net11Dotnet 'C:/Users/pingu/AppData/Local/dotnetup/dotnet/dotnet.exe' `
  -OutputDirectory ./artifacts/batch04/verified -RecordEvidence
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('identity.migrate-ui-framework-selection', 'routing.preserve-encoded-segment', 'grpc.replace-swagger-adapter') `
  -Query 'web-migration-batch' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin1178-batch' -BuildAndPack `
  -Dotnet 'C:/Users/pingu/AppData/Local/dotnetup/dotnet/dotnet.exe' -Node 'C:/Program Files/nodejs/node.exe' `
  -OutputDirectory ./artifacts/batch04/inspection
& ./eng/Test-WebMigrationConsumers.ps1 `
  -Dotnet 'C:/Users/pingu/AppData/Local/dotnetup/dotnet/dotnet.exe' `
  -Net11Dotnet 'C:/Users/pingu/AppData/Local/dotnetup/dotnet/dotnet.exe'
```

Expect these discoveries:

- `identity.migrate-ui-framework-selection`: **Move customized Identity UI off
  Bootstrap 4**. Goal: keep customized sign-in pages rendering by selecting
  Bootstrap5 and migrating affected markup, styles and published assets.
- `routing.preserve-encoded-segment`: **Preserve encoded route segments in
  proxy-style requests**. Goal: retain encoded slashes inside identifiers and
  remove compensation only after actual wire-level comparison.
- `grpc.replace-swagger-adapter`: **Replace the retired gRPC Swagger integration**.
  Goal: keep the transcoded JSON interface discoverable using a generated,
  protobuf-derived build contract after adapter retirement.

The Identity check uses published compiled pages/assets and retains antiforgery,
confirmation and local-return behavior. The separate retained Chromium screenshots
qualify login/validation rendering for the older, explicit5 and default publishes;
the command block does not repeat a browser session. Routing proves the measured
old absolute-form decoding difference and current preservation, with raw parser,
middleware and handler observations. gRPC proves actual old adapter/new generator,
gRPC and JSON values, nonempty operations/schemas/comments and negative requests.
The replacement document is **Swagger2.0/OpenAPIv2**, not a runtime drop-in adapter
or an automatic AddOpenApi integration. Neither generic provider compatibility,
all Google annotation patterns, all browser/platforms nor actual-GA proof follows.
Eight foundations and PIN-959/PIN-963/PIN-964/Human gates remain unchanged.

## Generated static endpoint order

This builds/packs the ordinary tool and installs that exact artifact through the
existing isolated local feed/cache. Real installed stdio list/get execute from an
unrelated directory with unusable proxies, displaying ID/title/goal, full canonical
Markdown, target11/edition11, applicability and fresh paired sample evidence.
The extra get checks target10's `no_applicable_edition` response. Inspection needs
PowerShell7, SDK10.0.403 and Node22.19+. The reconstructed consumer also requires
SDK11.0.100-rc.2.26504.105 and its matching runtime, a preview RC2 baseline, not GA.
The block passes both installed SDK executables explicitly; adjust those arguments
for another installation. Setup may restore online. Retrieval proves proxy
independence, not network isolation. No global installation is required.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId 'web.order-static-asset-routes' -Query 'static-assets' `
  -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin1176' -BuildAndPack `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory ./artifacts/pin1176/inspection
& ./eng/Test-AmberGalleryConsumer.ps1 `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Net11Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe'
```

Expect `web.order-static-asset-routes`, title "Give a generated static endpoint
deliberate route precedence" and goal "Choose which static resource wins when
generated asset routes overlap, using explicit endpoint order and verifying the
resource actually served." Complete Markdown includes both original projects,
physical assets, boundary files, manifest acquisition/projection and numbered
migration. The final helper reconstructs only returned Markdown in a temporary
unrelated directory, builds with both exact SDKs and no remote NuGet feeds, then
executes actual manifest/HTTP/failure checks. It writes routine results under
ignored `artifacts/pin1176/consumer`.

Current results qualify default/explicit/invalid string order, numeric JSON
failure, equal-order ambiguity and frozen-descriptor rejection; .NET10 retains
its working convention alternative. Only the bounded generated-manifest consumer
and Windows x64 managed HTTP lane are qualified, with no general arbitrary-schema,
browser, published-layout or GA guarantee. The eight .NET10 foundations and
PIN-959/PIN-963/PIN-964/Human gates remain unchanged.

## Dynamic OpenAPI document names

This builds/packs the ordinary tool and installs that exact artifact through an
isolated local feed/cache. Real installed stdio list/get run from an unrelated
directory with unusable proxies and display ID/title/goal, complete canonical
Markdown, target11/edition11, applicability and fresh sample evidence. The extra
get checks target10's explicit `no_applicable_edition` response. Setup requires
PowerShell7, SDK10.0.403 and Node22.19+ and may restore online; retrieval proves
proxy independence, not network isolation. No global installation is needed.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId 'openapi.discover-dynamic-documents' -Query 'dynamic-documents' `
  -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin1174' -BuildAndPack `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory ./artifacts/pin1174/inspection
```

Expect title "Generate contracts for dynamically named API documents" and goal
"Discover all OpenAPI document names offered by an application when names come
from configuration, and generate matching runtime and build-time contracts."
The complete body contains both original PlanetariumContracts projects, named
options, resolver, runtime checks, generation commands and numbered migration.
Both public/staff documents have distinct paths/titles. Current controls establish
exact duplicate elimination, explicit plus additional names and missing/empty/null
behavior through the real built-in provider/GetDocument host. Older explicit
registrations remain valid; no downlevel resolver, arbitrary-provider, client,
browser or actual-GA claim is made. PIN-959/PIN-963/PIN-964 and Human gates stay open.

To refresh the bounded proof with both actual SDK/runtime/package sets:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Test-Planetarium.ps1 `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Net11Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' -RecordEvidence
```

Expect real runtime and build-time PASS lines on both targets, plus expected
empty/null-name generation rejection on .NET11. The helper restores its process
settings and checks all PATH scopes. Generated JSON and routine logs stay under
ignored artifacts; named ongoing sample records retain hashes and observations.

## Closed native build extensions

This block builds/packs the ordinary tool, installs that exact artifact through
the existing isolated local feed/cache, then calls real installed stdio list/get
from an unrelated directory. It displays ID/title/goal, complete canonical
Markdown, target11/edition11, applicability and both managed plus actual native
evidence. PowerShell7, SDK10.0.403 and Node22.19+ are required for inspection.
Setup may restore online; unusable proxies during retrieval establish proxy
independence, not network isolation. The inspection itself needs no native tools.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId 'msbuild.register-aot-build-extensions' -Query 'native-build-extensions' `
  -TargetVersion '11.0' -PackageVersion '0.0.0-pin1120' -BuildAndPack `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory ./artifacts/pin1120/inspection
```

Expect "Register a closed set of build extensions in an AOT host", its reusable
goal, all project/SDK/task/resolver/logger/bootstrap/package snippets, numbered
migration, exact package/toolchain conditions and three fresh evidence records.
SDK10 retains the verified managed alternative. SDK11 uses public package18.11.0,
distinct from bundled18.12; native execution verifies the fixed set and missing
task/resolver MSB4283/MSB4282. IL2026 remains visible at the public build boundary.
No analyzer-clean/general AOT, down-level native, arbitrary plugin or actual-GA
claim is made. Target10 remains unsupported for this new registration recipe.

To refresh the actual paired/native proof with the exact prerequisites recorded
in the recipe and sample pins:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Record-NorthQuayBuildHost.ps1 `
  -Net10Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Net11Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe'
```

This first restores both locked package closures, then invokes the existing
sample evidence workflow and real ILC/platform-linker publish/native execution.
An empty `-Packages <absolute-cache-directory>` is supported; no incidental older
native compiler cache is required. It uses absolute executables and child-only
native include/library inputs; PATH and persistent environment must stay unchanged.
The public Framework package supplies the eight trim feature settings, checked
in the actual ILC response. IL2026 is retained, not suppressed. The eight approved
foundations and PIN-959/PIN-963/actual-GA/Human gates remain open.

## Partial project evaluation

This block builds/packs the ordinary tool, installs that exact artifact through
an isolated local feed/cache, and calls real installed stdio list/get from an
unrelated directory. It displays ID/title/goal, complete canonical Markdown,
target11/edition11, applicability and both current museum host evidence records.
PowerShell7, SDK10.0.403 and Node22.19+ are required for inspection. Setup may
restore online; retrieval with unusable proxies proves proxy independence, not
network isolation. No global tool or PATH change is needed.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId 'msbuild.evaluate-project-stages' -Query 'partial-evaluation' `
  -TargetVersion '11.0' -PackageVersion '0.0.0-pin1119' -BuildAndPack `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory ./artifacts/pin1119/inspection
```

Expect title "Evaluate only the project information a tool needs", the reusable
inspection goal, complete museum project/import/readers/bootstrap and numbered
migration. SDK10 retains full evaluation; SDK11 proves all stages and guard
failures, default/explicit Full, real CLI property/item values and separate target
execution. In actual RC2 partial `Targets.Count` throws, superseding Preview7's
zero-count example. The host uses Locator1.11.2 with complete installed SDK closure;
no down-level partial, universal speed, other-platform or actual-GA claim is made.
Target10 remains unsupported for this new partial-evaluation recipe.

To refresh the bounded managed host proof with both complete SDK/runtime installs:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
$previousIntegration = $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
try {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
  & ./eng/Record-Evidence.ps1 -Sample 'samples/net10.0/MuseumInventory/MuseumInventory.csproj' `
    -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe'
  & ./eng/Record-Evidence.ps1 -Sample 'samples/net11.0/MuseumInventory/MuseumInventory.csproj' `
    -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe'
}
finally { $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = $previousIntegration }
```

These sample records pin compiler/reference/native-host/runtime/MSBuild/SDK
resolver artifacts and record all loaded assemblies and actual SDK/import hashes.
The target-count guard and mutable factory failures are real execution evidence,
not zero counts fabricated for unavailable state. The exact eight foundations,
PIN-959/PIN-963/actual-GA and unrelated Human gates remain open.

## Typed custom build-task inputs

This block builds/packs the ordinary artifact and uses the existing isolated
local-only feed/cache Inspector harness. It invokes real installed stdio list/get
from an unrelated directory, displaying ID/title/goal, full canonical Markdown,
explicit target/edition, applicability and both fresh host evidence records.
Setup requires PowerShell 7, SDK 10.0.403 and Node 22.19+; setup can restore online.
Retrieval uses unusable proxies, proving proxy independence, not network isolation.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId 'msbuild.bind-typed-task-inputs' -Query 'typed-task-inputs' `
  -TargetVersion '11.0' -PackageVersion '0.0.0-pin1118' -BuildAndPack `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory ./artifacts/pin1118/inspection
```

Expect title "Bind typed file inputs in a custom MSBuild task", edition 11.0,
target 11.0, experimental `-mt` conditions and two current compilation/execution
records. Markdown retains the explicit-path older task, typed newer task,
environment-dependent defaults, same-binary constructor fallback, numbered
migration and exact-host limits. The saved runtime proof covers actual concurrent
same-process/different-thread task execution, metadata, unchanged task process
state, ordinary/sidecar controls and real missing-file failures. No down-level
typed-task, server-reuse/performance or actual-GA route is claimed.

To refresh the task evidence, run SDK 10 first because the SDK 11 constructor
check executes that same older compatibility DLL. Neither command installs a
global tool or changes PATH; SDK 11 and its matching runtime must already exist:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
$previousIntegration = $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
try {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
  & ./eng/Record-Evidence.ps1 -Sample 'samples/net10.0/ObservatoryTasks/ObservatoryTasks.csproj' `
    -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe'
  & ./eng/Record-Evidence.ps1 -Sample 'samples/net11.0/ObservatoryTasks/ObservatoryTasks.csproj' `
    -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe'
}
finally { $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = $previousIntegration }
```

PIN-959/PIN-963 and unrelated Human gates remain open. There is no recurring
native/client/OS matrix for this managed Windows x64 recipe.

## Typed VSTest message readers

This tested block builds/packs the ordinary tool, installs the exact package in
an isolated local feed/cache, then calls installed stdio list/get from an
unrelated directory. It prints ID/title/goal, complete canonical Markdown,
explicit target/edition, applicability and all three current evidence records.
PowerShell7, SDK 10.0.403 and Node22.19+ are required; setup may use the network.
Retrieval uses unusable proxies, proving proxy independence, not network isolation.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId 'testing.migrate-vstest-message-readers' -Query 'vstest-messages' `
  -TargetVersion '11.0' -PackageVersion '0.0.0-pin1073' -BuildAndPack `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory ./artifacts/pin1073/inspection
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId 'testing.migrate-vstest-message-readers' -Query 'vstest-messages' `
  -TargetVersion '10.0' -PackageVersion '0.0.0-pin1073' `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory ./artifacts/pin1073/inspection10
```

Expect title "Read VSTest messages through the supported typed contract",
edition 11.0 and a conditional target10 route. The laboratory examples retain
actual 18.7/18.11 package and SDK/runtime distinctions, typed TestCase/traits/
TestResult/ITestRunStatistics checks, protocol/header/failure semantics and the
concrete-statistics limitation. Custom resolver/converter extensions have no
replacement promised. `eng/Test-VSTestMessages.ps1 -Record` captures the separate
real published-consumer, foreign-wire, removed-symbol and ABI proof with both
healthy pinned SDK executables. Retrieval is not actual-GA or arbitrary test-host
integration evidence; PIN-959/PIN-963 and independent Human gates remain open.

## Deliberate database-provider replacement

This tested example selects the PIN-1190 worktree from an unrelated directory,
builds/packs the ordinary tool and installs that exact package through an isolated
local feed/cache. The helper calls actual installed stdio `list_recipes` and
`get_recipe`, showing ID/title/goal, complete canonical Markdown, target/edition,
applicability and fresh evidence. Setup requires PowerShell7, SDK10.0.403 and
Node22.19+ and may restore packages online. Retrieval with unusable proxies proves
proxy independence, not network isolation. No global tool or PATH change is needed.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId 'ef.replace-context-provider' -Query 'provider-replacement' `
  -TargetVersion '11.0' -PackageVersion '0.0.0-pin1190' -BuildAndPack `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory ./artifacts/pin1190/inspection
```

Expect `ef.replace-context-provider`, title "Replace a configured database provider
deliberately", its test-configuration goal, target11/edition11 and two current
ReedPermits evidence records. Full Markdown includes both complete original
projects. Provider proof covers actual SQLite/InMemory CRUD, retained/mixed
configuration failures and resolved lifetimes; InMemory is not relational proof.
The options helpers are separate infrastructure operations. Other providers,
platforms, factories/pooling, trimming/AOT and actual GA remain unverified.

## Custom client rules and component cache contracts

This block builds/packs the ordinary tool and uses the existing isolated local-feed/cache
Inspector harness. It changes to an unrelated temporary directory before calling
the exact installed executable over stdio, and displays both recipes' ID, title,
goal, complete Markdown, applicability and current compilation/execution evidence.
Setup requires PowerShell 7, SDK 10.0.403 and Node 22.19+; Inspector 2.11.0 is
installed locally when absent. Setup may use the network; retrieval uses unusable
proxies, which demonstrates proxy independence rather than network isolation.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('blazor.export-custom-client-validation-rules','blazor.declare-component-cache-safety') `
  -Query 'garden-library' -PackageVersion '0.0.0-garden-library' `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' -BuildAndPack `
  -OutputDirectory ./artifacts/garden-library/inspection
```

Expect two explicit `11.0` editions, full canonical-source equality, complete
static-SSR requirements and four current evidence records per recipe. The custom
rule body includes the .NET 10 duplicate handler and .NET 11 provider/JS mapping;
the cache body includes live parameter replay and refusal after initialization.
These checks deliver the guidance; the paired real Chromium/HTTP evidence comes
from `eng/Record-GardenLibrary.ps1`. RC2 is not GA proof. No native or distributed
provider behavior is claimed. The helper retains its unique temporary install/feed
for inspection and restores process settings; it does not install a global tool.

## JavaScript recovery and InputFile cleanup

For JavaScript failure recovery and derived InputFile cleanup, this tested block
builds/packs the ordinary tool, installs that exact package through the isolated
local feed/cache, and calls real stdio list/get from an unrelated temporary
directory. PowerShell 7, SDK 10.0.403 and Node 22.19+ are required for inspection;
setup may use the network. Retrieval uses unusable proxies, which proves proxy
independence rather than network isolation.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('blazor.handle-js-invocation-failures','blazor.dispose-input-file-subclass-resources') `
  -Query 'owned-lifetimes' -PackageVersion '0.0.0-owned-lifetimes' `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' -BuildAndPack `
  -OutputDirectory ./artifacts/owned-lifetimes/inspection
```

Expect both IDs/titles/goals, explicit 11.0 editions, complete expanded canonical
.NET 10/11 files, applicability and four fresh compilation/execution records per
recipe. Recovery preserves application errors; teardown distinguishes RC2's
internally guarded reference disposal from an actual disconnected module call.
InputFile cleanup preserves ordinary selection and its framework callback relay.
The paired browser proof is separate from retrieval, and neither establishes GA.
With both SDKs and the measured Chromium/Playwright installations available,
`eng/Test-OwnedLifetimesConsumer.ps1` reconstructs all four projects solely from
these responses and reruns their real browser/renderer checks. The script's
`-Playwright` and `-Chromium` parameters take the absolute installed paths.

## Live identity tools

For the live identity recipes, this block builds/packs the ordinary tool and
uses the existing isolated local-feed/cache installation. It calls real stdio
list/get from an unrelated directory and prints discovery ID/title/goal, complete
canonical Markdown, applicability and fresh sample evidence. The walkthrough was
launched from an unrelated directory. Setup may restore/download packages;
retrieval uses unusable proxies, which establishes proxy independence rather
than network isolation.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('signalr.refresh-live-credentials','blazor.react-to-refreshed-identity') `
  -Query '' -PackageVersion '0.0.0-live-tool-contracts' `
  -Dotnet 'V:\_worktrees\Playbook\t3-2784b7c8\artifacts\identity-tooling\pinned-dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' -BuildAndPack `
  -OutputDirectory ./artifacts/identity-tooling/inspection
```

Expect two explicit 11.0 editions, original .NET 10/RC2 consumers, the complete
TypeScript development source-build prerequisite, local identity/provider limits
and current compilation/execution records. `eng/Test-LiveIdentityConsumer.ps1`
reconstructs the .NET and TypeScript consumers solely from these installed
responses and executes their real hub and Interactive Server checks; supply
absolute `-Playwright`, `-Chromium`, `-Dotnet` and `-Node` paths. This is bounded
local execution, not Azure SignalR Service, production authentication or GA proof.
The task-local SDK root above preserves the recorded 10.0.403/RC2 baselines
after the machine's installation advanced to 10.0.403/RC3. An equivalent installed
root containing those exact SDKs and runtimes can be supplied instead.

## Runtime overrides and SDK selection for tool execution

From PowerShell 7 with SDK 10.0.403 and Node 22.19+, this builds/packs the
ordinary tool, then reuses the isolated local-feed/cache harness to inspect that
exact artifact over real stdio from an unrelated temporary directory. Inspector
2.11.0 is installed locally if missing. Setup may restore packages online;
retrieval uses unusable proxies (proxy independence, not network isolation).

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('deployment.control-runtime-overrides','sdk.select-tool-execution-sdk') `
  -Query 'host-selection' -PackageVersion '0.0.0-host-selection' `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' -BuildAndPack `
  -OutputDirectory ./artifacts/host-selection/inspection
```

Expect both IDs/titles/goals, explicit edition
11.0, complete expanded canonical Markdown, applicability and four current
compilation/execution records per recipe. The second recipe correctly reports
introduction 10.0: wrapper bypass was serviced into 10.0.302/10.0.400 and was
executed here on 10.0.403 and RC2. This adds no target-10 foundation. The existing
helper's name is historical; its recipe-ID/query parameters accept both recipes.

Actual behavior is separate from retrieval. `eng/Test-RuntimeOverrides.ps1`
executes the original .NET 10/11 samples with isolated matching hostfxr/runtime,
duplicate and nonduplicate properties, source regeneration, Release dev-file
generation/copy and a fresh production artifact allowlist.
`eng/Test-ToolSdkSelection.ps1` executes both original local packages, three SDK
pins, both shipped Windows wrappers, exact quoted arguments and failure exits.
These require the declared Windows x64 SDK/runtime installations; parameters
accept alternate absolute installation paths. SDK 9 is a negative command case
only. Unix wrapper execution and actual .NET 11 GA reconciliation remain unverified.
## Nested EF scalars and primitive sequences

This block builds/packs once, runs the existing installed MCP smoke scenario,
then uses that same artifact through the existing local-feed/cache Inspector
harness. It was checked starting from an unrelated directory. Requires PowerShell
7, SDK 10.0.403 and Node 22.19+; the standalone consumer checks additionally need
the declared .NET 10 and .NET 11 RC2 SDKs/packages. Setup may restore packages
online. Retrieval uses unusable proxies, demonstrating proxy independence rather
than network isolation. No external database or account is needed.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Test-Smoke.ps1 -PackageVersion '0.0.0-ef-models'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('ef.configure-nested-scalar','ef.configure-primitive-sequence') `
  -Query 'ef-models' -PackageVersion '0.0.0-ef-models' `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory ./artifacts/ef-models/inspection
```

Despite the helper's historical name, its recipe/query parameters reuse the same
packed stdio path for these EF recipes. Expect both IDs/titles/goals, explicit
11.0 editions, full expanded original .NET 10/11 projects, applicability and two
current compile-and-run records per recipe. EstuaryMarkers configures the nested
`marker_code`; KilnReadings distinguishes primitive JSON sequences from a binary
scalar. The inspection asserts full canonical body equality and matching
applicability; it does not rerun SQLite. The sample records establish real Windows
x64 SQLite model/schema/round-trips using EF 10.0.13 and EF11 RC2. Other providers,
native platforms, production data conversion and actual GA remain unverified.

## Package identity migration and restore sources

From PowerShell 7 with SDK 10.0.403 and Node 22.19+, this builds/packs the
ordinary tool and reuses the isolated local-feed/cache Inspector harness to call
that exact installed artifact from an unrelated temporary directory. Inspector
2.11.0 is installed locally if absent. Setup may use the network; retrieval uses
unusable proxies, demonstrating proxy independence rather than network isolation.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('nuget.migrate-package-identities','nuget.restore-from-declared-sources') `
  -Query 'nuget-migration' -PackageVersion '0.0.0-nuget-migration' `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' -BuildAndPack `
  -OutputDirectory ./artifacts/nuget-migration/inspection
```

Expect both discovery IDs/titles/goals, explicit 11.0 editions, full original
.NET 10/11 canonical projects/code, applicability and four current compilation/
execution records per recipe. The helper's historical name does not restrict its
recipe parameters. Identity guidance distinguishes legacy-invalid punctuation
from NU5052 and proves direct/transitive migration. Restore guidance records
selected and executed versions for direct/transitive, source/cache/failure cases.
`eng/Test-NuGetMigration.ps1` reproduces the paired local-feed behavior using the
declared SDK/runtime installations; its parameters accept alternate absolute paths.
No public feed operation, other OS execution or actual GA verification is claimed.
Sample SDKs/packages remain author-maintained under the dependency policy.

## EF query adapters and UInt128 parameters

This checked example starts from an unrelated directory, selects this worktree,
builds/packs the ordinary tool and uses the existing isolated feed/cache harness.
It displays ID/title/goal, complete canonical Markdown, applicability and current
sample evidence through real installed stdio list/get. Requires PowerShell 7,
SDK 10.0.403 and Node 22.19+; the separate EF1004 reproduction additionally needs
SDK 11.0.100-rc.2.26504.105. Setup can restore online. Retrieval uses unusable
proxies, demonstrating proxy independence rather than network isolation.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('ef.stream-query-without-reexecution','sqlite.bind-large-unsigned-value') `
  -Query 'data-adapters' -PackageVersion '0.0.0-data-adapters' -BuildAndPack `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory ./artifacts/data-adapters/inspection11
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId 'sqlite.bind-large-unsigned-value' -TargetVersion '10.0' `
  -Query 'data-adapters' -PackageVersion '0.0.0-data-adapters' `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory ./artifacts/data-adapters/inspection10
```

Expect both stable IDs at edition 11.0 and the conditional direct-Sqlite11 route
on target 10.0; no EF11-on-.NET10 route is advertised. Five saved real consumers
cover EF command/disposal and UInt128 padded TEXT/order checks. EF1004 is separately
reproduced by `eng/Test-EfQueryAdapter.ps1`. The EF body explains runtime11's
adapter fast path and SQLite's synchronous native I/O. The UInt128 body retains
explicit string reading/application parsing. Neither retrieval nor these Windows
x64 RC2 consumers establishes other native platforms/providers or actual GA.

## Embedded template-engine hosts

From an unrelated directory, this selects the exact PIN-1071 worktree, builds and
packs the ordinary tool, installs that artifact in a fresh local-only feed/cache,
then displays real stdio discovery ID/title/goal, full canonical Markdown,
applicability, edition and fresh evidence for both advertised targets. Requires
PowerShell 7, SDK 10.0.403 and Node; sample reproduction also needs SDK/runtime 11
RC2 and the public package feeds. Retrieval uses unusable proxies, proving proxy
independence rather than network isolation.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId 'sdk.retarget-template-engine-hosts' -Query 'template-engine' `
  -PackageVersion '0.0.0-template-host' -BuildAndPack `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory ./artifacts/template-host/inspection11
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId 'sdk.retarget-template-engine-hosts' -Query 'template-engine' `
  -TargetVersion '10.0' -PackageVersion '0.0.0-template-host' `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory ./artifacts/template-host/inspection10
```

Expect edition 11.0 on both targets, the conditional newer-package net10 route,
and four current compilation/execution records. The body distinguishes NU1701
implicit fallback from eight strict NU1202 failures, preserves the old netstandard
host library, and demonstrates generated net10 applications from both modern host
TFMs. `eng/Test-TemplateEngineHost.ps1` reproduces the real package/host/generated
consumer checks with absolute SDK parameters. Framework, other OS execution and
actual GA remain unverified; PIN-959/PIN-963 remain open. All new sample dependencies
are author-maintained, outside automated root-package update permissions.

## SQLite target and retained dependency

This checked block starts from an unrelated directory, selects the assigned
worktree, builds/packs the ordinary tool and reuses the existing isolated
local-feed/cache harness. Setup needs PowerShell7, SDK10.0.403 and Node22.19+;
Inspector2.11.0 is installed locally if absent. Setup can restore online;
retrieval uses unusable proxies (proxy independence, not network isolation).

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId 'sqlite.retain-supported-runtime' -TargetVersion '11.0' `
  -Query 'sqlite-runtime' -PackageVersion '0.0.0-sqlite-runtime' -BuildAndPack `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory ./artifacts/sqlite-runtime/inspection11
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId 'sqlite.retain-supported-runtime' -TargetVersion '10.0' `
  -Query 'sqlite-runtime' -PackageVersion '0.0.0-sqlite-runtime' `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory ./artifacts/sqlite-runtime/inspection10
```

Expect the ID/title/goal, retained edition11.0 on both explicit targets, full
canonical projects and Markdown, exact applicability and five current evidence
records. Target10 includes the conditional Sqlite11 route and retained Sqlite10
comparison. Sqlite11 uses net10.0 assets; EF11 still requires net11.0. The net48
alternative has real Windows Framework EXE execution, not a selectable catalogue
target. No UWP/classic Xamarin native execution or actual-GA proof is claimed.
`eng/Record-SqliteRuntime.ps1` separately recaptures the four real consumers with
the declared SDK10/SDK11 and Windows Framework runtime. Retrieval does not rerun SQL.

## Test and extension JSON dependencies

This verified block selects the assigned worktree from an unrelated directory,
builds/packs the ordinary tool, installs that exact package in an isolated local
feed/cache and calls real stdio list/get. Setup requires PowerShell7,
SDK10.0.403 and Node22.19+; the helper installs Inspector2.11.0 locally if absent.
Restores/setup use public packages; retrieval uses unusable proxies (proxy
independence, not network isolation).

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId 'testing.declare-json-dependencies' -TargetVersion '11.0' `
  -Query 'json-dependencies' -PackageVersion '0.0.0-json-dependencies' -BuildAndPack `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory ./artifacts/json-dependencies/inspection11
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId 'testing.declare-json-dependencies' -TargetVersion '10.0' `
  -Query 'json-dependencies' -PackageVersion '0.0.0-json-dependencies' `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory ./artifacts/json-dependencies/inspection10
```

Expect ID/title/goal, edition11.0 on both explicit targets, full original
Markdown/canonical snippets, the conditional newer-package target10 route and
seven current compilation/execution records. The body distinguishes old
transitive JSON, explicit runtime ownership, actual adapter/collector loading,
missing-direct/runtime failures, SDK-root copies and VSTest/MTP selection.
`eng/Test-JsonDependencies.ps1 -Record` separately captures the actual managed
consumer checks with its explicit SDK10/SDK11 parameters. Native CodeCoverage,
other operating systems and actual GA remain unverified; PIN-959/PIN-963 stay open.
The seven sample projects and their exact locks remain author-maintained under
the existing sample dependency policy; no new automatic update permission is added.

## Retired SQLite native bundle initialization

This block was executed from an unrelated directory. It builds/packs the ordinary
tool and reuses the isolated local-feed/cache harness for real installed stdio
list/get. Requires PowerShell7, SDK10.0.403 and Node22.19+; Inspector2.11.0 is
installed locally if absent. Setup may restore online. Retrieval uses unusable
proxies, proving proxy independence rather than network isolation.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId 'sqlite.replace-retired-native-bundle' -TargetVersion '11.0' `
  -Query 'sqlite-initialization' -PackageVersion '0.0.0-sqlite-initialization' -BuildAndPack `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory ./artifacts/sqlite-initialization/inspection
```

Expect ID/title/goal, edition11.0, full canonical .NET10/.NET11 files and prose,
Windows system-engine applicability and three current sample evidence records.
The older bundle comparison is not an additional .NET10 foundation or selectable
current target route. `eng/Record-SqliteInitialization.ps1` separately recaptures
actual Windows x64 native initialization, committed debit/reopen, password refusal
and omitted-initialization failure using SDK10.0.403 and SDK11 RC3. That helper
records the loaded DLL/version/hash; retrieval does not execute SQL. Encryption,
other platforms and actual GA remain unverified. No paid provider is selected.

## Protect data into owned storage

This checked block starts from an unrelated directory, selects the PIN-1171
worktree, builds/packs the ordinary tool and installs that exact artifact in the
existing isolated local feed/cache. It calls real stdio list/get and displays the
ID/title/goal, full canonical Markdown, explicit target/edition, applicability and
fresh evidence. Setup requires PowerShell7, SDK10.0.403 and Node22.19+; paired
sample reproduction also requires SDK11.0.100-rc.2.26504.105 at its declared path.
Setup may restore online; retrieval uses unusable proxies, demonstrating proxy
independence rather than network isolation. Inspector2.11.0 is installed locally.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId 'security.protect-into-buffer' -TargetVersion '11.0' `
  -Query 'owned-buffer' -PackageVersion '0.0.0-museum-buffer' -BuildAndPack `
  -Dotnet 'C:/Users/pingu/AppData/Local/dotnetup/dotnet/dotnet.exe' `
  -Node 'C:/Program Files/nodejs/node.exe' `
  -OutputDirectory ./artifacts/museum-buffer/inspection
```

Expect edition11.0, two current real compilation/execution records, complete
original .NET10 array/.NET11 span/fallback source and advanced AAD checks. The
actual Windows x64 framework CNG key encryptor handles token/empty/1MiB recovery;
wrong purpose/key ring, tampering and changed AAD reject and clear failed output.
Target10 has no applicable span edition; the old array comparison remains in the
11.0 body. No universal allocation count, other backend/platform or actual-GA
claim is made. PIN-959/PIN-964 and existing Human gates remain open. Samples stay
author-maintained under the existing dependency boundary.

## SQLite time components

This block was checked starting from an unrelated directory. It selects the exact
assigned worktree, builds/packs the ordinary tool, then uses the existing isolated
local-feed/cache harness for real installed stdio list/get. Requires PowerShell7,
SDK10.0.403 and Node22.19+; Inspector2.11.0 installs locally if absent. Setup may
restore online. Retrieval uses unusable proxies, proving proxy independence rather
than network isolation. Separate sample execution also needs SDK/runtime11 RC2.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId 'sqlite.query-time-components' -TargetVersion '11.0' `
  -Query 'time-components' -PackageVersion '0.0.0-sqlite-components' -BuildAndPack `
  -Dotnet 'C:/Users/pingu/AppData/Local/dotnetup/dotnet/dotnet.exe' `
  -Node 'C:/Program Files/nodejs/node.exe' `
  -OutputDirectory ./artifacts/sqlite-components/inspection
```

Expect ID/title/goal, explicit target11/edition11, complete canonical EF10/EF11
Markdown, applicability and two current compilation/execution records. The older
parameterized SQL remains server-side; the current predicates execute `%H`/`%M`/`%S`
through SQLite strftime. Native versions3.53.3/3.53.4, stored fractions/boundaries,
exact results and translation failures are captured by the separate HeronSignals
sample records. Inspection does not rerun those databases. No EF11-on-net10,
additional foundation, arbitrary precision/timezone, other-platform or actual-GA
claim is added; PIN-959/PIN-965 and existing Human gates remain open.

## Supply an application-specific cross-origin verdict

This checked block starts from an unrelated directory, selects the exact assigned
worktree, builds/packs the ordinary tool and installs that artifact in the existing
isolated local feed/cache. Real stdio list/get displays discovery ID/title/goal,
full canonical Markdown, explicit edition/target, applicability and fresh evidence.
Setup requires PowerShell7, SDK10.0.403 and Node22.19+; Inspector2.11.0 installs
locally when absent. Setup can restore online; retrieval uses unusable proxies
(proxy independence, not network isolation). Browser/paired consumer reproduction
additionally needs the declared SDK11 RC3 and Chrome; retrieval does not run them.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId 'web.supply-origin-protection-policy' -TargetVersion '11.0' `
  -Query 'seed-exchange' -PackageVersion '0.0.0-seed-exchange' -BuildAndPack `
  -Dotnet 'C:/Users/pingu/AppData/Local/dotnetup/dotnet/dotnet.exe' `
  -Node 'C:/Program Files/nodejs/node.exe' `
  -OutputDirectory ./artifacts/seed-exchange/inspection
```

Expect edition11.0, complete original .NET10 token+origin/.NET11 custom/default/token
source, exact applicability and two current compile-and-run records. The helper
asserts canonical body and structured/JSON-text equality. The custom rule trusts
operator-owned exact origins independently of CORS reads; JSON and token-required
clients remain separate. `eng/Record-SeedExchange.ps1` requires explicit absolute
`-Net10Dotnet` and `-Net11Dotnet` executable paths and recaptures actual matching
consumer/HTTP proof; the existing form evidence document retains actual Chrome156
loopback controls. No net10 ICsrfProtection route is advertised: explicit target10
discovery has no matching edition and get returns an actionable no-applicable-edition
error. No production authentication, other browser/OS, proxy or actual-GA claim is
made; PIN-959/PIN-964 and existing Human gates remain open. These isolated samples
remain author-maintained; no additional automatic dependency permission is granted.

## Inspect a response directly

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
$response = Get-Content -LiteralPath './artifacts/inspection/blazor.configure-browser-from-server.json' -Raw | ConvertFrom-Json
$recipe = $response.result.structuredContent.recipe
$recipe.markdown
$recipe.applicability | ConvertTo-Json -Depth 40
$recipe.evidence | Select-Object sample, kind, target, sdk, freshness | Format-Table
$recipe.humanProof | Format-List
```

Discovery is a compact selection summary: primary/optional routes, version conditions, restrictions, notes, opt-ins and stability warnings. `get_recipe` supplies full reasons, prerequisites, constraints, sources and evidence. Missing summary fields do not mean unrestricted compatibility. Requirements describe the consumer; exact SDK/package/toolchain pins describe recorded verification, not mandatory upgrades.

Both tools require `targetVersion` independently. Inspect the application's target frameworks and project/`global.json` context first. Supported aliases include `10`, `10.0`, `net10.0`, `11`, `11.0` and `net11.0`. Deliberate `latest` is for exploration/unknown targets; it selects the newest installed catalogue release and proves no consumer compatibility. The helper preserves the target as a JSON string.

For a direct Inspector call, use the same absolute Node, Inspector entry point and .NET/server paths established by the helper. For an installed artifact, substitute its absolute `playbook.exe mcp` command for `dotnet <server.dll> mcp`; see [local package installation](contributing.md#inspect-the-development-package). Direct Node launch avoids the Windows npx `inspector is not recognized` wrapper failure. Inspector exits nonzero for a tool error; do not treat an empty response as success.

## Choose further checks

Use the [contribution policy](contributing.md#choose-the-check-and-retain-its-result) for capture/refresh and retention. Inspection verifies delivery, not browser/provider/native execution or release readiness. Saved `freshness: current` means inputs matched at package build time; it does not inspect the consumer or current upstream APIs. Pending Human proof stays visible separately from compilation.

`eng/Test-Content.ps1`, `eng/Test-Evidence.ps1` and their `-Pack` variants exercise content and freshness rejection. The existing installed-package scenarios cover filter combinations, paging, release selection, complete structured/text equality, recovery and errors. Run those when changing the corresponding contract; do not copy their test catalogue into a ticket walkthrough or repeat a platform matrix for a link edit.

## Parsed API explorer route contracts

This tested block builds/packs the ordinary tool and installs that exact artifact
through the existing isolated local-feed/cache harness. Real installed stdio
list/get runs from an unrelated temporary directory and displays ID/title/goal,
complete canonical Markdown, target/edition, applicability and both current
compilation/execution records. Setup requires PowerShell7, SDK10.0.403 and
Node22.19+ and can restore online. Retrieval uses unusable proxies: this proves
proxy independence, not network isolation. No global tool or PATH change occurs.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId 'web.inspect-api-route-pattern' -Query 'route-pattern' `
  -TargetVersion '11.0' -PackageVersion '0.0.0-pin1173' -BuildAndPack `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory ./artifacts/pin1173/inspection
```

Expect "Read parsed route contracts in an API explorer extension", edition11.0,
target11.0, complete .NET10/11 MarineRoutes files and fresh evidence. Each actual
consumer discovers eight MVC/Minimal routes and checks constraints/defaults/
optional/catch-all, nullable/legacy controls and 18 real HTTP outcomes. .NET10
retains bounded source-template parsing; the new parsed property is unavailable
there. Other providers can leave it null. No native/browser matrix, universal
provider support or actual-GA claim; PIN-959/PIN-964 and Human gates stay open.

## Registration confirmation policy

This builds/packs the ordinary tool and installs that exact artifact through the
existing isolated local feed/cache. Real installed stdio list/get from an unrelated
directory display ID/title/goal, complete canonical Markdown, target11/edition11,
applicability and current paired compile/execution evidence. Target10 correctly has
no applicable edition; its working original pages are an executed comparison in
edition11, preserving exactly eight foundations. Retrieval uses unusable proxies:
proxy independence, not network isolation.

The block was executed from an unrelated directory. Setup needs PowerShell7,
SDK10.0.403 and Node22.19+; the final reconstruction also needs SDK11 RC3 and the
matching restored locked packages. Online package setup precedes offline retrieval.
No global tool, production account, messages or third-party provider is required.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId 'identity.confirm-before-registration-signin' -Query 'confirmation' `
  -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin1177' -BuildAndPack `
  -Dotnet 'C:/Users/pingu/AppData/Local/dotnetup/dotnet/dotnet.exe' `
  -Node 'C:/Program Files/nodejs/node.exe' `
  -OutputDirectory ./artifacts/pin1177/inspection
& ./eng/Test-AquariumVolunteersConsumer.ps1 `
  -Dotnet 'C:/Users/pingu/AppData/Local/dotnetup/dotnet/dotnet.exe' `
  -Net11Dotnet 'C:/Users/pingu/AppData/Local/dotnetup/dotnet/dotnet.exe'
```

Expect `identity.confirm-before-registration-signin`, title "Keep registration
aligned with confirmation policy" and goal "Require the configured email, phone
or account confirmation before granting a new registrant a session, across
password and external registration." Both original full projects, lock files and
numbered migration steps must be returned without unresolved snippet tokens.
The final helper recreates only returned files in an unrelated temporary directory,
builds both exact closures without remote feeds and runs 32 actual new-registration
checks plus later confirmation/sign-in and antiforgery/provider/return controls.
It observes the .NET10.0.13 packaged email/phone bypass honestly; the original
CanSignInAsync-gated pages and .NET11 packaged UI defer sessions correctly.
Controlled local external authentication is not third-party interoperability;
RC2 and local SQLite/HTTP evidence are not production, browser or actual-GA proof.

## Structured assembly-conflict and server events

From an unrelated directory, run the existing installed-package Inspector with the
exact SDK10 executable (the SDK11 fixture is a separate authoring prerequisite):

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId 'msbuild.inspect-structured-build-events' -Query 'structured assembly' `
  -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin1121' -BuildAndPack `
  -Dotnet 'C:/Users/pingu/AppData/Local/dotnetup/dotnet/dotnet.exe' `
  -Node 'C:/Program Files/nodejs/node.exe' `
  -OutputDirectory ./artifacts/pin1121/inspection
```

Expect ID `msbuild.inspect-structured-build-events`, title "Consume structured
assembly-conflict and server events", its explanatory goal, edition11.0 and target11.0,
complete canonical Markdown, applicability and current SDK10/11 sample evidence.
The helper builds/packs the ordinary tool, installs that exact artifact through a
local-only feed/fresh cache, then runs real stdio list/get from an unrelated temp
directory with unusable proxies. This proves proxy-independent offline retrieval,
not network isolation. No target10 typed route, unsupported server embedding,
short-lived experimental server execution, universal reuse/performance or actual-GA
claim is made. PIN-959/PIN-963/Human gates remain open.

## NuGet staging uploads

This builds/packs the ordinary tool, installs that exact package through an
isolated local feed/cache and calls real stdio list/get from an unrelated
directory. It prints ID/title/goal, the complete canonical Markdown, target11 /
edition11, applicability and fresh paired evidence. The target10 request must
return no_applicable_edition: the older valid ordinary-publication comparison
stays in the target11 body and is not staging support.

The block was executed from an unrelated directory. Refresh paired evidence with
`eng/Record-FoundryStaging.ps1` on both complete pinned Windows x64 SDK/runtime/ref
installations when sample inputs change. Inspection needs PowerShell7, SDK10.0.403
and Node22.19+. Setup may download tool/test dependencies;
retrieval uses unusable proxies, establishing proxy independence, not network
isolation. The evidence fixture uses only disposable literal127.0.0.1 HTTP and a
generated noncredential marker. No public package upload or real API key is needed.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId 'nuget.stage-package-uploads' -Query 'staging' `
  -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin1124' -BuildAndPack `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory ./artifacts/pin1124/inspection
```

Expect title "Upload packages to a V3 staging endpoint" and goal "Stage a package
and its symbols through a service-advertised endpoint when a publisher needs an
upload step before final publication, with explicit group, transport and failure
handling." Saved paired proof records original SDK pack/PDB validation and real
wire checks. Current staging verifies advertised endpoint/PUT/chunked multipart bytes,
original filenames, route/query, group present/null/empty and key present/absent
without printing its value, HTTP201/400, missing supported resource, default HTTP
refusal, same-origin authorization with default/fixture flags, loopback-only insecure opt-in, received cancellation and request timeout. URI-only policy checks are separate from HTTPS transport proof; malformed header/chunk/length connections also close with complete owned cleanup.
SDK10 verifies actual ordinary publication, fixed package.nupkg filename and its
HTTP400 failure, with no staging API/group equivalent. The fixture stops its own
listener/clients and removes temporary packages. Any nonzero exit, stale evidence,
missing recipe, wrong applicability or unexpanded snippet is a failure.

This qualifies the pinned SDK-bundled public API and faithful local protocol only.
No independently distributed NuGet.Protocol8 package, older-target staging route,
hosted service interoperability, TLS/certificate flow, final publication, atomicity
or GA is claimed. Exact eight foundations and PIN-959/PIN-963/Human gates stay open.

## Reused NuGet hosts, analyzer inventory and launch diagnostics

The following block builds/packs one ordinary artifact and inspects that exact
installed local-feed/cache artifact for all three recipes. Real stdio list/get
from an unrelated directory prints each ID/title/goal, complete canonical
Markdown, target11/edition11, applicability and fresh paired evidence; target10
gets `no_applicable_edition`. The final consumer reconstructs the default
net10/net11 projects and sources from returned Markdown, supplies standard SDK,
import and local-feed scaffolding, then runs actual same-process
MSBuild/configuration and benign-child behaviour outside the checkout.

Prerequisites: PowerShell7, Node22.19+, complete SDK10.0.403 and
SDK11.0.100-rc.2.26504.105 installations, runtime/ref10.0.13 and11RC2. Setup may
restore online; installed retrieval with unusable proxies proves proxy
independence, not network isolation. No global install, provider or real secret.
From an unrelated directory this literal worktree block was tested:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('nuget.reset-reused-host-state','nuget.inspect-analyzer-assets','nuget.own-plugin-launch-diagnostics') `
  -Query 'nuget' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin1125-batch03' -BuildAndPack `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory ./artifacts/batch03/inspection
& ./eng/Test-PackageWorkshopConsumer.ps1 `
  -Net10Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Net11Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe'
```

Expect all three discovery entries, full sources/requirements and current
evidence, then the returned-consumer PASS. The complete negative/lifecycle/asset
suite is separately reproducible with `eng/Record-PackageWorkshop.ps1` using the
same two absolute SDK parameters. It measures real same-host stale/reset/lazy
behaviour and owned open-file/unsubscription/failure controls; actual original
analyzer pack/locked restore/old-current-mixed compiler paths and diagnostic
failures; visible current CS0618 versus zero-warning fixed consumers, and real
missing/valid benign OS child launches with redaction. Expected negative compiler
errors and deliberate task failure are asserted passing controls, not suppressed
production warnings. The pinned SDK does not consume AnalyzerAssets: `_._` is
not enforcement. No third-party automatic reset, provider credential exchange,
other-platform, performance or GA guarantee is made. Eight approved foundations,
PIN-959 qualifying coverage/evidence, PIN-963 actual-GA and Human gates remain open.

## Verify withdrawn compiler guidance

Requires PowerShell 7, SDK 10.0.403 and Node 22.19+. This builds/packs the ordinary
tool and installs the exact artifact from an isolated local-only feed/cache.
Setup can restore online. Installed stdio calls run from an unrelated temporary
directory with unusable proxies (proxy independence, not network isolation).

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId 'razor.replace-runtime-compilation-registration' `
  -RemovedRecipeId @('razor.migrate-generated-document-contracts','razor.copy-intermediate-nodes','razor.choose-task-generation-path') `
  -Query '' -TargetVersion '11.0' -PackageVersion '0.0.0-scope-removal' `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' -BuildAndPack `
  -OutputDirectory ./artifacts/scope-removal/inspection
```

Expect three `PASS` lines confirming each removed ID is absent from discovery and
returns `unknown_recipe` on a direct request. The retained recipe prints its ID,
title, goal, full expanded Markdown and applicability, with current sample evidence
and equal structured/text responses. A remaining ID, any other error or stale/
changed retained content fails the check. No new compiler-consumer or GA claim follows.

## Registry challenges and native publish hooks

Requires PowerShell7, SDK10.0.403 and Node22.19+. This builds/packs the ordinary
tool, installs that exact artifact through a local-only feed and fresh cache,
then exercises real stdio list/get from an unrelated temporary directory with
unusable proxies. Setup can restore online; retrieval proves proxy independence,
not network isolation. The block was tested from an unrelated starting directory.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Test-Smoke.ps1 -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -PackageVersion '0.0.0-pin1142-batch04'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('sdk.configure-redirected-output-encoding','containers.migrate-registry-auth-challenges','sdk.migrate-optimized-publish-hooks') `
  -Query 'estuary' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin1142-batch04' `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory ./artifacts/estuary/inspection-final
```

Expect all three IDs, titles and explanatory goals; full canonical Markdown;
target11/edition11 applicability; and current paired compilation/execution
evidence. Target10 requests must return `no_applicable_edition`: the valid older
examples remain comparisons inside each target11 edition. A missing recipe,
unexpanded snippet, stale evidence or changed structured/text response fails.

The registry proof uses original workers, real Distribution3.1.2 TLS/token
exchanges and image push/run, seven SDK11 refusals before token requests/writes,
private child trust and an internal SDK network. Native proof uses actual Windows
x64 ILC/MSVC-link/PE execution and matched binlogs for old/current legacy versus
migrated hooks, default/false/true, NoBuild refresh/repeat and missing/wrong
sidecars. Refresh changed inputs with `eng/Test-EstuaryRegistry.ps1 -Record` or
`eng/Test-EstuaryWindows.ps1 -Record` on their declared authoring environments.
Inspection itself consumes saved evidence; it does not repeat those native/service
checks. Beta/RC2 limits, eight foundations and comprehensive/GA/Human gates remain.
Encoding proof captures actual paired SDK build diagnostics through pipes under
owned private CP1252/65001 consoles. An explicit UTF-8 reader preserves text and
real failed-build exits; the proposed and source-level opt-outs retain UTF-8 on
these artifacts and are disclosed as unavailable. No legacy producer encoding
route or newer-only reader API is claimed.

## Package streams and TAR hard-link policy

Requires PowerShell 7, SDK 10.0.403 with runtime 10.0.13, SDK/runtime
11.0.100-rc.2.26504.105 / 11.0.0-rc.2.26504.105, Node 22.19+ and
Windows NTFS for the real TAR identity proof. The exact packaging versions are
locked in the samples; no Open XML SDK or separate TAR package is needed.

This block was tested from an unrelated starting directory. Setup may restore
online. It builds/packs the ordinary tool, installs that exact artifact through a
local-only feed and fresh cache, and runs real stdio list/get from an unrelated
temporary directory with unusable proxies. This proves proxy independence, not
network isolation. No public package publication or global tool install occurs.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('archive.adapt-package-part-streams','archive.choose-hard-link-policy') `
  -Query 'archive' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin1040-1041' -BuildAndPack `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory ./artifacts/archive-policy/inspection
```

Expect both discovery IDs, titles and explanatory goals, full expanded canonical
Markdown, target11/edition11 applicability, and fresh compilation/execution
records. Both target10 requests must return `no_applicable_edition`: the valid
older code and RC2 package's measured net10 asset remain comparison evidence.
The script asserts canonical snippet identity and structured/text equality.

Refresh changed consumer evidence on the declared Windows environment:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
$previousIntegration = $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
try {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
  foreach ($sample in @(
    'samples/net10.0/MuseumPackage/MuseumPackage.csproj',
    'samples/net10.0/MuseumPackageRc2/MuseumPackageRc2.csproj',
    'samples/net10.0/ObservatoryTar/ObservatoryTar.csproj',
    'samples/net11.0/MuseumPackage/MuseumPackage.csproj',
    'samples/net11.0/ObservatoryTar/ObservatoryTar.csproj'
  )) {
    $runtime = if ($sample.StartsWith('samples/net11.0/')) {
      'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe'
    } else { 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' }
    & ./eng/Record-Evidence.ps1 -Sample $sample -Dotnet $runtime
  }
}
finally { $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = $previousIntegration }
```

Expect the four real package access cases for all three dependency combinations,
sequential/bounded random access with size failures, and actual NTFS IDs plus
TAR writer/extractor policies, sync/async path/stream extraction, overwrite and
cancellation failures, path/link-target rejection, partial extraction and cleanup.
The separate growth-control stream only instruments the buffering guard; package
and TAR behavior use actual library/filesystem operations. No link-incapable
receiver, other-platform or GA execution is claimed. Comprehensive and GA gates
remain open; no recurring platform matrix is added.

## Fractional rounding and small integer policies

Requires PowerShell7, SDK10.0.403/runtime10.0.13, SDK11.0.100-rc.2.26504.105 /
runtime11.0.0-rc.2.26504.105 and Node22.19+. The tensor samples lock matching
System.Numerics.Tensors10.0.13/11RC2 with actual net10/net11 assets. Scalar rounding
and small conversions need no package. No provider, workload, service or global
installation is needed.

This block was tested from an unrelated starting directory. It builds/packs the
ordinary tool, installs that exact artifact through an isolated local-only feed
and fresh cache, then runs real stdio list/get from an unrelated temporary directory.
Setup may restore online; unusable proxies during retrieval prove proxy independence,
not network isolation. The output displays ID/title/goal, complete canonical Markdown,
target11/edition11 applicability and fresh compilation/execution records.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('numerics.choose-fractional-rounding','numerics.select-small-integer-conversion') `
  -Query 'numeric-policy' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin1047-1048' -BuildAndPack `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory ./artifacts/numeric-policy/inspection
```

Expect both discovery IDs with their reusable titles/goals, full expanded snippets,
current paired evidence, matching structured/text results and no_applicable_edition
for target10. The .NET10 code is a working comparison, preserving eight foundations.
Refresh changed sample inputs using `eng/Record-NumericPolicies.ps1` with the two
absolute SDK executables (`-Net10Dotnet` / `-Net11Dotnet`); `-VerifySources` also
replays nine immutable audit-source hashes. Exact commands, inputs, dependency closure,
compiler/reference/runtime/package hashes and OS/architecture are saved in sample
records. Output includes original midpoint/large/digit/mode controls, real tensor
independent/in-place/capacity/overlap checks and all-five-destination small conversion
plus bounded writer checks. Native results are observations. Windows x64 CoreCLR JIT
is measured; no interpreter/Native AOT/Mono, newer tensor package on net10, other
platform or actual-GA proof. PIN-959/PIN-962 and Human gates remain open.

## Historical dates and tensor ownership preparation

Requires PowerShell7, Node22.19+, SDK10.0.403/runtime10.0.13 and
SDK11.0.100-rc.2.26504.105/runtime11RC2, plus outbound HTTPS to
raw.githubusercontent.com for the recorders' -VerifySources checks. The paired calendar consumers need
no package or workload and require non-invariant globalization with usable ja-JP.
Measured execution is Windows x64 CoreCLR JIT/ICU; NLS and other platforms are
not verified. Tensor examples lock10.0.13/11RC2 with net10/net11 assets. The
full-release storage contract requires a final package containing backport#135281;
successful current execution remains pending under PIN-962. Refresh changed
inputs with the two recorders below. No new platform/native matrix is required.

The following literal worktree block is exercised from an unrelated starting
directory. It builds/packs the ordinary tool, installs that exact artifact from
an isolated local-only feed/cache and calls real stdio list/get from an unrelated
temporary directory. Setup may restore online. Retrieval with unusable proxies
proves proxy independence, not network isolation.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Record-HistoryImport.ps1 -VerifySources
& ./eng/Record-GridTransform.ps1 -VerifySources
& ./eng/Test-Smoke.ps1 -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -PackageVersion '0.0.0-pin1051-1052'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('globalization.validate-japanese-calendar-range','numerics.copy-tensor-views-safely') `
  -Query 'data-validation' `
  -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin1051-1052' `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory ./artifacts/calendar-storage/inspection
```

Expect discovery ID/title/goal, complete canonical Markdown, target11/edition11,
applicability, fresh paired evidence and equal structured/text responses. Target10
must return `no_applicable_edition`: the older code remains a comparison within
the target11 guide and preserves the eight foundations. The recorder executes all
four historical boundaries with GetYear/GetEra, retains source Gregorian values,
and checks that invariant-mode children fail visibly. Any nonzero script exit,
stale evidence, missing recipe or unexpanded snippet is a failure. Tensor discovery
and detail must disclose pending final-package execution. Its current record is
compile with supplementary bounded ownership/Try false policy execution; the
expected-release overlap/output/constructor suite is not claimed to pass RC2/RC3.
After PIN-962 updates final pins, locks and metadata, run Record-GridTransform.ps1
with -RunReleaseChecks -VerifySources and retain actual results before clearing
pending proof. Inspection validates served content and freshness, not the missing
package behavior. RC2 execution does not close actual-GA, release-ready,
comprehensive coverage or existing Human gates.

## Complex signal classification and tensor shape preparation

Requires PowerShell7, SDK10.0.403/runtime10.0.13, SDK11.0.100-rc.2.26504.105 /
runtime11.0.0-rc.2.26504.105 and Node22.19+. The signal samples use framework
Complex. InstrumentBatch locks tensor10.0.13/11RC2 for previous execution/current
compilation; the intended tensor route requires the final11 package with backport
#135281. No provider, workload or experimental opt-in is required.

This block was tested from an unrelated starting directory. It builds/packs the ordinary
tool, installs that exact artifact through an isolated local-only feed and fresh
cache, then calls real stdio list/get from an unrelated temporary directory.
Setup may restore online; unusable proxies during retrieval establish proxy
independence, not network isolation.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('numerics.handle-complex-special-values','numerics.align-tensor-shapes') `
  -Query 'numeric-shapes' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin1049-1050' -BuildAndPack `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory ./artifacts/numeric-shapes/inspection
```

Expect both discovery IDs/titles/goals, complete expanded canonical Markdown,
target11/edition11 applicability, fresh paired compilation/execution evidence,
matching structured/text results and `no_applicable_edition` for target10.
Refresh changed signal inputs using `eng/Record-ComplexSignals.ps1`; add
`-VerifySources` to check the three immutable audit-source hashes. Exact commands,
compiler/reference/runtime identities, inputs, component bits and classifications
are saved in sample evidence. RC2 is not GA proof; only Windows x64 CoreCLR JIT
execution is measured.

Expect the tensor discovery goal and detail to disclose final-package execution
pending. Its previous record is compile-and-run; the current record is compile,
and humanProof.status is pending. Freshness means recorded inputs match, not that
new shape behavior ran. Published RC2/RC3 packages lack the behavior; inspection
checks served guidance and metadata only. The native-width suite remains unexecuted.
PIN-962 owns final SDK/runtime/package, all small and native checks and evidence
refresh. Under Pingu's preparation decision, this PR may deliver the guide while
release-ready packaging, comprehensive, GA and existing Human gates stay open.

Refresh affected tensor preparation evidence with:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Record-InstrumentBatch.ps1 -VerifySources
```

Expect previous-policy execution, current compilation and four pinned source hash
checks. This command intentionally does not execute the current expected-release
checks. After PIN-962 updates pins, locks, dependency declarations and metadata for
the actual final artifacts, use -RunReleaseChecks with the final SDK paths. That
explicit option also runs the large native proof, requiring Windows x64 and about
8GiB measured free physical/commit memory. A refused/failed native run leaves the
proof pending; do not clear it based on the smaller checks.
# Calibration bounds and host failure boundaries

Requires PowerShell7, Node22.19+, SDK10.0.403/runtime10.0.13 and
SDK11.0.100-rc.2.26504.105/runtime11RC2. CalibrationImport locks tensors10.0.13 /
11RC2; AquariumImport locks full Hosting10.0.13/11RC2 with its actual transitive
closure. No provider, native library/workload, account or service is needed.

The following block is tested from an unrelated starting directory. It builds/
packs the ordinary tool, installs that exact artifact from an isolated local-only
feed/cache, then calls real stdio list/get from an unrelated temporary directory.
Setup may restore online. Unusable proxies during retrieval prove proxy independence,
not network isolation. No public publication or global installation occurs.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Test-Smoke.ps1 -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -PackageVersion '0.0.0-pin1053-1054'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('numerics.validate-tensor-clamp-bounds','hosting.observe-background-failures') `
  -Query 'calibration-host' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin1053-1054' `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory ./artifacts/calibration-host/inspection
```

Expect one passing installed smoke, then both discovery IDs/titles/explanatory
goals, complete expanded canonical Markdown, target11/edition11 applicability,
fresh paired compilation/execution records, equal structured/text responses and
`no_applicable_edition` for both target10 requests. Missing/stale evidence or an
unexpanded/different snippet fails. The older code remains a comparison, preserving
exactly eight foundations. Inspection consumes saved evidence; it does not repeat
consumer behavior. To refresh changed consumer inputs and the eight immutable
source hashes on the declared environment:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Record-CalibrationHost.ps1 -VerifySources
```

Expect float/decimal raw-versus-validated scalar/pairwise cases at lengths1/3/33,
reversed/equal/ordered and late-invalid bounds, capacity/length/NaN and unchanged
failed output/correct values; then35 lifecycle children plus the canonical demo
per runtime. Host checks separately assert signalled startup, single/multiple/
shutdown exception identities, logs, chosen exit17 versus0, normal cancellation,
Ignore still running, disposal and child reaping. The explicit older failure
journal and current boundary both preserve failure exit17. PATH and persistent
environment values are checked unchanged. Expected diagnostic failure logs from
the demonstration are passing controls. The recorder ends successfully; a nonzero
script exit is a failure. Windows x64 CoreCLR JIT is measured; browser/custom-host,
other-platform/engine, newer-package-on-net10 and actual-GA claims are unverified.
PIN-959/PIN-962 and existing Human gates remain open; no recurring matrix is added.

## File load, watch root and certificate issuer boundaries

Requires PowerShell7, Node22.19+, SDK10.0.403/runtime10.0.13 and the separate
SDK11.0.100-rc.2.26504.105/runtime11RC2 installation. File-provider samples lock
Configuration.Json/FileProviders.Physical10.0.13/11RC2 with actual net10/net11
assets. Certificate consumers use framework X509 APIs and existing Linux Docker.
No workload, account, global install or production service is needed.

The following literal worktree block is tested from an unrelated starting directory.
It builds/packs the ordinary tool, installs the exact artifact through an isolated
local-only feed/cache, then calls real stdio list/get from an unrelated temporary
directory. Setup can restore online; retrieval with unusable proxies proves proxy
independence, not network isolation. The Linux behavior fixture separately uses
network-none containers with loopback available.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Test-Smoke.ps1 -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -PackageVersion '0.0.0-pin1055-1056-1060'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('configuration.observe-file-load-failures','io.compose-file-watcher-roots','crypto.supply-chain-intermediates') `
  -Query 'file-chain' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin1055-1056-1060' `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory ./artifacts/file-chain/inspection
```

Expect one passing installed smoke, then all three discovery IDs, reusable titles
and explanatory goals, complete expanded canonical Markdown, target11/edition11,
applicability, fresh paired evidence and identical structured/text responses.
Target10 must return `no_applicable_edition`; the valid older approaches remain
comparisons within target11 and preserve exactly eight foundations. A nonzero exit,
missing recipe/snippet, changed body or stale evidence fails the check.

Refresh changed sample inputs and eleven immutable source hashes on the declared
Windows NTFS/Docker environment:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Record-FileChain.ps1 -VerifySources
```

Expect actual JSON initial/explicit/reload/missing/deletion/recreation and Windows
exclusive-lock failure/recovery, callback type/Ignore/data/token results, separately
labelled synthetic stream/GetFileInfo failures and bounded unignored-reload children.
Expect both watcher constructors, null/NUL/unrelated/prefix siblings and current
real equal/ancestor/descendant/empty/relative/deferred tokens, recursion, arming,
canonical factories and cleanup. Older ancestor/relative construction acceptance
is not advertised as reliable monitoring; the older factory uses equal normalized
paths. There is no polling/browser/symlink-security or universal event-count claim.

ArchiveManifest compilation records are distinct from `linux.evidence.json`.
Each runtime executes four fresh, removed network-none/read-only containers with
private HOME/cache tmpfs and random CA material. Expect old three-fetch success,
current two-fetch PartialChain, supplied-chain success with zero downloads,
UntrustedRoot/NotTimeValid rejection and real manifest signature/tamper controls.
Loaded OpenSSL3.3.7/3.5.4 library versions/hashes, exact image digests and cleanup
are saved. CustomRootTrust/NoCheck are private offline-fixture policy, not production
trust or online-revocation/TLS proof. No shared roots/caches are edited. Process,
user and machine PATH and persistent user/machine environment are checked unchanged.

Windows x64 NTFS and Linux x64 OpenSSL are measured. Other-platform/filesystem,
newer-package-on-net10, TLS, online revocation and actual-GA behavior remain
unverified. Inspection consumes saved proof rather than rerunning these behaviors.
PIN-959/PIN-962 and existing Human gates stay open; no native CI matrix is added.

## Certificate extension state, client TLS chains and file aliases

Requires PowerShell7, Node22.19+, SDK10.0.403/runtime10.0.13 and the separate
SDK11.0.100-rc.2.26504.105/runtime11RC2 installation. All six consumers use framework
APIs with no NuGet packages/workloads. Authoring uses Windows x64 local NTFS and
the existing digest-pinned Linux x64 OpenSSL runtime containers.

This block was tested from an unrelated starting directory. It builds/packs the
ordinary tool, installs that exact artifact using an isolated local-only feed/cache,
then calls real stdio list/get from an unrelated temporary directory. Setup may
restore online; retrieval with unusable proxies proves proxy independence rather
than network isolation. The separate TLS fixtures use network-none containers.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Test-Smoke.ps1 -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -PackageVersion '0.0.0-pin1061-1063-1075'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('crypto.replace-encoded-extension-data','tls.supply-client-certificate-chain','io.create-hard-links') `
  -Query 'certificate-links' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin1061-1063-1075' `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory ./artifacts/certificate-links/inspection
```

Expect one passing installed smoke and all three discovery IDs, reusable titles
and explanatory goals, complete expanded canonical Markdown, target11/edition11
applicability, fresh paired evidence, structured/text equality and
`no_applicable_edition` for target10. The old consumers are comparisons within the
target11 guide; exactly eight foundations remain. A nonzero exit, changed/missing
snippet, mismatched body or stale evidence fails. Inspection consumes saved evidence;
it does not rerun native consumer behavior.

Refresh affected samples and fourteen immutable selected/supplemental source hashes:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Record-CertificateLinks.ps1 -VerifySources
```

Expect the original .NET10 RawData stale-cache failure and both supported update
choices on both releases, all decoded fields/raw bytes/OID/Critical true/false,
malformed lazy decode, getter-only use and the isolated current-target old-setter
SYSLIB0065 diagnostic without suppression. Current supported code builds with
warnings as errors. This covers one Basic Constraints extension type.

Expect Windows local NTFS native/File/FileInfo aliases to share writes in both
directions and retain data when either name is deleted first. Destination-exists,
absent-target and directory-target attempts reject without clobbering data or
creating the missing alias. Native error/current exception values are observations,
not an all-platform taxonomy. Cleanup verifies and removes only the created tree.
No Unix/network-share/unsupported-filesystem/cross-volume result is claimed.

Expect four fresh private containers per TLS runtime. The old no-custom-policy
flow fetches one missing client issuer and exchanges booking/ACK data; the current
flow fails PartialChain without fetching. Explicit server ExtraStore/downloads-off
succeeds on both, while unrelated-root and expired-client controls reject without
fetching. Clients send only their leaf; callbacks require strict validation. TLS1.3,
OpenSSL3.3.7/3.5.4, loaded native/managed hashes, certificate inputs, exact images,
commands and cleanup are recorded separately from Windows compilation. Private
HOME/cache/tmpfs and listeners/keys disappear; no shared trust store is modified.
NoCheck is for this offline fixture, not proof of online revocation. Compatibility
switches, client-supplied full chains, Windows TLS and final-GA behavior are untested.

Process/user/machine PATH and persistent user/machine environment remain unchanged.
No production resources, public publication or recurring native matrix is added.
PIN-959/PIN-962, other delivery owners and existing Human gates remain open.

## Legacy signatures and RSA padding

Requires PowerShell7, Node22.19+, SDK10.0.403/runtime10.0.13 and the separate
SDK11.0.100-rc.2.26504.105/runtime11RC2 installation. All four consumers use
framework cryptography APIs without packages/workloads. Windows x64 CAPI provider24
and disposable RSA2048 keys provide the real interoperability path. The public-only
DSA fixture was generated on supporting Windows; no Mac key generation is needed.

This literal worktree block is tested from an unrelated starting directory. It
builds/packs the ordinary tool, installs that exact artifact through an isolated
local-only feed and new cache, then calls real stdio list/get from an unrelated
temporary directory. Setup may restore online; unusable proxies during retrieval
prove proxy independence, not network isolation. No public/global installation,
production container access, native installation or persistent setting change occurs.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Test-Smoke.ps1 -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -PackageVersion '0.0.0-pin1057-1062'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('crypto.migrate-dsa-signatures','crypto.make-rsa-padding-explicit') `
  -Query 'signature-padding' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin1057-1062' `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory ./artifacts/signature-padding/inspection
```

Expect one passing installed smoke, then both discovery IDs, reusable titles and
explanatory goals, complete expanded canonical Markdown, target11/edition11,
applicability, fresh paired evidence and equal structured/text responses. Target10
returns no_applicable_edition; old code remains a working comparison and the eight
foundations remain unchanged. Missing/stale evidence, snippet/body mismatch or any
nonzero exit is a failure. Inspection consumes saved evidence; it does not repeat
provider behavior or establish the excluded macOS outcomes.

Refresh affected exact inputs, compilation/execution and nine immutable sources:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Record-SignaturePadding.ps1 -VerifySources
```

Expect paired Windows DSA fixture/ECDSA P256/SHA256/DER checks: imported public
legacy signature, independent certificates and SPKI peer, tamper/encoding/key/
algorithm rejection and explicit unauthenticated UnsupportedLegacy handling.
Mac .NET10 DSA verification, .NET11 DSA factory/provider/certificate failures and
both releases' ECDSA certificate/signature/routing checks remain sample-specific
not-tested under PIN-699; no Apple native asset inspection is claimed.

Expect three real CAPI processes: old-send, new-exchange and old-receive. Both
false/Pkcs1 and true/OaepSHA1 cross-decrypt in both directions; explicit/canonical
roundtrips succeed, wrong padding/zeroed ciphertext and OAEP-SHA256 reject. An
isolated unchanged old-source/current-target build fails specifically SYSLIB0064,
without suppression. Supported consumers build with zero warnings/errors.
Ciphertext hashes differ across fresh randomized encryptions; recovered plaintext
matches. The private disposable key/ciphertext/diagnostic directory is removed and
its absence checked; no named/production container is used. No stronger-provider
redesign or authentication guarantee follows. Process/user/machine PATH and
persistent user/machine environment are checked unchanged. RC2 evidence is not GA;
PIN-959/PIN-962 and existing Human gates stay open, with no recurring native matrix.

## Colour channels and span extrema

The original SignalLegend and ObservatoryBatch consumers require only the two
pinned shared SDKs. No package, native workload, provider or device is needed.
Refresh affected sample records and verify the eleven selected immutable sources:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Test-ChannelExtrema.ps1 -Record -VerifySources
```

Expect four passing real consumers: distinct RGBA/ARGB known words and endian
round trips, byte/float preservation, span length/copy and equality/hash checks;
value/reference/nullable extrema, explicit/default comparers, ignored/all-null
entries and distinct empty contracts. Reference SHA256 values match the source
audit; output records compiler/CoreLib identities. No colour processing, NaN,
performance, other-platform or final-GA claim follows.

This block is exercised from an unrelated starting directory. It builds/packs the
ordinary tool once and inspects that exact package through an isolated local-only
feed/cache and real installed stdio calls from a temporary unrelated directory.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Test-Smoke.ps1 -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -PackageVersion '0.0.0-pin1079-1080'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('numerics.preserve-color-channels','collections.find-span-extrema') `
  -Query 'channel-extrema' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin1079-1080' `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory ./artifacts/channel-extrema/inspection
```

Expect a passing installed smoke and both discovery IDs, reusable titles and goals,
full expanded canonical Markdown, target11/edition11, applicability and fresh paired
records. Structured/text fallback must agree. Target10 returns no_applicable_edition;
these .NET10 consumers compare older approaches within target11 guidance and add no
foundation. The Inspector prints exactly what an agent receives and saves JSON plus
the package hash. Retrieval with unusable proxies establishes proxy independence,
not network isolation. Setup may restore online; no new native/client/OS matrix.
PIN-959 comprehensive coverage, PIN-962 GA and existing Human gates remain open.

## Reduced-precision values and finite CBOR nesting

Requires PowerShell7, Node22.19+, SDK10.0.403/runtime10.0.13 and the separate
SDK11.0.100-rc.2.26504.105/runtime11RC2 installation. SensorArchive uses framework
APIs only. CalibrationGroups pins System.Formats.Cbor10.0.13/11RC2 with actual
lib/net10.0/lib/net11.0 assets. No account, provider, workload or native setup.

Refresh affected samples through the existing evidence workflow:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Test-PrecisionDepth.ps1 -Record -VerifySources
```

Expect four real passing consumers and ten matching immutable sources. Numeric
checks measure reading/mean error budgets, exact/rounded values, range versus
Half, signed zero/infinity/NaN, underflow/overflow and actual generic math. CBOR
checks compare accepted values/bytes, depth8/9, arrays/maps and definite/indefinite
containers, truncation/extra/trailing data, current zero/-1/below-1 and reader64/65
versus writer1000/1001. The actual old10.0.13 reader already enforces64; its writer
completes1001. Commands, locks, compiler/CoreLib and audit-matching references,
package archives and executed assemblies are recorded. PATH/persistent settings
are checked unchanged. A nonzero exit is failure.

This literal block was exercised from an unrelated starting directory. It builds
and packs the ordinary tool, installs that exact artifact through an isolated
local-only feed/cache, then calls real installed stdio list/get from an unrelated
temporary directory. Setup may restore online. Unusable proxies during retrieval
prove proxy independence, not network isolation.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Test-Smoke.ps1 -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -PackageVersion '0.0.0-pin1084-1085'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('numerics.choose-bfloat16','cbor.bound-nesting') `
  -Query 'precision-depth' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin1084-1085' `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory ./artifacts/precision-depth/inspection
```

Expect one passing installed smoke and both discovery IDs, reusable titles and
explanatory goals, complete expanded canonical Markdown, target11/edition11,
applicability and fresh paired evidence. Structured/text responses agree; target10
returns no_applicable_edition. Missing/stale evidence, changed snippets/body or
a nonzero exit fails. Inspection consumes saved consumer proof. Eight foundations
remain unchanged. Windows x64 RC2 is measured, not other-platform or final-GA
behavior; no BFloat16 serialization/performance or unlimited-depth claim follows.
PIN-959/PIN-962 and existing Human gates remain open; no recurring matrix is added.

## Atomic flags and typed function-pointer calls

Requires PowerShell7, Node22.19+, SDK10.0.403/runtime10.0.13 and the separate
SDK11.0.100-rc.2.26504.105/runtime11RC2 installation. Both StationFlags and
QuarryCalls use framework APIs only, with no packages/workloads. QuarryCalls
requires Windows x64 CoreCLR dynamic code and the system MulDiv export; unsafe
C# is enabled. No native compiler, account or external service is needed.

Refresh affected samples through the existing evidence workflow:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Test-RuntimeCalls.ps1 -Record -VerifySources
```

Expect four passing consumers and eight matching immutable sources. Flags checks
cover 128 synchronized prior-value chains and 40,000 mixed updates, unrelated-bit
preservation, current integer/enum backing sizes, sign/high bits and unsupported
struct/floating-point rejection. Call checks execute real managed and native MulDiv
calls, signed rounding, zero-divisor/overflow sentinels, direct pointer comparison,
injected false dynamic-code guard and current invalid-signature rejection. Saved
records retain exact commands/inputs, compiler/CoreLib/native identities and
audit-matching references. PATH/persistent settings must remain unchanged.

This literal block is tested from an unrelated starting directory. It builds and
packs the ordinary tool, installs that exact artifact through an isolated local-only
feed/cache, then calls installed stdio list/get from an unrelated temporary directory.
Setup may restore online; unusable proxies during retrieval prove proxy independence,
not network isolation.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Test-Smoke.ps1 -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -PackageVersion '0.0.0-pin1094-1096'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('threading.update-enum-flags-atomically','reflection.emit-function-pointer-calls') `
  -Query 'runtime-calls' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin1094-1096' `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory ./artifacts/runtime-calls/inspection
```

Expect one passing installed smoke and both discovery IDs, reusable titles and
explanatory goals, complete expanded canonical Markdown, target11/edition11,
applicability and fresh paired evidence. Structured/text responses must agree;
target10 returns no_applicable_edition. Missing/stale evidence, changed snippets/body
or a nonzero exit fails. Inspection consumes saved behaviour proof, without rerunning
consumers. Eight foundations remain unchanged. Windows x64 RC2 is measured; no
multi-field atomicity, performance, x86/other ABI, Native AOT, arbitrary ILGenerator,
marshalling or final-GA execution is established. The unsupported-dynamic-code
branch is injected, not an execution on an unavailable runtime. PIN-959/PIN-962
and existing Human gates remain open; no recurring native/client/OS matrix is added.

## AEAD key size and file content type mappings

Requires PowerShell7, Node.js and the existing SDK10.0.403/runtime10.0.13 and
SDK11.0.100-rc.2.26504.105/runtime11RC2 installations. ParcelKeys requires actual
AES-GCM and AES-CCM support; both execute on Windows x64 CNG. AttachmentLabels
uses only framework tables. No new package dependency, workload, native compiler,
production key or external service is required. Refresh affected evidence with:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Test-AttachmentFacts.ps1 -Record -VerifySources
```

Expect four passing actual consumers, ten immutable source hashes and matching
audit references/runtime/compiler identities. Key checks cover 16/24/32-byte keys,
real GCM/CCM round trips/tampering, invalid lengths and metadata versus disposed
operations, with loaded Windows CNG identities. Mapping checks cover string/span,
casing/unknown/null/default, path/final-extension/query/whitespace/parameters and
reverse ambiguity. Saved records use the existing input-freshness workflow.

This literal block is tested from an unrelated starting directory. Build/pack
may restore online. It installs that exact ordinary artifact through a fresh
local-only feed/cache, then calls installed stdio list/get from an unrelated
temporary directory with unusable proxies. This establishes proxy-independent
offline retrieval, not network isolation.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Test-Smoke.ps1 -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -PackageVersion '0.0.0-pin1102-1105'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('crypto.inspect-aead-key-size','mime.map-file-and-content-types') `
  -Query 'attachment-facts' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin1102-1105' `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory ./artifacts/attachment-facts/inspection
```

Expect one passing installed smoke and both discovery IDs, reusable titles and
explanatory goals, complete expanded canonical Markdown, target11/edition11,
applicability and current paired evidence. Structured/text responses must agree;
target10 returns no_applicable_edition. Missing/stale evidence, changed canonical
snippets/body or nonzero exit fails. No other-provider/platform, file-content
validation, upload safety, universal mapping stability or final-GA proof is
established. The exact eight foundations, PIN-959/PIN-962 and other Human gates
remain unchanged; no recurring native/client/OS matrix is added.

## SOCKS hostnames and Zstandard responses

Requires PowerShell7, Node.js, SDK10.0.403/runtime10.0.13 and the separate
SDK11.0.100-rc.2.26504.105/runtime11RC2 installation. CairnRelay needs loopback
TCP and a nonresolving relay.cairn.invalid name. CairnReports uses the runtime
native codec on .NET11 and the explicitly pinned managed ZstdSharp.Port0.8.6
lib/net9.0 asset on .NET10. No account, production service, system DNS/route,
workload or external native zstd installation is needed. Restore may use network.
Refresh changed consumer evidence with:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Test-ProxyResponses.ps1 -Record -VerifySources
```

Expect four passing real consumers and ten matching immutable publisher sources.
SOCKS checks receive ATYP3/hostname/port and connect to a real target, authenticate,
reject method/auth/CONNECT failures and cancel negotiation. The old socks5 route
works; its unsupported socks5h alias is rejected. HTTP checks negotiate actual
zstd/gzip/deflate/br, verify bodies/headers, preserve the inner layer in both
stacked orders, leave unknown outer encoding untouched, reject malformed zstd,
cancel a stalled body and enforce the canonical reader's 64KiB decoded limit.
The recorder verifies exact framework audit hashes, compiler/CoreLib/runtime,
resolved package archive/selected/executed decoder and loaded native codec.
PATH and persistent settings must remain unchanged. Any nonzero exit is failure.

This literal block is verified from an unrelated starting directory. It builds/
packs the ordinary tool and installs that exact artifact through a fresh local-only
feed/cache, then calls real installed stdio list/get from an unrelated temporary
directory. Build/restore may use network. Unusable retrieval proxies demonstrate
proxy-independent offline content, not network isolation.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Test-Smoke.ps1 -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -PackageVersion '0.0.0-pin1104-1107'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('http.resolve-dns-through-socks-proxy','http.decode-zstandard-responses') `
  -Query 'proxy-responses' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin1104-1107' `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory ./artifacts/proxy-responses/inspection
```

Expect one passing installed smoke and both discovery IDs, reusable titles and
explanatory goals, complete expanded canonical Markdown, target11/edition11,
applicability and fresh paired evidence. JSON text and structured output agree;
target10 returns no_applicable_edition. Missing/stale proof, changed snippet/body
or nonzero exit fails. No other-platform/native-handler, production proxy,
global DNS privacy or final-GA behavior is established. Eight foundations,
PIN-959/PIN-962 and existing Human gates remain intact; no recurring matrix added.

## Aligned memory and XML reference budgets

Requires PowerShell7, Node.js, SDK10.0.403/runtime10.0.13 and the separate
SDK11.0.100-rc.2.26504.105/runtime11RC2 installation. TypeLedger requires Windows
x64 CoreCLR dynamic code for RunAndCollect and unsafe pointer/span access.
PermitManifest pins Xml10.0.13/Xml11RC2 with matching transitive Pkcs and Windows
CNG RSA. No account, workload, native compiler, production key or service is used.
Refresh affected consumer evidence and verify the immutable publisher sources:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Test-LifetimeSignatures.ps1 -Record -VerifySources
```

Expect four passing consumers. Memory checks measure alignment1/8/16/64/256 and
initialized257-byte buffers across GC; observe collectible owner release without
post-release access; reject invalid size/alignment and raw current type inputs.
XML checks run default/strict/late/invalid processes with original local signed
fixtures at1/8/9/100/101 references, actual SignedInfo/SignedXml parsing and RSA
verification, malformed/tamper/wrong-key rejection and old application validation.
Recorded references/XML archives/selected/executed assets match the audit; compiler,
CoreLib/CoreCLR and Windows crypto binary identities are retained. Any nonzero
exit fails. Process/user/machine PATH and persistent settings must stay unchanged.

This literal block is tested from an unrelated starting directory. Build/pack may
restore online. The exact ordinary package is installed through a fresh local-only
feed/cache, then real installed stdio list/get runs from an unrelated temporary
directory. Unusable retrieval proxies prove proxy independence, not network isolation.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Test-Smoke.ps1 -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -PackageVersion '0.0.0-pin1108-1111'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('interop.allocate-aligned-type-lifetime-memory','crypto.bound-xml-signature-references') `
  -Query 'lifetime-signatures' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin1108-1111' `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory ./artifacts/lifetime-signatures/inspection
```

Expect one passing installed smoke and both discovery IDs, reusable titles and
goals, full canonical Markdown, target11/edition11 applicability and fresh paired
evidence. Structured/text responses agree; target10 returns no_applicable_edition.
Missing/stale proof or changed canonical content fails. No exact native memory
reclamation timing, other-runtime/provider/platform, authorization, Native AOT
collectible-type or final-GA proof is established. Eight foundations, PIN-959/
PIN-962 and other Human gates remain open; no recurring matrix or publication.

## Environment keys and empty services

Requires PowerShell7, Node22.19+, SDK10.0.403/runtime10.0.13 and the separate
SDK11.0.100-rc.2.26504.105/runtime11RC2 installation. ObservatoryVariables uses
Microsoft.AspNetCore.App on both targets; MuseumServices uses it only on .NET10,
with the .NET11 abstraction in the base framework. No NuGet dependencies,
workloads, external provider, account or native toolchain is needed.

Refresh changed evidence and verify eight immutable official sources:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Test-ExtensionContracts.ps1 -Record -VerifySources
```

Expect four passing real consumers: environment loading in isolated child modes,
exact leaf keys/values, prefix/case, underscore runs1–8, collisions and provider
precedence; DI missing/required/keyed services, built-ins/self queries, scopes and
sync/async disposal, closed reference/value enumerables, open generics and AnyKey.
Records retain exact inputs/commands, dependencies, compiler/runtime/loaded
Extensions identities and references matching the Extensions audit. Process/user/
machine PATH and persistent user/machine environments must stay unchanged.

This literal block is exercised from an unrelated directory. Build/pack may
restore online; the exact ordinary package is installed from an isolated local-only
feed/cache. Real installed stdio calls run from a temporary unrelated directory
with unusable proxies, establishing proxy independence rather than network isolation.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Test-Smoke.ps1 -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -PackageVersion '0.0.0-pin1112-1113'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('configuration.transform-environment-keys','di.supply-empty-services') `
  -Query 'extensions-contracts' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin1112-1113' `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory ./artifacts/extensions-contracts/inspection
```

Expect one passing installed smoke, then both discovery IDs/titles/goals, full
canonical Markdown, target11/edition11 applicability and fresh paired evidence.
Structured/text responses agree; target10 returns no_applicable_edition. Missing/
stale proof, changed snippet/body, unintended mapping or nonzero exit fails.
Special connection-string handling, Native AOT value-type enumeration, other-platform
and final-GA execution are not claimed. The eight foundations, PIN-959/PIN-962
and existing Human gates remain open; no recurring native/client/OS matrix added.

## Framework Extensions references and event provider identity

Requires PowerShell7, Node22.19+, SDK10.0.403/runtime10.0.13 and the separate
SDK11.0.100-rc.2.26504.105/runtime11RC2 installation. CelestialSchedule pins nine
Extensions10.0.13 packages on net10; net11 uses only the base framework. Supporting
controls pin Physical11RC2 and Logging.Abstractions11RC3 without upgrading the
baseline. BorealSignals uses framework EventSource only. No workload, account,
external service, native collector or production resource is needed.

Recheck the affected consumers and three immutable publisher sources. Add `-Record`
when changed inputs require fresh saved sample evidence:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Test-ExtensionIdentity.ps1 -VerifySources
```

Expect four passing actual primary consumers, missing-reference compile failures,
exact/minimum NU1510 controls, both multitarget library executions, retained Physical
implementation, RC3 override loading and two real framework-dependent published
consumers. Listener checks verify both new constructors, payload/identity/traits/
settings, disable/dispose, duplicate lifetime, ConstructionException versus thrown
settings errors and write-error policy. Records retain exact commands/inputs,
resolved dependencies, archive hashes, selected/executed assets and compiler/
CoreLib/reference identities. PATH/persistent settings must stay unchanged.

This block is tested from an unrelated directory. It builds/packs the ordinary
tool, installs that exact artifact from an isolated local-only feed/cache and
calls installed stdio list/get from an unrelated temporary directory. Restore may
use network. Unusable retrieval proxies establish proxy independence, not network
isolation.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Test-Smoke.ps1 -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -PackageVersion '0.0.0-pin1114-1116'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('dependencies.use-framework-extensions','diagnostics.choose-event-provider-identity') `
  -Query 'extension-identity' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin1114-1116' `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory ./artifacts/extension-identity/inspection
```

Expect one passing installed smoke and both discovery IDs, reusable titles/goals,
complete canonical Markdown, target11/edition11 applicability and fresh paired
evidence. Structured/text responses agree; target10 returns no_applicable_edition.
Missing/stale proof or changed canonical content fails. No ETW/EventPipe, native/
AOT, self-contained/single-file, other-platform or final-GA execution is claimed.
The eight foundations, PIN-959/PIN-962 and existing Human gates remain open; no
recurring native/client/OS matrix is added.

## Windows HTTP.sys requests and WSL trust guidance

Requires the existing Windows host, SDK10.0.403/runtime10.0.13 and separate
SDK11.0.100-rc.2.26504.105/runtime11 RC2. Owner setup uses the
[controlled HTTPS runbook](windows-web-verification.md); preserve the dedicated
resources until review completes, then have the owner clean only those resources.
Refresh the paired canonical request records with:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Test-WindowsRequests.ps1 `
  -AccessDirectory 'C:\Users\pingu\AppData\Local\Temp\pb-web-pin818-831-1175' -Record
```

Expect old HTTPS success and current Legacy=false/Medium=true/Strict=true endpoint
binding, SNI empty/small false with zero bytesReturned, exact516 success with no
SNI/flags2, and native error87 for the invalid property. These are anonymous HTTPS
checks, not authenticated relay-rejection proof. WSL trust/connectivity remains
pending H_WSL_PROOF/PIN-956; canonical EstuaryHttps projects have compilation only.

This block was checked from unrelated C:\. It builds/packs the ordinary tool,
installs that exact package through isolated local feed/cache, calls real stdio
list/get from an unrelated directory and displays ID/title/goal, full canonical
Markdown, target11/edition11, applicability and current evidence. Setup may use
network; unusable proxies during retrieval prove proxy independence, not OS
network isolation. Node22.19+ and the existing SDK10 installation are required.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('hosting.enforce-channel-binding','hosting.read-native-request-property','web.trust-wsl-development-certificate') `
  -Query 'windows-web' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin818-831-1175' -BuildAndPack `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory './artifacts/windows-inspection'
```

Expect three complete recipes and target10 no_applicable_edition controls. Pending
Human trust remains visible. This does not establish GA or comprehensive release
readiness; missing/stale compilation or canonical/applicability drift fails.
## Group winners and two-sided reconciliation

The paired original MarshSurvey consumers execute deterministic group winners with
related station names and matched/left-only/right-only reconciliation on the
approved isolated SQL Server route. Refresh their exact compilation/execution
records with `eng/Test-MarshSurvey.ps1 -Record`. The runner creates and removes
unique restricted databases and stops the server. It requires the existing SDK
10.0.403 and SDK11.0.100-rc.2.26504.105 installations and locked EF10/11 packages.
This is SQL2025/Windows consumer and RC2 evidence, without a SQLite or GA promise.

The following block is exercised from an unrelated starting directory. It builds
and packs the ordinary tool, installs it through an isolated local feed/cache and
calls real stdio list/get from an unrelated temporary working directory. Setup
may restore online; unusable retrieval proxies prove proxy independence rather
than operating-system network isolation.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('ef.compose-group-winner','ef.reconcile-unmatched-records') `
  -Query 'marsh-survey' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin849-850' -BuildAndPack `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory './artifacts/marsh-survey/inspection'
```

Expect ID/title/goal, complete canonical Markdown, applicability and fresh paired
compilation/execution evidence for both target11/edition11 recipes. Target10 must
return no_applicable_edition. No additional .NET10 foundation is introduced.

## Document indexes and vector reads

The paired consumers need the existing .NET10 SDK10.0.403 and .NET11 RC2 SDK,
locked EF/SqlClient packages, PowerShell7 and Docker Desktop Linux containers.
This runner creates/removes isolated databases and stops the server afterward:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Test-DuneArchive.ps1 -Record
```

Expect JSON index/query, exact distance, default entity projection and explicit
persisted-vector PASS checks. EF11 vector DDL/query generation also passes;
EF11 vector-index application and exact/approximate VectorSearch execution are explicitly
NOT TESTED: local SQL2025 CU10 rejects WITH APPROXIMATE. Pingu authorized this
execution exclusion on 2 November 2026. No GA or measured recall claim is made.

This builds/packs and installs the exact local tool artifact with an isolated
feed/cache and unrelated working directory. Real stdio list/get prints discovery
ID/title/goal, full canonical Markdown, applicability and target/edition11.
Setup restores packages online; retrieval uses unusable proxies, which establishes
proxy independence rather than operating-system network isolation.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('sql.index-json-path','sql.search-vector-index','sql.read-vector-explicitly') `
  -Query 'dune-archive' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin855-856' -BuildAndPack `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory './artifacts/sql-indexes/inspection'
```

Expect three complete entries and exact-artifact PASS; target10 has no applicable
edition. Older samples remain comparisons rather than additional foundations.

## Ranked text and combined vector relevance

Requires PowerShell7, the existing SDK10.0.403 and SDK11 RC3, locked EF/SqlClient
packages and the approved local SQL2025 Docker route. The paired consumers create
restricted disposable databases and remove them in finally, then stop the server.
Full-text requires its installed component and English language support. The
combined exact-distance query uses experimental EF vector APIs (EF9105), without
preview VECTOR_SEARCH/index syntax or a PREVIEW_FEATURES opt-in.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Test-QuarrySearch.ps1 -Record
```

Expect both builds and actual catalog/index, ranked table-function, combined
weighted SQL query and reverse-schema checks to pass. The semantic-only fixture
must remain in the result. These controlled vectors prove query mechanics, not
embedding quality, approximate recall, other engines or EF11 GA.

This example was run from an unrelated directory. It builds/packs the server,
installs the exact package in an isolated feed/cache and invokes real stdio
list/get. Output includes ID/title/goal, full Markdown, applicability and edition.
Unusable proxies establish proxy independence, not OS network isolation.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('sql.manage-ranked-text-search','sql.combine-semantic-and-text-search') `
  -Query 'quarry-search' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin857-858' -BuildAndPack `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory './artifacts/quarry-search/inspection'
```

Expect two complete recipe entries and exact-artifact PASS; target10 returns
no_applicable_edition. Previous samples are comparisons, not extra foundations.

## Ordered SQLite summaries and Cosmos embedded values

Requires PowerShell7.3+, the existing SDK10.0.403/runtime10.0.14 and SDK11 RC3,
locked EF10/11 packages and Docker Desktop Linux for the approved isolated Cosmos
route. No Azure account or cloud credentials are needed. Refresh actual paired
compilation and execution records with:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Test-EmbeddedValues.ps1 -Record
```

Expect ordered summaries on native SQLite3.44+ (including tied positions), and
Cosmos nested object/array round trips, value replacement, collection edits and
independent SDK document inspection. EF11 tracks one root entity; EF10 tracks
owned entries. Only uniquely marked test databases are removed; the empty emulator
is stopped. No cloud parity, performance, other-platform or GA proof is claimed.

This builds and packs the ordinary tool, installs its exact artifact using the
existing isolated local feed/cache and calls real stdio list/get from an unrelated
directory. Node22.19+ and the existing SDK10 installation are required. Setup may
restore online; unusable proxies during retrieval prove proxy independence rather
than operating-system network isolation.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('sqlite.order-concatenated-values','cosmos.map-embedded-values') `
  -Query 'embedded-values' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin861-865' -BuildAndPack `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory './artifacts/embedded-values/inspection'
```

Expect ID/title/goal, full canonical Markdown, applicability and fresh paired
execution evidence for target11/edition11. Target10 returns no_applicable_edition;
previous examples add no .NET10 foundation. Missing/stale evidence or canonical
content drift fails. Existing Human and GA reconciliation gates remain open.

## Cosmos index policy and save scope

Requires the existing pinned .NET10/11 SDKs, PowerShell7 and approved local Docker
Cosmos route. The samples use disposable marked databases and leave no cloud
resources. Run from any directory:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Test-CosmosPolicies.ps1 -Record
& ./eng/Inspect-Guidance.ps1 `
  -RecipeId @('cosmos.configure-index-policy','cosmos.choose-save-atomicity') `
  -TargetVersion '11.0' `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe'
```

Expect real policy read-back and transaction-mode PASS lines, followed by each
recipe's ID, title, goal, complete Markdown and applicability from real stdio.
Automatic mode is named WhenNeeded in the pinned API. Always rejects saves across
partitions/containers; Never permits partial persistence after a conflict.
Policy acceptance proves no vector/full-text query behavior or performance.
This is RC2/local emulator evidence, not GA or cloud parity. The installed-package
CosmosPolicyGuidance scenario separately verifies offline feed/cache retrieval,
complete canonical body, applicability, target edition and older-target refusal.
## Cosmos ingestion, session handoff and document ownership

The paired ReefDocuments consumers require the existing SDK10.0.403 and SDK11 RC3,
locked EF10/11 and Cosmos3.63.1 packages, PowerShell7 and the approved local Docker
Cosmos route. They create/remove uniquely marked databases and stop the empty
emulator. No Azure account, cloud credentials or production resources are used.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Test-ReefDocuments.ps1 -Record
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('cosmos.carry-session-consistency','cosmos.enable-bulk-writes','cosmos.preserve-document-fields','cosmos.read-raw-document') `
  -Query 'reef-documents' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin868-870' -BuildAndPack `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory './artifacts/reef-documents/inspection'
```

Expect bulk persistence/duplicate rejection, owner-scoped actual token handoff
between independent clients, old preservation/new removal of an unmapped field,
raw JSON round trip and stale-ETag rejection. Bulk uses automatic SDK sessions;
manual token work uses a separate non-bulk client. The pinned SDK rejects explicit
session request options with bulk enabled. Docs spell BulkExecutionEnabled;
the pinned RC2 assembly exposes BulkExecutionAllowed, used in the compiled sample.

The inspection block is checked from unrelated C:\. It packs/builds the ordinary
tool, installs the exact local artifact in an isolated feed/cache and retrieves
real stdio list/get from an unrelated temporary directory. It prints ID/title/goal,
complete canonical Markdown, applicability and fresh target11/edition11 evidence.
Target10 has no applicable edition. Unusable retrieval proxies establish proxy
independence, not OS network isolation. No throughput, cloud consistency enforcement,
multi-region or GA claim is made. Existing Human/GA gates remain open.

## Desktop clipboard policy and appearance

Requires PowerShell7.4+, the existing SDK10.0.403 and separate SDK11 RC3,
WindowsDesktop reference/runtime assets and Node22.19+. No workload installation,
clipboard replacement, global settings change, service or account is needed.
The paired samples self-close. Source-reviewed appearance restrictions are distinct
from measured settings/lifecycle checks; visual/accessibility transitions and
external clipboard-owner failures are not executed. No GA proof is claimed.

Run from an unrelated starting directory:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Test-DesktopPolicy.ps1 `
  -Net10Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Net11Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('wpf.restore-clipboard-error-handling','winforms.adopt-modern-rendering') `
  -Query 'desktop-policy' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin890-891' -BuildAndPack `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory './artifacts/desktop-policy/inspection'
```

Expect EMPTY for the older/default WPF reads and ERROR80004005 for the current
opt-in in a separate process. Expect two settings lifecycle passes and current
inheritance/override/PropertyGrid checks. Installed real stdio list/get prints
ID/title/goal, full canonical Markdown and target11/edition11 applicability;
target10 has no applicable edition. The harness uses an isolated local feed/cache
and unrelated working directory; unusable proxies prove proxy independence, not
OS network isolation. Existing Human/GA gates remain open.

## Cosmos identity and absent projections

Use the approved disposable local emulator with the existing SDK10.0.403 and
SDK11 RC3 below. This checks real EF10/11 identity and projection behavior;
RC evidence does not establish GA or Azure service parity.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
$metadata = Invoke-RestMethod 'https://builds.dotnet.microsoft.com/dotnet/release-metadata/11.0/releases.json'
$asset = ($metadata.releases | Where-Object 'release-version' -eq '11.0.0-rc.3').sdk.files |
  Where-Object { $_.rid -eq 'win-x64' -and $_.url.EndsWith('.zip') }
$sdk = Join-Path (Get-Location) 'artifacts/sdk11rc3'
New-Item -ItemType Directory -Path $sdk -Force | Out-Null
$archive = Join-Path $sdk 'sdk.zip'
Invoke-WebRequest $asset.url -OutFile $archive
if ((Get-FileHash $archive -Algorithm SHA512).Hash -ine $asset.hash) { throw 'SDK checksum mismatch' }
Expand-Archive -LiteralPath $archive -DestinationPath $sdk -Force
& ./eng/Test-ReefIdentity.ps1 -Record
```

Build and pack the real server, install through the existing isolated-feed/cache
harness and retrieve full Markdown, applicability and target11/edition11 over stdio:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('cosmos.retain-document-key-identity','cosmos.handle-missing-projection') `
  -Query 'reef-identity' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin871-872' -BuildAndPack `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory './artifacts/reef-identity/inspection'
```

Expect two complete entries and exact-artifact PASS. The provider runner records the RC undefined-projection mismatch without treating it as GA proof; retest the documented exception contract against real GA artifacts as approved by Pingu on 3 November 2026. Target10 has no applicable
edition; the older consumers remain comparisons rather than extra foundations.

## Observatory numeric results and Windows composite signatures

Requires PowerShell7, SDK 10.0.403/runtime10.0.14, SDK 11 RC3 installed alongside SDK 10 in the same dotnet host,
Node.js and public access for exact RC1 runtime/source downloads. No host install
or PATH integration occurs. The sample runner verifies source hashes, builds and
runs both consumers, and records compiler/reference/runtime/native identity.
It tests one agreed 44/P256 composite set, actual older RSA success and current
rejection, plus numerical drift and reversible disposable persistence.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Test-ObservatoryContracts.ps1
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('numerics.compare-ieee-decimal-results','crypto.select-supported-composite-signature') `
  -Query '' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin1038-1058' -BuildAndPack `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory './artifacts/observatory/inspection'
```

The exact ordinary package is installed through a private local feed/cache and
called over real stdio from an unrelated directory. Expect both discovery
ID/title/goal entries, full canonical Markdown, target11/edition11, applicability
and fresh evidence; target10 returns no_applicable_edition. Older samples are
comparisons, not new foundations. Unusable proxies show proxy independence,
not OS network isolation. GA, other Windows providers and other composite sets
are not claimed.

## Network monitoring and supported threading paths

Requires PowerShell7, Node24, the pinned .NET10/.NET11 SDK/runtime installations,
the existing Docker Desktop Linux engine, and the authoring Chromium/Playwright
paths declared in eng/Record-SupportPaths.ps1. The pinned local SDK10.0.403 Docker
image is required; the driver uses --pull never. Browser proof uses interpreted Mono;
no wasm-tools installation or persistent environment change is required.

Refresh affected evidence, then inspect the ordinary installed artifact from any
starting directory:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Record-SupportPaths.ps1 -Record
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('networking.guard-change-monitoring','runtime.select-threading-path') `
  -Query 'support-paths' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin1092-1093' -BuildAndPack `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory './artifacts/support-paths/inspection'
```

Expect the real missing-image startup cleanup regression to pass, then
score48/cancellation on both desktop and single-thread browsers, real Linux
address events and no callback for the separate post-disposal change, plus browser
manual-refresh fallback. The RC2 browser NetworkChange.IsSupported getter throws
without an explicit browser/WASI exclusion; the delivered route keeps that guard.
The cleanup control intentionally prints Docker's No such image error, followed by
a cleanup PASS; it performs no pull. Docker resources are removed. The installed
inspector prints ID/title/goal,
complete canonical Markdown, explicit target11/edition11, applicability and fresh
evidence, then exact-artifact PASS. Target10 returns no_applicable_edition.
No worker-count/speedup, in-flight browser cancellation, WASI, threaded-browser,
other-browser/platform or final-GA proof is claimed. PIN-959 and PIN-962 remain open.

## Empty owned lists and numeric materialization

From PowerShell7 with Docker Desktop running, SDK10.0.403 and the existing isolated SDK11 RC3 installation at `C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe`, run (or pass `-Net11Dotnet` with its actual absolute path):

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Test-ReefMaterialization.ps1 -Record
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('cosmos.treat-empty-owned-sequence','cosmos.preserve-rounding-intent') `
  -Query 'reef-materialization' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin873-874' -BuildAndPack `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory './artifacts/reef-materialization/inspection'
```

Expect real old/new consumer PASS: EF10's uninitialized empty owned list is null;
EF11 returns an empty list, while optional references retain absence. For 1.75,
EF10 int returns2 and EF11 int returns1; long returns1 and decimal1.75 in both.
Explicit rounding returns2; negative fixtures verify the corresponding signed
results. Midpoints, overflow and other numeric types are not covered.

The Inspector installs the exact package from an isolated feed/cache and calls
real stdio list/get from an unrelated directory. Expect two discovery entries
with ID/title/goal, complete canonical Markdown and applicability, target11 and
edition11; target10 has no applicable edition. Proxy independence is not OS
network isolation. This is local emulator/RC2 evidence, not cloud or GA proof.

## Cosmos I/O, discriminator names and migration histories

From PowerShell7 with the approved Docker Cosmos route and the existing SDK10
and isolated SDK11 RC3 installations:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Test-HarbourQueries.ps1 -Record
& ./eng/Test-BeaconMigrations.ps1 -Record
& ./eng/Inspect-Guidance.ps1 -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -RecipeId @('cosmos.use-async-io', 'cosmos.reference-model-discriminator', 'ef.resolve-migration-branch-divergence') `
  -TargetVersion '11.0'
```

Expect actual EF10/11 async query/save, sync-path controls, model/storage-name
checks and competing migration re-scaffolding checks. Inspector builds the real
server and displays each ID, title, goal, full Markdown and applicability. The
CosmosPolicyGuidance installed-package scenario additionally checks all three
through real stdio from an unrelated working directory and isolated feed/cache,
including canonical bodies and absence from the curated target10 catalogue.
Local execution uses exact RC2 artifacts, not GA or cloud Cosmos evidence.

## COM property contracts and custom browser hosts

These edition11 recipes compare actual .NET10 and RC2 .NET11 dependencies.
COM checks use MSVC Windows x64 and a real native IUnknown consumer. Browser
checks compile against the shipped TypeScript declarations and execute Mono10,
Mono11 and opt-in CoreCLR11 in Chromium, including real diagnostic provider traces.
No other ABI/browser/platform/AOT or final-GA verification is implied.

The setup prepares missing RC2 wasm-tools packs under ignored artifacts/native-dotnet;
it reuses the existing exact RC2 SDK without changing PATH or global tools. It
requires the existing .NET10 wasm-tools installation, MSVC, Node24 and the
Playwright/Chromium paths declared in eng/Test-LanternHosts.mjs. Run the following
from any directory; it was exercised from unrelated C:\.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Test-Smoke.ps1 -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' -PackageVersion '0.0.0-pin1099-1100'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('interop.generate-com-properties','wasm.migrate-custom-runtime-hosts') `
  -Query 'host-contracts' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin1099-1100' `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory './artifacts/host-contracts/inspection'
```

The saved native evidence covers COM success, rejected marshalling/mixed-accessor
cases and separate preview-safety controls. Three browser routes cover return17,
configured report, keep-alive/exit checks, failed-resource handling and selected-provider
traces. Expect Inspector to print both discovery IDs/titles/goals and full expanded
Markdown/applicability. The isolated local-feed/cache installation consumes the
exact ordinary package, with real stdio calls from an unrelated directory and
unusable proxies during retrieval. This is proxy-independent offline operation,
not OS network isolation. Both complete bodies and fresh evidence must match;
target10 returns no_applicable_edition. Saved responses are disposable artifacts.
PIN-959/PIN-962, Human gates and the eight .NET10 foundations remain unchanged.

## Windows Forms control rendering, startup and update scopes

Use the existing Windows Desktop runtimes, SDK10.0.403 and the separate SDK11 RC3,
PowerShell7.4+ and Node.js. No settings changes or account setup are needed for
the automatic samples. The settings check below is a separate owner action.
From an unrelated directory, select this checkout explicitly:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Test-FenDesktop.ps1 -Record `
  -Net10Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Net11Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('winforms.author-adaptive-control-rendering','winforms.control-initial-form-reveal','winforms.batch-control-mutations') `
  -Query 'fen-desktop' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin892-894' -BuildAndPack `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory './artifacts/fen-desktop/inspection'
```

Expect eight successful paired sample runs: system-color painting/current
renderer selection, C#/VB startup and exception-safe update resumption. The
actual installed package is called over stdio from an unrelated directory with
an isolated feed/cache. It prints ID/title/goal, complete canonical Markdown,
target11/edition11 applicability and current evidence. Target10 has no applicable
edition; previous examples add no foundations. Unusable retrieval proxies test
proxy independence, not OS network isolation. No GA, visual timing, flicker or
performance claim follows from these checks.

For PIN-897, Pingu runs the current FenStatus DLL with `--manual`, keeps it open
while applying a Windows Contrast theme, checks readable text and disappearance
of the accent strip, restores the exact original theme and checks recovery.
The console records `SETTINGS` categories and `STATE` values. The 3 November 2026 owner result is retained in
samples/net11.0/FenStatus/owner-settings.evidence.json: actual settings notifications,
Net11 -> Classic -> Net11 and restoration, with owner-reported visual success.
Refresh that bounded evidence after relevant sample or toolchain changes. Use the
[Windows Contrast themes instructions](https://support.microsoft.com/en-US/accessibility/windows/change-color-contrast-in-windows).

## Combined migrations, offline removal and omitted constraints

Use PowerShell7, existing SDK10.0.403 and isolated SDK11 RC3, Node.js and the
approved local SQL Server Docker route. The check creates and removes only unique
owned databases. It does not publish a package or deploy a consumer. From an
unrelated directory:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Test-QuarryMigrations.ps1 -Record
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('ef.scaffold-and-apply-migration','ef.remove-unapplied-migration-offline','ef.omit-database-foreign-key') `
  -Query 'quarry-migrations' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin878-880' -BuildAndPack `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory './artifacts/quarry-migrations/inspection'
```

Expect paired compilation and SQL schema/history/model/index PASS, EF11 combined
scaffold/apply with retained source files, unreachable offline removal and rejected
offline/force. Explicit connected removal fails at the unreachable selected server
without deleting the migration. An orphan insert succeeds after constraint omission.
The inspector installs the exact ordinary package through its isolated local feed
and cache, calls real stdio list/get from an unrelated directory, and prints each
ID/title/goal, complete canonical Markdown, applicability and fresh evidence.
Target11 selects edition11; target10 has no applicable edition. Proxy independence
is not OS network isolation. These are actual RC2 SQL/CLI checks, not PMC or GA
proof; PIN-957/PIN-965 and existing Human gates remain open.

## Provider-backed keys and mobile HTTP sends

Requires PowerShell 7, the existing pinned .NET10 SDK/runtime and isolated .NET11
RC2 installation, Node24 and Docker Desktop's Linux engine with the SDK10.0.403
image pinned by eng/Test-ProviderRequests.ps1 already available locally. The runner
fetches immutable official sources and the exact RC2 Linux runtime archive,
verifying its pinned Microsoft SHA512 before extraction. It
uses network-none disposable containers and removes their generated private keys.
No HSM/FIPS provider or native mobile toolchain is assumed.

From any starting directory:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Test-ProviderRequests.ps1 -Record
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('crypto.configure-provider-backed-keys','http.replace-mobile-synchronous-sends') `
  -Query 'provider-requests' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin1101-1106' -BuildAndPack `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory './artifacts/provider-keys/inspection'
```

Expect paired OpenSSL sign/verify/tamper, missing provider/key, ownership/disposal,
.NET11 ordered lists/property-query success/rejection, Windows CNG property-copy
checks and real desktop HTTP success/status/transport/cancellation checks to pass.
The synthetic Android call-site emits CA1416 only on .NET11; this is not a mobile
target build. Actual Android/iOS/tvOS native builds, handlers, device execution
and UI responsiveness remain explicitly not-tested under PIN-699.

Inspector builds/packs the ordinary tool, installs that exact package through an
isolated local feed/cache and calls real stdio list/get from an unrelated directory.
It prints discovery ID/title/goal, complete canonical Markdown, applicability and
fresh evidence, selecting target11/edition11. Target10 has no applicable edition.
Unusable proxies verify proxy independence, not OS network isolation. These are
actual RC2/default/base and Windows desktop observations, not HSM/FIPS/mobile/GA
verification. PIN-959 and PIN-962 remain open; the eight .NET10 foundations stay.

## Apple navigation and custom item handlers

PIN-907 extends the existing navigation edition; PIN-908 adds item-handler
migration. Requires PowerShell7.4+, SDK10.0.403 and Node22.19+. The existing
isolated SDK11 root lacks the MAUI SDK pack; Apple requires unavailable Xcode.
Older Windows compilation passes; all new native interactions remain not-tested
under PIN-699. Source/artifact review is distinct from native execution.

From any directory, build and inspect the exact installed package:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Test-JettyHandlers.ps1
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('maui.migrate-navigation-handlers','maui.migrate-custom-items-handlers') `
  -Query 'jetty-handlers' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin907-908' -BuildAndPack `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory './artifacts/jetty-authoring/inspection'
```

Expect ID/title/goal, complete canonical Markdown and applicability, target11 and
edition11. Target10 has no applicable edition. The existing harness installs from
an isolated feed/cache, retrieves both through real stdio from an unrelated working
directory and checks complete bodies with unusable retrieval proxies. That proves
proxy independence, not OS network isolation. No native behavior or GA claim follows.
## EF design dependencies, multiple contexts and clean SQL

PIN-884/PIN-885/PIN-886 add FleetDesign and FleetTooling EF10/EF11 comparisons.
Real checks reproduce the removed transitive Design dependency, restore service
registration, execute scripts, verify both updated SQLite schemas and drop both
scratch databases. EF11 RC2 wildcard list fails with a literal-context lookup;
Pingu authorized publication with that explicit gap on 4 November 2026. Use
explicit names for list until a fixed official artifact is verified. Wildcard
script combines both models; deploy separate per-context scripts to separate databases.

From an unrelated directory in PowerShell7, with the pinned SDK and Node22.19+:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Test-FleetDesign.ps1
& ./eng/Test-FleetTooling.ps1
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('ef.reference-design-services','ef.operate-across-contexts','ef.capture-clean-migration-script') `
  -Query 'fleet-tooling' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin884-886' -BuildAndPack `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory './artifacts/fleet/inspection'
```

Expect all three ID/title/goal summaries, full expanded Markdown, applicability
and edition11. Target10 returns no_applicable_edition. The harness installs the
actual package using isolated feed/cache and calls real stdio list/get from an
unrelated directory with unusable proxies. This proves offline guidance retrieval
under those conditions, not OS network isolation. No package is published.
Windows x64 RC2 execution does not establish GA or other provider/platform proof;
successful wildcard-list proof remains pending despite the publication exception.

## EF generation stages, split-read recovery and tenant history

PIN-887–889 add original FernGeneration, BrackenReads and MossArchive comparisons.
The stage selectors are officially documented and experimental. Pingu approved
PR delivery with generation proof pending on 4 November 2026. Ordinary compilation
disables generation; failed RC2/RC3 generation is not GA or NativeAOT proof.
Real SQL2025 checks reproduce split-query concurrency and whole-read recovery;
real EF9/10 SQLite checks retain tenant filtering when history includes archived rows.

From any directory, select this worktree. PowerShell7, SDK10.0.403 and Node22.19+
are required. Setup restores/builds locally; no package is published.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('ef.configure-generated-model-stage','ef.recover-split-query-consistency') `
  -Query '' -TargetVersion 11.0 -UnsupportedTargetVersion 10.0 `
  -PackageVersion 0.0.0-pin887-889 -BuildAndPack `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory ./artifacts/pin887-889/inspection11
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('ef.include-archived-with-tenant-filter') -RecipeEdition 10.0 `
  -Query moss-archive -TargetVersion 10.0 -UnsupportedTargetVersion 11.0 `
  -PackageVersion 0.0.0-pin887-889 `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory ./artifacts/pin887-889/inspection10
```

Expect ID/title/goal, full canonical Markdown, matching applicability, current
source-compilation evidence and the correct editions. Generated-stage detail
retains experimental status and pending generation proof. The existing harness
installs the exact package from an isolated local feed/cache, invokes real stdio
list/get outside the checkout and compares complete bodies and JSON/structured
results. Unusable proxies establish proxy independence, not OS network isolation.
The target10 foundation currently has no target11 route; EF9 remains comparison-only.

## Known tracked states, shared pooled options and unread scalars

PIN-1189/PIN-1191/PIN-1192 add original WrenTracking EF10/EF11 comparisons.
Both pinned projects execute real SQLite states/scans/persistence, pooled lease
reuse with explicit application cleanup, and partial SELECT/scalar UPDATE omission
with re-enable/default/loaded controls. RC2 evidence does not establish GA or
other providers/platforms, arbitrary request state or a vector fluent loader.

From an unrelated directory, in PowerShell7 with SDK10.0.403 and Node22.19+:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Test-WrenTracking.ps1
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('ef.select-known-tracked-states','ef.reuse-pooled-factory-configuration','ef.preserve-unloaded-scalar-values') `
  -Query 'wren-tracking' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin1189-1191-1192' -BuildAndPack `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory './artifacts/wren/inspection'
```

Expect three ID/title/goal results, complete canonical Markdown, applicability and
fresh sample evidence at edition11. Target10 returns no_applicable_edition.
The harness installs the exact ordinary local artifact in an isolated feed/cache,
then calls real stdio list/get from outside the checkout with unusable proxies.
Setup can restore packages; retrieval is embedded and offline. No package is
published. This establishes proxy independence, not OS network isolation.

## Transaction switches, collection SQL modes and parameterized raw SQL

PIN-1193–1195 add original AlderQueries EF10/EF11 comparisons. Requires
PowerShell7, pinned SDK10.0.403 and isolated SDK11 RC3, Node22.19+ and the approved
local SQL2025 Docker route. The runner creates/removes only unique owned databases,
uses restricted child credentials and in-memory bundled SQLite, and does not
publish packages or change persistent environment settings. From an unrelated directory:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/SqlServer.ps1 -Action Start
& ./eng/Test-AlderQueries.ps1 -Record
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('ef.replace-transaction-boolean','ef.migrate-collection-parameter-mode','ef.retain-parameterized-raw-sql') `
  -Query 'alder-queries' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin1193-1195' -BuildAndPack `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory './artifacts/alder/inspection'
```

Expect real rollback/persisted-row and single-command transaction checks,
collection SQL/parameter/result checks, native SQLite parameter/metacharacter and
already-canceled query/write controls, and actual compiler diagnostics. Each
collection mode has separate internal services to avoid compiled-query cache reuse.
The Inspector installs the exact ordinary package from an isolated feed/cache,
calls real stdio list/get from outside the checkout, and prints ID/title/goal,
complete canonical Markdown, target11/edition11, applicability and fresh evidence.
Target10 returns no_applicable_edition. Unusable retrieval proxies prove embedded
offline retrieval, not OS network isolation. No active-query cancellation, query
plan/performance, Cosmos, arbitrary provider/platform or actual GA proof follows;
PIN-959/PIN-965 and the eight foundations remain unchanged.

## Provider configuration and Cosmos identity

PIN-1196–1198 compare original EstuarySql/EstuaryParcels EF10/EF11 consumers.
PowerShell7, both pinned SDKs, Node22.19+, NuGet access and approved Docker
routes are required. Execute the focused database checks with
`eng/Test-EstuaryProviders.ps1 -Record`; owned unique databases are removed.
Leave shared SQL running while other authors use it; stop the empty Cosmos
emulator with `eng/Cosmos.ps1 -Action Stop` when finished.

From an unrelated directory, select this exact worktree and inspect the ordinary
packed/installed tool through real stdio. Nothing is published:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('sql.migrate-azure-provider-configuration','cosmos.migrate-partition-metadata','cosmos.remove-write-content-option') `
  -Query 'estuary-providers' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin1196-1198' -BuildAndPack `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory './artifacts/estuary/inspection'
```

Expect all three IDs/titles/goals, complete canonical Markdown, edition11,
applicability and fresh compilation/execution evidence; target10 has no applicable
edition. Local SQL2025 configuration/query proof excludes Azure service behaviour.
Cosmos proof is the approved legacy emulator, not cloud parity. Unusable retrieval
proxies prove proxy independence rather than OS network isolation. GA and
comprehensive-readiness gates remain outstanding.

## Query roots, missing principals and constructor bindings

From an unrelated directory, select this worktree and inspect the exact ordinary
packed tool. Requires PowerShell7, SDK10.0.403 and Node22.19+. Setup restores
packages online; the isolated installed server returns bundled guidance offline.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Test-LarkContracts.ps1
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('ef.migrate-structural-materializer','ef.query-unconstrained-relationship','ef.migrate-constructor-binding') `
  -Query 'lark-contracts' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin1202-1204' -BuildAndPack `
  -Dotnet 'V:\_worktrees\Playbook\t3-a392df65\artifacts\sdk-upgrade\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory './artifacts/lark/inspection'
```

Expect ID/title/goal, complete expanded canonical Markdown, edition11,
applicability and fresh evidence for all three recipes. Target10 has no applicable
edition. The same isolated local feed/cache harness installs that exact package
and calls real stdio list/get from outside the checkout with unusable proxies.
This establishes offline content availability, not OS network isolation.
SQLite and disposable SQL2025 execution are separate from MCP retrieval. Root
construction is a contract check; no custom query-translator execution, GA,
NativeAOT or other-provider/platform proof is claimed. Data owners must handle
missing principals; GetBindings does not accept an arbitrary constructor choice.
``Test-LarkContracts.ps1 -Record`` refreshes the committed sample receipts.

## Structural metadata, ignored types and file design commands

From an unrelated directory, select this worktree. Requires PowerShell7,
SDK10.0.403, SDK11 RC3 in the same dotnet installation and Node22.19+. Setup restores NuGet packages;
the tool then serves complete guidance offline. No package is published.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a392df65'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('ef.inspect-structural-model-mappings','ef.migrate-type-ignore-convention','ef.target-file-based-app') `
  -Query 'curlew' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin1205-1207' -BuildAndPack `
  -Dotnet 'V:\_worktrees\Playbook\t3-a392df65\artifacts\sdk-upgrade\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory './artifacts/curlew/inspection'
```

Expect real SQLite metadata/lookup and callback controls, generated migrations,
snapshot checks and persisted rows for conventional, file-target and mixed
project/startup-file routes. The last command installs that exact ordinary artifact
through an isolated local feed/cache, calls real stdio list/get outside the checkout,
and displays ID/title/goal, full canonical Markdown, edition11, applicability and
fresh evidence. Target10 has no applicable edition. Unusable retrieval proxies prove
embedded offline guidance, not OS network isolation. No native JSON-index,
file-to-file project reference, NativeAOT, other-platform or actual GA proof follows.
The separate mixed startup route was approved by Pingu; PIN-959/PIN-965 remain open.

Use eng/Test-CurlewModels.ps1 -Record and eng/Test-CurlewDesign.ps1 -Record to refresh the committed sample receipts after relevant input changes.

## Collection predicates, numeric parsing and Half

Build/pack and inspect the exact installed artifact. Setup restores packages and
installs pinned Inspector locally. Retrieval uses an isolated feed/cache, an
unrelated directory and unusable proxies; it is proxy-independent, not OS network
isolation. Expect all three IDs/titles/goals, full canonical Markdown,
applicability and fresh evidence at target/edition11. Target10 has no edition.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-8bad76cb'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('ef.translate-collection-existence','sql.translate-numeric-text-parse','sqlite.persist-half-values') `
  -Query 'osprey-readings' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin1208-1209-1211' -BuildAndPack `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory './artifacts/osprey/inspection'
```

Run `eng/Test-OspreyReadings.ps1 -Record` for real isolated SQL2025 and Windows
x64 SQLite execution. Exact RC package evidence is not actual GA, other-platform,
JSON Half mapping or a performance claim. No public package is published.

## Tuple projections, missing joins and CPM bundles

From an unrelated directory, the following selects this worktree explicitly,
builds/packs the ordinary tool, then installs that exact package through an isolated
local feed/cache and exercises real offline stdio discovery and retrieval:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-c2fd1474'
$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
& 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' pack ./src/PinguApps.Playbook -c Release -p:PackageVersion=0.0.0-pin1212-1214 --output ./artifacts
& ./eng/Inspect-PloverGuidance.ps1 -PackageVersion '0.0.0-pin1212-1214'
```

Expect IDs/titles/goals and complete canonical Markdown, edition11.0,
applicability and evidence for `ef.compose-tuple-projections`,
`ef.materialize-missing-projection` and `ef.bundle-centrally-managed-migrations`.
The inspector compares the retrieved body with compiled canonical snippets.
PIN-1213 explicitly teaches official intended behavior with final execution deferred:
actual RC2/RC3 materialization failures and the working marker fallback remain visible.
The installed-tool check establishes delivery, not successful direct materialization.
Run `eng/Test-PloverBundles.ps1 -Record` separately for actual generated-project,
CPM/non-CPM, repeated migration and SQLite persistence checks. No production resources,
additional native platforms or GA behavior are established.

## Android media recovery and system-bar contrast

From an unrelated directory, select this worktree explicitly. Setup restores
packages and installs the exact tool through an isolated local feed/cache.
The existing harness calls real stdio list/get outside the checkout with unusable
proxies and compares complete canonical Markdown and applicability.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-4d29ea06'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('maui.recover-interrupted-media-picking','maui.save-captured-media-to-gallery','maui.choose-status-bar-appearance') `
  -Query 'reef-survey' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin917-918' -BuildAndPack `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory './artifacts/reef/inspection'
```

Expect each ID/title/goal, complete expanded Markdown, edition11, applicability
and current evidence. Target10 has no applicable edition. Proxy independence
is not OS network isolation. Older Android compilation uses system SDK10.0.402
with RunAOTCompilation=false; no AOT success is claimed. Current SDK11 MAUI
compilation and all native device behavior are not-tested under PIN-699.
No gallery write, permission approval, process recreation or visual contrast is
proved by MCP retrieval. No new native matrix, Human gate or GA claim is added.
