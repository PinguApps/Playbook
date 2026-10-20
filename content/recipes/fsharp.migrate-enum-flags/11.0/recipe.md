# Migrate unsupported enum flag operations

## Goal

Replace rejected non-integer enum bit operations with integer-backed permissions.

## Older approach

The char-backed enum expressions compile on F# 10, but each of OR, AND and XOR throws NotSupportedException when executed. There is no working old char-backed flag behavior to preserve; preserve the intended independent permissions instead.

{{snippet:older}}

## Newer approach

F# 11 rejects the old expressions with FS0001. Integer power-of-two members provide actual combinable permissions: granted=3, mayUnload=true, toggled=Unload. All 64 pairs of three-bit values are checked for OR/AND/XOR.

{{snippet:newer}}

## Migration

1. Rebuild the original source under F# 11 and confirm FS0001 at the non-integer enum operation. The rejection is not preview-only.
2. Open System. Choose an integer underlying type, None=0 and distinct power-of-two values for independent permissions. Add Flags for intent/display and migrate the OR/AND/XOR operations.
3. If old characters were persisted or sent over a protocol, define an explicit boundary mapping before changing that contract. Do not reinterpret character code points as permission bits. Compare combinations, membership and toggling against the intended permission model.

Use .NET 11 SDK, net11.0 and FSharp.Core 11; exact evidence pins are SDK 11.0.100-rc.2.26504.105 and FSharp.Core 11.0.101-rc2.26504.105, selecting lib/net10.0. The comparison pins SDK 10.0.402 and FSharp.Core 10.1.402 (lib/netstandard2.1). Set LangVersion to 11.0 for the newer example and 10.0 for the comparison.

## Constraints

- FlagsAttribute alone does not repair a non-integer enum; the integer representation and bit values matter.
- Changing an existing public enum underlying type or persisted representation can break consumers. Fictional DockAccess is a replacement permission model; no automatic wire/data migration or already-working char semantics is claimed.

No provider, workload or rendering mode is required. This edition supports net11.0 on .NET-supported platforms. The net10.0 source is a historical comparison, not an advertised older-target route for the new feature.

## Evidence

Saved records contain exact build/run commands, actual outputs, resolved packages, authoring host and input hashes for both canonical projects. The real installed-tool harness checks full Markdown and applicability offline. Compiler and external-consumer checks run separately in eng/Test-FSharpBoundaries.ps1. This is actual managed execution on the recorded host, not a mock or cross-compilation. No IDE tooltip, trimmed/Native AOT publish, UI framework, all-platform or GA verification is claimed. RC2 evidence must be reconciled at GA.

Official sources: [Official F# release contract](https://github.com/dotnet/core/blob/main/release-notes/11.0/preview/rc2/fsharp.md); [F# implementation and tests](https://github.com/dotnet/fsharp/pull/20322).
