# Migrate custom browser runtime hosts

## Goal

Update a hand-written loader for a fictional Lantern Station report. Compile the
host against the declarations in its actual runtime package, then test startup,
configuration, resource loading, main lifetime and diagnostics in a real browser.
Treat Mono and opt-in CoreCLR as separate runtime contracts.

## Older approach

The .NET 10 Mono host configures the environment and application arguments before
creation, obtains exports and explicitly chooses RuntimeAPI.runMain or
RuntimeAPI.runMainAndExit. These runtime-level methods remain useful. The previous
builder's run method is not required for this older approach.

{{snippet:older}}

The diagnostic provider spelling is provider_name in this actual old package.
The host source is TypeScript: declare global is erased during compilation, not
JavaScript to paste directly into an HTML script. Serve the emitted main.js as
an ES module beside the SDK-generated _framework directory.

## Newer approach

.NET 11 adds builder-level runMain and runMainAndExit. The former keeps the runtime
alive after Main returns; the latter exits it and subsequent exported calls are
rejected. Builder.run is deprecated. Choose lifetime explicitly rather than
assuming that an upgrade keeps a finished application callable.

{{snippet:newer}}

The matching CoreCLR browser project selects its runtime explicitly. Its managed
program and host scenario are the same goal, but its packaged declarations and
native runtime are separately compiled and executed. There is no .NET 10 CoreCLR
browser equivalent; use the .NET 10 Mono comparison instead.

{{snippet:coreclr}}

The managed Lantern.Main records LANTERN_ROUTE and its command-line arguments and
returns 17. The exported Read method returns not-started before Main, then
quay:report|east afterwards. A separate exported Emit method writes a
Lantern-Station EventSource event while a trace is collecting. This lets the
checks distinguish a live runtime from a page that merely loaded JavaScript.

{{snippet:managed}}

## Migration

1. Select net10.0/Mono or net11.0/Mono/CoreCLR, with the matching SDK, runtime pack,
   WebAssembly SDK and native workload. The samples use SDK10.0.403/runtime10.0.13
   versus SDK11RC2/runtime11RC2, C#14/15 and TypeScript5.9.3. The project enables implicit usings and nullable annotations;
   retain those settings when copying the managed program. UseMonoRuntime=false is
   the narrow CoreCLR opt-in; Mono-specific Emscripten APIs are not a portable
   CoreCLR API. Neither route requires a rendering framework or provider service.
2. Preserve the original runtime archive and build output. Compare the shipped
   runtimes/browser-wasm/native/dotnet.d.ts, not just VMR source. The original audit
   hashes are checked by the authoring runner. Compile the original host against
   the old declarations and its replacement against each intended current package.
   The declarations explicitly provide no backwards-compatibility guarantee.
3. Configure environment variables and arguments on the builder before create.
   The current RuntimeAPI.setEnvironmentVariable is deprecated on Mono and absent
   from CoreCLR. The samples use withEnvironmentVariable on all three actual routes.
   Configuration is SDK-generated; preserve the complete published asset set.
4. Replace legacy custom configuration and assets deliberately. withConfigSrc is
   deprecated and documented as unsupported in the current declarations. A missing
   legacy configuration fails on .NET 10; actual RC2 Mono/CoreCLR ignore this
   override and still start with generated configuration. DotnetModuleConfig.configSrc,
   AssetEntry, behavior/virtualPath/loadRemote and the old thread-module asset path
   must not be copied into the new schema. Current packages export typed Asset,
   AssemblyAsset, Assets and ResourceList forms. CoreCLR uses LoaderConfig rather
   than MonoConfig and requires assembly/coreAssembly lists; its missing Emscripten
   filesystem hooks are not Mono removals. Let the matching SDK generate resources
   unless the application actually owns and verifies a full custom schema.
5. Use withResourceLoader for a deliberate alternate resource location. Return a
   usable URL for JavaScript modules; the RC2 runtime rejects undefined there even
   though a previous default-loading callback could return it. The samples return
   the provided URL for every observed request, preserving the SDK's selected file
   and normal loading path. Real tests observe those callbacks and deliberately
   return HTTP404 for the native wasm asset; startup rejects. This does not prove
   arbitrary CDN, credentials, service-worker or cache behavior.
6. Choose keep-alive or exit lifetime. The checks call an export before Main, assert
   its return17, then call the export again. Keep-alive returns the updated report;
   exit rejects the call. Use fresh pages/runtimes for each case. Do not reuse a
   runtime after shutdown or reinterpret a resolved Main promise as an alive host.
7. Update diagnostic provider options from provider_name to providerName in the
   actual current declarations. EnableDiagnostics=true includes diagnostics support
   and requires matching native builds/workloads. Compile the correct spelling for
   each package, reject the opposite spelling in the TypeScript check, and collect
   a real trace while emitting the selected EventSource. Inspect the selected
   provider in the returned trace; metadata presence alone does not prove tracing.
   Without diagnostic assets, a declared method may be absent at runtime. The
   sample catches collection errors for display; the verifier requires a real
   trace and fails that route if collection is unavailable.
8. Rebuild, publish and serve the emitted modules and complete _framework assets
   over HTTP. Use `dotnet build -c Release -p:PublishTrimmed=false` and
   `dotnet publish -c Release -p:PublishTrimmed=false` for these untrimmed samples;
   preserve that global property in locked restores so SDK-added ILLink package
   references do not change between build and publish. Recheck after changing runtime, loader configuration, diagnostic
   components or native toolchains. The interpreted application is not an AOT
   benchmark. Native diagnostic relinking is distinct from managed compilation.

## Constraints and alternatives

CoreCLR browser remains opt-in preview in the reviewed publisher notes. Its
runtime productVersion string can read 11.0.0 while the actual archive is RC2;
record archive/compiler hashes and commit rather than claiming GA verification.
Custom loader interfaces have no backwards-compatibility guarantee. This recipe
makes no support claim for every exported native/Emscripten helper, browser,
threaded runtime, AOT mode, rendering framework or operating system.

Keep the standard SDK loader when it already meets the application goal. Keep
supported older explicit runtime calls when their lifetime matches the application.
Deprecation, removal, unsupported runtime capability and new CoreCLR-only APIs are
separate findings; do not blanket-delete the old Mono contract.

## Evidence

eng/Test-LanternHosts.ps1 compiles the canonical TypeScript against all three actual
packaged declarations, checks rejected legacy fields/provider spellings, builds
and publishes each real managed/native dependency set, then runs Chromium through
eng/Test-LanternHosts.mjs. Saved compilation and browser records retain exact
commands, inputs, dependencies, runtime/reference/compiler/declaration/native asset
identities and browser version. The workload setup is worktree-local under ignored
artifacts; it does not change PATH or global tools. .NET10 uses Emscripten3.1.56;
RC2 uses Emscripten6.0.2. SDK and package pins record the measured prerequisites
separately from release-family applicability.

The ordinary packed tool is inspected offline through real installed stdio
list/get from an unrelated directory for discovery, complete canonical Markdown,
target11/edition11, applicability and freshness. Other browsers, platforms,
threaded/AOT applications and actual GA remain unverified. The exact eight .NET10
foundations, PIN-959/PIN-962 and existing Human gates remain unchanged. No recurring
native/browser matrix, production resource or public publication is added.
