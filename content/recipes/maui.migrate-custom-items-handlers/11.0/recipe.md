# Preserve jetty inspection lists when upgrading native handlers

## Goal

Keep the fictional jetty list and photo carousel registered to their own handlers
while choosing the supported implementation for each platform. The empty subclasses
make the registration/base-class seam explicit; they do not simulate native UI
testing or promise that renderer-specific overrides can be copied unchanged.

## Older approach

The .NET10 JettyItems app derives its custom handlers from Items.CollectionViewHandler
and Items.CarouselViewHandler. These were valid Apple customization seams. In
.NET11 those Apple classes are obsolete; derive from
Items2.CollectionViewHandler2 and Items2.CarouselViewHandler2 instead. The controls
CollectionView and CarouselView themselves are not obsolete. Port platform-view
overrides deliberately: review the new base lifecycle, disconnect native events,
and check selection, recycling, empty lists, grouping, scroll position and carousel
position updates. Never suppress the warning as evidence that migration works.

Windows has a separate change: CollectionView2 now defaults to an ItemsRepeater
implementation. JettyWindows changes its custom collection handler base to
Items2.CollectionViewHandler2. Windows CarouselView retains its existing handler;
the Apple carousel migration is not a Windows instruction. Selection highlights
and multiple-selection interaction can differ from the old ListView-based host.

## Newer approach

Use the Items2 base for the Apple custom controls and Windows custom list:

{{snippet:samples-net11-0-jettyitems-handlers-cs}}

## Migration

1. Identify each custom handler registration and its platform-specific base.
2. Change Apple collection/carousel bases and the Windows collection base to Items2.
3. Port native overrides against the new platform view; pair event cleanup with connection.
4. Exercise selection, scrolling and carousel position on an available target before deployment.
5. Use the supported Windows fallback below only while deliberately retaining the legacy base.

## Constraints


Set UseWindowsCollectionView2Handler=false in the Windows project when temporarily
retaining the legacy default. This does not rewrite explicit custom registrations:
keep a custom handler based on Items.CollectionViewHandler registered explicitly
if that is the implementation you intend to retain. The .NET10 JettyWindows
registration illustrates that still-supported Windows base. Apple's obsolete
attribute does not apply to it. Remove the switch after migrating and validating
the default controls as well as the custom ones.

{{snippet:jetty-windows-fallback}}

## Evidence

The .NET 11 SDK baseline is now 11.0.100 GA with preview selection disabled. The .NET 10 comparisons retain their declared SDKs. Earlier RC audit details below are historical; use the current sample pins, dependency locks and GA verification records for current coverage. Existing native exclusions and pending Human gates still apply.

The paired projects pin SDK10.0.403/Controls10.0.110/C#14 versus SDK11 GA
11.0.100/Controls11.0.0/C#15. Native iOS26/27
and Windows19041/20348 assets require their matching workloads; iOS minimum18.2
and Windows minimum17763 are explicit. No provider, account, Blazor rendering,
experimental API or preview-language opt-in is involved. This is stable API
guidance with RC2 evidence, not GA proof. .NET10 is a comparison, not a supported
target route in this catalogue edition. Mac Catalyst follows Apple's migration
but is not a separately compiled sample.

Older Windows Release compilation is actual local evidence. Apple compilation
requires unavailable Mac/Xcode; current Windows compilation fails NETSDK1147
because the isolated SDK11 root lacks Microsoft.Maui.Sdk. These builds and all
native interactions are explicitly not-tested under PIN-699. Exact pinned source,
artifact review and sample inputs are retained. No pending Human proof is created
for the excluded environment. Packed stdio list/get verifies offline delivery,
complete body and applicability only; it does not establish native UI behavior.
No package/account/resource publication or provisioning is performed.

## Canonical files

### samples/net10.0/JettyItems/Directory.Build.props

{{snippet:samples-net10-0-jettyitems-directory-build-props}}

### samples/net10.0/JettyItems/Directory.Build.targets

{{snippet:samples-net10-0-jettyitems-directory-build-targets}}

### samples/net10.0/JettyItems/Directory.Packages.props

{{snippet:samples-net10-0-jettyitems-directory-packages-props}}

### samples/net10.0/JettyItems/global.json

{{snippet:samples-net10-0-jettyitems-global-json}}

### samples/net10.0/JettyItems/Handlers.cs

{{snippet:samples-net10-0-jettyitems-handlers-cs}}

### samples/net10.0/JettyItems/JettyItems.csproj

{{snippet:samples-net10-0-jettyitems-jettyitems-csproj}}

### samples/net10.0/JettyItems/JettyPage.cs

{{snippet:samples-net10-0-jettyitems-jettypage-cs}}

### samples/net10.0/JettyItems/MauiProgram.cs

{{snippet:samples-net10-0-jettyitems-mauiprogram-cs}}

### samples/net10.0/JettyItems/Platforms/iOS/AppDelegate.cs

{{snippet:samples-net10-0-jettyitems-platforms-ios-appdelegate-cs}}

### samples/net10.0/JettyItems/Platforms/iOS/Info.plist

{{snippet:samples-net10-0-jettyitems-platforms-ios-info-plist}}

### samples/net10.0/JettyItems/Platforms/iOS/Program.cs

{{snippet:samples-net10-0-jettyitems-platforms-ios-program-cs}}

### samples/net10.0/JettyItems/Platforms/iOS/SceneDelegate.cs

