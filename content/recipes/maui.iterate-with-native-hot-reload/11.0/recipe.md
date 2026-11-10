# Edit a signal screen while it is running

## Goal

A radio operator has already marked a signal inspected. Change the spacing or label formatting without navigating away and check that the existing page updates. The SignalDeck page provides editable markup and a button with visible local state.

## Older approach

Build and run the Windows comparison to inspect each saved screen change, or use the supported IDE XAML Hot Reload workflow for its platform. The older sample preserves the same callsign, title and inspection interaction using typed C# bindings. .NET10 does not provide the new .NET11 preview incremental CLI engine; an older CLI redeployment is not equivalent to updating a live page. Existing IDE reload remains a useful workflow.

## Newer approach

{{snippet:samples-net11-0-signaldeck-signalpage-xaml}}

Incremental XAML Hot Reload applies patches to live instances of x:Class-backed pages through a MetadataUpdateHandler. Debug enables it by default; Release and publish disable it by default. This default does not make the engine stable. It also enables XAML updates in dotnet watch sessions. No EnablePreviewFeatures or x:Code expression is needed by this sample.

## Migration

1. Use a suitable .NET11 native authoring installation. Select SignalDeckAndroid (net11.0-android37.2) or SignalDeckiOS (net11.0-ios27.0); SignalDeck itself is the separate Windows example. Keep the workload set and Controls package compatible; intended RC2 pins and platform floors are shown below.
2. In the current SignalDeckAndroid project directory, start the SDK's absolute dotnet executable with `watch --project SignalDeckAndroid.csproj`. On a Mac use `watch --project SignalDeckiOS.csproj` in that project instead. Each project selects one explicit target framework; the interactive workflow selects the device/emulator. Select Android on this Windows development route; use a Mac for iOS. Do not mistake the Windows target or the selected Playbook catalogue edition for selection of an Android/iOS deployment target. Verify the project's framework and selected device in the launch output before editing. A noninteractive terminal cannot exercise the picker.
3. Deploy the Debug app, press Inspect signal, and keep that page visible. Edit VerticalStackLayout.Spacing from 12 to 20 in SignalPage.xaml and save. Expect the existing instance to change layout while its inspection label remains visible. Then edit the Echo binding's StringFormat and save: RC2 preserves the x:Reference source. These are expected checks; no native watch session ran here.
4. Restore the markup. Property/binding edits, local resources and supported child additions/removals/reordering can be incremental. Changes to the root type, files/packages or new fields needed by C# require rebuild/redeploy. A newly introduced x:Name cannot add a field to an already loaded type. Stop watch with Ctrl+C before changing project dependencies.
5. To opt out, add `<EnableMauiIncrementalHotReload>false</EnableMauiIncrementalHotReload>` to a PropertyGroup conditioned on Debug, then rebuild. This selects the existing engine path where supported; it does not promise an equivalent CLI experience. Keep the preview engine disabled for Release/publish and never use reload success as trimmed-publish proof.

## Current iOS boundary

The early Preview4 workaround set MtouchLink=None for simulator watch sessions. The upstream issue25295 maintainer reports that a plain MAUI simulator project works from Preview6 without changes. The cumulative overview still repeats the older workaround. Do not copy that setting into this RC2 sample or into Release builds just to follow the old overview. Diagnose the selected workload/runtime and actual startup failure first. This is an upstream resolution, not a local iOS test. The known-issues page still excludes launching iOS/tvOS mobile apps through the CLI on Windows: use a Mac for the iOS CLI workflow. Later notes describe physical Apple-device watch support over USB/Wi-Fi; device preparation, signing and trust still apply. A simulator result does not prove physical-device support for this sample.

## Constraints

Use .NET11 SDK/runtime and matching native MAUI assets. Intended sample baseline: SDK11.0.100-rc.3.26522.148, Controls11.0.0-rc.2.26505.5, C#15, SourceGen, workload set11.0.100-rc.2.26513.2. Its reviewed mapping selects Android37.2.0-rc.2.84 and Apple27.0.12212-net11-rc.2. Android preparation requires the matching Android SDK/JDK25; Apple preparation requires the matching Xcode27.0 baseline. These are intended authoring pins, not measured native installations. Platform binding levels differ from minimum deployment OS versions. The canonical project records those separately. No external provider, account or web renderer is used by Playbook. IDE reload or rebuild/redeploy is an alternative when the CLI route is unsuitable.

## Evidence

The .NET 11 SDK baseline is now 11.0.100 GA with preview selection disabled. The .NET 10 comparisons retain their declared SDKs. Earlier RC audit details below are historical; use the current sample pins, dependency locks and GA verification records for current coverage. Existing native exclusions and pending Human gates still apply.

