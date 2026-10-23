# Express no-result JavaScript calls

## Goal

Announce the latest breakwater reading to a browser output without pretending that JavaScript returns a useful value. A successful action updates the output; a rejected action remains observable to .NET.

## Older approach

The .NET 10 increase handler awaits InvokeAsync<object> and discards its result for the no-result announce export. That call works, but its generic result obscures the intent. InvokeVoidAsync was already supported on .NET 10: this same sample actually uses it for drawFormula and for the rejected announcement. Migrating a no-result call to InvokeVoidAsync does not require a new runtime capability.

Save all listed files in a separate BreakwaterLab directory. The complete app covers three related goals. Empty Directory files isolate parent build/package configuration; no template or _Imports.razor is required. The Microsoft.AspNetCore.Components.Web import enables @onclick.


#### .NET 10: `global.json`

{{snippet:n10-global-json}}

#### .NET 10: `Directory.Build.props`

{{snippet:n10-directory-build-props}}

#### .NET 10: `Directory.Build.targets`

{{snippet:n10-directory-build-targets}}

#### .NET 10: `Directory.Packages.props`

{{snippet:n10-directory-packages-props}}

#### .NET 10: `BreakwaterLab.csproj`

{{snippet:n10-breakwaterlab-csproj}}

#### .NET 10: `Program.cs`

{{snippet:n10-program-cs}}

#### .NET 10: `App.razor`

{{snippet:n10-app-razor}}

#### .NET 10: `StationServices.cs`

{{snippet:n10-stationservices-cs}}

#### .NET 10: `StationPanel.razor`

{{snippet:n10-stationpanel-razor}}

#### .NET 10: `wwwroot/station.js`

{{snippet:n10-wwwroot-station-js}}

## Newer approach

The .NET 11 increase handler awaits InvokeVoidAsync for announce. Keep InvokeAsync<IJSObjectReference> for importing the module because import really does return a reference. The source change follows the purpose of BL0010, without implementing an analyzer or claiming that a runtime API was newly introduced. The declared RC2 build also enforces BL0016 error handling; the complete component includes try/catch and module disposal.


#### .NET 11: `global.json`

{{snippet:n11-global-json}}

#### .NET 11: `Directory.Build.props`

{{snippet:n11-directory-build-props}}

#### .NET 11: `Directory.Build.targets`

{{snippet:n11-directory-build-targets}}

#### .NET 11: `Directory.Packages.props`

{{snippet:n11-directory-packages-props}}

#### .NET 11: `BreakwaterLab.csproj`

{{snippet:n11-breakwaterlab-csproj}}

#### .NET 11: `Program.cs`

{{snippet:n11-program-cs}}

#### .NET 11: `App.razor`

{{snippet:n11-app-razor}}

#### .NET 11: `StationServices.cs`

{{snippet:n11-stationservices-cs}}

#### .NET 11: `StationPanel.razor`

{{snippet:n11-stationpanel-razor}}

#### .NET 11: `wwwroot/station.js`

{{snippet:n11-wwwroot-station-js}}

#### .NET 11: `StationPanelBase.cs`

{{snippet:n11-stationpanelbase-cs}}

#### .NET 11: `StationPropertyPolicy.cs`

{{snippet:n11-stationpropertypolicy-cs}}

## Migration and ownership

1. Save the older and newer files separately and build against their stated pins.
2. Apply the source change described below, preserving service lifetime and browser ownership.
3. Run both real browser paths and compare the observable results under Build and exercise.

Check that callers do not need the return value. Replace only discarded result calls with InvokeVoidAsync, retaining the identifier, arguments, await and applicable cancellation or timeout behavior. Preserve generic calls when their values matter. No-result is not fire-and-forget: exceptions can still propagate through the awaited operation. In this sample an empty station makes JavaScript throw, and the component displays Announcement rejected without modifying the last successful announcement.

## Build and exercise

The older pin is SDK 10.0.402 / C# 14 / ASP.NET Core 10.0.13. The newer pin is
SDK 11.0.100-rc.2.26504.105 / C# 15 / ASP.NET Core 11.0.0-rc.2.26504.105.
Microsoft.NET.Sdk.Web supplies the implicit Internal.Assets package at the
matching framework version; no explicit PackageReference is needed. The shared
framework supplies the server/provider/interop assemblies. Restore needs NuGet
access or a prepopulated cache; consulting this recipe needs neither.

Save each release in its own directory. In PowerShell, select that directory,
set process-only DOTNET_ADD_GLOBAL_TOOLS_TO_PATH to 0, then use the absolute path
to your matching dotnet executable with build -c Release. Set process-only
ASPNETCORE_ENVIRONMENT and DOTNET_ENVIRONMENT to Development for build-output
static assets. Execute bin/Release/net10.0/BreakwaterLab.dll or
bin/Release/net11.0/BreakwaterLab.dll with that same executable, followed by
--urls http://127.0.0.1:5991 (older) or --urls http://127.0.0.1:5992 (newer).
These foreground hosts occupy their terminals; Ctrl+C stops them.

In a MathML-capable browser wait for Interactive. Expect West breakwater,
Multiplier: 3 and d = 2 × 3. Click Add one reading. Expect d = 3 × 3,
Announced reading 3 and West breakwater: 3. The output's data-calls is 1.
Click Try invalid announcement. Expect Announcement rejected, with the previous
output and count unchanged. Inspect namespaceURI on the working math root and
all descendants: each must be http://www.w3.org/1998/Math/MathML. The older
extra direct-control has the HTML namespace and is an intentional failure control.
Reload creates a fresh circuit/session and resets the reading. Session IDs remain
stable through a circuit's updates; they are not durable user identities.

## Constraints and applicability

Blazor JS interop authoring only; analyzer internals, diagnostic implementation and performance measurements are outside this recipe. Both actual modules executed in Windows Chrome. The new successful void call produces West breakwater: 3 once; the failing void call is caught and does not increment the DOM call counter. JS interop runs after interactive startup, never during static SSR. Disposal catches a disconnected server circuit; this does not guarantee browser cleanup after loss of connectivity.

## Evidence

API stability is stable in discovery/detail; the measured artifact is RC2, not GA.
Compilation records preserve exact inputs, SDK, target, language, dependencies and
source dirty state. Browser execution is separate from compilation. The focused
receipts are documented in docs/verification/PIN-801-803.md. This recipe makes no
cross-platform, trimming, NativeAOT, Hot Reload or general concurrent-container
support promise. The eight .NET 10 foundation editions are unchanged.

## Official references

- [Previous-release void module calls](https://github.com/dotnet/aspnetcore/blob/v10.0.13/src/JSInterop/Microsoft.JSInterop/src/JSObjectReferenceExtensions.cs)
- [Current module interop contract](https://github.com/dotnet/aspnetcore/blob/4a70870818bf5662f21b0c28f9a8c267dccb77f0/src/JSInterop/Microsoft.JSInterop/src/JSObjectReferenceExtensions.cs)
- [Release authoring guidance](https://learn.microsoft.com/en-us/aspnet/core/release-notes/aspnetcore-11?view=aspnetcore-11.0)
