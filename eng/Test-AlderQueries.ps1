#Requires -Version 7.3
[CmdletBinding()]
param([switch]$Record)
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$root = Split-Path $PSScriptRoot -Parent
$paths = @{}
foreach ($scope in 'Process','User','Machine') { $paths[$scope] = [Environment]::GetEnvironmentVariable('PATH',$scope) }
$integration = $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
try {
    $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
    foreach ($release in '10','11') {
        $dotnet = if ($release -eq '10') { 'C:\Program Files\dotnet\dotnet.exe' } else { 'C:\Users\pingu\.dotnet-playbook-11\dotnet.exe' }
        $sample = "samples/net$release.0/AlderQueries/AlderQueries.csproj"
        $directory = Split-Path (Join-Path $root $sample)
        Push-Location $directory
        try {
            $diagnostics = & $dotnet build -t:Rebuild -c Release -p:DefineConstants=DIAGNOSTIC_PROBE -v:q | Out-String
            if ($diagnostics -notmatch 'EF1002' -or ($release -eq '11' -and ($diagnostics -notmatch 'EF1003' -or $diagnostics -notmatch 'CS0618'))) { throw 'Expected raw SQL diagnostics absent.' }
            if ($release -eq '10' -and $diagnostics -match 'EF1003') { throw 'Unexpected older Format/Concat diagnostic.' }
            Write-Host "PASS: EF$release actual compiler/analyzer diagnostics"
        } finally { Pop-Location }
        if ($Record) { & "$PSScriptRoot/Record-Evidence.ps1" -Sample $sample -Dotnet $dotnet -Kind compile }
        else {
            Push-Location $directory
            try { & $dotnet build -c Release -p:RestoreLockedMode=true } finally { Pop-Location }
        }
        $database = 'playbook_alder_' + [guid]::NewGuid().ToString('N')
        & "$PSScriptRoot/SqlServer.ps1" -Action New -Database $database
        try {
            $output = & "$PSScriptRoot/SqlServer.ps1" -Action Run -Database $database -Executable $dotnet -CommandArguments @("$directory/bin/Release/net$release.0/AlderQueries.dll") -WorkingDirectory $directory | Out-String
            Write-Host $output
            if ($output -notmatch 'PASS: rollback Never' -or $output -notmatch 'PASS: canceled update' -or $output -notmatch 'PASS: executed SQL form Parameter') { throw 'Missing database proof.' }
            if ($Record) {
                $receipt = Get-Content "$directory/verification-evidence.json" -Raw | ConvertFrom-Json -AsHashtable
                $receipt.kind = 'execution'
                $receipt.command = 'eng/Test-AlderQueries.ps1 -Record; restricted disposable SQL2025 database'
                $receipt.scope = 'Real isolated SQL2025 transaction rollback/single-command and collection mode checks; actual native SQLite parameter/metacharacter/cancellation controls. Windows x64 RC2 is not GA or arbitrary-provider proof.'
                $receipt.output = $output.Trim()
                $receipt.output += "`nCOMPILE-ONLY DIAGNOSTIC CONTROLS (never executed):`n" + $diagnostics.Trim()
                $receipt | ConvertTo-Json -Depth 30 | Set-Content "$directory/behavior.evidence.json"
            }
        } finally { & "$PSScriptRoot/SqlServer.ps1" -Action Remove -Database $database }
    }
} finally {
    $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = $integration
    foreach ($scope in $paths.Keys) {
        if ($paths[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)) { throw "Unexpected $scope PATH change; shared settings preserved." }
    }
}

