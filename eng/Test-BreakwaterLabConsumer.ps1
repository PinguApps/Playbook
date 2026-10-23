[CmdletBinding()]
param(
  [string]$Inspection = (Join-Path (Split-Path $PSScriptRoot -Parent) 'artifacts/pin801-803/inspection'),
  [string]$Results = (Join-Path (Split-Path $PSScriptRoot -Parent) 'artifacts/pin801-803/consumer'),
  [string]$Dotnet = (Get-Command dotnet -CommandType Application | Select-Object -First 1).Source,
  [switch]$KeepRunning
)
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$Results = [IO.Path]::GetFullPath($Results)
$samples = Join-Path ([IO.Path]::GetTempPath()) ('breakwater-returned-markdown-' + [guid]::NewGuid().ToString('N'))
$files = @{}
$pattern = '(?ms)^#### \.NET (?<major>10|11): `(?<path>[^`]+)`\r?\n\r?\n```[a-z]+\r?\n(?<code>.*?)\r?\n```'
$before = @{}
foreach ($scope in @('Process','User','Machine')) { $before[$scope]=[Environment]::GetEnvironmentVariable('PATH',$scope) }
$previous = $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
try {
  foreach ($id in @('blazor.customize-injected-properties','blazor.render-mathematical-markup','blazor.use-void-interop-intent')) {
    $recipe = Get-Content (Join-Path $Inspection "$id.json") -Raw | ConvertFrom-Json
    $matches = [regex]::Matches($recipe.markdown,$pattern)
    if ($matches.Count -ne 22) { throw "Expected 22 complete returned files for $id; got $($matches.Count)." }
    foreach ($match in $matches) {
      $relative = "net$($match.Groups['major'].Value).0/BreakwaterLab/$($match.Groups['path'].Value)"
      if ($relative.Contains('..') -or $relative.Contains(':') -or $relative.Contains('\')) { throw 'Unexpected returned file path.' }
      $code = $match.Groups['code'].Value.Replace("`r`n","`n") + "`n"
      if ($files.ContainsKey($relative) -and $files[$relative] -cne $code) { throw "Recipe comparisons disagree for $relative." }
      $files[$relative] = $code
    }
  }
  foreach ($relative in $files.Keys) {
    $path = Join-Path $samples $relative
    New-Item -ItemType Directory -Path (Split-Path $path) -Force | Out-Null
    [IO.File]::WriteAllText($path,$files[$relative])
  }
  New-Item -ItemType Directory -Path $Results -Force | Out-Null
  $records = @()
  foreach ($major in @(10,11)) {
    $directory = Join-Path $samples "net$major.0/BreakwaterLab"
    Push-Location $directory
    try {
      $sdk = (& $Dotnet --version | Out-String).Trim()
      $pin = Get-Content global.json -Raw | ConvertFrom-Json
      if ($sdk -cne $pin.sdk.version) { throw "Wrong reconstruction SDK: $sdk" }
      & $Dotnet build BreakwaterLab.csproj -c Release
      $assets = Get-Content obj/project.assets.json -Raw | ConvertFrom-Json -AsHashtable
      $records += @{ target="net$major.0"; sdk=$sdk; dependencies=@($assets.libraries.Keys); dllSha256=(Get-FileHash "bin/Release/net$major.0/BreakwaterLab.dll").Hash.ToLowerInvariant() }
    } finally { Pop-Location }
  }
  $inputs = @{}
  foreach ($relative in $files.Keys) { $inputs[$relative]=(Get-FileHash (Join-Path $samples $relative)).Hash.ToLowerInvariant() }
  @{ kind='returned-markdown-reconstruction-and-compilation'; samples=$samples; files=$inputs; consumers=$records; source='All 22 file bodies extracted from each of three actual installed get_recipe responses and checked equal. No canonical checkout files copied.'; recordedAt=[DateTimeOffset]::UtcNow.ToString('O') } | ConvertTo-Json -Depth 10 | Set-Content (Join-Path $Results 'reconstruction.evidence.json')
  & (Join-Path $PSScriptRoot 'Test-BreakwaterLab.ps1') -Samples $samples -Results $Results -Dotnet $Dotnet -StartPort 5994 -KeepRunning:$KeepRunning
}
finally {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = $previous
  foreach ($scope in $before.Keys) { if ($before[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)) { throw "Unexpected $scope PATH change; shared settings preserved." } }
}
