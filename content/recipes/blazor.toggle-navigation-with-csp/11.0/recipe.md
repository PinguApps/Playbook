# Operate navigation without an inline script exception

## Goal

Use this when a Blazor Web App must open its menu and follow links while its browser policy rejects inline event handlers. Put the behavior beside NavMenu, load the external module, then retire only the exception that belonged to the removed handler. Bracken walks is an original fictional example; its trail names are not discovery requirements.

## Constraints: choose the layout path and preserve policy

The external module needs browser ES modules, ES2022 private class fields, custom elements and Element.closest support. Chrome 155 is the executed browser baseline; other browsers are not certified by these receipts.

Accepted NavMenu migration; module path differs with client layout. A Web App without global client interactivity keeps the component under `Host/Components/Layout`: request `Components/Layout/NavMenu.razor.js`. A `.Client` project with global interactivity puts it in `Client/Layout`: request `Layout/NavMenu.razor.js`. The project directory names are not URL prefixes. `@Assets[...]` supplies the actual asset name, including any fingerprint. A relative module URL resolves against this document's `/bracken/` base. Keep that base, middleware and static-asset endpoints aligned.

The browser already supports this module approach on .NET 10. The change is an accepted project-template migration in .NET 11, not a new JS runtime API. The older sample includes an actually compiled and executed module backport selected by `?external=true`; its ordinary route demonstrates a valid narrowly hashed inline handler. This edition is selected only for target 11.0 and adds no .NET 10 foundation.

The current sample has no inline toggle and no matching hash or unsafe-hashes policy token. In your app, delete the precise old handler's hash, which is sensitive to the exact handler bytes. Remove unsafe-hashes only if no other necessary inline handlers depend on it. Preserve unrelated hashes, nonces, trusted boot scripts, circuit connections and WebAssembly compilation requirements. Do not use unsafe-inline or disable enforcement to make the test pass. An enforcing header is different from a report-only header.

## Older approach: complete .NET 10 comparison

Create a fresh `BrackenMenu10` directory and write every file below at its stated relative path. Parent build/package boundaries stop unrelated repository inheritance. Both projects pin SDK 10.0.402, net10.0 and C# 14. Packages are exactly 10.0.13; locks record the resolved dependency closure. No workload installation is required.

The default old menu has an original handler scoped to its own containing menu. For the positive comparison start the host with `--AllowInlineMenu=true`; this allows only the matching SHA-256 handler hash `Q0PMW+uf6Mx6edY1pmu6zF3FR16ZFghnGK/H9yrsVQc=`. A different handler remains blocked. Start without that flag for the strict negative control: links still navigate, but the inline collapse is blocked. With the same strict header, append `?external=true` to use the valid module backport; its links retain the selection.

### File: BrackenMenu10/Directory.Build.props

{{snippet:n10-0}}

### File: BrackenMenu10/Directory.Build.targets

{{snippet:n10-1}}

### File: BrackenMenu10/Directory.Packages.props

{{snippet:n10-2}}

### File: BrackenMenu10/Host/global.json

{{snippet:n10-3}}

### File: BrackenMenu10/Host/BrackenMenu.Host.csproj

{{snippet:n10-4}}

### File: BrackenMenu10/Host/_Imports.razor

{{snippet:n10-5}}

### File: BrackenMenu10/Host/Program.cs

{{snippet:n10-6}}

### File: BrackenMenu10/Host/Routes.razor

{{snippet:n10-7}}

### File: BrackenMenu10/Host/Walk.razor

{{snippet:n10-8}}

### File: BrackenMenu10/Host/Components/Layout/MainLayout.razor

{{snippet:n10-9}}

### File: BrackenMenu10/Host/Components/Layout/NavMenu.razor

{{snippet:n10-10}}

### File: BrackenMenu10/Host/Components/Layout/NavMenu.razor.js

{{snippet:n10-11}}

### File: BrackenMenu10/Client/global.json

{{snippet:n10-12}}

### File: BrackenMenu10/Client/BrackenMenu.Client.csproj

{{snippet:n10-13}}

### File: BrackenMenu10/Client/_Imports.razor

{{snippet:n10-14}}

### File: BrackenMenu10/Client/Program.cs

{{snippet:n10-15}}

### File: BrackenMenu10/Client/Routes.razor

{{snippet:n10-16}}

### File: BrackenMenu10/Client/Walk.razor

