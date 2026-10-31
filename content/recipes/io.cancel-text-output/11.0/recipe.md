# Cancel asynchronous text output explicitly

## Goal

A fictional mooring report needs cancellation to reach its output writer. Stop
the operation cooperatively, while accepting that bytes already written remain.

## Older approach

.NET 10 already supports cancellation through the ReadOnlyMemory<char> overloads.
The original report helper uses them for text, a completed line and a blank line:

{{snippet:older}}

This is a valid approach to keep on .NET 10. AsMemory does not copy the string.
Calling the one-argument string overload instead would omit the operation token.

## Newer approach

.NET 11 adds string/token WriteAsync and WriteLineAsync, plus token-only
WriteLineAsync. Keep the same operation token at every call:

{{snippet:newer}}

The complete helper belongs in its own source file in a console/library project
with ImplicitUsings enabled. Supply an owned TextWriter, text and the operation's
CancellationToken; await completion before another write, flush or disposal.
The string conveniences delegate to the virtual memory/token overloads. The
token-only newline call writes the writer's configured newline through WriteAsync;
it does not dispatch through a custom WriteLineAsync override. These conveniences
do not strengthen a subclass's cancellation contract.

With a concrete UTF-8 StreamWriter whose NewLine is LF, text `Mooring 17: clear`
produces that text, ` | ready`, LF and another LF. Flush buffered output explicitly
when needed; completion of a write alone does not promise persistence or delivery.

## Migration

1. Keep the memory/token calls when supporting net10.0. The target11 recipe's older
   comparison does not add another .NET 10 foundation or advertise new APIs there.
2. For net11.0, include the complete current helper and pass the existing token.
   No NuGet package, preview-language switch or experimental diagnostic opt-in.
3. Choose encoding and NewLine on the concrete writer. Serialize access and keep
   its stream and input alive until the awaited operation settles.
4. Handle OperationCanceledException at the operation boundary. Do not report a
   cancelled report as complete or retry it into the same output automatically.
5. Test pre-cancellation and cancellation during an underlying write. Inspect the
   emitted prefix; do not infer cancellation from an immediate fake override alone.
6. Decide what to do with incomplete output. A separate temporary-file/commit
   design is needed for atomic publication; these overloads provide no rollback.

## Constraints

Primary route: net11.0/.NET 11 shared framework, canonical C#15. Older consumer:
net10.0/C#14. No packages, assets, providers, workloads, rendering modes, external
services or opt-ins. Actual proof uses Windows x64; other platforms and arbitrary
TextWriter subclasses are not executed claims. TextWriter.Null deliberately
ignores even pre-cancelled tokens in both tested releases.

The controlled stream sits beneath a real StreamWriter: its first byte write
succeeds and its second waits for the forwarded token. Cancellation leaves 128
accepted ASCII bytes from an 8192-character report at the operation boundary on
both tested runtimes. That count belongs to this fixture/buffer, not a general
limit. Buffering, encoding and the destination determine partial output; disposal
or a later flush can emit buffered data after cancellation. Await the failed task
before cleanup. Do not promise universal interruption or automatic rollback.

## Evidence

`eng/Test-TextBoundaries.ps1 -SampleNames MooringReport -Record -VerifySources`
captures both successful consumers with the existing evidence workflow. Real
StreamWriter checks cover complete text/newline output, no bytes after a
pre-cancelled operation plus flush, in-flight token forwarding and the retained
partial prefix. The Null writer exercises the subclass limitation.

Exact SDK/reference/runtime pins: 10.0.402 / 10.0.13 and
11.0.100-rc.2.26504.105 / 11.0.0-rc.2.26504.105. Compiler/CoreLib identities,
audit-matching reference hashes, commands, inputs and resolved dependencies (none)
are saved. These reproduction pins are separate from release-family prerequisites.
Pinned official TextWriter/StreamWriter sources establish availability and dispatch.
The ordinary installed package returns full canonical Markdown, applicability,
current paired evidence and explicit target11/edition11 offline. RC2 is not GA;
PIN-959/PIN-962 and existing Human gates remain open.
