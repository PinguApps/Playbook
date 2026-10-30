# Prepare a suspended child before allowing execution

## Goal

A beacon launcher must register the actual child PID with its supervisor before the worker writes its first record. Preparation before ordinary launch cannot supply that PID.

## Older approach

Create a net10.0 / C# 14 console project. Use System.Diagnostics; newer native examples also use Microsoft.Win32.SafeHandles, System.Runtime.InteropServices and System.Runtime.Versioning; omit imports for namespaces not used by the selected snippet. All snippets belong inside a static class in namespace Playbook.Samples; System, System.IO, System.Linq, System.Collections.Generic and System.Threading.Tasks come from implicit usings.

The honest managed .NET 10 comparison performs only prelaunch preparation. It has no managed suspended-start/resume pair. Achieving the newer post-creation boundary requires platform-specific native creation and cleanup; this recipe does not invent a portable equivalent.

{{snippet:older}}

## Newer approach

Create the corresponding net11.0 / C# 15 project, with the same imports and implicit usings.

On Windows or macOS, set StartSuspended before SafeProcessHandle.Start, perform setup with child.ProcessId, then Resume and wait. The catch path kills and waits for the child so a setup failure does not abandon a suspended process. Import System.Runtime.Versioning for the platform attributes.

{{snippet:newer}}

## Migration

1. Separate prelaunch configuration from work that truly needs the created PID. Resume only after successful setup. Always provide failure cleanup; closing the handle alone leaves the suspended child alive.
2. Preserve the observable result and error policy; compile with the actual declared target and dependencies. A newer installed SDK does not establish an older runtime route.
3. Run the focused checks described below before adopting the replacement.

## Constraints

StartSuspended and Resume are supported on Windows/macOS, not Linux. Platform annotations are part of the wrapper’s public contract; callers must guard the OS before invoking it. Windows execution checks that the child’s output marker is absent during setup, present after resume, and absent after a deliberately failed setup whose child has exited. macOS is source-supported and compiled as guarded code but is explicitly untested natively, as accepted by Pingu. No Apple execution, signing, entitlement or sandbox compatibility is inferred. A callback that blocks indefinitely also blocks startup; supply a bounded supervisor operation.

This edition targets .NET 11. It has no rendering mode, external provider, optional workload or experimental opt-in. It uses framework APIs and needs no additional package or package asset. The sample SDK pins 10.0.402 and 11.0.100-rc.2.26504.105 are reproducibility baselines, not mandatory exact consumer SDK installations. RC2 execution is not GA verification. No .NET 10 catalogue edition or conditional older-target package route is advertised. macOS support is explicitly untested natively, as accepted by Pingu; the not-tested disclosure remains visible without requiring a Mac or blocking publication.

## Evidence

The original ProcessExecution projects compile and run against their declared .NET 10/11 SDKs. Windows checks capture over 70,000 characters on each pipe, retain nonzero exit status, cancel a running worker, own/dispose native handles, check suspended setup/resume and failure cleanup, and distinguish normal termination. A separate pinned Linux container compiles the same canonical files and verifies SIGTERM, parent-bound/forgotten/detached lifetime, cancellation and redirected ANSI policy. Windows colour execution produces no ANSI in either mode. Suspended startup is not executed on Linux; macOS is not executed anywhere.

Saved evidence includes exact input hashes, source commit/dirty marker, SDK/runtime host, commands, dependencies and output. No mocks or cross-compilation substitute for execution. Consult the embedded records for what actually ran; no complete-catalogue, native-deployment or product-publication readiness is implied.

PIN-699 testing boundary: macOS execution: no Mac environment. Excluded under PIN-699; recorded Windows/Linux results remain separate. This explicit exclusion does not block publication and does not establish the untested behaviour.
