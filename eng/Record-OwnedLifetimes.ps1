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
foreach ($scope in 'Process','User','Machine') { $paths[$scope] = [Environment]::GetEnvironmentVariable('PATH',$scope) }
try {
  & (Join-Path $PSScriptRoot 'Test-OwnedLifetimes.ps1') -Net10Dotnet $Net10Dotnet -Net11Dotnet $Net11Dotnet
  $records = foreach ($major in 10,11) {
    foreach ($name in 'ReadingRoom','RestorationBench') {
      Get-Content -LiteralPath (Join-Path $root "samples/net$major.0/$name/verification-evidence.json") -Raw | ConvertFrom-Json -AsHashtable
    }
  }
  $reportPath = Join-Path $root 'artifacts/owned-lifetimes/browser.json'
  & $Node (Join-Path $PSScriptRoot 'Test-OwnedLifetimes.mjs') $Playwright $Chromium $Net10Dotnet $Net11Dotnet $reportPath
  $report = Get-Content -LiteralPath $reportPath -Raw | ConvertFrom-Json
  $analyzers = Get-Content -LiteralPath (Join-Path $root 'artifacts/owned-lifetimes/analyzers.json') -Raw | ConvertFrom-Json
  foreach ($record in $records) {
    $directory = Split-Path (Join-Path $root $record.sample)
    $name = Split-Path $directory -Leaf
    $version = $record.target.Substring(3)
    $dotnet = if ($version -eq '10.0') { $Net10Dotnet } else { $Net11Dotnet }
    $pack = if ($version -eq '10.0') { '10.0.13' } else { '11.0.0-rc.2.26504.105' }
    $installation = Split-Path $dotnet -Parent
    $files = @(
      "sdk/$($record.sdk)/Sdks/Microsoft.NET.Sdk.Razor/source-generators/Microsoft.CodeAnalysis.Razor.Compiler.dll",
      "packs/Microsoft.AspNetCore.App.Ref/$pack/analyzers/dotnet/cs/Microsoft.AspNetCore.Components.Analyzers.dll",
      "shared/Microsoft.AspNetCore.App/$pack/Microsoft.AspNetCore.Components.Web.dll",
      "shared/Microsoft.AspNetCore.App/$pack/Microsoft.JSInterop.dll"
    )
    $tooling = foreach ($relative in $files) {
      $file = Join-Path $installation $relative
      @{ file=$file; version=[Diagnostics.FileVersionInfo]::GetVersionInfo($file).ProductVersion; sha256=(Get-FileHash -LiteralPath $file).Hash.ToLowerInvariant() }
    }
    $cases = @($report.results | Where-Object { $_.version -eq $version -and $_.sample -eq $name })
    if ($cases.Count -ne 1) { throw 'Missing actual browser execution.' }
    $after = Get-EvidenceInputs $root $record.sample
    if (($after | ConvertTo-Json -Compress) -cne ($record.inputs | ConvertTo-Json -Compress)) { throw 'Inputs changed during execution.' }
    $record.kind = 'execution'
    $record.command = "& '$PSScriptRoot/Record-OwnedLifetimes.ps1' -Playwright '$Playwright' -Chromium '$Chromium' -Net10Dotnet '$Net10Dotnet' -Net11Dotnet '$Net11Dotnet' -Node '$Node'"
    $record.scope = if ($name -eq 'ReadingRoom') {
      'Real Interactive Server module success, actual JS exception/recovery/next-call success, unrelated application error visible through ErrorBoundary; connected cleanup and explicit-disconnect renderer disposal with actual closeReader JSDisconnectedException and separate old/current reference-disposal behavior. Actual BL0016 before/after and enclosing-catch limitation. No timeout, WASM, arbitrary race or GA guarantee.'
    } else {
      'Real rendered file selection and bounded stream read; two mount/remove cycles, delivered status, one subscription cancellation per mount, no callbacks after removal, completed read retained and actual framework callback relay rejection proving base cleanup. Older hook rejected/current hook compiled. No upload rollback, remote storage or GA claim.'
    }
    $record.recordedAt = [DateTimeOffset]::UtcNow.ToString('O')
    $record.output = "PASS: Chromium $($report.browser); Node $($report.node); Playwright core 1.61.1`n" + (@{
      cases=$cases; tooling=@($tooling); controls=@($analyzers | Where-Object target -EQ $record.target)
    } | ConvertTo-Json -Depth 20)
    [IO.File]::WriteAllText((Join-Path $directory 'behavior.evidence.json'), ($record | ConvertTo-Json -Depth 40).Replace("`r`n","`n") + "`n")
  }
  Write-Host 'PASS: four fresh compilation and four real browser/renderer execution records; PATH unchanged.'
} finally {
  foreach ($scope in $paths.Keys) { if ($paths[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)) { throw "Unexpected $scope PATH change; shared settings preserved." } }
}
