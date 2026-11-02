# Retain native exports with their source types

## Goal

Keep Tern Survey's native sounding export only when its associated source type
survives Native AOT compilation. EntryPoint chooses an exported symbol's name;
AssociatedSourceType controls whether that named export is retained. Named
exports already existed on .NET 10.

## Older approach

.NET 10 has no AssociatedSourceType field. A named UnmanagedCallersOnly export
is retained independently of an otherwise unused source type. Keep the build-time
feature decision and export declaration paired explicitly, or keep the named
export intentionally. This original older example uses the same KEEP symbol
for retaining the type and including its export. LEGACY_ALWAYS is a negative
control showing that EntryPoint alone still retains the export.

{{snippet:older}}

## Newer approach

Keep the export wrapper separate from the source type and associate it explicitly.
The current KEEP build roots an actual TernSurvey instance using GC.KeepAlive;
the TRIM build contains no such reference. Merely mentioning typeof in the
attribute must not itself retain the type, otherwise the condition would be useless.

{{snippet:newer}}

Both projects are AOT shared libraries. The current complete project is below.
TernVariant=KEEP or TRIM selects the control; TernMsvc/TernUm/TernUcrt and CppLinker
are explicit local toolchain inputs used by the runner, not general consumer
requirements. The sample inherits nullable/implicit usings from its parent.

{{snippet:project}}

This independent native Windows x64 caller loads the DLL, requires tern_root,
checks the conditional symbol and invokes tern_depth(35), expecting 42. An
UnmanagedCallersOnly method without EntryPoint has no named Unnamed export.
The test compiles EXPECT_DEPTH=1 for retained exports, 0 for trimmed exports.

{{snippet:native}}

## Migration

1. Use an actual supported Native AOT SDK/toolchain and a known native ABI. These
   projects use net10.0/C#14 or net11.0/C#15, win-x64 and NativeLib=Shared. Enable
   PublishAot and preserve the older project when older targets must be supported.
2. On .NET10 pair build-time export inclusion with the same feature decision that
   roots the source type. Do not add a nonexistent attribute field or describe
   EntryPoint as newly introduced.
3. On .NET11 apply AssociatedSourceType=typeof(TernSurvey) to the wrapper export.
   Retain the source type through real application behaviour. The sample's
   unconditional tern_root constructs an instance only in its KEEP variant.
4. Publish each variant separately. eng/Test-NativeBoundaries.ps1 supplies the
   absolute MSVC linker and SDK library paths without modifying PATH. Each
   publish sets TernVariant and a distinct PublishDir; the SDK recompiles the
   changed DefineConstants. Use locked restore for the exact ILC dependencies.
5. Inspect the produced DLL with link.exe /DUMP /EXPORTS. KEEP must include
   tern_depth; TRIM must omit it. Both must retain tern_root and omit Unnamed.
   The older LEGACY_ALWAYS control must retain tern_depth without the source root.
6. Compile loader.c with cl.exe /nologo /c /GS- /TC /DEXPECT_DEPTH=1 (or 0), then
   link with /NODEFAULTLIB /SUBSYSTEM:CONSOLE /ENTRY:mainCRTStartup and the matching
   kernel32.lib. Run beside the produced TernExports.dll. Exit zero establishes
   actual load, symbol checks and retained-entry-point invocation. Repeat after
   changing the trimming roots, SDK/compiler, dependencies or native ABI.

## Constraints

Only Windows x64 Native AOT is executed. This does not prove other platforms,
JIT export generation, dynamic unloading of individual managed types or ABI
compatibility of a replacement DLL. Keep the library loaded throughout native
calls; the caller releases it only after returning. A trimmed export is absent,
so callers must handle that feature absence before invocation.

UnmanagedCallersOnly methods are static, use blittable arguments and cannot be
called directly from managed code. Keep calling conventions consistent with the
native declaration and avoid exceptions crossing the boundary. This arithmetic
example has no managed references in its signature. No provider, rendering mode
or optional workload is involved. AssociatedSourceType is not an ownership,
security or per-instance-lifetime mechanism. Actual RC2 evidence is not GA proof.

## Evidence

eng/Test-NativeBoundaries.ps1 -Lane Windows -Record builds real SDK10.0.402 / ILC
10.0.13 and SDK11.0.100-rc.2.26504.105 / ILC11 RC2 artifacts. It captures locked
dependencies, MSVC/linker/SDK identities, audit-matching references, export tables,
DLL hashes and independent C invocation for five variants. Both previous/current
consumers compile and execute their actual native artifacts. Empty source-type
static initialization alone was insufficient to root the measured current type;
the canonical retained control roots a real instance.
Ordinary installed-package offline stdio list/get checks canonical snippets,
complete Markdown, discovery and applicability. PIN-959/PIN-962 and existing
Human gates remain open; the exact eight .NET10 foundations are unchanged.
