# Mobile composition and stability: PIN-695

Reviewed **25 October 2026**, for the **.NET 11 RC2 preparation baseline**.
This resolves inventory findings `mobile.resolve-artifact-and-status-baselines`
and `mobile.resolve-overview-lag`. It supplies authoring inputs, not native
compilation, installation, runtime evidence or GA certification.

## Artifact authority and the rebuild

The [source/artifact manifest](mobile-baselines.sources.json) records 16 official
source snapshots and nine downloaded NuGet packages by SHA-256. Source files use
immutable commits. The two GitHub release bodies and known-issues wiki are mutable
snapshots: replay deliberately fails if their reviewed bytes change. Release-body
hashes cover the UTF-8 `body` field, excluding API counters and other mutable metadata.

[Inspect-MobileBaselines.ps1](../../../eng/Inspect-MobileBaselines.ps1) checks
package identity, repository commit, the **complete 18-entry workload-set maps**,
all six mobile manifests and 20 selected SDK/reference/library/template/runtime
pack versions. It also inspects the AI assembly's actual experimental attribute
and all six shipped assembly asset paths, without loading or executing the assembly.
Source IDs below refer to the manifest; its 21 assertions retain the named conflicts
and opt-ins. Hashes establish the exact inspected inputs, not native functionality.

