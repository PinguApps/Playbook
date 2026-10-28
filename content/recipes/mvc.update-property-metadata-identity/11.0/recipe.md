# Preserve property identity in a custom metadata provider

## Goal

Migrate custom MVC model metadata creation to the PropertyInfo-based ModelMetadataIdentity.ForProperty overload on ASP.NET Core 11; retain property type, container, labels and accessors and rebuild dependent binaries.

The fictional BeaconLabels application keeps the same observable outcome across the comparison. This is a source/binary migration caused by removed APIs, not a new application feature.

## Older approach — .NET 10

A museum's exhibit card supplies a public caption to an editor. The .NET 10 custom provider builds a property identity from type, name and container, then supplies attributes and accessors for this one known model. A local warning suppression preserves the obsolete migration input. The reflection-based overload was already supported on .NET 10; code using it requires no replacement.

Create the following files in a standalone BeaconLabels directory. The empty Directory files prevent unrelated parent build/package rules from changing the example. With the declared SDK, run `dotnet restore BeaconLabels.csproj`, then `dotnet run --project BeaconLabels.csproj --configuration Release` from that directory.

### global.json

{{snippet:n10-global-json}}

### BeaconLabels.csproj

{{snippet:n10-beaconlabels-csproj}}

### Directory.Build.props

{{snippet:n10-directory-build-props}}

### Directory.Build.targets

{{snippet:n10-directory-build-targets}}

### Directory.Packages.props

{{snippet:n10-directory-packages-props}}

### Program.cs

{{snippet:n10-program-cs}}

### ExhibitMetadataProvider.cs

{{snippet:n10-exhibitmetadataprovider-cs}}

## Newer approach — .NET 11

Resolve the actual reflected property and pass that PropertyInfo with its model type and the intended container type. Keep attribute gathering and getter/setter behavior intact. The display-details provider continues to supply the public caption label. The proof checks the resulting MVC model metadata and the retained PropertyInfo identity rather than testing only a reflection lookup.

Use a separate directory for this target. Restore/build/run with the same commands as above, using these files and the declared .NET 11 SDK. The program performs its focused checks and exits; it is a reproducible consumer, not a persistent production host.

### global.json

{{snippet:n11-global-json}}

### BeaconLabels.csproj

{{snippet:n11-beaconlabels-csproj}}

### Directory.Build.props

{{snippet:n11-directory-build-props}}

### Directory.Build.targets

{{snippet:n11-directory-build-targets}}

### Directory.Packages.props

{{snippet:n11-directory-packages-props}}

### Program.cs

{{snippet:n11-program-cs}}

### ExhibitMetadataProvider.cs

{{snippet:n11-exhibitmetadataprovider-cs}}

## Migration

1. Locate custom metadata providers and factories calling ForProperty(Type, string, Type), including library binaries. Resolve the actual PropertyInfo for the intended public instance property, using nameof for a known model and handling a missing property explicitly.
2. Call ForProperty(propertyInfo, modelType, containerType). In this bounded sample modelType is property.PropertyType and containerType is the requested model. Preserve deliberate substitutions in an existing provider; do not blindly replace the container with DeclaringType for inherited properties.
3. Keep attribute discovery, custom display policies and property accessors. This one-model provider delegates other types to DefaultModelMetadataProvider; it is not a general replacement for MVC's reflection, inheritance or indexer handling.
4. Recompile the application and every dependent library that called the removed overload. Restore and run the complete BeaconLabels consumer below; it checks property name/type/container, the custom display name and working getter/setter. The .NET 11 check also asserts the retained PropertyInfo.

## Constraints

- Custom MVC model metadata providers, not arbitrary reflection utilities. The removed string/type overload is a source and binary compatibility break; recompile dependents.
- For inherited/hidden properties, choose the intended PropertyInfo and container deliberately. PropertyInfo.DeclaringType need not equal the model whose properties are being described. This sample uses one simple known model and proves no general inheritance or trimming strategy.
- The modelType argument describes the metadata's model type. This sample uses the property's declared string type; advanced providers may intentionally use another type and must retain that policy.
- Actual metadata, display label and accessors are exercised without rendering a form. No browser, validation integration, data provider, NativeAOT or trimming claim is made.
- Use matching ASP.NET Core framework/reference packs. No extra package, workload, service account or database is required. SDK 10.0.402 / 11.0.100-rc.2.26504.105 and C# 14 / 15 are the actual sample pins, not minimum language-feature requirements.
- Stable migration APIs; no experimental opt-in. The exact .NET 11 evidence is RC2. This recipe exposes target 11 only; the older sample is a fair previous-release comparison and not an additional .NET 10 foundation.

## Evidence

A real DefaultModelMetadataProvider subclass produces the expected property metadata, custom display label and working getter/setter on both baselines; .NET 11 also preserves reflected PropertyInfo. The saved `verification-evidence.json` beside each project records actual matching-SDK compilation and execution, runtime output, commands and input hashes. No mock, cross-compilation or source-only result is being presented as execution. Build-time freshness checks validate those inputs before embedding the recipe.

The original obsolete usages fail compilation against .NET 11; the migrated examples retain the intended outcome on the pinned artifacts. Official sources establish removal intent separately from those real consumer checks. Windows x64 loopback/managed execution establishes only the named behavior, not GA, browser, NativeAOT, trimming or every supported platform.

The installed-package Reqnroll `MvcMigration` scenario retrieves all three migrations using real stdio list/get from an isolated feed/cache and unrelated working directory, compares the full expanded canonical body and applicability, checks target-11 selection and rejects target-10 detail. Invalid proxy settings exercise proxy-independent retrieval; this is not OS-level network isolation.

- [Official ASP.NET Core 11 migration and removed APIs](https://learn.microsoft.com/en-us/aspnet/core/breaking-changes/11/modelmetadataidentity-forproperty-removed)
- [Pinned official migration source used for RC2 preparation](https://github.com/dotnet/AspNetCore.Docs/blob/ecf7aca5136aea60aa8db7330f66ac799d3edcf7/aspnetcore/breaking-changes/11/modelmetadataidentity-forproperty-removed.md)
- [Pinned framework metadata provider and reflected property identity](https://github.com/dotnet/aspnetcore/blob/4a70870818bf5662f21b0c28f9a8c267dccb77f0/src/Mvc/Mvc.Core/src/ModelBinding/Metadata/DefaultModelMetadataProvider.cs)
