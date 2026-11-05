#Requires -Version 7.3
[CmdletBinding()]
param([switch]$Record)
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$root = Split-Path $PSScriptRoot -Parent
$paths = @{}
foreach ($scope in 'Process','User','Machine') { $paths[$scope] = [Environment]::GetEnvironmentVariable('PATH',$scope) }
$previous = $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
try {
    $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
    foreach ($release in '10','11') {
        $dotnet = if ($release -eq '10') { (Get-Command dotnet -CommandType Application | Select-Object -First 1).Source } else { (Get-Command dotnet -CommandType Application | Select-Object -First 1).Source }
        $sample = "samples/net$release.0/WrenTracking/WrenTracking.csproj"
        if ($Record) { & "$PSScriptRoot/Record-Evidence.ps1" -Sample $sample -Dotnet $dotnet }
        else {
            Push-Location (Join-Path $root "samples/net$release.0/WrenTracking")
            try { & $dotnet run -c Release -p:RestoreLockedMode=true }
            finally { Pop-Location }
        }
    }
}
finally {
    $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = $previous
    foreach ($scope in $paths.Keys) {
        if ($paths[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)) { throw "Unexpected $scope PATH change; shared settings preserved." }
    }
}
