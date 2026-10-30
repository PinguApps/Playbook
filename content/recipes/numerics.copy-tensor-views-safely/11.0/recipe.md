# Copy tensor views with explicit storage ownership

## Goal

Transform a fictional survey grid without overwriting values that another input
view still needs. Choose independent output or an explicit snapshot when source
and destination views share storage. Separate tensor objects do not establish
separate ownership of the underlying array.

**Full-release preparation: successful current storage-contract execution is
pending under PIN-962.** The merged .NET 11 release-branch implementation defines
the intended contract below. Published tensor RC2 and RC3 do not implement it;
their successful compilation is not behavior proof. Pingu approved preparing this
guide while retaining pending proof and the release-ready packaging gate.

Use console projects with implicit System imports, nullable enabled,
`using System.Numerics.Tensors;` and namespace `Playbook.Samples`. Canonical
comparisons use net10.0/C#14 with tensor10.0.13 and net11.0/C#15 with tensor11RC2
for compilation only. The intended current behavior requires the final .NET 11
package containing release/11.0 backport #135281, followed by actual verification.

## Older approach

The grid importer already takes a logical snapshot before copying into shared
storage. Flattening into an independent array reads the view in logical order,
including gaps in its physical storage. The second copy cannot overwrite unread
source cells. For addition, an allocating result separates output from both inputs.
These explicit ownership choices are valid with the measured .NET 10 package.

{{snippet:older}}

The original grid backing values are `[13,29,47,61,83,97]`. A `[2,2]` source with
strides `[3,1]` contains logical values `[13,29,61,83]`. Copying the snapshot into
the first four cells produces `[13,29,61,83,83,97]`, retaining the intended logical
values rather than just avoiding an exception. A shared two-input addition uses
`[13,29,47,61]` and `[29,47,61,83]`, producing `[42,76,108,144]` independently.

Do not rely on the older documentation's general snapshot-style overlap promise.
Actual earlier implementations can depend on traversal order. The safe policy
remains useful without changing packages or assuming any new rejection behavior.

## Newer approach

Keep explicit ownership when moving to the intended final package. The library
rejects unsupported overlap before writing instead of silently allocating a full
snapshot. `TryCopyTo` and `TryFlattenTo` can throw for layout errors even when
their shape and capacity checks pass. Their `false` result has a narrower meaning.

{{snippet:newer}}

When a caller must reuse output storage for an elementwise operation, snapshot
**each** overlapping input before the first write, then operate on those snapshots.
Snapshotting only the terrain input would still leave the correction input exposed
to overwrites. The allocating `AddIndependent` route avoids that responsibility.
Use independently addressable output for repeated-storage destinations; input
snapshots cannot make one physical cell hold multiple distinct output values.

## Migration

1. Inventory each input's backing storage, starting position, lengths and strides,
   plus the destination's storage. Shared backing arrays can overlap even when
   shapes, objects or view variables differ. Strides must be nonnegative; use a
   reversal operation rather than inventing a negative-stride view.
2. Prefer an allocating result when the caller does not require a specific output
   buffer. Otherwise flatten every overlapping input into its own array before
   writing, reconstruct its view with the original lengths, and then operate.
   The sample's checked conversion bounds snapshots to ordinary managed arrays;
   allocation and available memory remain explicit application costs.
3. Retain the permitted operation-specific cases below. Do not ban every overlap
   or assume that dense storage permits every destination-taking operation.
4. Treat `false` from Try copying as insufficient capacity or, for TryCopyTo,
   incompatible shape. Validate configuration separately from layout exceptions.
   Test successful capacity/shape with unsupported overlap and verify every backing
   cell, including unused gaps, remains unchanged when rejection occurs.
5. Replace outputs with a repeated zero-stride dimension of length greater than
   one. Preserve valid zero strides on singleton dimensions and empty outputs.
   Snapshotting inputs does not resolve repeated destination storage.
6. Supply compatible `System.Array` elements. Writable views must use actual `T[]`
   storage rather than covariant reference arrays. A compatible read-only view
   can retain covariance; copying to a new `T[]` is a valid writable alternative.
