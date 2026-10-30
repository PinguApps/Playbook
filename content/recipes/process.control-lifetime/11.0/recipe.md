# Choose child lifetime independently of process bookkeeping

## Goal

A beacon importer can run as a foreground job, an independent worker or a task that must stop with its parent. Choose this policy before starting it; releasing a Process object is not a termination policy.

## Older approach

Create a net10.0 / C# 14 console project. Use System.Diagnostics; newer native examples also use Microsoft.Win32.SafeHandles, System.Runtime.InteropServices and System.Runtime.Versioning; omit imports for namespaces not used by the selected snippet. All snippets belong inside a static class in namespace Playbook.Samples; System, System.IO, System.Linq, System.Collections.Generic and System.Threading.Tasks come from implicit usings.

The scoped older helper kills its child during orderly cancellation/cleanup. It cannot run finally blocks after abrupt parent termination. .NET 10 has no managed KillOnParentExit equivalent; a Windows job object or a platform supervisor requires separate native plumbing and is not supplied or claimed here.

{{snippet:older}}

## Newer approach

Create the corresponding net11.0 / C# 15 project, with the same imports and implicit usings.

StartAndForget releases the launcher’s bookkeeping and returns a PID. It does not itself detach or request parent-death termination. StartDetached changes OS attachment; KillOnParentExit requests parent-bound termination. The wrapper rejects conflicting independent/parent-bound requests.

{{snippet:newer}}

## Migration

1. Choose parent-bound only for a subordinate task; use detached only when console/session attachment should change. Do not use Process.Dispose as a substitute for either. Keep independent workers’ logs and shutdown protocol explicit.
2. Preserve the observable result and error policy; compile with the actual declared target and dependencies. A newer installed SDK does not establish an older runtime route.
3. Run the focused checks described below before adopting the replacement.

## Constraints

The exact RC2 reference annotations permit KillOnParentExit on Windows, Linux and Android, contradicting the old cumulative Windows-only headline. This sample verifies Windows and Linux only; Android is not an advertised route. macOS supports the independent/detached route, not this parent-bound route. Windows may reject nested/restrictive job configurations; startup failures must remain visible. Retain explicit standard-handle policy: an independent child must not hold a parent’s capture pipe open. StartAndForget defaults unspecified standard handles to null. PIDs can be reused and are not durable identity tokens.

This edition targets .NET 11. It has no rendering mode, external provider, optional workload or experimental opt-in. It uses framework APIs and needs no additional package or package asset. The sample SDK pins 10.0.402 and 11.0.100-rc.2.26504.105 are reproducibility baselines, not mandatory exact consumer SDK installations. RC2 execution is not GA verification. No .NET 10 catalogue edition or conditional older-target package route is advertised. macOS support is explicitly untested natively, as accepted by Pingu; the not-tested disclosure remains visible without requiring a Mac or blocking publication.

## Evidence

The original ProcessExecution projects compile and run against their declared .NET 10/11 SDKs. Windows checks capture over 70,000 characters on each pipe, retain nonzero exit status, cancel a running worker, own/dispose native handles, check suspended setup/resume and failure cleanup, and distinguish normal termination. A separate pinned Linux container compiles the same canonical files and verifies SIGTERM, parent-bound/forgotten/detached lifetime, cancellation and redirected ANSI policy. Windows colour execution produces no ANSI in either mode. Suspended startup is not executed on Linux; macOS is not executed anywhere.

Saved evidence includes exact input hashes, source commit/dirty marker, SDK/runtime host, commands, dependencies and output. No mocks or cross-compilation substitute for execution. Consult the embedded records for what actually ran; no complete-catalogue, native-deployment or product-publication readiness is implied.

PIN-699 testing boundary: macOS execution: no Mac environment. Excluded under PIN-699; recorded Windows/Linux results remain separate. This explicit exclusion does not block publication and does not establish the untested behaviour.
