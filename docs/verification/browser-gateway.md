# Browser gateway evidence

Historical authoring observations from October 2026. Compilation stays in the
sample records; these observations do not establish a new run, broader platform
support or GA verification. Reuse only while the claim and relevant inputs remain
valid; see [capture and refresh](../contributing.md#capture-and-refresh-sample-evidence).
[Original capture and detailed provenance](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-796.md).

## Actual sources and compatibility

[Artifact identities](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-796.artifacts.json) pin the restored Gateway nupkg and
the installed SDK's actual template nupkg, including its project, extension and
initializer entry hashes. The SDK generated QuaysideRoutes.Defaults with
`new blazorwasm-servicedefaults --name QuaysideRoutes.Defaults --output Defaults --no-restore`.
Its actual project pins WebAssembly RC2, ServiceDiscovery 10.10.0, OpenTelemetry
exporter/hosting 1.19.1 and HTTP/runtime instrumentation 1.19.0. The generated
extension and initializer retain their behavior; project/build/central-pin
integration is adapted. The extension registers service discovery and optional
OpenTelemetry, without a retry/resilience registration.

The [upstream project build input](https://github.com/dotnet/aspnetcore/blob/4a70870818bf5662f21b0c28f9a8c267dccb77f0/src/ProjectTemplates/Web.ProjectTemplates/BlazorWasmServiceDefaults-CSharp.csproj.in)
is `.csproj.in`, with build-time version substitutions. It is **not** the exact
generated nupkg project. The nonexistent content-directory project URL was removed
before publication. Generated artifact pins and actual restored dependency closures
provide separate evidence. Other official references are embedded in the recipe.

Gateway's actual package supplies build/tool assets, not a normal application
library API. The sample invokes its bundled tool with evaluated development/static
endpoint manifests, following the package targets; it never calls the explicitly
framework-test-only BlazorGateway.BuildWebHost. RC2 needs
StaticWebAssetSpaFallbackEnabled=true: a real deep-link 404 was reproduced without
it, then corrected by the generated nonfile index endpoint. YARP's explicit API
route preserves backend 404 rather than returning the document.

The newer browser uses the generated initializer to fetch public configuration,
then the generated defaults' HTTP service-discovery handler resolves logical
`quays` to the browser entry origin. Gateway separately resolves logical
`quaybackend` to the loopback backend. The .NET 10 comparison uses an ASP.NET Core
host, WebAssembly.Server 10.0.13, YARP 2.3.0 and MapFallbackToFile. This remains a
valid older approach; no .NET 10 Gateway/template route is invented.

All six projects compile with their actual declared SDK: 10.0.402/C# 14/runtime
10.0.13 versus 11.0.100-rc.2.26504.105/C# 15/matching RC2 assets. Projects declare
their own target/language; isolated common build/package files supply the remaining
settings. Exact pins, lock files, imports, namespaces, document/CSS/script, backend,
proxy configuration and generated library/initializer are returned offline.
No workload, native compiler, AOT, external account or resource is required.

No Experimental diagnostic opt-in is present on this selected path. Stable API
classification does not establish GA behavior. ASPDEPR011 migration removes the
new path's obsolete inspectUri/UseWebAssemblyDebugging setup, while preserving the
valid .NET 10 development call. CLI/watch/debugger execution is not claimed.

## Execution and provenance

- [Canonical Chrome receipts](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-796.browser.evidence.json): six real WASM runs,
  covering direct north deep links, client navigation south and south reloads on
  both releases. Each managed button retrieves a new real backend counter value.
  Path/query/header preservation, missing API/file 404s, actual WASM resources and
  generated RC2 configuration/service-discovery requests are asserted.
- [HTTP receipts](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-796.http.evidence.json): both real entry/backend compositions
  serve deep links and preserve backend path/query/header. In the disposable
  reconstructed consumers, stopping each owned backend returns API 502 while the
  document remains 200. No mock or framework TestHost substitutes for this proof.
- [Execution fingerprints](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-796.execution.evidence.json): original DLLs were
  compiled on ecbc485 with dirty authored inputs; browser execution preceded
  feature commit 86ecf7d. Original compilation records remain at that commit.
  Later compilation captures at 756beb9 retain their actual dirty marker.
  The complete 29/30-input compilation sets differ only in the two non-executable
  snippet documents' final blank lines. Those compilation records were refreshed.
  All **711** application/host/uncompressed browser/manifest artifacts remain
  exactly equal. One copied evidence JSON outside wwwroot changed. This is measured
  reuse, not a claim that dirty captures ran on a clean final head.
- [Standalone reconstruction](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-796.consumer.evidence.json): the real stdio
  response's 29,640-character expanded Markdown supplies all 26 code fences and
  documented shared-file/SDK-copy instructions. The helper reconstructs all six
  projects in an unrelated temporary directory, restores/builds both actual SDK
  paths and checks every dependency closure against the served evidence. It copies
  no canonical checkout source or configuration. The old and new Backend projects
  are separate blocks with correct targets/language, repairing the preliminary
  shared-project omission before publication.
- [Six additional consumer Chrome runs](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-796.consumer-browser.evidence.json)
  prove the reconstructed apps' direct links, client navigation, reload and real
  backend reads. Their source paths/debug metadata and artifact identities are
  distinct from canonical builds; they do not replace canonical receipts.

The exact evaluated expression is the UTF-8 contents of
[QuaysideRoutes.browser.js](../../eng/QuaysideRoutes.browser.js), without appended
code; source and expression hashes are recorded separately. Chrome 155 used Pingu's
existing extension. T3 preview opened, but snapshot twice returned
`Preview snapshot failed: Preview automation snapshot timed out after 15000ms.`
before switching. No fresh profile or sibling tab/host was used.

Aspire dependency **when used** is explicit in discovery/detail. This local manual
composition does not use an Aspire orchestrator and makes no Aspire prerequisite
or runtime claim. Generated defaults alone do not establish Aspire integration.
OTLP delivery, production TLS/authentication/deployment, path bases, alternative
platform/browser matrices, trimming/AOT and GA remain outside the executed route.
No required Human-only proof is missing for the delivered local route.
