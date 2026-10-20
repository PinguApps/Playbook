# Distinguish normal exit from signal termination

## Goal

A beacon worker can finish with application code 7 or be asked to stop. A portable report must keep those events distinct rather than interpreting every integer as the application’s own result.

## Older approach

Create a net10.0 / C# 14 console project. Use System.Diagnostics; newer native examples also use Microsoft.Win32.SafeHandles, System.Runtime.InteropServices and System.Runtime.Versioning; omit imports for namespaces not used by the selected snippet. All snippets belong inside a static class in namespace Playbook.Samples; System, System.IO, System.Linq, System.Collections.Generic and System.Threading.Tasks come from implicit usings.

The older helper requests termination and returns the ordinary integer ExitCode. It is valid for a simple exit-code contract but cannot reliably reconstruct a POSIX signal from that integer alone; numeric conventions such as 128+signal are not universal proof.

{{snippet:older}}

## Newer approach

Create the corresponding net11.0 / C# 15 project, with the same imports and implicit usings.

On Linux/macOS send SIGTERM through Signal, then collect ProcessExitStatus. On Windows the example uses Kill and reports the Windows exit contract without inventing a POSIX signal. Normal exit, cancellation and signal information have distinct properties.

{{snippet:newer}}

## Migration

1. Preserve ExitCode, Signal and Canceled as separate report fields. Do not translate Windows termination into a fictional POSIX signal, or classify a normal numeric exit as a signal without status evidence.
2. Preserve the observable result and error policy; compile with the actual declared target and dependencies. A newer installed SDK does not establish an older runtime route.
3. Run the focused checks described below before adopting the replacement.

## Constraints

Signal is a request, not a guarantee of termination: a Unix application can handle or ignore SIGTERM. A production stop policy should impose a grace period, then force termination when its own contract allows it. false can indicate an exit race; still collect status. Permissions, an invalid/closed handle and a process exiting between lookup and signalling need their own error handling. The test’s default-signal worker terminates, and Linux preserves SIGTERM separately from normal exit 7. Windows execution checks Signal=null. macOS uses the guarded POSIX route but has no native execution proof. SafeProcessHandle offers the equivalent Signal/wait primitives without the Process component.

This edition targets .NET 11. It has no rendering mode, external provider, optional workload or experimental opt-in. It uses framework APIs and needs no additional package or package asset. The sample SDK pins 10.0.402 and 11.0.100-rc.2.26504.105 are reproducibility baselines, not mandatory exact consumer SDK installations. RC2 execution is not GA verification. No .NET 10 catalogue edition or conditional older-target package route is advertised. macOS support is explicitly untested natively, as accepted by Pingu; pending Human proof remains visible and prevents claiming release readiness.

## Evidence

The original ProcessExecution projects compile and run against their declared .NET 10/11 SDKs. Windows checks capture over 70,000 characters on each pipe, retain nonzero exit status, cancel a running worker, own/dispose native handles, check suspended setup/resume and failure cleanup, and distinguish normal termination. A separate pinned Linux container compiles the same canonical files and verifies SIGTERM, parent-bound/forgotten/detached lifetime, cancellation and redirected ANSI policy. Windows colour execution produces no ANSI in either mode. Suspended startup is not executed on Linux; macOS is not executed anywhere.

Saved evidence includes exact input hashes, source commit/dirty marker, SDK/runtime host, commands, dependencies and output. No mocks or cross-compilation substitute for execution. Consult the embedded records for what actually ran; no complete-catalogue, native-deployment or product-publication readiness is implied.
