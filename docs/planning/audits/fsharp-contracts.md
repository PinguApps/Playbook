# F# contracts audit

Reviewed **20 October 2026**. This closes the bounded source/asset investigation, **not recipe coverage**. The dispositions below describe the source review at capture. The current inventory and served recipes record later delivery; this audit is not current coverage status.

## Scope and provenance

Scope: [PIN-681](https://linear.app/pinguapps/issue/PIN-681), SPEC.md at `0a8989814ea11346354464b845c79c15bb71c5e9`, stories 47, 94, 158 and 159; approved proposal `b3778cc`, `SH_Q01`; inventory identities `fsharp.author-runtime-async-builder`, `fsharp.configure-compiler-service-memory` and `fsharp.aot-and-fsi-lifetimes` are preserved.

Consulted the agreed planning README, implementation/coverage/verification plans, source inventory, glossary, contribution guidance and ADRs 0001–0004. Root PLAN.md, CONTEXT.md and CONTEXT-MAP.md are absent. Old planning statements that implementation has not begun are superseded by repository truth. The originating spec and approved proposal are available in Git history; no explicit requirement pointer is unavailable.

The [input manifest](fsharp-contracts.sources.json) pins **21 raw official sources**, three downloaded NuGet archives, their hashes, asset paths, repository identities and dependency groups. The [metadata baseline](fsharp-contracts.assets.json) records seven actual asset surfaces, assembly versions, target-framework attributes, full selected CLR signatures, generic constraints and dynamic-code annotations. These are bounded contract observations, not a complete F# API diff or executed consumer verification.

- **Matching RC2 source:** both RC2 packages identify dotnet/dotnet commit `820230c386db5b0106c5430b3e5aee92a7826bf5`. That repository and its `src/fsharp/` files are accessible. Its source manifest identifies upstream F# commit `9cd6167a7265ce7264b22719503b7dfa9eb8f83c`. Looking up the VMR hash in dotnet/fsharp would select the wrong repository.
- **Canonical release notes:** dotnet/core `ffed591b0fb8f9aab50ecc28f2c765049fb28d8f`, RC2 F# notes.
- **Drift comparison only:** dotnet/fsharp main `8de08f6371c0ee34cb516a5e32dc7cc7ffe6a626`. Its runtime-async document enables the feature from F# **11.2**. Matching RC2 `LanguageFeatures.fs` gates it on **preview**. Current main must not replace RC2 requirements or be treated as GA verification.

## Actual distributable assets

| Artifact | Observed assets and boundary |
| --- | --- |
| FSharp.Core **10.1.402**, assembly 10.1.0.0 | `lib/netstandard2.0/FSharp.Core.dll`, `lib/netstandard2.1/FSharp.Core.dll`; no runtime-async intrinsics in either inspected surface. This is the repository's previous-release package baseline, not a claim of API introduction in 10.1.402. |
| FSharp.Core **11.0.101-rc2.26504.105**, assembly 11.0.0.0 | `lib/netstandard2.0`, `lib/netstandard2.1`, `lib/net10.0`. All six runtime-async intrinsics occur only in **net10.0**. Async-disposal `TaskBuilderBase.Using` occurs in netstandard2.1/net10.0, not netstandard2.0. |
| FSharp.Compiler.Service **43.13.101-rc2.26504.105**, assembly 43.13.101.0 | `lib/netstandard2.0/FSharp.Compiler.Service.dll`; public `FSharpChecker.Create` includes `FSharpOption<bool> shareImportedAssemblies`. Its nuspec requires the **exact** RC2 FSharp.Core above; the netstandard asset does not require upgrading a tool's runtime to .NET 11 by itself. Actual older-host dependency resolution remains a delivery check. |
| SDK **11.0.100-rc.2.26504.105**, `FSharp/FSharp.Core.dll` | Actual TargetFrameworkAttribute is **.NETCoreApp v10.0**. Selected signatures match the package's net10.0 surface, but the DLL hash differs: `cc9140abff3283005b2418dd270c6a097e70abcb90c22af3473fafac0d2687ca`. This is an SDK toolset copy, not a byte-identical NuGet asset. |

FCS's remaining declared dependencies are System.Buffers 4.6.1, System.Collections.Immutable 10.0.10, System.Diagnostics.DiagnosticSource 10.0.10, System.Memory 4.6.3, System.Reflection.Emit 4.7.0, System.Reflection.Metadata 10.0.10 and System.Runtime.CompilerServices.Unsafe 6.1.2. The nuspec ranges and exclusions, not guessed runtime minima, determine restore; restore/lockfile/compilation must still verify a proposed host.

The SDK's `fsi.runtimeconfig.json` requests `net11.0` / Microsoft.NETCore.App **11.0.0-rc.2.26504.105** (SHA-256 `bbfb42eb807421b5f3dbf03e8605e24eaa2f190489b312ec9ee9ede67173b37c`). `fsi.deps.json` labels its local Core entry `lib/net11.0/FSharp.Core.dll`, despite the DLL's net10.0 target attribute (deps hash `c9f3565ac8865c59edce8b25aa425ce3bfa0c19047ebaa2cb6487f351633e2ee`). A deps path label is not proof of a public NuGet net11.0 asset. No such package asset was observed.

## Contract dispositions

### Runtime-async builder authoring — included, proposals A and E

Matching [resumable.fsi][resumable] declares these public compiler intrinsics under `#if NET`, corroborated by the actual net10.0 DLL:

```fsharp
__runtimeAsyncReturn : 'T -> Task<'T>
__runtimeAsyncReturnValueTask : 'T -> ValueTask<'T>
__runtimeAsyncReturnUnit : unit -> Task
__runtimeAsyncReturnValueTaskUnit : unit -> ValueTask
__runtimeAsyncSequence : (unit -> seq<'T>) -> IAsyncEnumerable<'T>
__runtimeAsyncSequenceCancellationToken : unit -> CancellationToken
```

These are signatures abbreviated with standard .NET type names, not runnable example implementations. The [implementation][resumable-impl] supplies throwing stubs consumed by the compiler; calling them as ordinary helpers is invalid. Return markers require matching compiler lowering and .NET runtime-async support. [RC2 language gates][language] and [runtime contract][runtime] require preview, reference metadata exposing `MethodImplOptions.Async`, and an executing runtime that actually supports it. The Core asset's **net10.0** label does **not** make net10 application execution supported: the documented release route targets/runs on **.NET 11 RC2**.

`AsyncHelpers.Await`, `AwaitAwaiter` and `UnsafeAwaitAwaiter` are runtime suspension primitives, not replacements for `Task.Result` in ordinary code. Inline custom builders provide the author-facing API; `runtimeTask` is a [compiler-test builder][builder], not a shipped FSharp.Core value. Existing `task {}` / `async {}` are not switched over by this release. Do not copy the upstream test implementation into an ostensibly original recipe.

The source contract forbids `localloc` and synchronized methods, rejects tracked byref/byref-like/pinned values surviving suspension, and rejects suspension outside runtime-async methods. Suspending exception handlers/finally require compiler rewriting; pinned provenance through captured delegates/ref cells is not reliably diagnosed. Compilation alone cannot prove lifetime safety, stepping quality or runtime support. No AOT support is inferred for these intrinsics.

**Additional confirmed goal E:** direct asynchronous-sequence construction has two real distributed intrinsics, even though the [source document][runtime] calls its high-level implementation a proposal. The [sequence builder][sequence] remains test support. Statically known inline recipes, cancellation, pending awaits, early disposal and cleanup exception precedence need separate delivery. Opaque recipes, optimized tail handoff and concurrent move/dispose are unsupported; a non-cancellable await does not become cancellable; complete stepping is not guaranteed. A shipped intrinsic is not a shipped general-purpose sequence builder.

### Compiler-host import sharing — included, proposal B

Matching [service.fsi][service] adds `?shareImportedAssemblies: bool` to `FSharpChecker.Create`; the actual package signature contains the optional Boolean in that position. [service.fs][service-impl] defaults it to **true**. A tool author can deliberately construct `FSharpChecker.Create(shareImportedAssemblies = false)`; this is a hosted FCS API, not an MSBuild property or a `dotnet fsc` command-line switch.

The [import implementation][imports] shares imported assembly graphs only for matching file/configuration/reference closures. Project outputs, type-provider assemblies, multi-module imports and unaccountable closures remain outside sharing. Entries are weakly held while consumers retain their closure. Clearing internal compiler caches clears sharing, but **this audit does not advertise an additional public ClearCaches API**: it did not appear among the selected public methods in this package.

Default memory savings are an **excluded automatic-upgrade benefit**. The deliberate opt-out / hosted import-isolation decision is an **included authored configuration goal** tied to this first-party release, not standalone coverage of arbitrary FCS versions. Published benchmark percentages are workload-specific; no universal memory improvement or hard memory cap is established. Actual correctness of checkers with sharing on/off and an older host remains unexecuted.

### Array2D AOT migration — included, proposal C

Matching [array2.fs][array] and actual metadata identify all three annotated public entry points:

```fsharp
Array2D.zeroCreateBased : int -> int -> int -> int -> 'T[,]
Array2D.createBased : int -> int -> int -> int -> 'T -> 'T[,]
Array2D.initBased : int -> int -> int -> int -> (int -> int -> 'T) -> 'T[,]
```

The leading integers are base1, base2, length1 and length2. CLR names are **ZeroCreateBased**, **CreateBased** and **InitializeBased**. Each has RequiresDynamicCode in every RC2 asset, absent from the inspected previous-release assets. Their message rejects non-zero lower bounds in AOT and directs callers to zeroCreate/create/init. Even a call with literal zero bases still selects an annotated public entry point; prefer the unbased API rather than suppressing IL3050 indiscriminately.

`zeroCreate`, `create`, `init` and `rebase` use zero-based allocation; `map`, `mapi` and `copy` preserve input bounds through a private auxiliary path. The net10.0 asset can use Array.CreateInstanceFromArrayType for that path; netstandard assets use Array.CreateInstance. **No warning on a method is a guarantee for arbitrary non-zero-based data under AOT.** A based-data migration must deliberately translate coordinates/preserve values, or retain a supported JIT-only route. Zero-based improvements are automatic until an application changes representation or replaces an annotated call; that concrete migration qualifies. Whole-application Native AOT success and diagnostic propagation remain delivery checks.

### FSI asynchronous resource lifetime — included, proposal D

Matching [tasks.fsi][tasks] declares `TaskBuilderBase.Using<'Resource,... when 'Resource :> IAsyncDisposable>` only for `NETSTANDARD2_1 || NET`. Both previous-release and RC2 netstandard2.1 assets already have this overload: **it is not a newly introduced language binding**. The [release source][fcs-notes] attributes the FSI correction to shipping net10.0 Core in the SDK toolset ([PR 20555](https://github.com/dotnet/fsharp/pull/20555)). The inspected SDK copy has that overload and the expected target attribute.

The qualifying goal is choosing the correct FSI/toolset asset and using an asynchronous resource in `task { use ... }`, awaiting the resulting task through cleanup. This does not promise top-level asynchronous `use`, new `async {}` disposal semantics or Native AOT compilation of an interactive host. Do not infer success from package signatures alone: the real `dotnet fsi --exec` path and loaded Core identity still need execution evidence. No preview requirement is inferred for this ordinary task binding from the unrelated runtime-async experiment.

### Grouped and excluded obligations

- **Grouped:** [PIN-719](https://linear.app/pinguapps/issue/PIN-719) owns caller-start, bounded work and preserved rethrows, not custom runtime-async builder authoring or FSI asset selection.
- **Grouped:** [PIN-757](https://linear.app/pinguapps/issue/PIN-757) owns shared runtime/compiler settings and diagnostics. Link its runtime explanation from A/E; its existing scope does not deliver F# builder implementations.
- **Distinct:** [PIN-735](https://linear.app/pinguapps/issue/PIN-735) is the .NET 9/10 async-LINQ foundation in C#, not an F# runtime-async sequence-builder goal.
- **Excluded automatic effects:** general compiler memory reductions, collection allocation improvements and automatic embedding/linking of metadata-trimming rules. No independent authored workflow was established. These do not exclude C's AOT migration or D's explicit asset/lifetime goal.
- **Unresolved delivery evidence:** compiler/consumer execution, Native AOT, FSI runtime identity/cleanup and any older-target FCS route. Actual GA reconciliation stays in [PIN-961](https://linear.app/pinguapps/issue/PIN-961). [PIN-959](https://linear.app/pinguapps/issue/PIN-959) must require delivered recipes for all qualifying slices; this audit does not satisfy its coverage gate.

## Historical delivery proposals

These proposals record the audit decision at capture, not current delivery status. Consult the inventory and served recipes before planning more work. Proposed destination: PinguApps team `62f80270-b641-4fa3-b60c-46be4c4df2c8`, Playbook project `b22380c7-6fde-49f4-8826-e5631dc5df7a`, existing **02 - Language and library guidance** milestone `c5232470-50ac-4f56-aea8-bd8ff7d13b41`, Backlog. A team-and-project-scoped paginated search included all **312** issues with archived results; the highest allocated prefix was **02-048**. Recheck overlap and allocate numbers immediately before approved creation; draft keys A–E deliberately reserve no conflicting numbers. Existing tickets retain titles, estimates and states. Approved tickets should relate natively to PIN-681 and block applicable coverage/GA closure until delivered.

Each draft is one PR and includes these **required future acceptance criteria**:

1. Reconcile matching official source, actual compiler/reference/runtime/package assets and supported conditions. Keep exact verification pins separate from justified consumer requirements. Prove any older-target route; an absent API blocks delivery.
2. Author original F# examples in a fictional domain, normally .NET 10 versus .NET 11, plus full offline Markdown, metadata, canonical snippets, sources, migration/alternatives and explicit SDK/runtime/language/package-asset/platform/stability requirements. Explain unavailable older equivalents honestly. Preserve existing IDs; no ninth historical foundation is added.
3. Compile actual declared older/newer dependencies, record locks where applicable, and capture fresh compilation plus the focused execution/diagnostic checks below with the existing evidence workflow. Resolve missing native/toolchain proof before publication; never label metadata checks as execution.
4. Build/pack once, use the existing isolated installed-tool stdio list/get harness from an unrelated directory, and verify complete body, applicability, experimental labels and target selection offline. Update the disposition register; do not add an OS/client matrix. Keep GA proof separate and leave merging/publication to the owner.

### A — Author an experimental runtime-async task builder — L (5)

Preserve `fsharp.author-runtime-async-builder`. As a library author, I want to define an asynchronous workflow API so callers can compose waits while retaining clear failure and cleanup rules. Supply original builder support code, not a fictional FSharp.Core.runtimeTask. Exercise Task/ValueTask return and unit carriers, genuinely pending awaits, sequential binding, return!, exception propagation and asynchronous disposal on .NET 11 RC2/preview. Check the emitted runtime-async marker, rejection without preview/target support, and prohibited byref/suspension shapes. Keep an ordinary .NET 10 task-builder comparison. No AOT or general-purpose builder completeness claim. Estimate includes compiler/runtime coupling, original support code and negative/installed-consumer verification.

### B — Choose imported-assembly sharing in an F# compiler host — M (3)

Preserve `fsharp.configure-compiler-service-memory`. As a tool author, I want an explicit sharing policy for imported assemblies so analysis can retain correct project boundaries. Use FCS 43.13.101-rc2.26504.105 and its exact Core dependency initially; verify default and false configurations against projects sharing a binary plus projects with differing reference closures. Assert identical intended symbols/diagnostics and isolation after a referenced assembly changes. Prove the selected actual host target/dependencies; compare the prior host without inventing an older parameter. Explain exclusions, lifetime/caches and measured-versus-universal memory claims. No broad benchmark suite or hard-memory-cap promise. Estimate covers package compatibility, real checker fixtures and installed guidance.

### C — Migrate two-dimensional array indexing for Native AOT — M (3)

Retain coverage under `fsharp.aot-and-fsi-lifetimes`; propose child goal `fsharp.migrate-based-arrays-for-aot`. As an application author, I want to preserve matrix coordinates and values in an AOT-compatible representation. Replace all three annotated public Based operations appropriately; compare original lower-bound semantics against zero-based storage plus explicit offsets. Test create/init/zeroCreate and map/mapi/copy/rebase on the actual selected asset, including empty dimensions and index mapping. Capture real IL3050 call-site diagnostics with warnings-as-errors, and publish/run the supported zero-based .NET 11 Native AOT program on one suitable machine. Compare the previous .NET 10 implementation fairly; do not claim AOT supports arbitrary based inputs or suppress warnings as the solution. Estimate includes native publish/toolchain and coordinate-preservation checks.

### D — Await asynchronous cleanup in F# Interactive — S (2)

Retain coverage under `fsharp.aot-and-fsi-lifetimes`; propose child goal `fsharp.dispose-async-resources-in-fsi`. As a script author, I want asynchronous resources released before a script completes, including when work fails. Use an original IAsyncDisposable whose DisposeAsync actually suspends and records ordering. Run the script through both declared previous/current `dotnet fsi --exec` toolsets, identify loaded FSharp.Core location/target/hash, and show the actual older behavior without claiming the overload itself is new. Check normal and exceptional cleanup happens exactly once before completion; retain the body error/cleanup precedence actually observed. Include full commands and supported asset selection without manual SDK-file replacement. No top-level async-use, interactive AOT or preview requirement invented. Estimate covers two actual FSI processes and lifecycle/packaged guidance checks.

### E — Build an experimental pull-driven F# asynchronous sequence — L (5)

Cover the two sequence intrinsics under the existing runtime-builder inventory identity; propose child goal `fsharp.author-runtime-async-sequence`. As a library author, I want to produce records on demand so consumers can cancel or stop without leaking resources. Write original minimal support over actual intrinsics and show a fair previous-release implementation with its declared dependencies. Verify pending MoveNextAsync, incremental delivery, cancellation propagation and limits, early DisposeAsync, nested/exceptional cleanup and documented exception precedence. Verify unsupported concurrent/opaque/tail-handoff cases and preview/target gates. No shipped asyncSeq2/TaskSeq substitute, cancellation of non-cancellable work or complete debugger-stepping claim. Compilation and real execution must establish the viable bounded route before serving a recipe; unresolved compiler limitations remain explicit blockers. Estimate covers producer ownership, cancellation and compiler/runtime restrictions plus installed verification.

## Reproduce the bounded audit

Prerequisites: PowerShell 7; .NET SDK 10.0.402 under `C:\Program Files\dotnet` (MetadataLoadContext only), reference pack 10.0.13, and the existing SDK 11.0.100-rc.2.26504.105 under `C:\Users\pingu\.dotnet-playbook-11`. The script accepts `-Net10Root`, `-ReferencePackVersion` and `-Net11Root` for equivalent installations. HTTPS access downloads pinned official source and NuGet archives to a fresh temporary directory and removes only that directory. Nothing is installed; no SDK/compiler or inspected assembly code executes, and no PATH/environment integration runs.

Verified from an unrelated directory before selecting this exact worktree:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-4d84c6cb'
& './eng/Inspect-FSharpContracts.ps1' `
  -OutputPath './artifacts/fsharp-contracts.json' `
  -CompareWith './docs/planning/audits/fsharp-contracts.assets.json'
$assets = Get-Content './artifacts/fsharp-contracts.json' -Raw | ConvertFrom-Json
$assets | Format-Table package, version, asset, targetFramework
$assets.declarations | Where-Object { $_ -match '__runtimeAsync|shareImportedAssemblies|RequiresDynamicCode|IAsyncDisposable' }
```

Expected: **21 source hashes, 3 package hashes, 7 asset surfaces**; declaration counts **10, 11, 17, 10, 11, 1, 17**. A source/archive hash, DLL hash, target-framework, version or selected signature mismatch fails. Baseline comparison also detects unexpected presence/absence of the six intrinsics and the asynchronous Using overload in the inspected types. A deliberate corrupted source hash was rejected; the protected baseline-output path was also rejected. These negative checks validate the audit guard, not an F# consumer.



Actual evidence is source hashing and metadata inspection only. No restore/build of recipe samples, Native AOT publish/run, FSI execution, compiler-host behavior or GA verification ran. No served recipe/MCP surface changed, so a server build or Inspector call would not verify this audit. The work adds no runtime/data/configuration changes requiring recovery.

[resumable]: https://github.com/dotnet/dotnet/blob/820230c386db5b0106c5430b3e5aee92a7826bf5/src/fsharp/src/FSharp.Core/resumable.fsi
[resumable-impl]: https://github.com/dotnet/dotnet/blob/820230c386db5b0106c5430b3e5aee92a7826bf5/src/fsharp/src/FSharp.Core/resumable.fs
[language]: https://github.com/dotnet/dotnet/blob/820230c386db5b0106c5430b3e5aee92a7826bf5/src/fsharp/src/Compiler/Facilities/LanguageFeatures.fs
[runtime]: https://github.com/dotnet/dotnet/blob/820230c386db5b0106c5430b3e5aee92a7826bf5/src/fsharp/docs/runtime-async.md
[builder]: https://github.com/dotnet/dotnet/blob/820230c386db5b0106c5430b3e5aee92a7826bf5/src/fsharp/tests/FSharp.Compiler.ComponentTests/Language/RuntimeAsync/RuntimeTaskBuilder.fs
[sequence]: https://github.com/dotnet/dotnet/blob/820230c386db5b0106c5430b3e5aee92a7826bf5/src/fsharp/tests/FSharp.Compiler.ComponentTests/Language/RuntimeAsync/RuntimeAsyncSequenceBuilder.fs
[service]: https://github.com/dotnet/dotnet/blob/820230c386db5b0106c5430b3e5aee92a7826bf5/src/fsharp/src/Compiler/Service/service.fsi
[service-impl]: https://github.com/dotnet/dotnet/blob/820230c386db5b0106c5430b3e5aee92a7826bf5/src/fsharp/src/Compiler/Service/service.fs
[imports]: https://github.com/dotnet/dotnet/blob/820230c386db5b0106c5430b3e5aee92a7826bf5/src/fsharp/src/Compiler/Driver/CompilerImports.fs
[array]: https://github.com/dotnet/dotnet/blob/820230c386db5b0106c5430b3e5aee92a7826bf5/src/fsharp/src/FSharp.Core/array2.fs
[tasks]: https://github.com/dotnet/dotnet/blob/820230c386db5b0106c5430b3e5aee92a7826bf5/src/fsharp/src/FSharp.Core/tasks.fsi
[fcs-notes]: https://github.com/dotnet/dotnet/blob/820230c386db5b0106c5430b3e5aee92a7826bf5/src/fsharp/docs/release-notes/.FSharp.Compiler.Service/11.0.100.md
