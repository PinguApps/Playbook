[CmdletBinding()]
param([switch]$Record,
    [string]$Net10Dotnet = (Get-Command dotnet -CommandType Application | Select-Object -First 1).Source,
    [string]$Net11Dotnet = $Net10Dotnet)

$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$root = Split-Path $PSScriptRoot -Parent
$paths = @{}
foreach ($scope in 'Process','User','Machine') { $paths[$scope] = [Environment]::GetEnvironmentVariable('PATH',$scope) }
$previous = $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
$route = Join-Path $PSScriptRoot 'SqlServer.ps1'
$shell = (Get-Process -Id $PID).Path
$testFailure = $null
try {
    $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
    & $route -Action Start
    foreach ($release in '10','11') {
        $dotnet = if ($release -eq '10') { $Net10Dotnet } else { $Net11Dotnet }
        $sample = "samples/net$release.0/DuneArchive/DuneArchive.csproj"
        $directory = Join-Path $root (Split-Path $sample)
        $database = 'playbook_dune_' + [guid]::NewGuid().ToString('N')
        & $route -Action New -Database $database
        try {
            if ($Record) {
                & $route -Action Run -Database $database -Executable $shell -WorkingDirectory $root `
                    -CommandArguments @('-NoProfile','-File',(Join-Path $PSScriptRoot 'Record-Evidence.ps1'),'-Sample',$sample,'-Dotnet',$dotnet)
            }
            else {
                & $route -Action Run -Database $database -Executable $dotnet -WorkingDirectory $directory `
                    -CommandArguments @('run','--project','DuneArchive.csproj','-c','Release','-p:RestoreLockedMode=true','--no-launch-profile')
            }
        }
        finally { & $route -Action Remove -Database $database }
    }
}
catch { $testFailure = $_; throw }
finally {
    $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = $previous
    try {
        foreach ($scope in $paths.Keys) {
            if ($paths[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)) { throw "Unexpected $scope PATH change; shared settings preserved." }
    }
    }
    finally {
        try { & $route -Action Stop }
        catch {
            if ($testFailure) { Write-Warning "SQL cleanup failed: $($_.Exception.Message). Original test error preserved." }
            else { throw }
        }
    }
}
