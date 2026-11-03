# Keep Unix native-library packaging and callers aligned with output names

## Goal

Package and load the actual shared library produced by Native AOT. The original
TideWindow library computes a harbour clearance window: height 340 cm and required
clearance 300 cm give 80 minutes; height 290 gives zero. A real C process calls
the exported function rather than assuming that a managed assembly name is also
the native filename.

## Older approach

SDK 10.0.402, target net10.0, produces `TideWindow.so` on Linux with ordinary
`TargetName`. Existing exact-name packaging can consume this output. Preserve
that working route when retaining the old toolchain. Custom old builds can
already have prefixed names; inspect their outputs before migrating.

## Newer approach

Starting with .NET 11 Preview 3, non-executable Unix native outputs get a `lib`
prefix by default. The current Linux shared output is `libTideWindow.so`.
The corresponding macOS name is `libTideWindow.dylib`; macOS publication,
symbols, packaging and native invocation are **not-tested** here.

{{snippet:project}}

The original export has a stable explicit entry point:

{{snippet:export}}

Select exactly one intended library from a clean publish directory. Do not add
`lib` to an already prefixed filename. The packaging script accepts the publish
directory, a fresh destination, and `so` on Linux or `dylib` on macOS. It rejects
missing and ambiguous outputs and excludes debug companions. A fresh publish
directory prevents selection of a stale library.

{{snippet:package}}

The caller accepts that selected path, checks both numeric cases and retains
the loaded library until process exit:

{{snippet:caller}}

## Migration

1. Install the matching SDK/native packs and platform development tools on the
   authoring OS: Clang, linker and zlib development headers on Linux; Xcode command
   line tools on macOS. Select an explicit native RID, such as `linux-x64` or
   `osx-arm64`, matching that environment. Cross-OS RID selection is not execution.
2. Publish the matching project in Release mode to a new directory. Compare the
   actual output with the old exact filename. The Linux runner uses
   `dotnet publish -c Release -r linux-x64 -o <fresh-directory>`.
3. Run the canonical packaging script with that directory and a fresh destination.
   Inspect the packaged contents and exported `tide_window_minutes` symbol with
   the platform symbol tool. Update copy/load/link inputs to the selected path.
4. Compile the C caller (`clang caller.c -ldl -o caller` on Linux) and execute it
   with the absolute packaged library path. Expect the 80/zero calculation check
   to pass. macOS uses its native compiler/loader; its execution is not claimed.
5. If an external contract temporarily requires the old filename, publish the
   newer project with `-p:UseNativeLibPrefix=false` into another fresh directory.
   Verify the unprefixed artifact and native call independently. Prefer migrated
   consumers when the contract can change.
6. Remove temporary packages, containers and output directories after checking.

## Constraints

Only shared libraries are officially supported. The source-confirmed static `.a`
naming branch remains an **unresolved supported-delivery route**: static libraries
are unsupported and may require building Native AOT from source. No static
consumer or workaround is advertised. Reconcile that finding at actual GA.

Never unload a Native AOT library with `dlclose` or `FreeLibrary`. Executables and
Windows libraries are outside this Unix default-prefix condition; those branches
are source-reviewed, not executed by this Linux check. Self-contained native
libraries still need compatible OS/system libraries. The sample uses C# 14/15,
SDK 10.0.402/11 RC2 and explicit Linux x64 native packs; no optional provider,
rendering mode or workload is involved. RC2 evidence does not establish GA.

## Evidence

`eng/Test-TideAndMono.ps1 -Record` captures both actual Linux builds, default and
opt-out artifacts, symbol inspection, exact-old-name failure, packaged contents
and C invocation. Saved sample records retain input hashes, dependency closures,
SDK/OS/compiler/linker/package inventory and command output. macOS checks remain
explicitly not-tested under the [PIN-699 native testing policy](https://linear.app/pinguapps/issue/PIN-699). No native CI matrix is added.

Official references: [prefix migration](https://learn.microsoft.com/en-us/dotnet/core/compatibility/interop/11/nativeaot-lib-prefix)
and [shared-library support and unloading](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/libraries).
PIN-959 comprehensive coverage and PIN-963 actual-GA reconciliation remain open.
