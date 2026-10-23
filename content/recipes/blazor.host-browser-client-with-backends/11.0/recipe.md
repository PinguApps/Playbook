# Give Quayside links and backend calls one entrance

## Goal

Let a dispatcher bookmark /berths/north and fetch live quay data through the same browser origin. A page navigation must load the SPA document; an API call must reach the configured backend. The original fictional Quayside application demonstrates both, including an actual backend read counter rather than fixture responses.

## Older approach

.NET 10 can serve a standalone WASM client from an ASP.NET Core host. That host owns static framework assets, YARP proxy routes and a nonfile SPA fallback. This is a supported, useful approach. Gateway's newer package/tool and generated defaults template have no claimed .NET 10 equivalent here. Use the actual .NET 10 SDK for this comparison, rather than cross-compiling with SDK 11.

Create an older QuaysideRoutes directory with Client, Host and Backend subdirectories. Every file below is complete. The shared build/package files apply only inside this directory, isolating the pins from a surrounding repository. Save global.json both at this root and in each project subdirectory. Initial restore creates packages.lock.json; retain it and use --locked-mode afterward. These sample versions remain author-maintained.

`global.json`:

{{snippet:n10-global-json}}

`Directory.Build.props`:

{{snippet:n10-directory-build-props}}

`Directory.Build.targets`:

{{snippet:n10-directory-build-targets}}

`Directory.Packages.props`:

{{snippet:n10-directory-packages-props}}

`Client/QuaysideRoutes.Client.csproj`:

{{snippet:n10-client-quaysideroutes-client-csproj}}

`Client/Program.cs`:

{{snippet:n10-client-program-cs}}

`Host/QuaysideRoutes.Host.csproj`:

{{snippet:n10-host-quaysideroutes-host-csproj}}

`Host/Program.cs`:

{{snippet:n10-host-program-cs}}

`Host/appsettings.json`:

{{snippet:n10-host-appsettings-json}}

The client uses its entry origin as HttpClient.BaseAddress. The host's /api/{**catch-all} route forwards the original path/query to port 5913. MapFallbackToFile handles extensionless browser routes only after the API endpoint has been selected. UseWebAssemblyDebugging is valid on this older development path; it is not copied into the Gateway migration.

## Newer approach

Create a separate newer QuaysideRoutes directory with Client, Defaults and Backend subdirectories. The matching RC2 SDK is 11.0.100-rc.2.26504.105 with C# 15 mode; no C# 15-only syntax is required. Save the following common files at the root, and copy global.json into all three project directories. The new edition is target 11 only, not a new .NET 10 foundation.

`global.json`:

{{snippet:n11-global-json}}

`Directory.Build.props`:

{{snippet:n11-directory-build-props}}

`Directory.Build.targets`:

{{snippet:n11-directory-build-targets}}

`Directory.Packages.props`:

{{snippet:n11-directory-packages-props}}

`Client/QuaysideRoutes.Client.csproj`:

{{snippet:n11-client-quaysideroutes-client-csproj}}

`Client/Program.cs`:

{{snippet:n11-client-program-cs}}

`Client/appsettings.json`:

{{snippet:n11-client-appsettings-json}}

StaticWebAssetSpaFallbackEnabled=true causes the SDK to add an index.html endpoint with a nonfile catch-all and last-match order to the static endpoint manifest. Without it this tested RC2 Gateway returns 404 for /berths/north. The project references Components.Gateway for build/tool assets, with PrivateAssets=all. There is no normal application lib API to call; BlazorGateway.BuildWebHost is explicitly framework-test-only. The bundled tool reads the actual generated development/static endpoint manifests.

Two service-discovery hops have different owners. The generated browser initializer fetches /_blazor/_configuration before managed startup and injects the public services__quays__http__0 variable. The RC2 configuration provider makes that value visible to the generated defaults library. AddBlazorClientServiceDefaults installs the service-discovery handlers for HttpClientFactory, so http://quays resolves to http://localhost:5912 and preserves api/quays/... . Gateway independently resolves http://quaybackend to loopback port 5914 before YARP forwards the request. The browser never needs direct access to the backend port or a CORS exception.

