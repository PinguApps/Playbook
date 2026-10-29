# Inspect the guidance an agent receives

`playbook mcp` speaks protocol over stdin/stdout. Use MCP Inspector as a client; it initializes and closes each connection automatically. `eng/Inspect-Guidance.ps1` builds the real server, installs pinned Inspector 2.11.0 under ignored `artifacts/` if needed, calls both tools, and prints **ID/title/goal, full expanded Markdown and applicability**. Responses default to `artifacts/inspection/<id>.json`.

Requires PowerShell 7, the repository's .NET SDK 10.0.402 and Node 22.19+. Setup may restore/download packages online; guidance itself is embedded and offline. The helper invokes the resolved executables by absolute path without changing PATH or global tools. It disables .NET first-run PATH integration. No native workloads or account are required for server inspection.

## Read a recipe

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

## Live identity refresh

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
  -Query 'live-identity' -PackageVersion '0.0.0-live-identity' `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' -BuildAndPack `
  -OutputDirectory ./artifacts/identity-tooling/inspection
```

Expect both explicit 11.0 editions, original .NET 10/RC2 consumers, the complete
TypeScript development source-build prerequisite, local identity/provider limits
and current compilation/execution records. `eng/Test-LiveIdentityConsumer.ps1`
reconstructs the .NET and TypeScript consumers solely from these installed
responses and executes their real hub and Interactive Server checks; supply
absolute `-Playwright`, `-Chromium`, `-Dotnet` and `-Node` paths. This is bounded
local execution, not Azure SignalR Service, production authentication or GA proof.

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
