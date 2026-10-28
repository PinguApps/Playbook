# Handle browser-call failures without hiding application defects

## Goal

A fictional reading room reads a shelf label through a browser module. When that browser operation fails, offer a printed-list fallback. A broken catalogue record must still reach the application's error boundary. A disconnected circuit must be able to dispose the owned module without treating expected connection loss as an application defect.

## Older approach

The scoped recovery below already works on .NET 10. It is a valid older approach, not a new runtime invention in .NET 11. Import only after interactive rendering; await the browser operation inside its small try block; validate the returned application data after that block. The original unguarded control used for diagnostic proof calls module.InvokeAsync outside a try/catch. It is deliberately unsafe for this recovery policy and is not the recommended older implementation.

### .NET 10: `global.json`

{{snippet:n10-global-json}}

### .NET 10: `Directory.Build.props`

{{snippet:n10-directory-build-props}}

### .NET 10: `Directory.Build.targets`

{{snippet:n10-directory-build-targets}}

### .NET 10: `Directory.Packages.props`

{{snippet:n10-directory-packages-props}}

### .NET 10: `ReadingRoom.csproj`

{{snippet:n10-readingroom-csproj}}

### .NET 10: `Program.cs`

{{snippet:n10-program-cs}}

### .NET 10: `App.razor`

{{snippet:n10-app-razor}}

### .NET 10: `Desk.razor`

{{snippet:n10-desk-razor}}

### .NET 10: `ReadingAudit.cs`

{{snippet:n10-readingaudit-cs}}

### .NET 10: `ReadingPanel.razor`

{{snippet:n10-readingpanel-razor}}

### .NET 10: `wwwroot/reading.js`

{{snippet:n10-wwwroot-reading-js}}

## Newer approach

.NET 11 adds BL0016 to identify unguarded interop calls. Keep the working boundary rather than adding a broad catch around the entire event handler. The actual RC2 analyzer accepts any enclosing catch, even an irrelevant FormatException catch around an unawaited operation. A clean diagnostic result therefore establishes neither exception handling nor safe rendering/disposal timing.

These two deliberate controls are excluded from ordinary compilation by their
`.cs.txt` suffix. To reproduce the diagnostic, copy only the first to
`Unguarded.cs` in the .NET 11 project and build with warnings as errors. Then
replace it with the second and build again: BL0016 disappears, although the
unawaited browser failure is still not handled by that catch. Remove
`Unguarded.cs` before using the working consumer. Do not adopt the second control
as a recovery pattern; the awaited narrow catch in ReadingPanel is the solution.

### .NET 11: `Before/Unguarded.cs.txt`

{{snippet:n11-before-unguarded}}

### .NET 11: `Before/IrrelevantCatch.cs.txt`

{{snippet:n11-before-irrelevant-catch}}

### .NET 11: `global.json`

{{snippet:n11-global-json}}

### .NET 11: `Directory.Build.props`

{{snippet:n11-directory-build-props}}

### .NET 11: `Directory.Build.targets`

{{snippet:n11-directory-build-targets}}

### .NET 11: `Directory.Packages.props`

{{snippet:n11-directory-packages-props}}

### .NET 11: `ReadingRoom.csproj`

{{snippet:n11-readingroom-csproj}}

### .NET 11: `Program.cs`

{{snippet:n11-program-cs}}

### .NET 11: `App.razor`

{{snippet:n11-app-razor}}

### .NET 11: `Desk.razor`

{{snippet:n11-desk-razor}}

### .NET 11: `ReadingAudit.cs`

{{snippet:n11-readingaudit-cs}}

### .NET 11: `ReadingPanel.razor`

{{snippet:n11-readingpanel-razor}}

### .NET 11: `wwwroot/reading.js`

{{snippet:n11-wwwroot-reading-js}}

## Migration