The Client/appsettings.json file is server-side Gateway configuration, outside wwwroot. Only ConfigResponse is deliberately returned to the browser. ConfigResponse is a JSON string inside the outer JSON object. Keep service addresses/configuration public and omit secrets. Changing the entry origin or ports requires updating both the launch arguments and the public service-discovery value coherently.

## Generated service defaults

The following library comes from the actual SDK RC2 blazorwasm-servicedefaults template, generated as QuaysideRoutes.Defaults with hosted=false. Its extension and initializer retain Microsoft's license and behavior; they are generated support code, not original application prose. Only project/pin/build integration is adapted. The referenced .csproj.in is the upstream template build input; it is not the exact generated project in the installed SDK nupkg. The generated project and resolved pins were inspected and compiled separately. Save these complete files, or generate the template with the exact SDK and reconcile all resulting files/pins against these declared artifacts. The template's description mentions resilience, but its actual RC2 Extensions.cs registers service discovery and optional OpenTelemetry only; this recipe claims no retry/resilience policy.

`Defaults/QuaysideRoutes.Defaults.csproj`:

{{snippet:n11-defaults-quaysideroutes-defaults-csproj}}

`Defaults/Extensions.cs`:

{{snippet:n11-defaults-extensions-cs}}

`Defaults/wwwroot/QuaysideRoutes.Defaults.lib.module.js`:

{{snippet:n11-defaults-wwwroot-quaysideroutes-defaults-lib-module-js}}

The generated extension returns early from telemetry setup when OTEL_EXPORTER_OTLP_ENDPOINT is absent. This local route leaves it unset. Retain the generated OpenTelemetry dependency closure even though OTLP delivery is not exercised. Renaming the library requires preserving the matching assembly-named .lib.module.js static initializer. No additional startup JavaScript is needed beyond the document's framework script.

## Complete shared application and backend

Save App.razor, index.html and Backend/Program.cs identically at the indicated paths in both release directories. Save each Backend project only in its stated release directory; its target and language settings differ. App and Berth declare their imports/namespaces explicitly; no hidden _Imports.razor, layout, CSS framework, JS package or generated guide application is required. index.html is the complete document, stylesheet and script setup.

`Client/App.razor`:

{{snippet:n11-client-app-razor}}

`Client/wwwroot/index.html`:

{{snippet:n11-client-wwwroot-index-html}}

Older (.NET 10) `Backend/QuaysideRoutes.Backend.csproj`:

{{snippet:n10-backend-quaysideroutes-backend-csproj}}

Newer (.NET 11) `Backend/QuaysideRoutes.Backend.csproj`:

{{snippet:n11-backend-quaysideroutes-backend-csproj}}

`Backend/Program.cs`:

{{snippet:n11-backend-program-cs}}

The routed component differs only in its injected HTTP service and request creation. Older Client/Berth.razor:

`Client/Berth.razor`:

{{snippet:n10-client-berth-razor}}

Newer Client/Berth.razor:

`Client/Berth.razor`:

{{snippet:n11-client-berth-razor}}

## Run and inspect

Use PowerShell 7 and the actual pinned SDK at each sample root. Before every .NET child, set DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=0; keep PATH unchanged. On the verified Windows host the SDK executables are C:\Program Files\dotnet\dotnet.exe (10.0.402/runtime 10.0.13) and C:\Users\pingu\.dotnet-playbook-11\dotnet.exe (11.0.100-rc.2.26504.105/runtime 11.0.0-rc.2.26504.105). Initial NuGet restore is online; installed guidance retrieval is offline. Neither build requires wasm-tools, native compilation, AOT, a container or an account.

