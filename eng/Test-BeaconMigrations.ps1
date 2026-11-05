#Requires -Version 7.3
[CmdletBinding()]
param(
    [switch]$Record,
    [string]$Net10Dotnet = (Get-Command dotnet -CommandType Application | Select-Object -First 1).Source,
    [string]$Net11Dotnet = (Get-Command dotnet -CommandType Application | Select-Object -First 1).Source
)
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$root = Split-Path $PSScriptRoot -Parent
. (Join-Path $PSScriptRoot 'Evidence.ps1')
$paths = @{}
foreach ($scope in 'Process','User','Machine') { $paths[$scope] = [Environment]::GetEnvironmentVariable('PATH',$scope) }
$previous = $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
try {
    $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
    foreach ($release in '10','11') {
        $dotnet = if ($release -eq '10') { $Net10Dotnet } else { $Net11Dotnet }
        $version = if ($release -eq '10') { '10.0.13' } else { '11.0.0-rc.2.26504.105' }
        $source = Join-Path $root "samples/net$release.0/BeaconMigrations"
        $sample = "samples/net$release.0/BeaconMigrations/BeaconMigrations.csproj"
        $before = Get-EvidenceInputs $root $sample
        $run = Join-Path $root ('artifacts/beacon-' + [guid]::NewGuid().ToString('N'))
        New-Item -ItemType Directory $run -Force | Out-Null
        # Copy only canonical inputs. Generated histories stay in the disposable run.
        foreach ($file in 'Program.cs','BeaconMigrations.csproj','Directory.Build.props','Directory.Build.targets','Directory.Packages.props','global.json','packages.lock.json') {
            Copy-Item (Join-Path $source $file) $run
        }
        Push-Location $run
        try {
            & $dotnet restore --locked-mode
            $tools = Join-Path $run 'tools'
            & $dotnet tool install dotnet-ef --version $version --tool-path $tools
            $tool = @(Get-ChildItem $tools -Recurse -Filter dotnet-ef.dll)[0].FullName
            function Invoke-Ef([string[]]$Arguments) {
                & $dotnet $tool @Arguments --project (Join-Path $run 'BeaconMigrations.csproj')
                if ($LASTEXITCODE -ne 0) { throw 'EF scaffolding failed.' }
            }
            $program = Get-Content Program.cs -Raw
            Invoke-Ef @('migrations','add','Initial')
            $baseline = ($run + '-baseline')
            Copy-Item Migrations $baseline -Recurse
            (Get-Content Program.cs -Raw).Replace('// branch-properties','e.Property<string>("SignalColour");') | Set-Content Program.cs
            Invoke-Ef @('migrations','add','SignalColour')
            $branchA = ($run + '-branchA')
            Copy-Item Migrations $branchA -Recurse
            $aSnapshot = Get-Content (Join-Path $branchA 'BeaconContextModelSnapshot.cs') -Raw
            $aId = (Get-ChildItem Migrations -Filter '*_SignalColour.cs').BaseName
            # A second branch starts from the same baseline, without adopting A's history.
            Get-ChildItem Migrations -File | Remove-Item
            Copy-Item "$baseline/*" Migrations
            $program.Replace('// branch-properties','e.Property<int>("LensCount");') | Set-Content Program.cs
            Invoke-Ef @('migrations','add','LensCount')
            $bSnapshot = Get-Content Migrations/BeaconContextModelSnapshot.cs -Raw
            $bId = (Get-ChildItem Migrations -Filter '*_LensCount.cs').BaseName
            if ($release -eq '11' -and (!$aSnapshot.Contains($aId) -or !$bSnapshot.Contains($bId))) {
                throw 'EF11 snapshots did not record competing latest migration IDs.'
            }
            if ($release -eq '10' -and ($aSnapshot.Contains($aId) -or $bSnapshot.Contains($bId))) {
                throw 'Historical EF10 unexpectedly recorded latest IDs.'
            }
            # Return to B's coherent pre-merge baseline. Keep its model change,
            # adopt A's generated history, then scaffold B again on that history.
            Get-ChildItem Migrations -File | Remove-Item
            Copy-Item "$branchA/*" Migrations
            $program.Replace('// branch-properties','e.Property<string>("SignalColour"); e.Property<int>("LensCount");') | Set-Content Program.cs
            Invoke-Ef @('migrations','add','LensCountAfterSignal')
            $latest = (Get-ChildItem Migrations -Filter '*_LensCountAfterSignal.cs').BaseName
            $snapshot = Get-Content Migrations/BeaconContextModelSnapshot.cs -Raw
            $migration = Get-Content "Migrations/$latest.cs" -Raw
            if (!$snapshot.Contains('SignalColour') -or !$snapshot.Contains('LensCount') -or !$migration.Contains('LensCount') -or $migration.Contains('SignalColour')) {
                throw 'Re-scaffolded migration did not isolate B while retaining A.'
            }
            if ($release -eq '11' -and !$snapshot.Contains($latest)) { throw 'Latest ID not coherent.' }
            Invoke-Ef @('migrations','has-pending-model-changes')
            Invoke-Ef @('database','update')
            $result = "PASS EF$release actual competing scaffolds $aId / $bId; reconciled $latest, both properties, no pending model change, SQLite applied."
            Write-Host $result
            if ($Record) {
                $after = Get-EvidenceInputs $root $sample
                if (($before | ConvertTo-Json -Compress) -cne ($after | ConvertTo-Json -Compress)) { throw 'Inputs changed during scaffolding.' }
                $receipt = Get-Content (Join-Path $source 'verification-evidence.json') -Raw | ConvertFrom-Json -AsHashtable
                $receipt.kind = 'execution'
                $receipt.command = "eng/Test-BeaconMigrations.ps1 -Record; dotnet-ef $version migrations add Initial/SignalColour/LensCount/LensCountAfterSignal; migrations has-pending-model-changes; database update"
                $receipt.scope = 'Actual competing generated histories; reconcile by re-scaffolding B on A, inspect snapshot and latest ID, no pending model changes, apply SQLite sequence. No shared database deployment.'
                $receipt.output = $result
                $receipt.inputs = $after
                $receipt.recordedAt = [DateTimeOffset]::UtcNow.ToString('O')
                $receipt | ConvertTo-Json -Depth 30 | Set-Content (Join-Path $source 'migration-checks.evidence.json') -Encoding utf8
            }
        }
        finally { Pop-Location }
    }
}
finally {
    $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = $previous
    foreach ($scope in $paths.Keys) {
        if ($paths[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)) { throw "Unexpected $scope PATH change; shared settings preserved." }
    }
}


