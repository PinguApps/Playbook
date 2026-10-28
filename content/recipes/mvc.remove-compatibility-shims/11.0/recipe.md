# Start MVC after removing retired compatibility settings

## Goal

Migrate MVC startup to ASP.NET Core 11 by removing CompatibilityVersion registrations and retired option types; rebuild applications and dependent libraries that referenced them.

The fictional QuayArchive application keeps the same observable outcome across the comparison. This is a source/binary migration caused by removed APIs, not a new application feature.

## Older approach — .NET 10

The archive desk retrieves one crate by number. Its .NET 10 startup still calls the obsolete compatibility registration, with an explicit warning suppression solely to compile this historical migration input. These switches stopped changing MVC behavior in ASP.NET Core 3.0. A .NET 10 application already using AddControllers without the call needs no substitute.

Create the following files in a standalone QuayArchive directory. The empty Directory files prevent unrelated parent build/package rules from changing the example. With the declared SDK, run `dotnet restore QuayArchive.csproj`, then `dotnet run --project QuayArchive.csproj --configuration Release` from that directory.

### global.json

{{snippet:n10-global-json}}

### QuayArchive.csproj

{{snippet:n10-quayarchive-csproj}}

### Directory.Build.props

{{snippet:n10-directory-build-props}}

### Directory.Build.targets

{{snippet:n10-directory-build-targets}}

### Directory.Packages.props

{{snippet:n10-directory-packages-props}}

### Program.cs

{{snippet:n10-program-cs}}

### CratesController.cs

{{snippet:n10-cratescontroller-cs}}

## Newer approach — .NET 11

Register the MVC features the application actually uses and keep its controller routes. The .NET 11 consumer serves the same crate response without referencing any compatibility-version API. Do not add a replacement compatibility service: none is needed.

Use a separate directory for this target. Restore/build/run with the same commands as above, using these files and the declared .NET 11 SDK. The program performs its focused checks and exits; it is a reproducible consumer, not a persistent production host.

### global.json

{{snippet:n11-global-json}}

### QuayArchive.csproj

{{snippet:n11-quayarchive-csproj}}

### Directory.Build.props

{{snippet:n11-directory-build-props}}

### Directory.Build.targets

{{snippet:n11-directory-build-targets}}

### Directory.Packages.props

{{snippet:n11-directory-packages-props}}

### Program.cs

{{snippet:n11-program-cs}}

### CratesController.cs

{{snippet:n11-cratescontroller-cs}}

## Migration

1. Search the application and its referenced libraries for CompatibilityVersion, SetCompatibilityVersion, MvcCompatibilityOptions and ConfigureCompatibilityOptions<TOptions>. Check both IMvcBuilder and IMvcCoreBuilder registrations.
2. Remove compatibility registrations, obsolete enum/type usages and any service deriving from ConfigureCompatibilityOptions<TOptions>. Retain real MvcOptions configuration and the existing choice of AddMvc, AddControllers or AddControllersWithViews; the sample is an API-only consumer.
3. Retarget and rebuild the application and affected library binaries against the chosen framework. An unchanged precompiled dependent can still reference the removed members; do not ship it solely because the host builds.
4. Restore and run the complete sample below for either target. It starts real Kestrel on an ephemeral loopback port, requests /archive/crates/27, checks Crate 27 is ready, and stops. Repeat representative application requests after your migration.

## Constraints

- MVC source and binary migration only. Removing the no-op registration does not opt an ASP.NET Core 2.x application into equivalent behavior; a 2.x upgrade needs its own migration analysis.
- The removed surfaces include SetCompatibilityVersion for IMvcBuilder and IMvcCoreBuilder, CompatibilityVersion, MvcCompatibilityOptions and ConfigureCompatibilityOptions<TOptions>. Remove usages rather than inventing implementations under the retired names.
- The sample proves controller startup and one successful request. It does not prove every MVC option, view, formatter or third-party dependency is compatible. Recompile dependent binaries to avoid removed type/method references.
- Use matching ASP.NET Core framework/reference packs. No extra package, workload, service account or database is required. SDK 10.0.402 / 11.0.100-rc.2.26504.105 and C# 14 / 15 are the actual sample pins, not minimum language-feature requirements.
- Stable migration APIs; no experimental opt-in. The exact .NET 11 evidence is RC2. This recipe exposes target 11 only; the older sample is a fair previous-release comparison and not an additional .NET 10 foundation.

## Evidence

Real Kestrel startup and GET /archive/crates/27 return the same representative MVC response on both baselines. The saved `verification-evidence.json` beside each project records actual matching-SDK compilation and execution, runtime output, commands and input hashes. No mock, cross-compilation or source-only result is being presented as execution. Build-time freshness checks validate those inputs before embedding the recipe.

The original obsolete usages fail compilation against .NET 11; the migrated examples retain the intended outcome on the pinned artifacts. Official sources establish removal intent separately from those real consumer checks. Windows x64 loopback/managed execution establishes only the named behavior, not GA, browser, NativeAOT, trimming or every supported platform.

The installed-package Reqnroll `MvcMigration` scenario retrieves all three migrations using real stdio list/get from an isolated feed/cache and unrelated working directory, compares the full expanded canonical body and applicability, checks target-11 selection and rejects target-10 detail. Invalid proxy settings exercise proxy-independent retrieval; this is not OS-level network isolation.

- [Official ASP.NET Core 11 migration and removed APIs](https://learn.microsoft.com/en-us/aspnet/core/breaking-changes/11/mvc-compatibility-options-removed)
- [Pinned official migration source used for RC2 preparation](https://github.com/dotnet/AspNetCore.Docs/blob/ecf7aca5136aea60aa8db7330f66ac799d3edcf7/aspnetcore/breaking-changes/11/mvc-compatibility-options-removed.md)
