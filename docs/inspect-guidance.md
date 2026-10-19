# Inspect the guidance an agent receives

`playbook mcp` speaks the MCP protocol over stdin/stdout; it is not a PowerShell prompt. Use [MCP Inspector](https://github.com/modelcontextprotocol/inspector/tree/main/clients/cli) as a client to initialize the connection and call its tools. This exercises the real server and its generated, embedded content.

Requires PowerShell 7, the pinned .NET SDK 10.0.402, and Node 22.19 or later for Inspector. The first `npx` call downloads Inspector; Playbook itself does not fetch guidance. Reading this recipe does not require the .NET 9 SDK or any workloads.

From this worktree:

```powershell
Set-Location V:\_worktrees\Playbook\t3-33d73aa8
dotnet build ./src/PinguApps.Playbook -c Release
$server = (Resolve-Path ./src/PinguApps.Playbook/bin/Release/net10.0/PinguApps.Playbook.dll).Path
$inspector = @('--yes', '@modelcontextprotocol/inspector@2.11.0', '--cli', '--format', 'json', '--quiet')

# Discover the protocol tools: list_recipes and get_recipe.
npx @inspector --method tools/list -- dotnet $server mcp

# List the recipes declared for .NET 10.
$catalogue = npx @inspector --method tools/call --tool-name list_recipes --tool-arg 'targetVersion="10.0"' -- dotnet $server mcp | ConvertFrom-Json
$catalogue.result.structuredContent.recipes | Format-List id, title, goal
$catalogue.result.structuredContent.recipes.applicability | ConvertTo-Json -Depth 10

# Retrieve the complete recipe, then display its Markdown and constraints.
$response = npx @inspector --method tools/call --tool-name get_recipe --tool-arg id=csharp.validate-property-assignment --tool-arg 'targetVersion="10.0"' -- dotnet $server mcp | ConvertFrom-Json
$recipe = $response.result.structuredContent.recipe
$recipe.markdown | Show-Markdown
$recipe.applicability.routes | Format-List
$recipe.sources | Format-Table
$recipe.evidence | Format-List
$recipe.humanProof | Format-List
```

Expect one recipe, `csharp.validate-property-assignment`, titled **Normalize and validate properties with the C# field keyword**. Its `goal` explains when to use the feature: property setters that normalize or validate assignments without a separate backing-field declaration. The warehouse domain appears in the worked example; discovery describes the reusable capability.

Discovery applicability is now a compact selection summary: separate primary/optional routes, version conditions without reasons, essential restrictions, `selectionNotes`, opt-ins and stability warnings. It omits full prerequisites/constraints, source URLs and evidence links. Those fields remain in `get_recipe`; retrieve it and check the complete conditions before applying guidance. Missing fields in a summary do not mean unrestricted compatibility. See [PIN-974 measurements, budgets and the tested example](verification/PIN-974.md). This response-shape change is classified `major` under the existing compatibility policy.

Its Markdown includes both original implementations, migration steps, recursion/initialization constraints and evidence. Applicability has explicit `routes`: the primary .NET 10 / C# 14 requirements, plus an `optional-alternative` using the .NET 11 SDK while retaining `net10.0` and C# 14. Each route's `requirements` contains runtime, SDK, language, framework, package/asset, workload, provider, platform, rendering, prerequisite and constraint fields. Version conditions distinguish a `release-family` from a necessary `exact` pin. `sourceUrls` justify conditions; `evidenceSamples` links exact test records separately. These are requirements to check, not claims that Playbook inspected the consumer. `$response` retains the complete structured result and matching JSON text fallback; omit `Show-Markdown` to inspect raw Markdown.

Use [the PIN-972 example](verification/PIN-972.md#inspect-what-an-agent-receives) to expand the requirements next to exact SDK/language/runtime-output/dependency/workload evidence. SDK `10.0.402` and `11.0.100-rc.2.26504.105` are reproducibility baselines, not mandatory exact installations. Primary and optional routes are separate choices; RC execution is not GA verification.

Inspector initializes and closes each connection automatically. Both tools require `targetVersion` on every call, independently of earlier calls. Inspect the application's `TargetFramework`/`TargetFrameworks`, project and `global.json` context first; an SDK build is not the target release. Accepted aliases are `10` / `10.0` / `net10.0` and `11` / `11.0` / `net11.0`, plus deliberate `latest` for exploratory/theoretical guidance or an unknown target. `latest` selects the newest release in the installed catalogue and does not establish consumer compatibility. The inner double quotes in `'targetVersion="10.0"'` preserve a JSON string rather than Inspector converting `10.0` to a number. Use the stable recipe ID instead of a numeric ID. See the [companion instruction](agent-instructions.md).

To use an installed development package instead of the built DLL, replace `dotnet $server mcp` with `./artifacts/manual-tool/playbook.exe mcp` after following [the local install instructions](contributing.md#inspect-the-development-package). An installed global tool can be launched as `playbook mcp` in the same place. Each command is short-lived and shuts the server down; nothing needs stopping afterwards.

## Inspect combined filters and paging

After the build/setup above, run:

```powershell
$filtered = npx @inspector --method tools/call --tool-name list_recipes --tool-arg 'targetVersion="10.0"' --tool-arg 'areas=["csharp"]' --tool-arg 'languages=["csharp"]' --tool-arg 'query=VALIDATION field' --tool-arg includeShared=false --tool-arg offset=0 --tool-arg limit=1 -- dotnet $server mcp | ConvertFrom-Json
$filtered.result.structuredContent | Format-List resolvedTarget, total, offset, limit, nextOffset, supportedAreas, supportedExampleLanguages
$filtered.result.structuredContent.recipes | Format-List id, title, goal, languageNeutral, applicability

# A valid combination with no keyword match succeeds empty.
$empty = npx @inspector --method tools/call --tool-name list_recipes --tool-arg 'targetVersion="10.0"' --tool-arg 'areas=["csharp"]' --tool-arg query=unmatched -- dotnet $server mcp | ConvertFrom-Json
$empty.result.structuredContent | Format-List total, nextOffset, recipes

# An invalid identifier is a tool error, including corrections; it is not ignored.
$invalidLines = @(npx @inspector --method tools/call --tool-name list_recipes --tool-arg 'targetVersion="10.0"' --tool-arg 'areas=["blazro"]' -- dotnet $server mcp)
$invalid = $invalidLines[0] | ConvertFrom-Json
$invalid.result.content[0].text | ConvertFrom-Json | Format-List code, filter, invalidValues, supportedValues
```

Expect the property recipe, `total=1`, `offset=0`, `limit=1`, `nextOffset=null` and `languageNeutral=false`. Both keywords match, and the explicitly selected .NET 10 applicability remains visible. The empty query result has `total=0`, no recipes and no next offset. The typo produces `unknown_area`, `filter=areas`, rejected `blazro`, and supported `csharp`. Inspector exits nonzero (5 in this verification) for that intentional error (see the native-command preference note below).

The ordinary catalogue currently contains only this C# goal, so it cannot demonstrate a shared framework mapping, F#-applicable neutral configuration or multiple pages. The separate unpublished contract fixture and installed-package scenarios cover those cases, including all-term/title/goal/tag/API matching, OR within sets, AND between filters, shared inclusion disabled, unrelated Windows guidance excluded, C#-specific XML excluded from F#, and default/max-size traversal. Run the [verification commands](verification/PIN-971.md) for current evidence. Fixture metadata does not claim platform execution or additional shipped recipe coverage.

## Inspect release selection and failures

For an empty search, inspect `recovery` alongside the requested/resolved target and coverage. Its optional `suggestedRequest` is a complete `list_recipes` arguments object; executing it is the agent's choice. Keyword recovery removes keywords while preserving compatibility filters. Paging recovery resets only the offset. When the non-keyword combination is empty, there is no safe suggested retry. Nonempty discovery omits recovery. See the [tested empty-search and retry walkthrough](verification/PIN-975.md), including recovered ID/title/goal, full Markdown and applicability.

Continue in the same PowerShell window:

```powershell
# Explicit latest selects the newest release represented in the installed catalogue.
$latest = npx @inspector --method tools/call --tool-name list_recipes --tool-arg targetVersion=latest -- dotnet $server mcp | ConvertFrom-Json
$latest.result.structuredContent | Format-List requestedTarget, resolvedTarget, targetAssumed, supportedTargets, supportedAreas, supportedExampleLanguages
$latest.result.structuredContent.coverage | Format-Table target, scope, complete, description -Wrap

# An explicit alias selects the shared edition with only .NET 10 routes.
$retained = npx @inspector --method tools/call --tool-name get_recipe --tool-arg id=csharp.validate-property-assignment --tool-arg targetVersion=net10.0 -- dotnet $server mcp | ConvertFrom-Json
$retained.result.structuredContent.recipe | Format-List id, edition, introducedIn
$retained.result.structuredContent.recipe.applicability.routes | Format-List

# .NET 11 selects the same edition 10.0/introduction 10.0, with its own route.
$newer = npx @inspector --method tools/call --tool-name get_recipe --tool-arg id=csharp.validate-property-assignment --tool-arg targetVersion=net11.0 -- dotnet $server mcp | ConvertFrom-Json
$newer.result.structuredContent.recipe | Format-List id, edition, introducedIn
$newer.result.structuredContent.recipe.applicability.routes | ConvertTo-Json -Depth 10

# Detail can also deliberately request latest; it inherits nothing from discovery.
$latestDetail = npx @inspector --method tools/call --tool-name get_recipe --tool-arg id=csharp.validate-property-assignment --tool-arg targetVersion=latest -- dotnet $server mcp | ConvertFrom-Json
$latestDetail.result.structuredContent | Format-List requestedTarget, resolvedTarget, targetAssumed

# Missing, null and blank values all fail; Inspector exits nonzero for each expected error.
$previousNativePreference = $PSNativeCommandUseErrorActionPreference
try {
  $PSNativeCommandUseErrorActionPreference = $false
  foreach ($tool in @('list_recipes', 'get_recipe')) {
    foreach ($targetArgs in @(@(), @('--tool-arg', 'targetVersion=null'), @('--tool-arg', 'targetVersion=""'))) {
      $lines = @(npx @inspector --method tools/call --tool-name $tool --tool-arg id=csharp.validate-property-assignment @targetArgs -- dotnet $server mcp)
      ($lines[0] | ConvertFrom-Json).result.content[0].text | ConvertFrom-Json | Format-List code, message
    }
  }
}
finally { $PSNativeCommandUseErrorActionPreference = $previousNativePreference }

# Inspector exits nonzero on a tool error and emits a diagnostic JSON line on stderr.
# Read its first stdout line to inspect the MCP result and parseable JSON error body.
$failureLines = @(npx @inspector --method tools/call --tool-name get_recipe --tool-arg id=csharp.validate-property-assignment --tool-arg targetVersion=net10.0-windows -- dotnet $server mcp)
$failure = $failureLines[0] | ConvertFrom-Json
$failure.result | Format-List isError
$failure.result.content[0].text | ConvertFrom-Json | Format-List
```

Expect `requestedTarget=latest`, `resolvedTarget=11.0` and `targetAssumed=False` for explicit `latest` on both tools. Omitted, null and blank targets return `required_target` with a corrective message; none select a release. Concrete calls echo their original alias and resolved release. Coverage labels .NET 10 as `curated` and .NET 11 as `comprehensive`; **both are currently incomplete**. The latter is a launch commitment, not a claim of completed coverage. An empty result cannot establish that .NET 10 lacks a capability.

The last request must return `isError=True`, code `unsupported_target`, the exact rejected string and supported releases. It never strips the platform suffix. Patch and RC strings fail similarly. An unknown ID returns `unknown_recipe`. A known ID with no compatible edition returns `no_applicable_edition` with available edition labels; valid discovery with no matches succeeds empty. In a script with `$PSNativeCommandUseErrorActionPreference=$true`, temporarily set it to `$false` around the intentionally failing `npx` call.

The [verification script](contributing.md) also builds a **separate, unpublished contract fixture package** to exercise overlapping genuinely changed editions, a known .NET 11-only recipe and a pre-introduction conditional route. Fixture identities never appear in the ordinary package. Synthetic introduction metadata proves routing only; actual SDK/runtime compatibility comes from compiling and executing the original samples. .NET 9 remains comparison-only. The .NET 11 baseline is RC evidence, not GA verification; broader client and platform checks remain separate work.

Both property requests return edition `10.0`, introduction `10.0` and identical Markdown. Only applicability is narrowed to the requested target. See [PIN-973](verification/PIN-973.md) for the tested side-by-side discovery/Markdown/applicability example. No undeclared target becomes compatible.

## Inspect reusable verification evidence from the package

The [PIN-672 walkthrough](verification/PIN-672.md) installs the actual local package and calls both tools through Inspector. Detail includes the saved source commit/dirty marker, per-input SHA-256 hashes, exact target/SDK/language, commands, resolved dependencies, required workloads/native versions, authoring host, SDK information and actual output. `freshness: current` means the inputs matched when the package was built; installed use does not inspect the consumer or the current authoring checkout. `humanProof` keeps any targeted Human check visible independently of compilation. Neither freshness nor successful compilation establishes complete catalogue or product release readiness.
