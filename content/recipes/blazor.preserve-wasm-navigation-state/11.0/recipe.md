# Preserve WebAssembly navigation state

## Goal

A mineral archive opens a fluorite specimen with a return-selection value attached to that browser history entry. The app observes committed URI and history state, follows an ordinary galena link, then recovers the original selection through Back and Forward.

## Older approach

The historical custom adapter changed history through JavaScript, then called the obsolete two-argument JSInteropMethods.NotifyLocationChanged(uri, false). Its second argument reports interception, not history state. The notification did not navigate: the history.pushState call did that separately. It cannot convey the return-selection state.

The .NET 10 LegacyAdapter and legacy-adapter.js below compile that historical mechanism only. index.html intentionally does not load its script and the supported app never calls the class. Neither notification overload is supported for application code. The retained three-argument (uri, state, isInterceptedLink) overload belongs only to framework/host adapters with matching JS and managed assets; it is not an application migration recipe.

The supported .NET 10 app already uses NavigationManager.NavigateTo with NavigationOptions.HistoryEntryState and observes LocationChanged. It achieves the same state-preserving goal as the current app. Create a fresh .NET 10 directory with every file below; empty Directory files prevent inherited build/package configuration.

### .NET 10: `App.razor`

{{snippet:n10-app-razor}}

### .NET 10: `Directory.Build.props`

{{snippet:n10-directory-build-props}}

### .NET 10: `Directory.Build.targets`

{{snippet:n10-directory-build-targets}}

### .NET 10: `Directory.Packages.props`

{{snippet:n10-directory-packages-props}}

### .NET 10: `global.json`

{{snippet:n10-global-json}}

### .NET 10: `LegacyAdapter.cs`

{{snippet:n10-legacyadapter-cs}}

### .NET 10: `MineralArchive.csproj`

{{snippet:n10-mineralarchive-csproj}}

### .NET 10: `NavigationJournal.razor`

{{snippet:n10-navigationjournal-razor}}

### .NET 10: `Program.cs`

{{snippet:n10-program-cs}}

### .NET 10: `Specimen.razor`

{{snippet:n10-specimen-razor}}

### .NET 10: `wwwroot/index.html`

{{snippet:n10-wwwroot-index-html}}

### .NET 10: `wwwroot/legacy-adapter.js`

{{snippet:n10-wwwroot-legacy-adapter-js}}

## Newer approach

Keep application navigation on NavigationManager. Set HistoryEntryState on the navigation options that create the specimen entry. Observe LocationChangedEventArgs.Location, HistoryEntryState and IsNavigationIntercepted after the navigation commits; read NavigationManager.HistoryEntryState for the current entry. The persistent NavigationJournal subscribes once and unsubscribes on disposal.

The Router enables ordinary in-app link interception. The new galena link creates an entry without application state. Browser Back/Forward restores the target entry's state; it does not copy the previous entry's state to every route. In the measured releases, popstate notifications also report IsNavigationIntercepted=true: that flag is not proof that the user clicked an anchor. Initial component setup is not a LocationChanged event.

The supported path has no JSInteropMethods call, history.pushState or manually forged framework notification. Remove the unused legacy files in the current directory. Create all files below for .NET 11.

### .NET 11: `App.razor`

{{snippet:n11-app-razor}}

### .NET 11: `Directory.Build.props`

{{snippet:n11-directory-build-props}}

### .NET 11: `Directory.Build.targets`

{{snippet:n11-directory-build-targets}}

### .NET 11: `Directory.Packages.props`

{{snippet:n11-directory-packages-props}}

### .NET 11: `global.json`

{{snippet:n11-global-json}}

### .NET 11: `MineralArchive.csproj`

{{snippet:n11-mineralarchive-csproj}}

### .NET 11: `NavigationJournal.razor`

{{snippet:n11-navigationjournal-razor}}

### .NET 11: `Program.cs`

{{snippet:n11-program-cs}}

### .NET 11: `Specimen.razor`

{{snippet:n11-specimen-razor}}

### .NET 11: `wwwroot/index.html`

{{snippet:n11-wwwroot-index-html}}

## Migration

