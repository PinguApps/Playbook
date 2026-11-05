#Requires -Version 7.3
[CmdletBinding()]
param([switch]$Record,
    [string]$Dotnet10 = (Get-Command dotnet -CommandType Application | Select-Object -First 1).Source,
    [string]$Dotnet11 = (Get-Command dotnet -CommandType Application | Select-Object -First 1).Source)
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$root = Split-Path $PSScriptRoot -Parent
$before = @{}
foreach ($scope in 'Process','User','Machine') { $before[$scope] = [Environment]::GetEnvironmentVariable('PATH',$scope) }
$integration = $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
try {
    $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
    foreach ($release in '10','11') {
        $dotnet = if ($release -eq '10') { $Dotnet10 } else { $Dotnet11 }
        foreach ($name in 'TernGauges','TernPatches','TernPermits') {
            $sample = "samples/net$release.0/$name/$name.csproj"
            $directory = Split-Path (Join-Path $root $sample)
            if ($Record) { & "$PSScriptRoot/Record-Evidence.ps1" -Sample $sample -Dotnet $dotnet -Kind compile }
            else {
                Push-Location $directory
                try { & $dotnet build -c Release -p:RestoreLockedMode=true } finally { Pop-Location }
            }
            $database = $null
            try {
                $arguments = @("$directory/bin/Release/net$release.0/$name.dll")
                if ($name -eq 'TernPermits') { $output = & $dotnet @arguments | Out-String }
                else {
                    $database = 'playbook_tern_' + [guid]::NewGuid().ToString('N')
                    & "$PSScriptRoot/SqlServer.ps1" -Action New -Database $database
                    $output = & "$PSScriptRoot/SqlServer.ps1" -Action Run -Database $database -Executable $dotnet -CommandArguments $arguments -WorkingDirectory $directory | Out-String
                }
                Write-Host $output
                $expected = if ($name -eq 'TernPatches') { 4 } else { 1 }
                if ([regex]::Matches($output,'PASS ').Count -ne $expected) { throw 'Missing execution checks.' }
                if ($Record) {
                    $receipt = Get-Content "$directory/verification-evidence.json" -Raw | ConvertFrom-Json -AsHashtable
                    $receipt.kind = 'execution'
                    $receipt.command = 'eng/Test-TernExtensions.ps1 -Record'
                    $receipt.scope = 'Actual SQL2025 scalar/JSON checks or Windows x64 bundled SQLite generator persistence. No NativeAOT, GA, provider replacement or other-platform proof.'
                    $receipt.output = $output.Trim()
                    foreach ($file in Get-ChildItem "$directory/bin/Release/net$release.0" -Recurse -Filter 'e_sqlite3.dll') {
                        $receipt.output += "`nNATIVE $($file.Name) sha256=$((Get-FileHash $file.FullName).Hash)"
                    }
                    $receipt | ConvertTo-Json -Depth 30 | Set-Content "$directory/behavior.evidence.json"
                }
            } finally { if ($database) { & "$PSScriptRoot/SqlServer.ps1" -Action Remove -Database $database } }
        }
    }
} finally {
    $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = $integration
    foreach ($scope in $before.Keys) {
        if ($before[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)) { throw "Unexpected $scope PATH change; shared settings preserved." }
    }
}
