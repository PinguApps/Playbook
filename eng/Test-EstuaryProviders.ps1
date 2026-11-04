#Requires -Version 7.3
[CmdletBinding()]
param(
    [switch]$Record,
    [string]$Dotnet10 = 'C:\Program Files\dotnet\dotnet.exe',
    [string]$Dotnet11 = 'C:\Users\pingu\.dotnet-playbook-11\dotnet.exe'
)
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
foreach ($executable in $Dotnet10,$Dotnet11) {
    if (-not [IO.Path]::IsPathFullyQualified($executable) -or -not (Test-Path -LiteralPath $executable -PathType Leaf)) { throw 'Supply existing absolute -Dotnet10 and -Dotnet11 executable paths.' }
}
$root = Split-Path $PSScriptRoot -Parent
$before = @{}
foreach ($scope in 'Process','User','Machine') { $before[$scope] = [Environment]::GetEnvironmentVariable('PATH',$scope) }
$integration = $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
$cosmosDatabase = $null
try {
    $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
    & "$PSScriptRoot/SqlServer.ps1" -Action Start
    & "$PSScriptRoot/Cosmos.ps1" -Action Start
    $sharedDatabase = 'playbook_estuary_' + [guid]::NewGuid().ToString('N')
    & "$PSScriptRoot/Cosmos.ps1" -Action New -Database $sharedDatabase
    $cosmosDatabase = $sharedDatabase
    foreach ($release in '10','11') {
        $dotnet = if ($release -eq '10') { $Dotnet10 } else { $Dotnet11 }
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
            $database = if ($name -eq 'EstuaryParcels') { $cosmosDatabase } else { 'playbook_estuary_' + [guid]::NewGuid().ToString('N') }
            if ($name -eq 'EstuarySql') { & $route -Action New -Database $database }
            try {
                $arguments = @("$directory/bin/Release/net$release.0/$name.dll")
                if ($name -eq 'EstuaryParcels' -and $release -eq '11') { $arguments += '--reuse' }
                $output = & $route -Action Run -Database $database -Executable $dotnet -CommandArguments $arguments -WorkingDirectory $directory | Out-String
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
                    foreach ($file in Get-ChildItem "$directory/bin/Release/net$release.0" -Filter '*ServiceInterop*') {
                        $receipt.output += "`nNATIVE $($file.Name) sha256=$((Get-FileHash $file.FullName).Hash)"
                    }
                    $receipt | ConvertTo-Json -Depth 30 | Set-Content "$directory/behavior.evidence.json"
                }
            } finally { if ($name -eq 'EstuarySql') { & $route -Action Remove -Database $database } }
        }
    }
} finally {
    if ($cosmosDatabase) { & "$PSScriptRoot/Cosmos.ps1" -Action Remove -Database $cosmosDatabase }
    # Shared SQL service can be in use by other authors; leave its lifecycle unchanged.
    $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = $integration
    foreach ($scope in $before.Keys) {
        if ($before[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)) { throw "Unexpected $scope PATH change; shared settings preserved." }
    }
}
