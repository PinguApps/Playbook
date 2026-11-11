[CmdletBinding()]
param(
  [string]$DotnetRoot = 'C:/Users/pingu/AppData/Local/dotnetup/dotnet',
  [string]$OutputPath = (Join-Path $PSScriptRoot '../artifacts/ga-library-contracts.json'),
  [string]$PackageRoot = (Join-Path ([Environment]::GetFolderPath('UserProfile')) '.nuget/packages')
)
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$ga = '297acf930fb0f5c07fd4e51d78e521b4ad2e5d1f'
$rc = '820230c386db5b0106c5430b3e5aee92a7826bf5'
$release = Invoke-RestMethod 'https://builds.dotnet.microsoft.com/dotnet/release-metadata/11.0/releases.json'
if (-not ($release.releases | Where-Object 'release-version' -EQ '11.0.0')) { throw 'GA release required.' }
if (-not (Get-Content (Join-Path $DotnetRoot 'sdk/11.0.100/.version') -Raw).Contains($ga)) { throw 'GA SDK source mismatch.' }
$families = @('data-io','serialization','extensions','runtime-security','numeric-and-parsing','cryptography-platforms')
$seen = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
$comparisons = foreach ($family in $families) {
  $manifest = Get-Content (Join-Path $root "docs/planning/audits/$family.sources.json") -Raw | ConvertFrom-Json
  foreach ($source in $manifest.sources) {
    if ($source.url -notmatch $rc -or -not $seen.Add($source.url)) { continue }
    $oldBytes = (Invoke-WebRequest $source.url).RawContentStream.ToArray()
    $oldHash = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($oldBytes)).ToLowerInvariant()
    if ($oldHash -cne $source.sha256) { throw "Retained baseline changed: $($source.url)" }
    $url = $source.url.Replace($rc,$ga)
    $newBytes = (Invoke-WebRequest $url).RawContentStream.ToArray()
    $newHash = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($newBytes)).ToLowerInvariant()
    $old = [Text.Encoding]::UTF8.GetString($oldBytes) -split "`n"
    $new = [Text.Encoding]::UTF8.GetString($newBytes) -split "`n"
    $delta = @(Compare-Object $old $new -CaseSensitive)
    [pscustomobject]@{ family=$family; rcUrl=$source.url; rcSha256=$oldHash; gaUrl=$url; gaSha256=$newHash; unchanged=$oldHash -ceq $newHash; added=@($delta | Where-Object SideIndicator -EQ '=>' | ForEach-Object InputObject); removed=@($delta | Where-Object SideIndicator -EQ '<=' | ForEach-Object InputObject) }
  }
}
$catalogue = foreach ($file in Get-ChildItem (Join-Path $root 'content/recipes') -Recurse -Filter recipe.json) {
  $recipe = Get-Content $file.FullName -Raw | ConvertFrom-Json
  if ($recipe.edition -ne '11.0' -or 'libraries' -notin $recipe.areas) { continue }
  [pscustomobject]@{ id=$recipe.id; title=$recipe.title; stability=$recipe.stability; metadataSha256=(Get-FileHash $file.FullName).Hash.ToLowerInvariant(); markdownSha256=(Get-FileHash (Join-Path $file.DirectoryName 'recipe.md')).Hash.ToLowerInvariant(); requirements=$recipe.requirements }
}
$artifacts = foreach ($file in Get-ChildItem (Join-Path $DotnetRoot 'shared/Microsoft.NETCore.App/11.0.0') -Filter '*.dll') {
  [pscustomobject]@{ file=$file.Name; productVersion=[Diagnostics.FileVersionInfo]::GetVersionInfo($file.FullName).ProductVersion; sha256=(Get-FileHash $file.FullName).Hash.ToLowerInvariant() }
}
$package = Join-Path $PackageRoot 'system.numerics.tensors/11.0.0'
[xml]$nuspec = Get-Content (Join-Path $package 'system.numerics.tensors.nuspec') -Raw
if ($nuspec.package.metadata.repository.commit -cne $ga) { throw 'Tensor package source identity mismatch.' }
$tensor = [ordered]@{ version='11.0.0'; source=$ga; asset='lib/net11.0/System.Numerics.Tensors.dll'; sha256=(Get-FileHash (Join-Path $package 'lib/net11.0/System.Numerics.Tensors.dll')).Hash.ToLowerInvariant() }
$backport = Get-Content (Join-Path $root 'docs/planning/audits/tensor-release.sources.json') -Raw | ConvertFrom-Json
$backportComparison = foreach ($source in $backport.sources) {
  $url = $source.url.Replace('dotnet/runtime/' + $backport.sourceCommit, 'dotnet/dotnet/' + $ga + '/src/runtime')
  $bytes = (Invoke-WebRequest $url).RawContentStream.ToArray()
  $hash = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes)).ToLowerInvariant()
  [pscustomobject]@{ id=$source.id; gaUrl=$url; gaSha256=$hash; backportSha256=$source.sha256; unchanged=$hash -ceq $source.sha256 }
}
$result = [ordered]@{ scope='GA library source/artifact reconciliation; source equality does not prove consumer execution'; sdk='11.0.100'; runtime='11.0.0'; gaSource=$ga; rcSource=$rc; comparisons=@($comparisons); catalogue=@($catalogue | Sort-Object id); artifacts=@($artifacts); tensor=$tensor; backportComparison=@($backportComparison) }
$output = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($OutputPath)
New-Item -ItemType Directory -Force (Split-Path $output) | Out-Null
[IO.File]::WriteAllText($output,($result | ConvertTo-Json -Depth 50).Replace("`r`n","`n") + "`n")
Write-Host "PASS source comparison: $(@($comparisons).Count) files, $(@($catalogue).Count) library editions; report $output"
