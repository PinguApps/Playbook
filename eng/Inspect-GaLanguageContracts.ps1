[CmdletBinding()]
param(
  [string]$DotnetRoot = 'C:/Users/pingu/AppData/Local/dotnetup/dotnet',
  [string]$OutputPath = (Join-Path $PSScriptRoot '../artifacts/ga-language-contracts.json')
)

# Source/artifact audit only. Never installs, executes SDKs, or rewrites RC evidence.
$ErrorActionPreference = 'Stop'
$ga = '297acf930fb0f5c07fd4e51d78e521b4ad2e5d1f'
$rc = '820230c386db5b0106c5430b3e5aee92a7826bf5'
$sources = [Collections.Generic.List[object]]::new()
function Read-Source([string]$Url) {
  $text = (Invoke-WebRequest $Url).Content
  $hash = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData([Text.Encoding]::UTF8.GetBytes($text))).ToLowerInvariant()
  $sources.Add([pscustomobject]@{ url = $Url; sha256 = $hash })
  return $text
}
$notes = foreach ($path in @('src/roslyn/docs/compilers/CSharp/Compiler%20Breaking%20Changes%20-%20DotNet%2011.md', 'src/roslyn/src/Compilers/VisualBasic/Portable/LanguageVersion.vb')) {
  $null = Read-Source "https://raw.githubusercontent.com/dotnet/dotnet/$ga/$path"
}
$release = Invoke-RestMethod 'https://dotnetcli.blob.core.windows.net/dotnet/release-metadata/11.0/releases.json'
if (-not ($release.releases | Where-Object 'release-version' -EQ '11.0.0')) { throw 'Actual GA release is required.' }
$sdkRoot = Join-Path $DotnetRoot 'sdk/11.0.100'
if (-not ((Get-Content (Join-Path $sdkRoot '.version') -Raw).Contains($ga))) { throw 'Installed GA SDK source identity mismatch.' }
$artifacts = foreach ($path in @('Roslyn/bincore/Microsoft.CodeAnalysis.dll', 'Roslyn/bincore/Microsoft.CodeAnalysis.CSharp.dll', 'Roslyn/bincore/Microsoft.CodeAnalysis.VisualBasic.dll', 'FSharp/FSharp.Core.dll', 'FSharp/FSharp.Compiler.Service.dll')) {
  $file = Join-Path $sdkRoot $path
  [pscustomobject]@{ path = $path; productVersion = [Diagnostics.FileVersionInfo]::GetVersionInfo($file).ProductVersion; sha256 = (Get-FileHash -LiteralPath $file -Algorithm SHA256).Hash.ToLowerInvariant() }
}
$api = foreach ($area in @('Core', 'CSharp', 'VisualBasic')) {
  $sets = @{}
  foreach ($ref in @($rc, $ga)) {
    $lines = foreach ($kind in @('Shipped', 'Unshipped')) {
      (Read-Source "https://raw.githubusercontent.com/dotnet/dotnet/$ref/src/roslyn/src/Compilers/$area/Portable/PublicAPI.$kind.txt") -split "`n" | Where-Object { $_ -and -not $_.StartsWith('#') }
    }
    $sets[$ref] = @($lines | Sort-Object -Unique)
  }
  $delta = @(Compare-Object $sets[$rc] $sets[$ga])
  [pscustomobject]@{ area = $area; added = @($delta | Where-Object SideIndicator -EQ '=>' | ForEach-Object InputObject); removed = @($delta | Where-Object SideIndicator -EQ '<=' | ForEach-Object InputObject) }
}
$fsharp = foreach ($source in (Get-Content (Join-Path $PSScriptRoot '../docs/planning/audits/fsharp-contracts.sources.json') -Raw | ConvertFrom-Json).sources) {
  if ($source.url -notmatch $rc) { continue }
  $old = Read-Source $source.url
  $new = Read-Source $source.url.Replace($rc, $ga)
  [pscustomobject]@{ path = $source.path; unchanged = $old -ceq $new; added = @((Compare-Object ($old -split "`n") ($new -split "`n")) | Where-Object SideIndicator -EQ '=>' | ForEach-Object InputObject); removed = @((Compare-Object ($old -split "`n") ($new -split "`n")) | Where-Object SideIndicator -EQ '<=' | ForEach-Object InputObject) }
}
$catalogue = foreach ($file in Get-ChildItem (Join-Path $PSScriptRoot '../content/recipes') -Recurse -Filter recipe.json) {
  $recipe = Get-Content -LiteralPath $file.FullName -Raw | ConvertFrom-Json
  if ($recipe.edition -ne '11.0' -or $recipe.id -notmatch '^(csharp|fsharp|languages)\.') { continue }
  [pscustomobject]@{ id = $recipe.id; stability = $recipe.stability; metadataHash = (Get-FileHash -LiteralPath $file.FullName).Hash.ToLowerInvariant(); markdownHash = (Get-FileHash -LiteralPath (Join-Path $file.DirectoryName 'recipe.md')).Hash.ToLowerInvariant() }
}
$result = [ordered]@{ scope = 'GA language/compiler source and bundled artifact reconciliation; no consumer execution'; gaSource = $ga; rcSource = $rc; sdk = '11.0.100'; runtime = '11.0.0'; artifacts = @($artifacts); roslynApi = @($api); fsharpSources = @($fsharp); catalogue = @($catalogue | Sort-Object id); sources = @($sources) }
$output = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($OutputPath)
New-Item -ItemType Directory -Force (Split-Path $output -Parent) | Out-Null
$result | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath $output -Encoding utf8
Write-Host "PASS: GA source/artifact audit; $($catalogue.Count) catalogue editions. Output: $output"
