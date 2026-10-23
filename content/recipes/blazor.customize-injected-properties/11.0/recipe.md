# Customize component property injection

## Goal

A breakwater instrument panel needs its station name, a keyed depth scale and a circuit-owned session. Keep those services in their intended scopes while choosing how the panel receives them.

## Older approach

On .NET 10 the ordinary framework injector can populate a service adapter whose constructor requests the keyed scale. This is a supported way to assemble the same dependencies. There is no equivalent public IComponentPropertyActivator hook in the declared older assets. IComponentActivator creates instances; it does not replace the framework's later property injection. Do not use internal switches to disable that phase.

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

Register IComponentPropertyActivator with the application's service collection. The new renderer asks this policy for a delegate and supplies the current provider when it invokes that delegate. The original StationPropertyPolicy uses an explicit mapping for StationPanel, including properties inherited from StationPanelBase. The key depth belongs to Scale; the other dependencies are unkeyed. The singleton policy never captures the current provider, a component or a scoped service.


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

Use the default injector when its normal keyed/unkeyed behavior already meets the need. Adopt the hook for a concrete container or selection policy. This example deliberately owns a closed component set: every injected property on StationPanel must be assigned explicitly. Adding a property or another component with injection requires extending the mapping. Types outside that mapping are scanned through their base types and rejected if they declare injection; types without injection receive a no-op, including framework render-mode boundaries. This is not a general-purpose replacement for all Blazor injection, Hot Reload cache invalidation or third-party containers.

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

Advanced DI only. The inherited public properties, Microsoft DI keyed lookup and a real server circuit were executed. A missing key must fail, rather than quietly supply an unkeyed service. The type annotation matches the interface but does not establish trimming support: the fallback uses reflection and no trimmed publish, AOT or Hot Reload run was performed. The stateless delegate resolves from its invocation provider; no broad concurrency guarantee is claimed. Third-party providers, WebAssembly and Hybrid are unverified.

## Evidence

API stability is stable in discovery/detail; the measured artifact is RC2, not GA.
Compilation records preserve exact inputs, SDK, target, language, dependencies and
source dirty state. Browser execution is separate from compilation. The focused
receipts are documented in docs/verification/PIN-801-803.md. This recipe makes no
cross-platform, trimming, NativeAOT, Hot Reload or general concurrent-container
support promise. The eight .NET 10 foundation editions are unchanged.

## Official references

- [Property activation contract](https://github.com/dotnet/aspnetcore/blob/4a70870818bf5662f21b0c28f9a8c267dccb77f0/src/Components/Components/src/IComponentPropertyActivator.cs)
- [Framework invokes property activation separately](https://github.com/dotnet/aspnetcore/blob/4a70870818bf5662f21b0c28f9a8c267dccb77f0/src/Components/Components/src/ComponentFactory.cs)
- [Older component activation](https://github.com/dotnet/aspnetcore/blob/v10.0.13/src/Components/Components/src/ComponentFactory.cs)
