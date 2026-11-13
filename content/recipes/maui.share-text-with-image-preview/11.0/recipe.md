# Share text or links with an image preview

## Goal

Kittiwake shares a walk invitation with a recognisable local image. This decorates the native sharing UI; it does not save captured media, create map markers or change an app icon.

## Older approach

The .NET10 comparison leaves the preview unset. It shares text and a link without claiming a built-in older PreviewImage equivalent. Platform integration would be a separate implementation.

{{snippet:net100-tripshare-cs}}

## Newer approach

Set ShareTextRequest.PreviewImage to a ShareFile for an existing PNG. Leaving it null preserves the old route. Android10/API29+ uses a content URI and read permission; iOS/Mac Catalyst use link metadata. Other platforms, including Windows, ignore it. A receiver decides how to present the content; no thumbnail or delivery guarantee follows.

{{snippet:net110-tripshare-cs}}

Call from the native UI thread with a readable, app-owned durable file. Copy picked or captured input to private storage before calling; close the write stream first. Retain that file after RequestAsync: Android returns after launching the chooser and recipients may read later. Do not delete it in finally or immediately after await. Use a bounded application retention policy on a later launch when no active share uses the file. Sensitive image exposure follows the user's sharing choice.

The API takes no CancellationToken and returns no success/cancel result. The sample checks cancellation before dispatch only. Cancellation after dispatch cannot dismiss the sheet or prove delivery. Native dismissal is an ordinary completion on Apple. Errors opening the file, validating the request or launching the share UI must be shown to the user; do not report delivery success. Unexpected platform errors should reach the application's error boundary.
## Constraints

Current: SDK11.0.100, C#15, Microsoft.Maui.Controls11.0.0; previous: SDK10.0.402, C#14, Controls10.0.110. Native MAUI, no Blazor rendering, provider, account or experimental opt-in. For a native host use matching workloads: selected GA set11.0.100.1 pins MAUI11.0.0, Android37.2.31 and Apple27.2.11039. Android needs matching API37 SDK/build tools and JDK21/25 (Gradle9.1+ for JDK25). Apple needs Xcode27.2/macOS Tahoe26.6+; upstream's16.6+ conflicts with Apple's host requirements. Binding levels are not deployment floors.

## Evidence

Both exact KittiwakeExchange console projects compile and execute against actual packages. They check operation selection and local successful copy/move, rejected destination and duplicate controls. Cancelled gestures do not enter the local drop handler; native cancellation is not executed here. Application share request construction, retained-file, pre-dispatch cancellation and error controls use an IShare spy; this is controlled application proof, not native share execution. Shared code is real API compilation, not a mock MAUI surface. No native share sheet, preview, delivery, Windows branch or drag gesture execution is claimed. PIN-699 marks these unavailable-native behaviours not-tested for both samples. GA immutable source hashes are retained in samples/KittiwakeExchange/sources.json; prior RC evidence remains historical. PIN-967 remains the final readiness gate.

## Migration

1. Preserve the .NET10 comparison behaviour.
2. Compile the current common code against the pinned actual MAUI package.
3. Integrate into a matching native host, preserving platform permissions and application ownership.
4. Exercise native UI and cancellation on that platform before relying on untested interactions.

## Canonical configuration
{{snippet:net100-kittiwakeexchange-csproj}}

{{snippet:net100-directory-build-props}}

{{snippet:net100-directory-build-targets}}

{{snippet:net100-directory-packages-props}}

{{snippet:net100-global-json}}

{{snippet:net110-kittiwakeexchange-csproj}}

{{snippet:net110-directory-build-props}}

{{snippet:net110-directory-build-targets}}

{{snippet:net110-directory-packages-props}}

{{snippet:net110-global-json}}