{{snippet:n10-17}}

### File: BrackenMenu10/Client/Layout/MainLayout.razor

{{snippet:n10-18}}

### File: BrackenMenu10/Client/Layout/NavMenu.razor

{{snippet:n10-19}}

### File: BrackenMenu10/Client/Layout/NavMenu.razor.js

{{snippet:n10-20}}

### File: BrackenMenu10/Host/App.razor

{{snippet:n10-21}}

### File: BrackenMenu10/Host/Confirmation.razor

{{snippet:n10-22}}

### File: BrackenMenu10/Host/wwwroot/menu.css

{{snippet:n10-23}}

### File: BrackenMenu10/Host/wwwroot/policy-probe.js

{{snippet:n10-24}}

## Newer approach: complete .NET 11 migration

Create a separate `BrackenMenu11` directory. Write all the following files, rather than overlaying them on the old directory. This baseline uses SDK 11.0.100-rc.2.26504.105, matching runtime/browser assets and WebAssembly packages, net11.0 and C# 15. These are RC2 reproducibility pins, not GA proof or an experimental API opt-in. The migration itself needs no C# 15-only syntax.

The custom element attaches a listener to its own menu when connected and removes that exact listener when disconnected. It ignores outside clicks and only clears a checked checkbox after a click inside the scroll area. Thus a second event cannot reopen a closed menu. Registration is guarded; enhanced updates can insert a new instance after a module has already loaded. No one-time query captures a stale menu node. The fictional implementation is independent of the migration guide's global listener.

### File: BrackenMenu11/Directory.Build.props

{{snippet:n11-0}}

### File: BrackenMenu11/Directory.Build.targets

{{snippet:n11-1}}

### File: BrackenMenu11/Directory.Packages.props

{{snippet:n11-2}}

### File: BrackenMenu11/Host/global.json

{{snippet:n11-3}}

### File: BrackenMenu11/Host/BrackenMenu.Host.csproj

{{snippet:n11-4}}

### File: BrackenMenu11/Host/_Imports.razor

{{snippet:n11-5}}

### File: BrackenMenu11/Host/Program.cs

{{snippet:n11-6}}

### File: BrackenMenu11/Host/Routes.razor

{{snippet:n11-7}}

### File: BrackenMenu11/Host/Walk.razor

{{snippet:n11-8}}

### File: BrackenMenu11/Host/Components/Layout/MainLayout.razor

{{snippet:n11-9}}

### File: BrackenMenu11/Host/Components/Layout/NavMenu.razor

{{snippet:n11-10}}

### File: BrackenMenu11/Host/Components/Layout/NavMenu.razor.js

{{snippet:n11-11}}

### File: BrackenMenu11/Client/global.json

{{snippet:n11-12}}

### File: BrackenMenu11/Client/BrackenMenu.Client.csproj

{{snippet:n11-13}}

### File: BrackenMenu11/Client/_Imports.razor

{{snippet:n11-14}}

### File: BrackenMenu11/Client/Program.cs

{{snippet:n11-15}}

### File: BrackenMenu11/Client/Routes.razor

{{snippet:n11-16}}

### File: BrackenMenu11/Client/Walk.razor

{{snippet:n11-17}}

### File: BrackenMenu11/Client/Layout/MainLayout.razor

{{snippet:n11-18}}

### File: BrackenMenu11/Client/Layout/NavMenu.razor

{{snippet:n11-19}}

### File: BrackenMenu11/Client/Layout/NavMenu.razor.js

{{snippet:n11-20}}

### File: BrackenMenu11/Host/App.razor

{{snippet:n11-21}}

### File: BrackenMenu11/Host/Confirmation.razor

{{snippet:n11-22}}

### File: BrackenMenu11/Host/wwwroot/menu.css

{{snippet:n11-23}}

### File: BrackenMenu11/Host/wwwroot/policy-probe.js

{{snippet:n11-24}}

## Migration

1. Write the complete old/new project files above into separate roots and select each matching SDK.
2. Build the actual host and client; preserve the app base, imports and static assets.
3. Move the handler to its collocated module, load the layout-specific asset and remove only its matching policy exception.
4. Execute both applicable module paths and the policy controls below before applying the change to a consuming application.

