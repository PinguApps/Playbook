# Construct immutable VSTest assembly-resolution context

## Goal

A fictional Tern extension loader must load Tern.Plugin and its Tern.Rules dependency
from a controlled directory while retaining the real requesting assembly. Supply
RequestingAssembly at construction after its setter disappears. Verify actual
assembly resolution, missing dependencies and the platform's null-requestor path.

## Older approach

The audited SDK10 PlatformAbstractions DLL has a mutable RequestingAssembly setter.
**Public18.7.0 does not have that property at all.** Public18.9 already has the new
constructor. The source audit compares SDK builds, so package numbering cannot be
used as a substitute for its actual old contract. The old resolver below therefore
references exactly SDK10.0.402's PlatformAbstractions product18.7.0-release-26505-111,
SHA256 e52ae9db24b333b65ccb15173d2db1fabd944f7f861e1b9209e613710bf624b5.
This is a qualified historical direct-SDK consumer, not an independently stable
SDK-embedding guarantee or a nonexistent public-package18.7 route.

Save samples/TernTesting/Resolver/TernResolver.csproj; MSBuildBinPath resolves the
pinned SDK. Use this parent package boundary only for the declared sample layout:

{{snippet:old-resolver}}

{{snippet:old-packages}}

## Newer approach

The actual public18.11 contract has AssemblyResolveEventArgs(name,requestingAssembly)
and a read-only RequestingAssembly. The old single-name constructor remains and
leaves it null. Name is still mutable; the entire event object is not immutable.
No package-major-to-target-framework inference is needed.

Save the current resolver as samples/net11.0/TernHost/Resolver/TernResolver.csproj,
with its parent Directory.Packages.props:

{{snippet:new-resolver}}

{{snippet:new-packages}}

The actual current-package .NET10 route is also compiled and executed:

{{snippet:current10-resolver}}

{{snippet:current10-packages}}

Save samples/TernTesting/ResolverChecks.cs. LEGACY_PLATFORM selects only the actual
old SDK setter. The current path uses the two-argument constructor. The AppDomain
bridge receives the **real** plugin requestor, resolves only the allowlisted rules
assembly and returns null for unrelated names. A separate real PlatformAssemblyResolver
check demonstrates its .NET ALC path: ALC.Resolving does not provide a requestor,
so null is correct. Do not fabricate one or infer Framework behavior from this run.

{{snippet:resolver}}

Save samples/TernTesting/ResolverProgram.cs:

{{snippet:entry}}

The controlled original dependency graph uses these complete sources. Save Rules.cs
under rules-source and Plugin.cs under plugin-source in a disposable directory:

{{snippet:rules}}

{{snippet:plugin}}

Use Rules.csproj in rules-source and Plugin.csproj in plugin-source:

{{snippet:rules-project}}

{{snippet:plugin-project}}

## Migration

1. Inspect the actual resolver assembly, target and source contract. The public
   SDK10 source audit's setter differs from package18.7; do not silently mix its DLL
   into a current TestHost closure. Keep the old SDK comparison in its own process.
2. Create the projects/sources above. Pin the parent sample global.json to10.0.402
   or11.0.100-rc.2.26504.105 with rollForward=disable, and copy that pin beside
   each nested resolver project. Set LEGACY_PLATFORM only on the
   old SDK resolver project. Restore/build each resolver in Release and retain its
   generated lock. Current projects obtain the complete locked TestHost/ObjectModel
   closure; the old historical project copies just its actual standalone SDK resolver.
3. Build plugin-source/Plugin.csproj with the corresponding absolute SDK executable,
   Release and TernTargetFramework=net10.0 ornet11.0. The disposable fixture root has
   empty Directory.Build.props/targets/Directory.Packages.props so it inherits no
   application configuration. Its rules project is a real referenced library.
4. Copy only Tern.Plugin.dll to a clean deploy directory and only Tern.Rules.dll to
   deploy/rules. Do not copy the rules library beside the plugin, a deps file, or
   into the resolver's probing paths; otherwise loading would bypass the callback.
5. From an unrelated directory, execute the built TernResolver.dll with the intended
   absolute dotnet executable and the absolute deploy directory. Expect west:13 from
   actual plugin invocation, one rules resolution with requestor Tern.Plugin, a
   real FileNotFoundException for Tern.Missing, and an unresolved platform event with
   RequestingAssembly=null. Disposal must detach that platform event. The bridge
   also returns null for an unknown name and retains an explicitly absent requestor.
6. Run eng/Test-TernTesting.ps1 -Record to capture all three resolvers and real plugin
   graphs plus old setter compile success and current CS0200 failure. A constructed
   event object alone, reflection-only API presence or mock loader is insufficient.

## Constraints

