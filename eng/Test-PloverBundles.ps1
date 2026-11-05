#Requires -Version 7.3
[CmdletBinding()]
param([switch]$Record)
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$root = Split-Path $PSScriptRoot -Parent
$dotnet = (Get-Command dotnet -CommandType Application | Select-Object -First 1).Source
$saved = @{}
foreach ($key in 'TEMP','TMP','DOTNET_ADD_GLOBAL_TOOLS_TO_PATH') { $saved[$key] = [Environment]::GetEnvironmentVariable($key) }
$paths = @{}
foreach ($scope in 'Process','User','Machine') { $paths[$scope] = [Environment]::GetEnvironmentVariable('PATH',$scope) }
try {
    $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
    $run = Join-Path $root ('artifacts/plover-bundles-' + [guid]::NewGuid().ToString('N'))
    New-Item -ItemType Directory $run -Force | Out-Null
    foreach ($release in '10','11') {
        $version = if ($release -eq '10') { '10.0.13' } else { '11.0.0-rc.2.26504.105' }
        $proof = @()
        if ($Record) { & "$PSScriptRoot/Record-Evidence.ps1" -Sample "samples/net$release.0/PloverBundle/PloverBundle.csproj" -Dotnet $dotnet -Kind compile }
        $tools = Join-Path $run "tools$release"
        & $dotnet tool install dotnet-ef --version $version --tool-path $tools | Out-Host
        $tool = @(Get-ChildItem $tools -Recurse -Filter dotnet-ef.dll)[0].FullName
        foreach ($central in $true,$false) {
            $lane = Join-Path $run "net$release-central-$central"
            $source = Join-Path $root "samples/net$release.0/PloverBundle"
            $app = Join-Path $lane 'app'
            New-Item -ItemType Directory $app -Force | Out-Null
            Get-ChildItem $source -File -Force | Where-Object Name -NotLike '*evidence.json' | Copy-Item -Destination $app
            Copy-Item "$source/Migrations" "$app/Migrations" -Recurse
            Copy-Item "$source/global.json" "$lane/global.json"
            '<Project />' | Set-Content "$lane/Directory.Build.props"
            Copy-Item "$source/Directory.Packages.props" "$lane/Directory.Packages.props"
            $temp = Join-Path $lane 'temp'
            New-Item -ItemType Directory $temp -Force | Out-Null
            $env:TEMP = $temp; $env:TMP = $temp
            # The generated project inherits this target. Capture the real template before cleanup.
            @'
<Project><Target Name="CaptureBundleTemplate" BeforeTargets="CollectPackageReferences" Condition="'$(MSBuildProjectName)' != 'PloverBundle'">
<Copy SourceFiles="$(MSBuildProjectFullPath)" DestinationFiles="$(MSBuildThisFileDirectory)generated-bundle.csproj.txt" />
<WriteLinesToFile File="$(MSBuildThisFileDirectory)central-evaluated.txt" Lines="$(ManagePackageVersionsCentrally)" Overwrite="true" />
</Target></Project>
'@ | Set-Content "$lane/Directory.Build.targets"
            if (-not $central) {
                [xml]$project = Get-Content "$app/PloverBundle.csproj" -Raw
                [xml]$packages = Get-Content "$app/Directory.Packages.props" -Raw
                foreach ($reference in $project.Project.ItemGroup.PackageReference) {
                    $pin = @($packages.Project.ItemGroup.PackageVersion | Where-Object Include -EQ $reference.Include)[0].Version
                    $reference.SetAttribute('Version',$pin)
                }
                $project.Save("$app/PloverBundle.csproj")
                '<Project><PropertyGroup><ManagePackageVersionsCentrally>false</ManagePackageVersionsCentrally></PropertyGroup></Project>' | Set-Content "$app/Directory.Packages.props"
                Copy-Item "$app/Directory.Packages.props" "$lane/Directory.Packages.props"
            }
            Push-Location $app
            try {
                & $dotnet restore --force-evaluate | Out-Host
                $bundle = Join-Path $lane 'migrate.exe'
                $preference = $PSNativeCommandUseErrorActionPreference
                try {
                    $PSNativeCommandUseErrorActionPreference = $false
                    $output = & $dotnet $tool migrations bundle --verbose --force --output $bundle 2>&1 | Out-String
                    $exit = $LASTEXITCODE
                } finally { $PSNativeCommandUseErrorActionPreference = $preference }
                $output | Set-Content "$lane/bundle.log"
                if ($exit -ne 0 -and $release -eq '10' -and $central -and $output -match 'NU1008') {
                    Write-Host 'PASS reproduced EF10 inherited-CPM NU1008; applying isolated temporary-project workaround'
                    '<Project><PropertyGroup><ManagePackageVersionsCentrally>false</ManagePackageVersionsCentrally></PropertyGroup></Project>' | Set-Content "$lane/Directory.Packages.props"
                    & $dotnet $tool migrations bundle --force --output $bundle | Out-Host
                } elseif ($exit -ne 0) { throw "Bundle failed: $output" }
                if (-not (Test-Path "$lane/generated-bundle.csproj.txt")) { throw 'Generated project was not captured.' }
                Get-Content "$lane/generated-bundle.csproj.txt" | Write-Host
                $expectedCentral = if ($release -eq '11' -and $central) { 'true' } else { 'false' }
                if ((Get-Content "$lane/central-evaluated.txt" -Raw).Trim() -ne $expectedCentral) { throw 'Unexpected temporary CPM selection.' }
                $template = Get-Content "$lane/generated-bundle.csproj.txt" -Raw
                if ($release -eq '11' -and $template -notmatch 'Condition=".*ManagePackageVersionsCentrally') { throw 'Missing conditional Design package selection.' }
                $applied = & $bundle --connection "Data Source=$app/plover.db" | Out-String
                $proof += $applied; Write-Host $applied
                $applied = & $bundle --connection "Data Source=$app/plover.db" | Out-String
                $proof += $applied; Write-Host $applied
                $persist = & $dotnet run --project PloverBundle.csproj -- --check | Out-String
                if ($persist -notmatch "PASS migrated schema") { throw "Persistence check missing: $persist" }
                $proof += $persist; Write-Host $persist
                $proof += "CPM=$central temporary CPM=$expectedCentral; " + (Get-Content "$lane/generated-bundle.csproj.txt" -Raw)
                Write-Host "PASS EF$release CPM=$central generated template, bundle, idempotent migration and persisted row"
            } finally { Pop-Location }
        }
    }
    if ($Record) {
        foreach ($release in '10','11') {
            $source = Join-Path $root "samples/net$release.0/PloverBundle"
            $receipt = Get-Content "$source/verification-evidence.json" -Raw | ConvertFrom-Json -AsHashtable
            $receipt.kind = 'execution'
            $receipt.command = 'eng/Test-PloverBundles.ps1 -Record; matching dotnet-ef migrations bundle; migrate.exe --connection <isolated file>; repeat; dotnet run -- --check'
            $receipt.scope = "EF$release actual CPM/non-CPM template, repeated migration and persisted PLO-7; Windows x64 framework-dependent only. No GA proof."
            $receipt.output = 'All four lanes passed migration and persistence assertions. Generated projects:' + [Environment]::NewLine + ((Get-ChildItem "$run/net$release-central-*/generated-bundle.csproj.txt" | Get-Content -Raw) -join [Environment]::NewLine)
            $receipt | ConvertTo-Json -Depth 40 | Set-Content "$source/behavior.evidence.json"
        }
    }
    Write-Host "Artifacts: $run"
} finally {
    foreach ($key in $saved.Keys) { [Environment]::SetEnvironmentVariable($key,$saved[$key]) }
    foreach ($scope in $paths.Keys) {
        if ($paths[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)) { throw "Unexpected $scope PATH change; shared settings preserved." }
    }
}
