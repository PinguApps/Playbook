# Match native C structure and union layouts

## Goal

Exchange a fictional Moss Shoal depth packet with a C instrument library. Use
runtime C aggregate layout when the native contract is a C struct or union, and
verify the actual ABI (the platform's binary calling and data-layout rules).
This is not a portable serialization format or permission to reinterpret arbitrary bytes.

## Older approach

.NET 10 can match a known ABI with explicit sizes and field offsets. This remains
valid on .NET 11. The following is specifically the measured MSVC Windows x64
contract, not a set of offsets to carry unchanged to every architecture.
Add `using System.Runtime.InteropServices;` and put these types in the
`Playbook.Samples` namespace when combining them with the consumer below.

{{snippet:older}}

## Newer approach

.NET 11 adds ExtendedLayoutAttribute with CStruct and CUnion algorithms. The
compiler emits extended-layout metadata (LayoutKind.Extended / TypeAttributes.ExtendedLayout)
and the runtime computes field layout. Do not combine the attribute with manual
FieldOffset declarations. Struct fields are placed in declaration order with C
alignment; union members overlap. These framework contracts are not marked
experimental in the tested RC2. No preview language switch or diagnostic suppression
is required to use the API. The sample suppresses CA1051 only because the analyzer
does not recognize the new aggregate-field contract.

{{snippet:newer}}

Copy either layout into a separate file with the imports/namespace above. Copy
this complete consumer into another file. Enable AllowUnsafeBlocks in the project.
For example, Revise(new Packet { Kind = 3, Depth = -12.25, Count = 9 }, fullPath)
returns kind 4, depth -11.75 and count 11. The helper owns no retained pointer.

{{snippet:consumer}}

This is the entire matching C helper, built as a Windows x64 DLL. Its local
size_t/offset definitions deliberately avoid SDK headers and the C runtime;
unsigned __int64 and long long are the measured 64-bit MSVC types. _fltused
satisfies MSVC's floating-point marker without linking a CRT. This is not a
general cross-platform C build recipe.

{{snippet:native}}

## Migration

1. Establish the actual compiler, target architecture, packing switches and native
   declarations. Use a .NET 11 SDK/runtime and net11.0. The sample declares C# 15.0
   and AllowUnsafeBlocks=true, and inherits nullable/implicit usings from
   samples/Directory.Build.props. Enable those settings in a standalone project.
   .NET 10/C# 14 uses the older
   explicit types. Neither route adds a managed NuGet package, provider or workload.
2. Build the helper with the existing compiler and linker by absolute path. From
   the repository, eng/Test-InteropSoundings.ps1 performs these exact operations:
   cl.exe /nologo /c /GS- /TC /Fo<output>/soundings.obj samples/net11.0/NativeSoundings/soundings.c,
   then link.exe /NOLOGO /DLL /NOENTRY /NODEFAULTLIB /MACHINE:X64
   /OUT:<output>/soundings.dll <output>/soundings.obj. The runner parameters select
   the absolute executables; it never modifies PATH. Copy that DLL beside the
   managed output before running. Supply an absolute DLL path to the consumer.
3. Replace the old explicit Packet/Reading declarations with CStruct/CUnion only
   when the native ABI matches. Preserve the explicit implementation for older
   targets or native contracts whose packing does not match the runtime algorithm.
4. Compare native sizeof, __alignof and field offsets with managed size, field
   addresses and nested aggregate alignment. On this ABI the packet is 24 bytes,
   alignment 8, offsets 0/8/16; the union is 8 bytes, alignment 8, both offsets 0.
   The included checks verify these independently for both actual runtimes.
5. Exercise real pointer round trips for the struct and each union interpretation.
   The native protocol must identify which union member is meaningful. Keep the
   native library loaded and managed storage valid throughout every call.
6. Reject a mismatched size/alignment/offset before invocation. The negative control
   uses a packed 13-byte struct and never passes it to native code. Re-run the ABI
   checks after changing compiler options, native headers, architecture or runtime.

## Constraints and alternatives

Extended layout does not infer pointer ownership, string encoding, native packing
pragmas, endianness, wire format, discriminants or safe field access. Do not read
an inactive union member as though it were a tagged value. This bounded example
uses blittable numeric fields and pointer parameters; it does not establish
by-value aggregate calling conventions, reference-containing aggregates, bitfields,
flexible arrays or every compiler's special cases.

Only Windows x64 CoreCLR with MSVC19.51.36260.0/linker14.51.36260.0 is executed.
Other ABIs, Mono, Native AOT and final GA are unverified. Keep a manual explicit
layout when it expresses a different known contract; no universal ABI claim follows
from this example. No rendering integration, provider or persistent setup is needed.

## Evidence

eng/Test-InteropSoundings.ps1 -Record -VerifySources builds/runs SDK10.0.402,
reference/runtime10.0.13 and SDK11.0.100-rc.2.26504.105,
reference/runtime11.0.0-rc.2.26504.105. Both actual reference hashes match the
runtime-security audit. Saved sample-evidence records include exact input hashes,
commands, empty managed dependency sets, loaded CoreLib and C compiler/linker/helper
identities. Pinned official paired contracts are in interop-soundings.sources.json.

The real native helper proves sizeof/alignment/offset agreement, struct mutations,
double/integer union mutations and safe rejection of the packed negative control.
The canonical consumer is executed too. The ordinary packed tool is installed via
an isolated local feed/cache and inspected from an unrelated directory through real
offline stdio list/get for ID/title/goal, complete Markdown, canonical snippet
identity, target11/edition11, applicability and current evidence.
No recurring platform matrix is added. PIN-959/PIN-962, existing Human gates and
the exact eight .NET10 foundations remain unchanged.
