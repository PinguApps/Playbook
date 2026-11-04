# Own and restore fullscreen presentation

## Goal

The Fen field briefing owns one presentation window. Presentation does not configure Windows Assigned Access, restrict other applications or provide security lockdown.

## Older approach

.NET10 supports explicitly saving border, bounds, window state and topmost, applying a borderless screen-sized window, then restoring state in finally. This remains valid for the bounded normal-window example. More complex ownership, maximized restoration, focus-local shortcuts and power requests require additional application code. The older sample makes no power-request claim.

## Newer approach

{{snippet:n11-fenpresentation-program-cs}}

KioskModeManager accepts a Form or UserControl through ContainerControl; a child container resolves its containing form. Own the component with the window and dispose it before disposing that window. FullScreen saves and restores border, restore bounds, window state and topmost. Disposing during presentation also restores the previous state. TopMostInFullScreen defaults false; leave it false when other applications must remain readily accessible.

F11 toggles only while the managed form or its children have focus; Escape exits by default. ToggleFullScreenKeys=None disables the shortcut. These are application message filters, not global keyboard hooks. MousePointerAutoHideDelay defaults zero (disabled); positive milliseconds hide the pointer during presentation and local mouse activity restores it. The sample leaves pointer hiding disabled.

AlwaysOn defaults false and is independent of FullScreen. Enabling it requests both display and system idle suppression through Windows power requests and may throw Win32Exception. Leaving fullscreen does not release an independently enabled request. Set AlwaysOn=false or dispose the component. It does not override every power policy or configure wake timers. Do not infer screensaver or policy guarantees from a successful setter.

## Migration

1. Keep the previous example as a normal-window restoration baseline.
2. Replace manual state bookkeeping with a component owned by the presentation window.
3. Choose shortcuts, topmost and pointer policy deliberately; acquire idle suppression only for the required lifetime.
4. Check normal and maximized restoration and disposal. Complete PIN-898 before claiming owner-visible or idle behaviour.

## Constraints

Windows Desktop runtime and STA are required. Current SDK11.0.100-rc.2.26504.105/C#15 is compared with SDK10.0.402/C#14. The framework supplies WindowsDesktop assets; no extra NuGet package, provider account, Blazor rendering mode or experimental opt-in. No .NET10 backport is claimed. RC2 compilation is not GA verification.

## Evidence

Both actual canonical projects compiled and ran. The .NET10 sample checked manual normal-window restoration. The .NET11 sample checked child-container resolution, normal restoration and maximized restoration on component disposal. In Pingu's approved Windows build26200 session, UI automation observed F11/Escape restoration from normal and maximized windows. Elevated powercfg /requests reported the component's display and system requests while the visible sample ran; both were absent before launch and after normal close. The sample and observer processes exited. The compact owner-session.evidence.json retains exact input/DLL identity and observations; Pingu confirmed: Yes, both looked correct. This satisfies the bounded PIN-898 owner check. No timed idle-to-sleep, screensaver, policy override, pointer hiding, other-platform or GA behaviour is proved.

## Complete canonical inputs

### 10 / FenPresentation / Directory.Build.props

{{snippet:n10-fenpresentation-directory-build-props}}

### 10 / FenPresentation / Directory.Build.targets

{{snippet:n10-fenpresentation-directory-build-targets}}

### 10 / FenPresentation / Directory.Packages.props

{{snippet:n10-fenpresentation-directory-packages-props}}

### 10 / FenPresentation / FenPresentation.csproj

{{snippet:n10-fenpresentation-fenpresentation-csproj}}

### 10 / FenPresentation / global.json

{{snippet:n10-fenpresentation-global-json}}

### 10 / FenPresentation / Program.cs

{{snippet:n10-fenpresentation-program-cs}}

### 11 / FenPresentation / Directory.Build.props

{{snippet:n11-fenpresentation-directory-build-props}}

### 11 / FenPresentation / Directory.Build.targets

{{snippet:n11-fenpresentation-directory-build-targets}}

### 11 / FenPresentation / Directory.Packages.props

{{snippet:n11-fenpresentation-directory-packages-props}}

### 11 / FenPresentation / FenPresentation.csproj

{{snippet:n11-fenpresentation-fenpresentation-csproj}}

### 11 / FenPresentation / global.json

{{snippet:n11-fenpresentation-global-json}}

### 11 / FenPresentation / Program.cs

{{snippet:n11-fenpresentation-program-cs}}

