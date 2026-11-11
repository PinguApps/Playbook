# Keep the signal deck connected after trimming

## Goal

A radio operator edits a callsign and sees the same text below it. Another label displays the containing page title. Both releases solve that same task; the newer version moves these two connections into XAML.

## Older approach

SignalDeck uses typed `Binding.Create` delegates in its constructor. The entry and page are explicit sources, and the compiler sees the accessed properties. This remains a valid approach when control lifetime and source selection are clearer in code. It does not need a string-based ancestor search. The Windows comparison was built with SDK10.0.403 and Controls10.0.110; it is not an older-target route to the .NET11 generator.

## Newer approach

{{snippet:samples-net11-0-signaldeck-signalpage-xaml}}

With `MauiXamlInflator=SourceGen`, `Echo` refers to the named `Callsign` Entry and binds its Text property. `Deck` searches for the containing SignalPage and binds its Title. A resolvable ancestor type produces a `TypedBinding<TAncestor,TProperty>` rather than a reflection-dependent string path. SourceGen resolves the reference element's actual type, including references from lazily created templates in RC2. Ordinary SourceGen bindings do not require preview C# expressions or `EnablePreviewFeatures`.

An ancestor deriving from Element selects FindAncestor; a non-Element ancestor type selects FindAncestorBindingContext. Set the mode explicitly if that inference does not express the desired source. Moving a target to a different visual tree can change the ancestor it finds even though its property path compiles.

## Migration

1. Keep the sample's isolated build/package boundaries and exact SDK/Controls pins. Install the matching native workload on a suitable authoring host. The .NET11 examples use separate Windows, Android and iOS projects, so select the project for the platform being checked.
2. Compare the original page and typed constructor with the current page. Move each connection into XAML only after identifying the actual source type and namescope. Keep shared BindingContext bindings correctly annotated with x:DataType; it is not a substitute for resolving the explicit source.
3. Build Release with SourceGen. Inspect generated compiler output for the ancestor/reference typed property accesses. The expected labels are the edited callsign and the page title. A successful managed Debug build alone does not establish trimmed behavior.
4. In a disposable copy, replace Echo's binding with the MissingCallsign probe. An unresolved x:Reference name must fail XAML compilation with MAUIG1001; an unresolved AncestorType also fails compilation. These are source-backed expectations, not diagnostics observed on this host.
5. Restore the valid page. The second probe deliberately accesses DisplayName through an untyped BindingContext. Resolving the named source does not guarantee that this property path compiles: an uncompileable path uses the runtime binding fallback. Self and TemplatedParent also remain runtime paths. Do not treat silence or suppression of a diagnostic as trimming proof. Keep such bindings out of a full-trimming/AOT claim until replaced with a typed route or separately preserved and tested.
6. Publish the intended supported platform/RID with full trimming, examine linker diagnostics and generated output, and run the published artifact. Change Callsign.Text and the page title and verify both labels. No trimmed/native output was produced here; the excluded checks below cannot be inferred from the older Windows build.

## Constraints

This edition targets .NET11 MAUI and Controls11 assets for the selected native platform, with the SourceGen build analyzer. The canonical project pins SDK11 GA, C#15 and Controls11.0.0. Platform bindings, deployment floors and exact intended workload composition are in Applicability and the canonical project. Android needs its SDK/JDK and a device/emulator; iOS requires a Mac, matching Xcode, signing where needed and a simulator/device. Windows has a separate WinUI host. There is no web rendering mode or external provider.

Typed C# bindings remain useful when runtime ancestor selection is unnecessary. Preserve intentional runtime bindings on supported untrimmed paths rather than describing all older applications as broken. This recipe's bindings are stable framework guidance prepared from RC2; it makes no GA or AOT execution claim.

## Evidence

The .NET 11 SDK baseline is now 11.0.100 GA with preview selection disabled. The .NET 10 comparisons retain their declared SDKs. Earlier RC audit details below are historical; use the current sample pins, dependency locks and GA verification records for current coverage. Existing native exclusions and pending Human gates still apply.

The original .NET10 Windows sample compiles in Release with zero warnings/errors. The .NET11 Windows attempt, restricted to that target, fails NETSDK1147 before compilation because this SDK root has no MAUI SDK pack. Android/iOS toolchains and devices are unavailable. Under PIN-699, current sample compilation, actual generated typed paths, negative diagnostics and trimmed/native execution are explicitly **not-tested**, rather than pending Human work. No upstream tests were executed. Official source tests and exact reviewed source hashes support the expected compiler behavior; they do not prove this sample ran.

Canonical inputs and freshness are recorded in verification.json/verification-evidence.json. Sources are pinned in samples/SignalDeck/sources.json; the mobile composition audit supplies exact reviewed package assets. Packed-tool inspection independently verifies complete offline Markdown and applicability. No service provisioning or public publication is part of this recipe.

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
