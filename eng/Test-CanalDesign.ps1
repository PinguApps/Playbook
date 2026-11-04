#Requires -Version 7.3
[CmdletBinding()]
param([switch]$Record,
    [string]$Net10Dotnet = 'C:\Program Files\dotnet\dotnet.exe',
    [string]$Net11Dotnet = 'C:\Users\pingu\.dotnet-playbook-11\dotnet.exe')
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$root = Split-Path $PSScriptRoot -Parent
. "$PSScriptRoot/Evidence.ps1"
$paths = @{}
foreach ($scope in 'Process','User','Machine') { $paths[$scope] = [Environment]::GetEnvironmentVariable('PATH',$scope) }
$previous = $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
try {
    $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
    foreach ($release in '10','11') {
        $dotnet = if ($release -eq '10') { $Net10Dotnet } else { $Net11Dotnet }
        $version = if ($release -eq '10') { '10.0.13' } else { '11.0.0-rc.2.26504.105' }
        $sample = "samples/net$release.0/CanalDesign/CanalDesign.csproj"
        $source = Split-Path (Join-Path $root $sample)
        if ($Record) { & "$PSScriptRoot/Record-Evidence.ps1" -Sample $sample -Dotnet $dotnet }
        else {
            Push-Location $source
            try { & $dotnet run -c Release -p:RestoreLockedMode=true } finally { Pop-Location }
        }
        $before = Get-EvidenceInputs $root $sample
        $run = Join-Path $root ('artifacts/canal-design-' + [guid]::NewGuid().ToString('N'))
        New-Item -ItemType Directory $run -Force | Out-Null
        foreach ($file in 'Program.cs','CanalDesign.csproj','Directory.Build.props','Directory.Build.targets','Directory.Packages.props','global.json','packages.lock.json','.config') {
            Copy-Item -LiteralPath (Join-Path $source $file) -Destination $run -Recurse
        }
        Push-Location $run
        try {
            & $dotnet build -c Release -p:RestoreLockedMode=true
            & $dotnet tool install dotnet-ef --version $version --tool-path "$run/tools"
            $tool = @(Get-ChildItem "$run/tools" -Recurse -Filter dotnet-ef.dll)[0].FullName
            New-Item -ItemType Directory "$run/commands/deep" -Force | Out-Null
            Push-Location "$run/commands/deep"
            try {
                $common = if ($release -eq '10') { @('--project',$run,'--startup-project',$run,'--context','CanalContext','--configuration','Release') } else { @() }
                $default = & $dotnet $tool dbcontext info @common --no-build | Out-String
                if ($default -notmatch 'Type: CanalSamples.CanalContext') { throw 'Wrong default context or configured relative project path.' }
                $override = if ($release -eq '10') { @('--project',$run,'--startup-project',$run,'--configuration','Release') } else { @() }
                $selected = & $dotnet $tool dbcontext info @override --context ArchiveContext --no-build | Out-String
                if ($selected -notmatch 'Type: CanalSamples.ArchiveContext') { throw 'Explicit context did not override configured default.' }
                Write-Host $default.Trim()
                Write-Host $selected.Trim()
            } finally { Pop-Location }
            $result = "PASS EF$release real CLI from nested directory; relative project/startup paths and explicit context override. No PMC execution."
            Write-Host $result
            if ($Record) {
                $after = Get-EvidenceInputs $root $sample
                if (($before | ConvertTo-Json -Compress) -cne ($after | ConvertTo-Json -Compress)) { throw 'Canonical inputs changed.' }
                $receipt = Get-Content "$source/verification-evidence.json" -Raw | ConvertFrom-Json -AsHashtable
                $receipt.kind = 'execution'
                $receipt.command = "eng/Test-CanalDesign.ps1 -Record; dotnet-ef $version dbcontext info from commands/deep with default and overridden context"
                $receipt.scope = 'Real CLI directory lookup, config-relative project/startup paths and context override; EF10 uses explicit options. No PMC, cross-platform or GA execution.'
                $receipt.output = $result + "`n" + $default.Trim() + "`n" + $selected.Trim()
                $receipt.recordedAt = [DateTimeOffset]::UtcNow.ToString('O')
                $receipt | ConvertTo-Json -Depth 30 | Set-Content "$source/cli-checks.evidence.json" -Encoding utf8
            }
        } finally { Pop-Location }
    }
} finally {
    $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = $previous
    foreach ($scope in $paths.Keys) {
        if ($paths[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)) { throw "Unexpected $scope PATH change; shared settings preserved." }
    }
}