1. Create either complete project above. Keep its three local Directory files to prevent the documentation tool's unrelated build/package settings from being inherited. Use net10.0/C# 14 with SDK 10.0.402, or net11.0/C# 15 with SDK 11.0.100-rc.2.26504.105 for the exact measured comparison. In global.json set sdk.version to that pin and sdk.rollForward to disable. The actual framework/implicit Internal.Assets versions are 10.0.13 and 11.0.0-rc.2.26504.105 respectively. Restore to create packages.lock.json, review it, then build with RestoreLockedMode=true. Supported family prerequisites and these evidence pins are separate.
2. Keep the supplied App and interactive Desk boundary. Prerender is disabled here, and module import runs in OnAfterRenderAsync. Moving the call to initialization or prerender is not made safe by a catch. Normal same-origin module loading and the server circuit must be permitted by the application's browser/security configuration.
3. Await only the intended browser call inside try. Catch JSDisconnectedException before JSException because disconnection derives from that base exception. Select a known recovery: here a printed shelf list for a JS failure, and reopening the room after connection loss. A real application may log details privately; do not show arbitrary exception messages to end users.
4. Keep the catalogue check outside that catch. The deliberate invalid-record control throws InvalidOperationException and the local ErrorBoundary shows its type/message for this fictional demonstration. In production use a suitable user message and normal error logging. Catching Exception here would hide application defects and unrelated failure categories.
5. Close the module's in-memory reader session when possible, then dispose the owned IJSObjectReference in finally. The disposed flag prevents late UI work; an import that completes after disposal releases its newly obtained reference. During circuit teardown the actual closeReader invocation can throw JSDisconnectedException, which is deliberately tolerated. Other failure types remain visible. The pinned .NET 10 reference disposal also propagates disconnection; RC2's JSObjectReference.DisposeAsync catches that exception internally, so referenceDisconnects is 1 on the old consumer and 0 on RC2. Retaining the compatibility catch is valid, but it is not required for that RC2 implementation. The shutdown call changes only module-local state: it does not manipulate disappearing DOM, guarantee browser cleanup after connection loss or reverse side effects.
6. Run dotnet build -c Release -p:RestoreLockedMode=true, then dotnet run -c Release --no-build --urls http://127.0.0.1:5186 from the chosen project directory using its matching executable. Click Read, Unreadable, then Read: expect the shelf, fallback, then shelf again. Click Invalid catalogue record: expect the explicit application-error boundary. Open a fresh page, wait for the controls to enable, then run Blazor.disconnect() in browser developer tools. GET /audit across these two circuits must reach disposed=2, disconnected=1 and referenceDisconnects=1 on .NET 10 or 0 on RC2. Explicit browser disconnect may dispose immediately; raw transport loss can wait for the local one-second retention control. Neither path's timing is production retention advice.

## Constraints and alternatives

The recovery policy is deliberately narrow. JSException can mean a faulty script or deployment; accepting it as recoverable is appropriate here only because the shelf list is an available alternative. A missing/invalid application record is not that failure. Timeouts/cancellation, serialization errors, authorization and ObjectDisposedException require their own decisions and are not silently swallowed. Analyzer silence does not justify catching them all or blindly retrying non-idempotent browser work.

Interactive Server connection loss can happen between any check and the call. Testing connectivity first cannot remove the race. The actual explicit-disconnect renderer teardown below qualifies disconnected disposal, not every possible race or reconnect guarantee. The post-await disposed checks prevent late state updates; no rendered UI is updated during disposal. Use browser-owned mechanisms such as MutationObserver for DOM removal work where appropriate, rather than trying to manipulate disappearing DOM from DisposeAsync.

No extra package, workload, native toolchain, experimental opt-in, provider or account is needed. WebAssembly, Auto/Hybrid, general retry/reconnection, timeout/cancellation, trimming/AOT, memory reclamation timing and actual GA are outside the executed scope. The 11.0 edition reflects the new analyzer; it does not add a .NET 10 foundation or remove the working older catch pattern.

## Evidence

Both exact framework consumers compile with their own SDK and lock. eng/Test-OwnedLifetimes.ps1 demonstrates no older BL0016, actual RC2 BL0016 on the unguarded control, and a clean migration plus the irrelevant-catch limitation. eng/Record-OwnedLifetimes.ps1 records Chromium calls, a real thrown JavaScript Error, the chosen fallback and successful next call, visible unrelated application failure, connected module disposal and actual JSDisconnectedException from closeReader during disconnected renderer teardown. The first disposal (`disposed=1`) is the ErrorBoundary removing the failed panel while its circuit remains connected; the second follows an explicit disconnect in a fresh circuit. It separately asserts the old/current reference-disposal exception difference. Compiler/analyzer/framework hashes and current sample-input records identify what ran. The installed ordinary tool supplies these complete canonical files, applicability and separate compilation/execution records offline. RC2 execution is not GA proof; comprehensive and GA reconciliation gates remain open.
