# Inspect the guidance an agent receives

`playbook mcp` speaks protocol over stdin/stdout. Use MCP Inspector as a client; it initializes and closes each connection automatically. `eng/Inspect-Guidance.ps1` builds the real server, installs pinned Inspector 2.11.0 under ignored `artifacts/` if needed, calls both tools, and prints **ID/title/goal, full expanded Markdown and applicability**. Responses default to `artifacts/inspection/<id>.json`.

Requires PowerShell 7, the repository's .NET SDK 10.0.402 and Node 22.19+. Setup may restore/download packages online; guidance itself is embedded and offline. The helper invokes the resolved executables by absolute path without changing PATH or global tools. It disables .NET first-run PATH integration. No native workloads or account are required for server inspection.

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

## Live identity and generated-document tools

For the live identity and compiler recipes, this block builds/packs the ordinary tool and
uses the existing isolated local-feed/cache installation. It calls real stdio
list/get from an unrelated directory and prints discovery ID/title/goal, complete
canonical Markdown, applicability and fresh sample evidence. The walkthrough was
launched from an unrelated directory. Setup may restore/download packages;
retrieval uses unusable proxies, which establishes proxy independence rather
than network isolation.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-2784b7c8'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('signalr.refresh-live-credentials','blazor.react-to-refreshed-identity','razor.migrate-generated-document-contracts') `
  -Query '' -PackageVersion '0.0.0-live-tool-contracts' `
  -Dotnet 'V:\_worktrees\Playbook\t3-2784b7c8\artifacts\identity-tooling\pinned-dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' -BuildAndPack `
  -OutputDirectory ./artifacts/identity-tooling/inspection
```

Expect three explicit 11.0 editions, original .NET 10/RC2 consumers, the complete
TypeScript development source-build prerequisite, local identity/provider limits
and current compilation/execution records. `eng/Test-LiveIdentityConsumer.ps1`
reconstructs the .NET and TypeScript consumers solely from these installed
responses and executes their real hub and Interactive Server checks; supply
absolute `-Playwright`, `-Chromium`, `-Dotnet` and `-Node` paths. This is bounded
local execution, not Azure SignalR Service, production authentication or GA proof.
The compiler guidance distinguishes an unavailable older public reader,
diagnostic-tree import, declaration/stub labels and constructor source/ABI
compatibility. `eng/Record-CatalogueCompiler.ps1` executes its actual compiler
checks with `-Dotnet` selecting the installed root containing both pinned SDKs.
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

## Copy custom Razor nodes

This tested example selects the assigned worktree from an unrelated directory,
builds/packs the ordinary tool and uses the existing isolated local-feed/cache
harness. Requires PowerShell 7, SDK 10.0.402 and Node 22.19+. The paired pipeline
checks additionally require SDK 11.0.100-rc.2.26504.105 at its declared path.
Setup may restore online; installed retrieval uses unusable proxies, proving
proxy independence rather than network isolation.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\agent-pin-1167-04-092-copy-intermediate-razor-nodes-before-a-custom'
& ./eng/Record-MapCompiler.ps1
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId 'razor.copy-intermediate-nodes' -Query 'node-copy' `
  -TargetVersion '11.0' -PackageVersion '0.0.0-map-copy' -BuildAndPack `
  -Dotnet 'C:/Program Files/dotnet/dotnet.exe' `
  -Node 'C:/Program Files/nodejs/node.exe' `
  -OutputDirectory ./artifacts/map-compiler/inspection
```

Expect the discovery ID/title/goal, explicit 11.0 edition/target, complete
canonical .NET 10/11 Markdown, applicability and four current compilation/execution
records. The helper asserts complete snippet/body and structured/JSON-text equality.
The focused recorder runs real Razor generation and compiles/executes both maps:
`[coast:blue]<p>Estuary route</p>` and `[ridge:amber]<p>Ridge route</p>`, preserving
the original. It checks copied writable state, shared immutable values, shallow-copy
leakage and unsupported nodes; private current-clone/older-reader probes are
verification-only. No public generic clone, arbitrary-tree, MVC/browser, older-target
backport or actual-GA support is inferred. Sample pins remain author-maintained.

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

## Authored Razor task generation

This tested block builds/packs the ordinary tool, installs that exact artifact
through the existing isolated local feed/cache and calls actual stdio list/get
from an unrelated temporary directory. Setup requires PowerShell 7, SDK
10.0.402 and Node 22.19+; Inspector 2.11.0 is installed locally when absent.
Setup may restore packages online. Retrieval uses unusable proxies, which proves
proxy independence, not network isolation.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\agent-pin-1168-04-093-select-the-hosted-generator-in-an-authored-razor'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId 'razor.choose-task-generation-path' -TargetVersion '11.0' `
  -Query 'build-task' -PackageVersion '0.0.0-exhibit-compiler' `
  -Dotnet 'C:/Program Files/dotnet/dotnet.exe' `
  -Node 'C:/Program Files/nodejs/node.exe' -BuildAndPack `
  -OutputDirectory ./artifacts/exhibit-compiler/inspection
```

Expect the ID, reusable title and goal, explicit 11.0 edition, complete original
.NET 10/RC2 files, applicability and four current compilation/execution records.
Full Markdown must equal canonical snippets and structured/text responses must
match. `eng/Record-ExhibitCompiler.ps1` separately runs actual old/current task
consumers and failure controls with the pinned SDKs. Generated C# renders the
Lantern collection and encoded badge. Current omitted/false flags use the engine;
true hosts the source generator. The private underscored forwarding property is
not a stable application setting. Binlogs/full generated artifacts are retained
under ignored `artifacts/exhibit-compiler`. No older-target hosted, browser,
native, arbitrary IDE or actual-GA proof is claimed. The helper retains its
unique temporary local install/feed for inspection and preserves PATH/settings.

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
