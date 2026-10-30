#Requires -Version 7.0
[CmdletBinding()]
param(
    [string]$Net10Dotnet = 'C:/Program Files/dotnet/dotnet.exe',
    [string]$Net11Dotnet = 'C:/Users/pingu/.dotnet-playbook-11/dotnet.exe',
    [switch]$Record
)
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$root = Split-Path $PSScriptRoot -Parent
$session = Join-Path ([IO.Path]::GetTempPath()) ('playbook-trails-' + [guid]::NewGuid().ToString('N'))
$previousDatabase = $env:PLAYBOOK_TRAIL_DATABASE
$previousIntegration = $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
$paths = @{}
foreach ($scope in @('Process','User','Machine')) { $paths[$scope] = [Environment]::GetEnvironmentVariable('PATH', $scope) }
New-Item -ItemType Directory -Path $session | Out-Null
try {
    $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
    $env:PLAYBOOK_TRAIL_DATABASE = Join-Path $session 'cards.db'
    foreach ($edition in @('net10.0','net11.0')) {
        $sample = "samples/$edition/TrailCards/TrailCards.csproj"
        $dotnet = if ($edition -eq 'net10.0') { $Net10Dotnet } else { $Net11Dotnet }
        if ($Record) {
            & (Join-Path $PSScriptRoot 'Record-Evidence.ps1') -Sample $sample -Dotnet $dotnet
        } else {
            Push-Location -LiteralPath (Join-Path $root "samples/$edition/TrailCards")
            try {
                & $dotnet build TrailCards.csproj -c Release -p:RestoreLockedMode=true
                & $dotnet "bin/Release/$edition/TrailCards.dll"
            } finally { Pop-Location }
        }
    }
    & $Net10Dotnet (Join-Path $root 'samples/net10.0/TrailCards/bin/Release/net10.0/TrailCards.dll') verify-migrated
    Write-Host 'PASS: actual EF10 SQLite database consumed and edited by EF11, then read back with EF10. No service provisioning.'
} finally {
    $env:PLAYBOOK_TRAIL_DATABASE = $previousDatabase
    $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = $previousIntegration
    # Check the resolved target before deleting the unique disposable test directory.
    $resolvedSession = [IO.Path]::GetFullPath($session)
    $tempRoot = [IO.Path]::GetFullPath([IO.Path]::GetTempPath()).TrimEnd([IO.Path]::DirectorySeparatorChar) + [IO.Path]::DirectorySeparatorChar
    if (-not $resolvedSession.StartsWith($tempRoot, [StringComparison]::OrdinalIgnoreCase) -or (Split-Path $resolvedSession -Leaf) -notlike 'playbook-trails-*') { throw 'Unsafe cleanup target.' }
    Remove-Item -LiteralPath $session -Recurse -Force
    foreach ($scope in $paths.Keys) {
        if ($paths[$scope] -cne [Environment]::GetEnvironmentVariable('PATH', $scope)) { throw "Unexpected $scope PATH change; inspect shared settings before changing them." }
    }
}
