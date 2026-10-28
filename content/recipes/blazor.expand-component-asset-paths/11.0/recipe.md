# Expand asset paths through reusable markup

## Goal

A maritime museum reuses one image component and one custom HTML attribute.
Both should receive the published fingerprinted URL for a sounding-weight image,
while an ordinary caption remains literal. Compare explicit asset lookup with
Razor compile-time expansion and check the actual published response at `/museum/`.

## Older approach

.NET 10 has a valid explicit `@Assets["images/sounding.svg"]` approach.
It resolves the same fingerprinted URL in component and HTML attributes. The new
annotation-based route is unavailable in the inspected .NET 10 framework/compiler;
do not backport it by adding invented attributes. Bare paths and ordinary strings
remain literal, and unknown assets are not created by lookup.

Create a separate project directory with every file below. The local build and
package files isolate it from unrelated parent policy. The tiny SVG is original
fixture content; an application's own static asset can replace it.

### .NET 10.0: `global.json`

{{snippet:n10-global-json}}

### .NET 10.0: `Directory.Build.props`

{{snippet:n10-directory-build-props}}

### .NET 10.0: `Directory.Build.targets`

{{snippet:n10-directory-build-targets}}

### .NET 10.0: `Directory.Packages.props`

{{snippet:n10-directory-packages-props}}

### .NET 10.0: `MaritimeGallery.csproj`

{{snippet:n10-maritimegallery-csproj}}

### .NET 10.0: `Program.cs`

{{snippet:n10-program-cs}}

### .NET 10.0: `App.razor`

{{snippet:n10-app-razor}}

### .NET 10.0: `ExhibitImage.razor`

{{snippet:n10-exhibitimage-razor}}

### .NET 10.0: `wwwroot/images/sounding.svg`

{{snippet:n10-wwwroot-images-sounding-svg}}

## Newer approach

Mark the string Parameter with AssetPath and use a complete `~/` literal.
Declare the exact custom element/attribute pair on a public class named
AssetPathAttributes. This declares compiler metadata; the example does not need
a browser custom-element implementation because it inspects the emitted attribute.

**Measured RC2 restriction:** default source generation expands the component
parameter but leaves both HTML attributes literal. The verified complete route
below sets the documented UseRazorSourceGenerator property to false, using the
SDK's task compiler. Its generated C# expands the component, custom element and
built-in img through Assets. If changing the build path is unsuitable, preserve
explicit lookup. This is not a general recommendation to disable source generation
or a claim about IDE/Hot Reload. The saved negative compiler control prevents this
limitation being silently forgotten.

Unknown `~/images/unknown.svg` becomes lookup of `images/unknown.svg` and falls
back to that key; a real request returns 404. Unmarked `~/` strings, a bare path
and `collection:lead-line` stay literal. Expansion neither discovers arbitrary
strings nor guarantees an asset exists.

### .NET 11.0: `global.json`

{{snippet:n11-global-json}}

### .NET 11.0: `Directory.Build.props`

{{snippet:n11-directory-build-props}}

### .NET 11.0: `Directory.Build.targets`

{{snippet:n11-directory-build-targets}}

### .NET 11.0: `Directory.Packages.props`

{{snippet:n11-directory-packages-props}}

### .NET 11.0: `MaritimeGallery.csproj`

{{snippet:n11-maritimegallery-csproj}}

### .NET 11.0: `Program.cs`

{{snippet:n11-program-cs}}

### .NET 11.0: `App.razor`

{{snippet:n11-app-razor}}

### .NET 11.0: `ExhibitImage.razor`

{{snippet:n11-exhibitimage-razor}}

### .NET 11.0: `AssetPathAttributes.cs`

{{snippet:n11-assetpathattributes-cs}}

### .NET 11.0: `wwwroot/images/sounding.svg`

{{snippet:n11-wwwroot-images-sounding-svg}}

## Migration

1. Keep each complete comparison in its own directory and build with its matching SDK/compiler and framework. Put real image bytes under wwwroot/images.
2. Preserve explicit Assets lookup on .NET 10, or when the current compiler route is unsuitable. Do not replace bare paths or arbitrary string values globally.
3. On .NET 11 annotate only the intended string Parameter with AssetPath, and declare custom HTML metadata on the public AssetPathAttributes class. Use complete static `~/` literals at the opted-in locations.
4. For the measured RC2 HTML route, keep UseRazorSourceGenerator=false in the compiling project. Verify the emitted obj/Release/net11.0/Razor/App.razor.g.cs uses Assets for both the parameter and custom attribute. Default source generation is an explicitly failing HTML control.
5. Register Razor components, MapStaticAssets and antiforgery as shown. Align UsePathBase("/museum") with base href="/museum/" so relative fingerprinted URLs resolve beneath the mounted app.
6. Publish each actual project with its matching dotnet executable using `publish -c Release -o <fresh-output>`. Run MaritimeGallery.dll from that output with `--urls http://127.0.0.1:5080` and request `/museum/`.
7. Inspect the restore manifest and published staticwebassets.endpoints.json; fetch the rendered fingerprinted URL and compare its bytes. Check the unmarked string, bare path, arbitrary string and unknown-key 404 controls. Metadata presence or compilation alone is insufficient.

## Constraints

Only complete static ~/ literals expand; [AssetPath] requires a string Parameter. Ordinary string parameters, bare paths and unrelated attributes are not inferred.

On the measured RC2 SDK, default source generation expands component parameters but leaves custom and built-in HTML ~/ paths literal. The executed HTML route uses documented UseRazorSourceGenerator=false. Retain explicit @Assets lookup if that route is unsuitable.

The public AssetPathAttributes convention class declares [AcceptsAssetPath("museum-image", "source")]. This annotates compiler input; it does not implement or register a browser custom element.

An unknown ~/ key lowers to Assets["key"]; the collection returns the unmapped key. HTTP 404 proves no asset was created. Do not mistake prefix removal for a valid published URL.

Verified static SSR and published fingerprinted URLs under /museum/. No interactive rendering, IDE/Hot Reload, arbitrary build host, runtime rewriting or older-target package route is claimed.

The exact test pins are SDK 10.0.402 / framework and packages 10.0.13 / C# 14,
and SDK 11.0.100-rc.2.26504.105 / framework and packages
11.0.0-rc.2.26504.105 / C# 15. Runtime/framework/package/SDK requirements are
separate from these measured baselines. No optional workload or native compilation
is required. Public metadata is not an experimental designation; no experimental
suppression is used. No net10-plus-new-package compatibility route is advertised.

## Evidence

Both actual targets compile with locked resolved dependencies through
eng/Record-Evidence.ps1. Their selected behavior.evidence.json records retain
inputs, actual commands, browser results and measured Razor compiler hashes.
eng/Record-TokenAssetBehavior.ps1 runs the focused real checks using
eng/Test-TokenAssetConsumers.mjs; neither cross-compilation nor metadata inspection
stands in for the claimed execution. Installed stdio inspection checks both
tools, discovery ID/title/goal, the full canonical body, explicit 11.0 edition,
applicability and current evidence from the exact locally installed ordinary tool.
Offline guidance is embedded; inspection does not contact a provider.

The evidence is preparation against RC2, not final GA qualification. PIN-959's
comprehensive delivery gate and PIN-964's actual-GA reconciliation remain open.
