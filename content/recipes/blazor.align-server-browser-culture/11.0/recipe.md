# Keep a harbour manifest consistent when the browser takes over

## Goal

A fictional Culture Harbour desk renders a departure date, cargo weight and handling instruction on the server. Keep those values in the same format and language when WebAssembly becomes interactive. Transfer the server's formatting culture and translation culture together. If the product intentionally gives the client its own language policy, disable transfer explicitly and make that transition understandable.

Prerendered WASM in Blazor Web Apps; satellite assembly timing. This recipe separates four decisions: request localization chooses server cultures; boot chooses the browser runtime's initial culture; persisted framework state restores the server choices; satellite assemblies supply translated resources. A culture name cannot replace missing translation assets.

## Older approach

.NET 10 supports an explicit startup handoff. The server emits its selected names as Razor-encoded data attributes, and client Program reads those names before RunAsync. It sets both default thread cultures before the .NET 10 host loads satellites. This is a valid supported approach, especially where the application already owns culture selection. Restoring a custom component's state after initial resource loading is too late to guarantee the first translated render.

Create an empty CultureHarbour directory with Host and Client children. Save the following complete files under those paths. Use SDK 10.0.402, runtime/framework/browser assets 10.0.13 and C# 14. Set DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=0 in each child environment before invoking the SDK. Both Host/global.json and Client/global.json contain exactly this JSON: {"sdk":{"version":"10.0.402","rollForward":"disable","allowPrerelease":false}}.

The host fixes this desk's request policy to en-GB formatting and fr-FR translations. Clearing request providers is deliberate for this bounded example; a real application can select supported cultures from a cookie, URL or user policy, applied before rendering. Do not trust an arbitrary unvalidated culture name. AddLocalization registers the server localizer; the client assembly contains the resources used during both SSR and WASM rendering.

### .NET 10: Host/Host.csproj

{{snippet:host-project-10}}

### .NET 10: Host/Directory.Packages.props

{{snippet:host-packages-10}}

### .NET 10: Client/Client.csproj

{{snippet:client-project-10}}

### .NET 10: Client/Directory.Packages.props

{{snippet:client-packages-10}}

### .NET 10: Host/Program.cs

{{snippet:server-10}}

### .NET 10: Host/App.razor

{{snippet:document-10}}

### .NET 10: Host/wwwroot/culture.js

{{snippet:startup-10}}

## Newer approach

.NET 11 transfers both server cultures through built-in persistent state. AddLocalization makes the pinned RC2 default enable transfer. An explicit Components:UseCultureFromServer value of true or 1 enables it without relying on that default; false or 0 disables it even when localization is registered. Missing or unrecognized values fall back to IStringLocalizerFactory availability, so use recognized values for an intentional policy.

Use the parameterless AddInteractiveWebAssemblyComponents registration shown below. The cumulative release notes' former public WebAssemblyComponentsOptions.UseCultureFromServer callback was removed during Preview 7. Do not recreate that obsolete API or configure the internal options class. These original samples follow the actual pinned artifact/source route.

Create a separate CultureHarbour directory for .NET 11 with the same Host/Client structure. Use SDK 11.0.100-rc.2.26504.105, matching ASP.NET/browser assets 11.0.0-rc.2.26504.105 and C# 15. Both Host/global.json and Client/global.json contain exactly this JSON: {"sdk":{"version":"11.0.100-rc.2.26504.105","rollForward":"disable","allowPrerelease":true}}.

The client Localization implementation is deliberately pinned to 10.0.13 in both comparisons. Its .NET 11 resolved abstractions/dependencies follow the WebAssembly RC2 graph; the committed locks and compilation records state that exact closure. No native workload, provider, credential or account is required. These APIs carry no reviewed Experimental diagnostic; prerelease RC2 execution does not establish GA behavior.

### .NET 11: Host/Host.csproj

{{snippet:host-project-11}}

### .NET 11: Host/Directory.Packages.props

{{snippet:host-packages-11}}

### .NET 11: Client/Client.csproj

{{snippet:client-project-11}}

### .NET 11: Client/Directory.Packages.props

{{snippet:client-packages-11}}

### .NET 11: Host/Program.cs

{{snippet:server-11}}

### .NET 11: Host/App.razor

{{snippet:document-11}}

### .NET 11: Host/wwwroot/culture.js

{{snippet:startup-11}}

## Shared complete files

The first-render JavaScript observation also sets document.documentElement.lang to the chosen UI culture. This keeps the document language aligned with its translated content when the independent client intentionally changes language after prerendering.

Save these files in both edition directories. Host/Routes.razor, Client/Program.cs, Client/Manifest.razor and all resource files are byte-identical between the canonical editions. Imports and namespaces are explicit in each Razor component; no _Imports.razor, template scaffold, hidden CSS, JS initializer or service worker is required. The full document CSS and boot JavaScript were supplied above.

Client Program records the initial runtime cultures before RunAsync. In the .NET 11 aligned route it makes no selection: RunAsync restores persisted server culture later, then loads that culture's resources before rendering. Never assume the server culture is already visible while Program is configuring services. The .NET 10 aligned route and both independent routes select explicitly before RunAsync.

BlazorWebAssemblyLoadAllGlobalizationData=true ships combined ICU for startup culture changes across the demonstrated choices. The sharded-ICU guard otherwise rejects some changed initial cultures. Combined ICU provides formatting data; it does not supply your translated strings. Keep the separate fr-FR and de-DE resource files and their generated satellites.

