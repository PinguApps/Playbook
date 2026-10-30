# Inspect the guidance an agent receives

`playbook mcp` speaks protocol over stdin/stdout. Use MCP Inspector as a client; it initializes and closes each connection automatically. `eng/Inspect-Guidance.ps1` builds the real server, installs pinned Inspector 2.11.0 under ignored `artifacts/` if needed, calls both tools, and prints **ID/title/goal, full expanded Markdown and applicability**. Responses default to `artifacts/inspection/<id>.json`.

Requires PowerShell 7, the repository's .NET SDK 10.0.402 and Node 22.19+. Setup may restore/download packages online; guidance itself is embedded and offline. The helper invokes the resolved executables by absolute path without changing PATH or global tools. It disables .NET first-run PATH integration. No native workloads or account are required for server inspection.

## Span storage and dynamic logical binding

This block selects the actual task worktree and was checked starting from an
unrelated directory. It builds/packs once, installs that exact ordinary artifact
through the existing isolated local feed/cache, then uses real installed stdio
list/get to display both IDs, titles, explanatory goals, complete canonical
Markdown, explicit target11/edition11, applicability and fresh evidence.
Each target10 request must return `no_applicable_edition`; direct retrieval of the
excluded readonly-metadata goal must return `unknown_recipe`.

Run this block in **PowerShell 7 (`pwsh`)**. Windows PowerShell 5.1 changes native
argument/error handling and is outside this helper's prerequisites.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-6b360ce7'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('csharp.preserve-span-storage','csharp.bind-dynamic-conditions') `
  -RemovedRecipeId @('csharp.resolve-readonly-metadata') -Query '' `
  -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin1032-1034' -BuildAndPack `
  -Dotnet 'C:/Program Files/dotnet/dotnet.exe' -Node 'C:/Program Files/nodejs/node.exe' `
  -OutputDirectory ./artifacts/compiler-migrations/inspection
```

Expect **Preserve span storage across nested scopes** and **Preserve dynamic
logical operator binding** with their discovery descriptions and actual paired
evidence. Setup requires PowerShell7, SDK10.0.402/runtime10.0.13 and Node22.19+;
package/Inspector setup may restore online. Retrieval with unusable proxies proves
proxy independence, not OS-level network isolation. The exact eight foundations
and comprehensive/actual-GA/Human gates remain unchanged.

