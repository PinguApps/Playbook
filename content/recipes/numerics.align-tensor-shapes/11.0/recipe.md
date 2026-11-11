# Align tensor shapes without losing metadata

## Goal

Combine a fictional instrument's measurement batches when one input stores a
channel vector and another includes redundant leading dimensions. Preserve the
dimensions requested by the caller. Decide separately whether the instrument's
protocol requires exact stored dimensions or accepts the library's equivalent
shapes.

Verified on Windows x64 with GA SDK11.0.100/runtime11.0.0 and System.Numerics.Tensors11.0.0, containing release/11.0 backport #135281. Historical RC preparation did not establish this proof.

Use an SDK console project with nullable and implicit System imports enabled,
namespace Playbook.Samples and `using System.Numerics.Tensors`. Add
System.Numerics.Tensors: the older comparison pins10.0.13; the intended current
route requires the final11.0 package containing the backport. SDK10/C#14 and
SDK11/C#15 are canonical sample configurations, not new syntax requirements.

## Older approach

The instrument has three channels. Its original policy accepts only a dense
three-element vector with optional leading singleton padding. It validates that
bounded shape and explicitly indexes each channel, using zero for every padded
axis. The caller's stored destination dimensions remain `[1,1,3]`. This is a valid
deliberate normalization policy on .NET 10, independent of reshape/alignment rules.

{{snippet:older}}

Inputs `[4,7,9]` with shape `[3]` and `[2,5,8]` with shape `[1,3]` yield
`[6,12,17]`. A `[3,1]` shape is outside this instrument policy: the helper rejects
it rather than silently discarding an internal dimension. This bounded helper
does not emulate every tensor operation, implement general broadcasting or
restore all historical library behavior.

## Newer approach

The final-release implementation aligns redundant leading singleton dimensions
across operations. Add can accept `[3]` and `[1,3]` and write the same sums into
the requested `[1,1,3]` destination. Keep explicit validation when exact stored
metadata is part of the instrument protocol; the supplied RequireExact helper
rejects `[1,1,3]` when the required stored shape is `[3]`.

{{snippet:newer}}

The relevant rules are:

- A default rank-zero empty tensor/span computes as `[0]`. Its stored Rank remains
  zero and its Lengths/Strides remain empty. An explicitly ranked empty `[2,0]`
  keeps that shape.
- Only redundant **leading** singleton dimensions are ignored for shape equality:
  `[1,1,3]` and `[3]` can compare equal when their values agree. `[2,1]` and `[1,2]`
  remain different shapes, even though an elementwise operation can broadcast
  them to `[2,2]`. Equality is not general broadcasting.
- Zero dimensions are significant. Effective `[0]` can broadcast to `[2,0]`,
  whereas `[0]` and `[0,2]` have incompatible trailing dimensions for a binary
  operation. Empty values do not make every shape combination compatible.
- Stack and concatenate interpret the axis relative to the **first input's
  effective shape**. First `[1,3]`, second `[3]`, axis1 gives concatenate `[1,6]`
  or stack `[1,2,3]`. Reversing the inputs makes the first shape `[3]`; axis1 is
  then invalid for concatenation, while stacking at the end gives `[3,2]`.
- Alignment preserves source/destination shape metadata. If a protocol requires
  exact Rank/Lengths, compare those explicitly before the operation.

## Migration

1. Choose whether leading singleton padding is meaningful to your instrument.
   Keep the explicit older normalization policy if its restrictions are desired,
   or use the new alignment rules with a verified final package.
2. Add the final System.Numerics.Tensors11 package containing backport #135281 to
   the net11.0 project. Keep the selected package version/asset separate from the
   runtime version; this change is delivered by the package. Do not assume an
   SDK installation alone replaces an explicitly pinned package.
3. Use AddAligned with independent output storage. Test the original channel
   values, source dimensions and caller-provided output dimensions. Apply
   RequireExact first where exact stored dimensions form the external contract.
4. Review Broadcast, BroadcastTo, TryBroadcastTo, elementwise operations and copy
   paths using both default and explicitly ranked empties. TryBroadcastTo returns
   false for an incompatible destination; binary shape mismatch can throw
   ArgumentException. Preserve dimensions containing zero and internal singletons.
