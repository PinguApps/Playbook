# Reference Extensions correctly after shared-framework inclusion

## Goal

Celestial Planetarium's console scheduler exchanges show slots through nine
Extensions contracts. Remove references that the target framework now supplies,
while preserving packages required by older targets and concrete implementations.
These project edits apply to C#, F# and VB; the executable checks use C#.

## Older approach

A plain .NET10 console application needs these nine packages when it has no
ASP.NET shared-framework reference. Set these properties inside the project's
PropertyGroup, under `<Project Sdk="Microsoft.NET.Sdk">`:

{{snippet:older}}

Add these package references as a sibling of PropertyGroup inside that Project:

{{snippet:packages}}

The separate Directory.Packages.props enables central versions and pins **each** listed package
to `[10.0.13]`. With central management disabled, put `Version="[10.0.13]"` on
each reference instead. Nullable/implicit-usings settings are enabled in the sample.
The real consumer resolves IDistributedCache, IConfiguration, IServiceCollection,
IMetricsBuilder, IFileProvider, IHost, ILogger, IOptions<T> and StringValues from
their nine assemblies, then checks the dusk/midnight show slots. These are
contracts, not a configured cache, host, logger or dependency container.

## Newer approach

For a plain net11.0 project using only those contracts, keep the target properties
and remove those PackageReference entries. These properties remain inside the
project's PropertyGroup; no ASP.NET framework reference is needed:

{{snippet:newer}}

The exact moved assemblies are Microsoft.Extensions.Caching.Abstractions,
Configuration.Abstractions, DependencyInjection.Abstractions,
Diagnostics.Abstractions, FileProviders.Abstractions, Hosting.Abstractions,
Logging.Abstractions, Options and Primitives (all names have the
Microsoft.Extensions prefix). This is a closed list. Hosting.Abstractions does
not include the Hosting implementation; FileProviders.Abstractions does not
include PhysicalFileProvider. The checked implementation control retains
`Microsoft.Extensions.FileProviders.Physical` `[11.0.0-rc.2.26504.105]`, selects
its and FileSystemGlobbing's lib/net11.0 assets, and reads its own
assembly's file entry. Removing that implementation reference prevents compilation.

## Migration

1. Inventory each project's TargetFramework/TargetFrameworks, SDK/global.json,
   framework references, central versions and transitive dependencies. Separate
   project needs from the Playbook tool host, which remains on .NET10. Rebuild
   dependent libraries against the new contracts; do not infer binary compatibility
   from matching type names or keep obsolete binaries solely because they load.
2. For net11-only code, remove only the nine references proven redundant for that
   consumer. Restore and build, then inspect obj/project.assets.json: `compile`
   and `runtime` entries show actual selected assets. Do not infer selection from
   the package name or version alone. Refresh reviewed lock files deliberately.
3. Interpret NU1510 with the actual version syntax. On the pinned RC2 SDK,
   direct `10.0.13` minimum-version references produce nine NU1510 warnings;
   exact `[10.0.13]` references produce none. Both controls retain package entries.
   Minimum-version references have `_._` compile/runtime placeholders; exact pins
   retain lib/net10.0 DLL entries, then build conflict resolution still leaves no
   app-local Extensions DLLs and runtime loading uses the framework. Inspect these
   stages separately: a restore entry does not prove which assembly executes. With warnings
   treated as errors, fix unnecessary references rather than broadly suppressing
   NU1510. Absence of the warning does not prove a reference is needed.
4. Keep references needed by older targets. The original net10 consumer runs with
   its nine explicit packages; deliberately omitting them produces missing-type/
   namespace compiler errors. A multitarget library can condition its reference:

   Set `<TargetFrameworks>net10.0;net11.0</TargetFrameworks>` in a library project.
   Include `Microsoft.Extensions.Options` with `Version="[10.0.13]"` only when
   `Condition="'$(TargetFramework)' == 'net10.0'"` on the PackageReference is true.
   The net11 target receives IOptions<T> from its base framework. Reference the
   library through ProjectReference from the corresponding console consumers.

   Its Schedule.Label method accepts IOptions<string>; the real dual-target
   consumer passes Options.Create("dusk") and receives `show:dusk` on both runtimes.
   The SDK11 builds both library targets. The net10 consumer executes on runtime10,
   not on runtime11 via roll-forward. Unconditional references can also be valid
   for multitarget libraries; review selection per TFM rather than assuming a
   net11-only warning or deleting the older dependency.
5. Treat an intentional newer package as a separate decision. The checked
   `Microsoft.Extensions.Logging.Abstractions` `[11.0.0-rc.3.26522.148]` override
   selects a real lib/net11.0 package asset and loads its app-local RC3 assembly
   under the RC2 runtime. It is newer than RC2's pruning ceiling and is retained.
   This one executed ILogger consumer establishes that combination only, not
   general forward compatibility for every newer Extensions implementation.
   Do not remove a deliberate servicing/feature override just to reduce references.
6. Publish and execute the actual application. For the demonstrated
   framework-dependent publish, use `dotnet publish -c Release --self-contained false`
   with the appropriate absolute SDK executable from the sample's directory.
   Inspect the real output, .deps.json and .runtimeconfig.json, then run the
   published DLL with its declared runtime. The net10 control has nine loose
   Extensions DLLs and nine package entries; the net11 control has none and
   resolves all nine assemblies from Microsoft.NETCore.App. An absent loose DLL
   does not mean the type is absent. Single-file and self-contained publication
   have different layouts and are not established by this observation.

## Constraints and alternatives

Application requirements are SDK/runtime11 for the base-framework route, and a
compatible language/project system. Exact SDK10.0.402/runtime10.0.13 and
SDK11.0.100-rc.2.26504.105/runtime11RC2 identify the measured consumers; C#14/15
are their declared language modes, not a new syntax requirement for project edits.
The primary net11 consumer has an empty NuGet closure. Samples retain isolated
package/build boundaries and exact locks; no root update permission is broadened.
No workload, account, external provider, rendering mode, native toolchain or
experimental opt-in is needed. The audited contracts are not marked experimental.

Existing ASP.NET applications may already receive these abstractions through
their framework reference. That is a valid older arrangement; adding a large
framework reference solely to remove a few console packages is not required here.
Other Extensions packages remain separate. A type/namespace collision after
retargeting needs qualification or a using alias, not deletion of an unrelated
dependency. Review intervening behavioural changes when upgrading old packages.

## Evidence

`eng/Test-ExtensionIdentity.ps1 -Record -VerifySources` runs the actual old/new
consumers, missing-reference compile control, exact/minimum reference controls,
multitarget library/consumer, retained implementation and newer override. It
inspects real selected assets and package archive hashes, compiler/CoreLib and
audit-matching framework references, executes framework-dependent published
applications and verifies their dependencies, layout and loaded assemblies.
The old selected net10 package assemblies identify audit commit 4099f839; new
framework assemblies identify commit 820230c3. RC3 is confined to the override control.

The ordinary tool is packed, installed from an isolated local-only feed/cache
and inspected over real stdio from an unrelated directory. Discovery, full
canonical Markdown/snippets, target11/edition11, applicability and fresh paired
evidence must agree. Target10 returns no_applicable_edition; its comparison
consumer adds no foundation. Unusable retrieval proxies demonstrate proxy
independence, not network isolation. Windows x64 managed RC2 proof establishes no
other-platform, Native AOT, self-contained, single-file or final-GA deployment.
PIN-959/PIN-962 and existing Human gates remain open; the exact eight foundations
remain unchanged.
