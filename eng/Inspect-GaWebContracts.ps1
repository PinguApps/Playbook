[CmdletBinding()]
param(
  [string]$DotnetRoot = (Split-Path (Get-Command dotnet -CommandType Application | Select-Object -First 1).Source -Parent),
  [string]$BlazorAssets = (Join-Path $PSScriptRoot '../artifacts/ga-blazor.assets.json'),
  [string]$WebAssets = (Join-Path $PSScriptRoot '../artifacts/ga-web.assets.json'),
  [string]$OutputPath = (Join-Path $PSScriptRoot '../artifacts/ga-web-contracts.json')
)
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$ga = '297acf930fb0f5c07fd4e51d78e521b4ad2e5d1f'
if (-not (Get-Content (Join-Path $DotnetRoot 'sdk/11.0.100/.version') -Raw).Contains($ga)) { throw 'Actual GA SDK required.' }
$feed = Invoke-RestMethod 'https://builds.dotnet.microsoft.com/dotnet/release-metadata/11.0/releases.json'
if (-not ($feed.releases | Where-Object 'release-version' -EQ '11.0.0')) { throw 'GA release unavailable.' }
$heads = @{
  'dotnet/aspnetcore' = '1def4cdc07dc10fc267136060e3ef5c1a96d4a99'
  'dotnet/AspNetCore.Docs' = 'fd01aafa737af65203d3bd4ea08f1920305c52a9'
  'dotnet/docs' = '0dad19ab65b096f0b86430c01b1b6a9e5832ea22'
  'dotnet/core' = '33918e8affbb517bf654c15559ea6f813c08bbae'
}
$sources = foreach ($family in @('blazor','web')) {
  (Get-Content (Join-Path $root "docs/planning/audits/$family.sources.json") -Raw | ConvertFrom-Json).sources
}
$comparisons = @($sources | Sort-Object url -Unique | ForEach-Object -Parallel {
  $ErrorActionPreference = 'Stop'
  $source = $_
  $heads = $using:heads
  $old = (Invoke-WebRequest $source.url).RawContentStream.ToArray()
  $oldHash = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($old)).ToLowerInvariant()
  if ($oldHash -cne $source.sha256) { throw "Retained source mismatch: $($source.url)" }
  $url = $source.url.Replace($source.commit,$heads[$source.repository])
  try { $new = (Invoke-WebRequest $url).RawContentStream.ToArray() }
  catch {
    if ([int]$_.Exception.Response.StatusCode -ne 404) { throw }
    return [ordered]@{ rcUrl=$source.url; rcSha256=$oldHash; gaUrl=$url; unavailable=404 }
  }
  $hash = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($new)).ToLowerInvariant()
  $delta = @(Compare-Object ([Text.Encoding]::UTF8.GetString($old) -split "`n") ([Text.Encoding]::UTF8.GetString($new) -split "`n") -CaseSensitive)
  [ordered]@{ rcUrl=$source.url; rcSha256=$oldHash; gaUrl=$url; gaSha256=$hash; unchanged=$hash -ceq $oldHash; added=@($delta | Where-Object SideIndicator -EQ '=>' | ForEach-Object InputObject); removed=@($delta | Where-Object SideIndicator -EQ '<=' | ForEach-Object InputObject) }
} -ThrottleLimit 8)
if ($comparisons.Count -ne @($sources | Sort-Object url -Unique).Count) { throw 'Incomplete source comparison.' }
$catalogue = foreach ($file in Get-ChildItem (Join-Path $root 'content/recipes') -Recurse -Filter recipe.json) {
  $recipe = Get-Content $file.FullName -Raw | ConvertFrom-Json
  if (-not @($recipe.areas | Where-Object { $_ -in @('blazor','aspnetcore','hosting') }).Count) { continue }
  [ordered]@{ id=$recipe.id; edition=$recipe.edition; title=$recipe.title; stability=$recipe.stability; metadataSha256=(Get-FileHash $file.FullName).Hash.ToLowerInvariant(); markdownSha256=(Get-FileHash (Join-Path $file.DirectoryName 'recipe.md')).Hash.ToLowerInvariant() }
}
$artifacts = foreach ($file in Get-ChildItem (Join-Path $DotnetRoot 'packs/Microsoft.AspNetCore.App.Ref/11.0.0/ref/net11.0') -Filter '*.dll') {
  [ordered]@{ file=$file.Name; sha256=(Get-FileHash $file.FullName).Hash.ToLowerInvariant(); productVersion=[Diagnostics.FileVersionInfo]::GetVersionInfo($file.FullName).ProductVersion }
}
$newContracts = foreach ($path in @('aspnetcore/breaking-changes/11/identity-login-ordinal-equality.md','aspnetcore/breaking-changes/11/openid-connect-max-age-validates-auth-time.md','aspnetcore/blazor/components/ai-components.md')) {
  $url = "https://raw.githubusercontent.com/dotnet/AspNetCore.Docs/$($heads['dotnet/AspNetCore.Docs'])/$path"
  $bytes = (Invoke-WebRequest $url).RawContentStream.ToArray()
  [ordered]@{ url=$url; sha256=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes)).ToLowerInvariant() }
}
$packages = foreach ($id in @('microsoft.aspnetcore.components.ai','microsoft.aspnetcore.components.server.autopause','microsoft.aspnetcore.components.gateway','microsoft.aspnetcore.components.gateway.cli','microsoft.aspnetcore.server.kestrel.transport.directtls')) {
  $url = "https://api.nuget.org/v3-flatcontainer/$id/index.json"
  $index = Invoke-RestMethod $url
  [ordered]@{ id=$id; index=$url; versions=@($index.versions); hasGa11='11.0.0' -in $index.versions }
}
if (@($catalogue).Count -ne 117 -or @($artifacts).Count -ne 132 -or $comparisons.Count -ne 266) { throw 'Reconcile changed audit coverage before recording evidence.' }
$metadata = foreach ($family in @('blazor','web')) {
  $path = if ($family -eq 'blazor') { $BlazorAssets } else { $WebAssets }
  $beforePath = Join-Path $root "docs/planning/audits/$family.assets.json"
  $baselineUrl = $null
  if ($family -eq 'web') {
    $baselineUrl = 'https://raw.githubusercontent.com/PinguApps/Playbook/e0d31f00bca55c5c8afb05a934ed6468fb43314a/docs/planning/audits/web.assets.json'
    $beforeBytes = (Invoke-WebRequest $baselineUrl).RawContentStream.ToArray()
  } else { $beforeBytes = [IO.File]::ReadAllBytes($beforePath) }
  $before = [Text.Encoding]::UTF8.GetString($beforeBytes) | ConvertFrom-Json
  $after = Get-Content $path -Raw | ConvertFrom-Json
  if ($after.baselines[1].sdk -ne '11.0.100' -or $after.baselines[1].referencePack -ne '11.0.0') { throw 'GA metadata required.' }
  $deltas = foreach ($kind in @('publicApiDiff','publicToolingDiff')) {
   foreach ($difference in $after.$kind) {
    $previous = $before.$kind | Where-Object asset -CEQ $difference.asset
    foreach ($side in @('added','removed')) {
      $oldLines = @($previous.diff.$side | Where-Object { $null -ne $_ })
      $newLines = @($difference.diff.$side | Where-Object { $null -ne $_ })
      $delta = @(Compare-Object $oldLines $newLines -CaseSensitive)
      if ($delta.Count) {
        [ordered]@{ kind=$kind; asset=$difference.asset; side=$side; introduced=@($delta | Where-Object SideIndicator -EQ '=>' | ForEach-Object InputObject); withdrawn=@($delta | Where-Object SideIndicator -EQ '<=' | ForEach-Object InputObject) }
      }
    }
   }
  }
  [ordered]@{ family=$family; baselineUrl=$baselineUrl; retainedSdk=$before.baselines[1].sdk; baselineSha256=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($beforeBytes)).ToLowerInvariant(); gaReportSha256=(Get-FileHash $path).Hash.ToLowerInvariant(); metadataDeltas=@($deltas); packages=@($after.baselines[1].packages | Select-Object id,version,sha256,commit,dependencies) }
}
$report = [ordered]@{ scope='PIN-964 source/artifact audit; no consumer execution claim'; sdk='11.0.100'; gaSource=$ga; sourceHeads=$heads; comparisons=@($comparisons | Sort-Object { $_.rcUrl }); newContracts=@($newContracts); packageAvailability=@($packages); catalogue=@($catalogue | Sort-Object { $_.id },{ $_.edition }); referenceAssemblies=@($artifacts); metadata=@($metadata) }
$output = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($OutputPath)
New-Item -ItemType Directory -Force (Split-Path $output) | Out-Null
[IO.File]::WriteAllText($output,($report | ConvertTo-Json -Depth 50).Replace("`r`n","`n") + "`n")
Write-Host "PASS: $($comparisons.Count) source comparisons, $(@($catalogue).Count) editions, $(@($artifacts).Count) reference assemblies; $output"
