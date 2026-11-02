[CmdletBinding()]
param(
  [string]$Net10Dotnet='C:/Program Files/dotnet/dotnet.exe',
  [string]$Net11Dotnet='C:/Users/pingu/.dotnet-playbook-11/dotnet.exe',
  [string]$X86Net10='C:/Program Files (x86)/dotnet/dotnet.exe',
  [string]$X86Net11='',
  [string]$VisualCppRoot='C:/Program Files (x86)/Microsoft Visual Studio/18/BuildTools/VC/Tools/MSVC/14.51.36231',
  [string]$WindowsSdkLibRoot='C:/Program Files (x86)/Windows Kits/10/Lib/10.0.26100.0',
  [string]$Docker='C:/Program Files/Docker/Docker/resources/bin/docker.exe',
  [ValidateSet('Windows','Unix','All')][string]$Lane='All',
  [switch]$Record
)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
. (Join-Path $PSScriptRoot 'EstuaryProof.ps1')
$scratch=Join-Path $root 'artifacts/native-boundaries'
$null=New-Item -ItemType Directory $scratch -Force
$log=[Collections.Generic.List[string]]::new()
$paths=@{};foreach($scope in 'Process','User','Machine'){$paths[$scope]=[Environment]::GetEnvironmentVariable('PATH',$scope)}
$persistent=@{};foreach($scope in 'User','Machine'){$persistent[$scope]=[Environment]::GetEnvironmentVariables($scope)|ConvertTo-Json -Compress}
try {
 foreach($source in (Get-Content (Join-Path $root 'samples/NativeBoundaries/sources.json') -Raw|ConvertFrom-Json).sources){
  $bytes=(Invoke-WebRequest $source.url).RawContentStream.ToArray()
  if([Convert]::ToHexStringLower([Security.Cryptography.SHA256]::HashData($bytes)) -cne $source.sha256){throw "Official source drift: $($source.url)"}
  $log.Add("SOURCE=$($source.url);sha256=$($source.sha256)")
 }
 if($Lane -in 'Windows','All'){
  if(-not $X86Net11){
   $X86Net11=Join-Path $scratch 'runtime11-x86/dotnet.exe'
   $archive=Join-Path $scratch 'runtime11-x86.zip'
   $pin=@((Get-Content (Join-Path $root 'samples/NativeBoundaries/sources.json') -Raw|ConvertFrom-Json).runtimeArchives|Where-Object name -CEQ 'win-x86-net11-rc2')[0]
   if(-not(Test-Path $archive)){Invoke-WebRequest $pin.url -OutFile $archive}
   $hash=(Get-FileHash $archive).Hash.ToLowerInvariant()
   if($hash -cne $pin.sha256){throw 'Pinned Windows x86 runtime archive hash mismatch'}
   $log.Add("RUNTIME_ARCHIVE=$($pin.url);sha256=$hash")
   if(-not(Test-Path $X86Net11)){
    Expand-Archive -LiteralPath $archive -DestinationPath (Split-Path $X86Net11) -Force
   }
  }
  $linker=Join-Path $VisualCppRoot 'bin/Hostx64/x64/link.exe'
  $compiler=Join-Path $VisualCppRoot 'bin/Hostx64/x64/cl.exe'
  $native=@("MSVC/$((Get-Item $compiler).VersionInfo.FileVersion)","MSVC-link/$((Get-Item $linker).VersionInfo.FileVersion)","Windows-SDK/$(Split-Path $WindowsSdkLibRoot -Leaf)")
  $nativeArgs=@("-p:CppLinker=$linker","-p:TernMsvc=$(Join-Path $VisualCppRoot 'lib/x64')","-p:TernUm=$(Join-Path $WindowsSdkLibRoot 'um/x64')","-p:TernUcrt=$(Join-Path $WindowsSdkLibRoot 'ucrt/x64')")
  foreach($major in '10','11'){
   $dotnet=if($major -eq '10'){$Net10Dotnet}else{$Net11Dotnet}
   $x86=if($major -eq '10'){$X86Net10}else{$X86Net11}
   $sdk=if($major -eq '10'){'10.0.402'}else{'11.0.100-rc.2.26504.105'}
   $lang=if($major -eq '10'){'14.0'}else{'15.0'}
   $audit=Get-Content (Join-Path $root 'docs/planning/audits/runtime-security.assets.json') -Raw|ConvertFrom-Json
   $baseline=@($audit.baselines|Where-Object target -CEQ "net$major.0")[0]
   foreach($name in 'System.Runtime.dll','System.Threading.dll','System.Runtime.InteropServices.dll'){
    $file=Join-Path (Split-Path $dotnet) "packs/Microsoft.NETCore.App.Ref/$($baseline.referencePack)/ref/net$major.0/$name"
    $expected=@($baseline.framework|Where-Object name -CEQ $name)[0].sha256
    if((Get-FileHash $file).Hash.ToLowerInvariant() -cne $expected){throw "Audit reference drift: $file"}
    $log.Add("AUDIT_REFERENCE=$file;sha256=$expected")
   }
   foreach($file in @((Join-Path (Split-Path $dotnet) "sdk/$sdk/Roslyn/bincore/Microsoft.CodeAnalysis.CSharp.dll"),(Join-Path (Split-Path $x86) "shared/Microsoft.NETCore.App/$($baseline.referencePack)/System.Private.CoreLib.dll"))){
    $log.Add("ACTUAL_ARTIFACT=$file;sha256=$((Get-FileHash $file).Hash)")
   }
   $sample="samples/net$major.0/MarshHeap/MarshHeap.csproj"
   $directory=Split-Path (Join-Path $root $sample)
   Assert-Estuary (Invoke-Estuary $dotnet $directory @('--version')) $sdk
   $null=Invoke-Estuary $dotnet $directory @('build','-c','Release')
   $inputs=Initialize-EstuaryInputs $sample
   $info=Invoke-Estuary $dotnet $directory @('--info')
   $dll="bin/Release/net$major.0/MarshHeap.dll"
   $actual=Invoke-Estuary $x86 $directory @($dll,'application')
   Assert-Estuary $actual 'ARCH=X86'
   Assert-Estuary $actual $(if($major -eq '10'){'.NET 10.0.13'}else{'.NET 11.0.0-rc.2.26504.105'})
   $actual=Invoke-Estuary $x86 $directory @($dll) -Exit $(if($major -eq '10'){-532462766}else{0})
   Assert-Estuary $actual $(if($major -eq '10'){'RETAINED=134217728'}else{'LIMIT=33554432'})
   Save-EstuaryEvidence $sample $sdk $lang "net$major.0" 'Real Windows x86 application budget, configured 32MiB managed-heap limit, bounded allocation and OOM; old runtime ignores setting, current enforces it. RSS measured separately.' $info 'eng/Test-NativeBoundaries.ps1 -Lane Windows -Record' -ExpectedInputs $inputs

   $sample="samples/net$major.0/TernExports/TernExports.csproj"
   $directory=Split-Path (Join-Path $root $sample)
   $null=Invoke-Estuary $dotnet $directory @('restore')
   $inputs=Initialize-EstuaryInputs $sample $native
   $variants=if($major -eq '10'){@('KEEP','TRIM','LEGACY_ALWAYS')}else{@('KEEP','TRIM')}
   foreach($variant in $variants){
    $output=Join-Path $scratch "$major-$variant"
    $null=Invoke-Estuary $dotnet $directory (@('publish','-c','Release','-p:RestoreLockedMode=true',"-p:TernVariant=$variant","-p:PublishDir=$output/")+$nativeArgs)
    $exports=Invoke-Estuary $linker $root @('/DUMP','/EXPORTS',"$output/TernExports.dll")
    $expected=$variant -ne 'TRIM'
    if(($exports -match '\btern_depth\b') -ne $expected){throw "Wrong export table: net$major $variant"}
    if($exports -notmatch '\btern_root\b' -or $exports -match '\bUnnamed\b'){throw 'Wrong unconditional/unnamed exports'}
    $null=Invoke-Estuary $compiler $root @('/nologo','/c','/GS-','/TC',"/DEXPECT_DEPTH=$([int]$expected)","/Fo$output/loader.obj",(Join-Path $root 'samples/net11.0/TernExports/loader.c'))
    $null=Invoke-Estuary $linker $root @('/NOLOGO','/NODEFAULTLIB','/SUBSYSTEM:CONSOLE','/ENTRY:mainCRTStartup',"/OUT:$output/loader.exe","$output/loader.obj",(Join-Path $WindowsSdkLibRoot 'um/x64/kernel32.lib'))
    $null=Invoke-Estuary "$output/loader.exe" $output @()
    $log.Add("PASS native loader net$major $variant; tern_depth=$expected;sha256=$((Get-FileHash "$output/TernExports.dll").Hash)")
   }
   Save-EstuaryEvidence $sample $sdk $lang "net$major.0" 'Actual Windows x64 AOT shared-library retained/trimmed export tables and independent C invocation; old unconditional export control, unnamed-export absence.' $info 'eng/Test-NativeBoundaries.ps1 -Lane Windows -Record' $native -ExpectedInputs $inputs
  }
 }
 if($Lane -in 'Unix','All'){ & (Join-Path $PSScriptRoot 'Test-QuayMutex.ps1') -Record:$Record -Net10Dotnet $Net10Dotnet -Net11Dotnet $Net11Dotnet -Docker $Docker }
 Write-Host "PASS native boundaries $Lane"
} finally {
 foreach($scope in $paths.Keys){if([Environment]::GetEnvironmentVariable('PATH',$scope) -cne $paths[$scope]){throw "Unexpected $scope PATH change; shared settings preserved"}}
 foreach($scope in $persistent.Keys){if(([Environment]::GetEnvironmentVariables($scope)|ConvertTo-Json -Compress) -cne $persistent[$scope]){throw "Unexpected persistent $scope environment change; shared settings preserved"}}
}
