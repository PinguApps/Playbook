[CmdletBinding()]
param([switch]$Record)
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$root = Split-Path $PSScriptRoot -Parent
$route = Join-Path $PSScriptRoot 'Cosmos.ps1'
$paths = @{}
foreach ($scope in 'Process','User','Machine') { $paths[$scope] = [Environment]::GetEnvironmentVariable('PATH',$scope) }
$previous = $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
try {
    $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
    & $route -Action Start
    foreach ($release in '10','11') {
        $dotnet = if ($release -eq '10') { 'C:\Program Files\dotnet\dotnet.exe' } else { 'C:\Users\pingu\.dotnet-playbook-11\dotnet.exe' }
        if ($Record) {
            & "$PSScriptRoot/Record-Evidence.ps1" -Sample "samples/net$release.0/HarbourLabels/HarbourLabels.csproj" -Dotnet $dotnet
        }
        $directory = Join-Path $root "samples/net$release.0/ReefManifest"
        $database = 'playbook_manifest_' + [guid]::NewGuid().ToString('N')
        & $route -Action New -Database $database
        try {
            if ($Record) {
                & $route -Action Run -Database $database -Executable (Get-Process -Id $PID).Path -WorkingDirectory $root -CommandArguments @('-NoProfile','-File',"$PSScriptRoot/Record-Evidence.ps1",'-Sample',"samples/net$release.0/ReefManifest/ReefManifest.csproj",'-Dotnet',$dotnet)
            }
            else {
                & $route -Action Run -Database $database -Executable $dotnet -WorkingDirectory $directory -CommandArguments @('run','-c','Release','-p:RestoreLockedMode=true')
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
