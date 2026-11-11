#Requires -Version 7.3
[CmdletBinding()]
param([switch]$Record,
    [string]$Net10Dotnet = (Get-Command dotnet -CommandType Application | Select-Object -First 1).Source,
    [string]$Net11Dotnet = (Get-Command dotnet -CommandType Application | Select-Object -First 1).Source)
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
        $version = if ($release -eq '10') { '10.0.13' } else { '11.0.0' }
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
        foreach ($file in 'Program.cs','CanalDesign.csproj','Directory.Build.props','Directory.Build.targets','Directory.Packages.props','global.json','packages.lock.json') {
            Copy-Item -LiteralPath (Join-Path $source $file) -Destination $run -Recurse
        }
        $commands = if ($release -eq '10') { 'Commands.ps1' } else { 'Configure.ps1' }
        Copy-Item -LiteralPath (Join-Path $source $commands) -Destination $run
        Push-Location $run
        try {
            if ($release -eq '11') {
                & ./Configure.ps1
                $actual = Get-Content .config/dotnet-ef.json -Raw | ConvertFrom-Json
                $expected = Get-Content "$source/.config/dotnet-ef.json" -Raw | ConvertFrom-Json
                if (($actual | ConvertTo-Json -Compress) -cne ($expected | ConvertTo-Json -Compress)) { throw 'Configuration script differs from canonical JSON.' }
            }
            & $dotnet build -c Release -p:RestoreLockedMode=true
            & $dotnet tool install dotnet-ef --version $version --tool-path "$run/tools"
            $tool = @(Get-ChildItem "$run/tools" -Recurse -Filter dotnet-ef.dll)[0].FullName
            New-Item -ItemType Directory "$run/commands/deep" -Force | Out-Null
            Push-Location "$run/commands/deep"
            try {
                $default = if ($release -eq '10') {
                    & "$run/Commands.ps1" -Dotnet $dotnet -EfTool $tool -ProjectDirectory $run | Out-String
                } else { & $dotnet $tool dbcontext info --no-build | Out-String }
                if ($default -notmatch 'Type: CanalSamples.CanalContext') { throw 'Wrong default context or configured relative project path.' }
                $selected = if ($release -eq '10') {
                    & "$run/Commands.ps1" -Dotnet $dotnet -EfTool $tool -ProjectDirectory $run -Context ArchiveContext | Out-String
                } else { & $dotnet $tool dbcontext info --context ArchiveContext --no-build | Out-String }
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
