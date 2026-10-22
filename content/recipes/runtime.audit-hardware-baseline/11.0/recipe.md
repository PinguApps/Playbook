# Check CPU requirements before a runtime upgrade

## Goal

Check the machines receiving a dispatch worker before upgrading its runtime. Separate the CPU instructions needed to start .NET from optional precompiled-code targets and the application's own higher requirements.

## Older approach

The .NET 10 x86/x64 baseline corresponds to x86-64-v1. An inventory tool can read CPUID rather than infer support from a processor marketing name. This original check tests the baseline bits (CX8, CMOV, SSE, SSE2); it is not a complete OS/application support certification.

{{snippet:older}}

## Newer approach

.NET 11 raises the x86/x64 minimum to x86-64-v2, adding SSE3, SSSE3, CX16, SSE4.1, SSE4.2 and POPCNT. The policy below evaluates the actual CPUID bits, including CX16 which is not covered by a simple SSE4 check. It deliberately fails outside x86/x64 rather than treating another architecture as unsupported .NET hardware.

{{snippet:newer}}

## Migration

1. Compile the newer Baseline.cs in a nullable/implicit-usings console project. The canonical project uses net11.0/C# 15.0; it checks an already-running runtime. To assess an old candidate machine **before upgrading**, compile this same ordinary CPUID policy into a net10.0/C# 14.0 inventory tool instead. There are no .NET 11-only APIs in the policy. The recipe's served edition still targets .NET 11; this preflight is an older-runtime comparison tool, not a claim that .NET 11 can start on unsupported hardware.

Call `Samples.Current.DispatchHardware.MeetsBaseline()` and stop rollout when it returns false; log the inspected machine/architecture and actual CPUID result. On a non-x86/x64 machine use its own platform support inventory instead of this probe. X86Base.IsSupported guards the instruction before calling CpuId. CPUID reports the virtual CPU exposed to this process, which can differ across hosts/VM configurations; repeat the inventory on deployment targets. A running .NET 11 app cannot provide a graceful startup fallback on a CPU where the runtime itself fails before Main. Do not use an intrinsic-disable or performance switch to waive a required instruction set.

## Constraints

For Windows/Linux x86/x64 the .NET 11 ReadyToRun target rises to x86-64-v3; Apple x86/x64 retains its v2 R2R target. AVX/AVX2, BMI1/BMI2, F16C, FMA, LZCNT and MOVBE in the v3 target are distinct from the v2 runtime minimum. A supported v2 machine can JIT affected ReadyToRun methods instead, with possible startup cost. This does not make an application or NativeAOT image deliberately compiled for a higher ISA portable to a lower one. Assess its actual publish configuration and dependencies independently.

Arm64 is separate: Linux retains an armv8.0-a baseline while the R2R target adds LSE. Apple retains the M1 baseline/target. For Windows Arm64 the detailed compatibility article and pinned RC2 cpufeatures implementation retain the armv8.0-a minimum, with R2R targeting armv8.2-a + RCPC. The runtime overview instead describes LSE as a new Windows minimum. That source conflict is explicit: this x64 recipe does not approve deployment to non-LSE Windows Arm64 machines; reconcile the actual final runtime and OS policy before that rollout. No Arm64 execution evidence is asserted. ReadyToRun fallback is not evidence that an unsupported runtime/OS can start.

## Evidence

Both pinned projects compile and execute on the actual Windows x64 host, recording CPUID and runtime versions. The RC2 check independently clears each newly required bit and verifies policy rejection; these synthetic register cases do not emulate or execute an old CPU. The same newer Baseline.cs is also compiled and run under the .NET 10 inventory tool in the focused authoring check. No unavailable hardware, Arm64, cross-compilation, NativeAOT deployment or startup-time benchmark is claimed. Minimum/R2R consequences are source-reviewed against official documentation and exact RC2 source; final GA reconciliation is pending.
