# Customize a scoped Android entry through public Material handlers

## Goal

Replace access to internal Material helpers with a public handler extension for one custom entry type; leave stock control styling to MAUI.

## Constraints

The fictional Marsh Survey app gives its custom station entry a native accessibility description. .NET10 uses SDK10.0.402, C#14 and Controls10.0.110; .NET11 uses SDK11.0.100-rc.2.26504.105, C#15 and Controls11.0.0-rc.2.26505.5. These are pinned preparation baselines, not GA execution evidence. No account, provider or web rendering mode is required.

## Scope the handler

SurveyEntry alone registers SurveyHandler. The adjacent stock Entry remains untouched. .NET10 uses its public EntryHandler and MauiAppCompatEditText; that remains a valid customization route without reflection. .NET11 uses public EntryHandler2 and MauiMaterialTextInputLayout. Calling base.CreatePlatformView retains the default native child wiring and changes only the native accessibility description. Do not substitute MauiMaterialEditText as the layout return type. Public helpers can also be subclassed when necessary; this small extension does not replace the inner edit control or bypass its event wiring.

This authored change is separate from automatic Material3 stock styling. Verify the custom entry announcement with Android accessibility tooling and confirm the stock entry still has its normal behavior; neither interaction was run here.

## Older approach

Use the net10.0 canonical project below; its public API route remains valid within the stated limits.

## Newer approach

Use the net11.0 canonical project below with its matched platform packages.

{{snippet:samples-net11-0-marshentry-mauiprogram-cs}}

## Migration

1. Select the matched SDK, workload and package pins from the canonical project.
2. Register SurveyHandler for SurveyEntry only; keep the stock Entry registration unchanged.
3. Call base.CreatePlatformView before setting the custom native accessibility description. Keep the older public EntryHandler route when it meets the app requirements.
4. Build Release and run the described native checks before relying on platform behavior.

## Canonical previous and current samples

### samples/net10.0/MarshEntry/Directory.Build.props

{{snippet:samples-net10-0-marshentry-directory-build-props}}

### samples/net10.0/MarshEntry/Directory.Build.targets

{{snippet:samples-net10-0-marshentry-directory-build-targets}}

### samples/net10.0/MarshEntry/Directory.Packages.props

{{snippet:samples-net10-0-marshentry-directory-packages-props}}

### samples/net10.0/MarshEntry/global.json

{{snippet:samples-net10-0-marshentry-global-json}}

### samples/net10.0/MarshEntry/MarshEntry.csproj

{{snippet:samples-net10-0-marshentry-marshentry-csproj}}

### samples/net10.0/MarshEntry/MauiProgram.cs

{{snippet:samples-net10-0-marshentry-mauiprogram-cs}}

### samples/net10.0/MarshEntry/Platforms/Android/AndroidManifest.xml

{{snippet:samples-net10-0-marshentry-platforms-android-androidmanifest-xml}}

### samples/net10.0/MarshEntry/Platforms/Android/MainActivity.cs

{{snippet:samples-net10-0-marshentry-platforms-android-mainactivity-cs}}

### samples/net10.0/MarshEntry/Platforms/Android/MainApplication.cs

{{snippet:samples-net10-0-marshentry-platforms-android-mainapplication-cs}}

### samples/net11.0/MarshEntry/Directory.Build.props

{{snippet:samples-net11-0-marshentry-directory-build-props}}

### samples/net11.0/MarshEntry/Directory.Build.targets

{{snippet:samples-net11-0-marshentry-directory-build-targets}}

### samples/net11.0/MarshEntry/Directory.Packages.props

{{snippet:samples-net11-0-marshentry-directory-packages-props}}

### samples/net11.0/MarshEntry/global.json

{{snippet:samples-net11-0-marshentry-global-json}}

### samples/net11.0/MarshEntry/MarshEntry.csproj

{{snippet:samples-net11-0-marshentry-marshentry-csproj}}

### samples/net11.0/MarshEntry/MauiProgram.cs

{{snippet:samples-net11-0-marshentry-mauiprogram-cs}}

### samples/net11.0/MarshEntry/Platforms/Android/AndroidManifest.xml

{{snippet:samples-net11-0-marshentry-platforms-android-androidmanifest-xml}}

### samples/net11.0/MarshEntry/Platforms/Android/MainActivity.cs

{{snippet:samples-net11-0-marshentry-platforms-android-mainactivity-cs}}

### samples/net11.0/MarshEntry/Platforms/Android/MainApplication.cs

{{snippet:samples-net11-0-marshentry-platforms-android-mainapplication-cs}}

## Evidence

Official source hashes are retained in samples/MarshInterop/sources.json and checked by eng/Test-MarshInterop.ps1. Evidence records distinguish source review from compilation. Android device rendering and accessibility interaction not tested; .NET11 Android compilation excluded where matched SDK11 MAUI workload unavailable. No pending Human obligation under PIN-699. No native execution or GA verification is claimed. Installed-package list/get verifies offline content delivery only. No verified older-target route is declared for this edition.

## Official references

- [EntryHandler2.Android.cs official source](https://raw.githubusercontent.com/dotnet/maui/d32b79928709cf53b671336dabd0caef5a679fc2/src/Core/src/Handlers/Entry/EntryHandler2.Android.cs)
- [MauiMaterialEditText.cs official source](https://raw.githubusercontent.com/dotnet/maui/d32b79928709cf53b671336dabd0caef5a679fc2/src/Core/src/Platform/Android/Material3Controls/MauiMaterialEditText.cs)
- [EntryHandler.Android.cs official source](https://raw.githubusercontent.com/dotnet/maui/9d881e94b1c56692e40a16262897677a3bb46fcb/src/Core/src/Handlers/Entry/EntryHandler.Android.cs)
