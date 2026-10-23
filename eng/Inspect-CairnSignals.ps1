[CmdletBinding()]
param(
  [string]$PackageVersion = '1.0.0-pin799.2',
  [string]$InspectorRoot,
  [string]$OutputDirectory = (Join-Path (Split-Path $PSScriptRoot -Parent) 'artifacts/pin799/inspection')
)
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$root = Split-Path $PSScriptRoot -Parent
$node = 'C:\Program Files\nodejs\node.exe'
$dotnet = 'C:\Program Files\dotnet\dotnet.exe'
$before = @{}
foreach ($scope in @('Process','User','Machine')) { $before[$scope]=[Environment]::GetEnvironmentVariable('PATH',$scope) }
$session = Join-Path ([IO.Path]::GetTempPath()) ('playbook-pin799-inspect-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $session -Force | Out-Null
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
$OutputDirectory = [IO.Path]::GetFullPath($OutputDirectory)
$previous = @{}
foreach ($key in @('DOTNET_CLI_HOME','NUGET_PACKAGES','DOTNET_ADD_GLOBAL_TOOLS_TO_PATH')) { $previous[$key]=[Environment]::GetEnvironmentVariable($key) }
try {
  if (-not $InspectorRoot) {
    $InspectorRoot = Join-Path $root 'artifacts/pin799/inspector'
    $entry = Join-Path $InspectorRoot 'node_modules/@modelcontextprotocol/inspector/clients/cli/build/index.js'
    if (-not (Test-Path $entry)) {
      & $node 'C:\Program Files\nodejs\node_modules\npm\bin\npm-cli.js' install --prefix $InspectorRoot --ignore-scripts --no-audit --no-fund '@modelcontextprotocol/inspector@2.11.0'
    }
  }
  $entry = Join-Path $InspectorRoot 'node_modules/@modelcontextprotocol/inspector/clients/cli/build/index.js'
  $feed = Join-Path $session 'feed'
  New-Item -ItemType Directory -Path $feed | Out-Null
  Copy-Item (Join-Path $root "artifacts/PinguApps.Playbook.$PackageVersion.nupkg") $feed
  $config = Join-Path $session 'NuGet.Config'
  '<configuration><packageSources><clear/><add key="local" value="{0}"/></packageSources></configuration>' -f [Security.SecurityElement]::Escape($feed) | Set-Content $config
  $env:DOTNET_CLI_HOME = Join-Path $session 'home'
  $env:NUGET_PACKAGES = Join-Path $session 'cache'
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
  & $dotnet tool install PinguApps.Playbook --version $PackageVersion --tool-path (Join-Path $session 'tool') --configfile $config --no-cache
  $tool = Join-Path $session 'tool/playbook.exe'
  Push-Location $session
  try {
    $options = @('--format','json','--quiet')
    & $node $entry @options --method tools/list -- $tool mcp | Set-Content (Join-Path $OutputDirectory 'tools.json')
    $list = & $node $entry @options --method tools/call --tool-name list_recipes --tool-arg 'targetVersion="11.0"' --tool-arg 'query=cairn-signals' -- $tool mcp | ConvertFrom-Json
    $detail = & $node $entry @options --method tools/call --tool-name get_recipe --tool-arg id=blazor.rename-custom-event --tool-arg 'targetVersion="11.0"' -- $tool mcp | ConvertFrom-Json
  } finally { Pop-Location }
  $list.result.structuredContent | ConvertTo-Json -Depth 50 | Set-Content (Join-Path $OutputDirectory 'list.json')
  $recipe = $detail.result.structuredContent.recipe
  $recipe | ConvertTo-Json -Depth 100 | Set-Content (Join-Path $OutputDirectory 'recipe.json')
  $list.result.structuredContent.recipes | Format-List id,title,goal,applicability
  $recipe.markdown
  $recipe | Select-Object id,edition,stability,humanProof,applicability,evidence | ConvertTo-Json -Depth 100
  if ($recipe.id -cne 'blazor.rename-custom-event' -or $recipe.edition -cne '11.0') { throw 'Wrong installed recipe edition.' }
  Write-Host "PASS: actual installed stdio list/get from unrelated $session; full response saved under $OutputDirectory."
}
finally {
  foreach ($key in $previous.Keys) { [Environment]::SetEnvironmentVariable($key,$previous[$key]) }
  foreach ($scope in $before.Keys) { if ($before[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)) { throw "Unexpected $scope PATH change; shared settings preserved." } }
}
