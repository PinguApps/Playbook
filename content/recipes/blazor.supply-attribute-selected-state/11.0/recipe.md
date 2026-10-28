# Supply scoped values through a custom cascading attribute

## Goal

Reed Archive holds letters and maps in two `CollectionContext` objects. A component must select both without mixing them or sharing state with another service scope. Use custom attribute selection when building a deliberate component integration. Named cascades remain a simpler choice for ordinary component code.

## Older approach

.NET 10 registers two named `CascadingValueSource<CollectionContext>` instances per DI scope. `[CascadingParameter(Name = ...)]` selects each value, and `NotifyChangedAsync` tells recipients to render. This is a valid supported approach and still works on newer releases. The custom supplier registration is absent from the actual .NET 10 reference set; do not backport the new call by changing only the SDK.

### .NET 10: `ArchiveHost.razor`

{{snippet:n10-archivehost-razor}}

### .NET 10: `ArchivePanel.razor`

{{snippet:n10-archivepanel-razor}}

### .NET 10: `ArchiveServices.cs`

{{snippet:n10-archiveservices-cs}}

### .NET 10: `ArchiveState.cs`

{{snippet:n10-archivestate-cs}}

### .NET 10: `global.json`

{{snippet:n10-global-json}}

### .NET 10: `Program.cs`

{{snippet:n10-program-cs}}

### .NET 10: `ReedArchive.csproj`

{{snippet:n10-reedarchive-csproj}}

## Newer approach

.NET 11 registers one scoped supplier for `FromArchiveAttribute`. Its callback checks the property type and selects a context by the attribute's collection name. Each subscription returns that context and retains a recipient callback. The component implements `IArchiveSubscriber` so notifications can dispatch `StateHasChanged` through the component itself. The example mutates the same object; it does not manufacture a `ParameterView` or promise lifecycle replay for replacing the value.

The provider disposes a subscription when the child leaves the render tree. Full renderer teardown on the pinned RC2 build does not perform that unsubscription. `ArchiveState.Dispose` therefore cleans remaining callbacks when the owning scope ends, after its renderer. Normal removal also unregisters that callback from its owner, so scope cleanup cannot repeat the disposal.

### .NET 11: `ArchiveHost.razor`

{{snippet:n11-archivehost-razor}}

### .NET 11: `ArchivePanel.razor`

{{snippet:n11-archivepanel-razor}}

### .NET 11: `ArchiveState.cs`

{{snippet:n11-archivestate-cs}}

### .NET 11: `ArchiveSupplier.cs`

{{snippet:n11-archivesupplier-cs}}

### .NET 11: `global.json`

{{snippet:n11-global-json}}

### .NET 11: `Program.cs`

{{snippet:n11-program-cs}}

### .NET 11: `ReedArchive.csproj`

{{snippet:n11-reedarchive-csproj}}

## Migration

1. Retain named cascades unless custom attribute matching solves a concrete integration need. For the new route, target `net11.0` with the matching ASP.NET Core runtime and Razor SDK; keep the old sample on its actual .NET 10 SDK/runtime.
2. Copy the complete current files above into one directory. The project is a Razor console harness with a framework reference, implicit usings and nullable checking. No NuGet package, service, native toolchain or optional workload is required.
3. Register `AddArchive` once before creating the renderer. Create a separate DI scope per independent archive, then give that scoped provider to its renderer. In a hosted integration, use the renderer/circuit's scope rather than resolving from the root or making state singleton.
4. Annotate both context properties and implement `IArchiveSubscriber`. Keep all context mutations on the renderer dispatcher and await notifications. Do not replace context instances or call the recipient after its scope has ended.
5. Remove children through a normal render. Dispose each renderer before its owning scope. Keep both normal unsubscription and the scoped teardown fallback; do not depend on renderer teardown alone.
6. Build with `dotnet build -c Release`, then run the resulting DLL with the matching runtime. Program asserts exact rendered counts, scope isolation, child removal and disposal counts. From the repository, `eng/Test-ArchiveFragments.ps1` also verifies old-target absence and a real BL0006 control.

## Constraints

`ComponentState` is described by the framework as an implementation detail. This exact RC2 consumer emits no BL0006 for its `Microsoft.AspNetCore.Components.Rendering.ComponentState` access, while the deliberate `RenderTree.Renderer` control does emit BL0006. The canonical consumer contains no suppression. If an altered integration emits a warning, inspect that access and acknowledge only the deliberate boundary; do not suppress the diagnostic project-wide or infer support from its absence.

This is a serial, renderer-owned mutable state example. Unknown names, wrong property types and recipients without the refresh interface fail explicitly. It provides no arbitrary component support, replacement-value semantics, concurrency, trimming or NativeAOT promise. In-process HtmlRenderer verifies generated output, not DOM delivery to a browser. Static request output is already sent after a response ends; it cannot receive these later updates. Interactive hosting requires its own normal transport/lifetime integration, which this recipe does not claim to have exercised.

## Evidence

The canonical .NET 10/C#14 and .NET 11/C#15 projects compile and execute against actual SDK 10.0.402/framework 10.0.13 and SDK 11.0.100-rc.2.26504.105/framework 11.0.0-rc.2.26504.105. Both have empty external dependency closures and lock files. `verification-evidence.json` captures the real commands, inputs and output through `eng/Record-Evidence.ps1`. Neither cross-compilation nor mocks establish these renderer checks.

`eng/Test-ArchiveFragments.ps1` runs both actual consumers, rejects the custom registration on .NET 10, and proves the RC2 analyzer with a deliberate RenderTree control. The installed-MCP consumer check reconstructs these complete files from the returned Markdown and runs them outside the checkout. Requirements above describe consumers; exact RC pins describe observations. No ExperimentalAttribute opt-in applies, and RC proof is not GA verification. PIN-959 and PIN-964 remain open for comprehensive and actual-GA reconciliation.
