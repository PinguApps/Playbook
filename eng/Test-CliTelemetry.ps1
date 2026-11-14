[CmdletBinding()]
param(
  [string]$Dotnet = (Get-Command dotnet -CommandType Application | Select-Object -First 1).Source,
  [string]$Net10Dotnet = $Dotnet,
  [string]$Node = (Get-Command node -CommandType Application | Select-Object -First 1).Source,
  [switch]$Record
)
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$root = Split-Path $PSScriptRoot -Parent
. (Join-Path $PSScriptRoot 'Evidence.ps1')
$paths = @{}
foreach ($scope in 'Process','User','Machine') { $paths[$scope] = [Environment]::GetEnvironmentVariable('PATH', $scope) }
$previous = $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
$result = Join-Path $root 'artifacts/ga-readiness/telemetry.json'
$before = @{}
try {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
  foreach ($release in '10','11') {
    $sample = "samples/net$release.0/CliTelemetry/CliTelemetry.csproj"
    $before[$release] = Get-EvidenceInputs $root $sample
    $hostDotnet = if ($release -eq '10') { $Net10Dotnet } else { $Dotnet }
    if ($Record) { & (Join-Path $PSScriptRoot 'Record-Evidence.ps1') -Sample $sample -Dotnet $hostDotnet -Kind compile }
    else {
      Push-Location (Join-Path $root (Split-Path $sample))
      try { & $hostDotnet build CliTelemetry.csproj -c Release -p:RestoreLockedMode=true }
      finally { Pop-Location }
    }
  }
  & $Node (Join-Path $PSScriptRoot 'Test-CliTelemetry.mjs') $Dotnet $Net10Dotnet $result
  if ($Record) {
    foreach ($release in '10','11') {
      $sample = "samples/net$release.0/CliTelemetry/CliTelemetry.csproj"
      $inputs = Get-EvidenceInputs $root $sample
      if ((ConvertTo-Json $before[$release] -Compress) -cne (ConvertTo-Json $inputs -Compress)) { throw 'Telemetry inputs changed during verification.' }
      $directory = Join-Path $root (Split-Path $sample)
      $receipt = Get-Content (Join-Path $directory 'verification-evidence.json') -Raw | ConvertFrom-Json -AsHashtable
      $receipt.kind = 'execution'
      $receipt.command = "eng/Test-CliTelemetry.ps1 -Dotnet '$Dotnet' -Net10Dotnet '$Net10Dotnet' -Node '$Node' -Record"
      $receipt.scope = 'Actual paired SDK opt-out wrappers and GA local OTLP traces/metrics receipt. No global network absence, Azure Monitor behavior, retry or production delivery guarantee.'
      $receipt.recordedAt = [DateTimeOffset]::UtcNow.ToString('O')
      $receipt.inputs = $inputs
      $receipt.output = Get-Content -LiteralPath $result -Raw
      [IO.File]::WriteAllText((Join-Path $directory 'telemetry.evidence.json'), ($receipt | ConvertTo-Json -Depth 40).Replace("`r`n", "`n") + "`n")
    }
  }
}
finally {
  [Environment]::SetEnvironmentVariable('DOTNET_ADD_GLOBAL_TOOLS_TO_PATH', $previous)
  foreach ($scope in $paths.Keys) { if ($paths[$scope] -cne [Environment]::GetEnvironmentVariable('PATH', $scope)) { throw "Unexpected $scope PATH change; shared settings preserved." } }
}
