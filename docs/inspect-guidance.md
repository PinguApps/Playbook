# Inspect the guidance an agent receives

`playbook mcp` speaks the MCP protocol over stdin/stdout; it is not a PowerShell prompt. Use [MCP Inspector](https://github.com/modelcontextprotocol/inspector/tree/main/clients/cli) as a client to initialize the connection and call its tools. This exercises the real server and its generated, embedded content.

Requires PowerShell 7, the pinned .NET SDK 10.0.402, and Node 22.19 or later for Inspector. The first `npx` call downloads Inspector; Playbook itself does not fetch guidance. Reading this recipe does not require the .NET 9 SDK or any workloads.

From this worktree:

```powershell
Set-Location V:\_worktrees\Playbook\t3-7406bc2b
dotnet build ./src/PinguApps.Playbook -c Release
$server = (Resolve-Path ./src/PinguApps.Playbook/bin/Release/net10.0/PinguApps.Playbook.dll).Path
$inspector = @('--yes', '@modelcontextprotocol/inspector@2.11.0', '--cli', '--format', 'json', '--quiet')

# Discover the protocol tools: list_recipes and get_recipe.
npx @inspector --method tools/list -- dotnet $server mcp

# List the recipes declared for .NET 10.
$catalogue = npx @inspector --method tools/call --tool-name list_recipes --tool-arg 'targetVersion="10.0"' -- dotnet $server mcp | ConvertFrom-Json
$catalogue.result.structuredContent.recipes | Format-List id, title, goal

# Retrieve the complete recipe, then display its Markdown and constraints.
$response = npx @inspector --method tools/call --tool-name get_recipe --tool-arg id=csharp.validate-property-assignment --tool-arg 'targetVersion="10.0"' -- dotnet $server mcp | ConvertFrom-Json
$recipe = $response.result.structuredContent.recipe
$recipe.markdown | Show-Markdown
$recipe.applicability.routes | Format-List
$recipe.sources | Format-Table
$recipe.evidence | Format-List
```

Expect one recipe, `csharp.validate-property-assignment`, titled **Normalize and validate properties with the C# field keyword**. Its `goal` explains when to use the feature: property setters that normalize or validate assignments without a separate backing-field declaration. The warehouse domain appears in the worked example; discovery describes the reusable capability.

Its Markdown includes both original implementations, migration steps, recursion/initialization constraints and evidence. Applicability states .NET 10 / C# 14 and the SDK baseline. `$response` retains the complete structured result and the matching JSON text fallback; omit `Show-Markdown` to inspect raw Markdown.

Inspector initializes and closes each connection automatically. There is no separate target-setting command: pass the canonical string `"10.0"` to each tool. The inner double quotes in `'targetVersion="10.0"'` preserve a JSON string rather than Inspector converting `10.0` to a number. Use the stable recipe ID instead of a numeric ID.

To use an installed development package instead of the built DLL, replace `dotnet $server mcp` with `./artifacts/manual-tool/playbook.exe mcp` after following [the local install instructions](contributing.md#inspect-the-development-package). An installed global tool can be launched as `playbook mcp` in the same place. Each command is short-lived and shuts the server down; nothing needs stopping afterwards.
