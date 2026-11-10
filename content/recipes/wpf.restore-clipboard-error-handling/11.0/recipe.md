# Choose a WPF clipboard retrieval error policy

## Goal

A station-label paste command needs to decide whether unavailable transferred data looks empty or produces a catchable failure. Choose that policy at startup; do not change it around individual reads.

## Older approach

The .NET 10 example uses WPF Clipboard.GetText on the UI thread and retains an exception boundary. An empty result cannot distinguish absent text from a swallowed data-provider failure. Existing empty-result handling is reasonable for optional paste operations. The controlled native provider below returns a failure through WPF DataObject; the older consumer observes EMPTY.

## Newer approach

{{snippet:n11-program-cs}}

Before the first retrieval, set AppContext switch `Windows.Clipboard.ThrowExceptionsForGetAPIs` to true if the application must report data-provider failures. This is a shared clipboard pipeline switch, not a new WinForms Clipboard member. The WPF call remains System.Windows.Clipboard.GetText. The switch value is cached; use separate processes to compare policies. Keep absent-data handling even after opting in: missing text is still not an error.

The opt-in does not make every empty read an exception or suppress argument/thread failures. Failure to acquire the clipboard is a separate stage and can already throw. Catch ExternalException at the paste boundary and offer retry without interpreting the failure as an empty user value. Do not catch every exception or silently overwrite application data after a failed paste.

## Migration

1. Decide whether empty text is acceptable for the paste action.
2. Select the process-wide policy before any clipboard/data-object read.
3. Keep the read on the WPF STA UI thread and handle both absent text and transfer errors.
4. Test the policy in separate processes. A same-process toggle is not a reliable test of a cached switch.

## Canonical samples

The native provider is a controlled failing fixture. It executes WPF's real COM adapter without reading or replacing the shared clipboard. TransferReader is the application paste boundary; its Clipboard.GetText call is compiled, not executed by this fixture.

### n10-directory-build-props
{{snippet:n10-directory-build-props}}

### n10-directory-build-targets
{{snippet:n10-directory-build-targets}}

### n10-directory-packages-props
{{snippet:n10-directory-packages-props}}

### n10-global-json
{{snippet:n10-global-json}}

### n10-program-cs
{{snippet:n10-program-cs}}

### n10-quaytransfer-csproj
{{snippet:n10-quaytransfer-csproj}}

### n10-transferreader-cs
{{snippet:n10-transferreader-cs}}

### n11-directory-build-props
{{snippet:n11-directory-build-props}}

### n11-directory-build-targets
{{snippet:n11-directory-build-targets}}

### n11-directory-packages-props
{{snippet:n11-directory-packages-props}}

### n11-global-json
{{snippet:n11-global-json}}

### n11-program-cs
{{snippet:n11-program-cs}}

### n11-quaytransfer-csproj
{{snippet:n11-quaytransfer-csproj}}

### n11-transferreader-cs
{{snippet:n11-transferreader-cs}}

## Constraints

Windows desktop, STA execution, SDK10.0.403/C#14 and SDK11.0.100-rc.3.26522.148/C#15. No added NuGet package, service account or remote provider. The current edition targets .NET11; the older sample is comparison evidence, not a .NET10 foundation or backport. Framework-reference and runtime assets must match their declared targets. No experimental API opt-in is required by these compiled samples. RC3 execution does not establish GA verification.

## Evidence

The .NET 11 SDK baseline is now 11.0.100 GA with preview selection disabled. The .NET 10 comparisons retain their declared SDKs. Earlier RC audit details below are historical; use the current sample pins, dependency locks and GA verification records for current coverage. Existing native exclusions and pending Human gates still apply.

Both canonical projects compile and execute on Windows x64 with runtime10.0.14 or runtime11RC3. Saved verification-evidence.json records exact inputs and commands. See metadata for each sample's actual scope. No global Windows settings are changed. No external clipboard-owner failure, pixel-level rendering, High Contrast transition, Designer round trip or GA behavior is claimed. Existing Human and final-release gates remain open.

## References

[Official RC2 desktop contracts](https://raw.githubusercontent.com/dotnet/core/60871330fce2b3da383461be027ebdf3ce30f8fb/release-notes/11.0/preview/rc2/winforms.md).

[Pinned shared clipboard switch caching implementation](https://raw.githubusercontent.com/dotnet/dotnet/820230c386db5b0106c5430b3e5aee92a7826bf5/src/winforms/src/System.Private.Windows.Core/src/System/Private/Windows/CoreAppContextSwitches.cs).
