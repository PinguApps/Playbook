# Native authoring access assessment

Access assessments, not recipe execution proof. The Windows desktop/MAUI route
was measured on 31 October 2026 below; the other capabilities retain their
21 October assessment. Recheck the required capability before using a route. Pingu revised ownership
through PIN-699 on 30 October 2026: the [native testing policy](../contributing.md#native-testing-policy)
limits authoring checks to this Windows machine and existing Docker. The missing
hardware/toolchain handoffs below are historical, not outstanding obligations;
record unavailable checks as sample-specific `not-tested` exclusions. Pingu owns
feasible interactive approvals and refresh coordination. [Measured preflights](native-authoring.json).

## Windows desktop and MAUI authoring, 31 October 2026

[Inspect-WindowsAuthoring.ps1](../../eng/Inspect-WindowsAuthoring.ps1) creates
six independent original probes in a unique short `%TEMP%/pbw-*` directory.
It pins SDK/target/language, isolates MSBuild and central-package inheritance,
restores NuGet dependencies and builds each project. `-Launch` additionally
starts only successful builds, waits at most 30 seconds per process and stops
only that probe if it hangs. Each window closes itself after its lifecycle
receipt. It installs no SDK/workload, registers no application package and
changes no certificate trust, Developer Mode or system settings. Internet access
to the configured NuGet sources is needed for uncached packages.

The existing x64 machine reports Windows build **26200**. The measured routes:

- **WPF and WinForms, .NET 10:** `C:/Program Files/dotnet/dotnet.exe`, SDK
  **10.0.402**, C# **14.0**, target `net10.0-windows`, runtime **10.0.13**.
  Both Release builds succeeded with zero warnings/errors; WPF reached
  `ContentRendered` and WinForms reached `Shown`, then exited successfully.
- **WPF and WinForms, .NET 11:** `C:/Users/pingu/.dotnet-playbook-11/dotnet.exe`,
  SDK **11.0.100-rc.2.26504.105**, C# **15.0**, target `net11.0-windows`, runtime
  **11.0.0-rc.2.26504.105**. Both Release builds and the same controlled lifecycle
  checks succeeded with zero warnings/errors. These are RC results, not GA proof.
- **MAUI Windows, .NET 10:** the same .NET 10 SDK/runtime, target
  `net10.0-windows10.0.19041.0`, RID `win-x64`, workload
  `maui-windows` **10.0.110/10.0.100**, actual SDK pack **10.0.110**, and
  `Microsoft.Maui.Controls` **10.0.110**. Its original C# page and Windows XAML
  application built with zero warnings/errors, reached `Label.Loaded` and closed
  successfully. `WindowsPackageType=None`, `WindowsAppSDKSelfContained=true`
  and `SelfContained=false` copy the Windows App SDK dependencies locally while
  using the selected .NET runtime. Resolved Windows App SDK **1.8.260529003**,
  WinUI **1.8.260528001** and Windows SDK BuildTools **10.0.26100.8249** are recorded
  with the complete package closure in `native-authoring.json`.
- **MAUI Windows, .NET 11 — not-tested:** the separate .NET 11 root advertises
  `maui-windows` **11.0.0-preview.6.26360.8/11.0.100-preview.6**, but contains no
  `Microsoft.Maui.Sdk` pack. The exact `net11.0-windows10.0.19041.0` / `win-x64`
  probe with intended Controls **11.0.0-preview.6.26360.8** failed before
  compilation with `NETSDK1147` (diagnostic names `maui-tizen`). The workload
  listing does not prove a usable Windows toolchain. Its compilation and launch
  are excluded under PIN-699; no workload restore/install is required or performed.

The [saved compact assessment](native-authoring.json) under `windowsAuthoring`
retains the script hash, shared/project input hashes, exact sample paths,
measured SDK/workload inventories, resolved dependencies, commands, lifecycle
observations and failed-route scope. Temporary source, locks and full logs are
disposable; the committed script reproduces their authored inputs. This is
capability proof only, not saved compilation evidence for any served recipe.
Future canonical samples still use `verification.json`, `Record-Evidence.ps1`
and focused behaviour reports as described in the [contribution guide](../contributing.md#capture-and-refresh-sample-evidence).

### Recheck and launch safely

Use PowerShell 7.4+ (for child-process environment overrides). This command was
checked from an unrelated starting directory. Omit `-Launch` for build-only
assessment. The generated windows contain only the fictional Willow Quay label;
they access no accounts, devices or provider services.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-7e9779e7'
$report = & ./eng/Inspect-WindowsAuthoring.ps1 -Launch
$report.routes | Select-Object framework, target, sdk, build, launch, observation
$report.work # Unique scratch directory containing report.json, projects and logs.
```

Expect five `build=passed, launch=passed` routes with the runtime versions above,
and one .NET 11 MAUI `build=failed, launch=not-tested` route with `NETSDK1147`.
The helper is an assessment: it continues after a route failure and returns its
report, so an exit code of zero alone does not establish availability. Inspect
every route and its `limitation`; another failure is not the expected result.
`report.json` also captures the complete measured workload listing and input hashes.
Keep a short scratch root: an earlier deep worktree/cache path produced XAML
`WMC1006` metadata-resolution failures despite the DLLs existing. The shorter
default path built and launched the same MAUI inputs successfully.

Launches explicitly set child `DOTNET_ROOT` and `DOTNET_ROOT_X64` to the selected
SDK installation. Without this, the first .NET 11 apphost selected the machine's
newer RC3 runtime, which did not establish RC2 execution. Bare `dotnet` currently
resolves to the separate dotnetup installation; its advertised SDK 10.0.402
failed to load (`hostpolicy.dll` / missing `dotnet.runtimeconfig.json`). Use the
absolute executable paths above. Do not repair or replace shared installations
as part of an authoring check.

The helper disables .NET global-tool PATH integration before creating its isolated
CLI home, restores its process environment and compares process/user/machine PATH
before and after. The measured run left all three unchanged. Scratch files remain
for inspection; no service remains running. After collecting needed evidence,
remove only the exact `pbw-*` directory printed by that run, after checking its
resolved path is inside the chosen temporary root. Never delete a computed parent.

### Permissions, excluded behaviour and Human handoff

The interactive desktop allowed all five lifecycle checks without elevation or
owner intervention. The read-only Developer Mode registry observation was **1**;
that is not permission to change it or evidence of packaged deployment. These
probes do not establish visual correctness, accessibility, Designer integration,
clipboard handling, fullscreen, activation, maps, WebView caching, passkeys or
permission prompts. They do not test MSIX installation/signing, camera/location
access, device/emulator flows, Android/Apple builds or GA .NET 11.

No UI/settings permission was denied during these checks. Elevation, trust/signing
changes and protected consent dialogs were not exercised or granted. For a feasible
recipe check needing them, the author first builds the actual canonical sample,
then gives **Pingu** its exact route, source/input identity, SDK/workload/dependency
versions, target/RID, command, required action and expected observation through the
content PR. Pingu supplies the interactive approval and returned result; the author
records it in the existing evidence format. Keep feasible outstanding approvals
`pending`. For the unavailable .NET 11 MAUI route or another unavailable native
environment, retain source review and exact sample/input identity, mark only that
sample/behaviour `not-tested`, and list it in `humanProof.untestedSamples`; do not
replace available local compilation or provider/security checks with that exclusion.
Recheck after relevant sample, toolchain, dependency or permission conditions change.
No new native CI is introduced; existing hosted package checks remain.

Source review: Microsoft's [Desktop SDK properties](https://learn.microsoft.com/en-us/dotnet/core/project-sdk/msbuild-props-desktop)
support the WPF/WinForms project switches. The [MAUI unpackaged CLI guidance](https://learn.microsoft.com/en-us/dotnet/maui/windows/deployment/publish-unpackaged-cli?view=net-maui-10.0)
and [Windows setup guidance](https://learn.microsoft.com/en-us/dotnet/maui/windows/setup?view=net-maui-10.0)
explain unpackaged settings and the separate Developer Mode requirement for packaged
deployment. Those sources explain configuration; only the observations above
establish what ran here. No publish/deployment route was tested by these builds.

## Measured access, 21 October 2026

[Inspect-NativeCapabilities.ps1](../../eng/Inspect-NativeCapabilities.ps1) builds
and executes independent temporary probes on both pinned SDKs. The saved
[preflight output](native-authoring.json) records actual results, SDK/workload
inventories, instruction support, loaded native library versions and native
compiler dependencies. These are preflight records, not recipe evidence records.

- **Windows x64:** Windows build 26200, AMD Ryzen Threadripper 3970X. SDK
  **10.0.402**, runtime **10.0.13**; SDK **11.0.100-rc.2.26504.105**, runtime
  **11.0.0-rc.2.26504.105**. Both probes compiled with zero warnings/errors.
- **Instructions:** both runtimes report SSE through SSE4.2, AVX/AVX2, AES,
  BMI1/BMI2, FMA, LZCNT, PCLMULQDQ and POPCNT support. Vector128/256 acceleration
  is true; Vector512, AVX-512 and Arm/SVE families are false. The JSON preserves
  every inspected public instruction-family `IsSupported` result. Support flags
  establish dispatch availability, not correctness of a future vector algorithm.
- **TLS/QUIC:** both runtimes completed a real TLS 1.3 client/server handshake and
  byte echo over an ephemeral loopback TCP port. The exchange uses temporary
  certificate keys and requires no certificate-store write. Both report
  `QuicListener.IsSupported` and `QuicConnection.IsSupported` true and loaded
  **MsQuic 2.5.11.159415436** plus Windows Schannel **10.0.26100.1**. The .NET 11
  runtime exports `TlsContext`, `TlsSession`, `TlsBufferSession`,
  `TlsSocketSession` and `TlsOperationStatus`. Type discovery is not execution
  of those experimental APIs. QUIC support detection is not a QUIC connection.
- **Native AOT:** minimal .NET 10 and .NET 11 apps were actually published for
  `win-x64` and their native executables each printed `Native AOT route: X64`.
  MSVC linker **14.51.36260.0**, tool directory **14.51.36231**, and Windows SDK
  libraries **10.0.26100.0** were supplied explicitly. The report records exact
  ILCompiler and ILLink dependencies. This does not test assembly introspection,
  trimming, single-file locations or Native AOT caller metadata.
- **Linux container:** Docker Desktop client/server **29.6.1**, Linux x86_64 on
  WSL2 kernel **6.6.87.2-microsoft-standard-WSL2**, BuildKit **0.31.1**. An actual
  network-disabled container ran from the existing pinned SDK image used by
  [the Linux process check](../../eng/Test-ProcessPlatforms.ps1): Ubuntu
  **26.04.1**, SDK **11.0.100-rc.2.26504.105**, OpenSSL **3.5.5** and `libssl.so.3`.
  No `cc`, `clang`, `ld`, `emcc`, `podman` or cached `libmsquic` was found in that
  image. It is not currently a Linux native compiler or QUIC route.
- **Container engine:** `desktop-linux` is running and advertises linux/amd64,
  arm64, riscv64, ppc64le, s390x and arm variants. Only x64 was executed. The
  advertised platforms do not prove native hardware, successful cross-builds,
  manifest output, reproducibility or Podman compatibility. No Podman executable
  was found on the Windows host either.

## Selected routes and remaining owner handoffs

Use temporary directories on this Windows host for x64 managed/SIMD, TLS/QUIC
and Native AOT authoring. Use disposable Docker containers for Linux checks,
with no production mounts, published ports, privileged access or shared service
changes. Per-recipe checks must still compile the actual canonical inputs and
capture their own claim-specific execution evidence.

- **PIN-739 — vector lanes/instruction selection:** x64 authoring and AVX2
  dispatch are available. **Pingu owns the missing suitable Arm/SVE/SVE2 and,
  where required, AVX-VNNI-512 execution route.** Supply an accessible isolated
  host and measured OS, ISA flags, SDK/runtime and agent commands. A container's
  `arm64` label or emulation does not establish SVE support. The content author
  must compare scalar/vector results and exercise fallback independently.
- **PIN-742 — assembly metadata/deployment modes:** Windows x64 Native AOT
  compilation and execution are available. Its author must separately verify
  the actual metadata/caller and unavailable-location cases for each deployment
  mode; the minimal executable above does not satisfy those recipe criteria.
- **PIN-752 — caller-owned TLS I/O:** use the .NET 11 Windows route, with
  experimental contracts reconciled against the exact artifacts. The real
  SslStream exchange proves OS TLS access only. Its author still owes a handshake
  and data transfer through the caller-owned API, including ownership/state and
  diagnostic checks. **Pingu owns an alternative isolated platform handoff if
  the supported contract cannot execute on this Windows route.**
- **PIN-755 — QUIC priority:** Windows has the shipped native library and true
  runtime support detection. Its author must open an actual loopback QUIC
  connection and exercise priority with the supported API. This preflight does
  not establish UDP connectivity, scheduling fairness or priority guarantees.
  Linux needs a compatible measured `libmsquic` before it can be used instead.
- **PIN-758 — native CoreCLR WebAssembly:** **Pingu owns the missing proven
  relinking route.** Workload listing advertises `wasm-tools` for .NET 10 and
  .NET 11 RC, but the inspected separate .NET 11 root has no browser/Emscripten
  packs; the main installation has older Emscripten **3.1.56 / 10.0.13** assets.
  Neither a workload label nor Mono assets proves CoreCLR native relinking.
  Supply/reconcile the matching runtime pack, Emscripten, workload and JS host
  versions in an isolated authoring installation, with absolute-path commands.
  The recipe author must link and invoke its actual original native function.
- **PIN-765 — container artifacts:** the existing Docker engine is usable for
  isolated authoring. **Pingu owns the missing Podman route** required by the
  assigned Podman-specific inventory item. Supply an existing isolated endpoint
  and agent-executable commands/version inventory. Its author must separately
  publish local images/manifests for the selected architectures and check stable
  inputs/digests. No registry publication or new host is authorized here.

The missing capabilities above no longer require obtaining other environments.
Their guidance remains explicitly untested in those conditions. Available local
checks, evidence freshness and the remaining release gates still apply. This
historical assessment establishes none of those recipe executions.
