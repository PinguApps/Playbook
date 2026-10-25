# F# lifetimes evidence

Historical October 2026 authoring observations. Follow the [evidence reuse policy](../contributing.md#choose-the-check-and-retain-its-result).
[Original capture and detailed provenance](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-982-984.md).

## Matching source and actual assets

The [PIN-681 input manifest](../planning/audits/fsharp-contracts.sources.json) and
[metadata baseline](../planning/audits/fsharp-contracts.assets.json) were reproduced: **21 official source
hashes, three package hashes and seven asset surfaces**, all matched. The correct
RC2 package repository is dotnet/dotnet commit
`820230c386db5b0106c5430b3e5aee92a7826bf5`, containing the matching F# source under
`src/fsharp/`; its upstream F# identity is `9cd6167a7265ce7264b22719503b7dfa9eb8f83c`.
The manifest's separately pinned main source is drift evidence, not a replacement
for the matching compiler. Canonical RC2 notes are pinned at dotnet/core
`ffed591b0fb8f9aab50ecc28f2c765049fb28d8f`.

- SDK 10.0.402, FSharp.Core 10.1.402: actual compiled package asset is
  `lib/netstandard2.1/FSharp.Core.dll`. FSI instead loads the SDK's netstandard2.0
  copy, hash `7c1574193332ea24678d32103df140c094c85cf1b08708ffda88ef44540b03a6`.
- SDK 11.0.100-rc.2.26504.105, FSharp.Core 11.0.101-rc2.26504.105: package consumers
  select `lib/net10.0/FSharp.Core.dll`; actual locks and resolved dependencies are
  retained. Array/FSI examples use ordinary F# 11.0; sequence examples use preview.
- Current FSI's actual SDK tools copy targets `.NETCoreApp,Version=v10.0`, hash
  `cc9140abff3283005b2418dd270c6a097e70abcb90c22af3473fafac0d2687ca`. It is not
  byte-identical to the NuGet DLL and its deps path is not a public package asset.
- Native evidence uses the actual 11.0.0-rc.2.26504.105 ILCompiler/linker packs,
  MSVC 14.51.36231 and Windows SDK 10.0.26100.0. The SDK's recorded discovery output
  identifies those exact native library paths. No optional .NET workload is used.
  Within those installed toolsets, the measured linker file version is
  14.51.36260.0 and resource compiler file version is 10.0.26100.8249; the supplemental
  record retains their hashes separately from toolset directory versions.

These are exact reproducibility baselines. Recipe requirements separately explain
SDK/runtime/language/package-asset/platform/stability constraints. This is RC2
evidence; actual GA reconciliation remains PIN-961.

## Acceptance coverage and observed differences

**Array migration:** all three Based allocations become unbased allocation plus
explicit offsets. Original lower bounds, translated indexed mapping, empty first/
second/both dimensions, values and copy independence are checked. The same based
JIT comparison is compiled/executed against the newer selected asset too. Real
Native AOT analysis rejects ZeroCreateBased/CreateBased/InitializeBased with
IL3050 errors even for literal zero bases. The supported zero-based program is
published and executed as a native Windows x64 PE, with warnings as errors and
no suppression. The supplemental record hashes the executable and checks that
it has no managed metadata. Unannotated helper methods do not promise arbitrary
based-array AOT support. A plain chart class and concrete integer formatting
avoid unrelated record/printf reflection roots encountered during authoring.

**FSI:** real previous/current `dotnet fsi --exec` processes record loaded location,
target attribute and hash. Gated IAsyncDisposable cleanup runs once and completes
before script completion in all four body-success/failure and cleanup-success/
failure cases. The previous SDK's direct asynchronous Using binding fails FS0001;
its explicit awaited cleanup alternative succeeds. That same unchanged explicit
alternative also executes on the newer FSI. The newer ordinary task binding waits
for pending disposal but **loses a subsequent cleanup failure**: a successful body
returns its value and a failed body retains its exception. Explicit cleanup
propagates a cleanup failure, taking precedence over a body failure. This measured
RC2 limitation is in discovery constraints and detail. No new Using overload,
preview requirement, top-level async use or interactive AOT is claimed.

**Sequence:** original minimal inline support uses actual __runtimeAsyncSequence
and __runtimeAsyncSequenceCancellationToken intrinsics. It compiles and executes
with genuinely pending moves, incremental results, normal exhaustion, pending
early disposal, nested token propagation/cancellable waiting, non-cancellable
waiting followed by a token check, and pending nested/exceptional cleanup. Outer
cleanup failures replace inner cleanup failures; inner cleanup failures replace
body failures; successful cleanup preserves body exception identity. A small
serial adapter rejects overlapping moves/disposal before entering unsupported
raw iterator state. It makes no performance claim. Opaque recipes and optimized
tail handoff fail FS3922; missing preview and runtime-reference support fail
FS3350/FS3351. No shipped general-purpose builder, TaskSeq substitute, full
debugger stepping, .NET 10 runtime-async route or Native AOT support is claimed.
