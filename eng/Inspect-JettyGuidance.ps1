[CmdletBinding()]
param(
  [string]$PackageVersion = '0.0.0-pin809-810-978',
  [string]$InspectorRoot,
  [string]$OutputDirectory = (Join-Path (Split-Path $PSScriptRoot -Parent) 'artifacts/pin809-810/inspection')
)
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$root = Split-Path $PSScriptRoot -Parent
$node = (Get-Command node -CommandType Application | Select-Object -First 1).Source
$dotnet = (Get-Command dotnet -CommandType Application | Select-Object -First 1).Source
$before = @{}
foreach ($scope in @('Process','User','Machine')) { $before[$scope]=[Environment]::GetEnvironmentVariable('PATH',$scope) }
$session = Join-Path ([IO.Path]::GetTempPath()) ('playbook-jetty-inspect-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $session -Force | Out-Null
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
$OutputDirectory = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($OutputDirectory)
$previous = @{}
foreach ($key in @('DOTNET_CLI_HOME','NUGET_PACKAGES','DOTNET_ADD_GLOBAL_TOOLS_TO_PATH')) { $previous[$key]=[Environment]::GetEnvironmentVariable($key) }
try {
  if (-not $InspectorRoot) {
    $InspectorRoot = Join-Path $root 'artifacts/pin809-810/inspector'
    $entry = Join-Path $InspectorRoot 'node_modules/@modelcontextprotocol/inspector/clients/cli/build/index.js'
    if (-not (Test-Path $entry)) {
      $npm = Join-Path (Split-Path $node) 'node_modules/npm/bin/npm-cli.js'
      & $node $npm install --prefix $InspectorRoot --ignore-scripts --no-audit --no-fund '@modelcontextprotocol/inspector@2.11.0'
    }
  }
  $entry = Join-Path $InspectorRoot 'node_modules/@modelcontextprotocol/inspector/clients/cli/build/index.js'
  $feed = Join-Path $session 'feed'
  New-Item -ItemType Directory -Path $feed | Out-Null
  $package = Join-Path $root "artifacts/PinguApps.Playbook.$PackageVersion.nupkg"
  Copy-Item $package $feed
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
    $catalogue = @()
    foreach ($query in @('jetty-access','mooring-clock')) {
      $list = & $node $entry @options --method tools/call --tool-name list_recipes --tool-arg 'targetVersion="11.0"' --tool-arg "query=$query" -- $tool mcp | ConvertFrom-Json
      $list.result.structuredContent | ConvertTo-Json -Depth 100 | Set-Content (Join-Path $OutputDirectory "list-$query.json")
      $catalogue += @($list.result.structuredContent.recipes)
    }
    $catalogue | Format-List id,title,goal,stability,applicability
    $ids = @('authorization.share-policy-metadata','identity.control-test-clock')
    if (($catalogue.id -join ',') -cne ($ids -join ',')) { throw 'Incomplete installed discovery.' }
    foreach ($id in $ids) {
      $detail = & $node $entry @options --method tools/call --tool-name get_recipe --tool-arg "id=$id" --tool-arg 'targetVersion="11.0"' -- $tool mcp | ConvertFrom-Json
      $recipe = $detail.result.structuredContent.recipe
      $recipe | ConvertTo-Json -Depth 100 | Set-Content (Join-Path $OutputDirectory "$id.json")
      $recipe.markdown
      $recipe | Select-Object id,edition,stability,humanProof,applicability,evidence | ConvertTo-Json -Depth 100
      if ($recipe.id -cne $id -or $recipe.edition -cne '11.0' -or $recipe.markdown.Contains('{{snippet:')) { throw 'Incomplete/wrong installed recipe.' }
    }
  } finally { Pop-Location }
  @{ packageVersion=$PackageVersion; packageSha256=(Get-FileHash $package).Hash.ToLowerInvariant(); workingDirectory=$session; isolatedFeed=$feed; isolatedCache=$env:NUGET_PACKAGES; inspectorVersion='2.11.0'; recordedAt=[DateTimeOffset]::UtcNow.ToString('O') } | ConvertTo-Json | Set-Content (Join-Path $OutputDirectory 'inspection.evidence.json')
  Write-Host "PASS: actual installed stdio tools/list and full list/get outside the checkout; saved under $OutputDirectory."
}
finally {
  foreach ($key in $previous.Keys) { [Environment]::SetEnvironmentVariable($key,$previous[$key]) }
  foreach ($scope in $before.Keys) { if ($before[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)) { throw "Unexpected $scope PATH change; shared settings preserved." } }
}
