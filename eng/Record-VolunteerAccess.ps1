[CmdletBinding()]
param(
  [Parameter(Mandatory)][string]$Playwright,
  [Parameter(Mandatory)][string]$Chromium,
  [string]$Net10Dotnet = (Get-Command dotnet -CommandType Application | Select-Object -First 1).Source,
  [string]$Net11Dotnet = (Get-Command dotnet -CommandType Application | Select-Object -First 1).Source,
  [string]$Node = (Get-Command node -CommandType Application | Select-Object -First 1).Source
)
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$root = Split-Path $PSScriptRoot -Parent
. (Join-Path $PSScriptRoot 'Evidence.ps1')
$paths = @{}
foreach ($scope in 'Process','User','Machine') { $paths[$scope] = [Environment]::GetEnvironmentVariable('PATH',$scope) }
$previous = $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
try {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
  $publication = Join-Path $root 'artifacts/web-migrations'
  & "$PSScriptRoot/Test-WebMigrationBatch.ps1" -Dotnet $Net10Dotnet -Net11Dotnet $Net11Dotnet -OutputDirectory $publication -RecordEvidence
  $report = Join-Path $root 'artifacts/volunteer-browser/browser.json'
  & $Node "$PSScriptRoot/Test-VolunteerAccess.mjs" $Playwright $Chromium $Net10Dotnet $Net11Dotnet $publication $report
  foreach ($major in 10,11) {
    $directory = Join-Path $root "samples/net$major.0/VolunteerAccess"
    $record = Get-Content "$directory/verification-evidence.json" -Raw | ConvertFrom-Json -AsHashtable
    $null = Read-VerificationEvidence $root @{sample=$record.sample;target=$record.target;sdk=$record.sdk;language=$record.language;kind='compile';scope='Matching compilation prerequisite.'}
    $record.kind = 'execution'
    $record.recordedAt = [DateTimeOffset]::UtcNow.ToString('O')
    $record.command = "eng/Record-VolunteerAccess.ps1; $Node eng/Test-VolunteerAccess.mjs '$Playwright' '$Chromium' '$Net10Dotnet' '$Net11Dotnet' '$publication' '$report'"
    $record.scope = 'Actual Chromium DOM/layout/assets and login form interactions for both published consumers and the .NET11 property-removal publication. Invalid input, unconfirmed denial, confirmed cookie/session and local return behavior; private fixture processes/data cleaned. No screenshot, other-browser, assistive-technology, mobile or GA claim.'
    $record.output = Get-Content $report -Raw
    [IO.File]::WriteAllText("$directory/browser.evidence.json", ($record | ConvertTo-Json -Depth 40).Replace("`r`n","`n") + "`n")
  }
} finally {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = $previous
  foreach ($scope in $paths.Keys) { if ($paths[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)) { throw "Unexpected $scope PATH change; shared settings preserved." } }
}
