[CmdletBinding()]
param(
  [Parameter(Mandatory)][string]$Chromium,
  [string]$Net10Dotnet = 'C:\Program Files\dotnet\dotnet.exe',
  [string]$Net11Dotnet = 'C:\Users\pingu\.dotnet-playbook-11\dotnet.exe',
  [string]$Node = (Get-Command node -CommandType Application | Select-Object -First 1).Source
)
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
. (Join-Path $PSScriptRoot 'Evidence.ps1')
$root = Split-Path $PSScriptRoot -Parent
$before = @{}
foreach ($scope in 'Process','User','Machine') { $before[$scope] = [Environment]::GetEnvironmentVariable('PATH',$scope) }
$previous = $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
try {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
  $playwrightRoot = Join-Path $root 'artifacts/cedar-payload/browser-tools'
  $entry = Join-Path $playwrightRoot 'node_modules/playwright-core/index.mjs'
  $npm = Join-Path (Split-Path $Node -Parent) 'node_modules/npm/bin/npm-cli.js'
  & $Node $npm install --prefix $playwrightRoot --ignore-scripts --no-audit --no-fund 'playwright-core@1.61.1'
  $records = @()
  foreach ($major in 10,11) {
    $dotnet = if ($major -eq 10) { $Net10Dotnet } else { $Net11Dotnet }
    $sample = "samples/net$major.0/CedarPlanetarium/CedarPlanetarium.csproj"
    & (Join-Path $PSScriptRoot 'Record-Evidence.ps1') -Sample $sample -Dotnet $dotnet -Kind compile
    $records += Get-Content -LiteralPath (Join-Path (Split-Path (Join-Path $root $sample)) 'verification-evidence.json') -Raw | ConvertFrom-Json -AsHashtable
  }
  $output = Join-Path $root 'artifacts/cedar-payload/browser.json'
  & $Node (Join-Path $PSScriptRoot 'Test-CedarPayload.mjs') $entry $Chromium $Net10Dotnet $Net11Dotnet $output
  $report = Get-Content -LiteralPath $output -Raw | ConvertFrom-Json
  foreach ($record in $records) {
    $major = if ($record.target -eq 'net10.0') { 10 } else { 11 }
    $results = @($report.results | Where-Object major -EQ $major)
    if ($results.Count -ne 1) { throw 'Incomplete real browser results.' }
    $after = Get-EvidenceInputs $root $record.sample
    if (($after | ConvertTo-Json -Compress) -cne ($record.inputs | ConvertTo-Json -Compress)) { throw 'Inputs changed during execution.' }
    $record.kind = 'execution'
    $record.command += "; & '$Node' '$PSScriptRoot/Test-CedarPayload.mjs' '$entry' '$Chromium' '$Net10Dotnet' '$Net11Dotnet' '$output'"
    $record.scope = 'Real Interactive Server JS interop and DI protected session storage; app record and unhandled reflection fallback, ElementReference converter, two isolated circuits/tabs and storage, ciphertext tampering; RC2 resolver precedence and separate per-scope options. Ephemeral local Data Protection only; no AOT, full protocol or GA proof.'
    $record.recordedAt = [DateTimeOffset]::UtcNow.ToString('O')
    $record.output = "Chromium $($report.browser); Node $($report.node); Playwright core 1.61.1`n" + ($results | ConvertTo-Json -Depth 30)
    $record | ConvertTo-Json -Depth 40 | Set-Content -LiteralPath (Join-Path (Split-Path (Join-Path $root $record.sample)) 'behavior.evidence.json') -Encoding utf8
  }
  Write-Host 'PASS: two unchanged-input real circuit/browser execution records saved.'
} finally {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = $previous
  foreach ($scope in $before.Keys) { if ($before[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)) { throw "Unexpected $scope PATH change; shared settings preserved." } }
}
