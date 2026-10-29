[CmdletBinding()]
param(
  [string]$Net10Dotnet = 'C:\Program Files\dotnet\dotnet.exe',
  [string]$Net11Dotnet = 'C:\Users\pingu\.dotnet-playbook-11\dotnet.exe',
  [ValidateSet('10','11')][string[]]$Releases = @('10','11'),
  [ValidateSet('WildlifeTags','StationReadings')][string[]]$Samples = @('WildlifeTags','StationReadings'),
  [switch]$Record
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
. (Join-Path $PSScriptRoot 'Evidence.ps1')
$root = Split-Path $PSScriptRoot -Parent
$scratch = Join-Path ([IO.Path]::GetTempPath()) ('playbook-nuget-migration-' + [guid]::NewGuid().ToString('N'))
$null = New-Item -ItemType Directory -Path $scratch
$paths = @{}
foreach ($scope in 'Process','User','Machine') { $paths[$scope] = [Environment]::GetEnvironmentVariable('PATH',$scope) }
$log = [Collections.Generic.List[string]]::new()

function Invoke-Sdk([string]$Directory,[string[]]$Arguments,[string]$Cache,[int]$ExpectedExit = 0) {
  $start = [Diagnostics.ProcessStartInfo]::new($dotnet)
  foreach ($argument in $Arguments) { $start.ArgumentList.Add($argument) }
  $start.WorkingDirectory = $Directory
  $start.UseShellExecute = $false
  $start.CreateNoWindow = $true
  $start.RedirectStandardOutput = $true
  $start.RedirectStandardError = $true
  $start.RedirectStandardInput = $true
  $start.Environment['DOTNET_ADD_GLOBAL_TOOLS_TO_PATH'] = '0'
  $start.Environment['DOTNET_CLI_HOME'] = Join-Path $scratch "home-$release"
  $start.Environment['NUGET_PACKAGES'] = $Cache
  $start.Environment['NUGET_HTTP_CACHE_PATH'] = Join-Path $scratch "http-$release"
  $start.Environment['DOTNET_NOLOGO'] = '1'
  $start.Environment['DOTNET_CLI_USE_MSBUILD_SERVER'] = 'false'
  $start.Environment['MSBUILDUSESERVER'] = '0'
  $start.Environment['DOTNET_ROOT'] = Split-Path $dotnet
  $start.Environment['DOTNET_ROOT_X64'] = Split-Path $dotnet
  $process = [Diagnostics.Process]::Start($start)
  $process.StandardInput.Close()
  $stdout = $process.StandardOutput.ReadToEndAsync()
  $stderr = $process.StandardError.ReadToEndAsync()
  if (-not $process.WaitForExit(120000)) { $process.Kill($true); throw 'NuGet migration check timed out.' }
  $output = $stdout.GetAwaiter().GetResult() + $stderr.GetAwaiter().GetResult()
  $observed = ($output -split '\r?\n' | Where-Object { $_ -match 'NU[0-9]{4}|error |reading=|admitted=|^[0-9]+\.[0-9]+\.' }) -join "`n"
  $log.Add("cwd=$Directory`ncache=$Cache`n$dotnet $($Arguments -join ' ')`nexit=$($process.ExitCode)`n$observed")
  [IO.File]::WriteAllText((Join-Path $scratch 'commands.log'),($log -join "`n"))
  if ($process.ExitCode -ne $ExpectedExit) { throw "Expected exit $ExpectedExit, received $($process.ExitCode): $output" }
  return $output
}

function Assert-Text([string]$Actual,[string]$Expected) {
  if (-not $Actual.Contains($Expected,[StringComparison]::Ordinal)) { throw "Missing '$Expected': $Actual" }
}

function Write-Config([string]$Directory,[string]$Feed) {
  $config = Join-Path $Directory 'NuGet.Config'
  '<configuration><packageSources><clear/><add key="local" value="{0}"/></packageSources><fallbackPackageFolders><clear/></fallbackPackageFolders></configuration>' -f [Security.SecurityElement]::Escape($Feed) | Set-Content -LiteralPath $config
  return $config
}

function New-Consumer([string]$Name,[string]$Id,[string]$Version,[string]$Program) {
  $directory = Join-Path $area $Name
  $null = New-Item -ItemType Directory -Path $directory
  Copy-Item -LiteralPath (Join-Path $source 'global.json') -Destination $directory
  $project = (Get-Content -LiteralPath (Join-Path $source 'Consumer.xml') -Raw).Replace('PACKAGE_ID',$Id).Replace('PACKAGE_VERSION',$Version)
  [IO.File]::WriteAllText((Join-Path $directory 'Consumer.csproj'),$project)
  [IO.File]::WriteAllText((Join-Path $directory 'Program.cs'),$Program)
  return $directory
}

function Assert-Assets([string]$Directory,[string]$Id,[string]$Version,[bool]$CheckLock = $true) {
  $assets = Get-Content -LiteralPath (Join-Path $Directory 'obj/project.assets.json') -Raw | ConvertFrom-Json -AsHashtable
  $key = "$Id/$Version"
  if (-not $assets.libraries.ContainsKey($key)) { throw "Missing selected library $key" }
  $target = $assets.targets["net$release.0"][$key]
  if (@($target.compile.Keys | Where-Object { $_ -like '*.dll' }).Count -ne 1 -or @($target.runtime.Keys | Where-Object { $_ -like '*.dll' }).Count -ne 1) { throw "Missing actual compile/runtime assets: $key" }
  if ($CheckLock) {
    $lock = Get-Content -LiteralPath (Join-Path $Directory 'packages.lock.json') -Raw | ConvertFrom-Json -AsHashtable
    if ($lock.dependencies["net$release.0"][$Id].resolved -cne $Version) { throw "Lock/assets mismatch: $key" }
  }
  $log.Add("selected=$key;compile=$($target.compile.Keys -join ',');runtime=$($target.runtime.Keys -join ',');checkedLock=$CheckLock")
  return $assets
}

function Restore-Consumer([string]$Directory,[string]$Config,[string]$Cache,[int]$Exit = 0) {
  Invoke-Sdk $Directory @('restore','Consumer.csproj','--configfile',$Config,'--force','--no-http-cache','-p:NuGetAudit=false') $Cache $Exit
}

function Run-Consumer([string]$Directory,[string]$Cache,[string]$Expected) {
  $null = Invoke-Sdk $Directory @('build','Consumer.csproj','-c','Release','--no-restore') $Cache
  Assert-Text (Invoke-Sdk $Directory @("bin/Release/net$release.0/Consumer.dll" ) $Cache) $Expected
}

function Pack-Sample([string]$Id,[string]$Version,[string]$Feed,[string]$Analysis,[int]$Exit = 0,[bool]$WarningsAsErrors = $false) {
  $output = Invoke-Sdk $source @('pack',"$sampleName.csproj",'-c','Release','-o',$Feed,"-p:PackageId=$Id","-p:Version=$Version","-p:SdkAnalysisLevel=$Analysis","-p:TreatWarningsAsErrors=$WarningsAsErrors",'-p:NuGetAudit=false') (Join-Path $area 'pack-cache') $Exit
  if ($Exit -eq 0) {
    $package = Join-Path $Feed "$Id.$Version.nupkg"
    $zip = [IO.Compression.ZipFile]::OpenRead($package)
    try {
      if ($null -eq $zip.GetEntry("lib/net$release.0/$sampleName.dll")) { throw 'Missing packed library.' }
      $entry = @($zip.Entries | Where-Object FullName -Like '*.nuspec')[0]
      $reader = [IO.StreamReader]::new($entry.Open())
      try { [xml]$nuspec = $reader.ReadToEnd() } finally { $reader.Dispose() }
      if ($nuspec.package.metadata.id -cne $Id) { throw 'Package identity drift.' }
    }
    finally { $zip.Dispose() }
    $log.Add("package=$package;sha256=$((Get-FileHash -LiteralPath $package).Hash.ToLowerInvariant());asset=lib/net$release.0/$sampleName.dll")
  }
  return $output
}

try {
  foreach ($release in $Releases) {
    $dotnet = if ($release -eq '10') { $Net10Dotnet } else { $Net11Dotnet }
    $sdk = if ($release -eq '10') { '10.0.402' } else { '11.0.100-rc.2.26504.105' }
    foreach ($sampleName in $Samples) {
      $sample = "samples/net$release.0/$sampleName/$sampleName.csproj"
      $source = Split-Path (Join-Path $root $sample)
      $inputs = Get-EvidenceInputs $root $sample
      $area = Join-Path $scratch "$release-$sampleName"
      $null = New-Item -ItemType Directory -Path $area
      $firstLog = $log.Count
      Assert-Text (Invoke-Sdk $source @('--version') (Join-Path $area 'pack-cache')) $sdk
      $info = Invoke-Sdk $source @('--info') (Join-Path $area 'pack-cache')
      $sdkDirectory = Join-Path (Split-Path $dotnet) "sdk/$sdk"
      foreach ($assembly in 'NuGet.Commands.dll','NuGet.Packaging.dll') {
        $file = Join-Path $sdkDirectory $assembly
        $log.Add("SDK asset=$file;product=$([Diagnostics.FileVersionInfo]::GetVersionInfo($file).ProductVersion);sha256=$((Get-FileHash -LiteralPath $file).Hash.ToLowerInvariant())")
      }
      $feed = Join-Path $area 'feed'
      $null = New-Item -ItemType Directory -Path $feed
      $config = Write-Config $area $feed
      $analysis = if ($release -eq '10') { '10.0.300' } else { '11.0.100' }
      $program = Get-Content -LiteralPath (Join-Path $source 'Consumer.cs.txt') -Raw
      if ($sampleName -eq 'WildlifeTags') {
        $old = 'Bérwyn.Wildlife.Tags'
        $new = 'Berwyn.Wildlife.Tags'
        $output = Pack-Sample $old '1.0.0' $feed $analysis
        if ($release -eq '11') {
          Assert-Text $output 'NU5052'
          $failedFeed = Join-Path $area 'warnings-as-errors'
          Assert-Text (Pack-Sample $old '1.0.0' $failedFeed $analysis 1 $true) 'NU5052'
          if (Test-Path -LiteralPath (Join-Path $failedFeed "$old.1.0.0.nupkg")) { throw 'Warnings-as-errors emitted a package.' }
          $optOut = Pack-Sample $old '1.0.0' (Join-Path $area 'older-analysis') '10.0.300'
          if ($optOut.Contains('NU5052')) { throw 'Older analysis unexpectedly warned.' }
        }
        elseif ($output.Contains('NU5052')) { throw 'SDK 10 unexpectedly emitted NU5052.' }
        foreach ($valid in @($new,'_Berwyn_Wildlife.Tags')) {
          $output = Pack-Sample $valid '1.0.0' $feed $analysis 0 $true
          if ($output.Contains('NU5052')) { throw 'ASCII underscore identity warned.' }
        }
        foreach ($invalid in 'Berwyn..Wildlife','Berwyn--Wildlife','Berwyn.-Wildlife','Berwyn-.Wildlife','Berwyn Wildlife') {
          $output = Pack-Sample $invalid '1.0.0' (Join-Path $area 'invalid') $analysis 1
          Assert-Text $output 'contains invalid characters'
          if ($output.Contains('NU5052')) { throw 'Legacy invalid ID incorrectly classified as NU5052.' }
        }
        foreach ($id in @($old,$new)) {
          $consumer = New-Consumer $(if ($id -eq $old) { 'old-consumer' } else { 'new-consumer' }) $id '1.0.0' $program
          $cache = Join-Path $area ([guid]::NewGuid().ToString('N'))
          $null = Restore-Consumer $consumer $config $cache
          $null = Assert-Assets $consumer $id '1.0.0'
          Run-Consumer $consumer $cache 'admitted=TERN-042;rejected=OTTER-007'
        }
        # A dependent package keeps the old package identity until explicitly repacked.
        $dependent = New-Consumer 'dependent' $old '1.0.0' 'public static class IntakeMarker {}'
        $null = Invoke-Sdk $dependent @('pack','Consumer.csproj','-c','Release','-o',$feed,'-p:PackageId=Berwyn.Wildlife.Intake','-p:AssemblyName=WildlifeIntake','-p:Version=1.0.0','-p:OutputType=Library','--configfile',$config,'-p:NuGetAudit=false') (Join-Path $area 'dependent-cache')
        $downstream = New-Consumer 'downstream-old' 'Berwyn.Wildlife.Intake' '1.0.0' $program
        $cache = Join-Path $area 'downstream-cache'
        $null = Restore-Consumer $downstream $config $cache
        $null = Assert-Assets $downstream $old '1.0.0'
        $assets = Get-Content -LiteralPath (Join-Path $downstream 'obj/project.assets.json') -Raw | ConvertFrom-Json -AsHashtable
        if ($assets.libraries.ContainsKey("$new/1.0.0")) { throw 'Rename silently redirected a dependency.' }
        Run-Consumer $downstream $cache 'admitted=TERN-042;rejected=OTTER-007'
        $project = Join-Path $dependent 'Consumer.csproj'
        (Get-Content -LiteralPath $project -Raw).Replace($old,$new) | Set-Content -LiteralPath $project
        $null = Invoke-Sdk $dependent @('pack','Consumer.csproj','-c','Release','-o',$feed,'-p:PackageId=Berwyn.Wildlife.Intake','-p:AssemblyName=WildlifeIntake','-p:Version=2.0.0','-p:OutputType=Library','--configfile',$config,'-p:NuGetAudit=false') (Join-Path $area 'replacement-cache')
        $downstream = New-Consumer 'downstream-new' 'Berwyn.Wildlife.Intake' '2.0.0' $program
        $cache = Join-Path $area 'downstream-new-cache'
        $null = Restore-Consumer $downstream $config $cache
        $assets = Assert-Assets $downstream $new '1.0.0'
        if ($assets.libraries.ContainsKey("$old/1.0.0")) { throw 'Repacked dependency still selects the legacy identity.' }
        Run-Consumer $downstream $cache 'admitted=TERN-042;rejected=OTTER-007'
        $log.Add('PASS: NU5052/analysis/warnings-as-errors, legacy punctuation rejection, underscore acceptance, package content and direct/transitive identity migration with real consumers.')
      }
      else {
        $id = 'Cwmglas.Station.Readings'
        $seedFeed = Join-Path $area 'seed-feed'
        foreach ($version in '2.0.0','2.3.0','2.5.0','2.8.0') { $null = Pack-Sample $id $version $seedFeed $analysis }
        foreach ($version in '2.0.0','2.8.0') { Copy-Item -LiteralPath (Join-Path $seedFeed "$id.$version.nupkg") -Destination $feed }
        $seedConfig = Write-Config $seedFeed $seedFeed
        # This actual parent package declares a nonfloating minimum of 2.3.0.
        $parent = New-Consumer 'survey-package' $id '2.3.0' $program
        $null = Invoke-Sdk $parent @('pack','Consumer.csproj','-c','Release','-o',$feed,'-p:PackageId=Cwmglas.Station.Survey','-p:AssemblyName=StationSurvey','-p:Version=1.0.0','--configfile',$seedConfig,'-p:NuGetAudit=false') (Join-Path $area 'parent-cache')
        $zip = [IO.Compression.ZipFile]::OpenRead((Join-Path $feed 'Cwmglas.Station.Survey.1.0.0.nupkg'))
        try {
          $reader = [IO.StreamReader]::new(@($zip.Entries | Where-Object FullName -Like '*.nuspec')[0].Open())
          try { [xml]$nuspec = $reader.ReadToEnd() } finally { $reader.Dispose() }
          $dependency = $nuspec.package.metadata.dependencies.group.dependency
          if ($dependency.id -cne $id -or $dependency.version -cne '2.3.0') { throw 'Parent package does not declare the intended minimum.' }
          $log.Add("Survey/1.0.0 dependency=$($dependency.id);minimum=$($dependency.version)")
        }
        finally { $zip.Dispose() }
        $repaired = New-Consumer 'repaired-request' $id '[2.8.0]' $program
        $repairCache = Join-Path $area 'repair-cache'
        $repairOutput = Restore-Consumer $repaired $config $repairCache
        if ($repairOutput -match 'NU160[13]') { throw 'Existing-source exact-version repair approximated the request.' }
        $null = Assert-Assets $repaired $id '2.8.0'
        Run-Consumer $repaired $repairCache 'reading=12.5;package=2.8.0'
        # A fresh diagnostic control exercises the documented legacy resolver's
        # NU1603. Default resolver diagnostics are measured separately below.
        $diagnostic = New-Consumer 'nu1603-control' 'Cwmglas.Station.Survey' '1.0.0' $program
        $diagnosticCache = Join-Path $area 'nu1603-cache'
        Assert-Text (Invoke-Sdk $diagnostic @('restore','Consumer.csproj','--configfile',$config,'--force','--no-http-cache','-p:NuGetAudit=false','-p:RestoreUseLegacyDependencyResolver=true','-p:RestorePackagesWithLockFile=false') $diagnosticCache) 'NU1603'
        $null = Assert-Assets $diagnostic $id '2.8.0' $false
        Run-Consumer $diagnostic $diagnosticCache 'reading=12.5;package=2.8.0'
        foreach ($route in 'direct','transitive') {
          $reference = if ($route -eq 'direct') { $id } else { 'Cwmglas.Station.Survey' }
          $request = if ($route -eq 'direct') { '2.3.0' } else { '1.0.0' }
          foreach ($scenario in 'higher-source','no-higher-source','exact-cache','uncached') {
            $caseFeed = Join-Path $area "$route-$scenario-feed"
            $null = New-Item -ItemType Directory -Path $caseFeed
            Copy-Item -Path (Join-Path $feed '*.nupkg') -Destination $caseFeed
            if ($scenario -in 'no-higher-source','exact-cache') { Remove-Item -LiteralPath (Join-Path $caseFeed "$id.2.8.0.nupkg") }
            $caseConfig = Write-Config $caseFeed $caseFeed
            $cache = Join-Path $area "$route-$scenario-cache"
            if ($scenario -ne 'uncached') {
              $seedVersion = if ($scenario -eq 'exact-cache') { '2.3.0' } else { '2.5.0' }
              $seed = New-Consumer "$route-$scenario-seed" $id "[$seedVersion]" $program
              $null = Restore-Consumer $seed $seedConfig $cache
              $null = Assert-Assets $seed $id $seedVersion
            }
            $consumer = New-Consumer "$route-$scenario" $reference $request $program
            if ($scenario -eq 'no-higher-source' -and $release -eq '11') {
              Assert-Text (Restore-Consumer $consumer $caseConfig $cache 1) 'NU1102'
              $log.Add("PASS: $route cache-only 2.5.0 cannot satisfy missing 2.3.0 on SDK 11 without a higher source version.")
              continue
            }
            $expected = if ($scenario -eq 'exact-cache') { '2.3.0' } elseif ($scenario -eq 'uncached' -or $release -eq '11') { '2.8.0' } else { '2.5.0' }
            $output = Restore-Consumer $consumer $caseConfig $cache
            if ($scenario -eq 'exact-cache' -and $output -match 'NU160[13]') { throw 'Exact cached requested version unexpectedly approximated.' }
            if ($scenario -ne 'exact-cache' -and $route -eq 'direct') { Assert-Text $output 'NU1601' }
            if ($route -eq 'transitive' -and $output -match 'NU160[13]') { throw 'Default transitive diagnostic changed; reconcile the recipe against the measured artifacts.' }
            $null = Assert-Assets $consumer $id $expected
            Run-Consumer $consumer $cache "reading=12.5;package=$expected"
            $log.Add("PASS: $route/$scenario selected and executed $expected.")
          }
        }
      }
      Write-Host "PASS: SDK $sdk $sampleName; real local-feed consumers and failure cases."
      if ($Record) {
        $evidence = Get-Content -LiteralPath (Join-Path $source 'verification-evidence.json') -Raw | ConvertFrom-Json -AsHashtable
        $evidence.kind = 'execution'
        $evidence.command = "eng/Test-NuGetMigration.ps1 -Net10Dotnet '$Net10Dotnet' -Net11Dotnet '$Net11Dotnet' -Releases $release -Samples $sampleName -Record"
        $evidence.recordedAt = [DateTimeOffset]::UtcNow.ToString('O')
        $evidence.inputs = Get-EvidenceInputs $root $sample
        if (($inputs | ConvertTo-Json -Compress) -cne ($evidence.inputs | ConvertTo-Json -Compress)) { throw 'Inputs changed during execution.' }
        $evidence.sdkInfo = $info
        $evidence.output = ($log | Select-Object -Skip $firstLog) -join "`n"
        [IO.File]::WriteAllText((Join-Path $source 'commands.evidence.json'),($evidence | ConvertTo-Json -Depth 30).Replace("`r`n","`n") + "`n")
      }
    }
  }
}
finally {
  foreach ($scope in $paths.Keys) {
    if ($paths[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)) { throw "Unexpected $scope PATH change; shared settings preserved." }
  }
  Write-Host "NuGet migration scratch: $scratch"
}