| Evidence | Reconciliation |
| --- | --- |
| `rc2-notes` HTML comment and `maui-build-dependencies` at MAUI commit `d32b79928709cf53b671336dabd0caef5a679fc2` name Apple **27.0.12211-net11-rc.2** | These are upstream source/build dependency records. They do not override the downloaded workload set. The comment's macios range ends at `35d8bf5a42cd71edf4e2700b1e45f6c4b65f3fbe`. |
| [Published Apple RC2 release](https://github.com/dotnet/macios/releases/tag/dotnet-11.0.1xx-rc2-12212), `apple-release` | Names **27.0.12212-net11-rc.2** for iOS, tvOS, Mac Catalyst and macOS and workload set **11.0.100-rc.2.26509.2**. All four downloaded manifests agree; their nuspec provenance is `a75b11fe9cdbd66a6ddb8bfea120d006c1ec1e8d`. |
| [MAUI RC2 rebuild release](https://github.com/dotnet/maui/releases/tag/11.0.100-rc.2.26513.2), `maui-release` | Names workload set **11.0.100-rc.2.26513.2**, not a MAUI library version of that number. Its set package has a new provenance commit but an **identical complete manifest map** to 26509.2. It still selects MAUI **11.0.0-rc.2.26505.5**, Android **37.2.0-rc.2.84** and Apple **12212**. |

No selected entry in either set names 12211. The exact public NuGet URL for
`Microsoft.NET.Sdk.iOS.Manifest-11.0.100-rc.2/27.0.12211-net11-rc.2` returned 404
during discovery; that single failed URL does not establish absence on every feed
or invalidate the source history. Neither set justifies substituting a 12211 pack
or mixing arbitrary newer manifests. [PIN-694](mobile-api-gaps.md) independently
inspected the four actual 12212 reference packages; its API conclusions remain separate.

## Exact preparation sets

Both rows below are published **RC2 preparation choices**, with the same native
composition. The exact SDK pin is **11.0.100-rc.2.26504.105**, recorded in the MAUI
rebuild's `global.json` (`maui-sdk`); it is a reproducibility baseline, not proof
that every later SDK/workload combination is supported. The manifest band is
**11.0.100-rc.2**. CLI workload-set versions and NuGet package versions have
different numeric layouts; do not pass a NuGet package version to `--version`.

| Preparation choice | CLI workload-set version | `Microsoft.NET.Workloads.11.0.100-rc.2` package version | Package repository commit (`dotnet/workload-versions`) |
| --- | --- | --- | --- |
| Apple RC2 release set | `11.0.100-rc.2.26509.2` | `11.100.0-rc.2.26509.2` | `f15e4a7d836bbc5245fd2dd98635fd64ab27588a` |
| MAUI RC2 rebuild set | `11.0.100-rc.2.26513.2` | `11.100.0-rc.2.26513.2` | `b87f5469a4cba33c3f57f7a58e76e984ec0e57f5` |

| Selected manifest / relevant packs | Exact version and source |
| --- | --- |
| `Microsoft.NET.Sdk.Maui.Manifest-11.0.100-rc.2`; MAUI net11 SDK/templates, Controls, Essentials, Graphics, WebView library packs | `11.0.0-rc.2.26505.5`; `dotnet/maui` **d32b79928709cf53b671336dabd0caef5a679fc2**. The manifest also selects net10 SDK/templates **10.0.110**; do not relabel these as net11 packs. Maps is an explicit package choice, not a pack in this workload manifest. |
| `Microsoft.NET.Sdk.Android.Manifest-11.0.100-rc.2`; net11 SDK, API 37.2 reference and CoreCLR runtime | `37.2.0-rc.2.84`; `dotnet/android` **801f9ecbab5118b13d21cb52409e507ad4b2feb7**. The net10 SDK entry is separately **36.1.118**. The actual CoreCLR pack name contains `37.android-arm64`, not `37.2.android-arm64`. |
| `Microsoft.NET.Sdk.{iOS,tvOS,MacCatalyst,macOS}.Manifest-11.0.100-rc.2`; each net11 SDK/reference pack | `27.0.12212-net11-rc.2`; `dotnet/macios` **a75b11fe9cdbd66a6ddb8bfea120d006c1ec1e8d**. These base workloads are distinct from MAUI's supported platform list. |
| Emscripten Current/net6–net10 and Mono.ToolChain Current/net6–net10 (12 other set entries) | `11.0.100-rc.2.26504.105/11.0.100-rc.2` in both sets. Pinning just the six mobile entries would miss part of the composition; replay checks all 18. |

Use platform TFMs such as **net11.0-android37.2**, **net11.0-ios27.0**,
**net11.0-maccatalyst27.0**, **net11.0-tvos27.0** or **net11.0-macos27.0** for the
corresponding binding level; `net11.0` alone cannot describe these requirements.
MAUI's Apple app platforms here are iOS/Mac Catalyst, not an implied MAUI macOS/tvOS
backend. Target binding level, minimum deployment OS and authoring host OS are
different requirements; each content slice must record the actual effective
deployment floor and member availability rather than copying the TFM suffix.

- **Apple authoring:** the selected 12212 release requires **Xcode 27.0 on macOS
  26.6+**. Retain that exact pairing. The MAUI release table's `>= 27.0` wording
  does not establish compatibility with arbitrary later Xcode versions. The same
  Apple installation paragraph still says “sixth preview”; it is stale prose,
  not a supported Preview 6 alternative. Scene lifecycle migration remains required
  for iOS/Mac Catalyst apps built with the Xcode 27 SDKs (`rc2-notes`).
- **Android preparation:** the MAUI release recommends **JDK 21.0.9** and lists
  `build-tools;37.0.0`, `cmdline-tools;21.0`, `platforms;android-37.0` and
  platform-tools **37.0.1**. Canonical RC2 notes explicitly support JDK **21 or 25**
  (minimum 21); Gradle with JDK 25 requires **Gradle 9.1+**. API **37.2** is the
  binding/TFM level in the actual reference pack. The release's “API-37.0” tools
  headline does not replace that binding level. Record the selected tools/packages
  and any further API-specific Android SDK requirements in the actual consumer
  check; this audit did not install or resolve an Android SDK.
- The Playbook tool stays workload-free. This audit changes no project, package
  reference, SDK pin, installed workload, shared build/CI file or Renovate permission.
  It does not define an approved older/current native **build** matrix.
  [PIN-698](https://linear.app/pinguapps/issue/PIN-698) retains Mac access and matched
  older/current toolchains; native content slices retain actual build/publish proof.

## Stability and cumulative overview conflicts

Release stage and feature status are separate. All preparation artifacts above
are prerelease RC2 evidence. A publicly shipped API, a default-on feature or a
recipe targeting GA does not establish GA verification or remove an experimental label.

| Capability / source | Status and instructions to carry into discovery and detail |
| --- | --- |
| SourceGen XAML C# expressions (`rc2-notes`) and `x:Code` (`preview4-notes`, `overview`) | **Preview authoring.** Require `MauiXamlInflator=SourceGen` and `EnablePreviewFeatures=true`. Do not infer that ordinary SourceGen XAML requires the preview opt-in: `xaml-targets` selects SourceGen by default. |
| Incremental XAML Hot Reload (`rc2-notes`, `xaml-targets`) | **Preview in .NET 11.** Enabled by default for Debug, disabled by default for Release/publish; `EnableMauiIncrementalHotReload=false` opts out. A default is not a stability guarantee. CLI/IDE/device execution remains content work. |
| Apple NativeAOT (`apple-nativeaot` at the actual 12212 commit) | **Experimental publishing**, enabled by `PublishAot=true`, with native linking/trimming/registrar limitations. Its document still introduces the capability as a .NET 8 experiment; neither that historical wording nor the overview's unqualified “opt-in alternative” establishes stabilization for these RC2 assets. Preserve the conservative experimental disclosure pending explicit later evidence. |
| Android NativeAOT (`overview`) | **Experimental, opt-in publishing**, distinct from the default CoreCLR route. Do not transfer generic desktop NativeAOT stability to Android or infer native behavior from available runtime packs. |
| `Microsoft.Maui.Essentials.AI` (`preview5-notes`, `ai-overview`, `ai-assembly`, actual assembly) | **Experimental MAUIAI0001.** Preview 5's “stable for the .NET 11 release branch” sentence retains the diagnostic. The inspected package has an **assembly-level ExperimentalAttribute("MAUIAI0001")**. A local pragma or appended `NoWarn` suppresses the diagnostic; it does not make the contract stable. |
| Apple Shell handler (`rc2-notes`) | **Opt-in**, `UseiOSShellHandler`; the compatibility ShellRenderer remains default. Do not turn every opt-in into an experimental diagnostic or extrapolate Android's default switch to Apple. No blanket “stable” classification is made for unaudited APIs. |

The cumulative overview is pinned at docs-maui
**e0fae4d0def74961bfe9633f96ca3420b18c4364** (`overview`), dated 10 September.
It still names **Xcode 26.4 Stable** and lists only Preview 1–7 links, while later
sections include RC1 material. It describes incremental reload's default without
RC2's explicit preview warning and omits RC2's C# UI template, Apple Shell opt-in
and scene migration. Treat it as discovery/history, not a single authoritative
current version/status table. PIN-694 already reconciles its misleading managed
HMError-removal implication; this audit does not redo that API scope.
The reviewed known-issues wiki only documents Windows CLI app launching; its
silence about NativeAOT cannot supersede the pinned NativeAOT document.

## Separately pinned AI preparation input

The actual public package inspected is **Microsoft.Maui.Essentials.AI
11.0.0-preview.7.26512.5**, SHA-256
`879ebc349f1b7260ab7ff40644b6b126cf65376cf358110669f754b39c3c734f`.
Its repository is **dotnet/maui-labs**, commit
**bf5365abb26e85c3076e4a9c1395e55d9cad765a**. It is **not** an implicit RC2 MAUI
workload pack and must not inherit MAUI's RC2 package number by assumption.

It ships assemblies only for **net10.0**, **net10.0-android36.0**,
**net10.0-ios26.0**, **net10.0-maccatalyst26.0**, **net10.0-macos26.0** and
**net10.0-windows10.0.19041**; there is no net11 or tvOS assembly asset.
Each nuspec dependency group declares AI.Abstractions **10.4.1** and
DependencyInjection/Hosting/Logging.Abstractions **10.0.5** version constraints.
These are declared minimum constraints, not a resolved lock file or compilation
result. A net11 consumer's chosen older asset, transitive resolution and native
bridge must be verified in the AI content slice before declaring that route supported.
The `.NET 11` package version and source platform annotations do not prove those facts.

`ai-requirements` distinguishes Foundation Models chat (Apple 26+, eligible device,
enabled Apple Intelligence and an available model) from Natural Language embeddings
(no Apple Intelligence requirement, model/language availability still matters).
The package bridge's deployment requirements take precedence over the underlying
API's older introduction version. Generic Android/Windows assembly assets do not
establish a native AI implementation; the current docs say those implementations
are unavailable. The source chat type even has a tvOS availability annotation,
but that cannot create a tvOS package asset. Image-input claims need a package
with verified image support, Apple 27 target frameworks and Xcode 27. The selected
26-labelled assets alone establish neither availability nor absence of image
support. Do not infer it from current source or documentation alone. No model,
simulator or device execution ran here.

## Replay and remaining evidence

Requires PowerShell 7.2+ and HTTPS. Verified starting in `C:\Windows`:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-78bc9c15'
& ./eng/Inspect-MobileBaselines.ps1
if (-not $?) { throw 'Mobile baseline audit failed' }
```

Expect both exact 18-entry sets, six manifest/pack checks, **six net10.0 AI
assets** and **assembly experimental MAUIAI0001**, ending with **PASS: 16 pinned
source snapshots, 21 source assertions, 9 package hashes/provenance**. A changed
hash, set mapping, selected pack, provenance commit, asset path or missing
experimental annotation fails. A mutable-source drift failure requires fresh
source review; never simply bless a new hash. Nothing is installed or left running.

scope. No served recipe or MCP tool changes in this research task. Actual
previous/current consumer compilation, native platform behavior, external-service
or AI execution remain with their content slices; final GA reconciliation remains
PIN-966. Native absence is an explicit scope boundary, not a waived failed test.
