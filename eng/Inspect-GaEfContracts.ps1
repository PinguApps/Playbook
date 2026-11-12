[CmdletBinding()]
param(
  [string]$Assets = (Join-Path $PSScriptRoot '../artifacts/ga-ef.assets.json'),
  [string]$OutputPath = (Join-Path $PSScriptRoot '../artifacts/ga-ef-contracts.json')
)
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$heads = @{
  'dotnet/core' = '33918e8affbb517bf654c15559ea6f813c08bbae'
  'dotnet/EntityFramework.Docs' = '315648487e25e5657760174a2064fe525c9f781b'
  'dotnet/dotnet' = '297acf930fb0f5c07fd4e51d78e521b4ad2e5d1f'
  'dotnet/docs' = '0dad19ab65b096f0b86430c01b1b6a9e5832ea22'
}
$feed = Invoke-RestMethod 'https://builds.dotnet.microsoft.com/dotnet/release-metadata/11.0/releases.json'
if (-not ($feed.releases | Where-Object 'release-version' -EQ '11.0.0')) { throw 'Actual GA release required.' }
$sources = (Get-Content (Join-Path $root 'docs/planning/audits/ef.sources.json') -Raw | ConvertFrom-Json).sources
if (@($sources).Count -ne @($sources | Sort-Object url -Unique).Count) { throw 'Duplicate retained source records.' }
$comparisons = @($sources | Sort-Object url -Unique | ForEach-Object -Parallel {
  $ErrorActionPreference = 'Stop'
  $source = $_
  $heads = $using:heads
  if (-not $heads.ContainsKey($source.repository)) { throw "Unknown GA source repository: $($source.repository)" }
  $old = (Invoke-WebRequest $source.url).RawContentStream.ToArray()
  $oldHash = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($old)).ToLowerInvariant()
  if ($oldHash -cne $source.sha256) { throw "Retained source mismatch: $($source.url)" }
  $url = $source.url.Replace($source.commit,$heads[$source.repository])
  try { $new = (Invoke-WebRequest $url).RawContentStream.ToArray() }
  catch {
    if ([int]$_.Exception.Response.StatusCode -ne 404) { throw }
    return [ordered]@{ retainedUrl=$source.url; retainedSha256=$oldHash; gaUrl=$url; unavailable=404 }
  }
  $hash = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($new)).ToLowerInvariant()
  $delta = @(Compare-Object ([Text.Encoding]::UTF8.GetString($old) -split "`n") ([Text.Encoding]::UTF8.GetString($new) -split "`n") -CaseSensitive)
  [ordered]@{ retainedUrl=$source.url; retainedSha256=$oldHash; gaUrl=$url; gaSha256=$hash; unchanged=$hash -ceq $oldHash; added=@($delta | Where-Object SideIndicator -EQ '=>' | ForEach-Object InputObject); removed=@($delta | Where-Object SideIndicator -EQ '<=' | ForEach-Object InputObject) }
} -ThrottleLimit 8)
if ($comparisons.Count -ne @($sources | Sort-Object url -Unique).Count) { throw 'Incomplete source comparison.' }
if (@($comparisons | Where-Object { -not $_.gaSha256 }).Count) { throw 'GA source unavailable; retain an explicit availability wait before recording reconciliation.' }
$baselineUrl = 'https://raw.githubusercontent.com/PinguApps/Playbook/b7b6c2edc489fe264efe0d559a526a676dca7686/docs/planning/audits/ef.assets.json'
$beforeBytes = (Invoke-WebRequest $baselineUrl).RawContentStream.ToArray()
$before = [Text.Encoding]::UTF8.GetString($beforeBytes) | ConvertFrom-Json
$after = Get-Content $Assets -Raw | ConvertFrom-Json
if ($before.baselines[1].sdk -notlike '*rc*' -or $after.baselines[1].sdk -ne '11.0.100' -or $after.baselines[1].packageVersion -ne '11.0.0') { throw 'Distinct RC and actual GA metadata required.' }
$firstParty = @($after.baselines[1].packages | Where-Object { $_.id -match '^microsoft\.(entityframeworkcore|data\.sqlite)(\.|$)' -or $_.id -eq 'dotnet-ef' })
if ($firstParty.Count -ne 20 -or @($firstParty | Where-Object { $_.version -ne '11.0.0' -or $_.commit -ne $heads['dotnet/dotnet'] }).Count) { throw 'Complete package-matched GA EF/provider metadata required.' }
$deltas = foreach ($kind in @('publicApiDiff','toolingTextDiff')) {
  $assetsToCompare = @(@($before.$kind.asset) + @($after.$kind.asset) | Sort-Object -Unique -CaseSensitive)
  foreach ($asset in $assetsToCompare) {
    $previous = $before.$kind | Where-Object asset -CEQ $asset
    $current = $after.$kind | Where-Object asset -CEQ $asset
    foreach ($side in @('added','removed')) {
      $delta = @(Compare-Object @($previous.diff.$side | Where-Object { $null -ne $_ }) @($current.diff.$side | Where-Object { $null -ne $_ }) -CaseSensitive)
      if ($delta.Count) { [ordered]@{ kind=$kind; asset=$asset; side=$side; introduced=@($delta | Where-Object SideIndicator -EQ '=>' | ForEach-Object InputObject); withdrawn=@($delta | Where-Object SideIndicator -EQ '<=' | ForEach-Object InputObject) } }
    }
  }
}
$catalogue = foreach ($file in Get-ChildItem (Join-Path $root 'content/recipes') -Recurse -Filter recipe.json) {
  $recipe = Get-Content $file.FullName -Raw | ConvertFrom-Json
  if (-not @($recipe.areas | Where-Object { $_ -in @('ef-core','sql-server','sqlite','cosmos') }).Count) { continue }
  [ordered]@{ id=$recipe.id; edition=$recipe.edition; title=$recipe.title; stability=$recipe.stability; metadataSha256=(Get-FileHash $file.FullName).Hash.ToLowerInvariant(); markdownSha256=(Get-FileHash (Join-Path $file.DirectoryName 'recipe.md')).Hash.ToLowerInvariant() }
}
$gaSources = foreach ($path in @('EFCore.Sqlite.Core/Migrations/SqliteMigrationsSqlGenerator.cs','EFCore.Relational/Metadata/RelationalJsonIndex.cs','EFCore.Sqlite.Core/Infrastructure/Internal/SqliteModelValidator.cs','EFCore.SqlServer/Infrastructure/Internal/SqlServerModelValidator.cs')) {
  $url = "https://raw.githubusercontent.com/dotnet/dotnet/$($heads['dotnet/dotnet'])/src/efcore/src/$path"
  $bytes = (Invoke-WebRequest $url).RawContentStream.ToArray()
  [ordered]@{ url=$url; sha256=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes)).ToLowerInvariant() }
}
if ($comparisons.Count -ne 123 -or @($catalogue).Count -ne 81) { throw 'Reconcile changed family/source coverage before recording a release decision.' }
$url = "https://raw.githubusercontent.com/dotnet/core/$($heads['dotnet/core'])/release-notes/11.0/11.0.0/11.0.0.md"
$bytes = (Invoke-WebRequest $url).RawContentStream.ToArray()
$gaSources += [ordered]@{ url=$url; sha256=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes)).ToLowerInvariant() }
$report = [ordered]@{ scope='PIN-965 release-decision audit; metadata/source review does not establish database execution'; sourceHeads=$heads; baselineUrl=$baselineUrl; baselineSha256=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($beforeBytes)).ToLowerInvariant(); retainedSdk=$before.baselines[1].sdk; gaReportSha256=(Get-FileHash $Assets).Hash.ToLowerInvariant(); comparisons=@($comparisons | Sort-Object retainedUrl); metadataDeltas=@($deltas); packages=@($after.baselines[1].packages | Select-Object id,version,sha256,commit,selectedDependencyTarget,dependencies); catalogue=@($catalogue | Sort-Object id,edition) }
$output = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($OutputPath)
$report.gaSources = @($gaSources)
New-Item -ItemType Directory -Force (Split-Path $output) | Out-Null
[IO.File]::WriteAllText($output,($report | ConvertTo-Json -Depth 50).Replace("`r`n","`n") + "`n")
Write-Host "PASS: $($comparisons.Count) sources, $(@($catalogue).Count) editions, $(@($deltas).Count) metadata delta groups; $output"
