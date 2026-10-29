# Select native tool package RIDs from a measured toolchain

## Goal

Skylark packages a native dispatch command for systems it can actually build and
verify. A requested RID (runtime identifier) is not proof that its compiler exists.
Use the SDK extension point to choose the package set from a deliberate support
matrix, then inspect and install the real native payload.

## Requirements and actual artifacts

This edition demonstrates net11.0/C#15 NativeAOT packaging on Windows x64. The
native executable embeds its runtime. Its managed comparison and authoring SDK
still need the complete official runtime/SDK installations. No provider, rendering
mode, workload, public feed publication or production resource is involved.

| Input or route | Older comparison | Current example |
| --- | --- | --- |
| SDK / Roslyn | 10.0.402 / 5.9.0-1.26505.111 | 11.0.100-rc.2.26504.105 / 5.12.0-1.26504.105 |
| Managed target/language | net10.0 / C#14.0 | net11.0 / C#15.0 |
| Runtime/reference/ILCompiler | 10.0.13 | 11.0.0-rc.2.26504.105 |
| Declared package RID | win-x64 | win-x64 |
| Candidate defaults inspected separately | No new hook | win-x64; win-arm64 |
| Delivered native RID | win-x64, explicit per-RID pack | win-x64, selected outer/inner pack |
| Native linker/libraries | MSVC link 14.51.36260.0; tool directory 14.51.36231; Windows Kit 10.0.26100.0 | Same measured x64 tools/libraries |
| Arm64 route | Unsupported by this sample | Deliberately excluded; no cross-compilation or execution claimed |

NativeAOT's supported Windows prerequisites include Visual Studio with Desktop
development with C++ and the matching Windows SDK. The table is the exact measured
baseline, not a universal minimum. Supply an absolute **CppLinker** and three
library directories through `SkylarkMsvcLibraries`, `SkylarkUmLibraries` and
`SkylarkUcrtLibraries`. `IlcUseEnvironmentalTools=true` avoids automatic tool discovery
and child-shell PATH setup. No persistent PATH or shell integration is required.
The exact ILCompiler/ILLink packages and their complete closures are locked; sample
dependencies remain manually maintained. Current proof is RC2, not GA.

## Older approach

SDK10's native outer packaging orchestration does not offer this extension point.
Keep a valid explicit per-RID pack for a proven host instead of pretending the
new target exists. This sample's old command uses `dotnet pack -r win-x64` with the
measured linker/library arguments and produces the RID implementation package.
Run a second outer `dotnet pack` without `-r` into the same feed to create the
installable RID graph package; SDK10 does not orchestrate the native inner build.

Save as `samples/net10.0/SkylarkNative/SkylarkNative.csproj` and pin its
`global.json` to **10.0.402**, `rollForward: "disable"`:

{{snippet:older}}

## Newer approach

Save the current project under `samples/net11.0/SkylarkNative`, pinning
`global.json` to **11.0.100-rc.2.26504.105**, `rollForward: "disable"`:

{{snippet:newer}}

Both projects link `samples/net10.0/SkylarkNative/Program.cs`:

{{snippet:program}}

The supported hook is a target with
`BeforeTargets="ComputeToolPackageRuntimeIdentifiersToPack"`. Add one
`ToolPackageRuntimeIdentifiersToPack` item per supported RID while in the outer
build, where RuntimeIdentifier is empty. The SDK orchestrates one Pack inner build
for each selected item. Keep the outer/inner distinction and SDK-generated package
identities/layout; this is distinct from basic PackAsTool configuration.

When the custom item list is empty, SDK11 computes defaults. When separately evaluating both candidates on the measured
win-x64 host, its NativeAOT defaults include win-x64 and win-arm64. That is a build-capability heuristic, not confirmation of installed Arm
tools or a tested Arm machine. The original sample deliberately selects just
win-x64; it does not publish an empty placeholder for win-arm64.

## Migration

1. Establish a support matrix before packing. For every advertised RID, identify
   its actual compiler, native libraries/runtime and execution host. Mark unrun
   cross-builds separately; this example advertises only executed win-x64.
2. Create the shown tree and exact pins. Supply the measured link.exe through
   `-p:CppLinker=<absolute-link.exe>`, MSVC `lib/x64` through
   `-p:SkylarkMsvcLibraries=<absolute-directory>`, and Windows Kit `um/x64` and
   `ucrt/x64` through `SkylarkUmLibraries` and `SkylarkUcrtLibraries`. Pass separate
   properties so MSBuild escaping cannot turn three library paths into one.
3. Restore the real native compiler/runtime packages and retain the generated
   `packages.lock.json`; repeat with locked restore. Build the managed comparison,
   then run SDK11 `dotnet pack SkylarkNative.csproj -c Release -o <local-feed>`
   with those native arguments. SDK10 additionally needs `-r win-x64`.
4. Inspect each nupkg. Both release outputs must contain the outer
   `Skylark.Native.Tool` and the `Skylark.Native.Tool.win-x64` implementation at
   version1.0.0, with the generated RID graph identifying the selected package.
   Exactly one RID inner package is expected. Inspect the generated ToolPackage
RID entries in DotnetToolSettings.xml: only win-x64 is declared. No win-arm64
package may appear.
5. Install `Skylark.Native.Tool` from that local-only feed into a disposable
   `--tool-path`, using a fresh cache/home and `DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=0`.
   On SDK10 first create both packages as described above.
   Invoke its absolute `skylark-native.cmd` from an unrelated directory. Expect
   crates=21, architecture=X64 and dynamic=False. Inspect the stored payload PE:
   it must be a native AMD64 executable without a CLR header, not a dummy file.
6. Request win-arm64 explicitly and check SKYLARK001, then pass a nonexistent linker
   and check SKYLARK002. The sample rejects unsupported/missing toolchains early.
   Separately inspect SDK11 defaults with `SkylarkUseDefaults=true` and
   `SkylarkInspectCandidateDefaults=true` through target evaluation; do not pack their unverified Arm64 payload. Add a new RID only after
   supplying and verifying its real tools, artifacts and execution evidence.

## Constraints

The RID list controls what gets packaged; it does not install a compiler or make
cross-platform NativeAOT possible. Restrict restore RIDs and actual package graph
consistently with the supported matrix. An empty custom selection falls back to
SDK defaults; use an explicit error if your policy allows no deliverable RID.

This bounded sample verifies one declared RID out of two separately inspected candidates.
It makes no Arm, Linux, macOS, cross-build, startup-performance, arbitrary native
library or actual-GA claim. Real applications may need additional native dependency
and tool-install qualification. Keep all eight historical foundations and
PIN-959/PIN-963/Human gates unchanged. Local packing/installing grants no public
publication authority.

## Evidence

`eng/Test-SkylarkBuild.ps1 -Record` compiles/runs both actual managed projects,
uses real ILC and MSVC-link to pack native executables, checks package counts and
identities/RID JSON, installs into an isolated local tool directory and executes
the native payload on Windows x64. PE inspection distinguishes the native payload
from a managed launcher; actual unsupported/missing-toolchain cases fail.

Fresh sample records preserve exact commands, locks, resolved package closure,
SDK/compiler/native versions, package/payload hashes and outputs. Ordinary
build/pack checks their inputs. Installed offline MCP retrieval separately checks
the complete recipe, explicit target11/edition11, applicability and snippets.
Pinned SDK target source and official NativeAOT/tool-authoring references support
the extension point; evaluated defaults alone do not establish native execution.
