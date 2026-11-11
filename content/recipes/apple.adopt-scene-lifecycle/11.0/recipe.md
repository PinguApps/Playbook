# Move Apple launch handling to window scenes

## Goal

Migrate existing iOS and Mac Catalyst startup and launch actions to the scene lifecycle required by Xcode 27 while preserving a single window.

## Newer approach

The Puffin notebook receives a new-observation shortcut when launched or while open.
The previous application-delegate implementation is valid on its older toolchain.
With Xcode27 SDKs, iOS and Mac Catalyst require a scene manifest and delegate.
Move window creation and cold launch data to scene connection; warm shortcut actions
arrive on the window-scene delegate. Do not process the same launch in both places.

MAUI owns window creation: derive from MauiUISceneDelegate and preserve its base
implementation. Use the exact default configuration key shown below. The MAUI
variant observes cold and warm URLs through scene lifecycle events; its shortcut
registration acknowledges every invocation, including false for unknown actions.
The framework dispatches cold shortcuts after activation. Do not dispatch them again
from custom connection code. A logging-only shortcut observer must complete(false).

### net11-0-puffinmaui-app-cs

{{snippet:net11-0-puffinmaui-app-cs}}

### net11-0-puffinmaui-appdelegate-cs

{{snippet:net11-0-puffinmaui-appdelegate-cs}}

### net11-0-puffinmaui-directory-build-props

{{snippet:net11-0-puffinmaui-directory-build-props}}

### net11-0-puffinmaui-directory-build-targets

{{snippet:net11-0-puffinmaui-directory-build-targets}}

### net11-0-puffinmaui-directory-packages-props

{{snippet:net11-0-puffinmaui-directory-packages-props}}

### net11-0-puffinmaui-global-json

{{snippet:net11-0-puffinmaui-global-json}}

### net11-0-puffinmaui-info-plist

{{snippet:net11-0-puffinmaui-info-plist}}

### net11-0-puffinmaui-mauiprogram-cs

{{snippet:net11-0-puffinmaui-mauiprogram-cs}}

### net11-0-puffinmaui-program-cs

{{snippet:net11-0-puffinmaui-program-cs}}

### net11-0-puffinmaui-puffinmaui-csproj

{{snippet:net11-0-puffinmaui-puffinmaui-csproj}}

### net11-0-puffinmaui-scenedelegate-cs

{{snippet:net11-0-puffinmaui-scenedelegate-cs}}

### net11-0-puffinscenes-appdelegate-cs

{{snippet:net11-0-puffinscenes-appdelegate-cs}}

### net11-0-puffinscenes-directory-build-props

{{snippet:net11-0-puffinscenes-directory-build-props}}

### net11-0-puffinscenes-directory-build-targets

{{snippet:net11-0-puffinscenes-directory-build-targets}}

### net11-0-puffinscenes-directory-packages-props

{{snippet:net11-0-puffinscenes-directory-packages-props}}

### net11-0-puffinscenes-global-json

{{snippet:net11-0-puffinscenes-global-json}}

### net11-0-puffinscenes-info-plist

{{snippet:net11-0-puffinscenes-info-plist}}

### net11-0-puffinscenes-observationview-cs

{{snippet:net11-0-puffinscenes-observationview-cs}}

### net11-0-puffinscenes-program-cs

{{snippet:net11-0-puffinscenes-program-cs}}

### net11-0-puffinscenes-puffinscenes-csproj

{{snippet:net11-0-puffinscenes-puffinscenes-csproj}}

### net11-0-puffinscenes-scenedelegate-cs

{{snippet:net11-0-puffinscenes-scenedelegate-cs}}

## Older approach

The .NET10 canonical comparison below solves the same observation-notebook goal.
It remains valid for its declared older toolchain. The .NET11 edition supplies no
older-target route and leaves the eight curated .NET10 foundations unchanged.

### net10-0-puffinmaui-app-cs

{{snippet:net10-0-puffinmaui-app-cs}}

### net10-0-puffinmaui-appdelegate-cs

{{snippet:net10-0-puffinmaui-appdelegate-cs}}

### net10-0-puffinmaui-directory-build-props

{{snippet:net10-0-puffinmaui-directory-build-props}}

### net10-0-puffinmaui-directory-build-targets

{{snippet:net10-0-puffinmaui-directory-build-targets}}

### net10-0-puffinmaui-directory-packages-props