{{snippet:samples-net10-0-jettyitems-platforms-ios-scenedelegate-cs}}

### samples/net10.0/JettyWindows/Directory.Build.props

{{snippet:samples-net10-0-jettywindows-directory-build-props}}

### samples/net10.0/JettyWindows/Directory.Build.targets

{{snippet:samples-net10-0-jettywindows-directory-build-targets}}

### samples/net10.0/JettyWindows/Directory.Packages.props

{{snippet:samples-net10-0-jettywindows-directory-packages-props}}

### samples/net10.0/JettyWindows/global.json

{{snippet:samples-net10-0-jettywindows-global-json}}

### samples/net10.0/JettyWindows/Handlers.cs

{{snippet:samples-net10-0-jettywindows-handlers-cs}}

### samples/net10.0/JettyWindows/JettyPage.cs

{{snippet:samples-net10-0-jettywindows-jettypage-cs}}

### samples/net10.0/JettyWindows/JettyWindows.csproj

{{snippet:samples-net10-0-jettywindows-jettywindows-csproj}}

### samples/net10.0/JettyWindows/MauiProgram.cs

{{snippet:samples-net10-0-jettywindows-mauiprogram-cs}}

### samples/net10.0/JettyWindows/Platforms/Windows/App.xaml

{{snippet:samples-net10-0-jettywindows-platforms-windows-app-xaml}}

### samples/net10.0/JettyWindows/Platforms/Windows/App.xaml.cs

{{snippet:samples-net10-0-jettywindows-platforms-windows-app-xaml-cs}}

### samples/net11.0/JettyItems/Directory.Build.props

{{snippet:samples-net11-0-jettyitems-directory-build-props}}

### samples/net11.0/JettyItems/Directory.Build.targets

{{snippet:samples-net11-0-jettyitems-directory-build-targets}}

### samples/net11.0/JettyItems/Directory.Packages.props

{{snippet:samples-net11-0-jettyitems-directory-packages-props}}

### samples/net11.0/JettyItems/global.json

{{snippet:samples-net11-0-jettyitems-global-json}}

### samples/net11.0/JettyItems/Handlers.cs

{{snippet:samples-net11-0-jettyitems-handlers-cs}}

### samples/net11.0/JettyItems/JettyItems.csproj

{{snippet:samples-net11-0-jettyitems-jettyitems-csproj}}

### samples/net11.0/JettyItems/JettyPage.cs

{{snippet:samples-net11-0-jettyitems-jettypage-cs}}

### samples/net11.0/JettyItems/MauiProgram.cs

{{snippet:samples-net11-0-jettyitems-mauiprogram-cs}}

### samples/net11.0/JettyItems/Platforms/iOS/AppDelegate.cs

{{snippet:samples-net11-0-jettyitems-platforms-ios-appdelegate-cs}}

### samples/net11.0/JettyItems/Platforms/iOS/Info.plist

{{snippet:samples-net11-0-jettyitems-platforms-ios-info-plist}}

### samples/net11.0/JettyItems/Platforms/iOS/Program.cs

{{snippet:samples-net11-0-jettyitems-platforms-ios-program-cs}}

### samples/net11.0/JettyItems/Platforms/iOS/SceneDelegate.cs

{{snippet:samples-net11-0-jettyitems-platforms-ios-scenedelegate-cs}}

### samples/net11.0/JettyWindows/Directory.Build.props

{{snippet:samples-net11-0-jettywindows-directory-build-props}}

### samples/net11.0/JettyWindows/Directory.Build.targets

{{snippet:samples-net11-0-jettywindows-directory-build-targets}}

### samples/net11.0/JettyWindows/Directory.Packages.props

{{snippet:samples-net11-0-jettywindows-directory-packages-props}}

### samples/net11.0/JettyWindows/global.json

{{snippet:samples-net11-0-jettywindows-global-json}}

### samples/net11.0/JettyWindows/Handlers.cs

{{snippet:samples-net11-0-jettywindows-handlers-cs}}

### samples/net11.0/JettyWindows/JettyPage.cs

{{snippet:samples-net11-0-jettywindows-jettypage-cs}}

### samples/net11.0/JettyWindows/JettyWindows.csproj

{{snippet:samples-net11-0-jettywindows-jettywindows-csproj}}

### samples/net11.0/JettyWindows/LegacyDefault.props

{{snippet:samples-net11-0-jettywindows-legacydefault-props}}

### samples/net11.0/JettyWindows/MauiProgram.cs

{{snippet:samples-net11-0-jettywindows-mauiprogram-cs}}

### samples/net11.0/JettyWindows/Platforms/Windows/App.xaml

{{snippet:samples-net11-0-jettywindows-platforms-windows-app-xaml}}

### samples/net11.0/JettyWindows/Platforms/Windows/App.xaml.cs

{{snippet:samples-net11-0-jettywindows-platforms-windows-app-xaml-cs}}

## Official references

- [MAUI11 handler migration](https://learn.microsoft.com/dotnet/maui/whats-new/dotnet-11)
- [Pinned native default registrations](https://raw.githubusercontent.com/dotnet/maui/d32b79928709cf53b671336dabd0caef5a679fc2/src/Controls/src/Core/Hosting/AppHostBuilderExtensions.cs)

The ShellOptIn.props and LegacyDefault.props examples are configuration fragments:
copy the property into the actual project PropertyGroup or explicitly import the
fragment. Merely placing an unimported .props file beside a project has no effect.
