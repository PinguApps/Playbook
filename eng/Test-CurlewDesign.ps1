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
    $run = Join-Path $root ('artifacts/curlew-design-' + [guid]::NewGuid().ToString('N'))
    New-Item -ItemType Directory $run -Force | Out-Null
    foreach ($release in '10','11') {
        $dotnet = if ($release -eq '10') { $Dotnet10 } else { $Dotnet11 }
        $version = if ($release -eq '10') { '10.0.13' } else { '11.0.0' }
        $names = @(if ($release -eq '10') { @('CurlewDesign') } else { @('CurlewFileModel','CurlewFile','CurlewStartup') })
        foreach ($name in $names) {
            $file = switch ($name) { 'CurlewFile' { 'Marsh.cs' } 'CurlewStartup' { 'Startup.cs' } default { "$name.csproj" } }
            $sample = "samples/net$release.0/$name/$file"
            if ($Record) { & "$PSScriptRoot/Record-Evidence.ps1" -Sample $sample -Dotnet $dotnet -Kind compile }
            $destination = Join-Path $run "net$release/$name"
            New-Item -ItemType Directory $destination -Force | Out-Null
            Get-ChildItem (Split-Path (Join-Path $root $sample)) -File -Force |
                Where-Object { $_.Name -ne 'verification-evidence.json' -and $_.Name -notlike '*.evidence.json' } |
                Copy-Item -Destination $destination
        }
        Push-Location (Join-Path $run "net$release/$($names[-1])")
        try {
            & $dotnet tool install dotnet-ef --version $version --tool-path "$run/tools$release"
            $tool = @(Get-ChildItem "$run/tools$release" -Recurse -Filter dotnet-ef.dll)[0].FullName
            $routes = if ($release -eq '10') { @('project') } else { @('file','startup-file') }
            foreach ($route in $routes) {
                $directory = if ($route -eq 'file') { Join-Path $run "net$release/CurlewFile" } else { (Get-Location).Path }
                Push-Location $directory
                try {
                    $entry = if ($route -eq 'project') { 'CurlewDesign.csproj' } elseif ($route -eq 'file') { 'Marsh.cs' } else { 'Startup.cs' }
                    & $dotnet build $entry -c Release -p:RestoreLockedMode=true
                    $selection = if ($route -eq 'project') { @('--project','.') } elseif ($route -eq 'file') { @('--file','Marsh.cs') } else { @('--project','../CurlewFileModel','--startup-file','Startup.cs') }
                    $output = & $dotnet $tool migrations add Initial --namespace CurlewMigrations --output-dir Migrations @selection | Out-String
                    $list = & $dotnet $tool migrations list @selection | Out-String
                    if ($list -notmatch '_Initial') { throw 'Generated migration missing from actual assembly.' }
                    $sql = & $dotnet $tool migrations script @selection | Out-String
                    if ($sql -notmatch 'CREATE TABLE "Nests"' -or $sql -notmatch '__EFMigrationsHistory') { throw 'Missing migration SQL.' }
                    $output += & $dotnet $tool database update @selection | Out-String
                    $pending = & $dotnet $tool migrations has-pending-model-changes @selection 2>&1 | Out-String
                    if ($pending -notmatch 'No changes') { throw "Snapshot/model check failed: $pending" }
                    $output += $pending
                    $runArgs = if ($route -eq "project") { @("run","--project",$entry) } else { @("run",$entry) }
                    $persist = & $dotnet @runArgs -c Release -- --check | Out-String
                    if ($persist -notmatch 'PASS migrated schema') { throw 'Migrated persistence check missing.' }
                    $output += $list + $sql + $persist
                    if ($route -eq 'file') {
                        $preference = $PSNativeCommandUseErrorActionPreference
                        try {
                            $PSNativeCommandUseErrorActionPreference = $false
                            $invalid = & $dotnet $tool dbcontext info --file Marsh.cs --project . 2>&1 | Out-String
                            if ($LASTEXITCODE -eq 0 -or $invalid -notmatch 'mutually exclusive|cannot.*both|cannot be used') { throw "Expected exclusive target options: $invalid" }
                            $output += $invalid
                        } finally { $PSNativeCommandUseErrorActionPreference = $preference }
                    }
                    if ($route -eq 'startup-file') {
                        $preference = $PSNativeCommandUseErrorActionPreference
                        try {
                            $PSNativeCommandUseErrorActionPreference = $false
                            $invalid = & $dotnet $tool dbcontext info --project ../CurlewFileModel --startup-file Startup.cs --startup-project . 2>&1 | Out-String
                            if ($LASTEXITCODE -eq 0 -or $invalid -notmatch 'mutually exclusive|cannot.*both|cannot be used') { throw "Expected exclusive startup options: $invalid" }
                            $output += $invalid
                        } finally { $PSNativeCommandUseErrorActionPreference = $preference }
                    }
                    Write-Host "PASS EF$release $route generated/listed/scripted/applied migration and persisted row"
                    if ($Record) {
                        $name = if ($route -eq 'project') { 'CurlewDesign' } elseif ($route -eq 'file') { 'CurlewFile' } else { 'CurlewStartup' }
                        $source = Join-Path $root "samples/net$release.0/$name"
                        $receipt = Get-Content "$source/verification-evidence.json" -Raw | ConvertFrom-Json -AsHashtable
                        $receipt.kind = 'execution'
                        $receipt.command = "eng/Test-CurlewDesign.ps1 -Record; dotnet-ef $version migrations add/list/script and database update; $route"
                        $receipt.scope = 'Actual generated migrations, SQLite schema and persisted row. File target and mixed project/startup-file routes are separate. No file-to-file reference, NativeAOT, other-platform or GA proof.'
                        $receipt.output = $output.Trim()
                        $receipt | ConvertTo-Json -Depth 30 | Set-Content "$source/behavior.evidence.json"
                    }
                } finally { Pop-Location }
            }
        } finally { Pop-Location }
    }
} finally {
    $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = $integration
    foreach ($scope in $before.Keys) {
        if ($before[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)) { throw "Unexpected $scope PATH change; shared settings preserved." }
    }
}
