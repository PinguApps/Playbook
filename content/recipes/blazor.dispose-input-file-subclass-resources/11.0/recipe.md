# Release resources owned by a derived file input

## Goal

A fictional restoration workbench reads a small manuscript file and listens for upload-status notifications. Removing its file control must cancel the owned subscription once, release its token registration and preserve the framework's own browser callback cleanup. A finished read stays finished.

## Older approach

On .NET 10 a disposable wrapper owns the subscription and renders an ordinary InputFile child. The renderer separately disposes that child and its callback relay. InputFile has an explicit IDisposable implementation but no protected virtual Dispose(bool) hook on the measured baseline; hiding or reimplementing disposal in a subclass is not the wrapper pattern and can skip framework cleanup. Composition remains a valid option on newer targets too.

### .NET 10: `global.json`

{{snippet:n10-global-json}}

### .NET 10: `Directory.Build.props`

{{snippet:n10-directory-build-props}}

### .NET 10: `Directory.Build.targets`

{{snippet:n10-directory-build-targets}}

### .NET 10: `Directory.Packages.props`

{{snippet:n10-directory-packages-props}}

### .NET 10: `RestorationBench.csproj`

{{snippet:n10-restorationbench-csproj}}

### .NET 10: `Program.cs`

{{snippet:n10-program-cs}}

### .NET 10: `App.razor`

{{snippet:n10-app-razor}}

### .NET 10: `Desk.razor`

{{snippet:n10-desk-razor}}

### .NET 10: `StatusFeed.cs`

{{snippet:n10-statusfeed-cs}}

### .NET 10: `RestorationInput.razor`

{{snippet:n10-restorationinput-razor}}

## Newer approach

.NET 11 exposes the protected override. Move the same owned subscription into a derived InputFile while retaining the existing OnChange/AdditionalAttributes behavior. Cleanup only the managed resources when disposing is true, guard repeated calls, and always chain to base.Dispose(disposing), including when owned cleanup throws. Do not add a second IDisposable implementation or a finalizer.

### .NET 11: `global.json`

{{snippet:n11-global-json}}

### .NET 11: `Directory.Build.props`

{{snippet:n11-directory-build-props}}

### .NET 11: `Directory.Build.targets`

{{snippet:n11-directory-build-targets}}

### .NET 11: `Directory.Packages.props`

{{snippet:n11-directory-packages-props}}

### .NET 11: `RestorationBench.csproj`

{{snippet:n11-restorationbench-csproj}}

### .NET 11: `Program.cs`

{{snippet:n11-program-cs}}

### .NET 11: `App.razor`

{{snippet:n11-app-razor}}

### .NET 11: `Desk.razor`

{{snippet:n11-desk-razor}}

### .NET 11: `StatusFeed.cs`

{{snippet:n11-statusfeed-cs}}

### .NET 11: `RestorationInput.cs`

{{snippet:n11-restorationinput-cs}}

## Migration

1. Create either complete project above in its own directory. Retain all three local Directory files. Use SDK 10.0.402/net10.0/C# 14 for the wrapper or SDK 11.0.100-rc.2.26504.105/net11.0/C# 15 for the measured subclass. Set sdk.version and sdk.rollForward=disable in global.json. Restore generates packages.lock.json; review and preserve the implicit Internal.Assets pin (10.0.13 or 11.0.0-rc.2.26504.105), then build with RestoreLockedMode=true. No extra PackageReference is needed. These exact test artifacts are distinct from the supported .NET 11 family prerequisites.
2. Register the demonstration StatusFeed and ordinary Interactive Server services as shown. Its singleton is one deliberately local status channel for this disposable host, not production shared upload state or a tenant-safe service design. Use an appropriately scoped/identified feed in a real application. Ephemeral Data Protection prevents this test host creating persistent keys.
3. Subscribe with the component-owned token. Forward the status through its EventCallback on the renderer dispatcher and check the disposed flag after dispatch. The publisher awaits notifications so callback errors are observable; cancellation removes future notifications, while a callback already copied by the publisher can still be scheduled. The disposed check prevents that late callback from updating this component's observer.
4. Override Dispose(bool), not IDisposable.Dispose. Set the guard before cancellation, dispose the registration and token source in finally, then always call base.Dispose(disposing). The hook supplies lifecycle ownership; it does not change the file-stream API. Do not dispose services obtained from DI just because this component uses them. The old wrapper can remain when inheritance provides no useful simplification.
5. Build with dotnet build -c Release -p:RestoreLockedMode=true and start dotnet run -c Release --no-build --urls http://127.0.0.1:5187 from the chosen project directory using its matching executable. Click Attach input, select a text file smaller than 1024 bytes, and click Publish status. Expect the name/content in file-result and Proof ready in status. Reading uses a caller-chosen maximum, not the browser's advertised size as a safety limit; a production upload needs its own content/security/storage design.
6. Click Remove input, then Remove input and Publish status again. GET /audit must show active=0, subscriptions=1, cancellations=1, callbacks=1. The completed file read remains displayed. Attach again and repeat: the final counts become 0,2,2,2. Each new component owns a new subscription; removal does not reuse a disposed token source.

## Constraints and alternatives

The current hook releases the base InputFile callback relay through base.Dispose(true). Skipping that call can retain .NET callback references even if the application subscription was cancelled successfully. The real browser check captures that actual relay during initialization, successfully calls NotifyChange while mounted, then confirms rejection after removal. This temporary test instrumentation uses an internal browser function only to observe cleanup; application code must not depend on that internal seam.

There is one owned cancellable status subscription per rendered control. Cancellation does not undo a completed upload or file read, delete remote data, wait for every in-flight callback or cancel the browser's file selection. This sample completes a bounded read before removal; it establishes no concurrent upload/stream cancellation or rollback contract. StatusFeed's lock protects its bookkeeping; this is not a distributed notification system or a promise that arbitrary callbacks may race safely with disposal.

The executed path is Interactive Server without prerender. Static SSR has no interactive selection; WebAssembly/Auto/Hybrid, persistent storage, multi-user status routing, concurrent callback races, native platforms, trimming/AOT and actual GA remain outside this proof. No experimental suppression, native workload, external provider or account is required. The .NET 10 hook fails to compile; the wrapper genuinely runs on its older framework. Existing recipe identities, removed stream-option work and the eight approved .NET 10 foundations are preserved.

## Evidence

Both actual Razor consumers compile against their separate SDK/framework and locked implicit assets. The focused negative control rejects the protected override on .NET 10 (CS0115) and accepts it on RC2. Actual Chromium file selection and stream reading succeed on both; two mount/remove cycles each deliver one status and cancel one subscription, no callback is delivered after removal, the completed read remains, and the real framework relay rejects calls after disposal. Current compilation and execution records include exact inputs/tooling hashes and observed values. Installed ordinary-tool list/get checks cover full canonical files, applicability and offline evidence. They do not clear comprehensive coverage or actual-GA gates.
