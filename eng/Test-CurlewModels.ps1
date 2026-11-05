#Requires -Version 7.3
[CmdletBinding()]
param([switch]$Record,
    [string]$Dotnet10 = 'C:\Program Files\dotnet\dotnet.exe',
    [string]$Dotnet11 = 'C:\Users\pingu\.dotnet-playbook-11\dotnet.exe')
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
        foreach ($name in 'CurlewMappings','CurlewConventions') {
            $sample = "samples/net$release.0/$name/$name.csproj"
            $directory = Split-Path (Join-Path $root $sample)
            if ($Record) { & "$PSScriptRoot/Record-Evidence.ps1" -Sample $sample -Dotnet $dotnet -Kind compile }
            else {
                Push-Location $directory
                try { & $dotnet build -c Release -p:RestoreLockedMode=true } finally { Pop-Location }
            }
            $output = & $dotnet "$directory/bin/Release/net$release.0/$name.dll" | Out-String
            if ($name -eq 'CurlewConventions') {
                $output += & $dotnet "$directory/bin/Release/net$release.0/$name.dll" --after-added | Out-String
            }
            if ($name -eq "CurlewMappings" -and $release -eq "11") {
                $output += & $dotnet "$directory/bin/Release/net$release.0/$name.dll" --invalid-complex-index | Out-String
            }
            if ($name -eq "CurlewConventions" -and $release -eq "10") {
                Push-Location $directory
                try {
                    & $dotnet build -c Release -t:Rebuild -p:RestoreLockedMode=true -p:DefineConstants=LEGACY_CALLBACK
                    $legacy = & $dotnet "$directory/bin/Release/net$release.0/$name.dll" | Out-String
                    if ($legacy -notmatch 'CONTROL LEGACY_CALLBACK') { throw 'Legacy variant did not execute.' }
                    $output += $legacy
                } finally { Pop-Location }
            }
            Write-Host $output
            if ($output -notmatch 'PASS ') { throw 'Missing execution proof.' }
            if ($Record) {
                $receipt = Get-Content "$directory/verification-evidence.json" -Raw | ConvertFrom-Json -AsHashtable
                $receipt.kind = 'execution'
                $receipt.command = 'eng/Test-CurlewModels.ps1 -Record'
                $receipt.scope = 'Real Windows x64 SQLite table/view lookup and ignored-type convention ordering controls. No native JSON-index, GA or other-platform proof.'
                $receipt.output = $output.Trim()
                $receipt | ConvertTo-Json -Depth 30 | Set-Content "$directory/behavior.evidence.json"
            }
        }
    }
} finally {
    $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = $integration
    foreach ($scope in $before.Keys) {
        if ($before[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)) { throw "Unexpected $scope PATH change; shared settings preserved." }
    }
}