The .NET10 Windows comparison builds in Release with zero warnings/errors. The .NET11 project cannot compile on this host: SDK11 lacks MAUI SDK packs (NETSDK1147), and Android/iOS native toolchains/devices are unavailable. Current sample compilation, target selection/device picker interaction, live-instance state retention, engine opt-out behavior and iOS simulator/device execution are **not-tested** under PIN-699. No Human approval is pending and no native installation is requested by this exclusion. Official source/target and RC2 artifact review supports the instructions and preview/default declarations; it is not execution evidence. Neither this older build nor an installed MCP response proves Hot Reload occurred.

verification.json/verification-evidence.json preserve exact source inputs, source-review classification and freshness. samples/SignalDeck/sources.json retains official source hashes and the precise upstream iOS resolution. Packed installation/list/get checks prove offline delivery of the full recipe, applicability and target edition, with unusable proxies rather than network isolation. No new native test matrix, provisioning or public publication is authorized.

## Canonical samples

### samples/net10.0/SignalDeck/Directory.Build.props

{{snippet:samples-net10-0-signaldeck-directory-build-props}}

### samples/net10.0/SignalDeck/Directory.Build.targets

{{snippet:samples-net10-0-signaldeck-directory-build-targets}}

### samples/net10.0/SignalDeck/Directory.Packages.props

{{snippet:samples-net10-0-signaldeck-directory-packages-props}}

### samples/net10.0/SignalDeck/global.json

{{snippet:samples-net10-0-signaldeck-global-json}}

### samples/net10.0/SignalDeck/MauiProgram.cs

{{snippet:samples-net10-0-signaldeck-mauiprogram-cs}}

### samples/net10.0/SignalDeck/Platforms/Windows/App.xaml

{{snippet:samples-net10-0-signaldeck-platforms-windows-app-xaml}}

### samples/net10.0/SignalDeck/Platforms/Windows/App.xaml.cs

{{snippet:samples-net10-0-signaldeck-platforms-windows-app-xaml-cs}}

### samples/net10.0/SignalDeck/SignalDeck.csproj

{{snippet:samples-net10-0-signaldeck-signaldeck-csproj}}

### samples/net10.0/SignalDeck/SignalPage.xaml

{{snippet:samples-net10-0-signaldeck-signalpage-xaml}}

### samples/net10.0/SignalDeck/SignalPage.xaml.cs

{{snippet:samples-net10-0-signaldeck-signalpage-xaml-cs}}

### samples/net11.0/SignalDeck/BindingFailures.txt

{{snippet:samples-net11-0-signaldeck-bindingfailures-txt}}

### samples/net11.0/SignalDeck/Directory.Build.props

{{snippet:samples-net11-0-signaldeck-directory-build-props}}

### samples/net11.0/SignalDeck/Directory.Build.targets

{{snippet:samples-net11-0-signaldeck-directory-build-targets}}

### samples/net11.0/SignalDeck/Directory.Packages.props

{{snippet:samples-net11-0-signaldeck-directory-packages-props}}

### samples/net11.0/SignalDeck/global.json

{{snippet:samples-net11-0-signaldeck-global-json}}

### samples/net11.0/SignalDeck/MauiProgram.cs

{{snippet:samples-net11-0-signaldeck-mauiprogram-cs}}

### samples/net11.0/SignalDeck/Platforms/Windows/App.xaml

{{snippet:samples-net11-0-signaldeck-platforms-windows-app-xaml}}

### samples/net11.0/SignalDeck/Platforms/Windows/App.xaml.cs

{{snippet:samples-net11-0-signaldeck-platforms-windows-app-xaml-cs}}

### samples/net11.0/SignalDeck/SignalDeck.csproj

{{snippet:samples-net11-0-signaldeck-signaldeck-csproj}}


### samples/net11.0/SignalDeck/SignalPage.xaml.cs

{{snippet:samples-net11-0-signaldeck-signalpage-xaml-cs}}

### samples/net11.0/SignalDeckAndroid/Directory.Build.props

{{snippet:samples-net11-0-signaldeckandroid-directory-build-props}}

### samples/net11.0/SignalDeckAndroid/Directory.Build.targets

{{snippet:samples-net11-0-signaldeckandroid-directory-build-targets}}

### samples/net11.0/SignalDeckAndroid/Directory.Packages.props

{{snippet:samples-net11-0-signaldeckandroid-directory-packages-props}}

### samples/net11.0/SignalDeckAndroid/global.json

{{snippet:samples-net11-0-signaldeckandroid-global-json}}

### samples/net11.0/SignalDeckAndroid/MauiProgram.cs

{{snippet:samples-net11-0-signaldeckandroid-mauiprogram-cs}}

### samples/net11.0/SignalDeckAndroid/Platforms/Android/AndroidManifest.xml

{{snippet:samples-net11-0-signaldeckandroid-platforms-android-androidmanifest-xml}}

### samples/net11.0/SignalDeckAndroid/Platforms/Android/MainActivity.cs

{{snippet:samples-net11-0-signaldeckandroid-platforms-android-mainactivity-cs}}

### samples/net11.0/SignalDeckAndroid/Platforms/Android/MainApplication.cs

