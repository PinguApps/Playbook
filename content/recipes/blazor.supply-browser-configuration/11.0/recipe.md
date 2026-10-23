# Give an Estuary dispatch board public settings at startup

## Goal

Build a browser application once, then choose its public dispatch endpoint when it starts. The fictional Estuary board reads IConfiguration rather than embedding an environment's endpoint in compiled code. Two public profiles change the rendered environment and endpoint while the application assembly, native WASM runtime and Blazor script remain identical within each release build.

The capability here is configuration-provider precedence and build reuse. Browser startup/reconnection policy is a separate goal. All client-delivered settings are public and user-modifiable; keep secrets and authoritative access decisions on a backend.

## Older approach

.NET 10 supports runtime-supplied browser environment variables and an explicit configuration provider. CreateDefault loads appsettings.json and appsettings.{Environment}.json but does not add that provider. Adding it manually remains a valid older approach. No newer compiler targeting .NET 10 substitutes for the actual SDK 10.0.402/C# 14 comparison below.

Create a standalone EstuaryBoard directory and save these as EstuaryBoard.csproj, Directory.Packages.props and Program.cs. The project declares all standalone settings and has no dependency on this repository's parent build files. Add global.json containing {"sdk":{"version":"10.0.402","rollForward":"disable","allowPrerelease":true}}. The exact package pins are author-maintained; restore generates the dependency lock file, and later restores should use --locked-mode.

{{snippet:project-10}}

{{snippet:packages-10}}

The initial endpoint captures JSON precedence before the manual environment registration. The normal route then adds that provider before an intentional memory override. ?skipProvider=1 is an older-release negative control: the runtime still receives the variables, but IConfiguration continues to return the environment JSON endpoint. ?duplicate=1 is a migration control, not a recommended registration pattern.

{{snippet:program-10}}

## Newer approach

Create a separate EstuaryBoard directory with these project/package/program files. Add global.json containing {"sdk":{"version":"11.0.100-rc.2.26504.105","rollForward":"disable","allowPrerelease":true}}. Use the actual .NET 11 RC2 SDK, C# 15 mode and matching RC2 browser/runtime assets. No C# 15-only syntax is needed. These APIs have no Experimental opt-in in the reviewed artifacts; prerelease verification still does not establish GA behavior.

{{snippet:project-11}}

{{snippet:packages-11}}

CreateDefault now adds one environment provider after JSON initialization. Remove the old unprefixed AddEnvironmentVariables call. A later explicit provider still wins: the sample's memory provider changes only the caption. Retaining an old registration after that override adds a second environment provider and changes the caption back, even though both environment providers contain the same values. The negative control makes that ordering error visible.

{{snippet:program-11}}

## Complete shared files

Save these identical files in both directories. ConfigurationTrace.cs snapshots provider order and per-provider values; it does not replace IConfiguration. Dashboard.razor reads the actual injected configuration and includes all imports and its namespace. No _Imports.razor, CSS package, worker asset, server project or authentication setup is required.

ConfigurationTrace.cs:

{{snippet:trace}}

Dashboard.razor:

{{snippet:dashboard}}

wwwroot/index.html supplies the complete document and demonstration CSS. Disable autostart so the public profile arrives before runtime creation.

{{snippet:document}}

wwwroot/board-start.js fetches public JSON, selects the managed environment, then supplies variables through the browser runtime builder. The flat standalone startup options used here are supported by both pinned releases. Board__Endpoint becomes Board:Endpoint through the environment provider's double-underscore convention. These are browser runtime variables explicitly supplied by JavaScript, not automatic access to the hosting machine's OS environment. In a hosted .NET 11 Web App, InteractiveWebAssembly browser options can supply public environment variables, but that separate composition is not claimed executed here.

{{snippet:browser}}

Save the following complete JSON files in both directories. Their names are case-sensitive on relevant hosts. The environment JSON source wins over base JSON, and the environment provider subsequently wins for overlapping keys. JsonOnly and BaseOnly show fallback to earlier sources.

wwwroot/appsettings.json:

{{snippet:base-json}}

wwwroot/appsettings.Testing.json:

{{snippet:testing-json}}

wwwroot/appsettings.Production.json:

{{snippet:production-json}}

environments/Testing.json:

{{snippet:testing-public}}

environments/Production.json:

{{snippet:production-public}}

serve.mjs is a complete local Node fixture. Its public-settings response comes from the environments directory outside the served asset root. The profile query is restricted to the two fictional profiles. The settings response and static assets use no-store for this focused check; design an appropriate coherent cache policy for a real host/CDN. The fixture contacts no fictional endpoint and does not expose server OS variables.

