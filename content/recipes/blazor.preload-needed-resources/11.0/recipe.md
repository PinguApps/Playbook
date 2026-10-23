# Load needed resources during enhanced navigation

## Goal

Keep control of resources a destination actually needs when Blazor replaces a page through enhanced navigation. Add deliberate preload and modulepreload links through HeadContent, then keep the normal data fetch and module import. The fictional Tern dispatch ledger needs a JSON summary and a formatting module when its details open.

Do not preload every page asset. A hint spends bandwidth and cache space before consumption; measure the destination's likely need. These stable HTML/Blazor APIs are tested with .NET 11 RC2, not GA; no Experimental API opt-in is involved.

## Older approach

Explicit hints are already valid on .NET 10. The pinned Microsoft change guide describes a .NET 11 change to implicit framework hints, but the actual serviced 10.0.13 SSRRenderModeBoundary also returns early for enhanced-navigation requests. Selected RC2 has that guard too. Do not infer an all-10-versus-11 request difference from the guide's introduction label. There is no unavailable older public API to invent: the comparison deliberately preserves the same supported HeadContent pattern.

Initial/full navigation retains the framework's separate preload behavior. Framework PR 63544 also changes DOM synchronization of preload links; neither source change guarantees one network request in every browser. Our static SSR sample observes custom needed resources, not WebAssembly boot-preload generation.

Create an empty net10 directory, with no inherited repository build/package settings. Save global.json containing exactly `{"sdk":{"version":"10.0.402","rollForward":"disable","allowPrerelease":false}}`. SDK 10.0.402 uses ASP.NET/runtime assets 10.0.13 and C# 14. Save TernLedger.csproj:

{{snippet:project-10}}

Save Program.cs in both applications with these three imports before the program below: `using System.Collections.Concurrent;`, `using System.Runtime.InteropServices;` and `using TernLedger;`. This bounded local request receipt endpoint records the real Accept and Sec-Fetch-Dest headers. The enhanced-navigation marker is the blazor-enhanced-nav=on parameter in Accept, not a separate request header. UseStaticFiles precedes explicit UseRouting so the custom files retain their ten-minute public cache lifetime rather than the Development static-asset endpoint's no-cache policy. Never use this diagnostic endpoint unchanged for production user data.

{{snippet:program-10}}

## Newer approach

Create a separate empty net11 directory. Save global.json containing exactly `{"sdk":{"version":"11.0.100-rc.2.26504.105","rollForward":"disable","allowPrerelease":true}}`. The actual newer framework/browser assets are 11.0.0-rc.2.26504.105, with C# 15. No external PackageReference, provider, native toolchain or optional workload is required. The SDK resolves its matching Microsoft.AspNetCore.App.Internal.Assets; standalone restore produces packages.lock.json. Repository captures use reviewed locked closure.

Save TernLedger.csproj and Program.cs:

{{snippet:project-11}}

{{snippet:program-11}}

Save every shared file below in **both** directories. These are complete files, not fragments. The router and HeadOutlet are static: adding a global interactive render mode would change navigation into interactive routing and invalidate this enhanced-navigation check.

Save _Imports.razor:

{{snippet:imports}}

Save App.razor (the full HTML document):

{{snippet:app}}

Save Routes.razor:

{{snippet:routes}}

Save Home.razor:

{{snippet:home}}

Save Ledger.razor. Only hinted/full destinations author links. The control deliberately keeps normal consumption without hints. as="fetch" and application/json describe the JSON destination; modulepreload describes a JavaScript module and needs no as attribute here. Anonymous CORS on both hints matches same-origin fetch/import credentials. A cross-origin deployment must also supply appropriate server CORS headers. A changed query string is a different resource identity.

{{snippet:ledger}}

Create wwwroot and save wwwroot/tern.js. Explicit startup installs one delegated click handler for DOM replaced by enhanced navigation. The persistent document ID and enhancedload count make a full reload distinguishable. The button performs actual import/fetch; preloading alone must not evaluate the module.

{{snippet:browser}}

Save wwwroot/ledger-module.js:

{{snippet:module}}

Save wwwroot/ledger-data.json:

