[CmdletBinding()]
param(
  [string[]]$RecipeId = @('csharp.validate-property-assignment'),
  [string]$TargetVersion = '11.0',
  [string]$Dotnet = (Get-Command dotnet -CommandType Application | Select-Object -First 1).Source,
  [string]$Node = (Get-Command node -CommandType Application | Select-Object -First 1).Source,
  [string]$OutputDirectory = (Join-Path $PSScriptRoot '../artifacts/inspection'),
  [switch]$NoBuild
)

$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$root = Split-Path $PSScriptRoot -Parent
$output = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($OutputDirectory)
$previous = $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
Push-Location -LiteralPath $root
try {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
  if (-not $NoBuild) { & $Dotnet build ./src/PinguApps.Playbook -c Release }
  $server = (Resolve-Path ./src/PinguApps.Playbook/bin/Release/net10.0/PinguApps.Playbook.dll).Path
  $inspectorRoot = Join-Path $root 'artifacts/inspector'
  $inspector = Join-Path $inspectorRoot 'node_modules/@modelcontextprotocol/inspector/clients/launcher/build/index.js'
  if (-not (Test-Path -LiteralPath $inspector)) {
    $npm = Join-Path (Split-Path $Node -Parent) 'node_modules/npm/bin/npm-cli.js'
    & $Node $npm install --prefix $inspectorRoot --ignore-scripts --no-audit --no-fund '@modelcontextprotocol/inspector@2.11.0' | Out-Host
  }
  New-Item -ItemType Directory -Path $output -Force | Out-Null
  function Invoke-Tool([string]$Name, [string[]]$Arguments) {
    $call = @('--cli', '--format', 'json', '--quiet', '--method', 'tools/call', '--tool-name', $Name,
      '--tool-arg', ('targetVersion="' + $TargetVersion + '"'))
    foreach ($argument in $Arguments) { $call += @('--tool-arg', $argument) }
    $response = & $Node $inspector @call -- $Dotnet $server mcp | ConvertFrom-Json
    if ($response.result.isError) { throw ($response.result.content | ConvertTo-Json -Depth 10) }
    $response
  }
  $entries = @()
  $offset = 0
  do {
    $page = (Invoke-Tool 'list_recipes' @('limit=50', "offset=$offset")).result.structuredContent
    $entries += @($page.recipes)
    $offset = $page.nextOffset
  } while ($null -ne $offset)
  foreach ($id in $RecipeId) {
    $entry = @($entries | Where-Object id -CEQ $id)
    if ($entry.Count -ne 1) { throw "Expected one discovery entry for $id at target $TargetVersion." }
    $entry | Format-List id, title, goal | Out-String | Write-Host
    $response = Invoke-Tool 'get_recipe' @("id=$id")
    $response | ConvertTo-Json -Depth 100 | Set-Content -LiteralPath (Join-Path $output "$id.json") -Encoding utf8
    $recipe = $response.result.structuredContent.recipe
    if (-not $recipe.markdown -or $recipe.markdown.Contains('{{snippet:')) { throw "Incomplete Markdown for $id." }
    Write-Host $recipe.markdown
    $recipe.applicability | ConvertTo-Json -Depth 40 | Write-Host
  }
}
finally {
  Pop-Location
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = $previous
}
