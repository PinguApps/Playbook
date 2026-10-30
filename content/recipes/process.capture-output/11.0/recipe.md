# Capture command output and cancel its execution

## Goal

A fictional beacon survey invokes a reporting utility. Preserve stdout, stderr and a nonzero exit code separately. Start both reads before waiting: sequential draining can deadlock when either pipe fills.

## Older approach

Create a net10.0 / C# 14 console project. Use System.Diagnostics; newer native examples also use Microsoft.Win32.SafeHandles, System.Runtime.InteropServices and System.Runtime.Versioning; omit imports for namespaces not used by the selected snippet. All snippets belong inside a static class in namespace Playbook.Samples; System, System.IO, System.Linq, System.Collections.Generic and System.Threading.Tasks come from implicit usings.

The older wrapper owns a Process, drains both streams concurrently and kills the process tree when waiting is cancelled. This remains a valid implementation when you need its tree-wide termination policy.

{{snippet:older}}

## Newer approach

Create the corresponding net11.0 / C# 15 project, with the same imports and implicit usings.

The framework helper coordinates output collection and waiting. With a ProcessStartInfo, set UseShellExecute=false and both RedirectStandardOutput and RedirectStandardError=true before calling it. The string/argument-list overload configures capture itself. This wrapper preserves the older tuple contract and propagates cancellation.

{{snippet:newer}}

## Migration

1. Replace the manual coordination when direct-child termination and complete text buffering match the utility. Keep argument tokens in ArgumentList; never concatenate untrusted arguments into a shell command. Handle startup exceptions separately from a nonzero exit status.
2. Preserve the observable result and error policy; compile with the actual declared target and dependencies. A newer installed SDK does not establish an older runtime route.
3. Run the focused checks described below before adopting the replacement.

## Constraints

Capture buffers complete output in memory; bound the command/output externally or stream large results. Run/RunAsync return status without collecting text. A started Process also exposes ReadAllText/ReadAllBytes and async equivalents; ReadAllLinesAsync yields Content and StandardError per line. Start both redirections before using these readers. Cross-stream ordering is not guaranteed. Process.RunAndCaptureTextAsync kills its direct child on cancellation; the older wrapper requests entireProcessTree=true, so these are not identical descendant policies. Cancellation can throw during reading or return a Canceled status during waiting; inspect both paths. Kill is asynchronous; cancellation is not proof that the OS has already reaped the child.

This edition targets .NET 11. It has no rendering mode, external provider, optional workload or experimental opt-in. It uses framework APIs and needs no additional package or package asset. The sample SDK pins 10.0.402 and 11.0.100-rc.2.26504.105 are reproducibility baselines, not mandatory exact consumer SDK installations. RC2 execution is not GA verification. No .NET 10 catalogue edition or conditional older-target package route is advertised. macOS support is explicitly untested natively, as accepted by Pingu; the not-tested disclosure remains visible without requiring a Mac or blocking publication.

## Evidence

The original ProcessExecution projects compile and run against their declared .NET 10/11 SDKs. Windows checks capture over 70,000 characters on each pipe, retain nonzero exit status, cancel a running worker, own/dispose native handles, check suspended setup/resume and failure cleanup, and distinguish normal termination. A separate pinned Linux container compiles the same canonical files and verifies SIGTERM, parent-bound/forgotten/detached lifetime, cancellation and redirected ANSI policy. Windows colour execution produces no ANSI in either mode. Suspended startup is not executed on Linux; macOS is not executed anywhere.

Saved evidence includes exact input hashes, source commit/dirty marker, SDK/runtime host, commands, dependencies and output. No mocks or cross-compilation substitute for execution. Consult the embedded records for what actually ran; no complete-catalogue, native-deployment or product-publication readiness is implied.

PIN-699 testing boundary: macOS execution: no Mac environment. Excluded under PIN-699; recorded Windows/Linux results remain separate. This explicit exclusion does not block publication and does not establish the untested behaviour.
