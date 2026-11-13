# Request drag moves and preserve application data

## Goal

Kittiwake assigns a planned walk by dragging it onto a destination. Copy keeps the planned entry; move removes it only after the destination validates and accepts it.

The comparison files share conditional source: NET11_0_OR_GREATER selects the new API; it is excluded from the .NET10 compilation.

## Older approach

The .NET10 comparison requests Copy and retains the source. There is no invented older Move API route.

{{snippet:net100-walkboard-cs}}

## Newer approach

{{snippet:net110-walkboard-cs}}

DragEventArgs.AcceptedOperation defaults to Copy. Move is a request to the native system, not a data transaction. On iOS/Mac Catalyst the native session must allow moving; move-only requests are rejected otherwise. When Copy and Move are both accepted, Copy wins. Custom native drop proposals can override the default mapping. On Windows the source must include Move in native DragStartingEventArgs.AllowedOperations. The conditional branch sets native text and both handled flags: this exact GA Windows handler uses an OR condition, so PlatformArgs.Handled alone does not bypass its default Copy assignment. The legacy Handled flag needs a scoped CS0618 suppression; recheck this workaround when upgrading. Android's implementation is unchanged; this sample keeps Copy there rather than claiming new native move support.

Attach wires two labels in a native host. The neutral console consumer compiles the common code only; it does not compile the Windows branch or launch a host. Apple session permission must be checked in a real native app before relying on Move. DragOver rejects unrelated or stale payloads with None before proposing Move; Drop validates the local ownership marker again. This example accepts only one in-process board's payload: it is not a cross-app source-deletion protocol.

Only the Drop handler commits data, after reading and validating the payload. Rejected destinations and cancelled gestures do not commit. CommitDrop writes the destination first, then removes the source for a move. This synchronous in-memory example assumes the UI thread; durable storage needs an atomic transaction or acknowledgement protocol before removal. Do not remove source data in DragOver, DragLeave or DropCompleted: the latter has no portable public success result and can also fire on cancellation. No automatic source deletion, view movement or CollectionView reordering is promised. Refresh rendered lists from application state in a real host.
## Constraints

Current: SDK11.0.100, C#15, Microsoft.Maui.Controls11.0.0; previous: SDK10.0.402, C#14, Controls10.0.110. Native MAUI, no Blazor rendering, provider, account or experimental opt-in. For a native host use matching workloads: selected GA set11.0.100.1 pins MAUI11.0.0, Android37.2.31 and Apple27.2.11039. Android needs matching API37 SDK/build tools and JDK21/25 (Gradle9.1+ for JDK25). Apple needs Xcode27.2/macOS Tahoe26.6+; upstream's16.6+ conflicts with Apple's host requirements. Binding levels are not deployment floors.

## Evidence

Both exact KittiwakeExchange console projects compile and execute against actual packages. They check operation selection and local successful copy/move, rejected destination and duplicate controls. Cancelled gestures do not enter the local drop handler; native cancellation is not executed here. The shared consumer also contains separate sharing checks; those are not drag evidence. Shared code is real API compilation, not a mock MAUI surface. No native share sheet, preview, delivery, Windows branch or drag gesture execution is claimed. PIN-699 marks these unavailable-native behaviours not-tested for both samples. GA immutable source hashes are retained in samples/KittiwakeExchange/sources.json; prior RC evidence remains historical. PIN-967 remains the final readiness gate.

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


