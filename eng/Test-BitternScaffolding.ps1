#Requires -Version 7.3
[CmdletBinding()]
param([switch]$Record, [string]$Dotnet = 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe')
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$root = Split-Path $PSScriptRoot -Parent
. "$PSScriptRoot/Evidence.ps1"
$paths = @{}
foreach ($scope in 'Process','User','Machine') { $paths[$scope] = [Environment]::GetEnvironmentVariable('PATH',$scope) }
$previous = $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
try {
 $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
 foreach ($v in '10','11') {
  $sample = "samples/net$v.0/BitternScaffolding/BitternScaffolding.csproj"
  $source = Join-Path $root "samples/net$v.0/BitternScaffolding"
  $output = Join-Path ([IO.Path]::GetTempPath()) ('playbook-bittern-' + [guid]::NewGuid().ToString('N'))
  New-Item -ItemType Directory -Path $output | Out-Null
  if ($Record) { & "$PSScriptRoot/Record-Evidence.ps1" -Sample $sample -Dotnet $Dotnet -Kind compile }
  $before = Get-EvidenceInputs $root $sample
  Push-Location $source
  try { $generation = & $Dotnet run -c Release -p:RestoreLockedMode=true -- $output | Out-String } finally { Pop-Location }
  foreach ($f in 'global.json','Directory.Build.props','Directory.Build.targets','Directory.Packages.props') { Copy-Item "$source/$f" "$output/$f" }
  Copy-Item "$source/BitternScaffolding.csproj" "$output/GeneratedConsumer.csproj"
  Copy-Item "$source/Consumer.cs.txt" "$output/Program.cs"
  Push-Location $output
  try { $consumer = & $Dotnet run -c Release -- $output | Out-String } finally { Pop-Location }
  $generation; $consumer
  $after = Get-EvidenceInputs $root $sample
  if (($before | ConvertTo-Json -Depth 100 -Compress) -cne ($after | ConvertTo-Json -Depth 100 -Compress)) { throw 'Inputs changed during execution' }
  if ($Record) {
   $receipt = Get-Content "$source/verification-evidence.json" -Raw | ConvertFrom-Json -AsHashtable
   $receipt.kind = 'execution'
   $receipt.command = "$Dotnet run -c Release -p:RestoreLockedMode=true -- $output; $Dotnet run -c Release -- $output (generated consumer)"
   $receipt.output = $generation + $consumer
   $receipt.scope = 'Windows x64 SQLite generation, actual generated C# compilation and fresh-context query; no GA or other provider proof.'
   $receipt.recordedAt = [DateTimeOffset]::UtcNow.ToString('O')
   $receipt.inputs = $after
   $receipt | ConvertTo-Json -Depth 100 | Set-Content "$source/behavior.evidence.json"
  }
 }
} finally {
 $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = $previous
 foreach ($scope in $paths.Keys) { if ($paths[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)) { throw "Unexpected $scope PATH change; shared settings preserved." } }
}