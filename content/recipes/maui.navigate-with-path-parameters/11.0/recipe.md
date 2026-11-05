# Open a harbour pier and explain its available actions

## Goal

A harbour operator opens North pier, marks its inspection and checks three departures awaiting clearance. The route carries the pier identifier; visible text and spoken labels explain the destination and action independently.

## Older approach

{{snippet:samples-net10-0-harbournavigation-navigation-cs}}

The canonical .NET 10 Windows app registers `pier` and passes `pierId` in a query string. `IQueryAttributable` receives the identifier and rejects unknown values. Query strings remain a valid choice; escape each dynamic value with `Uri.EscapeDataString` when constructing them. Object-based navigation is another valid alternative. The comparison uses a fixed safe identifier, a count in the departure title and a font icon with its own colour. It does not pretend .NET 10 had built-in badge, independent back-label or explicit SwipeItem text-colour APIs.

## Newer approach

{{snippet:samples-net11-0-harbournavigation-navigation-cs}}

Register `pier/{pierId}` and navigate to `//home/pier/NORTH-7`. `home` belongs to the Shell visual hierarchy; it is not another registered detail route. Route templates currently require absolute navigation. Plain registered routes are unaffected; a relative URI containing a template value is not a supported substitute. Do not change every existing query-based route unnecessarily.

Required, optional, defaulted, constrained, catch-all and mixed segments are supported. Choose a bounded identifier rather than accepting arbitrary paths. The sample deliberately accepts only NORTH-7. Route constraints are matching rules, not authorization. Encode dynamic path segments, validate the captured value and apply your normal access checks before loading private data. Avoid secrets in URIs.

The destination implements `IQueryAttributable` rather than using reflection-based `QueryPropertyAttribute`; this is the supported receiving choice for full trimming and NativeAOT. That choice alone does not prove the whole app is trim-safe or AOT-tested. Object dictionaries may retain objects for the destination's navigation lifetime; use single-use `ShellNavigationQueryParameters` where that lifetime is inappropriate.

`BackButtonBehavior.AccessibilityLabel` says “Return to harbour departures” while `TextOverride` remains “Back”. For NavigationPage, use its independent `BackButtonAccessibilityLabel` attached property instead. Check the toolbar on the actual target: a platform can render an icon rather than visible back text. A semantic description on the content label does not replace the toolbar's own spoken label.

The swipe action sets `IconColor` and `TextColor` explicitly. With IconColor unset, font icons use FontImageSource.Color, then TextColor, then background contrast; PNG/SVG icons retain their original colours. When migrating a non-font icon that depended on implicit tinting, set IconColor deliberately. Android/iOS/Mac Catalyst tint resolved icons including streams. Windows supports font and packaged file tinting; URI, rooted-path and stream icons retain original colours, and packaged files use a monochrome mask. Tizen ignores IconColor. Use `SetAppThemeColor` or AppThemeBinding for deliberate light/dark choices; check actual contrast in both themes. The fixed light palette here is a small comparison, not a complete theme system.

DepartureTabs is a separate window, not nested in Shell: Shell does not support TabbedPage children. BadgeText `"3"` gives the count, an empty string requests a dot and null hides it. Windows renders non-numeric text as a dot. Set BadgeColor and BadgeTextColor when your design needs them, but iOS/Mac Catalyst system tab bars from version 18 may retain those values while drawing system colours. Keep the meaningful count in page content too; a badge is not the only explanation.

## Migration

1. Preserve the existing route identity and decide which identifiers should be path segments. Keep query-based routes when they already suit the goal.
2. Register the template once and give its absolute URI a real Shell hierarchy anchor. Receive and validate the captured values through IQueryAttributable.
3. Author the independent back label, swipe colours and TabbedPage badge. Keep TabbedPage outside Shell and provide a visible action/count explanation.
4. On an available native target, navigate forward/back, check unknown identifiers, invoke the swipe, update/hide the badge and listen to the toolbar with the platform screen reader. Those current native interactions were not executed here.

## Constraints

Use the .NET 11 native Controls assets and a matching MAUI platform workload. TabbedPage stays outside Shell; absolute route templates need a Shell hierarchy anchor. Badge colours, swipe tinting and toolbar labels have the platform limits described above. No provider, account or Blazor rendering mode is involved. A missing native workload is not repaired by installing the guidance tool.

## Evidence

The complete canonical projects below pin .NET 10 SDK10.0.403/C#14/Controls10.0.110 and .NET 11 SDK11.0.100-rc.3.26522.148/C#15/Controls11.0.0-rc.2.26505.5. The current Windows target is net11.0-windows10.0.20348.0, win-x64/minimum17763. Matching MAUI SDK/platform workloads are required. These are native controls with no Blazor mode, provider or account. APIs are stable guidance prepared against RC2, not GA verification or experimental C# UI.

The older Windows project compiled in Release. Its optional NavigationCheck actually ran forward navigation, query delivery and back navigation in a native Windows process. Set HARBOUR_NAVIGATION_RECEIPT to a writable disposable file to run that probe; normal launches stay interactive. Current Windows compilation fails NETSDK1147 because this SDK installation lacks Microsoft.Maui.Sdk. Current native navigation, rendering, swipe/tint, badge and spoken-label behavior are source-reviewed and explicitly not-tested under PIN-699. Android/Apple device checks are unavailable and excluded; package retrieval does not establish native behavior. No older-target route for these new APIs is claimed. Source and exact package review support the contracts; upstream tests were not executed.

