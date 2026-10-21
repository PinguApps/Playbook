# Package an installable command with the SDK tool contract

## Goal

Deliver a local berth-manifest command that a developer can install from a NuGet tool package and run outside its source checkout.

## Older approach

SDK 10 already generates a correct tool package from PackAsTool and ToolCommandName. Keeping that standard contract is a valid migration baseline:

{{snippet:older}}

Program.cs contains `Console.WriteLine("manifest=PIER-42");` for a minimal independent utility. The canonical example also prints assembly/base paths for the companion single-file check. It needs no external packages or service. Build under the SDK 10.0.402 pin and pack Release with `dotnet pack DeliveryTool.csproj -c Release -o <local-feed>`. That creates Pier.Manifest.Tool.1.0.0.nupkg; the package ID and installed command name deliberately differ.

## Newer approach

For standalone C# projects, add `<ImplicitUsings>enable</ImplicitUsings>` and `<Nullable>enable</Nullable>` inside the project PropertyGroup. The canonical comparisons obtain these settings from their isolated shared sample properties; do not depend on the Playbook checkout to supply them.

The corresponding SDK-11 project retains the SDK's tool packaging contract:

{{snippet:newer}}

Use SDK 11.0.100-rc.2.26504.105 to reproduce this RC2 verification. Leave NuspecFile unset. SDK 11 rejects combining PackAsTool=true with a custom NuspecFile using NETSDK1235. A nuspec used for an ordinary library package is not the tool settings/layout contract. Do not work around this diagnostic by switching off PackAsTool while still expecting dotnet tool install to recognise the output.

## Migration

1. Preserve the package identity and command users already invoke. Set PackageId, Version, PackAsTool=true and ToolCommandName in the SDK project; keep required package metadata.
2. Move metadata from a custom tool nuspec into supported project properties/items. Remove NuspecFile for the tool route. Let the SDK generate the tool package's tools/<TFM>/any payload and DotnetToolSettings.xml command mapping.
3. Pack locally in Release. For an isolated install create a NuGet.Config with `<configuration><packageSources><clear/><add key="local" value="ABSOLUTE_LOCAL_FEED"/></packageSources></configuration>`, using an XML-escaped feed path.
4. With a fresh child-process NUGET_PACKAGES cache and DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=0, run `dotnet tool install Pier.Manifest.Tool --version 1.0.0 --tool-path <absolute-tool-directory> --configfile <absolute-config> --no-cache`. This needs no global-tool or PATH change.
5. Execute the installed command by its absolute path from an unrelated working directory. Expect manifest=PIER-42. A successful pack alone does not prove that the package installs or that the command's entry-point/runtime requirements are correct.
6. Keep the companion single-file deployment profile separate: it sets PackAsTool=false for publish. A standalone published executable and a NuGet-installable tool are distinct deliverables.

## Constraints

The canonical consumers target net10.0/C#14 and net11.0/C#15 under their corresponding SDKs. The installed framework-dependent tool needs a compatible runtime. This edition declares SDK/target 11 and does not create a separate historical foundation or unverified older-target route. The example uses no external packages, native workload, provider, rendering mode or experimental opt-in. Proof is Windows x64 local install/run; other intended platforms still need appropriate verification. Local package creation is not permission to upload it to NuGet or create an account/resource.

## Evidence

Both original project targets compile; each real package is packed, installed through a fresh local-only feed/cache, then executed by absolute path outside the checkout. The newer negative pack actually returns NETSDK1235 for a custom NuspecFile. The older baseline demonstrates the supported SDK route, not a claim that an arbitrary historical custom nuspec was installable. The existing packed Playbook harness separately retrieves this recipe over real stdio, including its full body, canonical projects, goal and target-11 applicability. No mock install, external publication or GA claim is used.