For the older sample, restore/build Host/QuaysideRoutes.Host.csproj and Backend/QuaysideRoutes.Backend.csproj with the SDK 10 executable using build -c Release. The host builds the referenced client. In a separate terminal at Backend, launch that executable with bin/Release/net10.0/QuaysideRoutes.Backend.dll --urls http://127.0.0.1:5913. At Host, set ASPNETCORE_ENVIRONMENT=Development and launch bin/Release/net10.0/QuaysideRoutes.Host.dll --urls http://localhost:5911. The development environment loads the actual client static-web-assets manifest. Each launch occupies its terminal; Ctrl+C stops that process.

For the newer sample, build Client/QuaysideRoutes.Client.csproj and Backend/QuaysideRoutes.Backend.csproj with the SDK 11 executable using build -c Release. The client builds Defaults. Launch the backend from its directory with bin/Release/net11.0/QuaysideRoutes.Backend.dll --urls http://127.0.0.1:5914. From Client, use that SDK's msbuild QuaysideRoutes.Client.csproj -p:Configuration=Release -getProperty:_BlazorGatewayDll,_GatewayStaticWebAssetsManifest,_GatewayEndpointsManifest. Read the Properties object. Then invoke the absolute SDK 11 executable with the returned _BlazorGatewayDll path, --environment Development --staticWebAssets followed by _GatewayStaticWebAssetsManifest, --ClientApps:app:EndpointsManifest followed by _GatewayEndpointsManifest, and --urls http://localhost:5912. Keep the current directory at Client so the tool reads the supplied appsettings.json. Do not invent a manifest or reference a lib DLL as an app API. The equivalent package targets set RunCommand/RunArguments for normal SDK launching; explicit absolute invocation avoids selecting another installed dotnet through PATH. These evaluated property names are pinned-build details to reconcile on upgrade. The detailed verification walkthrough supplies the tested copyable PowerShell launch commands.

Check ports 5911–5914 are free before starting; stop only processes you own. Open /berths/north on each entry point. Expect Quay north; cargo=slate; read=N. Read quay increments the real backend counter. South berth changes the client route and returns Quay south; reloading that URL must still start WASM and retrieve data. GET /api/quays/north?cargo=granite must retain north/granite and X-Quayside-Backend: real-loopback. GET /api/not-found and /absent.js must both stay 404, not return the SPA document. A backend failure must remain an HTTP gateway failure, not a successful HTML response. In the newer browser, network records must show the generated .lib.module.js, /_blazor/_configuration and same-origin /api requests; a request to the unresolved quays hostname fails the check.

## Migration

1. Keep the older host until you have reproduced its deep-link and proxy behavior. Preserve intentional API paths/transforms, host authentication and error status handling.
2. For this standalone .NET 11 path, replace the WebAssembly.DevServer package/run tool with the pinned Components.Gateway build/tool assets. A separately installed Gateway.Cli is an alternative distribution, not required or executed here. Do not retain obsolete inspectUri browser-debugger launch configuration or copy UseWebAssemblyDebugging into a new Gateway application. ASPDEPR011 belongs to this migration; the valid .NET 10 host retains its older development debugging call. This focused sample contains no debugger launch profile and does not claim debugger/watch execution.
3. Enable the SPA manifest fallback and pass the actual matching development/static endpoint manifests. Configure explicit API proxy routes before accepting the fallback. Remove only the old hosting machinery that the selected Gateway path replaces.
4. Generate/reference the real defaults library, retain its initializer and call AddBlazorClientServiceDefaults before constructing named clients. Supply public browser service configuration and separate Gateway backend resolution. Verify both hops with real network responses.
5. Aspire dependency when used: a compatible Aspire AppHost and its WASM/Gateway integration must supply service references, endpoints and client configuration. The generated defaults library alone proves neither Aspire orchestration nor its prerequisites. This manual route does not author or execute an Aspire alternative; validate the actual Aspire version/tooling/runtime before adopting one.

## Why this helps

