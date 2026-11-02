#Requires -Version 7.0
[CmdletBinding()]
param(
    [switch]$Record,
    [string]$Net10Dotnet = 'C:\Program Files\dotnet\dotnet.exe',
    [string]$Net11Dotnet = 'C:\Users\pingu\.dotnet-playbook-11\dotnet.exe'
)
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$root = Split-Path $PSScriptRoot -Parent
$paths = @{}
foreach ($scope in 'Process','User','Machine') { $paths[$scope] = [Environment]::GetEnvironmentVariable('PATH',$scope) }
$previous = $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
$route = Join-Path $PSScriptRoot 'SqlServer.ps1'
$shell = (Get-Process -Id $PID).Path
$started = $false
try {
    $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
    & $route -Action Start
    $started = $true
    foreach ($release in '10','11') {
        $dotnet = if ($release -eq '10') { $Net10Dotnet } else { $Net11Dotnet }
        $sample = "samples/net$release.0/QuarrySearch/QuarrySearch.csproj"
        $directory = Join-Path $root (Split-Path $sample)
        $database = 'playbook_search_' + [guid]::NewGuid().ToString('N')
        & $route -Action New -Database $database
        try {
            if ($Record) {
                & $route -Action Run -Database $database -Executable $shell -WorkingDirectory $root `
                    -CommandArguments @('-NoProfile','-File',(Join-Path $PSScriptRoot 'Record-Evidence.ps1'),'-Sample',$sample,'-Dotnet',$dotnet)
            }
            else {
                & $route -Action Run -Database $database -Executable $dotnet -WorkingDirectory $directory `
                    -CommandArguments @('run','--project','QuarrySearch.csproj','-c','Release','-p:RestoreLockedMode=true','--no-launch-profile')
            }
        }
        finally { & $route -Action Remove -Database $database }
    }
}
finally {
    if ($started) { & $route -Action Stop }
    $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = $previous
    foreach ($scope in $paths.Keys) {
        if ($paths[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)) { throw "Unexpected $scope PATH change; shared settings preserved." }
    }
}

