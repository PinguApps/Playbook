[CmdletBinding()]
param(
  [string]$DotnetRoot = 'C:/Users/pingu/AppData/Local/dotnetup/dotnet',
  [string]$OutputPath = (Join-Path $PSScriptRoot '../artifacts/ga-sdk-contracts.json'),
  [string]$AssetReport = (Join-Path $PSScriptRoot '../artifacts/ga-sdk-build.assets.json')
)
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$ga = '297acf930fb0f5c07fd4e51d78e521b4ad2e5d1f'
$rc = '820230c386db5b0106c5430b3e5aee92a7826bf5'
$feed = Invoke-RestMethod 'https://builds.dotnet.microsoft.com/dotnet/release-metadata/11.0/releases.json'
if (-not ($feed.releases | Where-Object 'release-version' -EQ '11.0.0')) { throw 'Actual GA release required.' }
if (-not (Get-Content (Join-Path $DotnetRoot 'sdk/11.0.100/.version') -Raw).Contains($ga)) { throw 'GA SDK source mismatch.' }
$seen = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
$comparisons = foreach ($family in @('sdk-build','sdk-package-hosts')) {
  $manifest = Get-Content (Join-Path $root "docs/planning/audits/$family.sources.json") -Raw | ConvertFrom-Json
  foreach ($source in $manifest.sources) {
    if ($source.url -notmatch $rc -or -not $seen.Add($source.url)) { continue }
    $old = (Invoke-WebRequest $source.url).RawContentStream.ToArray()
    $oldHash = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($old)).ToLowerInvariant()
    if ($oldHash -cne $source.sha256) { throw "Retained RC source mismatch: $($source.url)" }
    $url = $source.url.Replace($rc,$ga)
    $new = (Invoke-WebRequest $url).RawContentStream.ToArray()
    $hash = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($new)).ToLowerInvariant()
    $delta = @(Compare-Object ([Text.Encoding]::UTF8.GetString($old) -split "`n") ([Text.Encoding]::UTF8.GetString($new) -split "`n") -CaseSensitive)
    [ordered]@{ rcUrl=$source.url; rcSha256=$oldHash; gaUrl=$url; gaSha256=$hash; unchanged=$hash -ceq $oldHash; added=@($delta | Where-Object SideIndicator -EQ '=>' | ForEach-Object InputObject); removed=@($delta | Where-Object SideIndicator -EQ '<=' | ForEach-Object InputObject) }
  }
}
$catalogue = foreach ($file in Get-ChildItem (Join-Path $root 'content/recipes') -Recurse -Filter recipe.json) {
  $recipe = Get-Content $file.FullName -Raw | ConvertFrom-Json
  if ($recipe.edition -ne '11.0' -or 'sdk' -notin $recipe.areas) { continue }
  [ordered]@{ id=$recipe.id; title=$recipe.title; stability=$recipe.stability; metadataSha256=(Get-FileHash $file.FullName).Hash.ToLowerInvariant(); markdownSha256=(Get-FileHash (Join-Path $file.DirectoryName 'recipe.md')).Hash.ToLowerInvariant(); requirements=$recipe.requirements }
}
$baseline = Get-Content (Join-Path $root 'docs/planning/audits/sdk-build.assets.json') -Raw | ConvertFrom-Json
$retained = $baseline.baselines | Where-Object target -EQ 'net11.0'
$assets = foreach ($asset in $retained.assemblies) {
  $path = Join-Path $DotnetRoot "sdk/11.0.100/$($asset.path)"
  [ordered]@{ path=$asset.path; rcSha256=$asset.sha256; gaSha256=(Get-FileHash $path).Hash.ToLowerInvariant(); productVersion=[Diagnostics.FileVersionInfo]::GetVersionInfo($path).ProductVersion }
}
$current = Get-Content $AssetReport -Raw | ConvertFrom-Json
if ($current.baselines[1].sdk -ne '11.0.100') { throw 'Actual GA metadata report required.' }
if ($current.baselines[1].sdkIdentity[0] -ne $ga) { throw 'Metadata source identity mismatch.' }
foreach ($asset in $assets) {
  $measured = @($current.baselines[1].assemblies | Where-Object path -CEQ $asset.path)
  if ($measured.Count -ne 1 -or $measured[0].sha256 -cne $asset.gaSha256) { throw "Metadata/artifact mismatch: $($asset.path)" }
}
$deltas = foreach ($difference in $current.differences) {
  $previous = $baseline.differences | Where-Object { $_.kind -eq $difference.kind -and $_.asset -eq $difference.asset }
  foreach ($side in @('added','removed')) {
    $before = @($previous.diff.$side | Where-Object { $null -ne $_ })
    $after = @($difference.diff.$side | Where-Object { $null -ne $_ })
    $delta = @(Compare-Object $before $after -CaseSensitive)
    if ($delta.Count) {
      [ordered]@{ kind=$difference.kind; asset=$difference.asset; side=$side; introduced=@($delta | Where-Object SideIndicator -EQ '=>' | ForEach-Object InputObject); withdrawn=@($delta | Where-Object SideIndicator -EQ '<=' | ForEach-Object InputObject) }
    }
  }
}
$report = [ordered]@{ scope='PIN-963 bounded GA SDK source/artifact reconciliation; hashes do not prove execution'; gaSource=$ga; rcSource=$rc; retainedSdk=$retained.sdk; comparisons=@($comparisons); catalogue=@($catalogue | Sort-Object id); tooling=@($assets); metadataReportSha256=(Get-FileHash $AssetReport).Hash.ToLowerInvariant(); metadataDeltas=@($deltas) }
$output = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($OutputPath)
New-Item -ItemType Directory -Force (Split-Path $output) | Out-Null
[IO.File]::WriteAllText($output,($report | ConvertTo-Json -Depth 50).Replace("`r`n","`n") + "`n")
Write-Host "PASS: $(@($comparisons).Count) source comparisons, $(@($catalogue).Count) SDK editions, $(@($assets).Count) tooling assets; $output"
