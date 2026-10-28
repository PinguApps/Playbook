# Keep a derived image helper rendering versioned markup

## Goal

Update a custom ImageTagHelper when moving MVC or Razor Pages to ASP.NET Core 11: use the supported three-service constructor and remove reads of the retired HostingEnvironment and Cache properties.

The fictional FernPosters application keeps the same observable outcome across the comparison. This is a source/binary migration caused by removed APIs, not a new application feature.

## Older approach — .NET 10

A print archive renders a fern poster with a content-versioned image URL and an encoded collection label. Its .NET 10 derived helper forwards five services and reads the old protected properties, reproducing a legacy extension with deliberate local obsolete-warning suppression. The framework image processing did not use those properties. The supported three-service constructor was already available on .NET 10; existing extensions using it are valid.

Create the following files in a standalone FernPosters directory. The empty Directory files prevent unrelated parent build/package rules from changing the example. With the declared SDK, run `dotnet restore FernPosters.csproj`, then `dotnet run --project FernPosters.csproj --configuration Release` from that directory.

### global.json

{{snippet:n10-global-json}}

### FernPosters.csproj

{{snippet:n10-fernposters-csproj}}

### Directory.Build.props

{{snippet:n10-directory-build-props}}

### Directory.Build.targets

{{snippet:n10-directory-build-targets}}

### Directory.Packages.props

{{snippet:n10-directory-packages-props}}

### Program.cs

{{snippet:n10-program-cs}}

### PostersController.cs

{{snippet:n10-posterscontroller-cs}}

### FernImageTagHelper.cs

{{snippet:n10-fernimagetaghelper-cs}}

### Views/_ViewImports.cshtml

{{snippet:n10-views-viewimports-cshtml}}

### Views/Posters/Index.cshtml

{{snippet:n10-views-posters-index-cshtml}}

### wwwroot/posters/fern.svg

{{snippet:n10-wwwroot-posters-fern-svg}}

## Newer approach — .NET 11

Forward IFileVersionProvider, HtmlEncoder and IUrlHelperFactory to the supported base constructor. Call base.Process to retain URL resolution and versioning, then add the collection attribute through TagHelperOutput. MVC constructs the derived helper during real Razor rendering; no mock encoder or version provider is substituted.

Use a separate directory for this target. Restore/build/run with the same commands as above, using these files and the declared .NET 11 SDK. The program performs its focused checks and exits; it is a reproducible consumer, not a persistent production host.

### global.json

{{snippet:n11-global-json}}

### FernPosters.csproj

{{snippet:n11-fernposters-csproj}}

### Directory.Build.props

{{snippet:n11-directory-build-props}}

### Directory.Build.targets

{{snippet:n11-directory-build-targets}}

### Directory.Packages.props

{{snippet:n11-directory-packages-props}}

### Program.cs

{{snippet:n11-program-cs}}

### PostersController.cs

{{snippet:n11-posterscontroller-cs}}

### FernImageTagHelper.cs

{{snippet:n11-fernimagetaghelper-cs}}

### Views/_ViewImports.cshtml

{{snippet:n11-views-viewimports-cshtml}}

### Views/Posters/Index.cshtml

{{snippet:n11-views-posters-index-cshtml}}

### wwwroot/posters/fern.svg

{{snippet:n11-wwwroot-posters-fern-svg}}

## Migration

1. Find derived ImageTagHelper constructors and usages of the retired HostingEnvironment and Cache properties. Remove the two unused constructor dependencies and property reads as shown. Rebuild affected libraries as well as the host.
2. If your own extension actually needs an environment or cache for independent work, inject and retain that service in your own field. Removing a base property does not remove your extension's responsibilities. Delegate image versioning to IFileVersionProvider.
3. Enable the derived helper in Views/_ViewImports.cshtml. This standalone sample imports only FernPosters; in an existing app with built-in MVC helpers imported, remove Microsoft.AspNetCore.Mvc.TagHelpers.ImageTagHelper from this view scope before adding the derived replacement, so both do not process the same image.
4. Create the wwwroot/posters/fern.svg asset and Razor view as shown. The SVG is decorative: the migration requires a physical file at that path, not this particular artwork. Keep the original src, alt and asp-append-version markup. The custom marker selects this derived helper; Process removes that marker from the result.
5. Restore, build and run from the chosen FernPosters directory. The application requests /posters through real MVC/Razor, checks the resolved /posters/fern.svg?v= URL, encoded Fern &amp; Moss attribute and alt text, and verifies helper-only attributes are absent before shutdown.

## Constraints

- MVC and Razor Pages Tag Helpers only; this constructor migration does not apply to Blazor img elements.
- Removed members are the five-argument constructor, protected HostingEnvironment and protected Cache. The three-argument constructor is not a new .NET 11 invention. Arbitrary precompiled subclasses must be rebuilt.
- Both built-in and derived image helpers can match the same element when imported together. Import the replacement deliberately; the sample imports only its own assembly. For selective coexistence, choose mutually exclusive targets or scopes.
- Versioning requires the real web-root file provider to find the image. A missing local asset may keep an unversioned URL. This sample tests generated markup with a local file, not browser image loading, a CDN or production cache invalidation.
- Use matching ASP.NET Core framework/reference packs. No extra package, workload, service account or database is required. SDK 10.0.402 / 11.0.100-rc.2.26504.105 and C# 14 / 15 are the actual sample pins, not minimum language-feature requirements.
- Stable migration APIs; no experimental opt-in. The exact .NET 11 evidence is RC2. This recipe exposes target 11 only; the older sample is a fair previous-release comparison and not an additional .NET 10 foundation.

## Evidence

Real MVC Razor activation resolves the derived helper's services and renders a versioned src, encoded custom attribute and retained alt text on both baselines. The saved `verification-evidence.json` beside each project records actual matching-SDK compilation and execution, runtime output, commands and input hashes. No mock, cross-compilation or source-only result is being presented as execution. Build-time freshness checks validate those inputs before embedding the recipe.

The original obsolete usages fail compilation against .NET 11; the migrated examples retain the intended outcome on the pinned artifacts. Official sources establish removal intent separately from those real consumer checks. Windows x64 loopback/managed execution establishes only the named behavior, not GA, browser, NativeAOT, trimming or every supported platform.

The installed-package Reqnroll `MvcMigration` scenario retrieves all three migrations using real stdio list/get from an isolated feed/cache and unrelated working directory, compares the full expanded canonical body and applicability, checks target-11 selection and rejects target-10 detail. Invalid proxy settings exercise proxy-independent retrieval; this is not OS-level network isolation.

- [Official ASP.NET Core 11 migration and removed APIs](https://learn.microsoft.com/en-us/aspnet/core/breaking-changes/11/image-tag-helper-obsolete-members-removed)
- [Pinned official migration source used for RC2 preparation](https://github.com/dotnet/AspNetCore.Docs/blob/ecf7aca5136aea60aa8db7330f66ac799d3edcf7/aspnetcore/breaking-changes/11/image-tag-helper-obsolete-members-removed.md)
- [Supported image-helper construction in the pinned framework source](https://github.com/dotnet/aspnetcore/blob/4a70870818bf5662f21b0c28f9a8c267dccb77f0/src/Mvc/Mvc.TagHelpers/src/ImageTagHelper.cs)
