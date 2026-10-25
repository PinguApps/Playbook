# Show environment-specific application chrome

## Goal

Help people recognise a rehearsal system before they mistake its parcels for live work. Copperline Desk shows a rehearsal notice and notes button in Review, and a receipts link in Production. Choose chrome from the environment of the renderer doing the work. This is a visual cue, never permission to perform an operation.

## Older approach

The .NET 10 application reads IHostEnvironment on the server and IWebAssemblyHostEnvironment in the browser, then passes the name into ordinary Razor conditionals. This remains a supported, useful approach. There is no EnvironmentView in the declared .NET 10 framework; this comparison does not invent a backport or install a newer component package on an older target.

Create adjacent Host and Client directories. In each, save global.json selecting SDK 10.0.402 with rollForward disable and allowPrerelease false. Save the following Directory.Packages.props in BOTH directories, then Host/Host.csproj and Client/Client.csproj respectively. Runtime and packages are 10.0.13; language mode is C# 14. All effective project settings are supplied: no repository parent build/package imports are needed.

{{snippet:old-packages}}

{{snippet:old-host-project}}

{{snippet:old-client-project}}

Save Client/Chrome.razor. Is compares a name without depending on server-only services in the shared component. Each conditional explicitly describes the equivalent chrome decision; the intentionally overlapping blocked notice demonstrates that exclusions take priority.

{{snippet:old-chrome}}

Save Host/Server.razor and Client/BrowserChrome.razor. Each wrapper reads the actual environment service for its renderer. BrowserEnvironment.Environment and HostEnvironment.EnvironmentName are separate values.

{{snippet:old-server}}

{{snippet:old-browser}}

Use the shared host/client startup, routing, document and JavaScript supplied under Newer approach unchanged in this older pair. Those files use supported .NET 10 APIs; only the component wrappers and chrome implementation differ. The older example therefore has the same real server and browser routes and observed output as the newer one.

## Newer approach

The .NET 11 component expresses the decision directly with EnvironmentView. It obtains IHostEnvironment from the renderer's service provider. WebApplication supplies it on the server; WebAssemblyHostBuilder registers the browser adapter. Do not manually inject a desktop test environment into the browser and treat that as client proof.

Create another adjacent Host/Client pair. In BOTH directories save global.json selecting SDK 11.0.100-rc.2.26504.105 with rollForward disable and allowPrerelease true. Use runtime and packages 11.0.0-rc.2.26504.105, C# 15. Save Directory.Packages.props in BOTH directories, followed by Host/Host.csproj and Client/Client.csproj. These exact pins are the executed RC2 baseline, not evidence about future GA binaries. EnvironmentView is a stable API in that prerelease framework; no Experimental diagnostic or opt-in is declared on this API.

{{snippet:new-packages}}

{{snippet:new-host-project}}

{{snippet:new-client-project}}

Save Client/Chrome.razor. Include accepts several names; names may contain surrounding whitespace and differ in case. Exclude wins even when Include also matches. No Include means render unless excluded. For a nonempty environment, whitespace-only and comma-only Include both leave the help chrome visible.

{{snippet:new-chrome}}

Save Host/Server.razor and Client/BrowserChrome.razor. Environment injection here labels the proof surface; Chrome itself has no environment parameter, service injection or manual environment conditional.

{{snippet:new-server}}

{{snippet:new-browser}}

The remaining files are identical for BOTH release pairs. Every Razor file contains its own using directives: no _Imports.razor is needed. Save Host/Program.cs, then Client/Program.cs. UseStaticWebAssets enables referenced build-output assets while exercising named environments locally; this is a loopback test setup, not a deployment procedure.

{{snippet:host}}

{{snippet:client}}

Save Host/App.razor (including the CSS), Host/Routes.razor, then Host/Browser.razor. The client route disables prerender deliberately so its chrome comes only from the browser renderer. The server route is static SSR and has no interactive circuit.

{{snippet:document}}

{{snippet:routes}}

{{snippet:browser-page}}

Save Host/wwwroot/chrome-start.js. The normal demonstration browser environment is Review. A fresh /client?browserEnvironment=Production boot selects Production; mixed-case rEvIeW is also accepted. This public switch is solely a reproducible demonstration, not a trusted environment selector or an authorization mechanism. Never put secrets in chrome, startup scripts or browser assets. For an actual application, configure the intended browser environment consistently with its hosting setup instead of retaining this demonstration switch.

