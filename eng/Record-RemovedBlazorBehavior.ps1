[CmdletBinding()]
param(
  [Parameter(Mandatory)][string]$Playwright,
  [Parameter(Mandatory)][string]$Net10Dotnet,
  [Parameter(Mandatory)][string]$Net11Dotnet,
  [string]$Node = (Get-Command node -CommandType Application | Select-Object -First 1).Source
)

$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
. (Join-Path $PSScriptRoot 'Evidence.ps1')
$root = Split-Path $PSScriptRoot -Parent
$records = @()
foreach ($version in @('10.0', '11.0')) {
  foreach ($name in @('TidalStations', 'SailRepairs')) {
    $sample = "samples/net$version/$name/$name.csproj"
    $record = Get-Content -LiteralPath (Join-Path (Split-Path (Join-Path $root $sample)) 'verification-evidence.json') -Raw | ConvertFrom-Json -AsHashtable
    $expected = @{ sample = $sample; target = "net$version"; sdk = $record.sdk; language = $record.language; kind = 'compile'; scope = 'Actual target compilation prerequisite.' }
    $null = Read-VerificationEvidence $root $expected
    $records += $record
  }
}
$reportPath = Join-Path $root 'artifacts/removed-api/behavior.json'
& $Node (Join-Path $PSScriptRoot 'Test-RemovedBlazorConsumers.mjs') $Playwright $Net10Dotnet $Net11Dotnet $reportPath
$report = Get-Content -LiteralPath $reportPath -Raw | ConvertFrom-Json
$previous = $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
try {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
  foreach ($record in $records) {
    $directory = Split-Path (Join-Path $root $record.sample)
    $name = Split-Path $directory -Leaf
    $dotnet = if ($record.target -eq 'net10.0') { $Net10Dotnet } else { $Net11Dotnet }
    $execution = @($report.results | Where-Object { "net$($_.version)" -eq $record.target -and $_.sample -eq $name })
    if ($execution.Count -ne 1) { throw "Missing actual browser result for $($record.sample)." }
    $output = 'PASS: Chromium ' + $report.browser + "`n" + ($execution[0] | ConvertTo-Json -Depth 10)
    $command = "& '$Node' '$PSScriptRoot/Test-RemovedBlazorConsumers.mjs' '$Playwright' '$Net10Dotnet' '$Net11Dotnet' '$reportPath'"
    if ($name -eq 'SailRepairs') {
      $dll = Join-Path $directory "bin/Release/$($record.target)/SailRepairs.dll"
      $output += "`n" + (& $dotnet $dll --verify | Out-String)
      $command += "; & '$dotnet' '$dll' --verify"
    }
    $after = Get-EvidenceInputs $root $record.sample
    if (($after | ConvertTo-Json -Compress) -cne ($record.inputs | ConvertTo-Json -Compress)) { throw 'Inputs changed during actual execution.' }
    $record.kind = 'execution'
    $record.command = $command
    $record.scope = if ($name -eq 'SailRepairs') { 'Real scoped DI, field/submit validation, one handler/message, three replacements, detached old subscriptions and final disposal; .NET 10 providerless failure fixtures. Synchronous catalogue rule; no generated graph or async-rule claim.' } else { 'Real HTTP static SSR exact/parameter routes and live Interactive Server navigation with prerender disabled. Asset constructor calls are compilation-only evidence.' }
    $record.recordedAt = [DateTimeOffset]::UtcNow.ToString('O')
    $record.output = $output
    $record.sourceCommit = (& (Get-Command git -CommandType Application).Source -C $root rev-parse HEAD).Trim()
    $record.sourceDirty = [bool](& (Get-Command git -CommandType Application).Source -C $root status --porcelain)
    $record | ConvertTo-Json -Depth 30 | Set-Content -LiteralPath (Join-Path $directory 'behavior.evidence.json') -Encoding utf8
  }
}
finally { $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = $previous }
Write-Host 'PASS: four fresh execution records captured with unchanged canonical inputs.'
