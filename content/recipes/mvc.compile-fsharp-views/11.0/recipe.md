# Publish F# MVC views without runtime compilation

## Goal

Migrate an existing F# MVC app to build- and publish-time Razor compilation and explicit view-assembly registration so pages render from clean published output without source views or a runtime compiler.

The Star survey observatory must still render its nightly page after deployment. Unlike the C# path, F# needs the legacy Razor compiler to produce a separate C# views assembly and MVC must discover that assembly. The sample is original; the actual SDK templates and migration source establish the tooling boundary.

## Older approach — runtime-compiled F# views on .NET 10

The historical setup retains source views and RuntimeCompilation10.0.13. Runtime compilation was already obsolete on .NET10. Its local F# warning44 acknowledgement exists only for this comparison. The exact lock includes the package's legacy Razor6/Roslyn4 compiler closure; SDK and runtime compiler versions are different artifacts.

Save the following files under the indicated names in a separate directory. Restore once to create a lock file from the exact direct pins; commit that lock for repeatable builds.

### Directory.Build.props

{{snippet:n10-directory-build-props}}

### Directory.Build.targets

{{snippet:n10-directory-build-targets}}

### Directory.Packages.props

{{snippet:n10-directory-packages-props}}

### global.json

{{snippet:n10-global-json}}

### StarSurvey.fsproj

{{snippet:n10-starsurvey-fsproj}}

### NightController.fs

{{snippet:n10-nightcontroller-fs}}

### Program.fs

{{snippet:n10-program-fs}}

### Views/Night/Index.cshtml

{{snippet:n10-views-night-index-cshtml}}

## Newer approach — publish compiled F# views on .NET 11

These complete files build and register StarSurvey.Views.dll. The ordered registration module loads the exact application-owned assembly beside the executable, verifies its load context and adds its compiled Razor application part. A normal deployment missing that DLL fails at startup. Keep all published files together.

Save the following files under the indicated names in a separate directory. Restore once to create a lock file from the exact direct pins; commit that lock for repeatable builds.

### Directory.Build.props

{{snippet:n11-directory-build-props}}

### Directory.Build.targets

{{snippet:n11-directory-build-targets}}

### Directory.Packages.props

{{snippet:n11-directory-packages-props}}

### global.json

{{snippet:n11-global-json}}

### StarSurvey.fsproj

{{snippet:n11-starsurvey-fsproj}}

### NightController.fs

{{snippet:n11-nightcontroller-fs}}

### SurveyViews.fs

{{snippet:n11-surveyviews-fs}}

### Program.fs

{{snippet:n11-program-fs}}

### Views/Night/Index.cshtml

{{snippet:n11-views-night-index-cshtml}}

## Migration steps

1. Remove the runtime-compilation package and registration from the migrated project. Preserve AddControllersWithViews, controller sources and endpoint mapping.

2. Set UseRazorSourceGenerator=false, RazorCompileOnBuild=true and RazorCompileOnPublish=true. F# cannot consume the C# source generator directly; a successful F# build alone says nothing about whether a view exists.

3. Compile SurveyViews.fs after the controller and before Program.fs. Register the generated .Views.dll in the application assembly's load context before building the app. Adapt the fixed assembly name if renaming this example; this is not a general plugin loader.

4. Build, publish to a fresh directory and inspect the output. Require StarSurvey.Views.dll and no source .cshtml or RuntimeCompilation binaries. Run the published executable from that directory and request the real MVC page. Confirm the encoded model text, not merely HTTP200.

5. Run the independent disabled-compilation negative control. It must build/publish successfully yet return the specific MVC missing-view exception on a real request. The --expect-missing test switch bypasses registration to expose that failure; do not use it in an application.

6. Use normal edit/build cycles or evaluate supported development tooling separately. This recipe does not verify a Hot Reload session, trimming, NativeAOT, single-file packaging or a custom AssemblyLoadContext deployment.

## Constraints

Application route: ASP.NET Core11 with its matching SDK; F#11 plus Razor's C# view compiler and matching FSharp.Core. No runtime-compilation package on the migrated side. Exact reproduction uses SDK10.0.402/framework10.0.13 versus SDK11.0.100-rc.2.26504.105/framework11.0.0-rc.2.26504.105. The source audit's Http.Features/MVC references and F# MVC template artifacts are reconciled separately from these runtime checks. Compiler, runtime, selected package assets, commands and input hashes are retained in the existing sample-evidence records.

The old FSharp.Core10.1.402/RuntimeCompilation10.0.13 graph and current FSharp.Core11.0.101-rc2.26504.105 graph are exact test pins, not minimum supported versions. The current build emits and actually loads the separate .Views.dll from the default application load context. The fresh published consumer contains no source views or runtime compiler; a fresh disabled-compilation publish demonstrates the missing-view failure. Build-time compilation can also be configured on older targets; no untested older-target route is advertised here.

Server-rendered MVC HTML and model encoding through real HTTP. No browser, Blazor, live edit or Hot Reload session is claimed. Windows x64 is the executed lane. No package-native assets, workloads, external provider, resource provisioning or public publication are required. Stable migration guidance is based on RC2 evidence; actual-GA reconciliation and comprehensive readiness remain open. The eight approved .NET10 foundations and existing recipe/edition identities remain unchanged.

## Evidence and local verification

Fresh sample records contain actual paired compilation/rendering, exact artifact qualification and the .NET11 clean published-consumer controls. The positive page renders encoded model data; the fresh disabled-compilation publish observes the specific missing-view exception. A build-only result would not satisfy this claim.

From each complete sample directory, use its matching absolute dotnet executable to restore, build in Release and run the resulting DLL. The default execution performs real request checks and exits. Retain the initial lock and use restore --locked-mode for later checks.

For the newer project, also run publish -c Release --no-restore -o <fresh-directory>. Change into that fresh output and run StarSurvey.dll with the matching runtime. It requests its own dynamically allocated loopback URL, prints the compiled view assembly/context and checks rendered/encoded content. The repository's Test-WebRetirement helper performs these positive and negative published controls through Record-Evidence; evidence freshness is checked during ordinary build/pack. A copied .Views.dll from an older build must not mask missing compilation.

## Official references

- [F# MVC migration requirements](https://github.com/dotnet/AspNetCore.Docs/blob/ecf7aca5136aea60aa8db7330f66ac799d3edcf7/aspnetcore/migration/100-to-110/includes/mvc.md)
- [Razor SDK compilation properties](https://learn.microsoft.com/en-us/aspnet/core/razor-pages/sdk?view=aspnetcore-11.0)
- [Runtime compilation was already obsolete in .NET10](https://learn.microsoft.com/en-us/aspnet/core/breaking-changes/10/razor-runtime-compilation-obsolete)
- [Current F# MVC template](https://github.com/dotnet/aspnetcore/tree/4a70870818bf5662f21b0c28f9a8c267dccb77f0/src/ProjectTemplates/Web.ProjectTemplates/content/StarterWeb-FSharp)
