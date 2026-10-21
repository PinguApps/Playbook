[CmdletBinding()]
param([string]$Net11Dotnet = 'C:\Users\pingu\.dotnet-playbook-11\dotnet.exe')

$ErrorActionPreference = 'Stop'
$dotnet = (Get-Command $Net11Dotnet -CommandType Application | Select-Object -First 1).Source
$root = Split-Path $PSScriptRoot -Parent
$previousIntegration = $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
$previousNativePreference = $PSNativeCommandUseErrorActionPreference
Push-Location (Join-Path $root 'samples/net11.0/OptionsStartup')
try {
  $PSNativeCommandUseErrorActionPreference = $false
  $output = (& $dotnet build -c Release -p:RestoreLockedMode=true -p:DefineConstants=EXPLICIT_VALIDATE 2>&1 | Out-String)
  $code = $LASTEXITCODE
  if ($code -eq 0 -or $output -notmatch 'SYSLIB1205') { throw "Expected the explicit/generated ownership conflict SYSLIB1205. Actual exit=$code`n$output" }
  Write-Host 'PASS: actual options generator rejects explicit/generated synchronous ownership with SYSLIB1205.'
  $PSNativeCommandUseErrorActionPreference = $true
  & $dotnet build -c Release -p:RestoreLockedMode=true
  & $dotnet ./bin/Release/net11.0/OptionsStartup.dll
  Write-Host 'PASS: generated ownership restored; actual generated range validator rejects invalid crew.'
}
finally {
  $PSNativeCommandUseErrorActionPreference = $previousNativePreference
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = $previousIntegration
  Pop-Location
}
