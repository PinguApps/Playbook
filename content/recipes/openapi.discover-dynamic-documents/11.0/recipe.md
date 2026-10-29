# Generate contracts for dynamically named API documents

## Goal

Discover every OpenAPI document actually offered by an application when names come from configuration, then produce the corresponding runtime and build-time contracts. The fictional planetarium separates visitor show listings from staff projector information. Each description has its own title and exactly one endpoint. Description membership is not authorization: the local example deliberately leaves both handlers callable. Protect staff endpoints and document access separately in a deployed service.

## Older approach

On the actual .NET 10 package, read the configured names during startup and call AddOpenApi(name, options) for each. That valid approach produces both documents at runtime and at build time. Keep it when names are already known before service registration finishes. The actual OpenApi 10.0.13 assembly has neither IAdditionalOpenApiDocumentNameResolver nor public AddOpenApiCore; this recipe does not invent a downlevel equivalent or backport the .NET 11 package.

## Newer approach

On the tested .NET 11 RC2 package, AddOpenApiCore() registers generation services independently of a particular name. Implement the **singular** IAdditionalOpenApiDocumentNameResolver and register it in DI to provide names. Configure each name separately with Configure<OpenApiOptions>(name, callback). The resolver returns names; it does not configure title, endpoint inclusion or serialization. There is no AddOpenApiCore(options) overload.

{{snippet:focus}}

The complete example selects a small configuration-driven audience set. Its normal resolver returns public and staff without calling AddOpenApi for either. The explicit control adds public with AddOpenApi and an extra title transformer while its resolver returns only staff (twice). The actual built-in provider combines that registration with the resolver's sequence and emits one public and one staff document. Options callbacks compose in registration order; the explicit public transformer appends its marker after the shared title transformer.

## Migration

1. Recreate the complete files below in their separate directories. Use the exact SDK and package pins to reproduce these observations. Restore once to create a lock file, inspect the resolved closure, then use RestoreLockedMode=true. The saved repository locks resolve Microsoft.OpenApi 2.12.2 and 3.10.2 respectively. Do not use the root tool's package policy for samples.
2. Retain the .NET 10 loop of AddOpenApi registrations as the working older comparison. For .NET 11, replace that loop with AddOpenApiCore(), the resolver registration and separately named options. Keep names lowercase, nonempty and safe for URLs and filenames; validate configuration before returning it. These fixed fictional names need no external account or discovery service.
3. Keep MapOpenApi and both endpoint registrations in every host, including build-time startup. With PLANETARIUM_CASE unset, a normal run checks /openapi/public.json and /openapi/staff.json over real loopback HTTP, then exits. Public must contain only /planetarium/shows with title Planetarium public; staff must contain only /planetarium/projector with title Planetarium staff. Both use OpenAPI 3.0. Unknown names return 404.
4. Generate the real build documents with the commands below. Expect PlanetariumContracts_public.json and PlanetariumContracts_staff.json with those same titles and paths. Rebuild and use a fresh output directory for each configuration so cached or stale files cannot masquerade as current discovery. The explicit --openapi-version OpenApi3_0 tool argument aligns build serialization with the named runtime options; the tested older tool otherwise generates 3.1 despite runtime options selecting 3.0. Inspect actual files rather than inferring format from configuration.
5. On .NET 11 invoke Generate.ps1 with -Case duplicate, -Case missing and -Case explicit; each invocation uses a fresh temporary output directory. Duplicate yields exactly two files. Missing yields only public even though staff options still exist, and the runtime staff document returns 404. Explicit still yields two files, with the public title ending in (registered). Empty and null-name are deliberate invalid configurations: actual generation must fail. Never ship those branches as configuration handling.
6. Run the case control normally on .NET 11. Its actual provider returns public, staff, PUBLIC, while HTTP /openapi/PUBLIC.json resolves the lowercase public options. Do not generate case-only filename variants on a case-insensitive filesystem. Normalize and validate names at the application boundary instead of relying on provider deduplication. Review the resulting contract before distributing it.

### Run and generate

Save Generate.ps1 beside the reconstructed .NET 11 project and invoke it with PowerShell 7 (the canonical source is stored as Generate.ps1.txt). It selects its own sample directory and the absolute executable for its pinned SDK:

{{snippet:generate}}

For the older project use this corresponding Generate.ps1:

{{snippet:generate-old}}

 Its normal and missing controls execute the explicit-registration alternative with the actual older dependency set. The repository's eng/Test-Planetarium.ps1 automates these builds, all newer controls and input-hashed evidence capture. No persistent environment change is needed.

## Constraints

The pinned provider concatenates registered names with every additional resolver sequence, then calls Distinct() with the default string comparer. Exact duplicates disappear; case-only variants remain distinct in provider enumeration. Runtime name membership is ordinal case-insensitive and generation uses lowercase named services/options. A null element is not sanitized: the actual GetDocument path fails with NullReferenceException. An empty overall sequence fails with an unable-to-find-registered-documents error. Missing one name does not fabricate its document from configured options. These are measured built-in-provider behaviors, not a promise that arbitrary third-party providers share them. No guarantee is made for ordering as a public contract, hot-changing name sets, arbitrary characters or case-colliding output files.

