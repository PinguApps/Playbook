#Requires -Version 7.3
[CmdletBinding()]
param(
    [switch]$Record,
    [string]$Net10Dotnet = (Get-Command dotnet -CommandType Application | Select-Object -First 1).Source,
    [string]$Net11Dotnet = (Get-Command dotnet -CommandType Application | Select-Object -First 1).Source,
    [string]$Docker = 'C:\Program Files\Docker\Docker\resources\bin\docker.exe'
)
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$root = Split-Path $PSScriptRoot -Parent
$route = Join-Path $PSScriptRoot 'Cosmos.ps1'
$paths = @{}
foreach ($scope in 'Process','User','Machine') { $paths[$scope] = [Environment]::GetEnvironmentVariable('PATH',$scope) }
$previous = $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
$started = $false
$failure = $null
try {
    $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
    & $route -Docker $Docker -Action Start
    $started = $true
    foreach ($sampleName in 'HarbourQueries') {
    foreach ($release in '10','11') {
        $dotnet = if ($release -eq '10') { $Net10Dotnet } else { $Net11Dotnet }
        $directory = Join-Path $root "samples/net$release.0/$sampleName"
        $database = 'playbook_queries_' + [guid]::NewGuid().ToString('N')
        & $route -Docker $Docker -Action New -Database $database
        try {
            if ($Record) {
                & $route -Docker $Docker -Action Run -Database $database -Executable (Get-Process -Id $PID).Path -WorkingDirectory $root -CommandArguments @('-NoProfile','-File',"$PSScriptRoot/Record-Evidence.ps1",'-Sample',"samples/net$release.0/$sampleName/$sampleName.csproj",'-Dotnet',$dotnet)
            }
            else {
                & $route -Docker $Docker -Action Run -Database $database -Executable $dotnet -WorkingDirectory $directory -CommandArguments @('run','-c','Release','-p:RestoreLockedMode=true')
            }
        }
        finally { & $route -Docker $Docker -Action Remove -Database $database }
    }
    }
}
catch { $failure = $_; throw }
finally {
    try { if ($started) { & $route -Docker $Docker -Action Stop } }
    catch {
        if ($failure) { Write-Warning "Cleanup also failed: $_" }
        else { $failure = $_; throw }
    }
    finally {
        $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = $previous
        foreach ($scope in $paths.Keys) {
            if ($paths[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)) {
                $message = "Unexpected $scope PATH change; shared settings preserved."
                if ($failure) { Write-Warning $message } else { throw $message }
            }
        }
    }
}

