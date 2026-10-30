# Own process and redirected handles explicitly

## Goal

A beacon scheduler only needs a child’s completion code and wants to discard its output. Own the process handle directly rather than retaining the higher-level component and its associated state.

## Older approach

Create a net10.0 / C# 14 console project. Use System.Diagnostics; newer native examples also use Microsoft.Win32.SafeHandles, System.Runtime.InteropServices and System.Runtime.Versioning; omit imports for namespaces not used by the selected snippet. All snippets belong inside a static class in namespace Playbook.Samples; System, System.IO, System.Linq, System.Collections.Generic and System.Threading.Tasks come from implicit usings.

The older version keeps the Process alive while borrowing its SafeHandle and drains both redirected streams into Stream.Null. Do not dispose a borrowed SafeHandle independently; doing so can invalidate its owner’s operations. The newer version replaces that pipe-draining work with direct null-handle redirection.

{{snippet:older}}

## Newer approach

Create the corresponding net11.0 / C# 15 project, with the same imports and implicit usings.

SafeProcessHandle.Start gives the caller an owned handle. Keep the independently owned null-file handle alive through startup and dispose both deterministically. Use StandardInputHandle/StandardOutputHandle/StandardErrorHandle for native redirection; do not combine a corresponding Handle with RedirectStandardOutput/Error/Input.

{{snippet:newer}}

## Migration

1. Replace Process ownership only when the smaller handle API supplies everything needed. Keep redirection owners in an outer using scope. Handle startup/access failures explicitly and never use a stale PID as proof that the same process still exists.
2. Preserve the observable result and error policy; compile with the actual declared target and dependencies. A newer installed SDK does not establish an older runtime route.
3. Run the focused checks described below before adopting the replacement.

## Constraints

Disposal closes a handle; it does not kill the process. Wait before disposing when completion is required. Open/TryOpen obtain separately owned handles; permission failures and exit/PID-reuse races remain possible after a successful lookup. The sample opens its current PID, disposes that independent handle and rejects a nonexistent PID with TryOpen. InheritedHandles selects native handles for inheritance; keep all entries alive until creation completes and do not share unrelated inheritable secrets. Console.OpenStandardInputHandle/OpenStandardOutputHandle/OpenStandardErrorHandle expose standard handles for native wiring; their returned wrappers and a child’s copies have separate lifetimes. Native launch callbacks are advanced alternatives: WindowsProcessStartArguments.Start expects a callback returning the created process handle; UnixProcessStartArguments.Start expects a PID. They provide prepared arguments/environment/standard handles, not an automatic OS launcher. No custom native callback implementation or callback execution is claimed by this example.

This edition targets .NET 11. It has no rendering mode, external provider, optional workload or experimental opt-in. It uses framework APIs and needs no additional package or package asset. The sample SDK pins 10.0.402 and 11.0.100-rc.2.26504.105 are reproducibility baselines, not mandatory exact consumer SDK installations. RC2 execution is not GA verification. No .NET 10 catalogue edition or conditional older-target package route is advertised. macOS support is explicitly untested natively, as accepted by Pingu; the not-tested disclosure remains visible without requiring a Mac or blocking publication.

## Evidence

The original ProcessExecution projects compile and run against their declared .NET 10/11 SDKs. Windows checks capture over 70,000 characters on each pipe, retain nonzero exit status, cancel a running worker, own/dispose native handles, check suspended setup/resume and failure cleanup, and distinguish normal termination. A separate pinned Linux container compiles the same canonical files and verifies SIGTERM, parent-bound/forgotten/detached lifetime, cancellation and redirected ANSI policy. Windows colour execution produces no ANSI in either mode. Suspended startup is not executed on Linux; macOS is not executed anywhere.

Saved evidence includes exact input hashes, source commit/dirty marker, SDK/runtime host, commands, dependencies and output. No mocks or cross-compilation substitute for execution. Consult the embedded records for what actually ran; no complete-catalogue, native-deployment or product-publication readiness is implied.

PIN-699 testing boundary: macOS execution: no Mac environment. Excluded under PIN-699; recorded Windows/Linux results remain separate. This explicit exclusion does not block publication and does not establish the untested behaviour.