7. Select and execute the actual final package containing the backport. Refresh
   pins, lockfiles, requirements and sample evidence together through PIN-962.
   Do not use this guide to infer that an RC package already enforces the contract.

## Constraints

| Operation | Intended permitted shared storage |
| --- | --- |
| CopyTo / TryCopyTo | Dense source and destination with equal element counts, or exactly identical views, including nonrepeating strided views. |
| FlattenTo / TryFlattenTo | Dense source overlapping the flat destination span. |
| Unary, binary and tensor/scalar elementwise output | An input/output view with identical origin, lengths and strides and no repeated logical locations. Check each input independently; shifted and overlapping broadcast inputs are rejected. |
| ResizeTo | Dense source and destination overlap. Growing clears added logical elements; shrinking discards trailing elements. Unsupported strided overlap is rejected. |
| Reverse / ReverseDimension into the same view | Dense in-place reversal or a no-op reversal, such as an unchanged singleton axis. Other overlapping strided reversals are rejected. |

ResizeTo and destination-taking concatenation reject repeated logical output
locations caused by zero strides on nonsingleton dimensions. Empty destinations
and singleton zero strides are allowed. Both System.Array constructor overloads
on TensorSpan and ReadOnlyTensorSpan reject incompatible element types with
ArrayTypeMismatchException. Writable covariance is rejected; compatible reference-
type covariance remains allowed for read-only views.

The copy/flatten family includes IReadOnlyTensor and corresponding Tensor,
TensorSpan and ReadOnlyTensorSpan methods. Destination-taking elementwise,
resize, reversal and concatenation have their own layout checks. There is no
switch restoring arbitrary overlap. The separate TensorPrimitives APIs keep
their existing overlap contract; do not extrapolate this table to them.

The canonical current route targets the final .NET 11 framework/package. The
behavior is delivered by System.Numerics.Tensors, so a package update can affect
its other supported targets; this recipe establishes no newer-package-on-net10,
netstandard or .NET Framework multidimensional route. C#15 is the sample mode,
not a new syntax requirement. Actual compilation types have no experimental
annotation or required opt-in. No provider, rendering mode, native compiler,
workload, service or account is required. Measured previous execution is Windows
x64 CoreCLR JIT; other platforms and final artifacts remain unverified.

## Evidence

`eng/Record-GridTransform.ps1 -VerifySources` verifies ten immutable source hashes,
executes the original ownership/Try false controls with SDK10.0.402, C#14,
runtime/ref10.0.13 and tensor10.0.13/lib/net10.0, and compiles the full current
suite using SDK11.0.100-rc.2.26504.105, C#15, runtime/ref11RC2 and locked
tensor11RC2/lib/net11.0. A supplementary `--policy-only` execution checks safe
ownership and Try false controls on the current artifacts; it does not run the
expected-release storage contract. Commands, inputs, logical values, dependency closure and
compiler/reference/package hashes are captured through the existing sample workflow.

The committed expected-release suite covers all four copying methods through
read-only/writable/tensor/interface wrappers; before-write rejection of unsupported
overlap; dense/identical supported operations; each-input/broadcast cases; all
ResizeTo input wrappers; both reversal forms; repeated/singleton/empty output;
both Array constructor forms and covariance. It verifies corrected snapshot and
independent-output values, rather than treating lack of exceptions as success.

Actual RC2 and latest published RC3 accept unsupported strided copies in all four
copy/flatten methods, and RC2 fails the expected dense shifted-copy result.
Those negative qualifications explain the pending execution gate. No successful
current storage contract is claimed. PIN-962 owns final artifact selection,
`Record-GridTransform.ps1 -RunReleaseChecks -VerifySources`, all behavior results
and evidence refresh; only success can clear pending proof. Ordinary installed
stdio list/get checks served content and applicability, not that missing package
behavior. PIN-959, actual-GA, release-ready and Human gates remain open; the older
comparison adds no ninth .NET 10 foundation.
