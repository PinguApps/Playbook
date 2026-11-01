[CmdletBinding()]
param([switch]$Record)

$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$root = Split-Path $PSScriptRoot -Parent
$paths = @{}
foreach ($scope in 'Process','User','Machine') { $paths[$scope] = [Environment]::GetEnvironmentVariable('PATH',$scope) }
$previous = $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
$route = Join-Path $PSScriptRoot 'SqlServer.ps1'
$shell = (Get-Process -Id $PID).Path
try {
    $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
    & $route -Action Start
    foreach ($release in '10','11') {
        $dotnet = if ($release -eq '10') { 'C:\Program Files\dotnet\dotnet.exe' } else { 'C:\Users\pingu\.dotnet-playbook-11\dotnet.exe' }
        $sample = "samples/net$release.0/MoorlandDispatch/MoorlandDispatch.csproj"
        $directory = Join-Path $root (Split-Path $sample)
        $database = 'playbook_dispatch_' + [guid]::NewGuid().ToString('N')
        & $route -Action New -Database $database
        try {
            if ($Record) {
                & $route -Action Run -Database $database -Executable $shell -WorkingDirectory $root `
                    -CommandArguments @('-NoProfile','-File',(Join-Path $PSScriptRoot 'Record-Evidence.ps1'),'-Sample',$sample,'-Dotnet',$dotnet)
            }
            else {
                & $route -Action Run -Database $database -Executable $dotnet -WorkingDirectory $directory `
                    -CommandArguments @('run','--project','MoorlandDispatch.csproj','-c','Release','-p:RestoreLockedMode=true','--no-launch-profile')
            }
        }
        finally { & $route -Action Remove -Database $database }
    }
}
finally {
    & $route -Action Stop
    $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = $previous
    foreach ($scope in $paths.Keys) {
        if ($paths[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)) { throw "Unexpected $scope PATH change; shared settings preserved." }
    }
}
