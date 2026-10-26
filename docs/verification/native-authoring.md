# Native authoring access assessment

Historical access assessment from 21 October 2026, not recipe execution proof.
Recheck the required capability before using a route; owner handoffs remain with
Pingu and PIN-699. [Measured preflight](native-authoring.json).

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

These missing capabilities remain delivery prerequisites for the affected
content. Closing this access-assessment ticket does not waive them, establish
those recipe claims or make the catalogue release-ready. No related ticket's
status, blocking relation or requirements were changed.
