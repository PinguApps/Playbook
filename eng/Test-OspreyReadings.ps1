#Requires -Version 7.3
[CmdletBinding()]
param([switch]$Record)
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$root = Split-Path $PSScriptRoot -Parent
$dotnet = (Get-Command dotnet -CommandType Application | Select-Object -First 1).Source
$shell = (Get-Process -Id $PID).Path
$paths = @{}
foreach ($scope in 'Process','User','Machine') { $paths[$scope] = [Environment]::GetEnvironmentVariable('PATH',$scope) }
$integration = $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
try {
    $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
    & "$PSScriptRoot/SqlServer.ps1" -Action Start
    foreach ($release in '10','11') {
        $directory = Join-Path $root "samples/net$release.0/OspreyReadings"
        $sample = "samples/net$release.0/OspreyReadings/OspreyReadings.csproj"
        $database = 'playbook_osprey_' + [guid]::NewGuid().ToString('N')
        & "$PSScriptRoot/SqlServer.ps1" -Action New -Database $database
        try {
            if ($Record) {
                & "$PSScriptRoot/SqlServer.ps1" -Action Run -Database $database -Executable $shell -WorkingDirectory $root -CommandArguments @('-NoProfile','-File',"$PSScriptRoot/Record-Evidence.ps1",'-Sample',$sample,'-Dotnet',$dotnet)
            } else {
                & "$PSScriptRoot/SqlServer.ps1" -Action Run -Database $database -Executable $dotnet -WorkingDirectory $directory -CommandArguments @('run','-c','Release','-p:RestoreLockedMode=true')
            }
        } finally { & "$PSScriptRoot/SqlServer.ps1" -Action Remove -Database $database }
    }
} finally {
    $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = $integration
    foreach ($scope in $paths.Keys) {
        if ($paths[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)) { throw "Unexpected $scope PATH change; shared settings preserved." }
    }
}
