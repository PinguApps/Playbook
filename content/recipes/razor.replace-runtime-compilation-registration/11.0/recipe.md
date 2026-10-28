# Render MVC and Razor Pages without obsolete runtime-compilation registration

## Goal

The Moss observatory serves one MVC sightings view and one Razor Page. Keep both rendering while removing runtime-compilation registration that now emits ASPDEPR003 on recompilation. The default Razor SDK already compiles views during build and publish; development can use Hot Reload rather than a runtime compiler watching source files.

## Older approach — optional runtime compilation on .NET 10

The historical sample pins RuntimeCompilation 10.0.13 and calls both the parameterless MVC overload and the options-taking Razor Pages overload. A narrow ASPDEPR003 suppression keeps this obsolete comparison buildable with warnings as errors. Runtime compilation was already obsolete in .NET 10, and build-time compilation was already a valid alternative; this is not a requirement to install the package in a new application.

Save all files below in a separate MossViews directory with the indicated subdirectories. Restore once to generate packages.lock.json; retain the resolved lock for repeatability. Run from this directory so MVC/Razor Pages see the intended content root.

### Directory.Build.props

{{snippet:n10-directory-build-props}}

### Directory.Build.targets

{{snippet:n10-directory-build-targets}}

### Directory.Packages.props

{{snippet:n10-directory-packages-props}}

### global.json

{{snippet:n10-global-json}}

### MossViews.csproj

{{snippet:n10-mossviews-csproj}}

### Program.cs

{{snippet:n10-program-cs}}

### SightingsController.cs

{{snippet:n10-sightingscontroller-cs}}

### Views/Sightings/Sightings.cshtml

{{snippet:n10-views-sightings-sightings-cshtml}}

### Pages/Night.cshtml

Prepend `@page "/night"` as the first line, before the following canonical markup. Razor requires the page directive before HTML elements.

{{snippet:n10-pages-night-cshtml}}

## Newer approach — let the Razor SDK build the views

Keep AddControllersWithViews and AddRazorPages, and keep both endpoint mappings. Remove only AddRazorRuntimeCompilation and its package. There is no replacement service registration and no need to set RazorCompileOnBuild or RazorCompileOnPublish: the SDK defaults provide them. Do not retain an inherited false setting that disables compilation.

Save this complete project in its own directory. Running normally performs two real loopback requests and exits. Running with --serve keeps the server on port 5187 for authoring; choose another free port in the source if needed.

### Directory.Build.props

{{snippet:n11-directory-build-props}}

### Directory.Build.targets

{{snippet:n11-directory-build-targets}}

### Directory.Packages.props

{{snippet:n11-directory-packages-props}}

### global.json

{{snippet:n11-global-json}}

### MossViews.csproj

{{snippet:n11-mossviews-csproj}}

### Program.cs

{{snippet:n11-program-cs}}

### SightingsController.cs

{{snippet:n11-sightingscontroller-cs}}

### Views/Sightings/Sightings.cshtml

{{snippet:n11-views-sightings-sightings-cshtml}}

### Pages/Night.cshtml

Prepend `@page "/night"` as the first line, before the following canonical markup. Razor requires the page directive before HTML elements.

{{snippet:n11-pages-night-cshtml}}

## Migration

1. Find all AddRazorRuntimeCompilation calls, including options-taking overloads on MVC and MVC Core builders. Check launch settings and ASPNETCORE_HOSTINGSTARTUPASSEMBLIES for Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation; remove that activation as well. ASPDEPR003 is an obsoletion warning, not an experimental opt-in.
2. Remove the runtime-compilation package and configuration used only by it. Keep MVC/Razor Pages registrations and mappings. Review custom file providers or runtime-loaded views before removal: build-time compilation must include those view sources in the application or a referenced Razor class library; this example does not prove custom runtime sources equivalent.
3. Build from the .NET 11 directory using its pinned SDK and `dotnet build --configuration Release`. Run `dotnet bin/Release/net11.0/MossViews.dll`. Expect both pages to render, model text to be HTML encoded and a PASS line. Repeat with the .NET 10 comparison to verify the same page contract.
4. For local editing, set ASPNETCORE_ENVIRONMENT=Development and run `dotnet watch run -- --serve` from the new project directory with the matching SDK. Open /sightings and /night on http://127.0.0.1:5187. Hot Reload applies supported edits; accept a restart or rebuild for unsupported edits. Do not combine runtime compilation with Hot Reload: enabling the former disables the latter. Use the normal build/publish output in production, not dotnet watch.
5. Compare actual rendered responses after removing registration. Rebuild/deploy to pick up production source changes. Removal ends runtime source-file compilation; it does not make already deployed compiled views refresh themselves.

## Constraints

MVC/Razor Pages only. ASPDEPR003 completes runtime-compilation obsoletion; the runtime behavior of retained registration is unchanged. Remove registration/package for default build-time compilation, use Hot Reload during development. Build-time compilation was already valid on .NET 10. Not F# compiled-view publishing, custom runtime file-provider equivalence or Blazor component guidance.

The .NET 11 RC2 release note completes the diagnostic annotations on remaining overloads. It explicitly says runtime behavior is unchanged: retaining the obsolete registration still enables the same runtime compiler. Choosing to remove it changes the development workflow to build-time compilation/Hot Reload. No assertion is made that obsoletion itself disables view rendering.

Requirements are the .NET/ASP.NET Core 11 server runtime and a compatible SDK. Exact recorded pins are SDK 10.0.402/C# 14 and 11.0.100-rc.2.26504.105/C# 15; they are test baselines, not instructions to force those compiler modes onto older application targets. The current sample has no additional package, provider, native workload or account. Supported server platforms are broader than the measured Windows x64 execution.

## Evidence

The saved verification records capture actual builds and two real HTTP requests per project, including MVC and Razor Pages output and HTML encoding. Historical RuntimeCompilation assets and transitive packages are locked and measured; the replacement has no runtime-compilation dependency. This is execution evidence, not a mock or cross-compilation. Browser rendering, a live Hot Reload edit, custom file providers and F# publishing were not tested and are not claimed. The .NET 11 run uses RC2; GA reconciliation remains open.

Official sources: [RC2 overload diagnostics](https://github.com/dotnet/core/blob/main/release-notes/11.0/preview/rc2/aspnetcore.md), [obsoletion](https://learn.microsoft.com/en-us/aspnet/core/breaking-changes/10/razor-runtime-compilation-obsolete?view=aspnetcore-10.0), [Razor compilation](https://learn.microsoft.com/en-us/aspnet/core/mvc/views/view-compilation?view=aspnetcore-11.0), [Hot Reload](https://learn.microsoft.com/en-us/aspnet/core/test/hot-reload?view=aspnetcore-11.0), [historical package](https://www.nuget.org/packages/Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation/10.0.13).
