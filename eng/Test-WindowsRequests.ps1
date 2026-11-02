#requires -Version 7.4
[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$AccessDirectory,
    [switch]$Record
)
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$root = Split-Path $PSScriptRoot -Parent
$before = @{}
foreach ($scope in 'Process','User','Machine') { $before[$scope] = [Environment]::GetEnvironmentVariable('PATH', $scope) }
$previous = @{}
foreach ($key in 'PLAYBOOK_HTTPS_PREFIX','PLAYBOOK_CERTIFICATE','DOTNET_ADD_GLOBAL_TOOLS_TO_PATH','DOTNET_GENERATE_ASPNET_CERTIFICATE') { $previous[$key] = [Environment]::GetEnvironmentVariable($key) }
Push-Location -LiteralPath $root
try {
    $access = Get-Content -LiteralPath (Join-Path $AccessDirectory 'access.json') -Raw | ConvertFrom-Json
    $env:PLAYBOOK_HTTPS_PREFIX = $access.prefix
    $env:PLAYBOOK_CERTIFICATE = (Get-Content -LiteralPath (Join-Path $AccessDirectory 'certificate.txt') -Raw).Trim()
    $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
    $env:DOTNET_GENERATE_ASPNET_CERTIFICATE = 'false'
    foreach ($version in '10','11') {
        $dotnet = if ($version -eq '10') { 'C:/Program Files/dotnet/dotnet.exe' } else { 'C:/Users/pingu/.dotnet-playbook-11/dotnet.exe' }
        $directory = "samples/net$version.0/HarbourRequests"
        if ($Record) { & ./eng/Record-Evidence.ps1 -Sample "$directory/HarbourRequests.csproj" -Dotnet $dotnet }
        else { & $dotnet "$directory/bin/Release/net$version.0/HarbourRequests.dll" }
    }
}
finally {
    Pop-Location
    foreach ($key in $previous.Keys) { [Environment]::SetEnvironmentVariable($key, $previous[$key], 'Process') }
    foreach ($scope in $before.Keys) {
        if ($before[$scope] -cne [Environment]::GetEnvironmentVariable('PATH', $scope)) { throw "$scope PATH changed; shared settings were not restored." }
    }
}
