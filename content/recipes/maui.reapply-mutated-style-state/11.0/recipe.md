# Refresh a beacon panel without replacing its controls

## Goal

A harbour watch panel switches to its night palette. Keep the existing controls and update their style, active visual state and gradient. Preserve a friendly name beside each colour without relying on colour inheritance.

## Older approach

The .NET 10 BoardPage replaces the Style and state-group collection, then reselects Active. It paints a Rectangle with a gradient brush. Both are supported approaches: replacing a style is still useful when you want an independent style instance. The NamedSwatch subclass adds a name to Color; the Windows comparison compiles against Controls10.0.110. Do not transfer that inheritance to Controls11's sealed record Color.

## Newer approach

{{snippet:samples-net11-0-beaconboard-boardpage-cs}}

The .NET 11 page mutates an existing setter, calls heading.InvalidateStyle(), changes the active state's opacity setter and calls VisualStateManager.InvalidateVisualStates(heading). These are explicit reapplication hooks, not layout invalidation. They are particularly useful for tooling and deliberate in-place mutations; ordinary bound property updates need no blanket invalidation loop.

BoxView.Fill accepts a Brush. Its shape fill resolves to Fill when non-null, otherwise Color converted to paint. Color remains the solid-fill API; Background/BackgroundColor are inherited background properties and should not be described as aliases for Color. Clear Fill before expecting Color to control the shape again. The sample leaves the original Color set to show that the gradient wins.

NamedSwatch now contains a Color Value and a Name. Pass Value to MAUI rather than passing the wrapper. Composition preserves the name without subclassing a sealed record. It does not change channel order or introduce a binary colour format.

## Migration

1. Compare BoardPage.cs in both canonical versions. Keep ownership of shared style instances explicit: mutating one can affect multiple consumers; reapply to the elements you intentionally update.
2. Keep the initial Active state selected before changing its setter. InvalidateVisualStates reapplies current states; it does not choose a new state. Keep local property values and setter precedence in mind when assessing visible results.
3. Use var for GetVisualStateGroups, whose .NET 11 return is VisualStateGroupList. Adapt explicitly typed code to that contract rather than requiring List<VisualStateGroup>.
4. Replace the older Rectangle with BoxView only when its geometry fits. Set Fill to the gradient; keep Color for solid fills. Use a shape when you need more elaborate geometry.
5. Replace Color-derived domain types with wrappers and update call sites to use Value. Rebuild actual consumers; inheritance is a source-breaking change.

## Constraints

This is native MAUI guidance, with no Blazor rendering mode, service, provider, account or preview-language opt-in. The Windows comparison pins SDK10.0.403/C#14/Controls10.0.110. Current projects pin SDK11.0.100/C#15/Controls11.0.0. These are RC preparation artifacts, not GA verification. Applicability lists exact intended native assets and workload composition. Only .NET11 has a served edition; .NET10 remains comparison-only.

## Evidence

The .NET 11 SDK baseline is now 11.0.100 GA with preview selection disabled. The .NET 10 comparisons retain their declared SDKs. Earlier RC audit details below are historical; use the current sample pins, dependency locks and GA verification records for current coverage. Existing native exclusions and pending Human gates still apply.

The actual .NET10 Windows example builds in Release. Exact dependency/input identities are in its saved compilation record. Current Windows native compilation is not-tested because the installed SDK11 root lacks its MAUI SDK pack. Apple compilation and all native interaction/rendering checks are not-tested on this Windows-only authoring host; no Android hardware or Apple toolchain is assumed or requested. Under PIN-699 and Pingu's 4 November clarification these are exclusions, not pending Human obligations.

The pinned implementation and downloaded Controls.Core RC2 package establish member availability, Fill/Color precedence and the sealed Color contract. Source review is not sample execution. Packed stdio list/get independently checks offline guidance, edition and applicability, not the visual result.

### samples/net10.0/BeaconBoard/BeaconBoard.csproj

{{snippet:samples-net10-0-beaconboard-beaconboard-csproj}}

### samples/net10.0/BeaconBoard/BoardPage.cs

{{snippet:samples-net10-0-beaconboard-boardpage-cs}}

### samples/net10.0/BeaconBoard/Directory.Build.props

{{snippet:samples-net10-0-beaconboard-directory-build-props}}

### samples/net10.0/BeaconBoard/Directory.Build.targets

{{snippet:samples-net10-0-beaconboard-directory-build-targets}}

### samples/net10.0/BeaconBoard/Directory.Packages.props

{{snippet:samples-net10-0-beaconboard-directory-packages-props}}

### samples/net10.0/BeaconBoard/global.json

{{snippet:samples-net10-0-beaconboard-global-json}}

### samples/net10.0/BeaconBoard/MauiProgram.cs

{{snippet:samples-net10-0-beaconboard-mauiprogram-cs}}

### samples/net10.0/BeaconBoard/Platforms/Windows/App.xaml

{{snippet:samples-net10-0-beaconboard-platforms-windows-app-xaml}}

### samples/net10.0/BeaconBoard/Platforms/Windows/App.xaml.cs

{{snippet:samples-net10-0-beaconboard-platforms-windows-app-xaml-cs}}

### samples/net11.0/BeaconBoard/BeaconBoard.csproj

{{snippet:samples-net11-0-beaconboard-beaconboard-csproj}}

