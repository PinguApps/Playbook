# Choose a supported threading path

## Goal

Score the fictional Reed Station's independent survey plots on runtimes with threads, while producing the same score on a single-thread browser. Thread support is a capability, not a promise that parallel execution is faster or that several workers will run concurrently.

## Older approach

.NET10 has no audited RuntimeFeature.IsMultithreadingSupported property. A bounded application can retain a conservative browser/WASI exclusion and use its known desktop threading configuration elsewhere. This is not a general capability detector for every custom runtime. Sequential processing remains a valid choice even on a threaded runtime.

{{snippet:older}}

The complete PlotScorer class uses implicit usings for System, Linq, Threading and Tasks. The counts are immutable input during the operation; each iteration owns one output slot. For counts2,5,9, the score is48 in both paths.

## Newer approach

.NET11 supplies RuntimeFeature.IsMultithreadingSupported. Select the path using the actual runtime's capability rather than inferring it from a platform annotation. The false branch uses an ordinary loop, without Task.Run, Parallel APIs, thread creation or blocking waits.

{{snippet:newer}}

Copy the complete class with imports into a nullable-enabled net11.0 project. Call ScoreReedPlots with the plot counts and the caller's cancellation token. The method is synchronous; a large sequential workload can still block a browser UI. Chunking/cooperative asynchronous work is a separate application design decision.

## Migration

1. Build net11.0 with a .NET11 SDK/runtime and C#15. The tested RC2 SDK/runtime versions are evidence pins, not proof of final-GA behavior.
2. Replace the older bounded platform condition with IsMultithreadingSupported. Do not equate IsDynamicCodeSupported, processor count or platform annotations with threading support.
3. Keep independent work per iteration and deterministic result aggregation. Do not mutate counts while scoring. Thread availability does not remove data-race or ownership obligations.
4. Pass the caller's token to ParallelOptions and check the same token between sequential items. Preserve OperationCanceledException rather than returning an incomplete score. Cancellation is cooperative; a synchronous browser cannot receive UI cancellation while a long uninterrupted call blocks its event loop.
5. Keep the sequential branch free of thread scheduling and blocking primitives. Prefer it for small work even when threads are available if measurement supports that decision; this sample makes no speedup claim.
6. Run the actual threaded desktop and single-thread browser consumers with the same inputs and a canceled token. Check the selected capability and result, not only successful compilation.

## Constraints

The console route uses the shared framework with no package, provider, workload or experimental opt-in. The browser host uses matching locked WebAssembly packages and interpreted Mono runtime assets, with no native relinking/AOT workload. Windows/Linux x64 and single-thread Chromium are measured; threaded browser, WASI, other architectures and browsers remain unverified. A false capability selects a useful fallback rather than an exception-driven thread probe.

The sample checks cancellation before/between work and before returning; it does not prove in-flight externally signaled browser cancellation. Integer overflow is surfaced rather than silently wrapping. Parallel iteration exceptions can be aggregated by Parallel.For; callers must handle normal failures as well as cancellation. No fairness, worker count, ordering or throughput guarantee is made.

## Evidence

eng/Record-SupportPaths.ps1 -Record captures paired SDK10.0.402/runtime10.0.13 and SDK11RC2/runtime11RC2 consumers. Windows/Linux execute the threaded branch; actual Mono Chromium consumers report threads=False and use the sequential branch. Both produce score48 and propagate pre-cancellation. The browser path contains no unsupported scheduling; platform annotations alone are not used as execution evidence.

The sample-evidence workflow records exact inputs, dependencies and commands, with separate real behavior receipts. Paired pinned official sources qualify the new property. Installed stdio retrieval must verify the complete offline Markdown and explicit target11.0/edition11.0 applicability. The eight .NET10 foundations and the separate PIN-959/PIN-962 gates remain unchanged. No final-GA, performance or in-flight browser cancellation claim is made.