{{snippet:samples-net11-0-signaldeckandroid-platforms-android-mainapplication-cs}}

### samples/net11.0/SignalDeckAndroid/SignalDeckAndroid.csproj

{{snippet:samples-net11-0-signaldeckandroid-signaldeckandroid-csproj}}

### samples/net11.0/SignalDeckAndroid/SignalPage.xaml

{{snippet:samples-net11-0-signaldeckandroid-signalpage-xaml}}

### samples/net11.0/SignalDeckAndroid/SignalPage.xaml.cs

{{snippet:samples-net11-0-signaldeckandroid-signalpage-xaml-cs}}

### samples/net11.0/SignalDeckiOS/Directory.Build.props

{{snippet:samples-net11-0-signaldeckios-directory-build-props}}

### samples/net11.0/SignalDeckiOS/Directory.Build.targets

{{snippet:samples-net11-0-signaldeckios-directory-build-targets}}

### samples/net11.0/SignalDeckiOS/Directory.Packages.props

{{snippet:samples-net11-0-signaldeckios-directory-packages-props}}

### samples/net11.0/SignalDeckiOS/global.json

{{snippet:samples-net11-0-signaldeckios-global-json}}

### samples/net11.0/SignalDeckiOS/MauiProgram.cs

{{snippet:samples-net11-0-signaldeckios-mauiprogram-cs}}

### samples/net11.0/SignalDeckiOS/Platforms/iOS/AppDelegate.cs

{{snippet:samples-net11-0-signaldeckios-platforms-ios-appdelegate-cs}}

### samples/net11.0/SignalDeckiOS/Platforms/iOS/Info.plist

{{snippet:samples-net11-0-signaldeckios-platforms-ios-info-plist}}

### samples/net11.0/SignalDeckiOS/Platforms/iOS/Program.cs

{{snippet:samples-net11-0-signaldeckios-platforms-ios-program-cs}}

### samples/net11.0/SignalDeckiOS/SignalDeckiOS.csproj

{{snippet:samples-net11-0-signaldeckios-signaldeckios-csproj}}

### samples/net11.0/SignalDeckiOS/SignalPage.xaml

{{snippet:samples-net11-0-signaldeckios-signalpage-xaml}}

### samples/net11.0/SignalDeckiOS/SignalPage.xaml.cs

{{snippet:samples-net11-0-signaldeckios-signalpage-xaml-cs}}

### iOS scene bootstrap

{{snippet:samples-net11-0-signaldeckios-platforms-ios-scenedelegate-cs}}

## Official references

- [Preview7 official source](https://raw.githubusercontent.com/dotnet/core/60871330fce2b3da383461be027ebdf3ce30f8fb/release-notes/11.0/preview/preview7/dotnetmaui.md)
- [Rc2 official source](https://raw.githubusercontent.com/dotnet/core/60871330fce2b3da383461be027ebdf3ce30f8fb/release-notes/11.0/preview/rc2/dotnetmaui.md)
- [Overview official source](https://raw.githubusercontent.com/dotnet/docs-maui/e4fe7f5b5fbb367c030227c6ef0db6a2c8afa21b/docs/whats-new/dotnet-11.md)
- [Reload official source](https://raw.githubusercontent.com/dotnet/docs-maui/e4fe7f5b5fbb367c030227c6ef0db6a2c8afa21b/docs/xaml/hot-reload.md)
- [Relative Tests official source](https://raw.githubusercontent.com/dotnet/maui/d32b79928709cf53b671336dabd0caef5a679fc2/src/Controls/tests/SourceGen.UnitTests/InitializeComponent/RelativeSourceBindings.cs)
- [Compiled Tests official source](https://raw.githubusercontent.com/dotnet/maui/d32b79928709cf53b671336dabd0caef5a679fc2/src/Controls/tests/SourceGen.UnitTests/InitializeComponent/CompiledBindings.cs)
- [Diagnostics official source](https://raw.githubusercontent.com/dotnet/maui/d32b79928709cf53b671336dabd0caef5a679fc2/src/Controls/tests/SourceGen.UnitTests/BindingDiagnosticsTests.cs)
- [Xaml Targets official source](https://raw.githubusercontent.com/dotnet/maui/d32b79928709cf53b671336dabd0caef5a679fc2/src/Controls/src/Build.Tasks/nuget/buildTransitive/netstandard2.0/Microsoft.Maui.Controls.targets)
- [Known official source](https://raw.githubusercontent.com/wiki/dotnet/macios/Known-issues-in-.NET11.md)
- [Ios Fix official source](https://github.com/dotnet/macios/issues/25295#issuecomment-4994500229)

- [MAUI RC2 iOS scene bootstrap](https://raw.githubusercontent.com/dotnet/maui/d32b79928709cf53b671336dabd0caef5a679fc2/src/Templates/src/templates/maui-mobile/Platforms/iOS/Info.plist)
