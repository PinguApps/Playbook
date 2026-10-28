# Create typed event callbacks

## Goal

A greenhouse valve panel selects a valve from a keyboard event and confirms
it asynchronously from a pointer event. A reusable child receives typed callbacks;
the parent keeps its component receiver, argument values and one successful state
update for each action.

## Older approach

The .NET 10 fixture explicitly calls
`WebEventCallbackFactoryEventArgsExtensions.Create` rather than relying on overload
inference. Both its `Action<E>` and `Func<E, Task>` forms are obsolete. The narrow
CS0618 suppressions are retained only for this migration comparison.

The supported generic instance forms already exist on .NET 10. An ordinary
`factory.Create(receiver, typedDelegate)` may already bind to an instance member;
such code does not necessarily require editing. Preserve it when it still binds
correctly. The removed extension type does not mean typed events themselves are new.

Create a fresh .NET 10 sample directory with every file below. The empty Directory
files stop parent build/package inheritance. No template, CSS or implicit Razor
imports are needed.

### .NET 10: `global.json`

{{snippet:n10-global-json}}

### .NET 10: `Directory.Build.props`

{{snippet:n10-directory-build-props}}

### .NET 10: `Directory.Build.targets`

{{snippet:n10-directory-build-targets}}

### .NET 10: `Directory.Packages.props`

{{snippet:n10-directory-packages-props}}

### .NET 10: `GreenhouseValves.csproj`

{{snippet:n10-greenhousevalves-csproj}}

### .NET 10: `Program.cs`

{{snippet:n10-program-cs}}

### .NET 10: `App.razor`

{{snippet:n10-app-razor}}

### .NET 10: `ValveSwitch.razor`

{{snippet:n10-valveswitch-razor}}

### .NET 10: `ValvePanel.razor`

{{snippet:n10-valvepanel-razor}}

### .NET 10: `CallbackForms.cs`

{{snippet:n10-callbackforms-cs}}

## Newer approach

Replace each explicit extension call with the matching
`EventCallbackFactory.Create<E>(object, Action<E>)` or
`EventCallbackFactory.Create<E>(object, Func<E, Task>)` instance form. Supply the
same interactive component receiver and typed delegate. The parent does not call
`StateHasChanged` in either event handler: the component event pipeline schedules
rendering. The separate first-render call only exposes browser readiness.

ValveSwitch awaits `Confirmation.InvokeAsync(args)` before incrementing its
completion counter. During the parent's delay its phase is awaiting and both
completion counters remain zero; afterwards the argument and one update are
visible. Do not replace the Task-returning delegate with async void.

The finite compile fixture covers exactly the ten event types in the removed
class: ClipboardEventArgs, DragEventArgs, ErrorEventArgs, FocusEventArgs,
KeyboardEventArgs, MouseEventArgs, PointerEventArgs, ProgressEventArgs,
TouchEventArgs and WheelEventArgs. Every type has both delegate forms, making
twenty overload mappings in one recipe. The ErrorEventArgs alias disambiguates
the web event from System.IO.ErrorEventArgs.

Create a separate .NET 11 sample directory using every file below.

### .NET 11: `global.json`

{{snippet:n11-global-json}}

### .NET 11: `Directory.Build.props`

{{snippet:n11-directory-build-props}}

### .NET 11: `Directory.Build.targets`

{{snippet:n11-directory-build-targets}}

### .NET 11: `Directory.Packages.props`

{{snippet:n11-directory-packages-props}}

### .NET 11: `GreenhouseValves.csproj`

{{snippet:n11-greenhousevalves-csproj}}

### .NET 11: `Program.cs`

{{snippet:n11-program-cs}}

### .NET 11: `App.razor`

{{snippet:n11-app-razor}}

### .NET 11: `ValveSwitch.razor`

{{snippet:n11-valveswitch-razor}}

### .NET 11: `ValvePanel.razor`

{{snippet:n11-valvepanel-razor}}

### .NET 11: `CallbackForms.cs`

{{snippet:n11-callbackforms-cs}}

## Migration

1. Find explicit references to the removed extension class and identify each
   event-argument type and whether the delegate returns void or Task. Do not
   rewrite working inferred calls solely because the old extension class existed.
2. Use `EventCallback.Factory.Create<E>(receiver, typedDelegate)` with the same
   component receiver. Keep `Action<E>` synchronous and `Func<E, Task>` awaited
   through `InvokeAsync`; remove obsolete-only warning suppressions after migration.
