#Requires -Version 7.4
[CmdletBinding()]
param(
  [switch]$Record,
  [string]$Net10Dotnet = 'C:/Program Files/dotnet/dotnet.exe',
  [string]$Net11Dotnet = 'C:/Users/pingu/.dotnet-playbook-11/dotnet.exe'
)
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$root = Split-Path $PSScriptRoot -Parent
$before = @{}
foreach ($scope in 'Process','User','Machine') { $before[$scope] = [Environment]::GetEnvironmentVariable('PATH', $scope) }
$previous = $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
try {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
  foreach ($version in 10,11) {
    $dotnet = if ($version -eq 10) { $Net10Dotnet } else { $Net11Dotnet }
    foreach ($name in 'FenStatus','FenStartup','FenStartupVb','FenBatch') {
      $extension = if ($name -eq 'FenStartupVb') { 'vbproj' } else { 'csproj' }
      $sample = "samples/net$version.0/$name/$name.$extension"
      if ($Record) { & "$PSScriptRoot/Record-Evidence.ps1" -Sample $sample -Dotnet $dotnet }
      else {
        Push-Location (Join-Path $root "samples/net$version.0/$name")
        try { & $dotnet build "$name.$extension" -c Release -p:RestoreLockedMode=true }
        finally { Pop-Location }
        $dll = Join-Path $root "samples/net$version.0/$name/bin/Release/net$version.0-windows/$name.dll"
        & $dotnet $dll
      }
    }
  }
}
finally {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = $previous
  foreach ($scope in 'Process','User','Machine') {
    if ([Environment]::GetEnvironmentVariable('PATH', $scope) -cne $before[$scope]) { throw "PATH changed at $scope scope." }
  }
}