Use a matching SDK host for each directory. From either example root, restore/build `Host/BrackenMenu.Host.csproj` in Release. The host builds its actual client project too. Restore creates package locks; review and retain them, then use `-p:RestoreLockedMode=true` for repeated builds. These examples use `Microsoft.NET.Sdk.Web` and `Microsoft.NET.Sdk.BlazorWebAssembly`; the client has `StaticWebAssetProjectMode=Default`. No template-generated extra imports, namespaces, HTML, CSS, scripts or endpoint registration are assumed. All required files appear above. The policy probe is external observation code and does not implement navigation.

For example, from `BrackenMenu11/Host` with the matching SDK selected, run `dotnet run -c Release --no-build --no-launch-profile -- --urls http://localhost:5952 --environment Development`. Development loads the build's static-asset manifest; this is a local authoring setup, not deployment instructions. Open `http://localhost:5952/bracken/fern`. The shell is static SSR with enhanced navigation; its separate confirmation component is Interactive Server. A browser click that changes the confirmation output proves its circuit initialized.

For the global client layout, run the same host on a separate free port, adding `--GlobalClient=true`, and open `/bracken/client/fern`. The root document assigns InteractiveWebAssembly to the client Routes component; its layout is therefore rendered in the client project. Wait until `Browser: True` and confirmation responds, which proves real WebAssembly execution beyond prerendering. The matching boot script remains allowed. No import map or inline startup script is needed for these pinned samples.

On both layouts: open the checkbox, click outside (it stays open), select Reed (the menu closes and the heading becomes Bracken reed trail), reopen and select Fern (the heading returns). In the static shell, the external probe's enhanced update counter increases without changing its document token. Client routing changes the real URL/content without a full document replacement. Verify the actual module GET returns 200 from the correct fingerprinted path below `/bracken/`. Disconnect/reinsert the component to check listener cleanup and reconnection. A closed menu must stay closed on another scroll-area click.

For .NET 10 use its matching SDK/runtime and a different free port. With `--AllowInlineMenu=true`, the ordinary inline menu closes; without it the browser reports an enforced script-src-attr failure and the immediate collapse fails. Navigation itself remains available. With `?external=true` the external backport closes and navigates with zero ordinary CSP failures on both layouts. This preserves the valid older approach and proves the backport instead of inferring it from syntax.

Inspect the real enforcing Content-Security-Policy header and securitypolicyviolation events. Every route must also block a different deliberate inline button handler; inspect disposition=enforce, effectiveDirective=script-src-attr and blockedURI=inline. A DOM handler attribute, an HTTP 200 or a successful build alone does not prove enforcement. The loopback script/connection policy retains wasm-unsafe-eval for WebAssembly and ws://localhost:* for local circuits; restrict these to the consuming app's actual origins and needs. The framework's separate frame-ancestors policy can coexist with this header.

## Evidence: source reconciliation and execution boundaries

The accepted migration guide and the actual RC2 template are not byte-identical. The guide currently describes a global page handler; the pinned template wraps the menu in a custom element and adds/removes a scoped listener with its connection lifetime. The installed 11.0.0-rc.2.26504.105 template and pinned ASP.NET source commit 4a70870818bf5662f21b0c28f9a8c267dccb77f0 agree on that lifecycle capability. The actual .NET 10.0.13 template contains its older inline click handler and no collocated NavMenu module. The source location includes `BlazorWeb-CSharp/BlazorWebCSharp.1`; a different guessed directory is not evidence. Bracken's original markup, handler, module and trails are not copied guide examples.

Four real pinned host/client projects compile. Separate local Chrome 155 execution checks actual app initialization, both module path shapes under a non-root base, real links/content, enhanced static updates, checked-menu behavior, connection cleanup and enforcing policy controls. The older matching-hash, strict-negative and module-backport cases remain distinct. No mocks, cross-compilation or source-only assertions replace browser execution. Source archive hashes and exact execution artifacts are recorded separately from compilation.

The installed packed stdio harness retrieves this complete body offline, compares canonical snippets and every authored applicability/evidence field, checks edition 11.0/current evidence and rejects target 10.0. Temporary consumers reconstructed only from the returned Markdown compile under the matching SDKs and repeat the affected real navigation/enforcement checks. The tool itself is workload-free .NET 10 and never probes the consumer. One local Windows browser, static shell/Server and global WebAssembly are the verified scope; Auto, other browsers, production/TLS/AOT and GA remain unverified. No external provider or Human-only proof is required for these bounded local claims.
