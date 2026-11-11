#Requires -Version 7.0
[CmdletBinding()]
param(
  [Parameter(Mandatory)][string]$Dotnet,
  [Parameter(Mandatory)][string]$Node,
  [Parameter(Mandatory)][string]$PlaywrightModule,
  [switch]$Record
)
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$root = Split-Path $PSScriptRoot -Parent
. (Join-Path $PSScriptRoot 'Evidence.ps1')
$saved = @{}
foreach ($scope in 'Process','User','Machine') { $saved[$scope] = [Environment]::GetEnvironmentVariable('PATH',$scope) }
$previous = @{}
foreach ($key in 'DOTNET_ADD_GLOBAL_TOOLS_TO_PATH','DOTNET_GENERATE_ASPNET_CERTIFICATE') { $previous[$key] = [Environment]::GetEnvironmentVariable($key) }
try {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
  $env:DOTNET_GENERATE_ASPNET_CERTIFICATE = 'false'
  foreach ($name in 'MuseumCaptions','ObservatoryLog') {
    & (Join-Path $PSScriptRoot 'Record-Evidence.ps1') -Sample "samples/net11.0/$name/$name.csproj" -Dotnet $Dotnet -Kind compile
  }
  $inputs = @{}
  foreach ($name in 'MuseumCaptions','ObservatoryLog') { $inputs[$name] = Get-EvidenceInputs $root "samples/net11.0/$name/$name.csproj" }
  $renderCommit = (& 'C:/Program Files/Git/cmd/git.exe' -C $root rev-parse HEAD).Trim()
  $renderDirty = [bool](& 'C:/Program Files/Git/cmd/git.exe' -C $root status --porcelain)
  $output = (& $Node (Join-Path $PSScriptRoot 'Test-AgentViews.cjs') $PlaywrightModule $Dotnet | Out-String).Trim()
  Write-Host $output
  if ($Record) {
    foreach ($name in 'MuseumCaptions','ObservatoryLog') {
      $sample = "samples/net11.0/$name/$name.csproj"
      if (($inputs[$name] | ConvertTo-Json -Compress) -cne ((Get-EvidenceInputs $root $sample) | ConvertTo-Json -Compress)) { throw 'Inputs changed during rendered checks' }
      $path = Join-Path $root "samples/net11.0/$name/verification-evidence.json"
      $receipt = Get-Content $path -Raw | ConvertFrom-Json -AsHashtable
      $receipt.kind = 'execution'
      $receipt.command = "$Node eng/Test-AgentViews.cjs $PlaywrightModule $Dotnet"
      $receipt.scope = 'Real Chromium Interactive Server renderer and actual experimental AI/storage/protocol packages; scripted local endpoint/text source only. No remote model, provider, WASM/Hybrid or performance proof.'
      $receipt.recordedAt = [DateTimeOffset]::UtcNow.ToString('O')
      $receipt.sourceCommit = $renderCommit
      $receipt.sourceDirty = $renderDirty
      $receipt.output = @($output -split '\r?\n' | Where-Object { if ($name -eq 'MuseumCaptions') { $_ -like 'PASS: rendered typed*' } else { $_ -like 'PASS: rendered completed*' } }) -join "`n"
      if (-not $receipt.output) { throw "Missing rendered result for $name" }
      [IO.File]::WriteAllText((Join-Path $root "samples/net11.0/$name/rendered.evidence.json"), ($receipt | ConvertTo-Json -Depth 40).Replace("`r`n","`n")+"`n")
    }
  }
} finally {
  foreach ($key in $previous.Keys) { [Environment]::SetEnvironmentVariable($key,$previous[$key]) }
  foreach ($scope in $saved.Keys) { if ($saved[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)) { throw "Unexpected $scope PATH change; shared settings preserved" } }
}