The measured comparison is SDK **10.0.402**, C# **14**, runtime **10.0.13**
versus SDK **11.0.100-rc.2.26504.105**, C# **15**, runtime
**11.0.0-rc.2.26504.105**, on Windows x64. Public platform packages are pinned
independently: **18.7.0** and **18.11.0**, with NuGet-generated locks. Both CLI
packages actually ship a **net10.0** console, not a console TFM inferred from
package major numbers. TranslationLayer/TestHost/ObjectModel select their actual
**net8.0** assets; Build supplies **netstandard2.0**. The original adapter uses
matching Microsoft.NET.Test.Sdk/TestHost/ObjectModel. No workload, native compiler,
provider, rendering mode, device, credential or external service is required.

Public18.11 is a stable package set. The .NET11 SDK/runtime proof remains RC2;
no experimental opt-in is required for the replacement route. The SDK's bundled
18.7.0-release-26505-111 and 18.11.0-release-26504-105 files are separate artifacts,
not claimed byte-identical to public packages. The original package source commits
are 6f58ced50b40e074a07ffc21fb2d2eef95d31b60 and
36206450d47fec22e281ba9846445edc796f5080. Exact ZIP hashes, resolved asset paths,
compiler output, runtime output and SDK inventory are retained in sample evidence.

Only the original bounded Windows x64 consumers are qualified. No .NET Framework,
older runtime, other OS/architecture, arbitrary third-party adapter/collector,
remote execution, trimming, NativeAOT or general plugin-security promise follows.
No change to ordinary runner selection is needed. MTP is not assumed to be a
drop-in replacement. JSON API removal and pooled-session removal are separate facts;
neither is repaired by adding Newtonsoft.Json. Preserve existing working older
routes and the exact eight .NET10 foundations.

## Evidence

Run eng/Test-TernTesting.ps1 with -Record and the two absolute dotnet executable
paths to capture fresh verification-evidence.json records. The helper uses the
existing Evidence.ps1 input-hash/dependency workflow, fresh isolated CLI homes and
package caches, real locked builds, real console processes, real testhost/adapter
execution, compiler failure controls, plugin loading and effective-settings TRX.
It sets DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=0 in each child, checks process/user/machine
PATH and never edits persistent settings. Disposable raw output stays in artifacts/.
Compilation records are not mocks or cross-compilation. All source, shared config,
locked dependencies and capture inputs must remain unchanged when reusing proof.

Build and pack the ordinary Playbook tool, then use the Inspector walkthrough's
isolated local-feed installation. Real installed stdio list_recipes/get_recipe from
an unrelated directory must return this ID/title/goal, full canonical Markdown,
explicit target/edition, applicability and current evidence. Retrieval uses embedded
content; unusable proxies establish proxy independence, not network isolation.
PIN-959 qualifying coverage/evidence, PIN-963 actual-GA reconciliation and pending
Human gates remain open. This delivery authorizes no public publication.

## Sources

- [Publisher TranslationLayer custom discovery/execution contract](https://github.com/microsoft/vstest/blob/36206450d47fec22e281ba9846445edc796f5080/src/Microsoft.TestPlatform.VsTestConsole.TranslationLayer/README.md).
- [Current public console protocol wrapper](https://github.com/microsoft/vstest/blob/36206450d47fec22e281ba9846445edc796f5080/src/Microsoft.TestPlatform.VsTestConsole.TranslationLayer/VsTestConsoleWrapper.cs).
- [Current resolver event construction and requestor contract](https://github.com/microsoft/vstest/blob/36206450d47fec22e281ba9846445edc796f5080/src/Microsoft.TestPlatform.PlatformAbstractions/Interfaces/Runtime/IAssemblyResolver.cs).
- [Actual .NET resolver has no requesting assembly](https://github.com/microsoft/vstest/blob/36206450d47fec22e281ba9846445edc796f5080/src/Microsoft.TestPlatform.PlatformAbstractions/netcore/Runtime/PlatformAssemblyResolver.cs).
- [Current task quoting and explicit semicolon limitation](https://github.com/microsoft/vstest/blob/36206450d47fec22e281ba9846445edc796f5080/src/Microsoft.TestPlatform.Build/Tasks/TestTaskUtils.cs).
- [Public independently distributed console package](https://www.nuget.org/packages/Microsoft.TestPlatform.CLI/18.11.0).
- [Public translation-layer package](https://www.nuget.org/packages/Microsoft.TestPlatform.TranslationLayer/18.11.0).
- [Public test task package](https://www.nuget.org/packages/Microsoft.TestPlatform.Build/18.11.0).
- [SDK source baseline audited for actual compiler/tooling contracts](https://github.com/dotnet/dotnet/tree/a8dd0577c5c13f43a2f57fe33b4ff11b90f6371e).
- [Locked NuGet restore](https://learn.microsoft.com/en-us/nuget/consume-packages/package-references-in-project-files#locking-dependencies).
