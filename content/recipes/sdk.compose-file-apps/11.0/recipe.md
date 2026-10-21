# Grow a file application with shared code and explicit dependencies

## Goal

Calculate free rack slots in a fictional depot while moving the arithmetic into reusable code. Choose source inclusion, a project reference or an existing DLL deliberately. Keep package versions and build policy visible as this utility grows.

## Older approach

The include directive was serviced into SDK 10.0.300; it is not exclusive to .NET 11. The actual older comparison uses SDK 10.0.402, net10.0 and C# 14. Put Rack.cs beside slots.cs, and begin slots.cs with a shebang line `#!/usr/bin/env dotnet` before the source below. Run `dotnet run --file slots.cs -c Release -- --reserve 4`.

{{snippet:older}}

{{snippet:helper}}

The helper has no top-level statements. With ImplicitUsings and Nullable enabled, it checks that the reserved count is within the capacity and prints `free=8`. If shared code needs its own project boundary, .NET 10 also supports a handwritten project directive:

{{snippet:older-reference}}

Create RackLibrary/RackLibrary.csproj as an SDK library targeting the same framework, with ImplicitUsings and Nullable enabled. Compile Rack.cs there, rather than including it in the caller too. The committed library project links the same original helper; no second implementation is maintained.

## Newer approach

.NET 11 adds quoted values and trailing MSBuild item metadata. This independent variant records an assembly company containing spaces and accesses the pinned parser through an extern alias. Keep the same Rack.cs and the leading shebang. Run the same --reserve command; output is `free=8;company=Estuary Depot Tools`.

{{snippet:newer}}

A prebuilt library can instead be included as a reference in .NET 11, without a feature flag. Build RackLibrary/RackLibrary.csproj in Release first, then run dll-route.cs:

{{snippet:newer-reference}}

To maintain project references with the CLI, use `dotnet reference add --file slots.cs RackLibrary/RackLibrary.csproj`, `dotnet reference list --file slots.cs` and `dotnet reference remove --file slots.cs RackLibrary/RackLibrary.csproj`. These commands edit project directives. Do not simultaneously include Rack.cs and the library defining the same type. Direct DLL inclusion does not build the producer or restore its transitive dependencies.

## Migration

1. Start with sdk.run-file-utility when the utility is still one file. Use SDK 10.0.300 or later in the .NET 10 servicing line for source inclusion; the tested older baseline is 10.0.402.
2. Move only supporting declarations to Rack.cs. Use a literal include for this bounded example. Globs and additional file types have their own build/cache consequences.
3. Choose one owner for each type: included source, project reference or compiled assembly. Keep the referenced project's target compatible with the caller.
4. With the pinned package directive shown here, disable inherited central package management deliberately. Alternatively remove the version from the package directive, keep CPM enabled and declare System.CommandLine 2.0.1 in Directory.Packages.props. Both were actually exercised.
5. Restore once to create packages.lock.json, review it, then build with `-p:RestoreLockedMode=true`. The package contributes its net8.0 managed assets on both targets. Restore needs the dependency or a prepared cache; offline Playbook retrieval does not bundle that package.
6. Use quoted property values and the matching Aliases token under SDK 11. Included files may repeat sdk/property/package directives only when values match. A conflicting duplicate or colon in a property name is rejected; do not reinterpret a colon as configuration nesting.
7. Keep a net10.0/C# 14 consumer on those settings when using SDK 11 conditionally. The separate ComposedUtilitySdk11 sample compiles with RC2 and executes through a real .NET 10 host; it is not a C# 15-on-.NET-10 claim. That conditional proof covers source inclusion and quoted package/property metadata; DLL/reference-editing examples are verified on target 11, not asserted for this older-target route.

## Constraints

The source-include backport does not establish .NET 10 support for DLL includes, quoted metadata or reference --file editing. The full newer route requires SDK 11; the conditional target-10 route also requires SDK 11 and C# 14. A runtime alone cannot interpret these directives. The .NET 11 #ref token appears in RC2 metadata notes but is not advertised as a standalone supported route by the file-app reference; this recipe uses the documented DLL include instead.

File applications inherit shared build/target, package and NuGet configuration. The nearest samples/ boundaries here keep production analyzer/framework/package settings out; an unrelated consumer must inspect its own inheritance. PublishAot=false keeps this example managed. No provider, UI/rendering mode or native workload is required. The parser and SDK are pinned for reproducibility; RC execution is not GA reconciliation.

## Evidence

Actual older/newer file builds and the SDK-11/net10.0 route have separate saved records. The command check executes source, project and DLL routes, reference add/list/remove, invalid capacities, matching/conflicting directives and inherited CPM. It uses the actual files and library, not a simulated SDK parser. The packed-tool harness compares the entire rendered body with these canonical sources and verifies applicability. No Native AOT or transitive DLL deployment claim is made.