{{snippet:startup}}

## Migration

1. Identify the target from the project and matching SDK. Keep the .NET 10 conditionals when staying on net10.0. This goal has only an 11.0 edition, not a new .NET 10 foundation or an unverified older-target route.
2. Replace repeated environment checks around chrome with EnvironmentView in the .NET 11 component. Move comma-separated names into Include/Exclude; retain explicit server authorization for every protected operation.
3. Create every file above under a fresh Host/Client pair, including each global.json and Directory.Packages.props. Restore from Host using its matching absolute dotnet executable: dotnet restore Host.csproj, then dotnet build Host.csproj -c Release --no-restore. Restore downloads the pinned interpreted browser runtime/SDK packs; no AOT workload is installed or implied. Repository samples also retain lock files; fresh standalone reconstruction creates its own lock files.
4. Set DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=0 in that child PowerShell process. Set ASPNETCORE_ENVIRONMENT=Production and run dotnet run --project Host.csproj -c Release --no-build --no-launch-profile --urls http://localhost:5891 for the old pair or http://localhost:5892 for the new pair. The matching SDK selects the matching runtime. These are long-running loopback hosts; stop each with Ctrl+C before changing it or reusing its port.
5. Open /server: expect identity, live receipts, desk help and contact. Open /client?browserEnvironment=rEvIeW: expect identity, rehearsal notice, rehearsal notes, desk help and contact. The server remains Production while the browser is Review. Open a fresh /client?browserEnvironment=Production: expect the same chrome as the Production server. The excluded notice must never appear in either environment.
6. Restart the host with ASPNETCORE_ENVIRONMENT=Review and request /server again: expect the rehearsal notice/notes and no receipts. Use the rendered data-environment and data-runtime attributes to distinguish the renderer and actual runtime. Neither test grants access to any protected operation. Stop the disposable hosts when finished.

## Constraints

Server/client environments are distinct. A server prerender and subsequent browser render can legitimately select different chrome; coordinate them when you want the same result. This bounded sample proves static SSR and real InteractiveWebAssembly with prerender false. It makes no Auto, Hybrid, AOT, native workload or other-platform execution claim.

Matching is ordinal case-insensitive. Comma-separated names are trimmed and empty entries discarded; Exclude takes precedence. Unspecified or whitespace Include renders unless excluded; a comma-only include list behaves the same for a nonempty environment. Source review also establishes that an empty current name renders only when BOTH Include/Exclude are unspecified or whitespace. Empty hosting-environment names are not configured by this sample and that boundary is not claimed as browser execution.

Hidden chrome is not authorization. An unrendered button is only presentation; users can change browser environment values or call endpoints directly. Protect sensitive operations and data on the server. No authentication feature, secret, package publication, account, external resource or deployment is needed for this guidance.

Require matching Web/WebAssembly SDKs, component assemblies, boot JavaScript and browser runtime assets. The new application uses the actual RC2 EnvironmentView and WebAssembly IHostEnvironment adapter; it does not substitute a DI stub. No provider package is needed. RC2 execution cannot establish GA compatibility. The previous sample uses actual .NET 10 packages and services and is comparison-only for this goal.

## Evidence

Original canonical projects and prose are authored for this goal. Four declared projects compile with their matching SDKs, language modes and lock-file dependencies. Compile records are compilation, not cross-compilation, mocks or browser execution. Focused real loopback HTTP and Chrome/WASM records separately establish server and client chrome selection, mixed-case/multiple/whitespace include names, empty entries, missing Include, exclusion precedence and purposeful server/client environment differences.

The PIN-795 verification document records exact inputs, built artifacts, evaluated browser-expression identity, verifier source identity and downloaded runtime hashes separately. Standalone reconstruction from expanded real stdio Markdown verifies the complete offline setup without copying repository source or parent configuration. The existing packed-tool harness checks full canonical Markdown, every authored applicability property, current evidence, exact edition and target-10 rejection from an installed isolated package with no network or checkout mount. Windows compilation/browser execution and pinned Linux package retrieval are different evidence surfaces; neither is relabelled as the other.

See docs/verification/environment-content.md for commands, artifacts and limits. Official environment documentation and the pinned RC2 EnvironmentView/WebAssemblyHostBuilder source are linked in structured sources; they support the contract, not copied worked examples. No Human proof is required for this local capability. Future GA reconciliation must rerun affected proof with the actual GA artifacts.
