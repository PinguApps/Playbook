# Keep Android harbour navigation when upgrading MAUI

## Goal

Preserve an Android Shell flyout's harbour colours and destinations while removing an obsolete renderer registration and the optional compatibility-package dependency. This is the Android portion of the shared navigation-handler goal; Apple NavigationPage, TabbedPage, FlyoutPage and opt-in Apple Shell migration remain separate work.

## Older approach

{{snippet:samples-net10-0-harbourandroid-mauiprogram-cs}}

The .NET 10 canonical Android app explicitly registers the Controls.Core ShellRenderer for HarbourShell. Styling is already expressed through public Shell properties. The renderer route was valid; retaining it in .NET 10 does not make the application unsupported. The sample does not need the optional Microsoft.Maui.Controls.Compatibility package to obtain ShellRenderer: its compatibility namespace is not its package identity.

## Newer approach

{{snippet:samples-net11-0-harbourandroid-mauiprogram-cs}}

The .NET 11 Android app removes that registration and lets MAUI select its default handler-based Shell architecture. FlyoutBackgroundColor and the two destinations remain in shared code. A shared property is sufficient for this customization; do not port it into a native handler override merely to preserve the old structure.

For a real app that explicitly references Microsoft.Maui.Controls.Compatibility, remove its PackageReference and associated central version only after replacing the renderer types it actually supplies. That optional package is no longer built/shipped in .NET 11. Search project references, using directives, renderer registration and platform subclasses; inspect transitive references too. Replace each required customization with a supported control/handler or public property before removing its dependency. Do not add a fictional version 11 compatibility package or pin an older incompatible package to conceal the migration.

Crucially, `Microsoft.Maui.Controls.Handlers.Compatibility.ShellRenderer` remains in Controls.Core, and an explicit Android registration can retain the legacy Shell route while evaluating a specific regression. Compatibility-named Controls.Core classes are not removed by association. The default Android handler is `Microsoft.Maui.Controls.Handlers.ShellHandler`. If a genuine native customization remains, use its supported handler/property mapper and limit the mapping to your Shell subclass; global mapper changes otherwise affect every matching control. Pair any native event subscription with disconnection and avoid retaining old pages across reconnects. This example needs no mapper or native event subscription.

## Migration and checks

1. Inventory the actual dependency assets rather than deleting every compatibility namespace. Separate the removed optional package from surviving Controls.Core types.
2. Move supported styling to Shell properties. Remove the renderer registration to select Android's default handler. Keep any temporary legacy registration explicit and document the reason.
3. Build the actual Android project with the matching workload and inspect the resolved package graph for the removed package. On an available device, verify flyout open/close, destination switching, back navigation and accessibility. Source review does not establish those interactions.

Do not extrapolate this Android default to Apple. At the reviewed RC2 baseline, `UseiOSShellHandler` is an opt-in and the compatibility ShellRenderer remains the Apple default. Apple NavigationPage timing changes and item-handler migrations are outside this Android edition.

## Constraints

This edition covers Android only. Use matching Android build tools and the MAUI SDK for the selected release. It does not migrate Apple navigation or authorize dependency downgrades, package publication or native provisioning. Inspect package identity before removing a type merely because its namespace contains Compatibility.

## Evidence

Canonical previous/current projects use SDK10.0.402/C#14/Controls10.0.110 with net10.0-android36.0 and SDK11.0.100-rc.2.26504.105/C#15/Controls11.0.0-rc.2.26505.5 with net11.0-android37.0. Minimum Android API24 is declared explicitly. Matching native SDK/workloads and Android build tools are required; CoreCLR is the normal runtime, with no NativeAOT claim. This is native rendering with no provider, account or preview-language opt-in. Stable API guidance uses RC2 evidence, not GA verification.

The older Android project built in Release with the actual installed Android36.1.118 and MAUI10.0.110 toolchain. It was not deployed to a device. The current Android sample is official-review only: the SDK11 installation lacks Microsoft.Maui.Sdk. Current Android compilation and all device behavior are not-tested. This exclusion follows PIN-699 and creates no obligation to acquire another environment. No separately ticketed Human proof is linked to these targets. The source manifest retains exact reviewed source identities; real installed-tool list/get checks establish guidance delivery only. No package, account or resource publication/provisioning is performed.

## Canonical files

### samples/net10.0/HarbourAndroid/Directory.Build.props

{{snippet:samples-net10-0-harbourandroid-directory-build-props}}

### samples/net10.0/HarbourAndroid/Directory.Build.targets

{{snippet:samples-net10-0-harbourandroid-directory-build-targets}}

### samples/net10.0/HarbourAndroid/Directory.Packages.props

{{snippet:samples-net10-0-harbourandroid-directory-packages-props}}

### samples/net10.0/HarbourAndroid/global.json

{{snippet:samples-net10-0-harbourandroid-global-json}}

### samples/net10.0/HarbourAndroid/HarbourAndroid.csproj

{{snippet:samples-net10-0-harbourandroid-harbourandroid-csproj}}

### samples/net10.0/HarbourAndroid/MauiProgram.cs

{{snippet:samples-net10-0-harbourandroid-mauiprogram-cs}}

### samples/net10.0/HarbourAndroid/Platforms/Android/AndroidManifest.xml

{{snippet:samples-net10-0-harbourandroid-platforms-android-androidmanifest-xml}}

### samples/net10.0/HarbourAndroid/Platforms/Android/MainActivity.cs

{{snippet:samples-net10-0-harbourandroid-platforms-android-mainactivity-cs}}

### samples/net10.0/HarbourAndroid/Platforms/Android/MainApplication.cs

{{snippet:samples-net10-0-harbourandroid-platforms-android-mainapplication-cs}}

### samples/net11.0/HarbourAndroid/Directory.Build.props

{{snippet:samples-net11-0-harbourandroid-directory-build-props}}

### samples/net11.0/HarbourAndroid/Directory.Build.targets

{{snippet:samples-net11-0-harbourandroid-directory-build-targets}}

### samples/net11.0/HarbourAndroid/Directory.Packages.props

{{snippet:samples-net11-0-harbourandroid-directory-packages-props}}

### samples/net11.0/HarbourAndroid/global.json

{{snippet:samples-net11-0-harbourandroid-global-json}}

### samples/net11.0/HarbourAndroid/HarbourAndroid.csproj

{{snippet:samples-net11-0-harbourandroid-harbourandroid-csproj}}

### samples/net11.0/HarbourAndroid/MauiProgram.cs

{{snippet:samples-net11-0-harbourandroid-mauiprogram-cs}}

### samples/net11.0/HarbourAndroid/Platforms/Android/AndroidManifest.xml

{{snippet:samples-net11-0-harbourandroid-platforms-android-androidmanifest-xml}}

### samples/net11.0/HarbourAndroid/Platforms/Android/MainActivity.cs

{{snippet:samples-net11-0-harbourandroid-platforms-android-mainactivity-cs}}

### samples/net11.0/HarbourAndroid/Platforms/Android/MainApplication.cs

{{snippet:samples-net11-0-harbourandroid-platforms-android-mainapplication-cs}}

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
