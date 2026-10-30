[CmdletBinding()]
param(
  [string]$Net10Dotnet='C:/Program Files/dotnet/dotnet.exe',
  [string]$Net11Dotnet='C:/Users/pingu/.dotnet-playbook-11/dotnet.exe',
  [switch]$Record
)
$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest
$root=Split-Path $PSScriptRoot -Parent
. (Join-Path $PSScriptRoot 'Evidence.ps1')
. (Join-Path $PSScriptRoot 'QuayProof.ps1')
$scratch=Join-Path $root ('artifacts/quay-collector/'+[guid]::NewGuid().ToString('N'))
$null=New-Item -ItemType Directory $scratch
$log=[Collections.Generic.List[string]]::new()
$command='eng/Test-QuayCollector.ps1 -Record'
$before=@{};foreach($scope in 'Process','User','Machine'){$before[$scope]=[Environment]::GetEnvironmentVariable('PATH',$scope)}
$persistent=@{};foreach($scope in 'User','Machine'){$persistent[$scope]=[Environment]::GetEnvironmentVariables($scope)|ConvertTo-Json -Compress}
function Read-QuayResults([string]$Path){
  [xml]$trx=Get-Content -LiteralPath $Path
  $counters=$trx.TestRun.ResultSummary.Counters
  if($counters.total -ne '2' -or $counters.passed -ne '1' -or $counters.failed -ne '1'){throw 'Expected real passing and deliberate failing assertions.'}
  return @($trx.TestRun.Results.UnitTestResult | ForEach-Object {
    [pscustomobject]@{Id=$_.testId;Name=$_.testName;Outcome=$_.outcome}
  } | Sort-Object Name)
}
try{
  foreach($file in 'Collector.cs','Program.cs','SetterControl.cs','Collector.runsettings','Run-Collector.ps1','Tests/BerthTests.cs'){
    if((Get-FileHash (Join-Path $root "samples/net10.0/QuayCollector/$file")).Hash -cne (Get-FileHash (Join-Path $root "samples/net11.0/QuayCollector/$file")).Hash){throw "Shared canonical source drift: $file"}
  }
  foreach($major in 10,11){
    $startLog=$log.Count
    $dotnet=if($major -eq 10){$Net10Dotnet}else{$Net11Dotnet}
    $version=if($major -eq 10){'18.7.0'}else{'18.11.0'}
    $directory=Join-Path $root "samples/net$major.0/QuayCollector"
    $sdk=(Get-Content (Join-Path $directory 'global.json') -Raw|ConvertFrom-Json).sdk.version
    if((Invoke-Quay $dotnet @('--version') $directory).Trim() -cne $sdk){throw 'Unexpected SDK'}
    $environment=@{DOTNET_ROOT=(Split-Path $dotnet);DOTNET_ROOT_X64=(Split-Path $dotnet);DOTNET_HOST_PATH=$dotnet;VSTEST_DOTNET_HOST_PATH=$dotnet}
    $info=Invoke-Quay $dotnet @('--info') $directory -Environment $environment
    $pins=Get-Content (Join-Path $root 'samples/QuayContainerProof/sdk-pins.json') -Raw|ConvertFrom-Json -AsHashtable
    foreach($pin in $pins[$sdk]){
      $path=[IO.Path]::GetFullPath((Join-Path (Split-Path $dotnet) "sdk/$sdk/$($pin.path)"))
      if((Get-FileHash $path).Hash.ToLowerInvariant() -cne $pin.sha256){throw 'SDK/compiler/ref/runtime drift'}
      $log.Add("SDK_ASSET $($pin.path);sha256=$($pin.sha256);product=$($pin.product)")
    }
    $closures=@{}
    foreach($relative in 'QuayCollector.csproj','Tests/QuayTests.csproj'){
      $project=Join-Path $directory $relative
      $restore=@('restore',$project,'-p:NuGetAudit=false')
      if(-not $Record -or (Test-Path (Join-Path (Split-Path $project) 'packages.lock.json'))){$restore+='--locked-mode'}
      $null=Invoke-Quay $dotnet $restore $directory -Environment $environment
      $projectDirectory=Split-Path $project
      $lock=Join-Path $projectDirectory 'packages.lock.json'
      if($Record){Write-QuayFile $lock (Get-Content $lock -Raw)}
      $assets=Get-Content (Join-Path $projectDirectory 'obj/project.assets.json') -Raw|ConvertFrom-Json -AsHashtable
      $dependencies=@($assets.libraries.Keys|Sort-Object -CaseSensitive)
      $closures[$relative]=$dependencies
      foreach($asset in $assets.targets["net$major.0"].GetEnumerator()){
        $log.Add("RESOLVED $($asset.Key);sha512=$($assets.libraries[$asset.Key].sha512);assets=$($asset.Value|ConvertTo-Json -Depth 15 -Compress)")
      }
      $manifestPath=Join-Path $projectDirectory 'verification.json'
      $manifest=Get-Content $manifestPath -Raw|ConvertFrom-Json
      if($Record){$manifest.dependencies=$dependencies;Write-QuayFile $manifestPath (($manifest|ConvertTo-Json -Depth 12)+"`n")}
      elseif(($dependencies -join ',') -cne ($manifest.dependencies -join ',')){throw 'Dependency closure drift'}
      $null=Invoke-Quay $dotnet @('build',$project,'-c','Release','--no-restore','-p:UseSharedCompilation=false') $directory -Environment $environment
    }
    $artifactPins=Get-Content (Join-Path $root 'samples/TernTesting/artifacts.json') -Raw|ConvertFrom-Json
    # The CLI ZIP has an independent reviewed SHA256 pin. Other dependencies use locked-restore SHA512 content hashes.
    $cliPins=@($artifactPins.packages|Where-Object {$_.version -eq $version -and $_.id -eq 'microsoft.testplatform.cli'})
    if($cliPins.Count -ne 1){throw 'Expected one independently audited CLI package pin per version'}
    foreach($pin in $cliPins){
      $path=Join-Path $scratch "packages/$($pin.id)/$version/$($pin.id).$version.nupkg"
      if((Get-FileHash $path).Hash.ToLowerInvariant() -cne $pin.sha256){throw "Audited package ZIP drift: $($pin.id)"}
      $log.Add("PACKAGE $($pin.id)/$version;sha256=$($pin.sha256)")
    }
    $collector=Join-Path $directory "bin/Release/net$major.0/Quay.Collector.dll"
    $unit=Invoke-Quay $dotnet @($collector,$($major -eq 11).ToString()) $directory -Environment $environment
    Assert-QuayText $unit ('public-setters='+$($major -eq 11).ToString())
    $objectModel=Join-Path (Split-Path $collector) 'Microsoft.VisualStudio.TestPlatform.ObjectModel.dll'
    $log.Add("COMPILED_OBJECTMODEL sha256=$((Get-FileHash $objectModel).Hash.ToLowerInvariant());product=$([Diagnostics.FileVersionInfo]::GetVersionInfo($objectModel).ProductVersion)")
    $control=@('build','QuayCollector.csproj','-c','Release','--no-restore','-p:UseSharedCompilation=false','-p:QuaySetterControl=true',('-p:OutputPath='+(Join-Path $scratch "setter-$major")))
    if($major -eq 10){
      $rejected=Invoke-Quay $dotnet $control $directory -Environment $environment -Failure
      Assert-QuayText $rejected 'CS0200'
    }else{$null=Invoke-Quay $dotnet $control $directory -Environment $environment}
    $console=@(Get-ChildItem (Join-Path $scratch "packages/microsoft.testplatform.cli/$version/contentFiles/any") -Filter vstest.console.dll -Recurse)
    if($console.Count -ne 1){throw 'Expected one exact matching real runner'}
    $tests=Join-Path $directory "Tests/bin/Release/net$major.0/Quay.TestAdapter.dll"
    $settings=Join-Path $scratch "collector-$major.runsettings"
    Write-QuayFile $settings ((Get-Content (Join-Path $directory 'Collector.runsettings') -Raw).Replace('COLLECTOR_DLL',[Security.SecurityElement]::Escape($collector)))
    $results=@{}
    foreach($mode in 'disabled','enabled'){
      $resultDirectory=Join-Path $scratch "$major-$mode"
      $pwsh=(Get-Process -Id $PID).Path
      $arguments=@('-NoProfile','-File',(Join-Path $directory 'Run-Collector.ps1'),'-Dotnet',$dotnet,'-Console',$console[0].FullName,'-TestAssembly',$tests,'-CollectorDirectory',(Split-Path $collector),'-Settings',$settings,'-ResultsDirectory',$resultDirectory)
      if($mode -eq 'enabled'){$arguments+='-Collect'}
      $output=Invoke-Quay $pwsh $arguments $scratch -Environment $environment -Exit 1
      $results[$mode]=Read-QuayResults (Join-Path $resultDirectory 'quay.trx')
      if($mode -eq 'disabled'){
        if($output.Contains('QUAY_EVENT:',[StringComparison]::Ordinal)){throw 'Disabled collector emitted observations'}
        continue
      }
      Assert-QuayText $output 'QUAY_COLLECTOR:'
      Assert-QuayText $output (Join-Path $scratch "packages/microsoft.testplatform.cli/$version")
      Assert-QuayText $output 'expected=12;actual=13'
      $events=@([regex]::Matches($output,'QUAY_EVENT:(\{[^\r\n]+\})')|ForEach-Object {$_.Groups[1].Value|ConvertFrom-Json})
      if($events.Count -ne 4){throw "Expected four real start/end observations, received $($events.Count): $output"}
      foreach($result in $results.enabled){
        $observations=@($events|Where-Object Id -EQ $result.Id)
        if($observations.Count -ne 2){throw 'Event identity differs from actual TRX testId'}
        foreach($observation in $observations){if($observation.Name -cne $result.Name -or $observation.IsChild){throw 'Name/top-level child flag drift'}}
        $end=@($observations|Where-Object Phase -EQ 'end')
        $start=@($observations|Where-Object Phase -EQ 'start')
        if($start.Count -ne 1 -or $end.Count -ne 1 -or $null -ne $start[0].Outcome -or $end[0].Outcome -cne $result.Outcome){throw 'Observed start/end/outcome differs from real runner result'}
      }
    }
    if(($results.disabled|ConvertTo-Json -Compress) -cne ($results.enabled|ConvertTo-Json -Compress)){throw 'Collector changed actual test identities/outcomes'}
    $log.Add("PASS net$major.0: matching runner/host/collector $version; real pass/fail assertions unchanged with collector on/off; four identity/name/child=false/outcome observations match TRX; unit constructors separate; setters added rather than removed.")
    if($Record){foreach($relative in 'QuayCollector.csproj','Tests/QuayTests.csproj'){
      Save-QuayEvidence "samples/net$major.0/QuayCollector/$relative" $sdk $(if($major -eq 10){'14.0'}else{'15.0'}) $info $closures[$relative] (($log|Select-Object -Skip $startLog)-join "`n") 'Actual matching VSTest old/current collector start/end events compared with real TRX and disabled control; top-level cases only. Separate constructor fixtures and compile-only setters establish actual added API direction. No synthetic outcome used as production proof.'
    }}
    Write-Host "PASS real Quay collector net$major.0."
  }
}finally{
  foreach($scope in $before.Keys){if($before[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)){throw "Unexpected $scope PATH change; shared settings preserved."}}
  foreach($scope in $persistent.Keys){if($persistent[$scope] -cne ([Environment]::GetEnvironmentVariables($scope)|ConvertTo-Json -Compress)){throw "Unexpected persistent $scope environment change."}}
  Write-Host "Disposable output: $scratch"
}