3. Save each complete project in a separate directory. Set process-only
   `DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=0` and run the matching absolute .NET executable
   with `build -c Release` from that directory. The first restore generates a
   lock file; retain it and use `-p:RestoreLockedMode=true` subsequently.
4. Set process-only `ASPNETCORE_ENVIRONMENT=Development` and
   `DOTNET_ENVIRONMENT=Development`. With the matching executable run
   `bin/Release/net10.0/GreenhouseValves.dll --urls http://127.0.0.1:6220` or
   `bin/Release/net11.0/GreenhouseValves.dll --urls http://127.0.0.1:6221`.
5. Browse to each root and wait for Interactive: True. Focus Valve control and
   press v once: Selected becomes v and updates becomes 1. Click once with a
   mouse: phase becomes awaiting, then complete, Confirmed becomes mouse and
   updates becomes 1. The child completion counter becomes 1 only after the
   awaited parent returns. Reload between independent repetitions to reset state.
6. Compile CallbackForms against both actual targets to check every mapping.
   The .NET 10 fixture deliberately exercises all twenty old extension overloads;
   the .NET 11 fixture exercises their twenty generic replacements. Keyboard and
   pointer behavior are executed; no native browser interaction is claimed for
   the other eight event types. Close tabs and stop hosts when finished.

## Constraints

This is component-library/manual callback construction, demonstrated inside a
Web App using Interactive Server. Static SSR can render a button but cannot invoke
these .NET event handlers. The receiver must participate in Blazor's event pipeline;
passing an arbitrary object does not make it an interactive component. Factory
creation is not event subscription or a guarantee of exactly-once browser delivery.
The counters show one observed state update for each tested input, not one render
batch: asynchronous callbacks may render at start and completion.

Pointer type is the browser-supplied PointerEventArgs value. The sample checks a
mouse pointer, not touch/pen semantics. There is no device control, background
service, persistence or real valve hardware.

The current sample pins SDK 11.0.100-rc.2.26504.105, net11.0/C# 15 and
ASP.NET Core/runtime 11.0.0-rc.2.26504.105. The previous sample pins SDK 10.0.402,
net10.0/C# 14 and ASP.NET Core/runtime 10.0.13. These are exact verification
baselines, distinct from the release-family consumer prerequisites above.
Microsoft.NET.Sdk.Web implicitly resolves the matching
Microsoft.AspNetCore.App.Internal.Assets package; no other package/provider,
optional workload or native SDK is needed. The stable APIs need no experimental
opt-in. RC2 execution does not establish GA verification. Other browsers/OS,
WebAssembly, Hybrid, deployment and older-target/new-package routes are unverified.
This edition is selected only for target 11.0; it adds no .NET 10 foundation.

## Evidence

Fresh compile records retain the commands, exact resolved assets and input
hashes for both canonical projects. The pinned source/API audit was rerun against
the actual local reference assets; its artifact hashes and signatures match.
Separate real Chrome checks use the running components and browser transport,
not a DOM mock. See `docs/verification/blazor-streams-callbacks.md` for measured
observations, exact artifacts and limitations. Required browser evidence remains
distinct from compilation and ordinary installed-tool checks.

The ordinary packed tool is installed from an isolated local feed/cache, then
list_recipes/get_recipe are called over real stdio from an unrelated directory.
The inspection checks ID/title/goal, explicit target 11.0/edition 11.0, complete
expanded Markdown, canonical snippet identity, applicability, fresh evidence and
equal structured/JSON-text representations. No network fetch supplies the served
guidance. This does not waive the aggregate removal-family coverage or GA gates.

Official sources: [Previous twenty extensions](https://github.com/dotnet/aspnetcore/blob/0ef4bbfa3291b306a21e5001cb0491277bdd35bc/src/Components/Web/src/WebEventCallbackFactoryEventArgsExtensions.cs), [Supported generic factory](https://github.com/dotnet/aspnetcore/blob/4a70870818bf5662f21b0c28f9a8c267dccb77f0/src/Components/Components/src/EventCallbackFactory.cs), [Component event receiver](https://github.com/dotnet/aspnetcore/blob/4a70870818bf5662f21b0c28f9a8c267dccb77f0/src/Components/Components/src/ComponentBase.cs).
