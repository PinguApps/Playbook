[CmdletBinding()]
param(
  [string]$Net10Dotnet='C:/Program Files/dotnet/dotnet.exe',
  [string]$Net11Dotnet='C:/Users/pingu/.dotnet-playbook-11/dotnet.exe',
  [string]$VisualCppRoot='C:/Program Files (x86)/Microsoft Visual Studio/18/BuildTools/VC/Tools/MSVC/14.51.36231',
  [string]$WindowsSdkLibRoot='C:/Program Files (x86)/Windows Kits/10/Lib/10.0.26100.0',
  [switch]$Record
)
$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest
if(-not $IsWindows){throw 'Windows x64 is required'}
. (Join-Path $PSScriptRoot 'EstuaryProof.ps1')
$root=Split-Path $PSScriptRoot -Parent
$scratch=Join-Path $root ('artifacts/estuary-windows/'+[guid]::NewGuid().ToString('N'))
$null=New-Item -ItemType Directory $scratch -Force
$log=[Collections.Generic.List[string]]::new()
$paths=@{};foreach($scope in 'Process','User','Machine'){$paths[$scope]=[Environment]::GetEnvironmentVariable('PATH',$scope)}
$persistent=@{};foreach($scope in 'User','Machine'){$persistent[$scope]=[Environment]::GetEnvironmentVariables($scope)|ConvertTo-Json -Compress}
$linker=(Resolve-Path -LiteralPath (Join-Path $VisualCppRoot 'bin/Hostx64/x64/link.exe')).Path
$native=@("MSVC-link/$([Diagnostics.FileVersionInfo]::GetVersionInfo($linker).FileVersion)","Windows-SDK/$(Split-Path $WindowsSdkLibRoot -Leaf)")
$nativeArgs=@("-p:CppLinker=$linker","-p:EstuaryMsvcLibraries=$(Join-Path $VisualCppRoot 'lib/x64')","-p:EstuaryUmLibraries=$(Join-Path $WindowsSdkLibRoot 'um/x64')","-p:EstuaryUcrtLibraries=$(Join-Path $WindowsSdkLibRoot 'ucrt/x64')")
try {
  foreach($helper in 'ConsoleProbe'){
    $null=Invoke-Estuary $Net10Dotnet $root @('build',"samples/EstuarySdk/$helper",'-c','Release')
  }
  foreach($major in '10','11') {
    $dotnet=if($major -eq '10'){$Net10Dotnet}else{$Net11Dotnet}
    $sdk=if($major -eq '10'){'10.0.402'}else{'11.0.100-rc.2.26504.105'}
    $language=if($major -eq '10'){'14.0'}else{'15.0'}
    foreach($pin in (Get-Content (Join-Path $root 'samples/EstuarySdk/artifacts.json') -Raw|ConvertFrom-Json).artifacts|Where-Object sdk -CEQ $sdk){
      $file=Join-Path (Split-Path $dotnet) $pin.path
      if((Get-FileHash $file).Hash.ToLowerInvariant() -cne $pin.sha256){throw "Artifact drift: $file"}
      $log.Add("ARTIFACT $($pin.path);sha256=$($pin.sha256);product=$($pin.product)")
    }
    $encoding=Join-Path $root "samples/net$major.0/EstuaryEncoding"
    $info=Invoke-Estuary $dotnet $encoding @('--info')
    Assert-Estuary (Invoke-Estuary $dotnet $encoding @('--version')) $sdk
    $null=Invoke-Estuary $dotnet $encoding @('restore')
    $encodingInputs=Initialize-EstuaryInputs "samples/net$major.0/EstuaryEncoding/EstuaryEncoding.csproj"
    $reader=Join-Path $root 'samples/net11.0/EstuaryEncoding/Read-BuildOutput.ps1'
    foreach($page in 1252,65001) {
      $result=Join-Path $scratch "encoding-$major-$page.json"
      $arguments=@((Join-Path $root 'samples/EstuarySdk/ConsoleProbe/bin/Release/net10.0/ConsoleProbe.dll'),"$page",(Get-Process -Id $PID).Path,'-NoProfile','-File',(Join-Path $root 'eng/Test-EstuaryEncodingLane.ps1'),'-Dotnet',$dotnet,'-Directory',$encoding,'-Reader',$reader,'-Output',$result,'-ConsoleCodePage',"$page")
      $null=Invoke-Estuary $Net10Dotnet $root $arguments
      $observed=Get-Content $result -Raw|ConvertFrom-Json
      foreach($case in $observed|Where-Object { $_.PSObject.Properties.Name -contains 'codePage' }) {
        # The audited draft switch is ignored as an opt-out by these real artifacts.
        $expected=65001
        if($case.codePage -ne $expected){throw "Unexpected code page: net$major console$page $($case.force)=$($case.codePage)"}
      }
      $log.Add("PRIVATE_CONSOLE=$page;"+(Get-Content $result -Raw))
    }
    Assert-Estuary (Invoke-Estuary $dotnet $encoding @("bin/Release/net$major.0/EstuaryEncoding.dll")) 'ESTUARY_APPLICATION'
    Save-EstuaryEvidence "samples/net$major.0/EstuaryEncoding/EstuaryEncoding.csproj" $sdk $language "net$major.0" 'Real raw-byte pipe diagnostics with private Windows CP1252/65001 consoles; default/false/true, mismatched reader and failed SDK build. No shared console edits.' $info 'eng/Test-EstuaryWindows.ps1 -Record' -ExpectedInputs $encodingInputs

    $directory=Join-Path $root "samples/net$major.0/EstuaryNative"
    $null=Invoke-Estuary $dotnet $directory @('build',(Join-Path $root 'samples/EstuarySdk/BinlogReader/BinlogReader.csproj'),'-c','Release',"-p:EstuaryTargetFramework=net$major.0")
    $binReader=Join-Path $root "samples/EstuarySdk/BinlogReader/bin/Release/net$major.0/BinlogReader.dll"
    $null=Invoke-Estuary $dotnet $directory @('restore','--locked-mode')
    $nativeInputs=Initialize-EstuaryInputs "samples/net$major.0/EstuaryNative/EstuaryNative.csproj" $native
    foreach($case in @('legacy-default','legacy-false','legacy-true','publish-default')){
      # A fresh intermediate and publish directory makes stale hook output unable to hide failure.
      $intermediate=Join-Path $scratch "$major-$case/obj/"
      $publish=Join-Path $scratch "$major-$case/publish/"
      $hook=if($case -like 'legacy-*'){'legacy'}else{'publish'}
      $binlog=Join-Path $scratch "$major-$case.binlog"
      $arguments=@('publish','-c','Release',"-p:BaseIntermediateOutputPath=$intermediate","-p:PublishDir=$publish","-p:EstuaryHook=$hook","-bl:$binlog")+$nativeArgs
      if($case -like '*-false'){$arguments+='-p:UseOptimizedPublish=false'}
      if($case -like '*-true'){$arguments+='-p:UseOptimizedPublish=true'}
      $text=Invoke-Estuary $dotnet $directory $arguments
      $targets=Invoke-Estuary $dotnet $directory @($binReader,$binlog)
      $log.Add("BINLOG $case`n$targets")
      $broken=$major -eq '11' -and $hook -eq 'legacy' -and $case -ne 'legacy-false'
      $executable=Join-Path $publish 'EstuaryNative.exe'
      $stream=[IO.File]::OpenRead($executable);$pe=[Reflection.PortableExecutable.PEReader]::new($stream)
      try {if($null -ne $pe.PEHeaders.CorHeader -or $pe.PEHeaders.CoffHeader.Machine.ToString() -ne 'Amd64'){throw 'Not a native x64 PE'}}finally{$pe.Dispose();$stream.Dispose()}
      if($broken){
        Assert-Estuary (Invoke-Estuary $executable $scratch @('copper-lanterns') -Exit 23) 'ESTUARY_MISSING_MANIFEST'
        if($targets -match '(?m)^Build\r?$' -or $targets.Contains('GenerateEstuaryLegacyManifest')){throw 'Unexpected legacy hook execution'}
      } else {
        Assert-Estuary (Invoke-Estuary $executable $scratch @('copper-lanterns')) 'AOT=True'
        Assert-Estuary $targets $(if($hook -eq 'legacy'){'GenerateEstuaryLegacyManifest'}else{'GenerateEstuaryPublishManifest'})
      }
      if($case -eq 'publish-default'){
        $hash=(Get-FileHash $executable).Hash
        $null=Invoke-Estuary $dotnet $directory ($arguments+@('--no-build','-p:EstuaryCargo=amber-buoys'))
        Assert-Estuary (Invoke-Estuary $executable $scratch @('amber-buoys')) 'AOT=True'
        if((Get-FileHash $executable).Hash -cne $hash){throw 'NoBuild changed the compiled native program'}
        $null=Invoke-Estuary $dotnet $directory ($arguments+@('--no-build','-p:EstuaryCargo=amber-buoys'))
        Assert-Estuary (Invoke-Estuary $executable $scratch @('amber-buoys')) 'ESTUARY_NATIVE: amber-buoys'
        Assert-Estuary (Invoke-Estuary $executable $scratch @('wrong') -Exit 24) 'ESTUARY_WRONG_CARGO'
      }
      Write-Host "PASS net$major native $case; expectedMissing=$broken"
    }
    Save-EstuaryEvidence "samples/net$major.0/EstuaryNative/EstuaryNative.csproj" $sdk $language "net$major.0" 'Real Windows x64 ILC/link/native PE/run and binlog replay; old/current legacy and migrated hooks, default/false/true, NoBuild changed sidecar/repeat, missing/wrong sidecar failures.' $info 'eng/Test-EstuaryWindows.ps1 -Record' $native -ExpectedInputs $nativeInputs
  }
  Write-Host 'PASS: Estuary raw-byte encoding and native publish hooks'
} finally {
  foreach($scope in $paths.Keys){if($paths[$scope] -cne [Environment]::GetEnvironmentVariable('PATH',$scope)){throw "Unexpected $scope PATH change; shared settings preserved"}}
  foreach($scope in $persistent.Keys){if($persistent[$scope] -cne ([Environment]::GetEnvironmentVariables($scope)|ConvertTo-Json -Compress)){throw "Unexpected $scope persistent environment change"}}
  Write-Host "Commands and binlogs: $scratch"
}
