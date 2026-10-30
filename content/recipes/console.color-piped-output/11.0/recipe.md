# Choose colour policy for redirected console output

## Goal

A beacon report is piped into an ANSI-aware log viewer. Select whether escape sequences should be emitted; a pipe does not make its receiver capable of rendering colour.

## Older approach

Create a net10.0 / C# 14 console project. Use System.Diagnostics; newer native examples also use Microsoft.Win32.SafeHandles, System.Runtime.InteropServices and System.Runtime.Versioning; omit imports for namespaces not used by the selected snippet. All snippets belong inside a static class in namespace Playbook.Samples; System, System.IO, System.Linq, System.Collections.Generic and System.Threading.Tasks come from implicit usings.

For Unix .NET 10 Console output, the runtime-specific ANSI redirection switch can opt into colour. NO_COLOR is an existing convention, not new in .NET 11. Applications that emit escape sequences themselves already work independently of Console’s policy.

{{snippet:older}}

## Newer approach

Create the corresponding net11.0 / C# 15 project, with the same imports and implicit usings.

Set the environment on the child ProcessStartInfo before launch. .NET 11 Unix Console recognizes FORCE_COLOR. Remove a conflicting NO_COLOR for an explicit colour request; set NO_COLOR and remove FORCE_COLOR for plain output. This example deliberately avoids relying on precedence when both are present.

{{snippet:newer}}

## Migration

1. Apply ConfigureColor to the start information used by the capture helper. Set a suitable TERM for the actual receiver when needed. Inspect raw output for ESC characters as well as visually checking an ANSI-capable viewer.
2. Preserve the observable result and error policy; compile with the actual declared target and dependencies. A newer installed SDK does not establish an older runtime route.
3. Run the focused checks described below before adopting the replacement.

## Constraints

The exercised Linux route uses TERM=xterm-256color and Console.ForegroundColor/ResetColor with stdout redirected. Windows RC2 execution emitted no ANSI in either mode: this recipe does not claim FORCE_COLOR enables Windows redirected Console colouring. macOS follows the Unix implementation but has no native execution proof here. Other logging libraries and tools may interpret these conventions differently. FORCE_COLOR does not change IsOutputRedirected or guarantee renderer support. Child-local Environment entries do not mutate the parent, user or machine environment.

This edition targets .NET 11. It has no rendering mode, external provider, optional workload or experimental opt-in. It uses framework APIs and needs no additional package or package asset. The sample SDK pins 10.0.402 and 11.0.100-rc.2.26504.105 are reproducibility baselines, not mandatory exact consumer SDK installations. RC2 execution is not GA verification. No .NET 10 catalogue edition or conditional older-target package route is advertised. macOS support is explicitly untested natively, as accepted by Pingu; the not-tested disclosure remains visible without requiring a Mac or blocking publication.

## Evidence

The original ProcessExecution projects compile and run against their declared .NET 10/11 SDKs. Windows checks capture over 70,000 characters on each pipe, retain nonzero exit status, cancel a running worker, own/dispose native handles, check suspended setup/resume and failure cleanup, and distinguish normal termination. A separate pinned Linux container compiles the same canonical files and verifies SIGTERM, parent-bound/forgotten/detached lifetime, cancellation and redirected ANSI policy. Windows colour execution produces no ANSI in either mode. Suspended startup is not executed on Linux; macOS is not executed anywhere.

Saved evidence includes exact input hashes, source commit/dirty marker, SDK/runtime host, commands, dependencies and output. No mocks or cross-compilation substitute for execution. Consult the embedded records for what actually ran; no complete-catalogue, native-deployment or product-publication readiness is implied.

PIN-699 testing boundary: macOS execution: no Mac environment. Excluded under PIN-699; recorded Windows/Linux results remain separate. This explicit exclusion does not block publication and does not establish the untested behaviour.
