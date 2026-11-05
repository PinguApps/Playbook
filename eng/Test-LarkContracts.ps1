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
        foreach ($name in 'LarkBindings','LarkMaterializers','LarkRelationships') {
            $sample = "samples/net$release.0/$name/$name.csproj"
            $directory = Split-Path (Join-Path $root $sample)
            $diagnostics = ''
            if ($name -eq 'LarkMaterializers') {
                Push-Location $directory
                $nativePreference = $PSNativeCommandUseErrorActionPreference
                try {
                    $PSNativeCommandUseErrorActionPreference = $false
                    $diagnostics = & $dotnet build -c Release -p:RestoreLockedMode=true -p:DefineConstants=LEGACY_HOOK_PROBE -v:q 2>&1 | Out-String
                    if ($LASTEXITCODE -eq 0 -or $diagnostics -notmatch 'CS0619') { throw 'Expected rejected legacy materializer contract.' }
                    Write-Host "PASS EF$release compile-only rejected legacy materializer contract"
                } finally { $PSNativeCommandUseErrorActionPreference = $nativePreference; Pop-Location }
            }
            if ($Record) { & "$PSScriptRoot/Record-Evidence.ps1" -Sample $sample -Dotnet $dotnet -Kind compile }
            else {
                Push-Location $directory
                try { & $dotnet build -c Release -p:RestoreLockedMode=true } finally { Pop-Location }
            }
            $database = $null
            try {
                $arguments = @("$directory/bin/Release/net$release.0/$name.dll")
                if ($name -ne 'LarkRelationships') { $output = & $dotnet @arguments | Out-String }
                else {
                    $database = 'playbook_lark_' + [guid]::NewGuid().ToString('N')
                    & "$PSScriptRoot/SqlServer.ps1" -Action New -Database $database
                    $output = & "$PSScriptRoot/SqlServer.ps1" -Action Run -Database $database -Executable $dotnet -CommandArguments $arguments -WorkingDirectory $directory | Out-String
                }
                Write-Host $output
                $expected = if ($name -eq 'LarkRelationships') { 3 } else { 1 }
                if ([regex]::Matches($output,'PASS ').Count -ne $expected) { throw 'Missing execution checks.' }
                if ($Record) {
                    $receipt = Get-Content "$directory/verification-evidence.json" -Raw | ConvertFrom-Json -AsHashtable
                    $receipt.kind = 'execution'
                    $receipt.command = 'eng/Test-LarkContracts.ps1 -Record'
                    $receipt.scope = 'Real SQL2025 missing-principal joins/schema/indexes or Windows x64 SQLite structural and constructor consumers. Root construction is a contract check, not custom provider query execution. No GA, NativeAOT or other-platform proof.'
                    $receipt.output = $output.Trim()
                    if ($diagnostics) { $receipt.output += "`nCOMPILE-ONLY NEGATIVE CONTROL:`n" + $diagnostics.Trim() }
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

