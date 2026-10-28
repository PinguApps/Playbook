# Allocate custom web renderer identities

## Goal

A planetarium host runs separate Dome and Orrery control surfaces. Each custom renderer must receive its identity before browser attachment so a click reaches only its own component.

## Older approach

The .NET 10 RendererId init accessor was already obsolete and inert. Assigning 900 never changed the actual identity. InertIdentityProbe below exposes that failure honestly: after the two working controls obtain 201 and 202, the probe still obtains 203 from the hook. It is a comparison fixture, not a working ID-assignment technique.

The protected virtual GetWebRendererId hook already works on .NET 10. Both working hosts use it. Both framework releases retain the same four-argument base constructor: IServiceProvider, ILoggerFactory, JsonSerializerOptions and JSComponentInterop. There is no renderer-ID constructor parameter.

Create a fresh .NET 10 directory with all these files, including the empty Directory files that prevent inherited build/package settings. The local adapter is deliberately framework code: only its class scopes suppress BL0006; only the historical assignment suppresses CS0618.

### .NET 10: `App.razor`

{{snippet:n10-app-razor}}

### .NET 10: `BrowserThreadDispatcher.cs`

{{snippet:n10-browserthreaddispatcher-cs}}

### .NET 10: `Directory.Build.props`

{{snippet:n10-directory-build-props}}

### .NET 10: `Directory.Build.targets`

{{snippet:n10-directory-build-targets}}

### .NET 10: `Directory.Packages.props`

{{snippet:n10-directory-packages-props}}

### .NET 10: `global.json`

{{snippet:n10-global-json}}

### .NET 10: `InertIdentityProbe.cs`

{{snippet:n10-inertidentityprobe-cs}}

### .NET 10: `Planetarium.csproj`

{{snippet:n10-planetarium-csproj}}

### .NET 10: `PlanetariumRenderer.cs`

{{snippet:n10-planetariumrenderer-cs}}

### .NET 10: `Program.cs`

{{snippet:n10-program-cs}}

### .NET 10: `RendererServices.cs`

{{snippet:n10-rendererservices-cs}}

### .NET 10: `Surface.razor`

{{snippet:n10-surface-razor}}

### .NET 10: `wwwroot/index.html`

{{snippet:n10-wwwroot-index-html}}

### .NET 10: `wwwroot/planetarium.js`

{{snippet:n10-wwwroot-planetarium-js}}

## Newer approach

Remove the inert assignment; preserve the hook-based allocator. WebRenderer calls the override from its base constructor, before the derived constructor body initializes js. The override uses only a static counter. It cannot read constructor parameters, instance services or later initialization. The browser attachment records the allocated ID during construction, before StartAsync renders the control.

This small host deliberately uses framework contracts. Its JS forwards the actual Blazor attachment to the original registry, retains the real DotNetObjectReference, and dispatches browser clicks through that renderer's actual DispatchEventAsync. UpdateDisplayAsync reads the real component frames and replaces exactly one button. This is not a mock and is not a general Blazor DOM renderer.

RendererServices forwards to the real browser IJSRuntime through its public interface so the runtime retains its own DotNetObjectReference converters. A fresh JsonSerializerOptions alone on the private WASM fast path failed in the browser. BrowserThreadDispatcher accepts only its owning browser thread and fails if called from another thread; do not enable workers or multithreading. The adapter handles its own synchronous button scenario, not arbitrary asynchronous rendering or framework scheduling. Keep the internal JS protocol and managed assets from the same release.

Create the separate .NET 11 directory below. The previous probe is intentionally absent.

### .NET 11: `App.razor`

{{snippet:n11-app-razor}}

### .NET 11: `BrowserThreadDispatcher.cs`

{{snippet:n11-browserthreaddispatcher-cs}}

### .NET 11: `Directory.Build.props`

{{snippet:n11-directory-build-props}}

### .NET 11: `Directory.Build.targets`

{{snippet:n11-directory-build-targets}}

### .NET 11: `Directory.Packages.props`

{{snippet:n11-directory-packages-props}}

### .NET 11: `global.json`

{{snippet:n11-global-json}}

### .NET 11: `Planetarium.csproj`

{{snippet:n11-planetarium-csproj}}

### .NET 11: `PlanetariumRenderer.cs`

{{snippet:n11-planetariumrenderer-cs}}

### .NET 11: `Program.cs`

{{snippet:n11-program-cs}}

### .NET 11: `RendererServices.cs`

{{snippet:n11-rendererservices-cs}}

### .NET 11: `Surface.razor`

{{snippet:n11-surface-razor}}

### .NET 11: `wwwroot/index.html`

{{snippet:n11-wwwroot-index-html}}

### .NET 11: `wwwroot/planetarium.js`

{{snippet:n11-wwwroot-planetarium-js}}

## Migration