{{snippet:server}}

## Migration

1. Identify which settings are safe to deliver publicly. Move environment values out of compiled constants into a public startup response. Do not put credentials into browser options, environment variables or wwwroot files.
2. On .NET 10, register the environment provider explicitly after JSON. On .NET 11, remove that old unprefixed registration because CreateDefault supplies it. Keep intentional prefixed/custom providers only with a tested order.
3. Add application overrides after the default providers when they should win. Compare both rendered values and provider order before removing the migration control. Do not remove all providers indiscriminately.
4. Publish once for each application release. Change only the public startup configuration between environments; retain matching application/framework bytes. Reload after startup settings change. There is no live mutation or reload-provider promise in this example.

## Why this helps

One artifact can move through testing and production without rebuilding its managed code. Configuration makes the selected endpoint visible and keeps the source priority deliberate. Removing a duplicate registration prevents a seemingly harmless upgrade from moving environment values after an application override.

## Run and inspect

Use PowerShell 7, Node 24.21.0 and each pinned SDK at its actual sample directory. Set DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=0 in every .NET child environment. For .NET 10 use C:\Program Files\dotnet\dotnet.exe; for RC2 use C:\Users\pingu\.dotnet-playbook-11\dotnet.exe. Run restore, then publish -c Release --no-restore -p:PublishTrimmed=false with that absolute executable. This is interpreted, untrimmed WASM; no wasm-tools workload, native toolchain or AOT claim. The older runtime is 10.0.13; the newer runtime is 11.0.0-rc.2.26504.105. Initial restore is online; the returned guidance and later installed-tool retrieval are offline.

With each current directory still its sample root, run the absolute Node executable C:\Program Files\nodejs\node.exe with serve.mjs, bin/Release/net10.0/publish/wwwroot and port 5851 for the older app, or bin/Release/net11.0/publish/wwwroot and port 5852 for the newer app. First ensure the selected port is free. This occupies the terminal; Ctrl+C stops only that host. The fixture is local serving, not deployment guidance.

Open /?profile=Testing and /?profile=Production on the same host without rebuilding. Expect the selected environment, its public endpoint, Operator override caption, matching JsonOnly and Base JSON fallback, one environment provider and a working Check dispatch counter. Provider order is two JSON providers, environment, memory. On .NET 10 only, initial endpoint still shows the selected JSON endpoint before manual registration; on .NET 11 it already shows the public endpoint.

Open /?profile=Testing&duplicate=1: expect two environment providers, the second after memory, and Testing public caption. Remove the duplicate for the intended Operator override result. On .NET 10, /?profile=Testing&skipProvider=1 instead shows zero environment providers and Testing JSON endpoint, while the real WASM UI remains interactive. Startup failed, wrong values, unexpected provider order or changing application/runtime/script hashes across profiles fails the check.

## Constraints

- WASM; environment-source precedence and removal of duplicate registration. This edition selects .NET 11 only; the actual older sample is a comparison and adds no .NET 10 foundation edition.
- The new default provider follows base/environment JSON; it does not outrank providers explicitly added later. __ normalizes nested keys. Public configuration is neither secret storage nor authorization.
- This is standalone WebAssembly/CSR. Static SSR and Interactive Server use different server configuration. Hosted/Interactive Auto server/client composition, production CDNs, AOT and GA are outside execution evidence.
- Exact pinned packages, SDK and matching JS/runtime packs are required for this evidence. All packages remain author-maintained. No database/provider account, package publication or resource provisioning is needed.
- Values are captured at startup. This sample demonstrates no provider reload, runtime environment mutation or hidden server OS environment transfer. Public profile delivery and cache coherence belong to the host.

## Evidence

Both complete original projects compile using their actual declared SDKs and locked dependencies. Those records are compilation only. Separate real Chrome WASM execution verifies two profiles on unchanged assets, actual rendered IConfiguration values/provider order, the .NET 10 explicit-provider control and .NET 11 duplicate-provider migration. Exact inputs, artifacts, evaluated verifier identity and limits are recorded in docs/verification/PIN-793.md. Source review, compilation and browser execution remain distinct; there are no configuration-host mocks or newer-SDK cross-compilation claims.

The existing packed-tool harness independently reconstructs this full body from canonical sources, checks all nested applicability properties, edition, current compilation evidence and target-10 rejection via real stdio list/get from an isolated feed/cache and unrelated working directory with networking disabled. Standalone reconstruction uses the expanded returned Markdown, not checkout imports. No required Human-only proof remains for the local fixture route.
