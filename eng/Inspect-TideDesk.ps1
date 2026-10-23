Set-Location -LiteralPath 'V:\_worktrees\Playbook\agent-pin-788-04-016-server-configured-blazor-browser-startup'
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
$dotnet = 'C:\Program Files\dotnet\dotnet.exe'
$node = 'C:\Program Files\nodejs\node.exe'
& $dotnet build ./src/PinguApps.Playbook -c Release
& $node 'C:\Program Files\nodejs\node_modules\npm\bin\npm-cli.js' install --prefix ./artifacts/inspector --ignore-scripts --no-audit --no-fund @modelcontextprotocol/inspector@2.11.0
$inspector = (Resolve-Path ./artifacts/inspector/node_modules/@modelcontextprotocol/inspector/clients/launcher/build/index.js).Path
$server = (Resolve-Path ./src/PinguApps.Playbook/bin/Release/net10.0/PinguApps.Playbook.dll).Path
$common = @('--cli', '--format', 'json', '--quiet', '--method', 'tools/call', '--tool-arg', 'targetVersion="11.0"')
$list = & $node $inspector @common --tool-name list_recipes --tool-arg query=tide-policy -- $dotnet $server mcp | ConvertFrom-Json
$list.result.structuredContent.recipes | Format-List id, title, goal
$detail = & $node $inspector @common --tool-name get_recipe --tool-arg id=blazor.configure-browser-from-server -- $dotnet $server mcp | ConvertFrom-Json
$recipe = $detail.result.structuredContent.recipe
if ($recipe.id -cne 'blazor.configure-browser-from-server' -or $recipe.edition -cne '11.0') { throw 'Wrong recipe/edition' }
$recipe.markdown | Write-Output
$recipe.applicability | ConvertTo-Json -Depth 30
$recipe.sources | Format-Table -Wrap
$recipe.evidence | Select-Object sample, target, sdk, kind, freshness | Format-Table
$detail | ConvertTo-Json -Depth 60 | Set-Content ./artifacts/pin788/returned.json