1. Find the specialized WebRenderer subclass and remove RendererId init assignments. Do not add a nonexistent ID parameter to the base constructor. Ordinary Razor application components should continue using the framework renderer.
2. Override GetWebRendererId with a host-coordinated unique allocator that is safe during base construction. Reserve a disjoint range from built-in and other custom renderers; this standalone host reserves IDs above 200 and never reuses them during its lifetime.
3. Keep BL0006 suppression only around deliberate framework integration. The warning is a real compatibility constraint. Remove CS0618 suppression with the historical init call; never suppress these diagnostics across consumer code.
4. Save every file in its target directory. Set process-only DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=0, then invoke the matching absolute .NET executable with build -c Release. Retain the generated packages.lock.json; subsequently add -p:RestoreLockedMode=true. Exact SDK pins below are reproducible test baselines, not universal minimum requirements.
5. From each project directory use that executable with run --no-build --configuration Release --urls http://127.0.0.1:6230 for .NET 10, or port 6231 for .NET 11. Browse to the root. Expect Renderer IDs: 201, 202 and both counters initially zero. Click Dome: only Dome becomes 1. Click Orrery: only Orrery becomes 1. On .NET 10 (port 6230), the old probe must show requested 900 but actual 203. The .NET 11 run reports no init assignment.
6. Keep the two renderer instances alive for their surfaces, then dispose them when the host is removed. This one-page fixture tears down with browser navigation; it does not demonstrate reusable attachment teardown, reconnection or renderer-ID recycling. Stop the local development servers after checking.

## Constraints

This recipe is for specialized framework integrations, not ordinary component authoring. BL0006 explicitly warns that RenderTree types can change; the hook being available does not make it a stable application contract. JSComponentInterop and Blazor._internal are also framework-oriented. No experimental attribute or project-wide warning opt-in is required or recommended.

The adapter only accepts one Surface button per renderer, on one browser thread, and owns a dedicated identity range. Its script is loaded after blazor.webassembly.js with autostart=false, before Blazor.start. It forwards built-in attachments unchanged. It does not implement arbitrary nested components, event types, render batches, worker scheduling, teardown/re-attachment or a renderer framework. The legacy probe's disposed interop registration is not reused; leave the page to tear down this host.

There is no verified alternative that gives the removed init setter useful behavior. Preserve working hook overrides on .NET 10. Revalidate internal protocol, construction and event behavior whenever upgrading the framework.

Previous exact baseline: SDK 10.0.402, net10.0/C# 14, ASP.NET Core packages and browser runtime 10.0.13. Current exact baseline: SDK 11.0.100-rc.2.26504.105, net11.0/C# 15, packages/runtime 11.0.0-rc.2.26504.105. These are verification pins, separate from release-family application prerequisites. Matching WebAssembly and development-server packages are explicitly pinned. Default interpreted build/serve requires no optional workload, native SDK or external provider; AOT/linking and production publish are unverified. Stable application APIs do not require experimental opt-ins; renderer internal-contract risk remains separately disclosed. RC2 evidence is not GA proof. This edition is selected only for target 11.0 and adds no .NET 10 foundation.

## Evidence

Fresh canonical compile records retain exact SDK/dependency closure, commands and input hashes. The real reference/package artifact audit was rerun and matched the committed inventory. Separate real T3 integrated Chromium 152 browser checks, using actual WASM runtime assets, establish the focused behavior in docs/verification/blazor-renderer-navigation.md. They do not use mocked JS interop. The historical notification fixture is compile-only; no supported-path behavior relies on it.

The exact ordinary packed tool was installed through an isolated local-only feed/cache, then list_recipes/get_recipe were called over real stdio from an unrelated directory with explicit target 11.0. Full canonical Markdown, discovery ID/title/goal, applicability, evidence freshness and JSON-text/structured equality are checked. Offline retrieval is separate from online restore/setup. Aggregate removed-API coverage, comprehensive PIN-959 and actual-GA PIN-964 remain open.

Official sources: [Previous framework implementation](https://github.com/dotnet/aspnetcore/blob/0ef4bbfa3291b306a21e5001cb0491277bdd35bc/src/Components/Web/src/WebRenderer.cs), [Current framework implementation](https://github.com/dotnet/aspnetcore/blob/4a70870818bf5662f21b0c28f9a8c267dccb77f0/src/Components/Web/src/WebRenderer.cs), [Browser integration](https://github.com/dotnet/aspnetcore/blob/4a70870818bf5662f21b0c28f9a8c267dccb77f0/src/Components/Web.JS/src/Rendering/WebRendererInteropMethods.ts), [Interop boundary](https://github.com/dotnet/aspnetcore/blob/4a70870818bf5662f21b0c28f9a8c267dccb77f0/src/Components/Web/src/JSComponents/JSComponentInterop.cs).
