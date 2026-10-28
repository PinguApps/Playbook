[CmdletBinding()]
param(
  [Parameter(Mandatory)][string]$Playwright,
  [Parameter(Mandatory)][string]$Chromium,
  [string]$Net10Dotnet = 'C:\Program Files\dotnet\dotnet.exe',
  [string]$Net11Dotnet = 'C:\Users\pingu\.dotnet-playbook-11\dotnet.exe',
  [string]$Node = 'C:\Program Files\nodejs\node.exe'
)
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
. (Join-Path $PSScriptRoot 'Evidence.ps1')
$root = Split-Path $PSScriptRoot -Parent
$paths = @{}
foreach ($scope in 'Process','User','Machine') { $paths[$scope] = [Environment]::GetEnvironmentVariable('PATH', $scope) }
$previous = $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
$records = @()
try {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
  foreach ($major in 10,11) {
    $dotnet = if ($major -eq 10) { $Net10Dotnet } else { $Net11Dotnet }
    foreach ($name in 'GardenAccession','LibraryBadges') {
      $sample = "samples/net$major.0/$name/$name.csproj"
      & (Join-Path $PSScriptRoot 'Record-Evidence.ps1') -Sample $sample -Dotnet $dotnet -Kind compile
      $records += Get-Content -LiteralPath (Join-Path $root "samples/net$major.0/$name/verification-evidence.json") -Raw | ConvertFrom-Json -AsHashtable
    }
  }
  $reportPath = Join-Path $root 'artifacts/garden-library/behavior.json'
  & $Node (Join-Path $PSScriptRoot 'Test-GardenLibrary.mjs') $Playwright $Chromium $Net10Dotnet $Net11Dotnet $reportPath
  $report = Get-Content -LiteralPath $reportPath -Raw | ConvertFrom-Json
  foreach ($record in $records) {
    $directory = Split-Path (Join-Path $root $record.sample)
    $name = Split-Path $directory -Leaf
    $cases = @($report.results | Where-Object { "net$($_.version)" -eq $record.target -and $_.sample -eq $name })
    if (-not $cases.Count) { throw "Missing actual execution for $($record.sample)" }
    $after = Get-EvidenceInputs $root $record.sample
    if (($after | ConvertTo-Json -Compress) -cne ($record.inputs | ConvertTo-Json -Compress)) { throw 'Inputs changed during execution.' }
    $artifacts = foreach ($suffix in '.dll','.deps.json','.runtimeconfig.json') {
      $path = Join-Path $directory "bin/Release/$($record.target)/$name$suffix"
      @{ file = "$name$suffix"; sha256 = (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash.ToLowerInvariant() }
    }
    $record.kind = 'execution'
    $record.command = "& '$Node' '$PSScriptRoot/Test-GardenLibrary.mjs' '$Playwright' '$Chromium' '$Net10Dotnet' '$Net11Dotnet' '$reportPath'"
    $record.scope = if ($name -eq 'GardenAccession') {
      'Actual Chromium static SSR enhanced/plain validation, rule mapping, valid/invalid direct POSTs, bypass and .NET 11 per-form/global opt-outs. One root synchronous custom rule.'
    } else {
      'Actual cookie-authenticated SSR requests for two fictional users; lifecycle counts, public reuse, live output, captured parameter replay and .NET 11 Throw/User condition refusal/isolation. No HybridCache claim.'
    }
    $record.recordedAt = [DateTimeOffset]::UtcNow.ToString('O')
    $record.output = 'PASS: Chromium ' + $report.browser + "`n" + (@{ cases = $cases; builtArtifacts = @($artifacts) } | ConvertTo-Json -Depth 15)
    $record | ConvertTo-Json -Depth 35 | Set-Content -LiteralPath (Join-Path $directory 'behavior.evidence.json') -Encoding utf8
  }
}
finally {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = $previous
  foreach ($scope in 'Process','User','Machine') {
    if ([Environment]::GetEnvironmentVariable('PATH', $scope) -cne $paths[$scope]) { throw "Unexpected $scope PATH change; shared setting preserved for inspection." }
  }
}
Write-Host 'PASS: four compilation and four focused real execution records; PATH unchanged.'

