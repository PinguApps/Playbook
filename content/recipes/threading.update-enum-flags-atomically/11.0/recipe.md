# Update shared enum flags atomically

## Goal

Let independent workers at fictional Slate Fen station set and clear their own
status bits without overwriting another worker's update. Use this when one shared
integer or enum represents independent flags. Each operation returns the value
that it replaced, which lets the caller observe whether its bit was already set.
That prior value is not a promise about the state after another worker runs.

Copy the snippet into a source file in a nullable/implicit-usings console project.
It includes its imports and the Playbook.Samples namespace. Call from the same
assembly; the example types are internal. For example, create a StationFlagStore
with StationState.Powered, then call Set(StationState.Camera) and
Clear(StationState.Camera). Both calls retain the unrelated Powered bit.

## Older approach

.NET 10 already provides integer Interlocked.And/Or overloads. Keeping int storage
and casting enum masks around those operations is a valid shorter alternative.
The comparison below instead shows the explicit compare-and-swap (CAS) loop when
maintaining an existing update implementation: read a candidate, calculate the
replacement, and retry using the actual prior value if another writer won.
Do not cast an enum ref to a differently sized integer ref.

{{snippet:older}}

This original net10.0/C#14 consumer uses only the framework. Its enum is int-backed;
the code does not claim a generic old implementation for every backing size.
It remains valid after upgrading. It adds no .NET 10 catalogue foundation.

## Newer approach

.NET 11 provides generic Interlocked.Or<T> and And<T> for integer primitive values
and integer-backed enums. Store the enum itself and pass its masks directly.
Or adds bits; And with the complement of a mask removes only those bits. Both
return the pre-update value, not the calculated replacement.

{{snippet:newer}}

The public constraint is struct, but that is not the full supported contract.
Use byte/sbyte, short/ushort, int/uint, long/ulong, nint/nuint or C# enums backed
by the eight fixed-width integer types. These cover 1/2/4/8-byte values; native
integer width follows the process. Arbitrary structs, decimal, Guid, float and
double are not supported. Do not infer support from size alone or from a type
satisfying the generic constraint. No unsafe caller code is needed for this route.

## Migration

1. Target net11.0 with a .NET 11 SDK/runtime. The canonical project declares
   C#15.0. No package, workload, provider, rendering mode or experimental opt-in
   is required. Exact test pins below identify the measured RC artifacts.
2. Check the real backing type. Preserve existing enum numeric values and masks.
   Replace only casts/CAS plumbing for supported integer or enum storage; retain
   the int-based implementation on .NET 10. No package backport is claimed.
3. Use Or(ref state, bitsToSet) and And(ref state, ~bitsToClear). Passing the clear
   mask directly to And would retain those bits and erase others. The enum
   complement keeps its declared backing width, including high/sign bits.
4. Treat the result as the prior snapshot. For an atomic read, this example uses
   Or with None, which does not change the value. All concurrent writes must use
   the same atomic discipline; ordinary read/modify/write operators such as |=
   can lose updates. Read() is a snapshot, not a reservation.
5. Keep any retry calculation free of externally visible side effects. The old
   CAS calculation may be repeated. Use a lock or another complete coordination
   design when multiple fields or a set-and-clear pair must change together.
6. Run real concurrent checks using distinct masks and an unrelated retained bit.
   Verify both final values and the possible serialized chains of returned prior
   values. Include signed/high-bit masks and unsupported-type rejection where
   a generic wrapper accepts types from callers.

## Constraints and alternatives

Atomicity applies to one location per call. Two calls, two fields, and a returned
snapshot plus a later action do not form a transaction. This does not enforce
business rules between bits or prevent another worker from changing the same bit.
Clearing an already absent bit and setting an already present bit are valid.
Use a lock for compound invariants, or keep existing integer atomics when enum
storage offers no benefit. No throughput, fairness or lock-free progress benchmark
is claimed; the generic implementation may itself use CAS internally.

The API is not marked experimental in the audited contract. Actual execution here
is Windows x64 CoreCLR on the paired baselines. Other architectures/runtimes are
unmeasured; .NET 11 RC2 evidence does not establish final-GA verification.

## Evidence

eng/Test-RuntimeCalls.ps1 -Record -VerifySources captures actual SDK10.0.402 /
runtime10.0.13 and SDK11.0.100-rc.2.26504.105 /
runtime11.0.0-rc.2.26504.105 consumers. Both build with warnings as errors, without
NuGet dependencies or workloads. Saved records contain exact input hashes,
commands, compiler/CoreLib identities and System.Runtime/System.Threading reference
hashes reconciled against the originating runtime-security audit. The bounded
runtime-calls source manifest pins paired official contracts/implementations.

Two dedicated worker threads execute 128 synchronized set/clear rounds. The checks
assert the final masks and both allowed chains of returned prior values. Another
40,000 mixed updates preserve an unrelated power bit and each worker's own
prior-bit state. The current consumer also exercises ten integer/native-integer
types, every C# enum backing type, high/sign bits, and rejection by both generic
operations of a custom struct, float, double, decimal and Guid without mutation.
These are execution checks, not a mathematical proof of all schedules.

The installed ordinary package supplies complete expanded snippets, explicit
target11/edition11 applicability and current saved compilation evidence offline.
PIN-959/PIN-962, pending Human gates and the exact eight .NET 10 foundations retain
their existing scope. No recurring native/OS matrix or production resource is added.