Build-time generation starts the real application's startup with GetDocument's mock server. It preserves route registrations, named options and transformers. ContractChecks detects GetDocument.Insider only to let the tool own host lifetime instead of running loopback checks against that mock server. Generated JSON is produced by the real package tool, not a handcrafted result. Keep startup free of production service calls and other resource side effects.

This goal differs from openapi.generate-for-environment (PIN-826): that recipe chooses the generation process's environment. Here the host offers multiple named descriptions in one configuration. Both concerns can coexist, but setting an environment alone neither enumerates names nor configures their documents. No typed-client, parser, browser, AOT, trimming, native workload or additional OS matrix is claimed.

## Evidence

The previous sample uses SDK10.0.402, C#14, framework/runtime10.0.13, OpenApi10.0.13, ApiDescription.Server10.0.0 and Microsoft.OpenApi2.12.2. The current sample uses SDK11.0.100-rc.2.26504.105, C#15, framework/runtime11.0.0-rc.2.26504.105, matched OpenApi/ApiDescription.Server11 RC2 and Microsoft.OpenApi3.10.2. Compilation and execution records retain actual SDK/compiler/runtime identities separately from selected package assets and ZIP hashes. OpenApi selects lib/net10.0 or lib/net11.0 respectively; both Microsoft.OpenApi model packages select lib/net8.0. ApiDescription.Server supplies build props/targets and tools rather than a runtime library. Package version numbers alone do not establish a target framework. The source audit's previous ApiDescription.Server10.0.13 was metadata-inspected; the working older consumer here deliberately uses the existing 10.0.0 build-tool baseline and executes it. Different package archives are not described as byte-equal.

Consumer prerequisites are .NET/ASP.NET Core11, compatible OpenAPI runtime and build-tool assets exposing these APIs, named options, a supported managed server platform and restore access. The shown source selects C#15; its older alternative selects C#14. Exact verification pins are not minimum supported versions. Execution was measured on Windows x64 with Kestrel, the built-in document provider and System.Text.Json. No native toolchain, database/cloud provider or browser rendering mode is required. There is no experimental API or warning opt-in. RC2 preparation does not establish actual-GA execution; PIN-959, PIN-963/PIN-964 and existing Human gates remain open. Exactly eight approved .NET10 foundations and all existing recipe/edition identities are unchanged.

Both samples have fresh compile-and-run records. Separate contracts.evidence.json records retain real tool commands, configuration inputs, generated-file checks/hashes, runtime checks and the resolved package closure. The current record also covers duplicate/explicit names, missing/empty/null failures and ordinal provider versus HTTP lookup. Installed local-feed stdio list/get verifies complete canonical Markdown, target11/edition11, applicability and evidence with unusable proxies from an unrelated directory. That establishes proxy-independent offline consultation, not network isolation or a deployed service's security.

## Complete canonical projects

Preserve the relative layout: the older project links the shared ContractChecks.cs from the newer directory. The sample projects deliberately use the base SDK plus an explicit ASP.NET Core framework reference; no Razor or client assets are needed. Empty local build files isolate them from tool-host inheritance. OpenApiGenerateDocuments=false keeps ordinary runtime checks separate from the explicit generation commands above.

### samples/net10.0/PlanetariumContracts

**Directory.Build.props**

{{snippet:n10-directory-build-props}}

**Directory.Build.targets**

{{snippet:n10-directory-build-targets}}

**Directory.Packages.props**

{{snippet:n10-directory-packages-props}}

**global.json**

{{snippet:n10-global-json}}

**PlanetariumContracts.csproj**

{{snippet:n10-planetariumcontracts-csproj}}

**Program.cs**

{{snippet:n10-program-cs}}

### samples/net11.0/PlanetariumContracts

**Directory.Build.props**

{{snippet:n11-directory-build-props}}

**Directory.Build.targets**

{{snippet:n11-directory-build-targets}}

**Directory.Packages.props**

{{snippet:n11-directory-packages-props}}

**global.json**

{{snippet:n11-global-json}}

**PlanetariumContracts.csproj**

{{snippet:n11-planetariumcontracts-csproj}}

**Program.cs**

{{snippet:n11-program-cs}}

**ContractChecks.cs** (shared by both projects)

{{snippet:checks}}

Official references: [core/named registration](https://github.com/dotnet/aspnetcore/blob/4a70870818bf5662f21b0c28f9a8c267dccb77f0/src/OpenApi/src/Extensions/OpenApiServiceCollectionExtensions.cs), [resolver](https://github.com/dotnet/aspnetcore/blob/4a70870818bf5662f21b0c28f9a8c267dccb77f0/src/OpenApi/src/Extensions/IAdditionalOpenApiDocumentNamesResolver.cs), [provider enumeration](https://github.com/dotnet/aspnetcore/blob/4a70870818bf5662f21b0c28f9a8c267dccb77f0/src/OpenApi/src/Services/OpenApiDocumentProvider.cs), [GetDocument host/tool](https://github.com/dotnet/aspnetcore/blob/4a70870818bf5662f21b0c28f9a8c267dccb77f0/src/Tools/GetDocumentInsider/src/Commands/GetDocumentCommandWorker.cs), [runtime lookup](https://github.com/dotnet/aspnetcore/blob/4a70870818bf5662f21b0c28f9a8c267dccb77f0/src/OpenApi/src/Extensions/OpenApiEndpointRouteBuilderExtensions.cs), [OpenAPI generation](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/aspnetcore-openapi?view=aspnetcore-11.0).
