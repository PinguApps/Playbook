Set-Location -LiteralPath 'V:\_worktrees\Playbook\agent-pin-796-04-024-serve-a-standalone-client-with-configured-backend'
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
$dotnet = 'C:\Program Files\dotnet\dotnet.exe'
$node = 'C:\Program Files\nodejs\node.exe'
New-Item -ItemType Directory -Path ./artifacts/pin796 -Force | Out-Null
& $dotnet build ./src/PinguApps.Playbook -c Release -p:Version=0.0.0-pin796-inspect
& $node 'C:\Program Files\nodejs\node_modules\npm\bin\npm-cli.js' install --prefix ./artifacts/inspector --ignore-scripts --no-audit --no-fund @modelcontextprotocol/inspector@2.11.0
$inspector = (Resolve-Path ./artifacts/inspector/node_modules/@modelcontextprotocol/inspector/clients/launcher/build/index.js).Path
$server = (Resolve-Path ./src/PinguApps.Playbook/bin/Release/net10.0/PinguApps.Playbook.dll).Path
$call = { param($tool, $argument)
  & $node $inspector --cli --format json --quiet --method tools/call --tool-name $tool --tool-arg 'targetVersion="11.0"' --tool-arg $argument -- $dotnet $server mcp | ConvertFrom-Json
}
$entry = (& $call list_recipes 'query=quayside-routes').result.structuredContent.recipes
if ($entry.id -ne 'blazor.host-browser-client-with-backends') { throw 'Missing discovery' }
$entry | Format-List id, title, goal
$recipe = (& $call get_recipe 'id=blazor.host-browser-client-with-backends').result.structuredContent.recipe
if ($recipe.edition -ne '11.0' -or -not $recipe.markdown -or $recipe.markdown.Contains('{{snippet:')) { throw 'Incomplete recipe' }
$recipe | Format-List id, edition, stability
$recipe.markdown
$recipe.applicability.routes | ConvertTo-Json -Depth 30
$recipe.sources | Format-Table
$recipe.evidence | Select-Object sample, target, sdk, kind, freshness | Format-Table
[IO.File]::WriteAllText((Join-Path (Get-Location) 'artifacts/pin796/inspected-recipe.json'), ($recipe | ConvertTo-Json -Depth 50).Replace("`r`n", "`n") + "`n")
