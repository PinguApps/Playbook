#Requires -Version 7.3
[CmdletBinding()]
param([switch]$Record,
    [string]$Net10Dotnet = (Get-Command dotnet -CommandType Application | Select-Object -First 1).Source,
    [string]$Net11Dotnet = $Net10Dotnet)
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$root = Split-Path $PSScriptRoot -Parent
$paths = @{}
foreach ($scope in 'Process','User','Machine') { $paths[$scope] = [Environment]::GetEnvironmentVariable('PATH',$scope) }
$integration = $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
try {
    $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
    foreach ($release in '10','11') {
        $dotnet = if ($release -eq '10') { $Net10Dotnet } else { $Net11Dotnet }
        $sample = "samples/net$release.0/BrackenReads/BrackenReads.csproj"
        $directory = Split-Path (Join-Path $root $sample)
        if ($Record) { & "$PSScriptRoot/Record-Evidence.ps1" -Sample $sample -Dotnet $dotnet -Kind compile }
        else {
            Push-Location $directory
            try { & $dotnet build -c Release -p:RestoreLockedMode=true } finally { Pop-Location }
        }
        $database = 'playbook_bracken_' + [guid]::NewGuid().ToString('N')
        & "$PSScriptRoot/SqlServer.ps1" -Action New -Database $database
        try {
            $output = & "$PSScriptRoot/SqlServer.ps1" -Action Run -Database $database -Executable $dotnet -CommandArguments @("$directory/bin/Release/net$release.0/BrackenReads.dll") -WorkingDirectory $directory | Out-String
            Write-Host $output
            if ($output -notmatch 'PASS: separate SQL writer' -or ($release -eq '11' -and $output -notmatch 'Observed actual DbQueryConcurrencyException')) { throw 'Missing race/recovery proof.' }
            if ($Record) {
                $receipt = Get-Content "$directory/verification-evidence.json" -Raw | ConvertFrom-Json -AsHashtable
                $receipt.kind = 'execution'
                $receipt.command = 'eng/Test-BrackenReads.ps1 -Record; restricted disposable SQL2025 database'
                $receipt.scope = if ($release -eq '10') { 'Real SQL2025 concurrent writer between split SELECTs; EF10 original roots and fresh-context whole-read retry. No snapshot, NativeAOT, other-provider or GA proof.' } else { 'Real SQL2025 concurrent writer between split SELECTs; EF11 DbQueryConcurrencyException and fresh-context whole-read retry. No snapshot, NativeAOT, other-provider or GA proof.' }
                $receipt.output = $output.Trim()
                $receipt | ConvertTo-Json -Depth 30 | Set-Content "$directory/race.evidence.json"
            }
        } finally { & "$PSScriptRoot/SqlServer.ps1" -Action Remove -Database $database }
    }
} finally {
    $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = $integration
    foreach ($scope in $paths.Keys) {
        if ($paths[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)) { throw "Unexpected $scope PATH change; shared settings preserved." }
    }
}
