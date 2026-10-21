# Select supported vector instructions with a correct scalar fallback

## Goal

Dispatch a numeric operation to supported instruction families, build signed SVE loop predicates and preserve scalar results when a processor or operating system cannot run an accelerated path.

**Experimental:** Arm SVE/SVE2 contracts carry SYSLIB5003. This edition is incomplete evidence: the canonical code compiles, but SVE and AVX-VNNI-512 execution still require suitable hardware. Do not treat the x64 fallback run as verification of those branches.

Use an SDK console project with implicit usings and nullable enabled, in namespace Playbook.Samples. Import System.Runtime.Intrinsics, System.Runtime.Intrinsics.X86 and System.Runtime.InteropServices in the fallback file. The newer dispatch file also imports System.Numerics, System.Runtime.CompilerServices and System.Runtime.Intrinsics.Arm. The older dispatch file needs System.Runtime.Intrinsics.X86. The newer project targets net11.0 / C# 15 and sets AllowUnsafeBlocks=true for the pinned SVE memory operations. Its scoped pragma is the experimental opt-in; suppressing the warning does not establish support or stability.

## Older approach

A fictional tide-monitoring station applies an integer bias to sensor readings and computes the sum of signed short products. The .NET 10 route uses AVX2 only when Avx2.IsSupported; otherwise it uses scalar arithmetic. Full vectors are followed by a scalar tail. This remains valid and avoids requiring advanced hardware. Older SVE APIs have a smaller predicate surface; the comparison does not invent .NET 10 equivalents of the new signed predicate or the V512 VNNI overloads.

{{snippet:older}}

The following shared fallback implementation is part of both comparisons. Include it alongside either declaration above or below; all three files use the same partial class and namespace. Arithmetic deliberately wraps modulo 2^32, including pairwise multiplication overflow; this is not a checked financial accumulator.

{{snippet:fallback}}

## Newer approach

The signed SVE predicate activates only logical int positions before the exclusive end of a buffer. The selected SVE path uses the runtime's active 32-bit lane count and masks the final partial load/store. The x64 product path checks AvxVnni.V512.IsSupported for the exact 512-bit widening multiply-add operation, rather than inferring it from AVX2, AVX512F or vector width. All other machines retain the same AVX2/scalar route.

{{snippet:newer}}

The feature report queries the actual RC2 classes Sve, Sve2, SveSha3 and SveSm4. A true Sve2.IsSupported is not permission to call a separately guarded SHA3 or SM4 family. The cumulative notes use broader SVE2 feature terminology; the actual reference assembly uses the distinct names shown here. It exposes no separate SveAes class, so this recipe does not invent that class or use an AES capability claim as a dispatch condition. No cryptographic implementation is demonstrated.

## Migration

1. Define scalar semantics first: input lengths, integer overflow and tail handling. Here product spans must have equal lengths, output bias preserves length, and both calculations use wrapping int arithmetic.
2. Check the exact family immediately before dispatch. IsSupported includes the runtime and operating-system support decisions; an architecture string alone does not establish instruction availability.
3. Keep ISA-specific calls behind their guards. The SVE and VNNI bodies are separate noninlined helpers; the caller never enters them when the corresponding flag is false.
4. For SVE loops, match predicate lane type to the data type, choose an exclusive or inclusive boundary deliberately, use the runtime lane count and mask tail memory access. Do not hard-code a fixed scalable-vector width.
5. Run the shared scalar-agreement checks on real suitable hardware. The sample accepts --require-sve and --require-vnni512: each fails explicitly if its required branch is unavailable. A normal PASS with bias=AVX2 and products=AVX2 proves only those selected paths.
6. Run a fresh process with DOTNET_EnableHWIntrinsic=0 and verify bias=scalar and products=scalar. Keep disabled-intrinsic settings local to verification; do not change machine configuration or substitute that run for advanced-path execution.

## Constraints

This recipe is experimental because of Arm SVE, even though the selected AVX2/VNNI contracts have no SYSLIB5003 opt-in. SVE needs an Arm64 execution environment with actual SVE support exposed by the OS/runtime; SVE2/SHA3/SM4 are separate capabilities. The V512 path needs the exact AVX512-VNNI-capable x86/x64 environment recognized by AvxVnni.V512.IsSupported. Unsupported ISA calls can throw PlatformNotSupportedException; generic Vector128 software fallback in the lane-layout recipe is a different contract. Cross-compilation, a container architecture label or emulation does not prove native instruction support.

No extra NuGet package/assets, provider, workload or rendering mode is required. The APIs come from .NET 11; the exact RC2 build and C# 15 mode are the canonical baseline. There is no documented/tested older-target package route. This is numeric dispatch guidance, not a performance guarantee, hardware-detection library, encryption implementation or claim that every SVE2 operation is verified.

## Evidence

The actual .NET 10 and 11 SupportedInstructions projects compile and run on Windows x64, with AVX2=True and all inspected SVE families and AVX-VNNI-512=False on .NET 11. Both selected AVX2 operations agree with independently computed scalar results for lengths 0, 1, 7, 8, 9, 31, 32, 33 and 79, including negative values, overflow and tails. A forced-disabled run checks the scalar branches. SDK 10.0.402 / C# 14 and SDK 11.0.100-rc.2.26504.105 / C# 15 records retain exact input hashes, actual runtimes, commands and output.

**Pending Human proof:** no SVE/SVE2-capable or AVX-VNNI-512-capable host is available. The native branches compile but have not executed, and their required-mode checks fail explicitly on this machine. Publication of the combined PIN-739/PIN-740 change remains blocked. The pending status and scope accompany the offline recipe; compilation and fallback execution do not waive it. No mocks, native cross-compilation or GA proof are claimed.