### Both editions: Host/Routes.razor

{{snippet:routes}}

### Both editions: Client/Program.cs

{{snippet:client}}

### Both editions: Client/Manifest.razor

{{snippet:manifest}}

### Both editions: Client/Manifest.resx

{{snippet:fallback}}

### Both editions: Client/Manifest.fr-FR.resx

{{snippet:french}}

### Both editions: Client/Manifest.de-DE.resx

{{snippet:german}}

### Both editions: project boundaries

Save this identical file as Host/Directory.Build.props, Host/Directory.Build.targets, Client/Directory.Build.props and Client/Directory.Build.targets. These empty boundaries prevent unrelated parent MSBuild configuration from entering the comparison.

{{snippet:boundary}}

## Run and observe

Restore from each Host directory using its matching absolute executable: C:\Program Files\dotnet\dotnet.exe for .NET 10 or C:\Users\pingu\.dotnet-playbook-11\dotnet.exe for .NET 11. Invoke dotnet restore Host.csproj, then dotnet run --project Host.csproj -c Release --no-restore --no-launch-profile --urls http://localhost:5871 for .NET 10 or http://localhost:5872 for .NET 11. Set ASPNETCORE_ENVIRONMENT=Development for these local build/static-asset checks. First verify the chosen port is free; stop only the host you start. Restore downloads packages/runtime packs and generates packages.lock.json; subsequent repository restores can use --locked-mode. Interpreted WASM requires no wasm-tools workload or native AOT toolchain.

Both initial HTTP documents contain en-GB, fr-FR, 19/07/2032, 12,345.67 and Gardez la caisse bleue debout. They begin with Prerendered. The script deliberately boots applicationCulture en-US, distinct from both server choices. After hydration, the aligned document retains those five values and shows Interactive WASM. Confirm manifest increments from 0 to 1. The French satellite is fetched after client Program starts and before the first WASM render.

Stop your host and run the same command with --IndependentClient=true appended. On .NET 11 the host sets Components:UseCultureFromServer=false; client Program genuinely chooses de-DE for both cultures before RunAsync. On .NET 10 the manual handoff instead chooses de-DE. Initial server HTML remains the same. Hydration intentionally changes it to de-DE, de-DE, 19.07.2032, 12.345,67 and Die blaue Kiste aufrecht halten. The German satellite must load before rendering, and Confirm manifest still works. Disabling transfer alone does not choose a different client policy; the explicit selection is essential.

The server's RC2 emitted Blazor-Configuration comment was decoded in both modes. It contains environmentName and environmentVariables, with no applicationCulture member in these samples. The initial en-US comes from the shown webAssembly startup setting; the aligned en-GB/fr-FR comes from persisted state, not an inferred endpoint ApplicationCulture default.

## Migration

1. Decide which side owns formatting and translation policy. Apply supported server request localization before prerendering and keep both culture names.
2. On .NET 11 register localization and interactive WASM components. Retain the default transfer or explicitly set Components:UseCultureFromServer=true. Remove an older manual handoff only after verifying the actual restored culture and resource timing.
3. If the browser must be independent, set Components:UseCultureFromServer=false and choose both client cultures before RunAsync. Treat the changed SSR-to-client content as an intentional product transition.
4. Ship combined ICU when startup changes require it and include the actual resource satellites. Test with a browser/runtime culture different from the server, inspect real network timing, and use an interactive event to confirm WASM ownership.

## Constraints

This edition selects .NET 11 only. The compiled and executed .NET 10 manual approach is a previous-release comparison, not a backported automatic-transfer route or a ninth .NET 10 foundation. Static SSR has no browser handover; Interactive Server and Auto have different lifetimes/composition and are not executed here.

Culture transfer does not synchronize resource files, translate arbitrary text, persist a user's language choice, or configure server request localization. Loading a satellite successfully is distinct from seeing fallback English, and a date that happens to look the same cannot prove the cultures match. Test culture names, date, number and translated instruction separately.

Satellite selection happens before the first interactive render. Late OnAfterRender changes or state restored after resource selection cannot establish correct initial translations. Additional lazy resources, production trimming/AOT, later language switches, deployment/CDN cache policy and a different browser require their own validation. No remote deployment, publication, provider or account is part of this example.

## Evidence

The two original Host and Client pairs compile with their declared SDKs and actual locked dependencies. Those records are compilation only. Separate actual Chrome 155 WASM runs verify all four edition/policy combinations: real HTTP SSR, different en-US startup, matching server cultures or explicit de-DE opt-out, localized resource text, date/number, actual French/German satellite response before first render, native WASM assets and a working confirmation handler.

Browser records retain precise fetched artifact hashes, initial-document content and performance timestamps. Input and verifier hashes, dirty-capture provenance and later artifact-equality checks are recorded in docs/verification/PIN-794.md. There are no desktop-host mocks, cross-compilation substitutes, remote-provider assumptions or required Human-only checks. Local interpreted development proof is not production or GA proof.

The existing packed stdio harness checks this full expanded offline body, every authored nested route property, sources, target edition/current evidence and target-10 rejection through real list_recipes/get_recipe, isolated feed/cache and unrelated working directory. It preserves the existing scenario count and prior catalogue assertions.