{{snippet:data}}

No stylesheet or other hidden application file is needed. Paths assume an origin-root application with base href="/"; adapt URLs and base together when hosting below a path. Do not substitute a URL that differs from the eventual consumer, or an as/credentials setting that prevents reuse.

Use PowerShell, the matching installed SDK executable and a free loopback port. Set DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=0, DOTNET_GENERATE_ASPNET_CERTIFICATE=false and DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1 in each child environment. In net10 run `& 'C:\Program Files\dotnet\dotnet.exe' restore TernLedger.csproj`, then `& 'C:\Program Files\dotnet\dotnet.exe' build TernLedger.csproj -c Release --no-restore`, then `& 'C:\Program Files\dotnet\dotnet.exe' run --project TernLedger.csproj -c Release --no-build --no-restore --no-launch-profile --environment Development --urls http://localhost:5991`. In net11 use `C:\Users\pingu\.dotnet-playbook-11\dotnet.exe` and port 5992 with the same arguments. These are this author's absolute executables; use the actual installed paths on another machine without changing PATH. Restore needs the matching assets; installed Playbook retrieval itself is offline. Development enables build-output static web assets; a Production build-output launch without published assets is not this setup. Each server occupies its terminal; stop it with Ctrl+C.

## Migration

1. Inventory resources the destination actually needs and measure timing before adding hints. Retain valid older explicit hints; this .NET 10 comparison is not a new .NET 10 foundation or an older-target route to edition 11.0.
2. Preserve the static router, HeadOutlet and blazor.web.js enhanced-navigation setup. Add only the required HeadContent preload/as and modulepreload links with matching URLs, types, credentials and cache settings.
3. Open the home page in a real browser. Record window.tern.documentId. Select Hinted ledger; require the same document ID, an increased enhancedLoads count and a server /ledger/hinted request with Accept text/html; blazor-enhanced-nav=on. Require the two custom resource requests before pressing Open details, with no module evaluation yet. Then press Open details: expect "7 dispatches ready for Tern Quay" and one module evaluation.
4. Return Home and select Control ledger. Its distinct view=control URLs must have no resource requests before Open details; after pressing it, normal fetch/import still displays the same result. Revisit the hinted route with a warm cache and inspect performance entries plus /proof. A warm reuse need not create another server request. A new response/transfer or multiple requests is a diagnostic to investigate, not a universal framework failure.
5. Select Full navigation ledger from Home. Its data-enhance-nav=false link replaces the document ID; the server request lacks the enhanced-navigation Accept parameter. This is an optional per-link tradeoff, not a required migration. Compare actual behavior in supported browsers and deployment conditions.

## Constraints

Applicability is **Blazor enhanced navigation; only resources actually needed**. Normal fetch/import remains correct without hints; hints change scheduling, not application semantics. Modulepreload does not execute the module, and preload does not deliver parsed JSON to application code. Preserve content types, CORS, as and credentials; cache policy and browser implementation govern reuse. Never assert a universal speedup or exact request count from a single run.

The selected static sample has no interactive SPA router or WASM runtime. It does not establish WASM boot-preload behavior, other rendering modes, browsers, operating systems or production performance. The installed host stays workload-free .NET 10. Declared workloads:[] means none required, not that the authoring machine has none installed.

For request-derived application bases retrieve blazor.render-application-base/11.0; for current-path destinations retrieve blazor.navigate-within-current-path/11.0. Those goals remain separate.

## Evidence

Canonical old/new applications compile with their actual pinned SDKs. Real browser checks separately observe enhanced HTTP requests, stable document identity, exact hint/resource URLs, module evaluation on demand, no-hint control and warm-cache/full-navigation behavior. Compilation is labelled compile, not execution; source review, browser behavior and packed-tool retrieval are distinct. docs/verification/PIN-800.md records exact input/artifact/payload hashes and reconstruction solely from installed expanded Markdown. No mocks, source-only substitution, cross-compilation, GA or universal preload requirement is asserted.

Official references are included in metadata and served offline with this body. The pinned guide is reconciled against actual selected serviced framework sources rather than silently treating every .NET 10 release as historical behavior.
