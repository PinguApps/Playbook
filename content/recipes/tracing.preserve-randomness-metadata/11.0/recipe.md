# Preserve trace randomness independently of recording

## Goal

Hearth Archive forwards importer context between catalogue and indexing stages.
A recording decision must preserve the incoming trace ID and its randomness
metadata. Recorded is bit 1; RandomTraceId is bit 2. Neither implies the other.

## Older approach

.NET 10 can preserve unknown incoming flag bits while changing only Recorded.
This remains a valid forwarding approach. It has no named RandomTraceId or
HasRandomizedTraceId API, and its default roots do not advertise the new bit.
Do not invent an older randomness detector or replace the flags with just 0/1.

{{snippet:older}}

Copy the complete type into a nullable, implicit-usings net10.0/C#14 console
project. No packages, workloads or external provider are required. Given a
validated W3C traceparent, call `ActivityContext.TryParse(header, traceState,
true, out var parent)`, then `ArchivePropagation.SetRecorded(parent, false)`.
Forward both `ArchivePropagation.Format(context)` and `context.TraceState`.
The formatter creates a version-00 traceparent from an already valid context;
it is not an arbitrary-header validator or a future-version parser.

## Newer approach

.NET 11 names the metadata and exposes the Activity convenience property.
The forwarding algorithm still changes just the recording bit.

{{snippet:newer}}

Use a net11.0/C#15 nullable, implicit-usings console project. Call
`ArchivePropagation.HasRandomId(context)` for an ActivityContext, or the Activity
overload for a live activity. The framework supplies DiagnosticSource; this
route needs no package, exporter, collector, UI/rendering mode or opt-in. The
reviewed APIs are not experimental. Exact RC2 artifacts below are evidence pins,
not proof that GA has been checked or a backport package route exists.

## Migration

1. Parse the received header with ActivityContext.TryParse. Reject malformed
   input instead of forwarding a default/invalid context. Preserve TraceId,
   SpanId, TraceState and the remote marker when changing a context locally.
2. Preserve all incoming flags; change only Recorded using bitwise OR/AND.
   The four supported combinations are neither (00), recorded-only (01),
   random-only (02) and both (03). Do not test whole-flag equality to Recorded
   when deciding whether an activity is recorded.
3. For actual sampled children, create an ActivityListener for the source and
   return PropagationData or AllDataAndRecorded from its Sample callback. Call
   `source.StartActivity("catalogue", ActivityKind.Internal, parent)` and dispose
   the result. The listener can clear/set Recorded while the parent's randomness
   bit propagates. The current check exercises both ActivityContext and string
   parent overloads, and direct Activity.SetParentId propagation. A listener may
   return None, or be absent, in which case StartActivity can return null.
4. Read HasRandomizedTraceId to interpret metadata, separately from Recorded.
   It reports the bit, not an independent audit of an untrusted upstream
   generator. An incoming assertion is only as trustworthy as its producer.
5. Leave default root generation alone unless a custom generator is required.
   The measured .NET 11 default generator marks new W3C roots random, including
   roots that are not recorded. The .NET 10 default root is unmarked. This
   automatic benefit alone is not the authored migration; forwarding and
   interpretation are the reusable changes.
6. A custom Activity.TraceIdGenerator does not inherit the default generator's
   promise. The deterministic fixture returns a fixed ID and remains unmarked.
   Do not set RandomTraceId for deterministic/custom IDs without a generation
   contract establishing randomness. A few apparently varied IDs cannot prove
   distribution quality. A custom generator using random bytes is also not
   automatically marked by this runtime path.
7. Own global changes exclusively. In a dedicated test process, save and restore
   TraceIdGenerator, Current, DefaultIdFormat and ForceDefaultIdFormat in finally;
   dispose activities, source and listener. Never swap a process-wide generator
   concurrently with unrelated application work.

## Constraints and alternatives

This is in-process propagation and interpretation guidance. It establishes no
collector, OpenTelemetry exporter, network protocol interoperability or sampling
probability. Keep tracing source/rule configuration in the separate
`tracing.configure-source-rules` guidance. Recording controls collection; the
randomness bit describes generation metadata. Neither is a security credential.

Keep the older bit-preserving approach if named APIs are unavailable. The new
API route requires .NET 11; no current API on net10 or independently upgraded
DiagnosticSource package route is claimed. Formatting preserves the demonstrated
flags, but does not promise retention by every downstream product.

## Evidence

`eng/Test-ExtensionMetadata.ps1 -Record -VerifySources` builds/runs both original
ArchiveStages consumers: SDK10.0.402/runtime10.0.13/C#14 and
SDK11.0.100-rc.2.26504.105/runtime11RC2/C#15, on Windows x64. Locked package
closures are empty. Records retain exact commands, input hashes, selected assets,
compiler/CoreLib and audit-matching DiagnosticSource references. No native
toolchain, device, account or Human action is required.

Actual parsing/formatting, Activity, ActivitySource and ActivityListener checks
cover 00/01/02/03, both recording decisions, inherited trace/span/state, string
and context parents, direct parents, default/custom roots, balanced callbacks
and cleanup. This proves observed metadata behaviour, not statistical randomness
quality or other-runtime/platform execution.

The ordinary packed tool is installed through an isolated local-only feed/cache
and inspected through real stdio list/get from an unrelated directory. Discovery
ID/title/goal, full canonical Markdown/snippets, target11/edition11 applicability
and fresh paired evidence are checked. Target10 returns no_applicable_edition;
its comparison adds no foundation. Unusable retrieval proxies establish proxy
independence, not OS network isolation. PIN-959/PIN-962, existing Human gates and
the exact eight approved .NET10 foundations remain unchanged/open. RC2 proof is
not final-GA reconciliation.