For sample reproduction, also provide SDK11.0.100-rc.2.26504.105/ref/runtime11 RC2
and run `eng/Test-CompilerMigrations.ps1 -SetupBaseline -Record`. On Windows x64
that helper can download/extract the official SHA512-checked portable SDK10.0.100
historical reproducer under ignored `artifacts/`; no installer or PATH integration
runs. The helper checks real Span/ReadOnlySpan old acceptance versus CS8352,
both old dynamic binder exceptions versus CS7083, paired corrected storage/aliasing
and all16 logical truth-table/invocation-count cases. Ordinary readonly
method-group/lambda/local calls and emitted InAttribute modifiers succeed in both
supported sets. PIN-1033 uses its explicit evidenced-exclusion allowance: no
supported deficient reference closure/CS0518 repair is claimed or taught.
See the [bounded source reconciliation](planning/audits/compiler-contracts.md#compiler-migration-delivery-reconciliation).

## Selected base-image trust, Azure Linux migration and collector observation

This block was checked starting outside the checkout. It builds/packs the ordinary
tool and installs that exact artifact in
the Inspector's isolated local feed/cache. Real stdio list/get display each ID,
title, **goal**, complete canonical Markdown, target11/edition11, applicability and
fresh evidence. Both target10 gets must return `no_applicable_edition`; the paired
older workers add no ninth foundation.

Setup requires PowerShell7, SDK10.0.402/runtime10.0.13, Node 22.19+ and online package
setup. For sample reproduction, also supply SDK11.0.100-rc.2.26504.105 and a working
Linux amd64 Docker engine. Notation1.3.2/ORAS1.3.6 archives, exact Microsoft root
certificates and image digests are checked by the authoring helpers. Signature
verification needs public registry/certificate access; embedded guidance retrieval
uses unusable proxies, establishing proxy independence rather than OS-level network
isolation. Worker runs use Docker `--network none`. No system trust store, production
registry or persistent environment setting is changed by the sample checks.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-713eb23e'
$previousCertificateSetup = $env:DOTNET_GENERATE_ASPNET_CERTIFICATE
$env:DOTNET_GENERATE_ASPNET_CERTIFICATE = 'false'
try {
    & ./eng/Inspect-BlazorMigrationGuidance.ps1 `
        -RecipeId @('containers.verify-base-image-signatures','containers.migrate-azurelinux-base-images','testing.migrate-collector-event-inputs') `
        -Query 'quay' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
        -PackageVersion '0.0.0-pin1137-1139' -BuildAndPack `
        -Dotnet 'C:/Program Files/dotnet/dotnet.exe' -Node 'C:/Program Files/nodejs/node.exe' `
        -OutputDirectory ./artifacts/quay-containers/inspection
}
finally { $env:DOTNET_GENERATE_ASPNET_CERTIFICATE = $previousCertificateSetup }
```

Expect **Verify the signature of a selected .NET base image**, **Migrate an
authored Azure Linux base-image selection** and **Observe VSTest collector events
without rewriting outcomes**, with their explanatory discovery goals.
The first records real strict index/platform verification, existing .NET10 signing,
wrong-root/scope and missing/tampered envelope rejection. The second records actual
managed worker execution on digest-pinned Azure Linux3/4, OS/runtime/architecture,
zlib export/call, ICU fr-FR formatting and absent configured-library exit23.
`eng/Test-QuayWorker.ps1 -Record` then `eng/Test-QuaySignatures.ps1 -Record`
reproduce those checks and refresh the existing sample records. Azure Linux4 stays
**Beta**. NativeAOT, other variants/architectures, performance, arbitrary ABI and GA
support remain unqualified. `eng/Test-QuayCollector.ps1 -Record` additionally
restores/builds paired locked18.7/18.11 package closures and loads the real
collector in matching test runs. Four real start/end events match TRX identity,
name, IsChild=false and outcome; collection preserves the disabled run's one
passing and one deliberate failing assertion. Separate constructor fixtures and
a compile-only assignment control confirm public setters were added, not removed.
No child=true/nested-framework transport, MTP or arbitrary collector compatibility
is claimed. PIN-959,
PIN-963 and unrelated Human gates remain open. No recurring native matrix is added.

## Disposable restore graphs, referenced symbols and evaluated imports

This block was checked from an unrelated directory. It builds/packs the ordinary
tool once, installs that exact artifact through the existing isolated local
feed/cache and calls real installed stdio list/get. It prints each ID/title/goal,
complete canonical Markdown, target11/edition11, applicability and current paired
evidence. Every target10 get must return `no_applicable_edition`; the real older
comparisons add no ninth foundation.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-c5612d78'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('nuget.skip-disposable-restore-graph','sdk.publish-referenced-symbols','msbuild.inspect-import-tree') `
  -Query 'beacon-build' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin1134-1136' -BuildAndPack `
  -Dotnet 'C:/Program Files/dotnet/dotnet.exe' -Node 'C:/Program Files/nodejs/node.exe' `
  -OutputDirectory ./artifacts/beacon/inspection
```

Expect the reusable titles **Avoid writing a disposable CI restore graph**,
**Include referenced symbols in published output** and **Inspect the actual build
import tree**, with explanatory goals and fresh actual evidence. Separately,
`eng/Test-BeaconBuild.ps1 -Record` recreates the paired local-package/locked-restore,
publish and evaluated-import controls with the exact SDK10/11 executable defaults
declared in that helper. It builds/runs both actual consumers and every publish;
checks true/false/default dgspec outputs, exact package hashes/equal selected assets,
normal local repeat and missing-package failure; verifies plural versus ineffective
singular symbol policy and own/reference/snupkg boundaries; and evaluates nested,
conditional, SDK, unset/false, ordering and duplicate/parent import controls.

Setup needs PowerShell7, SDK10.0.402/runtime/ref10.0.13, Node22.19+ and, to reproduce
samples, complete SDK11.0.100-rc.2.26504.105/runtime/ref11RC2. Tool/Inspector setup
may restore online; sample dependencies are original and local-only. Retrieval
with unusable proxies proves proxy independence, not network isolation. Actual
SDK10 already honours graph suppression/import items and includes the reference
PDB. SDK11's plural symbol property defaults true; false excludes that PDB, while
the app's own symbol setting remains separate. No redundant older copy workaround,
cache/no-op equivalence, speed saving, single-file/debugger, arbitrary import order,
other-platform or GA claim is made. PATH/persistent settings, the exact eight
foundations and PIN-959/PIN-963/Human gates remain unchanged.

## Token-binding removal and published F# MVC views

These two 11.0 editions use the existing actual-package Inspector with an isolated
local feed/cache and unrelated working directory. Requires PowerShell7,
SDK10.0.402/runtime10.0.13, SDK11.0.100-rc.2.26504.105/runtime11 RC2 and Node22.19+.
Sample restore and Inspector setup may use the network; retrieved guidance is
embedded. No trust-store change, production identity provider or native workload.

The block was exercised starting outside the checkout. The first command runs
paired real HTTPS/cookie checks, reproduces ASPDEPR010, compiles/renders F# views,
publishes to a clean source-free directory and exercises the missing-view control.
It refreshes the existing sample records. The second command builds/packs once,
installs that exact ordinary artifact and prints what an agent receives.

Use PowerShell 7 (`pwsh`): Windows PowerShell 5.1 changes native JSON-argument
quoting. The fixture retains `artifacts/web-retirement-<guid>` as disposable
published-consumer output for inspection.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-61e6bfa1'
& ./eng/Test-WebRetirement.ps1 -RecordEvidence
& ./eng/Inspect-BlazorMigrationGuidance.ps1 -BuildAndPack `
  -Dotnet 'C:/Program Files/dotnet/dotnet.exe' `
  -PackageVersion '0.0.0-pin1185-1186' -Query 'web-retirement' `
  -RecipeId @('authentication.remove-token-binding-assumption','mvc.compile-fsharp-views') `
  -UnsupportedTargetVersion '10.0' -OutputDirectory ./artifacts/web-retirement/inspection
```

Expect both discovery IDs, titles and explanatory goals, complete expanded
Markdown, explicit11.0 applicability and current evidence, with text/structured
equality and unsupported10.0 rejection. The two comparisons add no .NET10
foundation. The F# current consumer prints the actual .Views.dll path and matching
owner load context; its negative publish builds but returns the specific missing
view500. Token binding is RFC8471, not RFC5929 channel binding; removal preserves
the fixture's real authorization/HTTPS policy and supplies no replacement binding
security. Proxy-blocked retrieval is not network-isolation proof. RC2 Windows x64
execution is not actual-GA, arbitrary loader/trimming, browser or Hot Reload proof.
PIN-959/PIN-964 and unrelated Human gates stay open.

## HTTP/3 budgets, JSON Patch intent and early integration-test setup

This block builds/packs the ordinary tool once and installs that exact artifact
through the existing isolated local feed/cache. Real installed stdio list/get run
from an unrelated directory and display ID/title/goal, complete canonical Markdown,
target11/edition11, full applicability and current paired sample evidence. Each
target10 get must return `no_applicable_edition`; these comparisons do not add
another .NET10 foundation. The last command reconstructs all six consumers solely
from the returned Markdown in a temporary directory, without remote NuGet feeds.

Setup needs PowerShell7, Node22.19+, both exact SDK/runtime sets below, Windows
x64 with TLS1.3 and working MsQuic2.5.11.159415436. The first command restores
the locked packages, compares audited ZIP/reference/compile assets and records
compiler/runtime/native identities plus actual requests. No certificate trust,
production resource or persistent environment change is made. Prepared packages
are reused for the returned-body consumers. Retrieval uses unusable proxies;
loopback is bypassed for actual requests. This proves proxy independence, not
network isolation. No recurring native/client/OS matrix is added.

The complete block was checked starting from an unrelated directory:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-5b4d9f99'
& ./eng/Test-WebResourceRecipes.ps1 `
  -Dotnet 'C:/Program Files/dotnet/dotnet.exe' `
  -Net11Dotnet 'C:/Users/pingu/.dotnet-playbook-11/dotnet.exe' -RecordEvidence
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('hosting.configure-http3-budgets', 'json-patch.serialize-operation-intent', 'testing.configure-web-application-builder') `
  -Query 'web-resource-recipes' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin1182-batch' -BuildAndPack `
  -Dotnet 'C:/Program Files/dotnet/dotnet.exe' -Node 'C:/Program Files/nodejs/node.exe' `
  -OutputDirectory ./artifacts/web-resources/inspection
& ./eng/Test-WebResourceConsumers.ps1 `
  -Dotnet 'C:/Program Files/dotnet/dotnet.exe' `
  -Net11Dotnet 'C:/Users/pingu/.dotnet-playbook-11/dotnet.exe'
```

Expect these discoveries and observations:

- `hosting.configure-http3-budgets`: **Configure HTTP/3 connection budgets without
  preview scaffolding**. Goal: set per-connection stream/buffer budgets and retain
  backpressure. Both real HTTP/3 consumers queue a third request behind two active
  requests on one connection, exhaust inbound unidirectional credit and pause/
  resume 16MiB uploads/downloads. The old CA2252 control warns; newer builds need
  no preview acknowledgement. No exact process-memory or application quota claim.
- `json-patch.serialize-operation-intent`: **Preserve JSON Patch operation
  serialization intent**. Goal: preserve required from/value intent and nulls,
  checking actual updates before removing custom rules. Six-operation round trips,
  wire members and HTTP failures pass. The current direct method call warns CS0618.
  New from metadata omits null, but retains a spurious non-null from. Both readers
  accept missing replace value as null; stricter validation remains app policy.
- `testing.configure-web-application-builder`: **Configure a web integration test
  through the application builder**. Goal: isolate early startup configuration/
  services and await fixture disposal. Actual separate entry assemblies/TestServer
  requests verify early/late order, production controls and async disposal; the
  newer hook rejects explicit IHostBuilder/IWebHostBuilder paths. Content-root
  attributes make the separate consumers independent of solution discovery; explicit
  service completion handles overlapping teardown. .NET10 keeps
  ConfigureWebHost; only older .NET11 preview fixtures used the renamed spelling.

Actual Windows x64 RC2 proof does not establish other OS/native lanes, strict
JSON Patch validation/security, physical TestServer network behavior or GA.
PIN-959/PIN-964, the eight foundations and unrelated Human gates stay unchanged.

## SDK servicing, partial ReadyToRun and native tool RIDs

The following block was verified starting from an unrelated directory. Setup
requires PowerShell7, Node22.19+, both exact SDK/runtime distributions below,
public NuGet access, MSVC14.51.36231/link14.51.36260.0 and Windows SDK10.0.26100.0
libraries. The focused check runs controlled advisory-cache cases (not a live
security verdict), full/selected PE publishing with JIT fallback, and real native
packing/install/run plus unsupported/missing-toolchain failures. It preserves
process/user/machine PATH and uses disposable homes/caches.

The Inspector builds/packs the ordinary tool once, installs that exact artifact
through an isolated local-only feed/cache, then calls real installed stdio list/get
from an unrelated directory. It displays ID/title/**goal**, full expanded Markdown,
target11/edition11, applicability and current paired sample evidence. Target10 must
return no_applicable_edition; these comparisons add no historical foundations.
Unusable proxies prove proxy independence, not OS-level network isolation.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-3263bd3f'
$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
& 'C:/Program Files/dotnet/dotnet.exe' build PinguApps.Playbook.slnx -c Release -p:Version=0.0.0-pin1131-batch
& 'C:/Program Files/dotnet/dotnet.exe' pack ./src/PinguApps.Playbook -c Release --no-build -p:PackageVersion=0.0.0-pin1131-batch --output ./artifacts
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('sdk.check-sdk-servicing-status', 'sdk.choose-partial-readytorun-inputs', 'sdk.customize-native-tool-package-rids') `
  -Query 'skylark-build' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin1131-batch' `
  -Dotnet 'C:/Program Files/dotnet/dotnet.exe' -Node 'C:/Program Files/nodejs/node.exe' `
  -OutputDirectory ./artifacts/skylark-build/inspection
```

Expect all six consumers to pass. Current controlled vulnerability/stale and EOL
metadata warn NETSDK1238 and NETSDK1239; missing/corrupt data silently skips.
ReadyToRun checks inspect actual headers, partial selection and exclusions, including
zero unprofiled partial native methods versus a nonempty full control, then
execute direct=21/late=35/cold=SK-7 normally and with forced JIT. Native commands
create outer plus one win-x64 inner package, install locally and report crates=21,
architecture=X64, dynamic=False from a real native payload; unsupported/missing
toolchains fail. Candidate Arm64 defaults are evaluated separately, never advertised
as a delivered payload. No live advisory freshness, performance, other-platform,
GA, production resource or public-publication claim is made. PIN-959/PIN-963 and
unrelated Human gates remain open.

## Read a recipe

### VSTest host, resolver and custom task migrations

This block was exercised from an unrelated directory. Prerequisites: PowerShell7,
the two exact SDK/runtime installations below, Node22.19+ and public NuGet access
for setup. The first command captures the real old/current platform consumers,
compiler controls, test counts/cancellation/cleanup, extension loading and effective
settings, including the scalar semicolon failure and XML alternative. It preserves
process/user/machine PATH and uses isolated homes/caches.

The Inspector then builds/packs the ordinary tool, installs that exact artifact
through a fresh local-only feed/cache and calls real installed stdio list/get from
an unrelated directory. Both targets are explicit. It displays ID/title/**goal**,
complete canonical Markdown, edition11, applicability and current sample evidence;
target10 is a separately executed conditional public-package route. Unusable
proxies qualify proxy independence, not network isolation.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-d4b8d1f7'
& ./eng/Test-TernTesting.ps1 `
  -Net10Dotnet 'C:/Program Files/dotnet/dotnet.exe' `
  -Net11Dotnet 'C:/Users/pingu/.dotnet-playbook-11/dotnet.exe' -Record
$recipes = @(
  'testing.migrate-vstest-session-host',
  'testing.migrate-assembly-resolution-context',
  'testing.pass-custom-task-runsettings'
)
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId $recipes -Query 'tern-testing' -TargetVersion '11.0' `
  -PackageVersion '0.0.0-pin1128-vstest' -BuildAndPack `
  -Dotnet 'C:/Program Files/dotnet/dotnet.exe' -Node 'C:/Program Files/nodejs/node.exe' `
  -OutputDirectory ./artifacts/tern-testing/inspection11
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId $recipes -Query 'tern-testing' -TargetVersion '10.0' `
  -PackageVersion '0.0.0-pin1128-vstest' `
  -Dotnet 'C:/Program Files/dotnet/dotnet.exe' -Node 'C:/Program Files/nodejs/node.exe' `
  -OutputDirectory ./artifacts/tern-testing/inspection10
```

Expect four distinct discoveries; each ordinary/post-cancel host run has two
passes and one deliberate failure, cancellation has zero results, and observed
owned testhosts exit. Actual plugin resolution returns west:13 with its real
requestor, while the platform's .NET ALC path has a null requestor. Each custom
task selects/passes exactly one settings test with spaces, backslashes, separators
and switch-looking values unchanged. Old escaped literal semicolons survive;
the current scalar rejects them and its XML-file alternative passes.
Missing Tern settings must fail one selected test; XML verification also checks
the effective literal-semicolon value in the real TRX.

The resolver's old setter belongs to the audited SDK10 binary, **not** public18.7;
public18.9 already has the constructor. The current package consumer works on10/11.
No pooled-lifetime equivalence, MTP drop-in, general plugin security, VSTestTask2
runtime, Unix backslash behavior, arbitrary adapters/collectors or actual-GA claim
is made. PIN-959/PIN-963/Human gates and all eight foundations remain open.

The following example was checked from an unrelated directory. Change `-RecipeId` and `-TargetVersion` for the affected guidance; both list and get receive the same explicit target.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-4d84c6cb'
& ./eng/Inspect-Guidance.ps1 `
  -Dotnet 'C:\Program Files\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -TargetVersion '11.0' `
  -RecipeId 'blazor.configure-browser-from-server'
```

Expect `blazor.configure-browser-from-server`, its reusable title and goal, complete original .NET 10/11 TideDesk setup, and full target-11 requirements. There must be no unresolved `{{snippet:...}}` token. Compilation, browser execution and historical RC2 limits remain separate. Use `-NoBuild` only after a current Release build. Use `-OutputDirectory` for another disposable output location.

To inspect several changed recipes, pass an array to `-RecipeId`. Discovery follows all returned pages with unchanged filters before selecting those IDs. A missing ID, failed command or incomplete body fails the helper. The file output is the actual MCP response, including `result.structuredContent.recipe`; it can feed existing returned-body consumer checks:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-4d84c6cb'
& ./eng/Test-TideDeskConsumer.ps1
```

That consumer uses the inspection response above, reconstructs both actual project pairs outside the checkout, compiles them with their declared SDKs, and checks real RC2 HTTP options. It requires both declared SDK installations. The report defaults to ignored `artifacts/tide-desk/consumer.json`; `-ResponsePath` and `-OutputPath` select alternatives. It does not repeat native browser reconnection.

## Identity UI, encoded routes and gRPC documents

These three original 11.0 editions share one ordinary package inspection. Setup
requires both exact sample SDK/runtime sets, PowerShell7, Node22.19+ and authenticated
GitHub CLI access to the public pinned generator release. The first command restores
the actual locked packages, verifies/acquires the Windows x64 generator, exercises
the paired samples/publishes and refreshes saved sample records. It does not create
fresh browser evidence. Existing browser observations remain valid only while their
relevant page/layout/CSS/package inputs remain unchanged.

The block was exercised from an unrelated directory. The Inspector builds/packs
once, installs that exact artifact through a fresh local feed/cache, and performs
real installed stdio list/get for **each** ID. It displays ID/title/goal, complete
canonical Markdown, target11/edition11, applicability and fresh evidence, compares
text/structured results, and checks target10's `no_applicable_edition` response.
The final command reconstructs all six original consumers solely from those
returned bodies. Prepared packages are reused with no remote NuGet feeds; remote
proxies are unusable and loopback is explicitly bypassed for the real requests.
This establishes proxy independence, not network isolation.

```powershell
Set-Location -LiteralPath 'V:/_worktrees/Playbook/agent-pin-1178-04-103-move-customized-identity-ui-off-bootstrap-4'
& ./eng/Test-WebMigrationBatch.ps1 `
  -Dotnet 'C:/Program Files/dotnet/dotnet.exe' `
  -Net11Dotnet 'C:/Users/pingu/.dotnet-playbook-11/dotnet.exe' `
  -OutputDirectory ./artifacts/batch04/verified -RecordEvidence
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('identity.migrate-ui-framework-selection', 'routing.preserve-encoded-segment', 'grpc.replace-swagger-adapter') `
  -Query 'web-migration-batch' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin1178-batch' -BuildAndPack `
  -Dotnet 'C:/Program Files/dotnet/dotnet.exe' -Node 'C:/Program Files/nodejs/node.exe' `
  -OutputDirectory ./artifacts/batch04/inspection
& ./eng/Test-WebMigrationConsumers.ps1 `
  -Dotnet 'C:/Program Files/dotnet/dotnet.exe' `
  -Net11Dotnet 'C:/Users/pingu/.dotnet-playbook-11/dotnet.exe'
```

Expect these discoveries:

- `identity.migrate-ui-framework-selection`: **Move customized Identity UI off
  Bootstrap 4**. Goal: keep customized sign-in pages rendering by selecting
  Bootstrap5 and migrating affected markup, styles and published assets.
- `routing.preserve-encoded-segment`: **Preserve encoded route segments in
  proxy-style requests**. Goal: retain encoded slashes inside identifiers and
  remove compensation only after actual wire-level comparison.
- `grpc.replace-swagger-adapter`: **Replace the retired gRPC Swagger integration**.
  Goal: keep the transcoded JSON interface discoverable using a generated,
  protobuf-derived build contract after adapter retirement.

The Identity check uses published compiled pages/assets and retains antiforgery,
confirmation and local-return behavior. The separate retained Chromium screenshots
qualify login/validation rendering for the older, explicit5 and default publishes;
the command block does not repeat a browser session. Routing proves the measured
old absolute-form decoding difference and current preservation, with raw parser,
middleware and handler observations. gRPC proves actual old adapter/new generator,
gRPC and JSON values, nonempty operations/schemas/comments and negative requests.
The replacement document is **Swagger2.0/OpenAPIv2**, not a runtime drop-in adapter
or an automatic AddOpenApi integration. Neither generic provider compatibility,
all Google annotation patterns, all browser/platforms nor actual-GA proof follows.
Eight foundations and PIN-959/PIN-963/PIN-964/Human gates remain unchanged.

## Generated static endpoint order

This builds/packs the ordinary tool and installs that exact artifact through the
existing isolated local feed/cache. Real installed stdio list/get execute from an
unrelated directory with unusable proxies, displaying ID/title/goal, full canonical
Markdown, target11/edition11, applicability and fresh paired sample evidence.
The extra get checks target10's `no_applicable_edition` response. Inspection needs
PowerShell7, SDK10.0.402 and Node22.19+. The reconstructed consumer also requires
SDK11.0.100-rc.2.26504.105 and its matching runtime, a preview RC2 baseline, not GA.
The block passes both installed SDK executables explicitly; adjust those arguments
for another installation. Setup may restore online. Retrieval proves proxy
independence, not network isolation. No global installation is required.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\agent-pin-1176-04-101-give-a-generated-static-endpoint-deliberate-route'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId 'web.order-static-asset-routes' -Query 'static-assets' `
  -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin1176' -BuildAndPack `
  -Dotnet 'C:\Program Files\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory ./artifacts/pin1176/inspection
& ./eng/Test-AmberGalleryConsumer.ps1 `
  -Dotnet 'C:\Program Files\dotnet\dotnet.exe' `
  -Net11Dotnet 'C:\Users\pingu\.dotnet-playbook-11\dotnet.exe'
```

Expect `web.order-static-asset-routes`, title "Give a generated static endpoint
deliberate route precedence" and goal "Choose which static resource wins when
generated asset routes overlap, using explicit endpoint order and verifying the
resource actually served." Complete Markdown includes both original projects,
physical assets, boundary files, manifest acquisition/projection and numbered
migration. The final helper reconstructs only returned Markdown in a temporary
unrelated directory, builds with both exact SDKs and no remote NuGet feeds, then
executes actual manifest/HTTP/failure checks. It writes routine results under
ignored `artifacts/pin1176/consumer`.

Current results qualify default/explicit/invalid string order, numeric JSON
failure, equal-order ambiguity and frozen-descriptor rejection; .NET10 retains
its working convention alternative. Only the bounded generated-manifest consumer
and Windows x64 managed HTTP lane are qualified, with no general arbitrary-schema,
browser, published-layout or GA guarantee. The eight .NET10 foundations and
PIN-959/PIN-963/PIN-964/Human gates remain unchanged.

## Dynamic OpenAPI document names

This builds/packs the ordinary tool and installs that exact artifact through an
isolated local feed/cache. Real installed stdio list/get run from an unrelated
directory with unusable proxies and display ID/title/goal, complete canonical
Markdown, target11/edition11, applicability and fresh sample evidence. The extra
get checks target10's explicit `no_applicable_edition` response. Setup requires
PowerShell7, SDK10.0.402 and Node22.19+ and may restore online; retrieval proves
proxy independence, not network isolation. No global installation is needed.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\agent-pin-1174-04-099-generate-contracts-for-dynamically-named-api'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId 'openapi.discover-dynamic-documents' -Query 'dynamic-documents' `
  -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin1174' -BuildAndPack `
  -Dotnet 'C:\Program Files\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory ./artifacts/pin1174/inspection
```

Expect title "Generate contracts for dynamically named API documents" and goal
"Discover all OpenAPI document names offered by an application when names come
from configuration, and generate matching runtime and build-time contracts."
The complete body contains both original PlanetariumContracts projects, named
options, resolver, runtime checks, generation commands and numbered migration.
Both public/staff documents have distinct paths/titles. Current controls establish
exact duplicate elimination, explicit plus additional names and missing/empty/null
behavior through the real built-in provider/GetDocument host. Older explicit
registrations remain valid; no downlevel resolver, arbitrary-provider, client,
browser or actual-GA claim is made. PIN-959/PIN-963/PIN-964 and Human gates stay open.

To refresh the bounded proof with both actual SDK/runtime/package sets:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\agent-pin-1174-04-099-generate-contracts-for-dynamically-named-api'
& ./eng/Test-Planetarium.ps1 `
  -Dotnet 'C:\Program Files\dotnet\dotnet.exe' `
  -Net11Dotnet 'C:\Users\pingu\.dotnet-playbook-11\dotnet.exe' -RecordEvidence
```

Expect real runtime and build-time PASS lines on both targets, plus expected
empty/null-name generation rejection on .NET11. The helper restores its process
settings and checks all PATH scopes. Generated JSON and routine logs stay under
ignored artifacts; named ongoing sample records retain hashes and observations.

## Closed native build extensions

This block builds/packs the ordinary tool, installs that exact artifact through
the existing isolated local feed/cache, then calls real installed stdio list/get
from an unrelated directory. It displays ID/title/goal, complete canonical
Markdown, target11/edition11, applicability and both managed plus actual native
evidence. PowerShell7, SDK10.0.402 and Node22.19+ are required for inspection.
Setup may restore online; unusable proxies during retrieval establish proxy
independence, not network isolation. The inspection itself needs no native tools.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\agent-pin-1120-03-025-register-a-closed-set-of-build-extensions-in-an-aot'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId 'msbuild.register-aot-build-extensions' -Query 'native-build-extensions' `
  -TargetVersion '11.0' -PackageVersion '0.0.0-pin1120' -BuildAndPack `
  -Dotnet 'C:\Program Files\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory ./artifacts/pin1120/inspection
```

Expect "Register a closed set of build extensions in an AOT host", its reusable
goal, all project/SDK/task/resolver/logger/bootstrap/package snippets, numbered
migration, exact package/toolchain conditions and three fresh evidence records.
SDK10 retains the verified managed alternative. SDK11 uses public package18.11.0,
distinct from bundled18.12; native execution verifies the fixed set and missing
task/resolver MSB4283/MSB4282. IL2026 remains visible at the public build boundary.
No analyzer-clean/general AOT, down-level native, arbitrary plugin or actual-GA
claim is made. Target10 remains unsupported for this new registration recipe.

To refresh the actual paired/native proof with the exact prerequisites recorded
in the recipe and sample pins:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\agent-pin-1120-03-025-register-a-closed-set-of-build-extensions-in-an-aot'
& ./eng/Record-NorthQuayBuildHost.ps1 `
  -Net10Dotnet 'C:\Program Files\dotnet\dotnet.exe' `
  -Net11Dotnet 'C:\Users\pingu\.dotnet-playbook-11\dotnet.exe'
```

This first restores both locked package closures, then invokes the existing
sample evidence workflow and real ILC/platform-linker publish/native execution.
An empty `-Packages <absolute-cache-directory>` is supported; no incidental older
native compiler cache is required. It uses absolute executables and child-only
native include/library inputs; PATH and persistent environment must stay unchanged.
The public Framework package supplies the eight trim feature settings, checked
in the actual ILC response. IL2026 is retained, not suppressed. The eight approved
foundations and PIN-959/PIN-963/actual-GA/Human gates remain open.

## Partial project evaluation

This block builds/packs the ordinary tool, installs that exact artifact through
an isolated local feed/cache, and calls real installed stdio list/get from an
unrelated directory. It displays ID/title/goal, complete canonical Markdown,
target11/edition11, applicability and both current museum host evidence records.
PowerShell7, SDK10.0.402 and Node22.19+ are required for inspection. Setup may
restore online; retrieval with unusable proxies proves proxy independence, not
network isolation. No global tool or PATH change is needed.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\agent-pin-1119-03-024-evaluate-only-the-project-information-a-tool-needs'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId 'msbuild.evaluate-project-stages' -Query 'partial-evaluation' `
  -TargetVersion '11.0' -PackageVersion '0.0.0-pin1119' -BuildAndPack `
  -Dotnet 'C:\Program Files\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory ./artifacts/pin1119/inspection
```

Expect title "Evaluate only the project information a tool needs", the reusable
inspection goal, complete museum project/import/readers/bootstrap and numbered
migration. SDK10 retains full evaluation; SDK11 proves all stages and guard
failures, default/explicit Full, real CLI property/item values and separate target
execution. In actual RC2 partial `Targets.Count` throws, superseding Preview7's
zero-count example. The host uses Locator1.11.2 with complete installed SDK closure;
no down-level partial, universal speed, other-platform or actual-GA claim is made.
Target10 remains unsupported for this new partial-evaluation recipe.

To refresh the bounded managed host proof with both complete SDK/runtime installs:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\agent-pin-1119-03-024-evaluate-only-the-project-information-a-tool-needs'
$previousIntegration = $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
try {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
  & ./eng/Record-Evidence.ps1 -Sample 'samples/net10.0/MuseumInventory/MuseumInventory.csproj' `
    -Dotnet 'C:\Program Files\dotnet\dotnet.exe'
  & ./eng/Record-Evidence.ps1 -Sample 'samples/net11.0/MuseumInventory/MuseumInventory.csproj' `
    -Dotnet 'C:\Users\pingu\.dotnet-playbook-11\dotnet.exe'
}
finally { $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = $previousIntegration }
```

These sample records pin compiler/reference/native-host/runtime/MSBuild/SDK
resolver artifacts and record all loaded assemblies and actual SDK/import hashes.
The target-count guard and mutable factory failures are real execution evidence,
not zero counts fabricated for unavailable state. The exact eight foundations,
PIN-959/PIN-963/actual-GA and unrelated Human gates remain open.

## Typed custom build-task inputs

This block builds/packs the ordinary artifact and uses the existing isolated
local-only feed/cache Inspector harness. It invokes real installed stdio list/get
from an unrelated directory, displaying ID/title/goal, full canonical Markdown,
explicit target/edition, applicability and both fresh host evidence records.
Setup requires PowerShell 7, SDK 10.0.402 and Node 22.19+; setup can restore online.
Retrieval uses unusable proxies, proving proxy independence, not network isolation.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\agent-pin-1118-03-023-bind-typed-parameters-in-a-safe-custom-build-task'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId 'msbuild.bind-typed-task-inputs' -Query 'typed-task-inputs' `
  -TargetVersion '11.0' -PackageVersion '0.0.0-pin1118' -BuildAndPack `
  -Dotnet 'C:\Program Files\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory ./artifacts/pin1118/inspection
```

Expect title "Bind typed file inputs in a custom MSBuild task", edition 11.0,
target 11.0, experimental `-mt` conditions and two current compilation/execution
records. Markdown retains the explicit-path older task, typed newer task,
environment-dependent defaults, same-binary constructor fallback, numbered
migration and exact-host limits. The saved runtime proof covers actual concurrent
same-process/different-thread task execution, metadata, unchanged task process
state, ordinary/sidecar controls and real missing-file failures. No down-level
typed-task, server-reuse/performance or actual-GA route is claimed.

To refresh the task evidence, run SDK 10 first because the SDK 11 constructor
check executes that same older compatibility DLL. Neither command installs a
global tool or changes PATH; SDK 11 and its matching runtime must already exist:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\agent-pin-1118-03-023-bind-typed-parameters-in-a-safe-custom-build-task'
$previousIntegration = $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
try {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
  & ./eng/Record-Evidence.ps1 -Sample 'samples/net10.0/ObservatoryTasks/ObservatoryTasks.csproj' `
    -Dotnet 'C:\Program Files\dotnet\dotnet.exe'
  & ./eng/Record-Evidence.ps1 -Sample 'samples/net11.0/ObservatoryTasks/ObservatoryTasks.csproj' `
    -Dotnet 'C:\Users\pingu\.dotnet-playbook-11\dotnet.exe'
}
finally { $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = $previousIntegration }
```

PIN-959/PIN-963 and unrelated Human gates remain open. There is no recurring
native/client/OS matrix for this managed Windows x64 recipe.

## Typed VSTest message readers

This tested block builds/packs the ordinary tool, installs the exact package in
an isolated local feed/cache, then calls installed stdio list/get from an
unrelated directory. It prints ID/title/goal, complete canonical Markdown,
explicit target/edition, applicability and all three current evidence records.
PowerShell7, SDK 10.0.402 and Node22.19+ are required; setup may use the network.
Retrieval uses unusable proxies, proving proxy independence, not network isolation.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\agent-pin-1073-03-021-read-vstest-messages-through-the-supported-typed'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId 'testing.migrate-vstest-message-readers' -Query 'vstest-messages' `
  -TargetVersion '11.0' -PackageVersion '0.0.0-pin1073' -BuildAndPack `
  -Dotnet 'C:\Program Files\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory ./artifacts/pin1073/inspection
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId 'testing.migrate-vstest-message-readers' -Query 'vstest-messages' `
  -TargetVersion '10.0' -PackageVersion '0.0.0-pin1073' `
  -Dotnet 'C:\Program Files\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory ./artifacts/pin1073/inspection10
```

Expect title "Read VSTest messages through the supported typed contract",
edition 11.0 and a conditional target10 route. The laboratory examples retain
actual 18.7/18.11 package and SDK/runtime distinctions, typed TestCase/traits/
TestResult/ITestRunStatistics checks, protocol/header/failure semantics and the
concrete-statistics limitation. Custom resolver/converter extensions have no
replacement promised. `eng/Test-VSTestMessages.ps1 -Record` captures the separate
real published-consumer, foreign-wire, removed-symbol and ABI proof with both
healthy pinned SDK executables. Retrieval is not actual-GA or arbitrary test-host
integration evidence; PIN-959/PIN-963 and independent Human gates remain open.

## Deliberate database-provider replacement

This tested example selects the PIN-1190 worktree from an unrelated directory,
builds/packs the ordinary tool and installs that exact package through an isolated
local feed/cache. The helper calls actual installed stdio `list_recipes` and
`get_recipe`, showing ID/title/goal, complete canonical Markdown, target/edition,
applicability and fresh evidence. Setup requires PowerShell7, SDK10.0.402 and
Node22.19+ and may restore packages online. Retrieval with unusable proxies proves
proxy independence, not network isolation. No global tool or PATH change is needed.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\agent-pin-1190-05-049-replace-a-configured-database-provider-deliberately'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId 'ef.replace-context-provider' -Query 'provider-replacement' `
  -TargetVersion '11.0' -PackageVersion '0.0.0-pin1190' -BuildAndPack `
  -Dotnet 'C:\Program Files\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory ./artifacts/pin1190/inspection
```

Expect `ef.replace-context-provider`, title "Replace a configured database provider
deliberately", its test-configuration goal, target11/edition11 and two current
ReedPermits evidence records. Full Markdown includes both complete original
projects. Provider proof covers actual SQLite/InMemory CRUD, retained/mixed
configuration failures and resolved lifetimes; InMemory is not relational proof.
The options helpers are separate infrastructure operations. Other providers,
platforms, factories/pooling, trimming/AOT and actual GA remain unverified.

## Custom client rules and component cache contracts

This block builds/packs the ordinary tool and uses the existing isolated local-feed/cache
Inspector harness. It changes to an unrelated temporary directory before calling
the exact installed executable over stdio, and displays both recipes' ID, title,
goal, complete Markdown, applicability and current compilation/execution evidence.
Setup requires PowerShell 7, SDK 10.0.402 and Node 22.19+; Inspector 2.11.0 is
installed locally when absent. Setup may use the network; retrieval uses unusable
proxies, which demonstrates proxy independence rather than network isolation.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-8fca8201'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('blazor.export-custom-client-validation-rules','blazor.declare-component-cache-safety') `
  -Query 'garden-library' -PackageVersion '0.0.0-garden-library' `
  -Dotnet 'C:\Program Files\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' -BuildAndPack `
  -OutputDirectory ./artifacts/garden-library/inspection
```

Expect two explicit `11.0` editions, full canonical-source equality, complete
static-SSR requirements and four current evidence records per recipe. The custom
rule body includes the .NET 10 duplicate handler and .NET 11 provider/JS mapping;
the cache body includes live parameter replay and refusal after initialization.
These checks deliver the guidance; the paired real Chromium/HTTP evidence comes
from `eng/Record-GardenLibrary.ps1`. RC2 is not GA proof. No native or distributed
provider behavior is claimed. The helper retains its unique temporary install/feed
for inspection and restores process settings; it does not install a global tool.

## JavaScript recovery and InputFile cleanup

For JavaScript failure recovery and derived InputFile cleanup, this tested block
builds/packs the ordinary tool, installs that exact package through the isolated
local feed/cache, and calls real stdio list/get from an unrelated temporary
directory. PowerShell 7, SDK 10.0.402 and Node 22.19+ are required for inspection;
setup may use the network. Retrieval uses unusable proxies, which proves proxy
independence rather than network isolation.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-e039c0ab'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('blazor.handle-js-invocation-failures','blazor.dispose-input-file-subclass-resources') `
  -Query 'owned-lifetimes' -PackageVersion '0.0.0-owned-lifetimes' `
  -Dotnet 'C:\Program Files\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' -BuildAndPack `
  -OutputDirectory ./artifacts/owned-lifetimes/inspection
```

Expect both IDs/titles/goals, explicit 11.0 editions, complete expanded canonical
.NET 10/11 files, applicability and four fresh compilation/execution records per
recipe. Recovery preserves application errors; teardown distinguishes RC2's
internally guarded reference disposal from an actual disconnected module call.
InputFile cleanup preserves ordinary selection and its framework callback relay.
The paired browser proof is separate from retrieval, and neither establishes GA.
With both SDKs and the measured Chromium/Playwright installations available,
`eng/Test-OwnedLifetimesConsumer.ps1` reconstructs all four projects solely from
these responses and reruns their real browser/renderer checks. The script's
`-Playwright` and `-Chromium` parameters take the absolute installed paths.

## Live identity tools

For the live identity recipes, this block builds/packs the ordinary tool and
uses the existing isolated local-feed/cache installation. It calls real stdio
list/get from an unrelated directory and prints discovery ID/title/goal, complete
canonical Markdown, applicability and fresh sample evidence. The walkthrough was
launched from an unrelated directory. Setup may restore/download packages;
retrieval uses unusable proxies, which establishes proxy independence rather
than network isolation.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-2784b7c8'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('signalr.refresh-live-credentials','blazor.react-to-refreshed-identity') `
  -Query '' -PackageVersion '0.0.0-live-tool-contracts' `
  -Dotnet 'V:\_worktrees\Playbook\t3-2784b7c8\artifacts\identity-tooling\pinned-dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' -BuildAndPack `
  -OutputDirectory ./artifacts/identity-tooling/inspection
```

Expect two explicit 11.0 editions, original .NET 10/RC2 consumers, the complete
TypeScript development source-build prerequisite, local identity/provider limits
and current compilation/execution records. `eng/Test-LiveIdentityConsumer.ps1`
reconstructs the .NET and TypeScript consumers solely from these installed
responses and executes their real hub and Interactive Server checks; supply
absolute `-Playwright`, `-Chromium`, `-Dotnet` and `-Node` paths. This is bounded
local execution, not Azure SignalR Service, production authentication or GA proof.
The task-local SDK root above preserves the recorded 10.0.402/RC2 baselines
after the machine's installation advanced to 10.0.403/RC3. An equivalent installed
root containing those exact SDKs and runtimes can be supplied instead.

## Runtime overrides and SDK selection for tool execution

From PowerShell 7 with SDK 10.0.402 and Node 22.19+, this builds/packs the
ordinary tool, then reuses the isolated local-feed/cache harness to inspect that
exact artifact over real stdio from an unrelated temporary directory. Inspector
2.11.0 is installed locally if missing. Setup may restore packages online;
retrieval uses unusable proxies (proxy independence, not network isolation).

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-72e14ee7'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('deployment.control-runtime-overrides','sdk.select-tool-execution-sdk') `
  -Query 'host-selection' -PackageVersion '0.0.0-host-selection' `
  -Dotnet 'C:\Program Files\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' -BuildAndPack `
  -OutputDirectory ./artifacts/host-selection/inspection
```

Expect both IDs/titles/goals, explicit edition
11.0, complete expanded canonical Markdown, applicability and four current
compilation/execution records per recipe. The second recipe correctly reports
introduction 10.0: wrapper bypass was serviced into 10.0.302/10.0.400 and was
executed here on 10.0.402 and RC2. This adds no target-10 foundation. The existing
helper's name is historical; its recipe-ID/query parameters accept both recipes.

Actual behavior is separate from retrieval. `eng/Test-RuntimeOverrides.ps1`
executes the original .NET 10/11 samples with isolated matching hostfxr/runtime,
duplicate and nonduplicate properties, source regeneration, Release dev-file
generation/copy and a fresh production artifact allowlist.
`eng/Test-ToolSdkSelection.ps1` executes both original local packages, three SDK
pins, both shipped Windows wrappers, exact quoted arguments and failure exits.
These require the declared Windows x64 SDK/runtime installations; parameters
accept alternate absolute installation paths. SDK 9 is a negative command case
only. Unix wrapper execution and actual .NET 11 GA reconciliation remain unverified.
## Nested EF scalars and primitive sequences

This block builds/packs once, runs the existing installed MCP smoke scenario,
then uses that same artifact through the existing local-feed/cache Inspector
harness. It was checked starting from an unrelated directory. Requires PowerShell
7, SDK 10.0.402 and Node 22.19+; the standalone consumer checks additionally need
the declared .NET 10 and .NET 11 RC2 SDKs/packages. Setup may restore packages
online. Retrieval uses unusable proxies, demonstrating proxy independence rather
than network isolation. No external database or account is needed.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-3101a2fc'
& ./eng/Test-Smoke.ps1 -PackageVersion '0.0.0-ef-models'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('ef.configure-nested-scalar','ef.configure-primitive-sequence') `
  -Query 'ef-models' -PackageVersion '0.0.0-ef-models' `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory ./artifacts/ef-models/inspection
```

Despite the helper's historical name, its recipe/query parameters reuse the same
packed stdio path for these EF recipes. Expect both IDs/titles/goals, explicit
11.0 editions, full expanded original .NET 10/11 projects, applicability and two
current compile-and-run records per recipe. EstuaryMarkers configures the nested
`marker_code`; KilnReadings distinguishes primitive JSON sequences from a binary
scalar. The inspection asserts full canonical body equality and matching
applicability; it does not rerun SQLite. The sample records establish real Windows
x64 SQLite model/schema/round-trips using EF 10.0.13 and EF11 RC2. Other providers,
native platforms, production data conversion and actual GA remain unverified.

## Package identity migration and restore sources

From PowerShell 7 with SDK 10.0.402 and Node 22.19+, this builds/packs the
ordinary tool and reuses the isolated local-feed/cache Inspector harness to call
that exact installed artifact from an unrelated temporary directory. Inspector
2.11.0 is installed locally if absent. Setup may use the network; retrieval uses
unusable proxies, demonstrating proxy independence rather than network isolation.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-fa570270'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('nuget.migrate-package-identities','nuget.restore-from-declared-sources') `
  -Query 'nuget-migration' -PackageVersion '0.0.0-nuget-migration' `
  -Dotnet 'C:\Program Files\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' -BuildAndPack `
  -OutputDirectory ./artifacts/nuget-migration/inspection
```

Expect both discovery IDs/titles/goals, explicit 11.0 editions, full original
.NET 10/11 canonical projects/code, applicability and four current compilation/
execution records per recipe. The helper's historical name does not restrict its
recipe parameters. Identity guidance distinguishes legacy-invalid punctuation
from NU5052 and proves direct/transitive migration. Restore guidance records
selected and executed versions for direct/transitive, source/cache/failure cases.
`eng/Test-NuGetMigration.ps1` reproduces the paired local-feed behavior using the
declared SDK/runtime installations; its parameters accept alternate absolute paths.
No public feed operation, other OS execution or actual GA verification is claimed.
Sample SDKs/packages remain author-maintained under the dependency policy.

## EF query adapters and UInt128 parameters

This checked example starts from an unrelated directory, selects this worktree,
builds/packs the ordinary tool and uses the existing isolated feed/cache harness.
It displays ID/title/goal, complete canonical Markdown, applicability and current
sample evidence through real installed stdio list/get. Requires PowerShell 7,
SDK 10.0.402 and Node 22.19+; the separate EF1004 reproduction additionally needs
SDK 11.0.100-rc.2.26504.105. Setup can restore online. Retrieval uses unusable
proxies, demonstrating proxy independence rather than network isolation.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-5b34b81d'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('ef.stream-query-without-reexecution','sqlite.bind-large-unsigned-value') `
  -Query 'data-adapters' -PackageVersion '0.0.0-data-adapters' -BuildAndPack `
  -Dotnet 'C:\Program Files\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory ./artifacts/data-adapters/inspection11
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId 'sqlite.bind-large-unsigned-value' -TargetVersion '10.0' `
  -Query 'data-adapters' -PackageVersion '0.0.0-data-adapters' `
  -Dotnet 'C:\Program Files\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory ./artifacts/data-adapters/inspection10
```

Expect both stable IDs at edition 11.0 and the conditional direct-Sqlite11 route
on target 10.0; no EF11-on-.NET10 route is advertised. Five saved real consumers
cover EF command/disposal and UInt128 padded TEXT/order checks. EF1004 is separately
reproduced by `eng/Test-EfQueryAdapter.ps1`. The EF body explains runtime11's
adapter fast path and SQLite's synchronous native I/O. The UInt128 body retains
explicit string reading/application parsing. Neither retrieval nor these Windows
x64 RC2 consumers establishes other native platforms/providers or actual GA.

## Embedded template-engine hosts

From an unrelated directory, this selects the exact PIN-1071 worktree, builds and
packs the ordinary tool, installs that artifact in a fresh local-only feed/cache,
then displays real stdio discovery ID/title/goal, full canonical Markdown,
applicability, edition and fresh evidence for both advertised targets. Requires
PowerShell 7, SDK 10.0.402 and Node; sample reproduction also needs SDK/runtime 11
RC2 and the public package feeds. Retrieval uses unusable proxies, proving proxy
independence rather than network isolation.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\agent-pin-1071-03-019-retarget-an-embedded-template-engine-host'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId 'sdk.retarget-template-engine-hosts' -Query 'template-engine' `
  -PackageVersion '0.0.0-template-host' -BuildAndPack `
  -Dotnet 'C:\Program Files\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory ./artifacts/template-host/inspection11
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId 'sdk.retarget-template-engine-hosts' -Query 'template-engine' `
  -TargetVersion '10.0' -PackageVersion '0.0.0-template-host' `
  -Dotnet 'C:\Program Files\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory ./artifacts/template-host/inspection10
```

Expect edition 11.0 on both targets, the conditional newer-package net10 route,
and four current compilation/execution records. The body distinguishes NU1701
implicit fallback from eight strict NU1202 failures, preserves the old netstandard
host library, and demonstrates generated net10 applications from both modern host
TFMs. `eng/Test-TemplateEngineHost.ps1` reproduces the real package/host/generated
consumer checks with absolute SDK parameters. Framework, other OS execution and
actual GA remain unverified; PIN-959/PIN-963 remain open. All new sample dependencies
are author-maintained, outside automated root-package update permissions.

## SQLite target and retained dependency

This checked block starts from an unrelated directory, selects the assigned
worktree, builds/packs the ordinary tool and reuses the existing isolated
local-feed/cache harness. Setup needs PowerShell7, SDK10.0.402 and Node22.19+;
Inspector2.11.0 is installed locally if absent. Setup can restore online;
retrieval uses unusable proxies (proxy independence, not network isolation).

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\agent-pin-863-05-021-keep-low-level-sqlite-access-on-a-supported-framework'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId 'sqlite.retain-supported-runtime' -TargetVersion '11.0' `
  -Query 'sqlite-runtime' -PackageVersion '0.0.0-sqlite-runtime' -BuildAndPack `
  -Dotnet 'C:\Program Files\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory ./artifacts/sqlite-runtime/inspection11
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId 'sqlite.retain-supported-runtime' -TargetVersion '10.0' `
  -Query 'sqlite-runtime' -PackageVersion '0.0.0-sqlite-runtime' `
  -Dotnet 'C:\Program Files\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory ./artifacts/sqlite-runtime/inspection10
```

Expect the ID/title/goal, retained edition11.0 on both explicit targets, full
canonical projects and Markdown, exact applicability and five current evidence
records. Target10 includes the conditional Sqlite11 route and retained Sqlite10
comparison. Sqlite11 uses net10.0 assets; EF11 still requires net11.0. The net48
alternative has real Windows Framework EXE execution, not a selectable catalogue
target. No UWP/classic Xamarin native execution or actual-GA proof is claimed.
`eng/Record-SqliteRuntime.ps1` separately recaptures the four real consumers with
the declared SDK10/SDK11 and Windows Framework runtime. Retrieval does not rerun SQL.

## Test and extension JSON dependencies

This verified block selects the assigned worktree from an unrelated directory,
builds/packs the ordinary tool, installs that exact package in an isolated local
feed/cache and calls real stdio list/get. Setup requires PowerShell7,
SDK10.0.402 and Node22.19+; the helper installs Inspector2.11.0 locally if absent.
Restores/setup use public packages; retrieval uses unusable proxies (proxy
independence, not network isolation).

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\agent-pin-1072-03-020-own-json-dependencies-in-test-projects-and-extensions'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId 'testing.declare-json-dependencies' -TargetVersion '11.0' `
  -Query 'json-dependencies' -PackageVersion '0.0.0-json-dependencies' -BuildAndPack `
  -Dotnet 'C:\Program Files\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory ./artifacts/json-dependencies/inspection11
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId 'testing.declare-json-dependencies' -TargetVersion '10.0' `
  -Query 'json-dependencies' -PackageVersion '0.0.0-json-dependencies' `
  -Dotnet 'C:\Program Files\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory ./artifacts/json-dependencies/inspection10
```

Expect ID/title/goal, edition11.0 on both explicit targets, full original
Markdown/canonical snippets, the conditional newer-package target10 route and
seven current compilation/execution records. The body distinguishes old
transitive JSON, explicit runtime ownership, actual adapter/collector loading,
missing-direct/runtime failures, SDK-root copies and VSTest/MTP selection.
`eng/Test-JsonDependencies.ps1 -Record` separately captures the actual managed
consumer checks with its explicit SDK10/SDK11 parameters. Native CodeCoverage,
other operating systems and actual GA remain unverified; PIN-959/PIN-963 stay open.
The seven sample projects and their exact locks remain author-maintained under
the existing sample dependency policy; no new automatic update permission is added.

## Retired SQLite native bundle initialization

This block was executed from an unrelated directory. It builds/packs the ordinary
tool and reuses the isolated local-feed/cache harness for real installed stdio
list/get. Requires PowerShell7, SDK10.0.402 and Node22.19+; Inspector2.11.0 is
installed locally if absent. Setup may restore online. Retrieval uses unusable
proxies, proving proxy independence rather than network isolation.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\agent-pin-864-05-022-restore-native-sqlite-initialization-after-bundle'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId 'sqlite.replace-retired-native-bundle' -TargetVersion '11.0' `
  -Query 'sqlite-initialization' -PackageVersion '0.0.0-sqlite-initialization' -BuildAndPack `
  -Dotnet 'C:\Program Files\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory ./artifacts/sqlite-initialization/inspection
```

Expect ID/title/goal, edition11.0, full canonical .NET10/.NET11 files and prose,
Windows system-engine applicability and three current sample evidence records.
The older bundle comparison is not an additional .NET10 foundation or selectable
current target route. `eng/Record-SqliteInitialization.ps1` separately recaptures
actual Windows x64 native initialization, committed debit/reopen, password refusal
and omitted-initialization failure using SDK10.0.402 and SDK11 RC2. That helper
records the loaded DLL/version/hash; retrieval does not execute SQL. Encryption,
other platforms and actual GA remain unverified. No paid provider is selected.

## Protect data into owned storage

This checked block starts from an unrelated directory, selects the PIN-1171
worktree, builds/packs the ordinary tool and installs that exact artifact in the
existing isolated local feed/cache. It calls real stdio list/get and displays the
ID/title/goal, full canonical Markdown, explicit target/edition, applicability and
fresh evidence. Setup requires PowerShell7, SDK10.0.402 and Node22.19+; paired
sample reproduction also requires SDK11.0.100-rc.2.26504.105 at its declared path.
Setup may restore online; retrieval uses unusable proxies, demonstrating proxy
independence rather than network isolation. Inspector2.11.0 is installed locally.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\agent-pin-1171-04-096-protect-data-directly-into-an-owned-buffer'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId 'security.protect-into-buffer' -TargetVersion '11.0' `
  -Query 'owned-buffer' -PackageVersion '0.0.0-museum-buffer' -BuildAndPack `
  -Dotnet 'C:/Program Files/dotnet/dotnet.exe' `
  -Node 'C:/Program Files/nodejs/node.exe' `
  -OutputDirectory ./artifacts/museum-buffer/inspection
```

Expect edition11.0, two current real compilation/execution records, complete
original .NET10 array/.NET11 span/fallback source and advanced AAD checks. The
actual Windows x64 framework CNG key encryptor handles token/empty/1MiB recovery;
wrong purpose/key ring, tampering and changed AAD reject and clear failed output.
Target10 has no applicable span edition; the old array comparison remains in the
11.0 body. No universal allocation count, other backend/platform or actual-GA
claim is made. PIN-959/PIN-964 and existing Human gates remain open. Samples stay
author-maintained under the existing dependency boundary.

## SQLite time components

This block was checked starting from an unrelated directory. It selects the exact
assigned worktree, builds/packs the ordinary tool, then uses the existing isolated
local-feed/cache harness for real installed stdio list/get. Requires PowerShell7,
SDK10.0.402 and Node22.19+; Inspector2.11.0 installs locally if absent. Setup may
restore online. Retrieval uses unusable proxies, proving proxy independence rather
than network isolation. Separate sample execution also needs SDK/runtime11 RC2.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\agent-pin-1210-05-069-filter-sqlite-rows-by-time-components'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId 'sqlite.query-time-components' -TargetVersion '11.0' `
  -Query 'time-components' -PackageVersion '0.0.0-sqlite-components' -BuildAndPack `
  -Dotnet 'C:/Program Files/dotnet/dotnet.exe' `
  -Node 'C:/Program Files/nodejs/node.exe' `
  -OutputDirectory ./artifacts/sqlite-components/inspection
```

Expect ID/title/goal, explicit target11/edition11, complete canonical EF10/EF11
Markdown, applicability and two current compilation/execution records. The older
parameterized SQL remains server-side; the current predicates execute `%H`/`%M`/`%S`
through SQLite strftime. Native versions3.53.3/3.53.4, stored fractions/boundaries,
exact results and translation failures are captured by the separate HeronSignals
sample records. Inspection does not rerun those databases. No EF11-on-net10,
additional foundation, arbitrary precision/timezone, other-platform or actual-GA
claim is added; PIN-959/PIN-965 and existing Human gates remain open.

## Supply an application-specific cross-origin verdict

This checked block starts from an unrelated directory, selects the exact assigned
worktree, builds/packs the ordinary tool and installs that artifact in the existing
isolated local feed/cache. Real stdio list/get displays discovery ID/title/goal,
full canonical Markdown, explicit edition/target, applicability and fresh evidence.
Setup requires PowerShell7, SDK10.0.402 and Node22.19+; Inspector2.11.0 installs
locally when absent. Setup can restore online; retrieval uses unusable proxies
(proxy independence, not network isolation). Browser/paired consumer reproduction
additionally needs the declared SDK11 RC2 and Chrome; retrieval does not run them.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\agent-pin-1172-04-097-supply-an-application-specific-cross-origin-verdict'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId 'web.supply-origin-protection-policy' -TargetVersion '11.0' `
  -Query 'seed-exchange' -PackageVersion '0.0.0-seed-exchange' -BuildAndPack `
  -Dotnet 'C:/Program Files/dotnet/dotnet.exe' `
  -Node 'C:/Program Files/nodejs/node.exe' `
  -OutputDirectory ./artifacts/seed-exchange/inspection
```

Expect edition11.0, complete original .NET10 token+origin/.NET11 custom/default/token
source, exact applicability and two current compile-and-run records. The helper
asserts canonical body and structured/JSON-text equality. The custom rule trusts
operator-owned exact origins independently of CORS reads; JSON and token-required
clients remain separate. `eng/Record-SeedExchange.ps1` requires explicit absolute
`-Net10Dotnet` and `-Net11Dotnet` executable paths and recaptures actual matching
consumer/HTTP proof; the existing form evidence document retains actual Chrome156
loopback controls. No net10 ICsrfProtection route is advertised: explicit target10
discovery has no matching edition and get returns an actionable no-applicable-edition
error. No production authentication, other browser/OS, proxy or actual-GA claim is
made; PIN-959/PIN-964 and existing Human gates remain open. These isolated samples
remain author-maintained; no additional automatic dependency permission is granted.

## Inspect a response directly

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-4d84c6cb'
$response = Get-Content -LiteralPath './artifacts/inspection/blazor.configure-browser-from-server.json' -Raw | ConvertFrom-Json
$recipe = $response.result.structuredContent.recipe
$recipe.markdown
$recipe.applicability | ConvertTo-Json -Depth 40
$recipe.evidence | Select-Object sample, kind, target, sdk, freshness | Format-Table
$recipe.humanProof | Format-List
```

Discovery is a compact selection summary: primary/optional routes, version conditions, restrictions, notes, opt-ins and stability warnings. `get_recipe` supplies full reasons, prerequisites, constraints, sources and evidence. Missing summary fields do not mean unrestricted compatibility. Requirements describe the consumer; exact SDK/package/toolchain pins describe recorded verification, not mandatory upgrades.

Both tools require `targetVersion` independently. Inspect the application's target frameworks and project/`global.json` context first. Supported aliases include `10`, `10.0`, `net10.0`, `11`, `11.0` and `net11.0`. Deliberate `latest` is for exploration/unknown targets; it selects the newest installed catalogue release and proves no consumer compatibility. The helper preserves the target as a JSON string.

For a direct Inspector call, use the same absolute Node, Inspector entry point and .NET/server paths established by the helper. For an installed artifact, substitute its absolute `playbook.exe mcp` command for `dotnet <server.dll> mcp`; see [local package installation](contributing.md#inspect-the-development-package). Direct Node launch avoids the Windows npx `inspector is not recognized` wrapper failure. Inspector exits nonzero for a tool error; do not treat an empty response as success.

## Choose further checks

Use the [contribution policy](contributing.md#choose-the-check-and-retain-its-result) for capture/refresh and retention. Inspection verifies delivery, not browser/provider/native execution or release readiness. Saved `freshness: current` means inputs matched at package build time; it does not inspect the consumer or current upstream APIs. Pending Human proof stays visible separately from compilation.

`eng/Test-Content.ps1`, `eng/Test-Evidence.ps1` and their `-Pack` variants exercise content and freshness rejection. The existing installed-package scenarios cover filter combinations, paging, release selection, complete structured/text equality, recovery and errors. Run those when changing the corresponding contract; do not copy their test catalogue into a ticket walkthrough or repeat a platform matrix for a link edit.

## Parsed API explorer route contracts

This tested block builds/packs the ordinary tool and installs that exact artifact
through the existing isolated local-feed/cache harness. Real installed stdio
list/get runs from an unrelated temporary directory and displays ID/title/goal,
complete canonical Markdown, target/edition, applicability and both current
compilation/execution records. Setup requires PowerShell7, SDK10.0.402 and
Node22.19+ and can restore online. Retrieval uses unusable proxies: this proves
proxy independence, not network isolation. No global tool or PATH change occurs.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\agent-pin-1173-04-098-read-parsed-route-contracts-in-an-api-explorer'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId 'web.inspect-api-route-pattern' -Query 'route-pattern' `
  -TargetVersion '11.0' -PackageVersion '0.0.0-pin1173' -BuildAndPack `
  -Dotnet 'C:\Program Files\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory ./artifacts/pin1173/inspection
```

Expect "Read parsed route contracts in an API explorer extension", edition11.0,
target11.0, complete .NET10/11 MarineRoutes files and fresh evidence. Each actual
consumer discovers eight MVC/Minimal routes and checks constraints/defaults/
optional/catch-all, nullable/legacy controls and 18 real HTTP outcomes. .NET10
retains bounded source-template parsing; the new parsed property is unavailable
there. Other providers can leave it null. No native/browser matrix, universal
provider support or actual-GA claim; PIN-959/PIN-964 and Human gates stay open.

## Registration confirmation policy

This builds/packs the ordinary tool and installs that exact artifact through the
existing isolated local feed/cache. Real installed stdio list/get from an unrelated
directory display ID/title/goal, complete canonical Markdown, target11/edition11,
applicability and current paired compile/execution evidence. Target10 correctly has
no applicable edition; its working original pages are an executed comparison in
edition11, preserving exactly eight foundations. Retrieval uses unusable proxies:
proxy independence, not network isolation.

The block was executed from an unrelated directory. Setup needs PowerShell7,
SDK10.0.402 and Node22.19+; the final reconstruction also needs SDK11 RC2 and the
matching restored locked packages. Online package setup precedes offline retrieval.
No global tool, production account, messages or third-party provider is required.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\agent-pin-1177-04-102-keep-registration-aligned-with-confirmation-policy'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId 'identity.confirm-before-registration-signin' -Query 'confirmation' `
  -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin1177' -BuildAndPack `
  -Dotnet 'C:/Program Files/dotnet/dotnet.exe' `
  -Node 'C:/Program Files/nodejs/node.exe' `
  -OutputDirectory ./artifacts/pin1177/inspection
& ./eng/Test-AquariumVolunteersConsumer.ps1 `
  -Dotnet 'C:/Program Files/dotnet/dotnet.exe' `
  -Net11Dotnet 'C:/Users/pingu/.dotnet-playbook-11/dotnet.exe'
```

Expect `identity.confirm-before-registration-signin`, title "Keep registration
aligned with confirmation policy" and goal "Require the configured email, phone
or account confirmation before granting a new registrant a session, across
password and external registration." Both original full projects, lock files and
numbered migration steps must be returned without unresolved snippet tokens.
The final helper recreates only returned files in an unrelated temporary directory,
builds both exact closures without remote feeds and runs 32 actual new-registration
checks plus later confirmation/sign-in and antiforgery/provider/return controls.
It observes the .NET10.0.13 packaged email/phone bypass honestly; the original
CanSignInAsync-gated pages and .NET11 packaged UI defer sessions correctly.
Controlled local external authentication is not third-party interoperability;
RC2 and local SQLite/HTTP evidence are not production, browser or actual-GA proof.

## Structured assembly-conflict and server events

From an unrelated directory, run the existing installed-package Inspector with the
exact SDK10 executable (the SDK11 fixture is a separate authoring prerequisite):

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\agent-pin-1121-03-026-consume-structured-assembly-conflict-and-server'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId 'msbuild.inspect-structured-build-events' -Query 'structured assembly' `
  -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin1121' -BuildAndPack `
  -Dotnet 'C:/Program Files/dotnet/dotnet.exe' `
  -Node 'C:/Program Files/nodejs/node.exe' `
  -OutputDirectory ./artifacts/pin1121/inspection
```

Expect ID `msbuild.inspect-structured-build-events`, title "Consume structured
assembly-conflict and server events", its explanatory goal, edition11.0 and target11.0,
complete canonical Markdown, applicability and current SDK10/11 sample evidence.
The helper builds/packs the ordinary tool, installs that exact artifact through a
local-only feed/fresh cache, then runs real stdio list/get from an unrelated temp
directory with unusable proxies. This proves proxy-independent offline retrieval,
not network isolation. No target10 typed route, unsupported server embedding,
short-lived experimental server execution, universal reuse/performance or actual-GA
claim is made. PIN-959/PIN-963/Human gates remain open.

## NuGet staging uploads

This builds/packs the ordinary tool, installs that exact package through an
isolated local feed/cache and calls real stdio list/get from an unrelated
directory. It prints ID/title/goal, the complete canonical Markdown, target11 /
edition11, applicability and fresh paired evidence. The target10 request must
return no_applicable_edition: the older valid ordinary-publication comparison
stays in the target11 body and is not staging support.

The block was executed from an unrelated directory. Refresh paired evidence with
`eng/Record-FoundryStaging.ps1` on both complete pinned Windows x64 SDK/runtime/ref
installations when sample inputs change. Inspection needs PowerShell7, SDK10.0.402
and Node22.19+. Setup may download tool/test dependencies;
retrieval uses unusable proxies, establishing proxy independence, not network
isolation. The evidence fixture uses only disposable literal127.0.0.1 HTTP and a
generated noncredential marker. No public package upload or real API key is needed.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\agent-pin-1124-03-029-upload-packages-to-a-v3-staging-endpoint'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId 'nuget.stage-package-uploads' -Query 'staging' `
  -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin1124' -BuildAndPack `
  -Dotnet 'C:\Program Files\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory ./artifacts/pin1124/inspection
```

Expect title "Upload packages to a V3 staging endpoint" and goal "Stage a package
and its symbols through a service-advertised endpoint when a publisher needs an
upload step before final publication, with explicit group, transport and failure
handling." Saved paired proof records original SDK pack/PDB validation and real
wire checks. Current staging verifies advertised endpoint/PUT/chunked multipart bytes,
original filenames, route/query, group present/null/empty and key present/absent
without printing its value, HTTP201/400, missing supported resource, default HTTP
refusal, same-origin authorization with default/fixture flags, loopback-only insecure opt-in, received cancellation and request timeout. URI-only policy checks are separate from HTTPS transport proof; malformed header/chunk/length connections also close with complete owned cleanup.
SDK10 verifies actual ordinary publication, fixed package.nupkg filename and its
HTTP400 failure, with no staging API/group equivalent. The fixture stops its own
listener/clients and removes temporary packages. Any nonzero exit, stale evidence,
missing recipe, wrong applicability or unexpanded snippet is a failure.

This qualifies the pinned SDK-bundled public API and faithful local protocol only.
No independently distributed NuGet.Protocol8 package, older-target staging route,
hosted service interoperability, TLS/certificate flow, final publication, atomicity
or GA is claimed. Exact eight foundations and PIN-959/PIN-963/Human gates stay open.

## Reused NuGet hosts, analyzer inventory and launch diagnostics

The following block builds/packs one ordinary artifact and inspects that exact
installed local-feed/cache artifact for all three recipes. Real stdio list/get
from an unrelated directory prints each ID/title/goal, complete canonical
Markdown, target11/edition11, applicability and fresh paired evidence; target10
gets `no_applicable_edition`. The final consumer reconstructs the default
net10/net11 projects and sources from returned Markdown, supplies standard SDK,
import and local-feed scaffolding, then runs actual same-process
MSBuild/configuration and benign-child behaviour outside the checkout.

Prerequisites: PowerShell7, Node22.19+, complete SDK10.0.402 and
SDK11.0.100-rc.2.26504.105 installations, runtime/ref10.0.13 and11RC2. Setup may
restore online; installed retrieval with unusable proxies proves proxy
independence, not network isolation. No global install, provider or real secret.
From an unrelated directory this literal worktree block was tested:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\agent-pin-1125-03-030-invalidate-custom-nuget-caches-between-reused-builds'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('nuget.reset-reused-host-state','nuget.inspect-analyzer-assets','nuget.own-plugin-launch-diagnostics') `
  -Query 'nuget' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin1125-batch03' -BuildAndPack `
  -Dotnet 'C:\Program Files\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory ./artifacts/batch03/inspection
& ./eng/Test-PackageWorkshopConsumer.ps1 `
  -Net10Dotnet 'C:\Program Files\dotnet\dotnet.exe' `
  -Net11Dotnet 'C:\Users\pingu\.dotnet-playbook-11\dotnet.exe'
```

Expect all three discovery entries, full sources/requirements and current
evidence, then the returned-consumer PASS. The complete negative/lifecycle/asset
suite is separately reproducible with `eng/Record-PackageWorkshop.ps1` using the
same two absolute SDK parameters. It measures real same-host stale/reset/lazy
behaviour and owned open-file/unsubscription/failure controls; actual original
analyzer pack/locked restore/old-current-mixed compiler paths and diagnostic
failures; visible current CS0618 versus zero-warning fixed consumers, and real
missing/valid benign OS child launches with redaction. Expected negative compiler
errors and deliberate task failure are asserted passing controls, not suppressed
production warnings. The pinned SDK does not consume AnalyzerAssets: `_._` is
not enforcement. No third-party automatic reset, provider credential exchange,
other-platform, performance or GA guarantee is made. Eight approved foundations,
PIN-959 qualifying coverage/evidence, PIN-963 actual-GA and Human gates remain open.

## Verify withdrawn compiler guidance

Requires PowerShell 7, SDK 10.0.402 and Node 22.19+. This builds/packs the ordinary
tool and installs the exact artifact from an isolated local-only feed/cache.
Setup can restore online. Installed stdio calls run from an unrelated temporary
directory with unusable proxies (proxy independence, not network isolation).

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-5ba443cc'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId 'razor.replace-runtime-compilation-registration' `
  -RemovedRecipeId @('razor.migrate-generated-document-contracts','razor.copy-intermediate-nodes','razor.choose-task-generation-path') `
  -Query '' -TargetVersion '11.0' -PackageVersion '0.0.0-scope-removal' `
  -Dotnet 'C:\Program Files\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' -BuildAndPack `
  -OutputDirectory ./artifacts/scope-removal/inspection
```

Expect three `PASS` lines confirming each removed ID is absent from discovery and
returns `unknown_recipe` on a direct request. The retained recipe prints its ID,
title, goal, full expanded Markdown and applicability, with current sample evidence
and equal structured/text responses. A remaining ID, any other error or stale/
changed retained content fails the check. No new compiler-consumer or GA claim follows.

## Registry challenges and native publish hooks

Requires PowerShell7, SDK10.0.402 and Node22.19+. This builds/packs the ordinary
tool, installs that exact artifact through a local-only feed and fresh cache,
then exercises real stdio list/get from an unrelated temporary directory with
unusable proxies. Setup can restore online; retrieval proves proxy independence,
not network isolation. The block was tested from an unrelated starting directory.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-d3fc93fb'
& ./eng/Test-Smoke.ps1 -Dotnet 'C:\Program Files\dotnet\dotnet.exe' `
  -PackageVersion '0.0.0-pin1142-batch04'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('sdk.configure-redirected-output-encoding','containers.migrate-registry-auth-challenges','sdk.migrate-optimized-publish-hooks') `
  -Query 'estuary' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin1142-batch04' `
  -Dotnet 'C:\Program Files\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory ./artifacts/estuary/inspection-final
```

Expect all three IDs, titles and explanatory goals; full canonical Markdown;
target11/edition11 applicability; and current paired compilation/execution
evidence. Target10 requests must return `no_applicable_edition`: the valid older
examples remain comparisons inside each target11 edition. A missing recipe,
unexpanded snippet, stale evidence or changed structured/text response fails.

The registry proof uses original workers, real Distribution3.1.2 TLS/token
exchanges and image push/run, seven SDK11 refusals before token requests/writes,
private child trust and an internal SDK network. Native proof uses actual Windows
x64 ILC/MSVC-link/PE execution and matched binlogs for old/current legacy versus
migrated hooks, default/false/true, NoBuild refresh/repeat and missing/wrong
sidecars. Refresh changed inputs with `eng/Test-EstuaryRegistry.ps1 -Record` or
`eng/Test-EstuaryWindows.ps1 -Record` on their declared authoring environments.
Inspection itself consumes saved evidence; it does not repeat those native/service
checks. Beta/RC2 limits, eight foundations and comprehensive/GA/Human gates remain.
Encoding proof captures actual paired SDK build diagnostics through pipes under
owned private CP1252/65001 consoles. An explicit UTF-8 reader preserves text and
real failed-build exits; the proposed and source-level opt-outs retain UTF-8 on
these artifacts and are disclosed as unavailable. No legacy producer encoding
route or newer-only reader API is claimed.
