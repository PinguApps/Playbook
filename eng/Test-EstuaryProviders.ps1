#Requires -Version 7.3
[CmdletBinding()]
param([switch]$Record)
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$root = Split-Path $PSScriptRoot -Parent
$before = @{}
foreach ($scope in 'Process','User','Machine') { $before[$scope] = [Environment]::GetEnvironmentVariable('PATH',$scope) }
$integration = $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
try {
    $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
    & "$PSScriptRoot/SqlServer.ps1" -Action Start
    & "$PSScriptRoot/Cosmos.ps1" -Action Start
    foreach ($release in '10','11') {
        $dotnet = if ($release -eq '10') { 'C:\Program Files\dotnet\dotnet.exe' } else { 'C:\Users\pingu\.dotnet-playbook-11\dotnet.exe' }
        foreach ($name in 'EstuarySql','EstuaryParcels') {
            $sample = "samples/net$release.0/$name/$name.csproj"
            $directory = Split-Path (Join-Path $root $sample)
            $diagnostics = ''
            if ($name -eq 'EstuaryParcels') {
                Push-Location $directory
                try {
                    $diagnostics = & $dotnet build -t:Rebuild -c Release -p:RestoreLockedMode=true -p:DefineConstants=DIAGNOSTIC_PROBE -v:q | Out-String
                    if (($release -eq '11') -ne ($diagnostics -match 'warning CS0618')) { throw 'Unexpected write-content obsoletion diagnostic.' }
                    $nativePreference = $PSNativeCommandUseErrorActionPreference
                    try {
                        $PSNativeCommandUseErrorActionPreference = $false
                        $nullProbe = & $dotnet build -c Release -p:RestoreLockedMode=true -p:DefineConstants=NULL_PROBE -v:q | Out-String
                        if ($LASTEXITCODE -eq 0 -or $nullProbe -notmatch 'CS1503') { throw 'Null unexpectedly accepted by bool configuration API.' }
                        $diagnostics += $nullProbe
                    } finally { $PSNativeCommandUseErrorActionPreference = $nativePreference }
                } finally { Pop-Location }
            }
            if ($Record) { & "$PSScriptRoot/Record-Evidence.ps1" -Sample $sample -Dotnet $dotnet -Kind compile }
            else {
                Push-Location $directory
                try { & $dotnet build -c Release -p:RestoreLockedMode=true } finally { Pop-Location }
            }
            $route = if ($name -eq 'EstuarySql') { "$PSScriptRoot/SqlServer.ps1" } else { "$PSScriptRoot/Cosmos.ps1" }
            $database = 'playbook_estuary_' + [guid]::NewGuid().ToString('N')
            & $route -Action New -Database $database
            try {
                $output = & $route -Action Run -Database $database -Executable $dotnet -CommandArguments @("$directory/bin/Release/net$release.0/$name.dll") -WorkingDirectory $directory | Out-String
                Write-Host $output
                $expected = if ($name -eq 'EstuarySql') { 2 } else { 3 }
                if ([regex]::Matches($output,'PASS (SQL|Cosmos)').Count -ne $expected) { throw 'Missing execution checks.' }
                if ($Record) {
                    $receipt = Get-Content "$directory/verification-evidence.json" -Raw | ConvertFrom-Json -AsHashtable
                    $receipt.kind = 'execution'
                    $receipt.command = 'eng/Test-EstuaryProviders.ps1 -Record; approved isolated providers'
                    $receipt.scope = 'Real local SQL2025 configuration and round-trip; legacy Cosmos emulator key identity, option removal, reload, ETag and stale-write checks. No Azure cloud, Entra or GA proof.'
                    $receipt.output = $output.Trim()
                    $receipt.output += "`nCOMPILE-ONLY CONTROLS:`n" + $diagnostics.Trim()
                    foreach ($file in Get-ChildItem "$directory/bin/Release/net$release.0" -Filter '*Service.Interop*') {
                        $receipt.output += "`nNATIVE $($file.Name) sha256=$((Get-FileHash $file.FullName).Hash)"
                    }
                    $receipt | ConvertTo-Json -Depth 30 | Set-Content "$directory/behavior.evidence.json"
                }
            } finally { & $route -Action Remove -Database $database }
        }
    }
} finally {
    # Shared SQL service can be in use by other authors; leave its lifecycle unchanged.
    $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = $integration
    foreach ($scope in $before.Keys) {
        if ($before[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)) { throw "Unexpected $scope PATH change; shared settings preserved." }
    }
}