{{snippet:samples-net10-0-harbournavigation-navigationcheck-cs}}

## Canonical files

### samples/net10.0/HarbourNavigation/Directory.Build.props

{{snippet:samples-net10-0-harbournavigation-directory-build-props}}

### samples/net10.0/HarbourNavigation/Directory.Build.targets

{{snippet:samples-net10-0-harbournavigation-directory-build-targets}}

### samples/net10.0/HarbourNavigation/Directory.Packages.props

{{snippet:samples-net10-0-harbournavigation-directory-packages-props}}

### samples/net10.0/HarbourNavigation/global.json

{{snippet:samples-net10-0-harbournavigation-global-json}}

### samples/net10.0/HarbourNavigation/HarbourNavigation.csproj

{{snippet:samples-net10-0-harbournavigation-harbournavigation-csproj}}

### samples/net10.0/HarbourNavigation/MauiProgram.cs

{{snippet:samples-net10-0-harbournavigation-mauiprogram-cs}}

### samples/net10.0/HarbourNavigation/Navigation.cs

{{snippet:samples-net10-0-harbournavigation-navigation-cs}}

### samples/net10.0/HarbourNavigation/Platforms/Windows/App.xaml

{{snippet:samples-net10-0-harbournavigation-platforms-windows-app-xaml}}

### samples/net10.0/HarbourNavigation/Platforms/Windows/App.xaml.cs

{{snippet:samples-net10-0-harbournavigation-platforms-windows-app-xaml-cs}}

### samples/net10.0/HarbourNavigation/Resources/AppIcon/appicon.svg

{{snippet:samples-net10-0-harbournavigation-resources-appicon-appicon-svg}}

### samples/net11.0/HarbourNavigation/Directory.Build.props

{{snippet:samples-net11-0-harbournavigation-directory-build-props}}

### samples/net11.0/HarbourNavigation/Directory.Build.targets

{{snippet:samples-net11-0-harbournavigation-directory-build-targets}}

### samples/net11.0/HarbourNavigation/Directory.Packages.props

{{snippet:samples-net11-0-harbournavigation-directory-packages-props}}

### samples/net11.0/HarbourNavigation/global.json

{{snippet:samples-net11-0-harbournavigation-global-json}}

### samples/net11.0/HarbourNavigation/HarbourNavigation.csproj

{{snippet:samples-net11-0-harbournavigation-harbournavigation-csproj}}

### samples/net11.0/HarbourNavigation/MauiProgram.cs

{{snippet:samples-net11-0-harbournavigation-mauiprogram-cs}}

### samples/net11.0/HarbourNavigation/Navigation.cs

{{snippet:samples-net11-0-harbournavigation-navigation-cs}}

### samples/net11.0/HarbourNavigation/Platforms/Windows/App.xaml

{{snippet:samples-net11-0-harbournavigation-platforms-windows-app-xaml}}

### samples/net11.0/HarbourNavigation/Platforms/Windows/App.xaml.cs

{{snippet:samples-net11-0-harbournavigation-platforms-windows-app-xaml-cs}}

### samples/net11.0/HarbourNavigation/Resources/AppIcon/appicon.svg

{{snippet:samples-net11-0-harbournavigation-resources-appicon-appicon-svg}}

## Official references

- [Routing.cs official implementation](https://raw.githubusercontent.com/dotnet/maui/d32b79928709cf53b671336dabd0caef5a679fc2/src/Controls/src/Core/Routing.cs)
- [TabbedPage.cs official implementation](https://raw.githubusercontent.com/dotnet/maui/d32b79928709cf53b671336dabd0caef5a679fc2/src/Controls/src/Core/TabbedPage/TabbedPage.cs)
- [SwipeItem.cs official implementation](https://raw.githubusercontent.com/dotnet/maui/d32b79928709cf53b671336dabd0caef5a679fc2/src/Controls/src/Core/SwipeView/SwipeItem.cs)
- [NavigationPage.cs official implementation](https://raw.githubusercontent.com/dotnet/maui/d32b79928709cf53b671336dabd0caef5a679fc2/src/Controls/src/Core/NavigationPage/NavigationPage.cs)
- [ShellHandler.Android.cs official implementation](https://raw.githubusercontent.com/dotnet/maui/d32b79928709cf53b671336dabd0caef5a679fc2/src/Controls/src/Core/Handlers/Shell/ShellHandler.Android.cs)
- [ShellHandler.cs official implementation](https://raw.githubusercontent.com/dotnet/maui/d32b79928709cf53b671336dabd0caef5a679fc2/src/Controls/src/Core/Handlers/Shell/ShellHandler.cs)
- [ShellRenderer.cs official implementation](https://raw.githubusercontent.com/dotnet/maui/d32b79928709cf53b671336dabd0caef5a679fc2/src/Controls/src/Core/Compatibility/Handlers/Shell/Android/ShellRenderer.cs)
- [BackButtonBehavior.cs official implementation](https://raw.githubusercontent.com/dotnet/maui/d32b79928709cf53b671336dabd0caef5a679fc2/src/Controls/src/Core/Shell/BackButtonBehavior.cs)
- [ShellRenderer.cs official implementation](https://raw.githubusercontent.com/dotnet/maui/9d881e94b1c56692e40a16262897677a3bb46fcb/src/Controls/src/Core/Compatibility/Handlers/Shell/Android/ShellRenderer.cs)

- [MAUI 11 navigation and compatibility migration](https://learn.microsoft.com/dotnet/maui/whats-new/dotnet-11)
