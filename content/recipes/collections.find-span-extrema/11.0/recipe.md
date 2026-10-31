# Find extrema directly in an existing span

## Goal

A fictional observatory already holds instrument readings in a span. Find minimum
and maximum values without converting the buffer to an array or enumerable. Keep
its chosen ordering, null handling and empty-input behavior explicit.

## Older approach

An original .NET 10 loop uses an optional comparer, skips null elements and tracks
whether it has selected a value. This remains a valid buffer-based solution:

{{snippet:older}}

The helper needs a console/library project with implicit System and
System.Collections.Generic imports (ImplicitUsings enabled). It does not require
an allocation or a LINQ adapter. The branch direction chooses minimum or maximum;
empty non-nullable values throw while empty nullable/reference values return null.

## Newer approach

.NET 11 adds generic ReadOnlySpan<T> Min/Max, with default-comparer and explicit
IComparer<T> overloads. Replace the custom traversal while preserving the policy:

{{snippet:newer}}

The complete helper above compiles explicit generic static calls. `T?` is an
annotation for an unconstrained T; choosing T=int still returns int, whereas
T=int? returns Nullable<int>. It does not turn every value type into Nullable<T>.
The shared consumer also compiles extension syntax on an existing Span<int> and
ReadOnlySpan<string?>. These are MemoryExtensions operations, not Enumerable calls.

For readings 42,-7,18,105,3, default ordering returns -7 and 105. A reverse comparer
returns 105 as minimum and -7 as maximum. A custom Station class can instead be
ordered by load; it need not implement IComparable when a comparer is supplied.
A null comparer selects Comparer<T>.Default. Without a custom comparer, the type
must support the default comparison contract (IComparable<T> or IComparable).

Null elements are skipped even with a custom comparer: the comparer receives only
non-null candidates. All-null input returns null. An empty span of non-nullable
values (such as int) throws InvalidOperationException; an empty reference or
nullable-value span returns null. There is no sentinel zero substituted for a
missing numeric reading.

## Migration

1. Identify the existing buffer and ordering policy. Retain the .NET10 loop when
   that target must remain supported; no new framework Min/Max route is advertised
   there, and no additional .NET10 foundation is introduced.
2. Target net11.0 and include the complete current helper in its own source file,
   with Nullable and ImplicitUsings enabled. No extra package or preview switch.
3. Pass the existing span and select T explicitly in shared helpers. Remove only
   array/enumerable adapters previously needed for aggregation. The old example
   already avoids them; a replacement does not establish a speed improvement.
4. Supply the domain comparer, or null for the default. Check reverse ordering
   and reference objects so an accidental default-comparer substitution is visible.
5. Decide whether missing readings are nullable. Verify mixed-null, all-null and
   empty cases, including the non-nullable exception, before replacing validation.
6. Run both actual consumers and retain unchanged input buffers. Treat floating
   NaN ordering as separate work if the application needs it; it is not taught here.

## Constraints

Primary route: net11.0/.NET 11 shared framework APIs with canonical C#15. The older
comparison uses net10.0/C#14. No NuGet package, asset, provider, native UI/workload,
rendering mode, external resource or experimental opt-in. The two operations are
separate aggregations; no single-pass or measured performance claim is made.
No mutation or ownership transfer is needed. This is independent of bit sets and
key-based equality guidance.

Exact tested baselines are SDK10.0.402/reference-runtime10.0.13 and
SDK11.0.100-rc.2.26504.105/reference-runtime11.0.0-rc.2.26504.105. These pins identify
proof; release-family requirements describe selection. Windows x64 execution only;
no floating-specific/NaN, other-platform, performance or final-GA behavior claim.

## Evidence

`eng/Test-ChannelExtrema.ps1 -Record -VerifySources` compiles/runs both original
ObservatoryBatch consumers against actual artifacts. Checks cover value inputs,
reference objects, null comparer/default and explicit reverse/ordinal/load ordering,
a comparer that rejects null, all-null/mixed-null nullable and reference inputs,
empty non-nullable exceptions and empty nullable/reference null results.
The current consumer compiles generic static and mutable/read-only extension calls.

The existing evidence workflow retains commands, resolved dependencies (none),
measured SDK/compiler/CoreLib/runtime, input hashes and audit-matching reference
identities. Immutable official sources qualify API availability and the contracts.
The packed installed tool returns complete canonical Markdown, ID/title/goal,
target11/edition11, applicability and fresh evidence offline. RC2 proof is not GA;
PIN-959, PIN-962 and existing Human gates stay open. No recurring native/client
matrix is added.
