[CmdletBinding()]
param([string]$Dotnet = (Get-Command dotnet -CommandType Application | Select-Object -First 1).Source)
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$saved = $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
$savedDatabase = $env:PLAYBOOK_AVOCET_DATABASE
$paths = @{}
foreach ($scope in 'Process','User','Machine') { $paths[$scope] = [Environment]::GetEnvironmentVariable('PATH', $scope) }
$run = Join-Path $root ('artifacts/avocet-tooling-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory $run | Out-Null
function Invoke-Ef([string[]]$Arguments, [switch]$Failure) {
    $output = & $dotnet $tool @Arguments 2>&1 | Out-String
    $output | Write-Host
    if (($LASTEXITCODE -ne 0) -ne [bool]$Failure) { throw $output }
    return $output
}
try {
    $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
    foreach ($r in '10','11') {
        $expected = if ($r -eq '10') { '10.0.13' } else { '11.0.0-rc.2.26504.105' }
        $tools = @(Get-ChildItem "$root/artifacts/ef$r" -Filter dotnet-ef.dll -Recurse |
            Where-Object { $_.FullName.Contains($expected, [StringComparison]::Ordinal) })
        if ($tools.Count -ne 1) { throw "Install dotnet-ef $expected under artifacts/ef$r before running this check." }
        $tool = $tools[0].FullName
        $version = Invoke-Ef -Arguments @('--version')
        if ($version -notmatch "(?m)^$([regex]::Escape($expected))\s*$") { throw "Unexpected dotnet-ef version: $version" }
        Write-Host "PASS EF$r exact dotnet-ef $expected"
        $lane = Join-Path $run "net$r"
        New-Item -ItemType Directory $lane | Out-Null
        foreach ($name in 'AvocetStore','AvocetStartup') {
            $source = Join-Path $root "samples/net$r.0/$name"
            $destination = Join-Path $lane $name
            New-Item -ItemType Directory $destination | Out-Null
            Get-ChildItem $source -File | Where-Object { $_.Name -notlike '*evidence.json' -and $_.Name -notlike 'avocet.db*' } | Copy-Item -Destination $destination
            if ($name -eq 'AvocetStore') { Copy-Item "$source/Migrations" "$destination/Migrations" -Recurse }
        }
        Push-Location "$lane/AvocetStartup"
        try {
            $env:PLAYBOOK_AVOCET_DATABASE = "Data Source=$lane/avocet.db"
            & $dotnet restore
            if ($LASTEXITCODE) { throw 'Restore failed' }
            $common = @('--project','../AvocetStore','--startup-project','.')
            $generated = Invoke-Ef -Arguments (@('migrations','add','OutputProbe','--verbose') + $common)
            if (-not (Get-ChildItem "$lane/AvocetStore/Migrations/*_OutputProbe.cs")) { throw 'Target migration was not generated' }
            $launch = @($generated -split '\r?\n' | Where-Object { $_.StartsWith('dotnet exec ', [StringComparison]::Ordinal) })
            if ($launch.Count -ne 1 -or $launch[0] -notmatch '--assembly .*AvocetStore\.dll') { throw 'EF did not select the target assembly' }
            $out = "bin/Debug/net$r.0"
            if ($r -eq '11') {
                foreach ($file in 'avocet-launch.deps.json','avocet-launch.runtimeconfig.json') {
                    if (-not (Test-Path -LiteralPath "$out/$file") -or -not $launch[0].Contains($file, [StringComparison]::Ordinal)) { throw "EF did not select the real $file" }
                }
                if ($launch[0].Contains('AvocetStartup.deps.json', [StringComparison]::Ordinal)) { throw 'EF guessed the conventional deps name' }
            }
            Invoke-Ef -Arguments (@('migrations','list') + $common) | Out-Null
            Invoke-Ef -Arguments (@('database','update','--verbose') + $common) | Out-Null
            if ($r -eq '11') {
                $deps = "$pwd/$out/avocet-launch.deps.json"
                Move-Item -LiteralPath $deps -Destination "$deps.held"
                try {
                    $missing = Invoke-Ef -Arguments (@('migrations','list','--no-build') + $common) -Failure
                    if ($missing -notmatch 'avocet-launch\.deps\.json.*does not exist') { throw 'Missing-file control failed for another reason' }
                } finally { Move-Item -LiteralPath "$deps.held" -Destination $deps }
                Write-Host 'PASS missing custom deps file rejected and restored'
                & $dotnet exec --runtimeconfig "$out/avocet-launch.runtimeconfig.json" --depsfile "$out/avocet-launch.deps.json" "$out/AvocetStartup.dll" --check
            } else { & $dotnet run -- --check }
            if ($LASTEXITCODE) { throw 'Startup persistence failed' }
            if ($r -eq '10') {
                $project = "$pwd/AvocetStartup.csproj"
                $original = [IO.File]::ReadAllText($project)
                try {
                    [xml]$xml = $original
                    foreach ($property in @(@('ProjectDepsFileName','$(TargetDir)avocet-launch.deps.json'), @('ProjectRuntimeConfigFileName','$(TargetDir)avocet-launch.runtimeconfig.json'))) {
                        $element = $xml.CreateElement($property[0]); $element.InnerText = $property[1]
                        $null = $xml.Project.PropertyGroup.AppendChild($element)
                    }
                    $xml.Save($project)
                    foreach ($file in 'AvocetStartup.deps.json','AvocetStartup.runtimeconfig.json') { Remove-Item -LiteralPath "$out/$file" }
                    $failed = Invoke-Ef -Arguments (@('migrations','list','--verbose') + $common) -Failure
                    if ($failed -notmatch 'AvocetStartup\.deps\.json.*does not exist') { throw 'EF10 custom-name failure was not reproduced' }
                    Write-Host 'PASS EF10 custom-output control rejects the guessed conventional deps name'
                } finally { [IO.File]::WriteAllText($project, $original) }
            }
            Write-Host "PASS EF$r separate startup/target generation, artifact selection, migration and persistence"
        } finally { Pop-Location }
    }
} finally {
    $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = $saved
    $env:PLAYBOOK_AVOCET_DATABASE = $savedDatabase
    foreach ($scope in $paths.Keys) {
        if ($paths[$scope] -cne [Environment]::GetEnvironmentVariable('PATH', $scope)) { throw "Unexpected $scope PATH change; shared settings preserved." }
    }
}