### samples/net11.0/BeaconBoard/BoardPage.cs

{{snippet:samples-net11-0-beaconboard-boardpage-cs}}

### samples/net11.0/BeaconBoard/Directory.Build.props

{{snippet:samples-net11-0-beaconboard-directory-build-props}}

### samples/net11.0/BeaconBoard/Directory.Build.targets

{{snippet:samples-net11-0-beaconboard-directory-build-targets}}

### samples/net11.0/BeaconBoard/Directory.Packages.props

{{snippet:samples-net11-0-beaconboard-directory-packages-props}}

### samples/net11.0/BeaconBoard/global.json

{{snippet:samples-net11-0-beaconboard-global-json}}

### samples/net11.0/BeaconBoard/MauiProgram.cs

{{snippet:samples-net11-0-beaconboard-mauiprogram-cs}}

### samples/net11.0/BeaconBoard/Platforms/Windows/App.xaml

{{snippet:samples-net11-0-beaconboard-platforms-windows-app-xaml}}

### samples/net11.0/BeaconBoard/Platforms/Windows/App.xaml.cs

{{snippet:samples-net11-0-beaconboard-platforms-windows-app-xaml-cs}}

### samples/net11.0/BeaconBoardiOS/BeaconBoardiOS.csproj

{{snippet:samples-net11-0-beaconboardios-beaconboardios-csproj}}

### samples/net11.0/BeaconBoardiOS/BoardPage.cs

{{snippet:samples-net11-0-beaconboardios-boardpage-cs}}

### samples/net11.0/BeaconBoardiOS/Directory.Build.props

{{snippet:samples-net11-0-beaconboardios-directory-build-props}}

### samples/net11.0/BeaconBoardiOS/Directory.Build.targets

{{snippet:samples-net11-0-beaconboardios-directory-build-targets}}

### samples/net11.0/BeaconBoardiOS/Directory.Packages.props

{{snippet:samples-net11-0-beaconboardios-directory-packages-props}}

### samples/net11.0/BeaconBoardiOS/global.json

{{snippet:samples-net11-0-beaconboardios-global-json}}

### samples/net11.0/BeaconBoardiOS/MauiProgram.cs

{{snippet:samples-net11-0-beaconboardios-mauiprogram-cs}}

### samples/net11.0/BeaconBoardiOS/Platforms/iOS/AppDelegate.cs

{{snippet:samples-net11-0-beaconboardios-platforms-ios-appdelegate-cs}}

### samples/net11.0/BeaconBoardiOS/Platforms/iOS/Info.plist

{{snippet:samples-net11-0-beaconboardios-platforms-ios-info-plist}}

### samples/net11.0/BeaconBoardiOS/Platforms/iOS/Program.cs

{{snippet:samples-net11-0-beaconboardios-platforms-ios-program-cs}}

### samples/net11.0/BeaconBoardiOS/Platforms/iOS/SceneDelegate.cs

{{snippet:samples-net11-0-beaconboardios-platforms-ios-scenedelegate-cs}}

## Official references

- [StyleableElement.cs implementation](https://raw.githubusercontent.com/dotnet/maui/d32b79928709cf53b671336dabd0caef5a679fc2/src/Controls/src/Core/StyleableElement/StyleableElement.cs)
- [VisualStateManager.cs implementation](https://raw.githubusercontent.com/dotnet/maui/d32b79928709cf53b671336dabd0caef5a679fc2/src/Controls/src/Core/VisualStateManager.cs)
- [BoxView.cs implementation](https://raw.githubusercontent.com/dotnet/maui/d32b79928709cf53b671336dabd0caef5a679fc2/src/Controls/src/Core/BoxView/BoxView.cs)
- [Color.cs implementation](https://raw.githubusercontent.com/dotnet/maui/d32b79928709cf53b671336dabd0caef5a679fc2/src/Graphics/src/Graphics/Color.cs)
- [LongPressGestureRecognizer.cs implementation](https://raw.githubusercontent.com/dotnet/maui/d32b79928709cf53b671336dabd0caef5a679fc2/src/Controls/src/Core/LongPressGestureRecognizer.cs)
- [ViewExtensions.cs implementation](https://raw.githubusercontent.com/dotnet/maui/d32b79928709cf53b671336dabd0caef5a679fc2/src/Controls/src/Core/ViewExtensions.cs)
- [LongPressGestureHandler.cs implementation](https://raw.githubusercontent.com/dotnet/maui/d32b79928709cf53b671336dabd0caef5a679fc2/src/Controls/src/Core/Platform/Android/LongPressGestureHandler.cs)
- [LongPressGestureHandler.Windows.cs implementation](https://raw.githubusercontent.com/dotnet/maui/d32b79928709cf53b671336dabd0caef5a679fc2/src/Controls/src/Core/Platform/GestureManager/LongPressGestureHandler.Windows.cs)

### samples/net10.0/BeaconBoard/Resources/AppIcon/appicon.svg

{{snippet:samples-net10-0-beaconboard-resources-appicon-appicon-svg}}

### samples/net11.0/BeaconBoard/Resources/AppIcon/appicon.svg

{{snippet:samples-net11-0-beaconboard-resources-appicon-appicon-svg}}

### samples/net11.0/BeaconBoardiOS/Resources/AppIcon/appicon.svg

{{snippet:samples-net11-0-beaconboardios-resources-appicon-appicon-svg}}
