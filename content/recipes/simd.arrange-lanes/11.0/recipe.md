# Arrange planar and interleaved vector lanes portably

## Goal

Convert independent channel vectors into interleaved lanes and back, select halves, reverse positions and construct repeated numeric patterns without writing instruction-set-specific shuffles.

Use an SDK console project with implicit usings, nullable enabled and System.Runtime.Intrinsics imported. Put the supplied class in namespace Playbook.Samples. Older and newer samples target net10.0 / C# 14 and net11.0 / C# 15 respectively.

## Older approach

A fictional tide-monitoring station keeps two channels in separate four-element int vectors. Explicit lane extraction/construction spells out the interleave, while Shuffle can reverse a vector using an index vector. These are valid .NET 10 techniques and can be preferable for a specialized layout. Fixed numeric patterns are already straightforward to construct explicitly.

{{snippet:older}}

## Newer approach

.NET 11's Zip returns two vectors: the first interleaves the lower halves of the inputs and the second interleaves the upper halves. Unzip then selects even and odd positions from the two outputs to recover the original channels. These operations cross the whole vector layout; their names describe element placement rather than a particular processor opcode.

{{snippet:newer}}

For left lanes 7, 19, -3, 42 and right lanes -2, 5, 11, 8, Zip produces 7, -2, 19, 5 and -3, 11, 42, 8. Concat chooses the named lower/upper half from each input in that order. The pattern helpers construct 3, 9, 27, 81; -4, 6, -4, 6; and floating reciprocals 1/2, 1/4, 1/6, 1/8. Harmonic integer arithmetic would truncate fractions, so the sample deliberately uses float.

## Migration

1. Write down the required lane order using distinct values for each input. Establish whether the result occupies one vector or the pair returned by Zip.
2. Replace the matching manual interleave with Zip, or ZipLower/ZipUpper when only one output is needed. Use Unzip or UnzipEven/UnzipOdd for the inverse selection.
3. Choose the ConcatLowerLower, ConcatLowerUpper, ConcatUpperLower or ConcatUpperUpper form matching the required halves. Use Reverse for the whole vector.
4. Replace an explicitly repeated pattern only when a sequence helper expresses the intended arithmetic. Keep multiplier constants at call sites where the API marks them ConstantExpected; the geometric sample uses the literal 3.
5. Retain a behavior test with nonuniform and negative lanes, and run once with hardware intrinsics disabled to check the portable path. Benchmark your actual workload before claiming a speed improvement.

## Constraints

These generic vector operations provide a software route when hardware acceleration is unavailable; they are different from ISA-specific intrinsic calls that require IsSupported guards. Vector128.IsHardwareAccelerated is a performance capability flag, not a prerequisite for this sample's semantics. Supported primitive element types and vector widths still matter: a generic T parameter does not make arbitrary record types valid vector elements. The release also supplies related operations on Vector64, Vector256, Vector512 and System.Numerics.Vector; this recipe's exact behavior evidence covers Vector128<int> and Vector128<float>, not every width/type/architecture combination.

The new APIs require .NET 11 reference/runtime assemblies. No extra packages/assets, providers, workloads, rendering modes or experimental opt-ins are required for this selected surface. SDK 11 / C# 15 are the canonical sample configuration rather than a new-syntax requirement. No older-target package route or general performance claim is established.

## Evidence

Both actual VectorLanes projects compile and execute exact zip/unzip order, all four concat selections, reverse and geometric/alternating/floating-harmonic patterns. The same built DLLs also pass with DOTNET_EnableHWIntrinsic=0 and report accelerated=False. Saved compilation/execution records identify SDK 10.0.402 / C# 14 and SDK 11.0.100-rc.2.26504.105 / C# 15 and exact canonical inputs. This establishes Windows x64 behavior and the software route; it is not native Arm/AVX-512 execution, mocks, cross-compilation, a benchmark or GA proof.