The dispatcher uses one entry URL while routing remains configurable. The browser owns client navigation; Gateway owns backend destinations and static endpoints. Distinguishing those responsibilities prevents a catch-all page response from hiding an API error and avoids baking a backend machine address into the browser assembly.

## Constraints

- WASM; Components.Gateway, generated service-defaults library; Aspire dependency when used. This is standalone CSR, not Static SSR, Interactive Server or Auto.
- Gateway and the generated template are RC2 artifacts. No Experimental API attribute/opt-in was found on the selected path; stable metadata describes that API classification, not GA proof. Refresh package/SDK/template/manifest/initializer evidence together for GA.
- Local HTTP deliberately disables Gateway HTTPS redirection and telemetry. Keep production TLS, authentication, authorization, proxy exposure and forwarded-header policy under the actual host's configuration. No production deployment or account/resource publication is claimed.
- Source review, actual SDK compilation, real Chrome WASM/HTTP execution and installed offline stdio tests are separate evidence. No mock, newer-SDK older-target cross-compilation or framework TestHost result substitutes for the local browser proof. OTLP export, Aspire, CLI/watch/debugger, production publishing, alternate platforms/browsers, path bases and AOT remain unexecuted scope.

## Evidence

All six actual old/new projects have matching-SDK compilation records and exact restored dependency closures. The real Chrome verifier checks the rendered client, increments the real backend counter, confirms path/query/header preservation, checks 404 boundaries and observes the generated RC2 initializer/configuration/service-discovery requests. Direct deep link, client navigation and deep-link reload are checked on both releases. Artifact/input hashes and the separate packed stdio/reconstructed-consumer evidence are recorded in docs/verification/PIN-796.md. Dirty authoring captures are labelled as such and are not relabelled clean-head execution.

## References

- [Gateway implementation and configuration contract](https://github.com/dotnet/aspnetcore/blob/4a70870818bf5662f21b0c28f9a8c267dccb77f0/src/Components/Gateway/src/BlazorGateway.cs)
- [Bundled Gateway MSBuild tool/manifest invocation](https://github.com/dotnet/aspnetcore/blob/4a70870818bf5662f21b0c28f9a8c267dccb77f0/src/Components/Gateway/src/build/Microsoft.AspNetCore.Components.Gateway.targets)
- [Gateway proxy and service-discovery framework tests](https://github.com/dotnet/aspnetcore/blob/4a70870818bf5662f21b0c28f9a8c267dccb77f0/src/Components/Gateway/test/BlazorGatewayProxyTests.cs)
- [Generated RC2 service-defaults implementation](https://github.com/dotnet/aspnetcore/blob/4a70870818bf5662f21b0c28f9a8c267dccb77f0/src/ProjectTemplates/Web.ProjectTemplates/content/BlazorWasmServiceDefaults-CSharp/Extensions.cs)
- [RC2 template build input; generated dependency pins come from the actual SDK nupkg](https://github.com/dotnet/aspnetcore/blob/4a70870818bf5662f21b0c28f9a8c267dccb77f0/src/ProjectTemplates/Web.ProjectTemplates/BlazorWasmServiceDefaults-CSharp.csproj.in)
- [Generated runtime configuration initializer](https://github.com/dotnet/aspnetcore/blob/4a70870818bf5662f21b0c28f9a8c267dccb77f0/src/ProjectTemplates/Web.ProjectTemplates/content/BlazorWasmServiceDefaults-CSharp/wwwroot/BlazorWasm.ServiceDefaults1.lib.module.js)
- [Official .NET 11 Blazor release notes](https://learn.microsoft.com/en-us/aspnet/core/release-notes/aspnetcore-11?view=aspnetcore-11.0)
- [Supported older ASP.NET Core hosting and fallback](https://learn.microsoft.com/en-us/aspnet/core/blazor/host-and-deploy/webassembly/?view=aspnetcore-10.0)
- [YARP route/cluster configuration](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/yarp/config-files?view=aspnetcore-10.0)
