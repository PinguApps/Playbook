# Stop one beacon animation when a hold ends

## Goal

A harbour operator holds a preparation label. Once the hold is recognized, the beacon grows for three seconds. Releasing, cancelling the gesture or leaving the page stops that animation and restores the beacon's scale.

## Older approach

The .NET10 Windows comparison uses Button.Pressed/Released, a cancellable 650ms delay and a named ScaleToAsync abort. This deliberately small button interaction remains valid for a button: it is not a general movement-aware gesture recognizer. Leaving the page cancels the delay or aborts the scale animation. The delay can throw OperationCanceledException; the animation returns a cancellation Boolean. Keep those two contracts separate.

## Newer approach

{{snippet:samples-net11-0-beaconboard-boardpage-cs}}

The .NET11 BoardPage attaches LongPressGestureRecognizer to a label. LongPressing Started creates one CancellationTokenSource and awaits ScaleToAsync with that token. Running does not start another task. Completed and Canceled cancel the active token. OnDisappearing also cancels it. The awaited task owns disposal; the terminal handler only requests cancellation. Rejecting another Started while hold is non-null prevents an older continuation from resetting a newer animation.

The animation result is true for cancellation and false for normal completion. A pre-cancelled token also yields true in the reviewed implementation; do not rely on OperationCanceledException for this animation. The callback's finally restores Scale, so the sample deliberately chooses reset rather than leaving the interrupted value visible. Cancel only this operation; CancelAnimations would stop unrelated animations on the view. These helpers use an animation name per operation: do not start concurrent ScaleToAsync operations on the same element and expect independent ownership.

## Platform boundary

The full Started/Running/Completed/Canceled lifecycle is documented for iOS and Mac Catalyst. The canonical Apple project targets iOS27.0; Mac Catalyst is documentation-only here, with no separate claimed build. Android and Windows send completion notifications in the reviewed backends; they do not supply the Started/Running stream this hold-to-animate sample needs. The Windows current project makes the code inspectable but does not claim that its label animates while held. Preserve a separate button/platform interaction there when a live hold lifecycle is required. A completion command is useful for a context action, but cannot start an animation during an already-finished hold.

MinimumPressDuration defaults to 500ms, AllowableMovement to 10 pixels and NumberOfTouchesRequired to one. This sample selects 650ms and 12 pixels. Android ignores the configurable duration and uses the system timeout; touch-count configuration is supported only on iOS/Mac Catalyst. LongPressed/Command represents successful recognition, not the full state stream; Command.CanExecute guards command invocation, not the separate event subscription. Do not wire both Command and LongPressing to start the same animation.

## Migration

1. Identify whether the target backend supplies live state changes. Use the built-in recognizer for the Apple lifecycle; document completion-only backends rather than inventing start/release events.
2. Replace the timer and name lookup with one token passed to ScaleToAsync. Keep all gesture/UI handling on the UI thread.
3. Cancel on both terminal states and page exit. Dispose only after the awaited operation settles. Choose reset or preserve-position semantics explicitly.
4. On an available native target, check release before recognition, release during animation, movement cancellation, normal completion, repeated holds and page exit. Those native checks were not executed here and are excluded, not promised as later Human work.

## Constraints

The .NET11 edition requires native Controls11 assets; there is no verified older-target package route. SDK/runtime11, C#15 and RC2 package/workload pins are explicit in Applicability and canonical projects. No service/provider account, web renderer, preview C# or experimental opt-in is required. GA reconciliation remains separate. The older Windows Button implementation is a comparison, not proof of Apple behavior.

## Evidence

The actual .NET10 Windows comparison compiles in Release. The token-aware overload, Boolean completion semantics, gesture event contract and Android/Windows backend limits were checked against pinned official implementation and the downloaded RC2 Controls.Core artifact. No upstream tests were executed. Current Windows compilation is not-tested because SDK11 lacks the MAUI SDK pack. iOS/Mac Catalyst/Android compilation and device interactions are not-tested because their native toolchains/devices are unavailable. All gesture/animation UI behavior is explicitly unexecuted; a Windows build or MCP retrieval does not establish it. PIN-699 and Pingu's 4 November clarification exclude those unavailable checks without requesting more hardware.

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
