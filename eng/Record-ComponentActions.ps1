[CmdletBinding()]
param(
  [Parameter(Mandatory)][string]$Playwright,
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
  $records = @()
  foreach ($version in '10.0','11.0') {
    $sample = "samples/net$version/WillowQuay/WillowQuay.csproj"
    $record = Get-Content -LiteralPath (Join-Path $root "samples/net$version/WillowQuay/verification-evidence.json") -Raw | ConvertFrom-Json -AsHashtable
    $null = Read-VerificationEvidence $root @{ sample=$sample; target="net$version"; sdk=$record.sdk; language=$record.language; kind='compile'; scope='Matching compilation prerequisite.' }
    $records += $record
  }
  $reportPath = Join-Path $root 'artifacts/component-actions/behavior.json'
  & $Node (Join-Path $PSScriptRoot 'Test-ComponentActions.mjs') $Playwright $Net10Dotnet $Net11Dotnet $reportPath
  $report = Get-Content -LiteralPath $reportPath -Raw | ConvertFrom-Json
  foreach ($record in $records) {
    $version = $record.target.Substring(3)
    $dotnet = if ($version -eq '10.0') { $Net10Dotnet } else { $Net11Dotnet }
    $pack = if ($version -eq '10.0') { '10.0.13' } else { '11.0.0-rc.2.26504.105' }
    $installation = Split-Path $dotnet -Parent
    $files = @(
      "sdk/$($record.sdk)/Sdks/Microsoft.NET.Sdk.Razor/source-generators/Microsoft.CodeAnalysis.Razor.Compiler.dll",
      "packs/Microsoft.AspNetCore.App.Ref/$pack/analyzers/dotnet/cs/Microsoft.AspNetCore.Components.Analyzers.dll",
      "shared/Microsoft.AspNetCore.App/$pack/Microsoft.AspNetCore.Components.dll"
    )
    $tooling = foreach ($relative in $files) {
      $file = Join-Path $installation $relative
      "$file | $([Diagnostics.FileVersionInfo]::GetVersionInfo($file).ProductVersion) | SHA256 $((Get-FileHash -LiteralPath $file).Hash.ToLowerInvariant())"
    }
    $execution = @($report.results | Where-Object version -EQ $version)
    if ($execution.Count -ne 3) { throw 'Missing before/after/necessary-render execution.' }
    $after = Get-EvidenceInputs $root $record.sample
    if (($after | ConvertTo-Json -Compress) -cne ($record.inputs | ConvertTo-Json -Compress)) { throw 'Inputs changed during execution.' }
    $record.kind = 'execution'
    $record.command = "& '$PSScriptRoot/Record-ComponentActions.ps1' -Playwright '$Playwright' -Net10Dotnet '$Net10Dotnet' -Net11Dotnet '$Net11Dotnet' -Node '$Node'"
    $record.scope = 'Real Interactive Server browser controls: old-source RC2 BL0012/BL0014/BL0015 and clean migration; first/last loop and foreach; necessary external timer render; actual JS identifier, visibility/input rejection and disposed object reference. No GA or alternate render-mode claim.'
    $record.output = "PASS: Chromium $($report.browser)`n" + ($execution | ConvertTo-Json -Depth 20) + "`n" + ($tooling -join "`n")
    $record.recordedAt = [DateTimeOffset]::UtcNow.ToString('O')
    $record.sourceCommit = (& (Get-Command git -CommandType Application).Source -C $root rev-parse HEAD).Trim()
    $record.sourceDirty = [bool](& (Get-Command git -CommandType Application).Source -C $root status --porcelain)
    [IO.File]::WriteAllText((Join-Path $root "samples/net$version/WillowQuay/behavior.evidence.json"), ($record | ConvertTo-Json -Depth 40).Replace("`r`n","`n") + "`n")
  }
  Write-Host 'PASS: fresh matching-framework browser and analyzer evidence.'
}
finally {
  foreach ($scope in $paths.Keys) { if ($paths[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)) { throw "Unexpected $scope PATH change; shared settings preserved." } }
}