{{snippet:net10-0-puffinmaui-directory-packages-props}}

### net10-0-puffinmaui-global-json

{{snippet:net10-0-puffinmaui-global-json}}

### net10-0-puffinmaui-info-plist

{{snippet:net10-0-puffinmaui-info-plist}}

### net10-0-puffinmaui-mauiprogram-cs

{{snippet:net10-0-puffinmaui-mauiprogram-cs}}

### net10-0-puffinmaui-program-cs

{{snippet:net10-0-puffinmaui-program-cs}}

### net10-0-puffinmaui-puffinmaui-csproj

{{snippet:net10-0-puffinmaui-puffinmaui-csproj}}

### net10-0-puffinscenes-appdelegate-cs

{{snippet:net10-0-puffinscenes-appdelegate-cs}}

### net10-0-puffinscenes-directory-build-props

{{snippet:net10-0-puffinscenes-directory-build-props}}

### net10-0-puffinscenes-directory-build-targets

{{snippet:net10-0-puffinscenes-directory-build-targets}}

### net10-0-puffinscenes-directory-packages-props

{{snippet:net10-0-puffinscenes-directory-packages-props}}

### net10-0-puffinscenes-global-json

{{snippet:net10-0-puffinscenes-global-json}}

### net10-0-puffinscenes-info-plist

{{snippet:net10-0-puffinscenes-info-plist}}

### net10-0-puffinscenes-observationview-cs

{{snippet:net10-0-puffinscenes-observationview-cs}}

### net10-0-puffinscenes-program-cs

{{snippet:net10-0-puffinscenes-program-cs}}

### net10-0-puffinscenes-puffinscenes-csproj

{{snippet:net10-0-puffinscenes-puffinscenes-csproj}}

## Migration

1. Inspect the actual target framework, workload and OS minimum before changing startup.
2. Compare the canonical declarations below with the existing application configuration.
3. Move only the shown configuration/callback responsibilities; preserve unrelated permissions and launch handling.
4. Keep one owner per launch action and acknowledge each completion exactly once.
5. Build with the declared workload, then test cold launch, warm launch, unknown actions and ordinary back/foreground behaviour on a supported device.

## Constraints

Use the explicit runtime, SDK, package assets, platforms and requirements returned
with this recipe. These samples target .NET11 with a .NET10 comparison. The ordinary
Playbook host requires no native workload. No external provider, account, render
mode or resource provisioning is involved. APIs shown are stable contracts;
the evidence covers GA reference/API inspection; native GA behavior remains not-tested.

## Evidence

The .NET 11 SDK baseline is now 11.0.100 GA with preview selection disabled. The .NET 10 comparisons retain their declared SDKs. Use the current sample pins, dependency locks and GA verification records for current coverage. Existing native exclusions and pending Human gates still apply.

The .NET10 samples compile on Windows. Current native builds fail NETSDK1147;
source/artifact review is separate from native compilation. Current canonical C# files also pass direct compilation against pinned real reference assemblies; that check excludes native build targets, manifest processing, packaging and execution. Android device and
Apple Xcode execution are not-tested under PIN-699. No Human execution or GA proof
is claimed. Exact source hashes, artifact versions and sample input freshness live
in the canonical verification records. Packed stdio inspection checks offline
discovery, Markdown and applicability; it does not verify native behaviour.

## References

- [Pinned official lifecycle contract](https://raw.githubusercontent.com/dotnet/maui/7deff1e006978d1eed341ffcac899a85510f76bb/src/Core/src/Platform/iOS/MauiUISceneDelegate.cs)
- [Pinned official lifecycle contract](https://raw.githubusercontent.com/dotnet/maui/7deff1e006978d1eed341ffcac899a85510f76bb/src/Templates/src/templates/maui-mobile/Platforms/iOS/Info.plist)
- [Pinned official lifecycle contract](https://raw.githubusercontent.com/dotnet/maui/7deff1e006978d1eed341ffcac899a85510f76bb/src/Templates/src/templates/maui-mobile/Platforms/iOS/SceneDelegate.cs)
- [Pinned official lifecycle contract](https://raw.githubusercontent.com/dotnet/core/60871330fce2b3da383461be027ebdf3ce30f8fb/release-notes/11.0/preview/rc2/dotnetmaui.md)
- [Apple RC2 release requirements](https://github.com/dotnet/macios/releases/tag/dotnet-11.0.1xx-rc2-12212)