5. Review Concatenate/ConcatenateOnDimension and Stack/StackAlongDimension after
   any input reordering. Use the first input's effective rank to select the axis;
   stacking inserts an axis, concatenation uses an existing one.
6. Retest Reshape, Split, SqueezeDimension, Unsqueeze, PermuteDimensions, SetSlice,
   ResizeTo, Reverse/ReverseDimension and SequenceEqual where shapes or views
   cross an instrument boundary. Check logical values as well as dimensions.
   Empty slices have no element to read; retain their valid source storage origin.
7. Test EqualsAny, GreaterThanAny, GreaterThanOrEqualAny, LessThanAny and
   LessThanOrEqualAny for both scalar and pairwise operands. All should return
   false for compatible empty inputs and inspect late matches across the full
   logical range. Also test a missing match; an early positive cannot prove full
   traversal. Check IndexOfMax/MaxMagnitude/Min/MinMagnitude against known indices.
8. Keep native-width proof separate from small managed-array checks. The committed
   opt-in suite uses three bounded native byte buffers of2,147,483,656 elements
   each, with late sentinels beyond int.MaxValue. It measures free physical and
   commit memory before allocating about6 GiB and requires another2 GiB headroom.
   It checks all five scalar/pairwise Any families, copy/equality, reverse, views,
   multidimensional indexing and extrema; buffers are freed in finally. Insufficient
   memory or an unsupported authoring platform leaves this proof incomplete.
9. When changing the tested SDK/package, refresh sample pins, locks, declared
   dependencies and evidence together. Run the full suite through
   `eng/Record-InstrumentBatch.ps1 -RunReleaseChecks` and inspect the newly packed
   tool. The GA verification recorded below completed both checks before clearing
   pending proof. No recurring native/client/OS matrix is required.

## Constraints

This recipe assumes independent destination storage. Shape equivalence does not
authorize unsupported overlapping buffers, arbitrary zero-stride output layouts,
invalid axes or changed element types. Use snapshots/independent output and review
each operation's storage rules separately. There is no compatibility switch
restoring the previous inconsistent shape handling. Views can share storage;
retaining metadata does not grant ownership of that storage.

The application route requires final .NET11 framework/package support and the
selected net11 package asset. Verification uses actual SDK11 GA,
runtime/ref11.0.0 and tensor11.0.0 for compilation and execution; these are reproducibility
pins, not mandatory exact consumer installations. The .NET10 comparison
executes on runtime10.0.14 with tensor10.0.13. No newer-package-on-net10,
netstandard/.NET Framework multidimensional route, AOT/Mono or other-platform
execution is established here. The eight target10 foundation editions remain.

No provider, UI rendering mode, workload, account or experimental API opt-in is
required by the application APIs. Unsafe code, NativeMemory and Windows system
memory-status calls belong only to the optional authoring proof. That proof is
implemented and executed on Windows x64. Concurrent allocations can
still cause failure after a successful preflight; a failed allocation is not
successful verification.

## Evidence

PIN-962 ran `eng/Record-InstrumentBatch.ps1 -RunReleaseChecks -VerifySources` against actual SDK11.0.100/runtime11.0.0 and locked tensor11.0.0/lib/net11.0. All committed release checks passed. The older policy passed on SDK10.0.403/runtime10.0.14 and tensor10.0.13/lib/net10.0. Saved records retain actual commands, inputs, dependency closure and compiler/reference/package hashes. Historical RC evidence remains in Git; source review and compilation alone do not establish behavior.

The Windows x64 native-backed run covered 2147483656 elements after measured physical/commit preflight: 6442450968 allocated bytes, 2147483648 bytes headroom, 2m52s elapsed. All five scalar/pairwise Any families and native-width copy/equality/reversal/views/indexing/extrema passed. Small checks cover default/ranked empties, leading/internal/zero dimensions, equality/broadcasting, first-input axes and destination metadata.

The selected GA Tensor types have no experimental annotation or opt-in. No newer-package-on-net10, Native AOT, Mono or other-platform execution is established. Recipe proof is verified; the global stable candidate gate remains PIN-967. No release is authorized.