1. Identify application code that manually changes history and calls JSInteropMethods.NotifyLocationChanged. Do not mechanically add a state parameter to this framework-only method. Preserve adapter-protocol documentation only inside deliberately maintained framework integrations.
2. Replace the application path with NavigationManager.NavigateTo(destination, new NavigationOptions { HistoryEntryState = yourState }). Use a small non-secret application value; state is not an authorization or storage boundary.
3. Subscribe to LocationChanged in a persistent component and unsubscribe on disposal. Observe committed location/state from the event rather than assuming a request to navigate has already committed. Keep ordinary Router handling and intercepted links intact.
4. Save all files under separate target directories. Set process-only DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=0 and use each matching absolute .NET executable with build -c Release. Retain generated packages.lock.json; subsequently build with -p:RestoreLockedMode=true. Use run --no-build --configuration Release --urls http://127.0.0.1:6240 for .NET 10, or port 6241 for .NET 11.
5. Browse to the root. Click Open fluorite with return selection: URI becomes /specimens/fluorite, state becomes return-selection=drawer-7 and observer 1 reports intercepted=False. Click Open galena link: URI becomes /specimens/galena, state is null and observer 2 reports intercepted=True.
6. Use browser Back twice, then Forward twice. Observe fluorite with its return state, catalogue with null, fluorite with its return state, then galena with null. Observers 3–6 report intercepted=True in both measured releases. Each action adds exactly one observed entry in this controlled flow. Stop the development servers after checking.

## Constraints

This is standalone client-side WebAssembly navigation with matching framework JS, managed assemblies and browser runtime assets. It does not establish server SSR, Interactive Server, Hybrid, remote authentication, relative-routing, CSP, reload/persistence or production behavior. Keep the base URI and routes shown; relative-to-current routing is separate guidance.

History state is scoped to one browser entry, is visible to client code and must not contain secrets or grant authorization. Intercepted links do not automatically carry the last entry's state. The journal shows the controlled committed sequence, not an exactly-once guarantee under arbitrary handlers, cancellation or concurrent requests. Subscriber exceptions and long-running work should not block navigation.

Do not call either JSInteropMethods overload from application code. Merely notifying the framework does not change the browser URI or history. A framework adapter must deliberately maintain the full matching three-argument protocol and owns its internal-contract compatibility risk.

Previous exact baseline: SDK 10.0.402, net10.0/C# 14, ASP.NET Core packages and browser runtime 10.0.13. Current exact baseline: SDK 11.0.100-rc.2.26504.105, net11.0/C# 15, packages/runtime 11.0.0-rc.2.26504.105. These are verification pins, separate from release-family application prerequisites. Matching WebAssembly and development-server packages are explicitly pinned. Default interpreted build/serve requires no optional workload, native SDK or external provider; AOT/linking and production publish are unverified. Stable application APIs do not require experimental opt-ins; renderer internal-contract risk remains separately disclosed. RC2 evidence is not GA proof. This edition is selected only for target 11.0 and adds no .NET 10 foundation.

## Evidence

Fresh canonical compile records retain exact SDK/dependency closure, commands and input hashes. The real reference/package artifact audit was rerun and matched the committed inventory. Separate real T3 integrated Chromium 152 browser checks, using actual WASM runtime assets, establish the focused behavior in docs/verification/blazor-renderer-navigation.md. They do not use mocked JS interop. The historical notification fixture is compile-only; no supported-path behavior relies on it.

The exact ordinary packed tool was installed through an isolated local-only feed/cache, then list_recipes/get_recipe were called over real stdio from an unrelated directory with explicit target 11.0. Full canonical Markdown, discovery ID/title/goal, applicability, evidence freshness and JSON-text/structured equality are checked. Offline retrieval is separate from online restore/setup. Aggregate removed-API coverage, comprehensive PIN-959 and actual-GA PIN-964 remain open.

Official sources: [Previous framework implementation](https://github.com/dotnet/aspnetcore/blob/0ef4bbfa3291b306a21e5001cb0491277bdd35bc/src/Components/WebAssembly/WebAssembly/src/Infrastructure/JSInteropMethods.cs), [Current framework implementation](https://github.com/dotnet/aspnetcore/blob/4a70870818bf5662f21b0c28f9a8c267dccb77f0/src/Components/WebAssembly/WebAssembly/src/Infrastructure/JSInteropMethods.cs), [History entry options](https://github.com/dotnet/aspnetcore/blob/4a70870818bf5662f21b0c28f9a8c267dccb77f0/src/Components/Components/src/NavigationOptions.cs), [WebAssembly navigation implementation](https://github.com/dotnet/aspnetcore/blob/4a70870818bf5662f21b0c28f9a8c267dccb77f0/src/Components/WebAssembly/WebAssembly/src/Services/WebAssemblyNavigationManager.cs).
