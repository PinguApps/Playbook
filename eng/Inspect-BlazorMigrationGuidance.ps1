[CmdletBinding()]
param(
  [string[]]$RecipeId = @('blazor.bound-upload-read','blazor.create-typed-event-callback'),
  [string]$PackageVersion = '0.0.0-blazor-migrations',
  [string]$Dotnet = (Get-Command dotnet -CommandType Application | Select-Object -First 1).Source,
  [string]$Node = (Get-Command node -CommandType Application | Select-Object -First 1).Source,
  [string]$OutputDirectory = (Join-Path $PSScriptRoot '../artifacts/blazor-migrations/inspection'),
  [string]$Query = 'migration',
  [switch]$BuildAndPack
)
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$root = Split-Path $PSScriptRoot -Parent
$OutputDirectory = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($OutputDirectory)
$session = Join-Path ([IO.Path]::GetTempPath()) ('playbook-blazor-migrations-' + [guid]::NewGuid().ToString('N'))
$before = @{}
foreach ($scope in 'Process','User','Machine') { $before[$scope] = [Environment]::GetEnvironmentVariable('PATH',$scope) }
$previous = @{}
foreach ($key in 'DOTNET_CLI_HOME','NUGET_PACKAGES','DOTNET_ADD_GLOBAL_TOOLS_TO_PATH','HTTP_PROXY','HTTPS_PROXY','ALL_PROXY') { $previous[$key] = [Environment]::GetEnvironmentVariable($key) }
New-Item -ItemType Directory -Path $session,$OutputDirectory -Force | Out-Null
try {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
  Push-Location -LiteralPath $root
  try {
    if ($BuildAndPack) {
      & $Dotnet build ./src/PinguApps.Playbook -c Release "-p:Version=$PackageVersion"
      & $Dotnet pack ./src/PinguApps.Playbook -c Release --no-build "-p:PackageVersion=$PackageVersion" --output ./artifacts
    }
    $inspectorRoot = Join-Path $root 'artifacts/inspector'
    $entry = Join-Path $inspectorRoot 'node_modules/@modelcontextprotocol/inspector/clients/cli/build/index.js'
    if (-not (Test-Path -LiteralPath $entry)) {
      $npm = Join-Path (Split-Path $Node -Parent) 'node_modules/npm/bin/npm-cli.js'
      & $Node $npm install --prefix $inspectorRoot --ignore-scripts --no-audit --no-fund '@modelcontextprotocol/inspector@2.11.0'
    }
    $package = Join-Path $root "artifacts/PinguApps.Playbook.$PackageVersion.nupkg"
    $feed = Join-Path $session 'feed'
    New-Item -ItemType Directory -Path $feed | Out-Null
    Copy-Item -LiteralPath $package -Destination $feed
    $config = Join-Path $session 'NuGet.Config'
    '<configuration><packageSources><clear/><add key="local" value="{0}"/></packageSources></configuration>' -f [Security.SecurityElement]::Escape($feed) | Set-Content -LiteralPath $config
    $env:DOTNET_CLI_HOME = Join-Path $session 'home'
    $env:NUGET_PACKAGES = Join-Path $session 'cache'
    & $Dotnet tool install PinguApps.Playbook --version $PackageVersion --tool-path (Join-Path $session 'tool') --configfile $config --no-cache
    $tool = Join-Path $session $(if ($IsWindows) { 'tool/playbook.exe' } else { 'tool/playbook' })
    # Proxy-independent offline retrieval after the isolated local-only installation.
    # This is not network isolation; the existing offline container harness owns that claim.
    $env:HTTP_PROXY = $env:HTTPS_PROXY = $env:ALL_PROXY = 'http://127.0.0.1:1'
    Push-Location -LiteralPath $session
    try {
      function Invoke-RecipeTool([string]$Name, [string[]]$Arguments) {
        $call = @('--format','json','--quiet','--method','tools/call','--tool-name',$Name,'--tool-arg','targetVersion="11.0"')
        foreach ($argument in $Arguments) { $call += @('--tool-arg',$argument) }
        $response = & $Node $entry @call -- $tool mcp | ConvertFrom-Json
        if ($response.result.isError) { throw "MCP failure: $Name" }
        $fallback = ($response.result.content | Where-Object type -EQ 'text').text | ConvertFrom-Json
        if (($fallback | ConvertTo-Json -Depth 100 -Compress) -cne ($response.result.structuredContent | ConvertTo-Json -Depth 100 -Compress)) { throw 'JSON-text/structured responses differ.' }
        $response
      }
      $entries = @()
      $offset = 0
      do {
        $filters = @('limit=50',"offset=$offset")
        if ($Query) { $filters += "query=$Query" }
        $page = (Invoke-RecipeTool 'list_recipes' $filters).result.structuredContent
        $entries += @($page.recipes)
        $offset = $page.nextOffset
      } while ($null -ne $offset)
      foreach ($id in $RecipeId) {
        $found = @($entries | Where-Object id -CEQ $id)
        if ($found.Count -ne 1) { throw "Missing or duplicate discovery: $id" }
        $metadata = Get-Content -LiteralPath (Join-Path $root "content/recipes/$id/11.0/recipe.json") -Raw | ConvertFrom-Json
        foreach ($route in $metadata.applicability.routes) {
          if (-not $route.PSObject.Properties['sharedAreas']) { $route | Add-Member -NotePropertyName sharedAreas -NotePropertyValue @() }
          if (-not $route.PSObject.Properties['neutralLanguages']) { $route | Add-Member -NotePropertyName neutralLanguages -NotePropertyValue @() }
        }
        if ($found[0].title -cne $metadata.title -or $found[0].goal -cne $metadata.goal) { throw "Discovery drift: $id" }
        $response = Invoke-RecipeTool 'get_recipe' @("id=$id")
        $recipe = $response.result.structuredContent.recipe
        if ($recipe.id -cne $id -or $recipe.edition -cne '11.0' -or $recipe.markdown.Contains('{{snippet:')) { throw "Wrong edition or unexpanded body: $id" }
        $actualApplicability = [System.Text.Json.Nodes.JsonNode]::Parse(($recipe.applicability | ConvertTo-Json -Depth 100))
        $expectedApplicability = [System.Text.Json.Nodes.JsonNode]::Parse(($metadata.applicability | ConvertTo-Json -Depth 100))
        if (-not [System.Text.Json.Nodes.JsonNode]::DeepEquals($actualApplicability,$expectedApplicability)) {
          $recipe.applicability | ConvertTo-Json -Depth 100 | Set-Content -LiteralPath (Join-Path $OutputDirectory 'actual-applicability.json')
          $metadata.applicability | ConvertTo-Json -Depth 100 | Set-Content -LiteralPath (Join-Path $OutputDirectory 'expected-applicability.json')
          throw "Applicability drift: $id"
        }
        foreach ($evidence in $recipe.evidence) { if ($evidence.freshness -cne 'current') { throw "Stale sample: $($evidence.sample)" } }
        $expectedBody = (Get-Content -LiteralPath (Join-Path $root "content/recipes/$id/11.0/recipe.md") -Raw).Replace("`r`n","`n")
        foreach ($snippet in $metadata.snippets.PSObject.Properties) {
          $value = $snippet.Value
          $source = (Get-Content -LiteralPath (Join-Path $root $value.path) -Raw).Replace("`r`n","`n")
          $match = [regex]::Match($source,'(?ms)^(?://|<!--) snippet:complete(?: -->)?\n(.*?)^(?://|<!--) endsnippet:complete(?: -->)?')
          if (-not $match.Success) { throw "Missing canonical region: $($value.path)" }
          $code = $match.Groups[1].Value.TrimEnd()
          $expectedBody = $expectedBody.Replace('{{snippet:' + $snippet.Name + '}}', ('```' + $value.language + "`n" + $code + "`n" + '```'))
        }
        if ($recipe.markdown.TrimEnd() -cne $expectedBody.TrimEnd()) { throw "Complete canonical Markdown differs: $id" }
        $response | ConvertTo-Json -Depth 100 | Set-Content -LiteralPath (Join-Path $OutputDirectory "$id.json")
        $found[0] | Format-List id,title,goal
        $recipe.markdown
        $recipe | Select-Object id,edition,applicability,evidence,humanProof | ConvertTo-Json -Depth 100
      }
      (Get-FileHash -LiteralPath $package).Hash.ToLowerInvariant() | Set-Content -LiteralPath (Join-Path $OutputDirectory 'package.sha256')
      Write-Host 'PASS: exact local-feed artifact, unrelated-directory installed stdio discovery/detail, full canonical Markdown, applicability, fresh evidence and text/structured equality.'
    }
    finally { Pop-Location }
  }
  finally { Pop-Location }
}
finally {
  foreach ($key in $previous.Keys) {
    if ($null -eq $previous[$key]) { Remove-Item -LiteralPath "Env:$key" -ErrorAction SilentlyContinue }
    else { [Environment]::SetEnvironmentVariable($key,$previous[$key]) }
  }
  foreach ($scope in $before.Keys) { if ($before[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)) { throw "Unexpected $scope PATH change; shared settings preserved." } }
  # Keep the unique local-only feed/tool as disposable inspection evidence under TEMP.
}
